using System;

namespace PowerLab.Expressions
{
    /// <summary>
    /// Describes a half-open range in expression source text.
    /// </summary>
    public readonly struct TextSpan : IEquatable<TextSpan>
    {
        /// <summary>
        /// Creates a span from its start and exclusive end positions.
        /// </summary>
        public TextSpan(TextPosition start, TextPosition end)
        {
            if (end.Offset < start.Offset)
            {
                throw new ArgumentException("The span end cannot precede its start.", nameof(end));
            }

            Start = start;
            End = end;
        }

        /// <summary>
        /// Gets the inclusive start position.
        /// </summary>
        public TextPosition Start { get; }

        /// <summary>
        /// Gets the exclusive end position.
        /// </summary>
        public TextPosition End { get; }

        /// <summary>
        /// Gets the number of UTF-16 characters in the span.
        /// </summary>
        public int Length
        {
            get { return End.Offset - Start.Offset; }
        }

        /// <summary>
        /// Gets the zero-based start offset.
        /// </summary>
        public int StartOffset
        {
            get { return Start.Offset; }
        }

        /// <summary>
        /// Gets the exclusive zero-based end offset.
        /// </summary>
        public int EndOffset
        {
            get { return End.Offset; }
        }

        /// <summary>
        /// Gets the zero-based start line.
        /// </summary>
        public int StartLine
        {
            get { return Start.Line; }
        }

        /// <summary>
        /// Gets the zero-based start column.
        /// </summary>
        public int StartColumn
        {
            get { return Start.Column; }
        }

        /// <summary>
        /// Gets the zero-based end line.
        /// </summary>
        public int EndLine
        {
            get { return End.Line; }
        }

        /// <summary>
        /// Gets the zero-based end column.
        /// </summary>
        public int EndColumn
        {
            get { return End.Column; }
        }

        /// <summary>
        /// Creates a span covering the complete source text.
        /// </summary>
        public static TextSpan FromSource(string source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            TextPosition position = new TextPosition(0, 0, 0);
            int index = 0;
            while (index < source.Length)
            {
                char character = source[index];
                index++;
                if (character == '\r')
                {
                    if (index < source.Length && source[index] == '\n')
                    {
                        index++;
                    }

                    position = new TextPosition(index, position.Line + 1, 0);
                }
                else if (character == '\n')
                {
                    position = new TextPosition(index, position.Line + 1, 0);
                }
                else
                {
                    position = new TextPosition(index, position.Line, position.Column + 1);
                }
            }

            return new TextSpan(new TextPosition(0, 0, 0), position);
        }

        /// <summary>
        /// Creates a span between two positions.
        /// </summary>
        public static TextSpan FromBounds(TextPosition start, TextPosition end)
        {
            return new TextSpan(start, end);
        }

        /// <inheritdoc />
        public bool Equals(TextSpan other)
        {
            return Start == other.Start && End == other.End;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is TextSpan other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Start, End);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("[{0}..{1})", Start.Offset, End.Offset);
        }
    }
}
