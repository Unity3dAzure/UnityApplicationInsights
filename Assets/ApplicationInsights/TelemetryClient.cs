using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UnityApplicationInsights
{
  public class TelemetryClient : MonoBehaviour
  {
    /// <summary>
    /// Insert your Instrumentation Key here
    /// </summary>
    [SerializeField]
    internal string InstrumentationKey;

    /// <summary>
    /// Represent the Analytic Custom Event object
    /// </summary>
    public Context Context;

    private bool initialized = false;

    /// <summary>
    /// Player prefs
    /// </summary>
    public const string kPLAYER_ID = "ApplicationInsights.playerId";

    private const string TelemetryEndpoint = "https://dc.services.visualstudio.com/v2/track";

    private List<byte[]> queue = new List<byte[]> ();
    private bool sending = false;

    /// <summary>
    /// Debug logging preferences
    /// </summary>
    private static ILogger logger = new Logger (Debug.unityLogger.logHandler);
    [Tooltip ("Enables debug logging in console window")]
    public bool DebugLogging = false;

    void Awake ()
    {
      InitializeContext ();
    }

    protected void InitializeContext ()
    {
      if (!initialized)
      {
        // Initialize telemetry context
        SetInstrumentationKey (InstrumentationKey);
        Context = new Context ();
        CreateUserSession ();
        GetDeviceInfo ();
        initialized = true;
        Log ("Telemetry client sessionId: {0} user: {1} device: {2}", Context.Session.Id, Context.User.Id, Context.Device.Id);
      }
    }

    public void SetInstrumentationKey (string instrumentationKey)
    {
      if (string.IsNullOrEmpty (instrumentationKey))
      {
        LogError ("Application Insights Instrumentation Key is required.");
        return;
      }
      InstrumentationKey = instrumentationKey.Trim ();
    }

    #region Telemetry Context

    /// <summary>
    /// Set up a user and sesson context in the telemetry client.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="sessionId"></param>
    public void CreateUserSession (string userId = "", string sessionId = "")
    {
      SetUserId (userId);
      SetSessionId (sessionId);
    }

    /// <summary>
    /// Used to set the user Id. NB: The same user Id should be used for future sessions to show retention.
    /// If no user Id is provided a player Id will be generated and saved to PlayerPrefs for reuse.
    /// NOTE: If you want to support a user across multiple devices you should use the user Id from their identity service provider instead.
    /// </summary>
    /// <param name="userId"></param>
    public void SetUserId (string userId = "")
    {
      if (!string.IsNullOrEmpty (userId))
      {
        Context.User.Id = userId;
      }
      // if no user id provided then try getting previous player id or create a new id.
      if (PlayerPrefs.HasKey (kPLAYER_ID))
      {
        string playerId = PlayerPrefs.GetString (kPLAYER_ID, "Anon");
        Log ("Welcome back player: {0}", playerId);
        Context.User.Id = PlayerPrefs.GetString (kPLAYER_ID, "Anon");
      }
      else
      {
        string newPlayerId = Guid.NewGuid ().ToString ();
        Context.User.Id = newPlayerId;
        PlayerPrefs.SetString (kPLAYER_ID, newPlayerId);
      }
    }

    /// <summary>
    /// Set a session id, or create a new session id.
    /// </summary>
    /// <param name="sessionId"></param>
    public void SetSessionId (string sessionId = "")
    {
      Context.Session.Id = string.IsNullOrEmpty (sessionId) ? Guid.NewGuid ().ToString () : sessionId;
    }

    /// <summary>
    /// Configures telemetry context with device info and client properties.
    /// https://docs.microsoft.com/en-us/dotnet/api/microsoft.applicationinsights.extensibility.implementation.deviceContext.id?view=azure-dotnet
    /// </summary>
    public void GetDeviceInfo ()
    {
      // Device info
      Context.Device.Id = SystemInfo.deviceUniqueIdentifier;
      Context.Device.Model = SystemInfo.deviceModel;
      Context.Device.OemName = ClientInfo.GetDeviceManufacturer ();
      Context.Device.OperatingSystem = SystemInfo.operatingSystemFamily.ToString ();
      Context.Device.Type = SystemInfo.deviceType.ToString ();
      // Custom client properties
      Context.Properties.Add ("NetworkType", Application.internetReachability.ToString ());
      Context.Properties.Add ("ScreenResolution", string.Format ("{0},{1}", Screen.width, Screen.height));
      Context.Properties.Add ("Language", Application.systemLanguage.ToString ());
      Context.Properties.Add ("Platform", Application.platform.ToString ());
      Context.Properties.Add ("OS", SystemInfo.operatingSystem);
      Context.Properties.Add ("OSVersion", ClientInfo.GetOSVersion ());
      Context.Properties.Add ("DeviceVersion", ClientInfo.GetDeviceVersion ());
      Context.Properties.Add ("Graphics", SystemInfo.graphicsDeviceName);
      Context.Properties.Add ("UnityVersion", Application.unityVersion);
      Context.Properties.Add ("Version", Application.version);
    }

    #endregion

    #region Track Telemetry
    public void TrackPageView (PageViewTelemetry pageViewTelemtry)
    {
      SendTelemetry (pageViewTelemtry);
    }

    public void TrackEvent (EventTelemetry eventTelemetry)
    {
      SendTelemetry (eventTelemetry);
    }

    public void TrackMetric (MetricTelemetry metricTelemetry)
    {
      SendTelemetry (metricTelemetry);
    }

    public void TrackException (ExceptionTelemetry exceptionTelemetry)
    {
      SendTelemetry (exceptionTelemetry);
    }

    public void TrackRequest (RequestTelemetry requestTelemetry)
    {
      SendTelemetry (requestTelemetry);
    }

    public void TrackTrace (TraceTelemetry traceTelemetry)
    {
      SendTelemetry (traceTelemetry);
    }

    #endregion

    #region Send Telemetry

    private void SendTelemetry (ITelemetry telemetry)
    {
      var data = GeneratePayload (telemetry);
      if (data == null)
      {
        LogError ("Failed to generate Telemetry payload to send!");
        return;
      }
      TrySendRequest (data);
    }

    private byte[] GeneratePayload (ITelemetry telemetry)
    {
      var contextTags = Context.GetTags ();
      var env = telemetry.ToEnvelope ();
      if (env == null) return null;
      env.SetContext (InstrumentationKey, contextTags);
      var payload = env.ToPayload ();
      Log ("Payload: {0}", payload);
      return Encoding.UTF8.GetBytes (payload);
    }

    private void TrySendRequest (byte[] data)
    {
      if (!sending)
      {
        StartCoroutine (SendTelemetryRequest (data));
      }
      else
      {
        QueueRequest (data);
      }
    }

    private void QueueRequest (byte[] data)
    {
      queue.Add (data);
    }

    private void NextQueued ()
    {
      if (queue.Count > 0)
      {
        var data = queue[0];
        queue.RemoveAt (0);
        StartCoroutine (SendTelemetryRequest (data));
      }
    }

    private IEnumerator SendTelemetryRequest (byte[] data = null)
    {
      sending = true;
      using (UnityWebRequest www = new UnityWebRequest (TelemetryEndpoint))
      {
        www.method = UnityWebRequest.kHttpVerbPOST;
        www.downloadHandler = new DownloadHandlerBuffer ();
        www.chunkedTransfer = false;
        www.uploadHandler = new UploadHandlerRaw (data);
        www.SetRequestHeader ("Content-Type", "application/json");

        yield return www.SendWebRequest ();

        if (www.isNetworkError || www.isHttpError)
        {
          LogError ("Error sending payload: {0} {1} data {2}", www.responseCode, www.error, data);
        }
        sending = false;
      }
      NextQueued ();
    }

    #endregion

    #region Debug Log

    private void log (string message, LogType logType = LogType.Log)
    {
      if (!DebugLogging) return;
      string logMessage = message;
      switch (logType)
      {
        case LogType.Error:
        case LogType.Exception:
        case LogType.Warning:
          logMessage = "<color=red>" + message + "</color>";
          break;
        default:
          logMessage = "<color=blue>" + message + "</color>";
          break;
      }
      logger.Log (logType, logMessage);
    }

    protected void Log (string format, params object[] args)
    {
      if (!DebugLogging) return;
      log (string.Format (format, args));
    }

    protected void LogError (string format, params object[] args)
    {
      if (!DebugLogging) return;
      log (string.Format (format, args), LogType.Error);
    }

    #endregion
  }
}