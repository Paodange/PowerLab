using System;
using System.Collections.Generic;

namespace PowerLab.Expressions
{
    /// <summary>
    /// Parses the controlled PowerLab V1 expression language into an immutable AST.
    /// </summary>
    public sealed class ExpressionParser
    {
        private readonly ExpressionParseOptions _options;
        private IReadOnlyList<Token> _tokens = Array.Empty<Token>();
        private readonly List<ExpressionDiagnostic> _diagnostics = new List<ExpressionDiagnostic>();
        private int _index;
        private int _parenthesisDepth;
        private int _unaryOperatorCount;
        private int _conditionalDepth;

        /// <summary>
        /// Creates a parser using the V1 default limits.
        /// </summary>
        public ExpressionParser()
            : this(new ExpressionParseOptions())
        {
        }

        /// <summary>
        /// Creates a parser using the supplied limits.
        /// </summary>
        public ExpressionParser(ExpressionParseOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            ValidateOptions(_options);
        }

        /// <summary>
        /// Parses one source string using the V1 default limits.
        /// </summary>
        public static ExpressionParseResult Parse(string source)
        {
            return new ExpressionParser().ParseText(source);
        }

        /// <summary>
        /// Parses one source string using the supplied limits.
        /// </summary>
        public static ExpressionParseResult Parse(string source, ExpressionParseOptions options)
        {
            return new ExpressionParser(options).ParseText(source);
        }

        /// <summary>
        /// Parses one source string with this parser's limits.
        /// </summary>
        public ExpressionParseResult ParseText(string source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (source.Length > _options.MaxSourceLength)
            {
                ExpressionDiagnostic diagnostic = new ExpressionDiagnostic(
                    "expression.syntax.sourceTooLong",
                    "The expression is longer than the configured source length limit.",
                    TextSpan.FromSource(source));
                return new ExpressionParseResult(null, new[] { diagnostic });
            }

            LexerResult lexerResult = new ExpressionLexer(_options).Analyze(source);
            _tokens = lexerResult.Tokens;
            _diagnostics.Clear();
            for (int diagnosticIndex = 0; diagnosticIndex < lexerResult.Diagnostics.Count; diagnosticIndex++)
            {
                _diagnostics.Add(lexerResult.Diagnostics[diagnosticIndex]);
            }

            _index = 0;
            _parenthesisDepth = 0;
            _unaryOperatorCount = 0;
            _conditionalDepth = 0;

            ExpressionSyntax? root = null;
            if (!IsAtEnd)
            {
                root = ParseConditional();
                if (!IsAtEnd)
                {
                    AddDiagnostic(
                        "expression.syntax.trailingInput",
                        "Unexpected input appears after the expression.",
                        Current.Span);
                    SkipToEnd();
                }
            }
            else
            {
                AddDiagnostic(
                    "expression.syntax.expectedExpression",
                    "An expression is required.",
                    Current.Span);
            }

            IReadOnlyList<ExpressionDiagnostic> diagnostics =
                new List<ExpressionDiagnostic>(_diagnostics).AsReadOnly();
            if (_diagnostics.Count != 0 || root == null)
            {
                return new ExpressionParseResult(null, diagnostics);
            }

            return new ExpressionParseResult(root, diagnostics);
        }

        private Token Current
        {
            get
            {
                if (_index < _tokens.Count)
                {
                    return _tokens[_index];
                }

                return _tokens[_tokens.Count - 1];
            }
        }

        private bool IsAtEnd
        {
            get { return Current.Kind == TokenKind.EndOfFile; }
        }

        private Token Advance()
        {
            Token token = Current;
            if (!IsAtEnd)
            {
                _index++;
            }

            return token;
        }

        private bool Match(TokenKind kind)
        {
            if (Current.Kind != kind)
            {
                return false;
            }

            Advance();
            return true;
        }

        private ExpressionSyntax? ParseConditional()
        {
            if (_conditionalDepth >= _options.MaxNestingDepth)
            {
                AddDiagnostic(
                    "expression.syntax.nestingLimitExceeded",
                    "The expression exceeds the configured nesting limit.",
                    Current.Span);
                SkipToEnd();
                return null;
            }

            _conditionalDepth++;
            try
            {
                ExpressionSyntax? condition = ParseLogicalOr();
                if (!Match(TokenKind.Question))
                {
                    return condition;
                }

                Token question = Previous;
                ExpressionSyntax? whenTrue = null;
                if (IsExpressionTerminator(Current.Kind))
                {
                    AddExpectedExpression();
                }
                else
                {
                    whenTrue = ParseConditional();
                }

                Token colon = Current;
                bool hasColon = Match(TokenKind.Colon);
                if (!hasColon)
                {
                    AddDiagnostic(
                        "expression.syntax.expectedToken",
                        "Expected ':' to separate the conditional branches.",
                        Current.Span);
                }

                ExpressionSyntax? whenFalse = null;
                if (IsExpressionTerminator(Current.Kind))
                {
                    AddExpectedExpression();
                }
                else
                {
                    whenFalse = ParseConditional();
                }

                if (!hasColon || condition == null || whenTrue == null || whenFalse == null)
                {
                    return null;
                }

                return new ConditionalExpressionSyntax(
                    condition,
                    whenTrue,
                    whenFalse,
                    question.Span,
                    colon.Span,
                    Combine(condition.Span, whenFalse.Span));
            }
            finally
            {
                _conditionalDepth--;
            }
        }

        private ExpressionSyntax? ParseLogicalOr()
        {
            return ParseBinaryLevel(ParseLogicalAnd, IsLogicalOr, TokenToBinaryOperator);
        }

        private ExpressionSyntax? ParseLogicalAnd()
        {
            return ParseBinaryLevel(ParseEquality, IsLogicalAnd, TokenToBinaryOperator);
        }

        private ExpressionSyntax? ParseEquality()
        {
            return ParseBinaryLevel(ParseRelational, IsEquality, TokenToBinaryOperator);
        }

        private ExpressionSyntax? ParseRelational()
        {
            return ParseBinaryLevel(ParseAdditive, IsRelational, TokenToBinaryOperator);
        }

        private ExpressionSyntax? ParseAdditive()
        {
            return ParseBinaryLevel(ParseMultiplicative, IsAdditive, TokenToBinaryOperator);
        }

        private ExpressionSyntax? ParseMultiplicative()
        {
            return ParseBinaryLevel(ParseUnary, IsMultiplicative, TokenToBinaryOperator);
        }

        private ExpressionSyntax? ParseBinaryLevel(
            Func<ExpressionSyntax?> parseOperand,
            Func<TokenKind, bool> isOperator,
            Func<TokenKind, BinaryOperatorKind> mapOperator)
        {
            ExpressionSyntax? left = parseOperand();
            while (isOperator(Current.Kind))
            {
                Token operatorToken = Advance();
                ExpressionSyntax? right = parseOperand();
                if (right == null)
                {
                    left = null;
                    continue;
                }

                if (left == null)
                {
                    continue;
                }

                left = new BinaryExpressionSyntax(
                    left,
                    mapOperator(operatorToken.Kind),
                    right,
                    operatorToken.Span,
                    Combine(left.Span, right.Span));
            }

            return left;
        }

        private ExpressionSyntax? ParseUnary()
        {
            UnaryOperatorKind operatorKind;
            if (!TryGetUnaryOperator(Current.Kind, out operatorKind))
            {
                return ParsePrimary();
            }

            Token operatorToken = Advance();
            if (_unaryOperatorCount >= _options.MaxUnaryOperatorCount)
            {
                AddDiagnostic(
                    "expression.syntax.nestingLimitExceeded",
                    "The expression contains more unary operators than the configured limit.",
                    operatorToken.Span);
                while (TryGetUnaryOperator(Current.Kind, out operatorKind))
                {
                    Advance();
                }

                return null;
            }

            _unaryOperatorCount++;
            try
            {
                ExpressionSyntax? operand = ParseUnary();
                if (operand == null)
                {
                    return null;
                }

                return new UnaryExpressionSyntax(
                    operatorKind,
                    operand,
                    operatorToken.Span,
                    Combine(operatorToken.Span, operand.Span));
            }
            finally
            {
                _unaryOperatorCount--;
            }
        }

        private ExpressionSyntax? ParsePrimary()
        {
            Token token = Current;
            switch (token.Kind)
            {
                case TokenKind.IntegerLiteral:
                    Advance();
                    return new LiteralExpressionSyntax(LiteralValueKind.Integer, token.Value!, token.Span);
                case TokenKind.NumberLiteral:
                    Advance();
                    return new LiteralExpressionSyntax(LiteralValueKind.Number, token.Value!, token.Span);
                case TokenKind.TrueKeyword:
                case TokenKind.FalseKeyword:
                    Advance();
                    return new LiteralExpressionSyntax(LiteralValueKind.Boolean, token.Value!, token.Span);
                case TokenKind.StringLiteral:
                    Advance();
                    return new LiteralExpressionSyntax(LiteralValueKind.String, token.Value!, token.Span);
                case TokenKind.Identifier:
                    return ParseIdentifierPrimary();
                case TokenKind.OpenParen:
                    return ParseParenthesized();
                default:
                    if (token.Kind != TokenKind.EndOfFile
                        && token.Kind != TokenKind.CloseParen
                        && token.Kind != TokenKind.Comma
                        && token.Kind != TokenKind.Colon)
                    {
                        Advance();
                    }

                    AddExpectedExpression();
                    return null;
            }
        }

        private ExpressionSyntax? ParseIdentifierPrimary()
        {
            Token first = Advance();
            string firstName = first.Text;
            if (IsUnsupportedWord(firstName))
            {
                AddDiagnostic(
                    "expression.syntax.unsupportedSyntax",
                    string.Format("The keyword '{0}' is not supported in expressions.", firstName),
                    first.Span);
                return null;
            }

            List<string> path = new List<string> { firstName };
            while (Match(TokenKind.Dot))
            {
                if (Current.Kind != TokenKind.Identifier)
                {
                    AddDiagnostic(
                        "expression.syntax.expectedToken",
                        "Expected an identifier after '.'.",
                        Current.Span);
                    return null;
                }

                path.Add(Advance().Text);
            }

            if (Current.Kind == TokenKind.OpenParen)
            {
                if (path.Count != 1)
                {
                    Token openParen = Advance();
                    AddDiagnostic(
                        "expression.syntax.unsupportedSyntax",
                        "Member method calls are not supported; use a bare function name.",
                        Combine(first.Span, openParen.Span));
                    SkipBalancedAfterOpenParen();
                    return null;
                }

                return ParseFunctionCall(first);
            }

            return new ReferenceExpressionSyntax(path.AsReadOnly(), Combine(first.Span, PreviousSpanOr(first.Span)));
        }

        private ExpressionSyntax? ParseParenthesized()
        {
            Token openParen = Advance();
            if (_parenthesisDepth >= _options.MaxNestingDepth)
            {
                AddDiagnostic(
                    "expression.syntax.nestingLimitExceeded",
                    "The expression exceeds the configured nesting limit.",
                    openParen.Span);
                SkipBalancedAfterOpenParen();
                return null;
            }

            _parenthesisDepth++;
            try
            {
                ExpressionSyntax? expression = ParseConditional();
                if (!Match(TokenKind.CloseParen))
                {
                    AddDiagnostic(
                        "expression.syntax.expectedToken",
                        "Expected ')' to close the parenthesized expression.",
                        Current.Span);
                    return null;
                }

                Token closeParen = Previous;
                if (expression == null)
                {
                    return null;
                }

                return new ParenthesizedExpressionSyntax(
                    expression,
                    openParen.Span,
                    closeParen.Span,
                    Combine(openParen.Span, closeParen.Span));
            }
            finally
            {
                _parenthesisDepth--;
            }
        }

        private ExpressionSyntax? ParseFunctionCall(Token nameToken)
        {
            Token openParen = Advance();
            if (_parenthesisDepth >= _options.MaxNestingDepth)
            {
                AddDiagnostic(
                    "expression.syntax.nestingLimitExceeded",
                    "The expression exceeds the configured nesting limit.",
                    openParen.Span);
                SkipBalancedAfterOpenParen();
                return null;
            }

            _parenthesisDepth++;
            try
            {
                List<ExpressionSyntax> arguments = new List<ExpressionSyntax>();
                TextSpan? closeParenSpan = null;
                if (IsAtEnd)
                {
                    AddExpectedExpression();
                }

                while (!IsAtEnd)
                {
                    if (Current.Kind == TokenKind.CloseParen)
                    {
                        closeParenSpan = Advance().Span;
                        break;
                    }

                    if (Current.Kind == TokenKind.Comma)
                    {
                        AddExpectedExpression();
                    }
                    else
                    {
                        ExpressionSyntax? argument = ParseConditional();
                        if (argument != null)
                        {
                            arguments.Add(argument);
                        }
                    }

                    if (Match(TokenKind.Comma))
                    {
                        if (Current.Kind == TokenKind.CloseParen || IsAtEnd)
                        {
                            AddExpectedExpression();
                            if (Current.Kind == TokenKind.CloseParen)
                            {
                                closeParenSpan = Advance().Span;
                            }

                            break;
                        }

                        continue;
                    }

                    if (Current.Kind == TokenKind.CloseParen)
                    {
                        closeParenSpan = Advance().Span;
                        break;
                    }

                    if (IsAtEnd)
                    {
                        AddDiagnostic(
                            "expression.syntax.expectedToken",
                            "Expected ')' to close the function argument list.",
                            Current.Span);
                        break;
                    }

                    AddDiagnostic(
                        "expression.syntax.expectedToken",
                        "Expected ',' between function arguments.",
                        Current.Span);
                    SkipToArgumentBoundary();
                    if (Current.Kind == TokenKind.CloseParen)
                    {
                        closeParenSpan = Advance().Span;
                    }

                    break;
                }

                if (IsAtEnd && closeParenSpan == null)
                {
                    AddDiagnostic(
                        "expression.syntax.expectedToken",
                        "Expected ')' to close the function argument list.",
                        Current.Span);
                }

                TextSpan span = closeParenSpan.HasValue
                    ? Combine(nameToken.Span, closeParenSpan.Value)
                    : Combine(nameToken.Span, openParen.Span);
                return new FunctionCallExpressionSyntax(
                    nameToken.Text,
                    arguments.AsReadOnly(),
                    nameToken.Span,
                    openParen.Span,
                    closeParenSpan,
                    span);
            }
            finally
            {
                _parenthesisDepth--;
            }
        }

        private void SkipBalancedAfterOpenParen()
        {
            int depth = 1;
            while (!IsAtEnd && depth > 0)
            {
                if (Current.Kind == TokenKind.OpenParen)
                {
                    depth++;
                }
                else if (Current.Kind == TokenKind.CloseParen)
                {
                    depth--;
                }

                Advance();
            }
        }

        private void SkipToArgumentBoundary()
        {
            int nestedParentheses = 0;
            while (!IsAtEnd)
            {
                if (Current.Kind == TokenKind.OpenParen)
                {
                    nestedParentheses++;
                }
                else if (Current.Kind == TokenKind.CloseParen)
                {
                    if (nestedParentheses == 0)
                    {
                        return;
                    }

                    nestedParentheses--;
                }
                else if (Current.Kind == TokenKind.Comma && nestedParentheses == 0)
                {
                    return;
                }

                Advance();
            }
        }

        private void SkipToEnd()
        {
            while (!IsAtEnd)
            {
                Advance();
            }
        }

        private void AddExpectedExpression()
        {
            AddDiagnostic(
                "expression.syntax.expectedExpression",
                "Expected an expression.",
                Current.Span);
        }

        private void AddDiagnostic(string code, string message, TextSpan span)
        {
            if (_diagnostics.Count < _options.MaxDiagnosticCount)
            {
                _diagnostics.Add(new ExpressionDiagnostic(code, message, span));
            }
        }

        private TextSpan PreviousSpanOr(TextSpan fallback)
        {
            if (_index == 0)
            {
                return fallback;
            }

            return _tokens[_index - 1].Span;
        }

        private static TextSpan Combine(TextSpan first, TextSpan second)
        {
            return new TextSpan(first.Start, second.End);
        }

        private static bool IsExpressionTerminator(TokenKind kind)
        {
            return kind == TokenKind.EndOfFile
                || kind == TokenKind.CloseParen
                || kind == TokenKind.Comma
                || kind == TokenKind.Colon;
        }

        private static bool IsLogicalOr(TokenKind kind)
        {
            return kind == TokenKind.PipePipe;
        }

        private static bool IsLogicalAnd(TokenKind kind)
        {
            return kind == TokenKind.AmpersandAmpersand;
        }

        private static bool IsEquality(TokenKind kind)
        {
            return kind == TokenKind.EqualsEquals || kind == TokenKind.BangEquals;
        }

        private static bool IsRelational(TokenKind kind)
        {
            return kind == TokenKind.GreaterThan
                || kind == TokenKind.GreaterThanOrEqual
                || kind == TokenKind.LessThan
                || kind == TokenKind.LessThanOrEqual;
        }

        private static bool IsAdditive(TokenKind kind)
        {
            return kind == TokenKind.Plus || kind == TokenKind.Minus;
        }

        private static bool IsMultiplicative(TokenKind kind)
        {
            return kind == TokenKind.Star
                || kind == TokenKind.Slash
                || kind == TokenKind.Percent;
        }

        private static bool TryGetUnaryOperator(TokenKind kind, out UnaryOperatorKind operatorKind)
        {
            switch (kind)
            {
                case TokenKind.Plus:
                    operatorKind = UnaryOperatorKind.Plus;
                    return true;
                case TokenKind.Minus:
                    operatorKind = UnaryOperatorKind.Minus;
                    return true;
                case TokenKind.Bang:
                    operatorKind = UnaryOperatorKind.LogicalNot;
                    return true;
                default:
                    operatorKind = UnaryOperatorKind.Plus;
                    return false;
            }
        }

        private static BinaryOperatorKind TokenToBinaryOperator(TokenKind kind)
        {
            switch (kind)
            {
                case TokenKind.Plus:
                    return BinaryOperatorKind.Add;
                case TokenKind.Minus:
                    return BinaryOperatorKind.Subtract;
                case TokenKind.Star:
                    return BinaryOperatorKind.Multiply;
                case TokenKind.Slash:
                    return BinaryOperatorKind.Divide;
                case TokenKind.Percent:
                    return BinaryOperatorKind.Modulo;
                case TokenKind.EqualsEquals:
                    return BinaryOperatorKind.Equal;
                case TokenKind.BangEquals:
                    return BinaryOperatorKind.NotEqual;
                case TokenKind.GreaterThan:
                    return BinaryOperatorKind.GreaterThan;
                case TokenKind.GreaterThanOrEqual:
                    return BinaryOperatorKind.GreaterThanOrEqual;
                case TokenKind.LessThan:
                    return BinaryOperatorKind.LessThan;
                case TokenKind.LessThanOrEqual:
                    return BinaryOperatorKind.LessThanOrEqual;
                case TokenKind.AmpersandAmpersand:
                    return BinaryOperatorKind.LogicalAnd;
                case TokenKind.PipePipe:
                    return BinaryOperatorKind.LogicalOr;
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }

        private static bool IsUnsupportedWord(string text)
        {
            switch (text)
            {
                case "null":
                case "new":
                case "if":
                case "else":
                case "for":
                case "foreach":
                case "while":
                case "do":
                case "switch":
                case "case":
                case "default":
                case "return":
                case "typeof":
                case "nameof":
                case "dynamic":
                case "var":
                    return true;
                default:
                    return false;
            }
        }

        private Token Previous
        {
            get { return _tokens[_index - 1]; }
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
