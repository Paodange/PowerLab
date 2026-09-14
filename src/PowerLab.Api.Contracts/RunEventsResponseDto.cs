using System.Collections.Generic;

namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Ordered event page returned when a client fills a sequence gap.
    /// </summary>
    public sealed class RunEventsResponseDto
    {
        public IReadOnlyList<RuntimeEventEnvelopeDto> Events { get; set; }
            = new List<RuntimeEventEnvelopeDto>();
        public long LastEventSequence { get; set; }
        public bool HasMore { get; set; }
    }
}
