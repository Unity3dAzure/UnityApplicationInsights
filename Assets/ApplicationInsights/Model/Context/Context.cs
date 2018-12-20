using System.Collections.Generic;
using UnityEngine;

namespace UnityApplicationInsights
{
  public class Context
  {
    public Device Device;
    public User User;
    public Session Session;
    public Operation Operation;
    public Dictionary<string, string> Properties;

    private Dictionary<string, string> _tags;

    public Context ()
    {
      Device = new Device ();
      User = new User ();
      Session = new Session ();
      Operation = new Operation ();
      Properties = new Dictionary<string, string> ();
    }

    /// https://github.com/Microsoft/ApplicationInsights-Home/blob/master/EndpointSpecs/Schemas/Docs/ContextTagKeys.md
    private void UpdateTags ()
    {
      var tags = new Dictionary<string, string> ();
      // Device
      tags.Add ("ai.device.id", Device.Id);
      tags.Add ("ai.device.locale", Device.Locale);
      tags.Add ("ai.device.model", Device.Model);
      tags.Add ("ai.device.oemName", Device.OemName);
      tags.Add ("ai.device.osVersion", Device.OperatingSystem);
      tags.Add ("ai.device.type", Device.Type);
      // User
      tags.Add ("ai.user.id", User.Id);
      // Session
      tags.Add ("ai.session.id", Session.Id);
      // Operation
      tags.Add ("ai.operation.id", Operation.Id);
      tags.Add ("ai.operation.name", Operation.Name);
      foreach (var prop in Properties)
      {
        tags.Add (prop.Key, prop.Value);
      }
      _tags = tags;
    }

    public Dictionary<string, string> GetTags ()
    {
      UpdateTags ();
      return _tags;
    }
  }
}