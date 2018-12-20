using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace UnityApplicationInsights
{
  [Serializable]
  public abstract class Envelope : IEnvelope
  {
    // The application's instrumentation key.
    public string iKey;

    // Event date time when telemetry item was created.
    public DateTime time;

    // Type name of telemetry data item.
    public string name;

    // Key/value collection of context properties.
    public Dictionary<string, string> tags;

    public Envelope (string name)
    {
      Initialize (name);
    }

    public Envelope (string name, string instrumentationKey, Dictionary<string, string> tags)
    {
      Initialize (name);
      SetContext (instrumentationKey, tags);
    }

    protected void Initialize (string name)
    {
      this.name = name;
      this.time = DateTime.UtcNow;
    }

    public void SetContext (string instrumentationKey, Dictionary<string, string> tags)
    {
      this.iKey = instrumentationKey;
      this.tags = tags;
    }

    public virtual string ToPayload ()
    {
      if (string.IsNullOrEmpty (iKey))
      {
        Debug.LogError ("An Application Insights Instrumentation Key is required to generate a payload.");
        return null;
      }
      return JsonConvert.SerializeObject (this); // NB: Can't use Unity's JsonUtility.ToJson(this) method here as it doesn't serialize dictionary key values.
    }
  }
}