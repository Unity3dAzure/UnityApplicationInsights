using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityApplicationInsights
{
  public static class ClientInfo
  {
    public static string GetDeviceManufacturer ()
    {
      string device = SystemInfo.deviceModel;
      Match match = Regex.Match (device, @"[A-z]+", RegexOptions.IgnoreCase);
      if (match.Success)
      {
        device = match.Value;
      }
      return device;
    }

    public static string GetDeviceVersion ()
    {
      string device = SystemInfo.deviceModel;
      Match match = Regex.Match (device, @"[0-9\.,]+", RegexOptions.IgnoreCase);
      if (match.Success)
      {
        device = match.Value;
      }
      return device;
    }

    public static string GetOSVersion ()
    {
      string os = SystemInfo.operatingSystem;
      Match match = Regex.Match (os, @"[0-9]+\.[0-9]+(\.[0-9]+)?(\.[0-9]+)?", RegexOptions.IgnoreCase);
      if (match.Success)
      {
        os = match.Value;
      }
      return os;
    }
  }
}