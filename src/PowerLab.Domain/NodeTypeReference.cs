namespace PowerLab.Domain
{
    /// <summary>
    /// Identifies a plugin node type and its compatible node contract version.
    /// </summary>
    public sealed class NodeTypeReference
    {
        public string PluginId { get; set; } = string.Empty;
        public string PluginVersion { get; set; } = string.Empty;
        public string NodeTypeId { get; set; } = string.Empty;
        public int NodeVersion { get; set; }
    }
}
