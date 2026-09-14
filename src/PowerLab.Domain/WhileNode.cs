namespace PowerLab.Domain
{
    /// <summary>
    /// A pre-condition loop node containing a child body scope.
    /// </summary>
    public sealed class WhileNode : WorkflowNodeDefinition
    {
        public WhileNode()
            : base("while")
        {
        }

        public ExpressionBinding Condition { get; set; } = new ExpressionBinding();
        public WorkflowScope BodyScope { get; set; } = new WorkflowScope();
    }
}
