using System.Collections.Generic;

namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Detailed device projection that never returns plaintext secret values.
    /// </summary>
    public sealed class DeviceDetailDto : DeviceSummaryDto
    {
        public IReadOnlyList<DeviceSettingSummaryDto> ConnectionSettings { get; set; }
            = new List<DeviceSettingSummaryDto>();
    }
}
