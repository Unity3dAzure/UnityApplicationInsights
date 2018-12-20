using System.Collections.Generic;

namespace UnityApplicationInsights
{
  // Based on https://github.com/Microsoft/ApplicationInsights-Home/tree/master/EndpointSpecs/Schemas/Bond
  public interface IEnvelope
  {
    string ToPayload ();

    void SetContext (string instrumentationKey, Dictionary<string, string> tags);
  }
}