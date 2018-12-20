using UnityEngine.EventSystems;

namespace UnityApplicationInsights
{
  public class ButtonTrackerBehaviour : TrackerBehaviour, IPointerClickHandler
  {
    public void OnPointerClick (PointerEventData eventData)
    {
      string targetName = eventData.selectedObject.name;
      string key = "Taps";
      double value = eventData.clickCount;
      // Log button taps
      TrackEvent ("Tap", targetName, key, value);
    }
  }
}