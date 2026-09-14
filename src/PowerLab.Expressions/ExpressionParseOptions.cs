namespace PowerLab.Expressions
{
    /// <summary>
    /// Resource limits for the V1 expression lexer and parser.
    /// </summary>
    public sealed class ExpressionParseOptions
    {
        /// <summary>
        /// Default maximum source length in UTF-16 characters.
        /// </summary>
        public const int DefaultMaxSourceLength = 4096;

        /// <summary>
        /// Default maximum nested parentheses or function calls.
        /// </summary>
        public const int DefaultMaxNestingDepth = 64;

        /// <summary>
        /// Default maximum consecutive unary operators.
        /// </summary>
        public const int DefaultMaxUnaryOperatorCount = 128;

        /// <summary>
        /// Default maximum emitted tokens.
        /// </summary>
        public const int DefaultMaxTokenCount = 2048;

        /// <summary>
        /// Default maximum diagnostics retained for one parse.
        /// </summary>
        public const int DefaultMaxDiagnosticCount = 100;

        /// <summary>
        /// Creates options using V1 defaults.
        /// </summary>
        public ExpressionParseOptions()
        {
            MaxSourceLength = DefaultMaxSourceLength;
            MaxNestingDepth = DefaultMaxNestingDepth;
            MaxUnaryOperatorCount = DefaultMaxUnaryOperatorCount;
            MaxTokenCount = DefaultMaxTokenCount;
            MaxDiagnosticCount = DefaultMaxDiagnosticCount;
        }

        /// <summary>
        /// Gets or sets the maximum source length.
        /// </summary>
        public int MaxSourceLength { get; set; }

        /// <summary>
        /// Gets or sets the maximum nested expression depth.
        /// </summary>
        public int MaxNestingDepth { get; set; }

        /// <summary>
        /// Gets or sets the maximum consecutive unary operator count.
        /// </summary>
        public int MaxUnaryOperatorCount { get; set; }

        /// <summary>
        /// Gets or sets the maximum token count, excluding an early EOF.
        /// </summary>
        public int MaxTokenCount { get; set; }

        /// <summary>
        /// Gets or sets the maximum diagnostics retained for one parse.
        /// </summary>
        public int MaxDiagnosticCount { get; set; }
    }
}
