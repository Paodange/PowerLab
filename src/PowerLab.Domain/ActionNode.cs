using System.Collections.Generic;

namespace PowerLab.Domain
{
    /// <summary>
    /// A plugin-provided action node instance.
    /// </summary>
    public sealed class ActionNode : WorkflowNodeDefinition
    {
        public ActionNode()
            : base("action")
        {
        }

        public NodeTypeReference NodeType { get; set; } = new NodeTypeReference();
        public IReadOnlyDictionary<string, InputBinding> Parameters { get; set; }
            = new Dictionary<string, InputBinding>();
        public IReadOnlyDictionary<string, string> DeviceBindings { get; set; }
            = new Dictionary<string, string>();
        public IReadOnlyDictionary<string, string> OutputMappings { get; set; }
            = new Dictionary<string, string>();
        public NodeDescriptor? DescriptorSnapshot { get; set; }
    }
}
