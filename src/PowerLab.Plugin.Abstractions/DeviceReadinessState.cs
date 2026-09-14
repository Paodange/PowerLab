namespace PowerLab.Plugin.Abstractions
{
    /// <summary>
    /// Standard readiness states reported by a device driver.
    /// </summary>
    public enum DeviceReadinessState
    {
        Unknown,
        Ready,
        NotReady,
        Faulted
    }
}
