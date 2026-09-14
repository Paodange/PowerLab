using System.Collections.Generic;

namespace PowerLab.Domain
{
    /// <summary>
    /// Defines a workflow input.
    /// </summary>
    public sealed class WorkflowInputDefinition
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public WorkflowValueType ValueType { get; set; }
        public bool Required { get; set; }
        public LiteralBinding? DefaultValue { get; set; }
        public string? Description { get; set; }
    }

    /// <summary>
    /// Defines a workflow variable and its initial binding.
    /// </summary>
    public sealed class WorkflowVariableDefinition
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public WorkflowValueType ValueType { get; set; }
        public InputBinding InitialValue { get; set; } = new LiteralBinding();
        public string? Description { get; set; }
    }
}
