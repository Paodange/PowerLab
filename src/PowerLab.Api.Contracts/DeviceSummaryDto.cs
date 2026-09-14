namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Safe list projection of a configured device instance.
    /// </summary>
    public class DeviceSummaryDto
    {
        public string DeviceId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string PluginId { get; set; } = string.Empty;
        public string DeviceTypeId { get; set; } = string.Empty;
        public DeviceState State { get; set; }
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.Never)]
        public string? DiagnosticMessage { get; set; }
    }
}
