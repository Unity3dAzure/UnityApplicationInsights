using UnityEngine;

namespace UnityApplicationInsights
{
  /// <summary>
  /// Requires instance of UnityApplicationInsights in the current scene
  /// </summary>
  public abstract class TrackerBehaviour : MonoBehaviour
  {
    protected virtual void TrackEvent (string name, string targetName)
    {
      if (ApplicationInsights.Instance == null) return;
      ApplicationInsights.Instance.TrackEvent (name, targetName);
    }

    protected virtual void TrackEvent (string name, string targetName, string key, double value)
    {
      if (ApplicationInsights.Instance == null) return;
      ApplicationInsights.Instance.TrackEvent (name, targetName, key, value);
    }

    protected virtual void TrackMetric (string name, string targetName, double value)
    {
      if (ApplicationInsights.Instance == null) return;
      ApplicationInsights.Instance.TrackMetric (name, targetName, value);
    }
  }
}