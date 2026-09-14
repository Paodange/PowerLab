namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Payload for a device.statusChanged event.
    /// </summary>
    public sealed class DeviceStatusChangedEventDto
    {
        public string DeviceId { get; set; } = string.Empty;
        public DeviceState State { get; set; }
        public string? DiagnosticMessage { get; set; }
    }
}
