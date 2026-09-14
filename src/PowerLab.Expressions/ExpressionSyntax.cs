using System;

namespace PowerLab.Expressions
{
    /// <summary>
    /// Base type for all immutable V1 expression syntax nodes.
    /// </summary>
    public abstract class ExpressionSyntax
    {
        /// <summary>
        /// Creates a syntax node.
        /// </summary>
        protected ExpressionSyntax(ExpressionSyntaxKind kind, TextSpan span)
        {
            Kind = kind;
            Span = span;
        }

        /// <summary>
        /// Gets the syntax node kind.
        /// </summary>
        public ExpressionSyntaxKind Kind { get; }

        /// <summary>
        /// Gets the complete source span of this node.
        /// </summary>
        public TextSpan Span { get; }
    }
}
