using System.Collections.Generic;

namespace PowerLab.Domain
{
    /// <summary>
    /// A single-entry, single-exit structured workflow scope.
    /// </summary>
    public sealed class WorkflowScope
    {
        public string Id { get; set; } = string.Empty;
        public string EntryNodeId { get; set; } = string.Empty;
        public string ExitNodeId { get; set; } = string.Empty;
        public IReadOnlyList<WorkflowNodeDefinition> Nodes { get; set; }
            = new List<WorkflowNodeDefinition>();
        public IReadOnlyList<ControlEdge> Edges { get; set; }
            = new List<ControlEdge>();
        public ScopeLayout? Layout { get; set; }
    }
}
