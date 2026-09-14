using System;
using System.Linq;
using PowerLab.Expressions;
using Xunit;

namespace PowerLab.Expressions.Tests
{
    public sealed class ParserTests
    {
        [Theory]
        [InlineData("100", LiteralValueKind.Integer)]
        [InlineData("10.5", LiteralValueKind.Number)]
        [InlineData("true", LiteralValueKind.Boolean)]
        [InlineData("\"ABC\"", LiteralValueKind.String)]
        public void ParsesScalarLiterals(string source, LiteralValueKind kind)
        {
            ExpressionParseResult result = ExpressionParser.Parse(source);

            LiteralExpressionSyntax literal = Assert.IsType<LiteralExpressionSyntax>(result.Root);
            Assert.True(result.Success);
            Assert.Equal(kind, literal.LiteralKind);
        }

        [Theory]
        [InlineData("Variables.Speed", "Variables", "Speed")]
        [InlineData("Inputs.TargetTemperature", "Inputs", "TargetTemperature")]
        [InlineData("Loop.Iteration", "Loop", "Iteration")]
        [InlineData("Context.RunId", "Context", "RunId")]
        public void StoresReferencesAsPathSegments(string source, string first, string second)
        {
            ExpressionParseResult result = ExpressionParser.Parse(source);

            ReferenceExpressionSyntax reference = Assert.IsType<ReferenceExpressionSyntax>(result.Root);
            Assert.True(result.Success);
            Assert.Equal(new[] { first, second }, reference.Path);
        }

        [Fact]
        public void AppliesMultiplicativePrecedenceBeforeAdditive()
        {
            BinaryExpressionSyntax root = Assert.IsType<BinaryExpressionSyntax>(ExpressionParser.Parse("a + b * c").Root);

            Assert.Equal(BinaryOperatorKind.Add, root.OperatorKind);
            Assert.IsType<ReferenceExpressionSyntax>(root.Left);
            BinaryExpressionSyntax right = Assert.IsType<BinaryExpressionSyntax>(root.Right);
            Assert.Equal(BinaryOperatorKind.Multiply, right.OperatorKind);
        }

        [Fact]
        public void ParenthesesOverrideDefaultPrecedence()
        {
            BinaryExpressionSyntax root = Assert.IsType<BinaryExpressionSyntax>(ExpressionParser.Parse("(a + b) * c").Root);

            Assert.Equal(BinaryOperatorKind.Multiply, root.OperatorKind);
            ParenthesizedExpressionSyntax left = Assert.IsType<ParenthesizedExpressionSyntax>(root.Left);
            BinaryExpressionSyntax enclosed = Assert.IsType<BinaryExpressionSyntax>(left.Expression);
            Assert.Equal(BinaryOperatorKind.Add, enclosed.OperatorKind);
        }

        [Fact]
        public void ParsesUnaryOperatorsAsRightAssociative()
        {
            UnaryExpressionSyntax root = Assert.IsType<UnaryExpressionSyntax>(ExpressionParser.Parse("!!flag").Root);

            Assert.Equal(UnaryOperatorKind.LogicalNot, root.OperatorKind);
            Assert.IsType<UnaryExpressionSyntax>(root.Operand);
            Assert.Equal(UnaryOperatorKind.LogicalNot, ((UnaryExpressionSyntax)root.Operand).OperatorKind);
        }

        [Fact]
        public void ParsesNegativeNumbersAsUnaryExpressions()
        {
            UnaryExpressionSyntax root = Assert.IsType<UnaryExpressionSyntax>(ExpressionParser.Parse("-10").Root);

            Assert.Equal(UnaryOperatorKind.Minus, root.OperatorKind);
            LiteralExpressionSyntax operand = Assert.IsType<LiteralExpressionSyntax>(root.Operand);
            Assert.Equal(10, operand.Value);
        }

        [Fact]
        public void AppliesLogicalAndBeforeLogicalOr()
        {
            BinaryExpressionSyntax root = Assert.IsType<BinaryExpressionSyntax>(ExpressionParser.Parse("a || b && c").Root);

            Assert.Equal(BinaryOperatorKind.LogicalOr, root.OperatorKind);
            BinaryExpressionSyntax right = Assert.IsType<BinaryExpressionSyntax>(root.Right);
            Assert.Equal(BinaryOperatorKind.LogicalAnd, right.OperatorKind);
        }

        [Fact]
        public void ParsesRelationalAndEqualityOperators()
        {
            BinaryExpressionSyntax root = Assert.IsType<BinaryExpressionSyntax>(
                ExpressionParser.Parse("a == b != c > d && e <= f").Root);

            Assert.Equal(BinaryOperatorKind.LogicalAnd, root.OperatorKind);
            BinaryExpressionSyntax left = Assert.IsType<BinaryExpressionSyntax>(root.Left);
            Assert.Equal(BinaryOperatorKind.NotEqual, left.OperatorKind);
            Assert.Equal(BinaryOperatorKind.Equal, ((BinaryExpressionSyntax)left.Left).OperatorKind);
            Assert.Equal(BinaryOperatorKind.GreaterThan, ((BinaryExpressionSyntax)left.Right).OperatorKind);
            Assert.Equal(BinaryOperatorKind.LessThanOrEqual, ((BinaryExpressionSyntax)root.Right).OperatorKind);
        }

        [Fact]
        public void ParsesConditionalExpressionsRightAssociatively()
        {
            ConditionalExpressionSyntax root = Assert.IsType<ConditionalExpressionSyntax>(
                ExpressionParser.Parse("a ? b : c ? d : e").Root);

            Assert.IsType<ReferenceExpressionSyntax>(root.Condition);
            Assert.IsType<ReferenceExpressionSyntax>(root.WhenTrue);
            ConditionalExpressionSyntax nested = Assert.IsType<ConditionalExpressionSyntax>(root.WhenFalse);
            Assert.Equal("c", Assert.IsType<ReferenceExpressionSyntax>(nested.Condition).Path[0]);
            Assert.Equal("e", Assert.IsType<ReferenceExpressionSyntax>(nested.WhenFalse).Path[0]);
        }

        [Fact]
        public void ParsesBareFunctionCallsAndNestedExpressions()
        {
            ExpressionParseResult result = ExpressionParser.Parse("contains(name, \"Control\")");

            FunctionCallExpressionSyntax call = Assert.IsType<FunctionCallExpressionSyntax>(result.Root);
            Assert.True(result.Success);
            Assert.Equal("contains", call.Name);
            Assert.Equal(2, call.Arguments.Count);
            Assert.Equal(LiteralValueKind.String, Assert.IsType<LiteralExpressionSyntax>(call.Arguments[1]).LiteralKind);
        }

        [Fact]
        public void AllowsUnknownFunctionNamesForLaterSemanticValidation()
        {
            ExpressionParseResult result = ExpressionParser.Parse("unknownFunction(a)");

            Assert.True(result.Success);
            Assert.Equal("unknownFunction", Assert.IsType<FunctionCallExpressionSyntax>(result.Root).Name);
        }

        [Fact]
        public void IncludesCompleteNodeSpans()
        {
            ExpressionParseResult result = ExpressionParser.Parse("a + b");
            BinaryExpressionSyntax binary = Assert.IsType<BinaryExpressionSyntax>(result.Root);

            Assert.Equal(0, binary.Span.StartOffset);
            Assert.Equal(5, binary.Span.Length);
            Assert.Equal(2, binary.OperatorSpan.StartOffset);
            Assert.Equal(1, binary.OperatorSpan.Length);
        }

        [Fact]
        public void RejectsEmptyOrWhitespaceOnlySource()
        {
            foreach (string source in new[] { string.Empty, " \t\r\n " })
            {
                ExpressionParseResult result = ExpressionParser.Parse(source);

                Assert.False(result.Success);
                Assert.Null(result.Root);
                Assert.Contains("expression.syntax.expectedExpression", result.Diagnostics.Select(diagnostic => diagnostic.Code));
            }
        }

        [Theory]
        [InlineData("a = 1", "expression.syntax.unsupportedSyntax")]
        [InlineData("a =", "expression.syntax.unsupportedSyntax")]
        [InlineData("a +", "expression.syntax.expectedExpression")]
        [InlineData("(a + b", "expression.syntax.expectedToken")]
        [InlineData("a + b)", "expression.syntax.trailingInput")]
        [InlineData("a ? b", "expression.syntax.expectedToken")]
        [InlineData("a ? : c", "expression.syntax.expectedExpression")]
        [InlineData("min(", "expression.syntax.expectedToken")]
        [InlineData("min(a,)", "expression.syntax.expectedExpression")]
        [InlineData("min(,a)", "expression.syntax.expectedExpression")]
        [InlineData("Device.Move()", "expression.syntax.unsupportedSyntax")]
        [InlineData("Samples[0]", "expression.syntax.unexpectedCharacter")]
        [InlineData("a ?? b", "expression.syntax.unsupportedSyntax")]
        [InlineData("a?.b", "expression.syntax.unsupportedSyntax")]
        [InlineData("new SomeObject()", "expression.syntax.unsupportedSyntax")]
        [InlineData("a; b", "expression.syntax.unexpectedCharacter")]
        [InlineData("\"unterminated", "expression.syntax.unterminatedString")]
        [InlineData("1.2.3", "expression.syntax.invalidNumber")]
        [InlineData("1e", "expression.syntax.invalidNumber")]
        [InlineData("a &&& b", "expression.syntax.unsupportedSyntax")]
        public void InvalidInputsReturnStructuredDiagnostics(string source, string expectedCode)
        {
            ExpressionParseResult result = ExpressionParser.Parse(source);

            Assert.False(result.Success);
            Assert.Null(result.Root);
            Assert.NotEmpty(result.Diagnostics);
            Assert.Contains(expectedCode, result.Diagnostics.Select(diagnostic => diagnostic.Code));
        }

        [Fact]
        public void DoesNotTreatAnExpressionResultAsAFunction()
        {
            ExpressionParseResult result = ExpressionParser.Parse("(a + b)(c)");

            Assert.False(result.Success);
            Assert.Contains("expression.syntax.trailingInput", result.Diagnostics.Select(diagnostic => diagnostic.Code));
        }

        [Fact]
        public void ReportsFunctionArgumentDelimiterErrors()
        {
            ExpressionParseResult result = ExpressionParser.Parse("min(a b)");

            Assert.False(result.Success);
            Assert.Contains("expression.syntax.expectedToken", result.Diagnostics.Select(diagnostic => diagnostic.Code));
            Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Message.Contains("Expected ','"));
        }

        [Fact]
        public void NullSourceIsAProgrammingError()
        {
            Assert.Throws<ArgumentNullException>(() => ExpressionParser.Parse(null!));
            Assert.Throws<ArgumentNullException>(() => ExpressionLexer.Lex(null!));
        }
    }
}
