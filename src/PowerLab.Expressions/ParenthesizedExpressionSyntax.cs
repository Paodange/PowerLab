using System;

namespace PowerLab.Expressions
{
    /// <summary>
    /// Preserves a pair of parentheses around an expression.
    /// </summary>
    public sealed class ParenthesizedExpressionSyntax : ExpressionSyntax
    {
        /// <summary>
        /// Creates a parenthesized syntax node.
        /// </summary>
        public ParenthesizedExpressionSyntax(
            ExpressionSyntax expression,
            TextSpan openParenSpan,
            TextSpan closeParenSpan,
            TextSpan span)
            : base(ExpressionSyntaxKind.Parenthesized, span)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            Expression = expression;
            OpenParenSpan = openParenSpan;
            CloseParenSpan = closeParenSpan;
        }

        /// <summary>
        /// Gets the enclosed expression.
        /// </summary>
        public ExpressionSyntax Expression { get; }

        /// <summary>
        /// Gets the opening-parenthesis span.
        /// </summary>
        public TextSpan OpenParenSpan { get; }

        /// <summary>
        /// Gets the closing-parenthesis span.
        /// </summary>
        public TextSpan CloseParenSpan { get; }
    }
}
