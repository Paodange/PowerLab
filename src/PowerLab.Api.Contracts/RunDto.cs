using System;

namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Summary projection of a workflow run.
    /// </summary>
    public sealed class RunDto
    {
        public string RunId { get; set; } = string.Empty;
        public string ReleaseId { get; set; } = string.Empty;
        public string WorkflowId { get; set; } = string.Empty;
        public RunStatus Status { get; set; }
        public RunMode Mode { get; set; }
        public int? QueuePosition { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.Never)]
        public DateTimeOffset? StartedAt { get; set; }
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.Never)]
        public DateTimeOffset? FinishedAt { get; set; }
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.Never)]
        public string? CurrentWaveId { get; set; }
        public int UnresolvedFaultCount { get; set; }
        public long LastEventSequence { get; set; }
    }
}
