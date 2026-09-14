using System;

namespace PowerLab.Expressions
{
    /// <summary>
    /// Represents a right-associative conditional expression.
    /// </summary>
    public sealed class ConditionalExpressionSyntax : ExpressionSyntax
    {
        /// <summary>
        /// Creates a conditional syntax node.
        /// </summary>
        public ConditionalExpressionSyntax(
            ExpressionSyntax condition,
            ExpressionSyntax whenTrue,
            ExpressionSyntax whenFalse,
            TextSpan questionSpan,
            TextSpan colonSpan,
            TextSpan span)
            : base(ExpressionSyntaxKind.Conditional, span)
        {
            if (condition == null)
            {
                throw new ArgumentNullException(nameof(condition));
            }

            if (whenTrue == null)
            {
                throw new ArgumentNullException(nameof(whenTrue));
            }

            if (whenFalse == null)
            {
                throw new ArgumentNullException(nameof(whenFalse));
            }

            Condition = condition;
            WhenTrue = whenTrue;
            WhenFalse = whenFalse;
            QuestionSpan = questionSpan;
            ColonSpan = colonSpan;
        }

        /// <summary>
        /// Gets the condition expression.
        /// </summary>
        public ExpressionSyntax Condition { get; }

        /// <summary>
        /// Gets the expression selected when the condition is true.
        /// </summary>
        public ExpressionSyntax WhenTrue { get; }

        /// <summary>
        /// Gets the expression selected when the condition is false.
        /// </summary>
        public ExpressionSyntax WhenFalse { get; }

        /// <summary>
        /// Gets the source span of the question mark.
        /// </summary>
        public TextSpan QuestionSpan { get; }

        /// <summary>
        /// Gets the source span of the colon.
        /// </summary>
        public TextSpan ColonSpan { get; }
    }
}
