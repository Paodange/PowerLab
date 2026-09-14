namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Public lifecycle and readiness state of a device instance.
    /// </summary>
    public enum DeviceState
    {
        Unknown,
        Disconnected,
        Connecting,
        Ready,
        Busy,
        Faulted,
        Disabled
    }
}
