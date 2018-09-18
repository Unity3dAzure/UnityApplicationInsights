using UnityEngine;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.Networking;

namespace UnityApplicationInsights
{
    public class ApplicationInsights : MonoBehaviour
    {
        /// <summary>
        /// Allows this class to behavior like a singleton
        /// </summary>
        public static ApplicationInsights Instance;

        /// <summary>
        /// Insert your Instrumentation Key here
        /// </summary>
        [SerializeField]
        internal string instrumentationKey = "";

        /// <summary>
        /// Represent the Analytic Custom Event object
        /// </summary>
        private static TelemetryClient telemetryClient;

        [Tooltip("Enables Application Insights button tracker behaviour scripts to automatically attach to Unity UI buttons. Acts on scene change event.")]
        public bool AddTrackerBehaviours = true;

        /// <summary>
        /// Created only once when changing scenes
        /// </summary>
        private static bool created = false;

        /// <summary>
        /// Debug logging preferences
        /// </summary>
        private static ILogger logger = new Logger(Debug.unityLogger.logHandler);
        [Tooltip("Enables debug logging in console window")]
        public bool DebugLogging = false;

        /// <summary>
        /// Custom Dimensions 
        /// </summary>
        public const string kTARGET = "target";
        public const string kSCENE = "scene";

        /// <summary>
        /// Player prefs
        /// </summary>
        public const string kPLAYER_ID = "ApplicationInsights.playerId";

        /// <summary>
        /// Can be used to prevent Unity Editor stats being logged
        /// </summary>
        [Tooltip("Disables Application Insights logging inside Unity Editor")]
        public bool DisableInsideUnityEditor = false;

        /// <summary>
        /// Sets this class instance as a singleton
        /// </summary>
        void Awake()
        {
            if (DisableInsideUnityEditor && Application.isEditor)
            {
                this.enabled = false;
                return;
            }
            if (!created && string.IsNullOrEmpty(instrumentationKey))
            {
                Debug.LogWarning("Application Insights Instrumentation Key is required.");
                this.enabled = false;
                return;
            }
            if (!created)
            {
                // Instantiate telemetry and metric
                telemetryClient = new TelemetryClient();
                telemetryClient.InstrumentationKey = instrumentationKey.Trim();
                CreateUserSession();
                GetDeviceInfo();
                Log("Telemetry client sessionId: {0} user: {1} device: {2}", telemetryClient.Context.Session.Id, telemetryClient.Context.User.Id, telemetryClient.Context.Device.Id);
                Instance = this;
                // Scene management hooks
                SceneManager.activeSceneChanged += ChangedActiveScene;
                // Retain this instance when changing scenes
                DontDestroyOnLoad(this.gameObject);
                created = true;
            }
        }

        void OnDestroy()
        {
            SceneManager.activeSceneChanged -= ChangedActiveScene;
        }

        /// <summary>
        /// Hook to track Unity scene changes
        /// </summary>
        /// <param name="current"></param>
        /// <param name="next"></param>
        private void ChangedActiveScene(Scene current, Scene next)
        {
            Log("Track scene change: {0}", next.name);
            // Setting operation name using the scene name as the identifier to group events together
            telemetryClient.Context.Operation.Name = next.name;
            // Track scene change
            TrackPageView(next.name, kSCENE);
            if (AddTrackerBehaviours)
            {
                AddButtonTracker();
            }
        }

        /// <summary>
        /// Script to attach custom event script to Unity Buttons (selectables)
        /// </summary>
        public void AddButtonTracker()
        {
            var buttons = Button.allSelectables;
            foreach (var button in buttons)
            {
                if (!button.gameObject.GetComponent<ButtonTrackerBehaviour>())
                {
                    button.gameObject.AddComponent<ButtonTrackerBehaviour>();
                }
            }
        }

        #region Telemetry Context

        /// <summary>
        /// Set up a user and sesson context in the telemetry client.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="sessionId"></param>
        public void CreateUserSession(string userId="", string sessionId="")
        {
            SetUserId(userId);
            SetSessionId(sessionId);
        }

        /// <summary>
        /// Used to set the user Id. NB: The same user Id should be used for future sessions to show retention.
        /// If no user Id is provided a player Id will be generated and saved to PlayerPrefs for reuse.
        /// NOTE: If you want to support a user across multiple devices you should use the user Id from their identity service provider instead.
        /// </summary>
        /// <param name="userId"></param>
        public void SetUserId(string userId="")
        {
            if (!string.IsNullOrEmpty(userId))
            {
                telemetryClient.Context.User.Id = userId;
            }
            // if no user id provided then try getting previous player id or create a new id.
            if (PlayerPrefs.HasKey(kPLAYER_ID))
            {
                string playerId = PlayerPrefs.GetString(kPLAYER_ID, "Anon");
                Log("Welcome back player: {0}", playerId);
                telemetryClient.Context.User.Id = PlayerPrefs.GetString(kPLAYER_ID, "Anon");
            }
            else
            {
                string newPlayerId = Guid.NewGuid().ToString();
                telemetryClient.Context.User.Id = newPlayerId;
                PlayerPrefs.SetString(kPLAYER_ID, newPlayerId);
            }
        }

        /// <summary>
        /// Set a session id, or create a new session id.
        /// </summary>
        /// <param name="sessionId"></param>
        public void SetSessionId(string sessionId="")
        {
            telemetryClient.Context.Session.Id = string.IsNullOrEmpty(sessionId) ? Guid.NewGuid().ToString() : sessionId;
        }

        /// <summary>
        /// Configures telemetry context with device info and client properties.
        /// https://docs.microsoft.com/en-us/dotnet/api/microsoft.applicationinsights.extensibility.implementation.devicecontext.id?view=azure-dotnet
        /// </summary>
        public void GetDeviceInfo()
        {
            // Device info
            telemetryClient.Context.Device.Id = SystemInfo.deviceUniqueIdentifier;
            telemetryClient.Context.Device.Model = SystemInfo.deviceModel;
            telemetryClient.Context.Device.OemName = ClientInfo.GetDeviceManufacturer();
            telemetryClient.Context.Device.OperatingSystem = SystemInfo.operatingSystemFamily.ToString();
            telemetryClient.Context.Device.Type = SystemInfo.deviceType.ToString();
            // Custom client properties
            telemetryClient.Context.Properties.Add("NetworkType", Application.internetReachability.ToString());
            telemetryClient.Context.Properties.Add("ScreenResolution", string.Format("{0},{1}", Screen.width, Screen.height));
            telemetryClient.Context.Properties.Add("Language", Application.systemLanguage.ToString());
            telemetryClient.Context.Properties.Add("Platform", Application.platform.ToString());
            telemetryClient.Context.Properties.Add("OS", SystemInfo.operatingSystem);
            telemetryClient.Context.Properties.Add("OSVersion", ClientInfo.GetOSVersion());
            telemetryClient.Context.Properties.Add("DeviceVersion", ClientInfo.GetDeviceVersion());
            telemetryClient.Context.Properties.Add("Graphics", SystemInfo.graphicsDeviceName);
            telemetryClient.Context.Properties.Add("UnityVersion", Application.unityVersion);
            telemetryClient.Context.Properties.Add("Version", Application.version);
        }

        #endregion

        #region Track Page View

        public void TrackPageView(PageViewTelemetry pageViewTelemtry)
        {
            telemetryClient.TrackPageView(pageViewTelemtry);
        }

        public void TrackPageView(string pageName)
        {
            var t = new PageViewTelemetry(pageName);
            telemetryClient.TrackPageView(t);
        }

        protected void TrackPageView(string pageName, string pageType)
        {
            var t = new PageViewTelemetry(pageName);
            t.Properties.Add("type", pageType);
            telemetryClient.TrackPageView(t);
        }

        #endregion

        #region Track Event

        /// <summary>
        /// User actions and other events. Used to track user behavior or to monitor performance.
        /// For example, send an event whenever a user wins the game!
        /// </summary
        /// <param name="eventTelemetry"></param>
        public void TrackEvent(EventTelemetry eventTelemetry)
        {
            telemetryClient.TrackEvent(eventTelemetry);
        }

        public void TrackEvent(string name, string targetName)
        {
            var t = new EventTelemetry(name);
            t.Properties.Add(kTARGET, targetName);
            Log("TrackEvent Name: {0} Target: {1}", name, targetName);
            TrackEvent(t);
        }

        public void TrackEvent(string name, string targetName, string metricKey, double metricValue)
        {
            var t = new EventTelemetry(name);
            t.Properties.Add(kTARGET, targetName);
            t.Metrics.Add(metricKey, metricValue);
            Log("TrackEvent Name: {0} Target: {1} Metric: {2}:{3}", name, targetName, metricKey, metricValue);
            TrackEvent(t);
        }

        #endregion

        #region Track Metric

        /// <summary>
        /// Performance measurements such as queue lengths not related to specific events.
        /// For example, you could monitor a queue length at regular intervals.
        /// </summary>
        /// <param name="metricTelemetry"></param>
        public void TrackMetric(MetricTelemetry metricTelemetry)
        {
            telemetryClient.TrackMetric(metricTelemetry);
        }

        public void TrackMetric(string name, string targetName, double value)
        {
            var t = new MetricTelemetry();
            t.Name = name;
            t.Value = value;
            t.Properties.Add(kTARGET, targetName);
            Log("TrackMetric Name: {0} Target: {1} Value: {2}", name, targetName, value);
            TrackMetric(t);
        }

        #endregion

        #region Track Exception 

        /// <summary>
        /// Logging exceptions for diagnosis. Trace where they occur in relation to other events and examine stack traces.
        /// </summary>
        public void TrackException(ExceptionTelemetry exceptionTelemetry)
        {
            telemetryClient.TrackException(exceptionTelemetry);
        }

        public void TrackException(Exception exception, string targetName)
        {
            var t = new ExceptionTelemetry(exception);
            t.Properties.Add(kTARGET, targetName);
            telemetryClient.TrackException(t);
        }

        #endregion

        #region Track Request 

        /// <summary>
        /// Logging the frequency and duration of server requests for performance analysis.
        /// </summary>
        public void TrackRequest(RequestTelemetry requestTelemetry)
        {
            telemetryClient.TrackRequest(requestTelemetry);
        }

        public void TrackRequest(UnityWebRequest www, DateTimeOffset timestamp, TimeSpan duration, bool success)
        {
            var t = new RequestTelemetry(www.url, timestamp, duration, www.responseCode.ToString(), success);
            telemetryClient.TrackRequest(t);
        }

        #endregion

        #region Track Trace 

        /// <summary>
        /// Diagnostic log messages. 
        /// Use TrackTrace to help diagnose problems by sending a "breadcrumb trail" to Application Insights.
        /// </summary>
        public void TrackTrace(TraceTelemetry traceTelemetry)
        {
            telemetryClient.TrackTrace(traceTelemetry);
        }

        public void TrackTrace(string message)
        {
            telemetryClient.TrackTrace(message);
        }

        #endregion

        #region Log

        protected void Log(string message)
        {
            if (!DebugLogging) return;
            logger.Log(LogType.Log, "<color=blue>" + message + "</color>");
        }

        protected void Log(string format, params object[] args)
        {
            if (!DebugLogging) return;
            Log(string.Format(format, args));
        }

        #endregion
    }
}


