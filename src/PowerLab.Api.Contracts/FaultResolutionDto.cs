using System;

namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Audit data for an explicit fault decision.
    /// </summary>
    public sealed class FaultResolutionDto
    {
        public FaultResolutionAction Action { get; set; }
        public DateTimeOffset ResolvedAt { get; set; }
        public string? ResolvedBy { get; set; }
        public string? Comment { get; set; }
        public string? ResolvedAttemptId { get; set; }
    }
}
