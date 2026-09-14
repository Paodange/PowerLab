using System;

namespace PowerLab.Expressions
{
    /// <summary>
    /// Represents a binary operation.
    /// </summary>
    public sealed class BinaryExpressionSyntax : ExpressionSyntax
    {
        /// <summary>
        /// Creates a binary syntax node.
        /// </summary>
        public BinaryExpressionSyntax(
            ExpressionSyntax left,
            BinaryOperatorKind operatorKind,
            ExpressionSyntax right,
            TextSpan operatorSpan,
            TextSpan span)
            : base(ExpressionSyntaxKind.Binary, span)
        {
            if (left == null)
            {
                throw new ArgumentNullException(nameof(left));
            }

            if (right == null)
            {
                throw new ArgumentNullException(nameof(right));
            }

            Left = left;
            OperatorKind = operatorKind;
            Right = right;
            OperatorSpan = operatorSpan;
        }

        /// <summary>
        /// Gets the left operand.
        /// </summary>
        public ExpressionSyntax Left { get; }

        /// <summary>
        /// Gets the binary operator.
        /// </summary>
        public BinaryOperatorKind OperatorKind { get; }

        /// <summary>
        /// Gets the right operand.
        /// </summary>
        public ExpressionSyntax Right { get; }

        /// <summary>
        /// Gets the source span of the operator token.
        /// </summary>
        public TextSpan OperatorSpan { get; }
    }
}
