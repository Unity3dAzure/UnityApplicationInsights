using System.Collections.Generic;

namespace UnityApplicationInsights
{
  public class EventTelemetry : BaseTelemetry
  {
    public double Value;
    public Dictionary<string, double> Metrics;

    public EventTelemetry () : base ()
    {
      this.Metrics = new Dictionary<string, double> ();
    }

    public EventTelemetry (string name) : base (name)
    {
      this.Metrics = new Dictionary<string, double> ();
    }

    public override IEnvelope ToEnvelope ()
    {
      return new EventEnvelope (this);
    }
  }
}