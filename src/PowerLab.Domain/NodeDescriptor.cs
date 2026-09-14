using System.Collections.Generic;

namespace PowerLab.Domain
{
    /// <summary>
    /// A serializable description of a plugin node type.
    /// </summary>
    public sealed class NodeDescriptor
    {
        public int DescriptorVersion { get; set; }
        public NodeTypeReference NodeType { get; set; } = new NodeTypeReference();
        public string DisplayName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Category { get; set; } = string.Empty;
        public IconReference? Icon { get; set; }
        public IReadOnlyList<ParameterSchema> Parameters { get; set; }
            = new List<ParameterSchema>();
        public IReadOnlyList<OutputSchema> Outputs { get; set; }
            = new List<OutputSchema>();
        public IReadOnlyList<DeviceSlotSchema> DeviceSlots { get; set; }
            = new List<DeviceSlotSchema>();
        public PauseMode PauseMode { get; set; }
    }
}
