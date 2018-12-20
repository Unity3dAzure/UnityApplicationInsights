using System;
using System.Collections.Generic;

namespace UnityApplicationInsights
{
  public class ExceptionTelemetry : BaseTelemetry
  {
    // Exception type name
    public string TypeName;

    // Exception message
    public string Message;

    // Identifier of where the exception was thrown in code. Used for exceptions grouping.
    public string ProblemId;

    public ExceptionTelemetry (Exception exception) : base ()
    {
      this.Message = exception.Message;
      this.TypeName = exception.Source;
      this.ProblemId = exception.HelpLink;
    }

    public override IEnvelope ToEnvelope ()
    {
      return new ExceptionEnvelope (this);
    }
  }
}