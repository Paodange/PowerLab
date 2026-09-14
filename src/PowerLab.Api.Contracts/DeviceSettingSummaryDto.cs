namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Non-secret projection of one device connection setting.
    /// </summary>
    public sealed class DeviceSettingSummaryDto
    {
        public string Name { get; set; } = string.Empty;
        public bool IsConfigured { get; set; }
        public bool IsSecret { get; set; }
    }
}
