using System;

namespace PowerLab.Expressions
{
    /// <summary>
    /// A stable, user-facing diagnostic produced while lexing or parsing.
    /// </summary>
    public sealed class ExpressionDiagnostic
    {
        /// <summary>
        /// Creates a diagnostic.
        /// </summary>
        public ExpressionDiagnostic(
            string code,
            string message,
            TextSpan span,
            DiagnosticSeverity severity = DiagnosticSeverity.Error)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("A diagnostic code is required.", nameof(code));
            }

            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            Code = code;
            Message = message;
            Span = span;
            Severity = severity;
        }

        /// <summary>
        /// Gets the stable diagnostic code.
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Gets the display-friendly diagnostic message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the source span associated with the diagnostic.
        /// </summary>
        public TextSpan Span { get; }

        /// <summary>
        /// Gets the start position of the diagnostic.
        /// </summary>
        public TextPosition Position
        {
            get { return Span.Start; }
        }

        /// <summary>
        /// Gets the zero-based line of the diagnostic.
        /// </summary>
        public int Line
        {
            get { return Span.Start.Line; }
        }

        /// <summary>
        /// Gets the zero-based column of the diagnostic.
        /// </summary>
        public int Column
        {
            get { return Span.Start.Column; }
        }

        /// <summary>
        /// Gets the diagnostic severity.
        /// </summary>
        public DiagnosticSeverity Severity { get; }
    }
}
