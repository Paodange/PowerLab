namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Payload for a run.faultResolved event.
    /// </summary>
    public sealed class RunFaultResolvedEventDto
    {
        public string FaultId { get; set; } = string.Empty;
        public string RunId { get; set; } = string.Empty;
        public FaultStatus Status { get; set; }
        public FaultResolutionDto Resolution { get; set; } = new FaultResolutionDto();
    }
}
