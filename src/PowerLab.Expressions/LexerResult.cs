using System;
using System.Collections.Generic;

namespace PowerLab.Expressions
{
    /// <summary>
    /// The tokens and lexical diagnostics for a source string.
    /// </summary>
    public sealed class LexerResult
    {
        /// <summary>
        /// Creates a lexer result.
        /// </summary>
        public LexerResult(IReadOnlyList<Token> tokens, IReadOnlyList<ExpressionDiagnostic> diagnostics)
        {
            if (tokens == null)
            {
                throw new ArgumentNullException(nameof(tokens));
            }

            if (diagnostics == null)
            {
                throw new ArgumentNullException(nameof(diagnostics));
            }

            Tokens = new List<Token>(tokens).AsReadOnly();
            Diagnostics = new List<ExpressionDiagnostic>(diagnostics).AsReadOnly();
        }

        /// <summary>
        /// Gets the tokens, including the EOF token.
        /// </summary>
        public IReadOnlyList<Token> Tokens { get; }

        /// <summary>
        /// Gets the lexical diagnostics.
        /// </summary>
        public IReadOnlyList<ExpressionDiagnostic> Diagnostics { get; }

        /// <summary>
        /// Gets whether lexing produced no errors.
        /// </summary>
        public bool Success
        {
            get { return Diagnostics.Count == 0; }
        }
    }
}
