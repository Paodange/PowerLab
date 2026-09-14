using System;

namespace PowerLab.Expressions
{
    /// <summary>
    /// Identifies a character position in expression source text.
    /// Offsets, lines and columns are all zero-based. A position at the end of
    /// the source is valid and is used by EOF diagnostics.
    /// </summary>
    public readonly struct TextPosition : IEquatable<TextPosition>
    {
        /// <summary>
        /// Creates a source position.
        /// </summary>
        public TextPosition(int offset, int line, int column)
        {
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if (line < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(line));
            }

            if (column < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(column));
            }

            Offset = offset;
            Line = line;
            Column = column;
        }

        /// <summary>
        /// Gets the zero-based UTF-16 character offset.
        /// </summary>
        public int Offset { get; }

        /// <summary>
        /// Gets the zero-based line number.
        /// </summary>
        public int Line { get; }

        /// <summary>
        /// Gets the zero-based column number.
        /// </summary>
        public int Column { get; }

        /// <inheritdoc />
        public bool Equals(TextPosition other)
        {
            return Offset == other.Offset && Line == other.Line && Column == other.Column;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is TextPosition other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Offset, Line, Column);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format("({0},{1})", Line, Column);
        }

        /// <summary>
        /// Compares two source positions.
        /// </summary>
        public static bool operator ==(TextPosition left, TextPosition right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two source positions.
        /// </summary>
        public static bool operator !=(TextPosition left, TextPosition right)
        {
            return !left.Equals(right);
        }
    }
}
