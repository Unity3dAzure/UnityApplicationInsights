using System;
using System.Collections.Generic;

namespace UnityApplicationInsights
{
  [Serializable]
  public class RequestEnvelope : Envelope
  {
    static public string kType = "RequestData";
    public RequestData data;

    public RequestEnvelope (RequestTelemetry requestTelemetry) : base (kType)
    {
      this.data = new RequestData (requestTelemetry);
    }
  }

  [Serializable]
  public class RequestData
  {
    public string baseType;
    public RequestBaseData baseData;

    public RequestData (RequestTelemetry requestTelemetry)
    {
      this.baseType = RequestEnvelope.kType;
      this.baseData = new RequestBaseData (
        requestTelemetry.Name,
        requestTelemetry.Properties,
        requestTelemetry.Measurements,
        requestTelemetry.Url,
        requestTelemetry.Duration,
        requestTelemetry.ResponseCode,
        requestTelemetry.Success
      );
    }
  }

  [Serializable]
  public class RequestBaseData
  {
    public string name;
    public Dictionary<string, string> properties;
    public Dictionary<string, double> measurements;
    public string id;
    public string url;
    public TimeSpan duration;
    public string responseCode;
    public bool success;

    public RequestBaseData (string name, Dictionary<string, string> properties, Dictionary<string, double> measurements, string url, TimeSpan duration, string responseCode, bool success)
    {
      this.name = name;
      this.properties = properties;
      this.measurements = measurements;
      this.id = Guid.NewGuid ().ToString ();
      this.url = url;
      this.duration = duration;
      this.responseCode = responseCode;
      this.success = success;
    }
  }
}