namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// List projection of a plugin node type.
    /// </summary>
    public class NodeTypeSummaryDto
    {
        public string PluginId { get; set; } = string.Empty;
        public string PluginVersion { get; set; } = string.Empty;
        public string NodeTypeId { get; set; } = string.Empty;
        public int NodeVersion { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Category { get; set; } = string.Empty;
    }
}
