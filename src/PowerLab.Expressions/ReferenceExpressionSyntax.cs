using System;
using System.Collections.Generic;

namespace PowerLab.Expressions
{
    /// <summary>
    /// Represents a dot-separated symbolic reference path.
    /// </summary>
    public sealed class ReferenceExpressionSyntax : ExpressionSyntax
    {
        /// <summary>
        /// Creates a reference syntax node.
        /// </summary>
        public ReferenceExpressionSyntax(IReadOnlyList<string> path, TextSpan span)
            : base(ExpressionSyntaxKind.Reference, span)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (path.Count == 0)
            {
                throw new ArgumentException("A reference must contain at least one path segment.", nameof(path));
            }

            List<string> copy = new List<string>(path.Count);
            for (int index = 0; index < path.Count; index++)
            {
                if (string.IsNullOrEmpty(path[index]))
                {
                    throw new ArgumentException("A reference path segment cannot be empty.", nameof(path));
                }

                copy.Add(path[index]);
            }

            Path = copy.AsReadOnly();
        }

        /// <summary>
        /// Gets the immutable path segments in source order.
        /// </summary>
        public IReadOnlyList<string> Path { get; }

        /// <summary>
        /// Gets the first path segment.
        /// </summary>
        public string RootName
        {
            get { return Path[0]; }
        }
    }
}
