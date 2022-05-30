using System;

namespace TqkLibrary.AdbDotNet
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public enum DeviceState
    {
        Device,
        Offline,
        Recovery,
        Authorizing,
        Unauthorized,
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}