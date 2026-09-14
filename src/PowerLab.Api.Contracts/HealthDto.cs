using System;
using System.Collections.Generic;

namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Health query response shape. It contains no live health-check implementation.
    /// </summary>
    public sealed class HealthDto
    {
        public HealthStatus Status { get; set; }
        public string RuntimeInstanceId { get; set; } = string.Empty;
        public DateTimeOffset CheckedAt { get; set; }
        public IReadOnlyList<HealthComponentDto> Components { get; set; }
            = new List<HealthComponentDto>();
    }
}
