namespace PowerLab.Domain
{
    /// <summary>
    /// A conditional node containing true and false child scopes.
    /// </summary>
    public sealed class IfNode : WorkflowNodeDefinition
    {
        public IfNode()
            : base("if")
        {
        }

        public ExpressionBinding Condition { get; set; } = new ExpressionBinding();
        public WorkflowScope TrueScope { get; set; } = new WorkflowScope();
        public WorkflowScope FalseScope { get; set; } = new WorkflowScope();
    }
}
