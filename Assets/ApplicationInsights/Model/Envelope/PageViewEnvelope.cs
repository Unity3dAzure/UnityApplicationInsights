using System;
using System.Collections.Generic;

namespace UnityApplicationInsights
{
  [Serializable]
  public class PageViewEnvelope : Envelope
  {
    static public string kType = "PageViewData";
    public PageViewData data;

    public PageViewEnvelope (PageViewTelemetry pageViewTelemetry) : base (kType)
    {
      this.data = new PageViewData (pageViewTelemetry);
    }
  }

  [Serializable]
  public class PageViewData
  {
    public string baseType;
    public PageViewBaseData baseData;

    public PageViewData (PageViewTelemetry pageViewTelemetry)
    {
      this.baseType = PageViewEnvelope.kType;
      this.baseData = new PageViewBaseData (
        pageViewTelemetry.Name,
        pageViewTelemetry.Properties,
        pageViewTelemetry.Measurements,
        pageViewTelemetry.Id,
        pageViewTelemetry.Url,
        pageViewTelemetry.Duration
      );
    }
  }

  [Serializable]
  public class PageViewBaseData
  {
    public string name;
    public Dictionary<string, string> properties;
    public Dictionary<string, double> measurements;

    public string id;
    public string url;
    public TimeSpan duration;

    public PageViewBaseData (string name, Dictionary<string, string> properties, Dictionary<string, double> measurements, string id, string url, TimeSpan duration)
    {
      this.name = name;
      this.properties = properties;
      this.measurements = measurements;
      this.id = id;
      this.url = url;
      this.duration = duration;
    }
  }
}