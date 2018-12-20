using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UnityApplicationInsights
{
  public class ApplicationInsights : TelemetryClient
  {
    /// <summary>
    /// App Insights Instance
    /// </summary>
    public static ApplicationInsights Instance;

    [Tooltip ("Enables Application Insights button tracker behaviour scripts to automatically attach to Unity UI buttons. (Triggers on scene change event)")]
    public bool AddTrackerBehaviours = true;

    /// <summary>
    /// Should be created only once in the initial scene
    /// </summary>
    private static bool created = false;

    /// <summary>
    /// Custom Dimensions keys
    /// </summary>
    public const string kTARGET = "target";
    public const string kSCENE = "scene";

    /// <summary>
    /// Can be used to prevent Unity Editor stats being logged
    /// </summary>
    [Tooltip ("Disables Application Insights logging inside Unity Editor")]
    public bool DisableInsideUnityEditor = false;

    /// <summary>
    /// Sets this class instance as a singleton
    /// </summary>
    void Awake ()
    {
      if (DisableInsideUnityEditor && Application.isEditor)
      {
        this.enabled = false;
        return;
      }
      if (!created && string.IsNullOrEmpty (InstrumentationKey))
      {
        LogError ("Application Insights Instrumentation Key is required!");
        this.enabled = false;
        return;
      }
      if (!created)
      {
        InitializeContext ();
        Instance = this;
        // Scene management hooks
        SceneManager.activeSceneChanged += ChangedActiveScene;

        // Retain this instance when changing scenes
        DontDestroyOnLoad (this.gameObject);
        created = true;
      }
    }

    void OnDestroy ()
    {
      SceneManager.activeSceneChanged -= ChangedActiveScene;
    }

    /// <summary>
    /// Hook to track Unity scene changes
    /// </summary>
    /// <param name="current"></param>
    /// <param name="next"></param>
    private void ChangedActiveScene (Scene current, Scene next)
    {
      Log ("Track scene change: {0}", next.name);
      // Setting operation name using the scene name as the identifier to group events together
      Context.Operation.Name = next.name;
      string id = Context.Operation.NewIdentifier ();

      // Track scene change
      TrackPageView (next.name, kSCENE);
      if (AddTrackerBehaviours)
      {
        AddButtonTracker ();
      }
    }

    /// <summary>
    /// Script to attach custom event script to Unity Buttons (selectables)
    /// </summary>
    public void AddButtonTracker ()
    {
      var buttons = Button.allSelectables;
      foreach (var button in buttons)
      {
        if (!button.gameObject.GetComponent<ButtonTrackerBehaviour> ())
        {
          button.gameObject.AddComponent<ButtonTrackerBehaviour> ();
        }
      }
    }

    #region Track Page View

    public void TrackPageView (string pageName)
    {
      var t = new PageViewTelemetry (pageName);
      t.Id = Context.Operation.Id;
      TrackPageView (t);
    }

    protected void TrackPageView (string pageName, string pageType)
    {
      var t = new PageViewTelemetry (pageName);
      t.Id = Context.Operation.Id;
      t.Properties.Add ("type", pageType);
      TrackPageView (t);
    }

    #endregion

    #region Track Event

    /// <summary>
    /// User actions and other events. Used to track user behavior or to monitor performance.
    /// For example, send an event whenever a user wins the game!
    /// </summary
    /// <param name="eventTelemetry"></param>
    public void TrackEvent (string name, string targetName)
    {
      var t = new EventTelemetry (name);
      t.Properties.Add (kTARGET, targetName);
      Log ("TrackEvent Name: {0} Target: {1}", name, targetName);
      TrackEvent (t);
    }

    public void TrackEvent (string name, string targetName, string metricKey, double metricValue)
    {
      var t = new EventTelemetry (name);
      t.Properties.Add (kTARGET, targetName);
      t.Metrics.Add (metricKey, metricValue);
      Log ("TrackEvent Name: {0} Target: {1} Metric: {2}:{3}", name, targetName, metricKey, metricValue);
      TrackEvent (t);
    }

    #endregion

    #region Track Metric

    /// <summary>
    /// Performance measurements such as queue lengths not related to specific events.
    /// For example, you could monitor a queue length at regular intervals.
    /// </summary>
    /// <param name="metricTelemetry"></param>
    public void TrackMetric (string name, string targetName, double value)
    {
      var t = new MetricTelemetry ();
      t.Name = name;
      t.Value = value;
      t.Properties.Add (kTARGET, targetName);
      Log ("TrackMetric Name: {0} Target: {1} Value: {2}", name, targetName, value);
      TrackMetric (t);
    }

    #endregion

    #region Track Exception

    /// <summary>
    /// Logging exceptions for diagnosis. Trace where they occur in relation to other events and examine stack traces.
    /// </summary>
    public void TrackException (Exception exception, string targetName)
    {
      var t = new ExceptionTelemetry (exception);
      t.Properties.Add (kTARGET, targetName);
      TrackException (t);
    }

    #endregion

    #region Track Request

    /// <summary>
    /// Logging the frequency and duration of server requests for performance analysis.
    /// </summary>
    public void TrackRequest (UnityWebRequest www, DateTimeOffset startTime, TimeSpan duration, bool success)
    {
      var t = new RequestTelemetry (www.url, startTime, duration, www.responseCode.ToString (), success);
      TrackRequest (t);
    }

    #endregion

    #region Track Trace

    /// <summary>
    /// Diagnostic log messages.
    /// Use TrackTrace to help diagnose problems by sending a "breadcrumb trail" to Application Insights.
    /// </summary>
    public void TrackTrace (string message)
    {
      TrackTrace (message);
    }

    #endregion
  }
}