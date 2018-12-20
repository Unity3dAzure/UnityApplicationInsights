using System.Collections.Generic;

namespace UnityApplicationInsights
{
  public class MetricTelemetry : BaseTelemetry
  {
    public double Value;
    public int Count;
    public double Max;
    public double Min;
    public double StandardDeviation;

    public MetricTelemetry () : base () { }

    public MetricTelemetry (string name) : base (name) { }

    public override IEnvelope ToEnvelope ()
    {
      return new MetricEnvelope (this);
    }
  }
}