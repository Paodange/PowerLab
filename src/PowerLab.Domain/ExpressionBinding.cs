namespace PowerLab.Domain
{
    /// <summary>
    /// Stores expression source without parsing or compiling it.
    /// </summary>
    public sealed class ExpressionBinding : InputBinding
    {
        /// <summary>
        /// Creates an expression binding using the V1 expression language.
        /// </summary>
        public ExpressionBinding()
            : base(BindingKind.Expression)
        {
            Language = "powerExpression";
            LanguageVersion = 1;
        }

        /// <summary>
        /// Gets or sets the expression language identifier.
        /// </summary>
        public string Language { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the expression language version.
        /// </summary>
        public int LanguageVersion { get; set; }

        /// <summary>
        /// Gets or sets the source text.
        /// </summary>
        public string Source { get; set; } = string.Empty;
    }
}
