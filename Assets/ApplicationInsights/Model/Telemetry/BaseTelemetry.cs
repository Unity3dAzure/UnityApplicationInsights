using System.Collections.Generic;

namespace UnityApplicationInsights
{
  public abstract class BaseTelemetry : ITelemetry
  {
    public string Name;
    public Dictionary<string, string> Properties;

    public BaseTelemetry ()
    {
      this.Properties = new Dictionary<string, string> ();
    }

    public BaseTelemetry (string name)
    {
      this.Name = name;
      this.Properties = new Dictionary<string, string> ();
    }

    public abstract IEnvelope ToEnvelope ();
  }
}