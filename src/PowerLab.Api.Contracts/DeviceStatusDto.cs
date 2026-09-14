using System;

namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Current status query response for a device instance.
    /// </summary>
    public sealed class DeviceStatusDto
    {
        public string DeviceId { get; set; } = string.Empty;
        public DeviceState State { get; set; }
        public string? DiagnosticMessage { get; set; }
        public DateTimeOffset ObservedAt { get; set; }
    }
}
