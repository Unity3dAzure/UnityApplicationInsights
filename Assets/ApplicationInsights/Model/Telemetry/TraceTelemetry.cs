using System.Collections.Generic;

namespace UnityApplicationInsights
{
  public class TraceTelemetry : BaseTelemetry
  {
    // Collection of custom measurements.
    public Dictionary<string, double> Measurements;

    public string Message;

    public TraceTelemetry (string message) : base (message)
    {
      this.Message = message;
    }

    public override IEnvelope ToEnvelope ()
    {
      return new TraceEnvelope (this);
    }
  }
}