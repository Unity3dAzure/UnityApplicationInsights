using System;
using System.Collections.Generic;

namespace UnityApplicationInsights
{
  [Serializable]
  public class TraceEnvelope : Envelope
  {
    static public string kType = "MessageData";
    public MessageData data;

    public TraceEnvelope (TraceTelemetry traceTelemetry) : base (kType)
    {
      this.data = new MessageData (traceTelemetry);
    }
  }

  [Serializable]
  public class MessageData
  {
    public string baseType;
    public MessageBaseData baseData;

    public MessageData (TraceTelemetry traceTelemetry)
    {
      this.baseType = TraceEnvelope.kType;
      this.baseData = new MessageBaseData (
        traceTelemetry.Name,
        traceTelemetry.Properties,
        traceTelemetry.Measurements,
        traceTelemetry.Message
      );
    }
  }

  [Serializable]
  public class MessageBaseData
  {
    public string name;
    public Dictionary<string, string> properties;
    public Dictionary<string, double> measurements;
    public string message;

    public MessageBaseData (string name, Dictionary<string, string> properties, Dictionary<string, double> measurements, string message)
    {
      this.name = name;
      this.properties = properties;
      this.measurements = measurements;
      this.message = message;
    }
  }
}