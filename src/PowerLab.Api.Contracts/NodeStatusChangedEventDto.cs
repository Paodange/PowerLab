namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Payload for a node.statusChanged event.
    /// </summary>
    public sealed class NodeStatusChangedEventDto
    {
        public string NodeId { get; set; } = string.Empty;
        public string AttemptId { get; set; } = string.Empty;
        public NodeRuntimeStatus Status { get; set; }
        public string? WaveId { get; set; }
    }
}
