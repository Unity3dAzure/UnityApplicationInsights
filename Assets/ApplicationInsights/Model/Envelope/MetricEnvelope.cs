using System;
using System.Collections.Generic;

namespace UnityApplicationInsights
{
  [Serializable]
  public class MetricEnvelope : Envelope
  {
    static public string kType = "MetricData";
    public MetricData data;

    public MetricEnvelope (MetricTelemetry metricTelemetry) : base (kType)
    {
      this.data = new MetricData (metricTelemetry);
    }
  }

  [Serializable]
  public class MetricData
  {
    public string baseType;
    public MetricBaseData baseData;

    public MetricData (MetricTelemetry metricTelemetry)
    {
      this.baseType = MetricEnvelope.kType;

      var metric = (metricTelemetry.Count > 0) ?
        new Metric (metricTelemetry.Name, metricTelemetry.Value, metricTelemetry.Count, metricTelemetry.Max, metricTelemetry.Min, metricTelemetry.StandardDeviation) :
        new Metric (metricTelemetry.Name, metricTelemetry.Value);

      this.baseData = new MetricBaseData (metric, metricTelemetry.Properties);
    }
  }

  [Serializable]
  public class MetricBaseData
  {
    // List of metrics.
    // Only one metric in the list is currently supported by Application Insights storage.
    // If multiple data points were sent only the first one will be used.
    public Metric[] metrics;
    public Dictionary<string, string> properties;

    public MetricBaseData (Metric metric, Dictionary<string, string> properties)
    {
      this.metrics = new Metric[] { metric };
      this.properties = properties;
    }
  }

  [Serializable]
  public class Metric
  {

    public const int kMeasurement = 0;
    public const int kAggregation = 1;

    public string name;
    public double value;

    public int kind;
    public int count;
    public double min;
    public double max;
    public double stdDev;

    public Metric (string metricName, double value)
    {
      this.name = metricName;
      this.value = value;
      this.kind = kMeasurement;
    }

    public Metric (string metricName, double value, int count, double max, double min)
    {
      this.name = metricName;
      this.value = value;
      this.kind = kAggregation;
      this.count = count;
      this.max = max;
      this.min = min;
    }

    public Metric (string metricName, double value, int count, double max, double min, double stdDev)
    {
      this.name = metricName;
      this.value = value;
      this.kind = kAggregation;
      this.count = count;
      this.max = max;
      this.min = min;
      this.stdDev = stdDev;
    }
  }
}