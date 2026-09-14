namespace PowerLab.Domain
{
    /// <summary>
    /// Base type for all persisted V1 workflow nodes.
    /// </summary>
    public abstract class WorkflowNodeDefinition
    {
        protected WorkflowNodeDefinition(string kind)
        {
            Kind = kind;
        }

        /// <summary>
        /// Creates a node for serializers and derived contract types.
        /// </summary>
        protected WorkflowNodeDefinition()
        {
        }

        /// <summary>
        /// Gets or sets the persisted node discriminator.
        /// </summary>
        public string Kind { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the stable node ID.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the designer-facing display name.
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets optional designer-only layout data.
        /// </summary>
        public NodeLayout? Layout { get; set; }
    }
}
