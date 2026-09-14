using System;
using System.Text.Json;

namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Versioned open-envelope event sent through the runtime stream.
    /// </summary>
    public sealed class RuntimeEventEnvelopeDto
    {
        public string EventId { get; set; } = string.Empty;
        public string StreamId { get; set; } = string.Empty;
        public long Sequence { get; set; }
        public string EventType { get; set; } = string.Empty;
        public int EventVersion { get; set; }
        public DateTimeOffset OccurredAt { get; set; }
        public string CorrelationId { get; set; } = string.Empty;
        public JsonElement Payload { get; set; }
    }
}
