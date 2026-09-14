using System.Collections.Generic;

namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// List projection of a device type supplied by a plugin.
    /// </summary>
    public class DeviceTypeSummaryDto
    {
        public string DeviceTypeId { get; set; } = string.Empty;
        public string PluginId { get; set; } = string.Empty;
        public string PluginVersion { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public IReadOnlyList<string> SupportedPlatforms { get; set; }
            = new List<string>();
    }
}
