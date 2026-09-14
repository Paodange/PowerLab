using System.Collections.Generic;

namespace PowerLab.Domain
{
    /// <summary>
    /// A node that atomically assigns one or more workflow variables.
    /// </summary>
    public sealed class SetVariableNode : WorkflowNodeDefinition
    {
        public SetVariableNode()
            : base("setVariable")
        {
        }

        public IReadOnlyList<VariableAssignment> Assignments { get; set; }
            = new List<VariableAssignment>();
    }

    /// <summary>
    /// A target variable and the binding used to calculate its new value.
    /// </summary>
    public sealed class VariableAssignment
    {
        public string VariableId { get; set; } = string.Empty;
        public InputBinding Value { get; set; } = new LiteralBinding();
    }
}
