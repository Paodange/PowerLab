using System.Collections.Generic;

namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// List projection of a discovered plugin.
    /// </summary>
    public class PluginSummaryDto
    {
        public string PluginId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string? Description { get; set; }
        public PluginLoadState LoadState { get; set; }
        public IReadOnlyList<PluginDiagnosticDto> Diagnostics { get; set; }
            = new List<PluginDiagnosticDto>();
        public int NodeTypeCount { get; set; }
        public int DeviceTypeCount { get; set; }
    }
}
