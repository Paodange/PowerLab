using System;
using System.Collections.Generic;

namespace PowerLab.Expressions
{
    /// <summary>
    /// The result of parsing one expression source string.
    /// </summary>
    public sealed class ExpressionParseResult
    {
        /// <summary>
        /// Creates a parse result.
        /// </summary>
        public ExpressionParseResult(ExpressionSyntax? root, IReadOnlyList<ExpressionDiagnostic> diagnostics)
        {
            if (diagnostics == null)
            {
                throw new ArgumentNullException(nameof(diagnostics));
            }

            Root = root;
            Diagnostics = new List<ExpressionDiagnostic>(diagnostics).AsReadOnly();
        }

        /// <summary>
        /// Gets the syntax tree when parsing succeeds; otherwise null.
        /// </summary>
        public ExpressionSyntax? Root { get; }

        /// <summary>
        /// Gets the read-only diagnostics.
        /// </summary>
        public IReadOnlyList<ExpressionDiagnostic> Diagnostics { get; }

        /// <summary>
        /// Gets whether a complete syntax tree was produced without errors.
        /// </summary>
        public bool Success
        {
            get { return Root != null && Diagnostics.Count == 0; }
        }
    }
}
