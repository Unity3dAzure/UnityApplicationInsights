using System;
using System.Collections.Generic;

namespace UnityApplicationInsights
{
  public class PageViewTelemetry : BaseTelemetry
  {
    // Collection of custom measurements.
    public Dictionary<string, double> Measurements;

    // Identifier of a page view instance. Used for correlation between page view and other telemetry items.
    public string Id;

    // Request URL with all query string parameters
    public string Url;

    // Request duration
    public TimeSpan Duration;

    public PageViewTelemetry (string name) : base (name) { }

    public PageViewTelemetry (string name, string url) : base (name)
    {
      this.Url = url;
    }

    public PageViewTelemetry (string name, string url, TimeSpan duration) : base (name)
    {
      this.Url = url;
      this.Duration = duration;
    }

    public override IEnvelope ToEnvelope ()
    {
      return new PageViewEnvelope (this);
    }
  }
}