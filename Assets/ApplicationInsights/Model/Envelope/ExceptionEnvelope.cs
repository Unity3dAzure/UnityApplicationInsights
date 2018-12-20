using System;
using System.Collections.Generic;

namespace UnityApplicationInsights
{
  [Serializable]
  public class ExceptionEnvelope : Envelope
  {
    static public string kType = "ExceptionData";
    public ExceptionData data;

    public ExceptionEnvelope (ExceptionTelemetry exceptionTelemetry) : base (kType)
    {
      this.data = new ExceptionData (exceptionTelemetry);
    }
  }

  [Serializable]
  public class ExceptionData
  {
    public string baseType;
    public ExceptionBaseData baseData;

    public ExceptionData (ExceptionTelemetry exceptionTelemetry)
    {
      this.baseType = ExceptionEnvelope.kType;
      this.baseData = new ExceptionBaseData (exceptionTelemetry);
    }
  }

  [Serializable]
  public class ExceptionBaseData
  {
    // List of exceptions (only the first one will be used)
    public ExceptionDetails[] exceptions;
    public Dictionary<string, string> properties;

    public string problemId;

    public ExceptionBaseData (ExceptionTelemetry exceptionTelemetry)
    {
      this.problemId = exceptionTelemetry.ProblemId;
      this.properties = exceptionTelemetry.Properties;
      var exception = new ExceptionDetails (exceptionTelemetry.Message, exceptionTelemetry.TypeName);
      this.exceptions = new ExceptionDetails[] { exception };
    }
  }

  [Serializable]
  public class ExceptionDetails
  {
    public string message;
    public string typeName;

    public ExceptionDetails (string message, string typeName)
    {
      this.message = message;
      this.typeName = typeName;
    }
  }
}