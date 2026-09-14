using System;

namespace PowerLab.Expressions
{
    /// <summary>
    /// A lexical token with its original source text and location.
    /// </summary>
    public sealed class Token
    {
        /// <summary>
        /// Creates a token.
        /// </summary>
        public Token(TokenKind kind, string text, TextSpan span, object? value = null)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            Kind = kind;
            Text = text;
            Span = span;
            Value = value;
        }

        /// <summary>
        /// Gets the token kind.
        /// </summary>
        public TokenKind Kind { get; }

        /// <summary>
        /// Gets the original token text.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Gets the original token text as an explicit raw-text alias.
        /// </summary>
        public string RawText
        {
            get { return Text; }
        }

        /// <summary>
        /// Gets the token source span.
        /// </summary>
        public TextSpan Span { get; }

        /// <summary>
        /// Gets the token start position.
        /// </summary>
        public TextPosition Position
        {
            get { return Span.Start; }
        }

        /// <summary>
        /// Gets the parsed scalar value, when the token is a literal or keyword.
        /// </summary>
        public object? Value { get; }

        /// <summary>
        /// Gets the integer value, when this token is an integer literal.
        /// </summary>
        public int? IntegerValue
        {
            get { return Value is int integer ? integer : (int?)null; }
        }

        /// <summary>
        /// Gets the number value, when this token is a number literal.
        /// </summary>
        public double? NumberValue
        {
            get { return Value is double number ? number : (double?)null; }
        }

        /// <summary>
        /// Gets the Boolean value, when this token is a Boolean keyword.
        /// </summary>
        public bool? BooleanValue
        {
            get { return Value is bool boolean ? boolean : (bool?)null; }
        }

        /// <summary>
        /// Gets the decoded string value, when this token is a string literal.
        /// </summary>
        public string? StringValue
        {
            get { return Value as string; }
        }
    }
}
