using System;

namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Current state projection for a workflow node in a run snapshot.
    /// </summary>
    public sealed class NodeRuntimeStateDto
    {
        public string NodeId { get; set; } = string.Empty;
        public NodeRuntimeStatus Status { get; set; }
        public string? CurrentAttemptId { get; set; }
        public int? AttemptNumber { get; set; }
        public string? WaveId { get; set; }
        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? FinishedAt { get; set; }
        public StructuredErrorDto? Error { get; set; }
    }
}
