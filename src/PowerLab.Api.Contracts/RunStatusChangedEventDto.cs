namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Payload for a run.statusChanged event.
    /// </summary>
    public sealed class RunStatusChangedEventDto
    {
        public string RunId { get; set; } = string.Empty;
        public RunStatus? PreviousStatus { get; set; }
        public RunStatus Status { get; set; }
        public string? CurrentWaveId { get; set; }
    }
}
