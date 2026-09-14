using System;

namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Payload for a node.attemptStarted event.
    /// </summary>
    public sealed class NodeAttemptStartedEventDto
    {
        public string AttemptId { get; set; } = string.Empty;
        public string RunId { get; set; } = string.Empty;
        public string NodeId { get; set; } = string.Empty;
        public int AttemptNumber { get; set; }
        public string? WaveId { get; set; }
        public DateTimeOffset StartedAt { get; set; }
    }
}
