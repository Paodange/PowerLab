using System;

namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// A node fault requiring an explicit run decision.
    /// </summary>
    public sealed class RunFaultDto
    {
        public string FaultId { get; set; } = string.Empty;
        public string RunId { get; set; } = string.Empty;
        public string NodeId { get; set; } = string.Empty;
        public string AttemptId { get; set; } = string.Empty;
        public StructuredErrorDto Error { get; set; } = new StructuredErrorDto();
        public FaultStatus Status { get; set; }
        public DateTimeOffset RaisedAt { get; set; }
        public FaultResolutionDto? Resolution { get; set; }
    }
}
