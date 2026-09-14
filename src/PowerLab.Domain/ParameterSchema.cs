using System.Collections.Generic;

namespace PowerLab.Domain
{
    /// <summary>
    /// Describes one parameter accepted by a plugin node type.
    /// </summary>
    public sealed class ParameterSchema
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public WorkflowValueType ValueType { get; set; }
        public bool Required { get; set; }
        public LiteralBinding? DefaultValue { get; set; }
        public IReadOnlySet<BindingKind> AllowedBindings { get; set; }
            = new HashSet<BindingKind>();
        public ParameterConstraints? Constraints { get; set; }
        public ParameterEditorHint? Editor { get; set; }
        public SchemaCondition? ApplicableWhen { get; set; }
        public SchemaCondition? VisibleWhen { get; set; }
        public SchemaCondition? EnabledWhen { get; set; }
        public string? Group { get; set; }
        public int Order { get; set; }
    }
}
