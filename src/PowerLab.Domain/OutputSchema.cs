namespace PowerLab.Domain
{
    /// <summary>
    /// Describes one output produced by a plugin node type.
    /// </summary>
    public sealed class OutputSchema
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public WorkflowValueType ValueType { get; set; }
        public bool Required { get; set; }
    }
}
