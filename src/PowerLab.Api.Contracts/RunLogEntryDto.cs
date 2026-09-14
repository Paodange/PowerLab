using System;
using System.Collections.Generic;

namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// A redacted, serializable runtime log entry.
    /// </summary>
    public sealed class RunLogEntryDto
    {
        public DateTimeOffset OccurredAt { get; set; }
        public RunLogLevel Level { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? NodeId { get; set; }
        public string? AttemptId { get; set; }
        public IReadOnlyDictionary<string, WorkflowValueDto> Properties { get; set; }
            = new Dictionary<string, WorkflowValueDto>();
    }
}
