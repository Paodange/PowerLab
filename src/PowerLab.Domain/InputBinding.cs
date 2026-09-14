namespace PowerLab.Domain
{
    /// <summary>
    /// Base type for a value supplied to a workflow parameter or variable.
    /// </summary>
    public abstract class InputBinding
    {
        protected InputBinding(BindingKind kind)
        {
            Kind = kind;
        }

        /// <summary>
        /// Creates a binding for serializers and derived contract types.
        /// </summary>
        protected InputBinding()
        {
        }

        /// <summary>
        /// Gets or sets the JSON discriminator.
        /// </summary>
        public BindingKind Kind { get; set; }
    }
}
