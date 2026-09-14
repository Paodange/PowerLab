using System.Collections.Generic;

namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Detailed plugin catalog projection.
    /// </summary>
    public sealed class PluginDetailDto : PluginSummaryDto
    {
        public string? AssemblyVersion { get; set; }
        public IReadOnlyList<NodeTypeSummaryDto> NodeTypes { get; set; }
            = new List<NodeTypeSummaryDto>();
        public IReadOnlyList<DeviceTypeSummaryDto> DeviceTypes { get; set; }
            = new List<DeviceTypeSummaryDto>();
    }
}
