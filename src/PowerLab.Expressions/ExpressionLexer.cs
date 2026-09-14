using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace PowerLab.Expressions
{
    /// <summary>
    /// Lexes the controlled PowerLab V1 expression language without executing it.
    /// </summary>
    public sealed class ExpressionLexer
    {
        private readonly ExpressionParseOptions _options;
        private readonly List<Token> _tokens = new List<Token>();
        private readonly List<ExpressionDiagnostic> _diagnostics = new List<ExpressionDiagnostic>();
        private string _source = string.Empty;
        private int _index;
        private TextPosition _position;
        private bool _tokenLimitReported;

        /// <summary>
        /// Creates a lexer using the V1 default limits.
        /// </summary>
        public ExpressionLexer()
            : this(new ExpressionParseOptions())
        {
        }

        /// <summary>
        /// Creates a lexer using the supplied limits.
        /// </summary>
        public ExpressionLexer(ExpressionParseOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            ValidateOptions(_options);
        }

        /// <summary>
        /// Lexes source using default limits.
        /// </summary>
        public static LexerResult Lex(string source)
        {
            return new ExpressionLexer().Analyze(source);
        }

        /// <summary>
        /// Lexes source using the supplied limits.
        /// </summary>
        public static LexerResult Lex(string source, ExpressionParseOptions options)
        {
            return new ExpressionLexer(options).Analyze(source);
        }

        /// <summary>
        /// Lexes source and returns tokens plus lexical diagnostics.
        /// </summary>
        public LexerResult Analyze(string source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            Reset(source);
            if (_source.Length > _options.MaxSourceLength)
            {
                AddDiagnostic(
                    "expression.syntax.sourceTooLong",
                    "The expression is longer than the configured source length limit.",
                    TextSpan.FromSource(_source));
                AddToken(TokenKind.BadToken, _source, TextSpan.FromSource(_source));
                AddEndOfFile();
                return CreateResult();
            }

            while (!IsAtEnd)
            {
                if (_tokens.Count >= _options.MaxTokenCount)
                {
                    AddDiagnostic(
                        "expression.syntax.sourceTooLong",
                        "The expression contains more tokens than the configured limit.",
                        CurrentSpan());
                    _tokenLimitReported = true;
                    MoveToEnd();
                    break;
                }

                if (char.IsWhiteSpace(Current))
                {
                    Advance();
                    continue;
                }

                ReadToken();
            }

            AddEndOfFile();
            return CreateResult();
        }

        /// <summary>
        /// Lexes source and returns the token sequence. Use <see cref="Analyze"/>
        /// when lexical diagnostics are also needed.
        /// </summary>
        public IReadOnlyList<Token> Tokenize(string source)
        {
            return Analyze(source).Tokens;
        }

        private bool IsAtEnd
        {
            get { return _index >= _source.Length; }
        }

        private char Current
        {
            get { return IsAtEnd ? '\0' : _source[_index]; }
        }

        private char Peek(int distance)
        {
            int peekIndex = _index + distance;
            return peekIndex >= 0 && peekIndex < _source.Length ? _source[peekIndex] : '\0';
        }

        private void Reset(string source)
        {
            _source = source;
            _tokens.Clear();
            _diagnostics.Clear();
            _index = 0;
            _position = new TextPosition(0, 0, 0);
            _tokenLimitReported = false;
        }

        private void ReadToken()
        {
            if (IsIdentifierStart(Current))
            {
                ReadIdentifier();
                return;
            }

            if (char.IsDigit(Current))
            {
                ReadNumber();
                return;
            }

            if (Current == '"')
            {
                ReadString();
                return;
            }

            if (Current == '/' && Peek(1) == '/')
            {
                ReadLineComment();
                return;
            }

            if (Current == '/' && Peek(1) == '*')
            {
                ReadBlockComment();
                return;
            }

            TextPosition start = _position;
            char character = Current;
            switch (character)
            {
                case '+':
                    if (Peek(1) == '+')
                    {
                        ReadUnsupportedOperator("++");
                    }
                    else
                    {
                        AdvanceAndAdd(TokenKind.Plus, start);
                    }

                    return;
                case '-':
                    if (Peek(1) == '-')
                    {
                        ReadUnsupportedOperator("--");
                    }
                    else
                    {
                        AdvanceAndAdd(TokenKind.Minus, start);
                    }

                    return;
                case '*':
                    if (Peek(1) == '*')
                    {
                        ReadUnsupportedOperator("**");
                    }
                    else
                    {
                        AdvanceAndAdd(TokenKind.Star, start);
                    }

                    return;
                case '/':
                    AdvanceAndAdd(TokenKind.Slash, start);
                    return;
                case '%':
                    AdvanceAndAdd(TokenKind.Percent, start);
                    return;
                case '=':
                    if (Peek(1) == '=')
                    {
                        Advance();
                        AdvanceAndAdd(TokenKind.EqualsEquals, start);
                    }
                    else
                    {
                        ReadUnsupportedOperator("=");
                    }

                    return;
                case '!':
                    if (Peek(1) == '=')
                    {
                        Advance();
                        AdvanceAndAdd(TokenKind.BangEquals, start);
                    }
                    else
                    {
                        AdvanceAndAdd(TokenKind.Bang, start);
                    }

                    return;
                case '>':
                    if (Peek(1) == '=')
                    {
                        Advance();
                        AdvanceAndAdd(TokenKind.GreaterThanOrEqual, start);
                    }
                    else if (Peek(1) == '>')
                    {
                        ReadUnsupportedOperator(">>");
                    }
                    else
                    {
                        AdvanceAndAdd(TokenKind.GreaterThan, start);
                    }

                    return;
                case '<':
                    if (Peek(1) == '=')
                    {
                        Advance();
                        AdvanceAndAdd(TokenKind.LessThanOrEqual, start);
                    }
                    else if (Peek(1) == '<')
                    {
                        ReadUnsupportedOperator("<<");
                    }
                    else
                    {
                        AdvanceAndAdd(TokenKind.LessThan, start);
                    }

                    return;
                case '&':
                    if (Peek(1) == '&')
                    {
                        Advance();
                        AdvanceAndAdd(TokenKind.AmpersandAmpersand, start);
                    }
                    else
                    {
                        ReadUnsupportedOperator("&");
                    }

                    return;
                case '|':
                    if (Peek(1) == '|')
                    {
                        Advance();
                        AdvanceAndAdd(TokenKind.PipePipe, start);
                    }
                    else
                    {
                        ReadUnsupportedOperator("|");
                    }

                    return;
                case '^':
                    ReadUnsupportedOperator("^");
                    return;
                case '?':
                    if (Peek(1) == '?' || Peek(1) == '.')
                    {
                        ReadUnsupportedOperator(string.Concat("?", Peek(1)));
                    }
                    else
                    {
                        AdvanceAndAdd(TokenKind.Question, start);
                    }

                    return;
                case ':':
                    AdvanceAndAdd(TokenKind.Colon, start);
                    return;
                case '(':
                    AdvanceAndAdd(TokenKind.OpenParen, start);
                    return;
                case ')':
                    AdvanceAndAdd(TokenKind.CloseParen, start);
                    return;
                case ',':
                    AdvanceAndAdd(TokenKind.Comma, start);
                    return;
                case '.':
                    AdvanceAndAdd(TokenKind.Dot, start);
                    return;
                default:
                    ReadUnexpectedCharacter();
                    return;
            }
        }

        private void ReadIdentifier()
        {
            TextPosition start = _position;
            while (IsIdentifierPart(Current))
            {
                Advance();
            }

            string text = Slice(start);
            if (text == "true")
            {
                AddToken(TokenKind.TrueKeyword, text, SpanFrom(start), true);
            }
            else if (text == "false")
            {
                AddToken(TokenKind.FalseKeyword, text, SpanFrom(start), false);
            }
            else
            {
                AddToken(TokenKind.Identifier, text, SpanFrom(start), text);
            }
        }

        private void ReadNumber()
        {
            TextPosition start = _position;
            bool invalid = false;
            while (char.IsDigit(Current))
            {
                Advance();
            }

            bool hasFraction = false;
            if (Current == '.')
            {
                hasFraction = true;
                Advance();
                if (!char.IsDigit(Current))
                {
                    invalid = true;
                }

                while (char.IsDigit(Current))
                {
                    Advance();
                }
            }

            if (Current == 'e' || Current == 'E')
            {
                hasFraction = true;
                Advance();
                if (Current == '+' || Current == '-')
                {
                    Advance();
                }

                if (!char.IsDigit(Current))
                {
                    invalid = true;
                }

                while (char.IsDigit(Current))
                {
                    Advance();
                }
            }

            if (Current == '.' || IsIdentifierPart(Current))
            {
                invalid = true;
                while (Current == '.' || IsIdentifierPart(Current))
                {
                    Advance();
                }
            }

            TextSpan span = SpanFrom(start);
            string text = Slice(start);
            if (invalid)
            {
                AddInvalidNumber(text, span);
                return;
            }

            if (!hasFraction)
            {
                int integerValue;
                if (int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out integerValue))
                {
                    AddToken(TokenKind.IntegerLiteral, text, span, integerValue);
                    return;
                }
            }
            else
            {
                double numberValue;
                if (double.TryParse(
                    text,
                    NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent,
                    CultureInfo.InvariantCulture,
                    out numberValue) && !double.IsInfinity(numberValue))
                {
                    AddToken(TokenKind.NumberLiteral, text, span, numberValue);
                    return;
                }
            }

            AddInvalidNumber(text, span);
        }

        private void ReadString()
        {
            TextPosition start = _position;
            StringBuilder value = new StringBuilder();
            bool invalid = false;
            Advance();

            while (!IsAtEnd)
            {
                if (Current == '"')
                {
                    Advance();
                    TextSpan span = SpanFrom(start);
                    if (invalid)
                    {
                        AddToken(TokenKind.BadToken, Slice(start), span, value.ToString());
                    }
                    else
                    {
                        AddToken(TokenKind.StringLiteral, Slice(start), span, value.ToString());
                    }

                    return;
                }

                if (Current == '\r' || Current == '\n')
                {
                    invalid = true;
                    AddDiagnostic(
                        "expression.syntax.unterminatedString",
                        "The string literal is not terminated before the end of the line.",
                        SpanFrom(start));
                    AddToken(TokenKind.BadToken, Slice(start), SpanFrom(start), value.ToString());
                    return;
                }

                if (Current != '\\')
                {
                    value.Append(Current);
                    Advance();
                    continue;
                }

                TextPosition escapeStart = _position;
                Advance();
                if (IsAtEnd)
                {
                    invalid = true;
                    AddDiagnostic(
                        "expression.syntax.invalidEscape",
                        "The string ends after an escape character.",
                        SpanFrom(escapeStart));
                    break;
                }

                char escaped = Current;
                switch (escaped)
                {
                    case '\\':
                        value.Append('\\');
                        Advance();
                        break;
                    case '"':
                        value.Append('"');
                        Advance();
                        break;
                    case 'n':
                        value.Append('\n');
                        Advance();
                        break;
                    case 'r':
                        value.Append('\r');
                        Advance();
                        break;
                    case 't':
                        value.Append('\t');
                        Advance();
                        break;
                    case 'u':
                        ReadUnicodeEscape(escapeStart, value, ref invalid);
                        break;
                    default:
                        invalid = true;
                        Advance();
                        AddDiagnostic(
                            "expression.syntax.invalidEscape",
                            string.Format("The escape sequence \\{0} is not supported.", escaped),
                            SpanFrom(escapeStart));
                        value.Append(escaped);
                        break;
                }
            }

            AddDiagnostic(
                "expression.syntax.unterminatedString",
                "The string literal is not terminated before the end of the expression.",
                SpanFrom(start));
            AddToken(TokenKind.BadToken, Slice(start), SpanFrom(start), value.ToString());
        }

        private void ReadUnicodeEscape(TextPosition escapeStart, StringBuilder value, ref bool invalid)
        {
            Advance();
            int codePoint = 0;
            bool valid = true;
            for (int index = 0; index < 4; index++)
            {
                if (IsHexDigit(Current))
                {
                    codePoint = (codePoint * 16) + HexValue(Current);
                    Advance();
                }
                else
                {
                    valid = false;
                    if (!IsAtEnd && Current != '"' && Current != '\r' && Current != '\n')
                    {
                        Advance();
                    }
                }
            }

            if (!valid)
            {
                invalid = true;
                AddDiagnostic(
                    "expression.syntax.invalidUnicodeEscape",
                    "A Unicode escape must contain exactly four hexadecimal digits.",
                    SpanFrom(escapeStart));
            }

            value.Append((char)codePoint);
        }

        private void ReadLineComment()
        {
            TextPosition start = _position;
            Advance();
            Advance();
            while (!IsAtEnd && Current != '\r' && Current != '\n')
            {
                Advance();
            }

            AddUnsupportedSyntax("Comments are not supported in expressions.", SpanFrom(start));
            AddToken(TokenKind.BadToken, Slice(start), SpanFrom(start));
        }

        private void ReadBlockComment()
        {
            TextPosition start = _position;
            Advance();
            Advance();
            while (!IsAtEnd)
            {
                if (Current == '*' && Peek(1) == '/')
                {
                    Advance();
                    Advance();
                    break;
                }

                Advance();
            }

            AddUnsupportedSyntax("Comments are not supported in expressions.", SpanFrom(start));
            AddToken(TokenKind.BadToken, Slice(start), SpanFrom(start));
        }

        private void ReadUnsupportedOperator(string text)
        {
            TextPosition start = _position;
            for (int index = 0; index < text.Length && !IsAtEnd; index++)
            {
                Advance();
            }

            AddUnsupportedSyntax(
                string.Format("The operator '{0}' is not supported in V1 expressions.", text),
                SpanFrom(start));
            AddToken(TokenKind.BadToken, Slice(start), SpanFrom(start));
        }

        private void ReadUnexpectedCharacter()
        {
            TextPosition start = _position;
            char character = Current;
            Advance();
            AddDiagnostic(
                "expression.syntax.unexpectedCharacter",
                string.Format("The character '{0}' is not valid in an expression.", character),
                SpanFrom(start));
            AddToken(TokenKind.BadToken, Slice(start), SpanFrom(start));
        }

        private void AddInvalidNumber(string text, TextSpan span)
        {
            AddDiagnostic(
                "expression.syntax.invalidNumber",
                string.Format("The number '{0}' is not a valid V1 literal.", text),
                span);
            AddToken(TokenKind.BadToken, text, span);
        }

        private void AddUnsupportedSyntax(string message, TextSpan span)
        {
            AddDiagnostic("expression.syntax.unsupportedSyntax", message, span);
        }

        private void AdvanceAndAdd(TokenKind kind, TextPosition start)
        {
            Advance();
            AddToken(kind, Slice(start), SpanFrom(start));
        }

        private void Advance()
        {
            if (IsAtEnd)
            {
                return;
            }

            char character = Current;
            _index++;
            if (character == '\r')
            {
                if (!IsAtEnd && Current == '\n')
                {
                    _index++;
                }

                _position = new TextPosition(_index, _position.Line + 1, 0);
            }
            else if (character == '\n')
            {
                _position = new TextPosition(_index, _position.Line + 1, 0);
            }
            else
            {
                _position = new TextPosition(_index, _position.Line, _position.Column + 1);
            }
        }

        private void MoveToEnd()
        {
            _index = _source.Length;
            _position = TextSpan.FromSource(_source).End;
        }

        private void AddEndOfFile()
        {
            if (!_tokenLimitReported || _tokens.Count < _options.MaxTokenCount + 1)
            {
                AddToken(TokenKind.EndOfFile, string.Empty, CurrentSpan());
            }
        }

        private void AddToken(TokenKind kind, string text, TextSpan span, object? value = null)
        {
            _tokens.Add(new Token(kind, text, span, value));
        }

        private void AddDiagnostic(string code, string message, TextSpan span)
        {
            if (_diagnostics.Count < _options.MaxDiagnosticCount)
            {
                _diagnostics.Add(new ExpressionDiagnostic(code, message, span));
            }
        }

        private LexerResult CreateResult()
        {
            return new LexerResult(
                new List<Token>(_tokens).AsReadOnly(),
                new List<ExpressionDiagnostic>(_diagnostics).AsReadOnly());
        }

        private TextSpan CurrentSpan()
        {
            return new TextSpan(_position, _position);
        }

        private TextSpan SpanFrom(TextPosition start)
        {
            return new TextSpan(start, _position);
        }

        private string Slice(TextPosition start)
        {
            return _source.Substring(start.Offset, _index - start.Offset);
        }

        private static bool IsIdentifierStart(char character)
        {
            return character == '_' || char.IsLetter(character);
        }

        private static bool IsIdentifierPart(char character)
        {
            return character == '_' || char.IsLetterOrDigit(character);
        }

        private static bool IsHexDigit(char character)
        {
            return (character >= '0' && character <= '9')
                || (character >= 'a' && character <= 'f')
                || (character >= 'A' && character <= 'F');
        }

        private static int HexValue(char character)
        {
            if (character >= '0' && character <= '9')
            {
                return character - '0';
            }

            if (character >= 'a' && character <= 'f')
            {
                return character - 'a' + 10;
            }

            return character - 'A' + 10;
        }

        private static void ValidateOptions(ExpressionParseOptions options)
        {
            if (options.MaxSourceLength <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(options.MaxSourceLength));
            }

            if (options.MaxNestingDepth <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(options.MaxNestingDepth));
            }

            if (options.MaxUnaryOperatorCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(options.MaxUnaryOperatorCount));
            }

            if (options.MaxTokenCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(options.MaxTokenCount));
            }

            if (options.MaxDiagnosticCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(options.MaxDiagnosticCount));
            }
        }
    }
}
