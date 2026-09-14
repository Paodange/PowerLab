using System;
using System.Collections.Generic;

namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Payload for a node.attemptCompleted event.
    /// </summary>
    public sealed class NodeAttemptCompletedEventDto
    {
        public string AttemptId { get; set; } = string.Empty;
        public string RunId { get; set; } = string.Empty;
        public string NodeId { get; set; } = string.Empty;
        public int AttemptNumber { get; set; }
        public NodeRuntimeStatus Status { get; set; }
        public DateTimeOffset FinishedAt { get; set; }
        public StructuredErrorDto? Error { get; set; }
        public IReadOnlyDictionary<string, WorkflowValueDto> Outputs { get; set; }
            = new Dictionary<string, WorkflowValueDto>();
    }
}
