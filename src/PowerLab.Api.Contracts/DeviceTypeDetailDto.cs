using System.Collections.Generic;

namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Detailed device type response with connection setting metadata.
    /// </summary>
    public sealed class DeviceTypeDetailDto : DeviceTypeSummaryDto
    {
        public IReadOnlyList<string> ConnectionSettingNames { get; set; }
            = new List<string>();
    }
}
