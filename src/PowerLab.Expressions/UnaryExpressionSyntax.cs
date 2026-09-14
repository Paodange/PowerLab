using System;

namespace PowerLab.Expressions
{
    /// <summary>
    /// Represents a unary operation.
    /// </summary>
    public sealed class UnaryExpressionSyntax : ExpressionSyntax
    {
        /// <summary>
        /// Creates a unary syntax node.
        /// </summary>
        public UnaryExpressionSyntax(
            UnaryOperatorKind operatorKind,
            ExpressionSyntax operand,
            TextSpan operatorSpan,
            TextSpan span)
            : base(ExpressionSyntaxKind.Unary, span)
        {
            if (operand == null)
            {
                throw new ArgumentNullException(nameof(operand));
            }

            OperatorKind = operatorKind;
            Operand = operand;
            OperatorSpan = operatorSpan;
        }

        /// <summary>
        /// Gets the unary operator.
        /// </summary>
        public UnaryOperatorKind OperatorKind { get; }

        /// <summary>
        /// Gets the operand.
        /// </summary>
        public ExpressionSyntax Operand { get; }

        /// <summary>
        /// Gets the source span of the operator token.
        /// </summary>
        public TextSpan OperatorSpan { get; }
    }
}
