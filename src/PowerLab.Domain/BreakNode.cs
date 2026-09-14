namespace PowerLab.Domain
{
    /// <summary>
    /// A control node that exits the nearest enclosing while loop.
    /// </summary>
    public sealed class BreakNode : WorkflowNodeDefinition
    {
        public BreakNode()
            : base("break")
        {
        }
    }
}
