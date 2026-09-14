namespace PowerLab.Domain
{
    /// <summary>
    /// A control node that continues the nearest enclosing while loop.
    /// </summary>
    public sealed class ContinueNode : WorkflowNodeDefinition
    {
        public ContinueNode()
            : base("continue")
        {
        }
    }
}
