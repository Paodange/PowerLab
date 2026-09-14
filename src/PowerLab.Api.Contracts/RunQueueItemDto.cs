using System;

namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// One queued run and its current queue position.
    /// </summary>
    public sealed class RunQueueItemDto
    {
        public string RunId { get; set; } = string.Empty;
        public string ReleaseId { get; set; } = string.Empty;
        public string WorkflowId { get; set; } = string.Empty;
        public RunMode Mode { get; set; }
        public int Position { get; set; }
        public DateTimeOffset EnqueuedAt { get; set; }
    }
}
