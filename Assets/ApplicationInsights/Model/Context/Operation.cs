using System;

namespace UnityApplicationInsights
{
  public class Operation
  {
    public string Id;
    public string Name;

    public Operation ()
    {
      NewIdentifier ();
      Name = "Root";
    }

    public string NewIdentifier ()
    {
      Id = Guid.NewGuid ().ToString ();
      return Id;
    }
  }
}