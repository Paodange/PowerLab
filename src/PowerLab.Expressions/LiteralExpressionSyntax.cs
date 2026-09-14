using System;

namespace PowerLab.Expressions
{
    /// <summary>
    /// Represents an integer, number, Boolean or string literal.
    /// </summary>
    public sealed class LiteralExpressionSyntax : ExpressionSyntax
    {
        /// <summary>
        /// Creates a literal syntax node.
        /// </summary>
        public LiteralExpressionSyntax(LiteralValueKind literalKind, object value, TextSpan span)
            : base(ExpressionSyntaxKind.Literal, span)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            LiteralKind = literalKind;
            Value = value;
        }

        /// <summary>
        /// Gets the scalar literal kind.
        /// </summary>
        public LiteralValueKind LiteralKind { get; }

        /// <summary>
        /// Gets the parsed scalar value.
        /// </summary>
        public object Value { get; }
    }
}
