using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityApplicationInsights
{
  public class ButtonHoverTrackerBehaviour : ButtonTrackerBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
  {
    float capturedTime = 0;
    bool shouldCaptureTime = false;

    void Update ()
    {
      if (!shouldCaptureTime)
      {
        return;
      }
      capturedTime += Time.deltaTime;
    }

    public void OnPointerEnter (PointerEventData eventData)
    {
      shouldCaptureTime = true;
      capturedTime = 0;
    }

    public void OnPointerExit (PointerEventData eventData)
    {
      shouldCaptureTime = false;
      // Log button hover time
      string targetName = eventData.pointerEnter.transform.parent.name;
      double value = Math.Round (capturedTime, 1);
      TrackMetric ("Hover", targetName, value);
    }
  }

}