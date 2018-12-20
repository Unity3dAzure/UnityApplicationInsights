using System;
using System.Collections.Generic;

namespace UnityApplicationInsights
{
  [Serializable]
  public class EventEnvelope : Envelope
  {
    static public string kType = "EventData";
    public EventData data;

    public EventEnvelope (EventTelemetry eventTelemetry) : base (kType)
    {
      this.data = new EventData (eventTelemetry);
    }
  }

  [Serializable]
  public class EventData
  {
    public string baseType;
    public EventBaseData baseData;

    public EventData (EventTelemetry eventTelemetry)
    {
      this.baseType = EventEnvelope.kType;
      this.baseData = new EventBaseData (eventTelemetry.Name, eventTelemetry.Properties, eventTelemetry.Metrics);
    }
  }

  [Serializable]
  public class EventBaseData
  {
    public string name;
    public Dictionary<string, string> properties;
    // List of measurements
    public Dictionary<string, double> measurements;

    public EventBaseData (string name, Dictionary<string, string> properties, Dictionary<string, double> measurements)
    {
      this.name = name;
      this.properties = properties;
      this.measurements = measurements;
    }
  }
}