using System;
using System.Collections.Generic;

namespace UnityApplicationInsights
{
  public class RequestTelemetry : BaseTelemetry
  {
    // Collection of custom measurements.
    public Dictionary<string, double> Measurements;

    // Identifier of a request call instance
    public string Id;
    // Request URL with all query string parameters
    public string Url;
    // Request duration
    public TimeSpan Duration;
    // HTTP status code for HTTP request
    public string ResponseCode;
    // Indication of successfull or unsuccessfull call
    public bool Success;
    public DateTimeOffset StartTime;

    public RequestTelemetry (string url, DateTimeOffset startTime, TimeSpan duration, string responseCode, bool success) : base (url)
    {
      this.Id = new Guid ().ToString ();
      this.Url = url;
      this.Duration = duration;
      this.ResponseCode = responseCode;
      this.Success = success;
      this.StartTime = startTime;
    }

    public override IEnvelope ToEnvelope ()
    {
      return new RequestEnvelope (this);
    }
  }
}