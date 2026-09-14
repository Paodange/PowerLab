using System;
using System.Collections.Generic;

namespace PowerLab.Expressions
{
    /// <summary>
    /// Represents a call to a bare, named function.
    /// </summary>
    public sealed class FunctionCallExpressionSyntax : ExpressionSyntax
    {
        /// <summary>
        /// Creates a function call syntax node.
        /// </summary>
        public FunctionCallExpressionSyntax(
            string name,
            IReadOnlyList<ExpressionSyntax> arguments,
            TextSpan nameSpan,
            TextSpan openParenSpan,
            TextSpan? closeParenSpan,
            TextSpan span)
            : base(ExpressionSyntaxKind.FunctionCall, span)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("A function name is required.", nameof(name));
            }

            if (arguments == null)
            {
                throw new ArgumentNullException(nameof(arguments));
            }

            Name = name;
            Arguments = new List<ExpressionSyntax>(arguments).AsReadOnly();
            NameSpan = nameSpan;
            OpenParenSpan = openParenSpan;
            CloseParenSpan = closeParenSpan;
        }

        /// <summary>
        /// Gets the bare function name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the immutable argument list.
        /// </summary>
        public IReadOnlyList<ExpressionSyntax> Arguments { get; }

        /// <summary>
        /// Gets the source span of the function name.
        /// </summary>
        public TextSpan NameSpan { get; }

        /// <summary>
        /// Gets the source span of the opening parenthesis.
        /// </summary>
        public TextSpan OpenParenSpan { get; }

        /// <summary>
        /// Gets the source span of the closing parenthesis, if present.
        /// </summary>
        public TextSpan? CloseParenSpan { get; }
    }
}
