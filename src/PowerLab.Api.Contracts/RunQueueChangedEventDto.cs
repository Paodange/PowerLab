namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Payload for a run.queueChanged event.
    /// </summary>
    public sealed class RunQueueChangedEventDto
    {
        public string RunId { get; set; } = string.Empty;
        public int? QueuePosition { get; set; }
        public int QueueLength { get; set; }
    }
}
