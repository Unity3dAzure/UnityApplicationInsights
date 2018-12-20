namespace UnityApplicationInsights
{
  // Based on https://github.com/Microsoft/ApplicationInsights-Home/tree/master/EndpointSpecs/Schemas/Docs
  public interface ITelemetry
  {
    IEnvelope ToEnvelope ();
  }
}