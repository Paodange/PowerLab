namespace PowerLab.Domain
{
    /// <summary>
    /// A persisted control-flow edge between node ports.
    /// </summary>
    public sealed class ControlEdge
    {
        public string Id { get; set; } = string.Empty;
        public ControlEndpoint Source { get; set; } = new ControlEndpoint();
        public ControlEndpoint Target { get; set; } = new ControlEndpoint();
    }

    /// <summary>
    /// The node and port at one end of a control-flow edge.
    /// </summary>
    public sealed class ControlEndpoint
    {
        public string NodeId { get; set; } = string.Empty;
        public string PortId { get; set; } = string.Empty;
    }
}
