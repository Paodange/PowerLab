using System.Collections.Generic;

namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Payload for a log.appended event.
    /// </summary>
    public sealed class LogAppendedEventDto
    {
        public RunLogLevel Level { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? NodeId { get; set; }
        public string? AttemptId { get; set; }
        public IReadOnlyDictionary<string, WorkflowValueDto> Properties { get; set; }
            = new Dictionary<string, WorkflowValueDto>();
    }
}
