namespace PowerLab.Domain
{
    /// <summary>
    /// Supplies a scalar literal value.
    /// </summary>
    public sealed class LiteralBinding : InputBinding
    {
        /// <summary>
        /// Creates a literal binding.
        /// </summary>
        public LiteralBinding()
            : base(BindingKind.Literal)
        {
            Value = WorkflowValue.FromString(string.Empty);
        }

        /// <summary>
        /// Gets or sets the scalar value.
        /// </summary>
        public WorkflowValue Value { get; set; }
    }
}
