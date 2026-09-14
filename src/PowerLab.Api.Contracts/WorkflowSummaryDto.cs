using System;

namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// List projection of a mutable workflow draft.
    /// </summary>
    public class WorkflowSummaryDto
    {
        public string WorkflowId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public int? CurrentReleaseNumber { get; set; }
    }
}
