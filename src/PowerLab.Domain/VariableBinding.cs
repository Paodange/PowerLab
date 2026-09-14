namespace PowerLab.Domain
{
    /// <summary>
    /// References a workflow variable by stable ID.
    /// </summary>
    public sealed class VariableBinding : InputBinding
    {
        /// <summary>
        /// Creates a variable binding.
        /// </summary>
        public VariableBinding()
            : base(BindingKind.Variable)
        {
        }

        /// <summary>
        /// Gets or sets the referenced workflow variable ID.
        /// </summary>
        public string VariableId { get; set; } = string.Empty;
    }
}
