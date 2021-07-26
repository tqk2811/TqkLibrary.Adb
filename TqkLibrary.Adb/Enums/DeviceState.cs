using System;

namespace TqkLibrary.Adb
{
  [Flags]
  public enum DeviceState
  {
    All = 0,
    Device = 1 << 0,
    Offline = 1 << 1,
    Recovery = 1 << 2,
    Authorizing = 1 << 3,
    Unauthorized = 1 << 4,
  }
}