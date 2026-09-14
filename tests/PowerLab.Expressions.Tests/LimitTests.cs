using System.Linq;
using PowerLab.Expressions;
using Xunit;

namespace PowerLab.Expressions.Tests
{
    public sealed class LimitTests
    {
        [Fact]
        public void RejectsSourceLongerThanConfiguredLimit()
        {
            ExpressionParseOptions options = new ExpressionParseOptions { MaxSourceLength = 8 };

            ExpressionParseResult result = ExpressionParser.Parse("123456789", options);

            Assert.False(result.Success);
            Assert.Contains("expression.syntax.sourceTooLong", result.Diagnostics.Select(diagnostic => diagnostic.Code));
            Assert.Equal(0, result.Diagnostics[0].Span.StartOffset);
            Assert.Equal(9, result.Diagnostics[0].Span.Length);
        }

        [Fact]
        public void RejectsDeepParenthesesWithoutStackOverflow()
        {
            ExpressionParseOptions options = new ExpressionParseOptions { MaxNestingDepth = 8 };
            string source = new string('(', 20) + "1" + new string(')', 20);

            ExpressionParseResult result = ExpressionParser.Parse(source, options);

            Assert.False(result.Success);
            Assert.Contains("expression.syntax.nestingLimitExceeded", result.Diagnostics.Select(diagnostic => diagnostic.Code));
        }

        [Fact]
        public void RejectsLongUnaryChainsWithoutStackOverflow()
        {
            ExpressionParseOptions options = new ExpressionParseOptions { MaxUnaryOperatorCount = 8 };
            string source = new string('!', 30) + "flag";

            ExpressionParseResult result = ExpressionParser.Parse(source, options);

            Assert.False(result.Success);
            Assert.Contains("expression.syntax.nestingLimitExceeded", result.Diagnostics.Select(diagnostic => diagnostic.Code));
        }

        [Fact]
        public void RejectsExcessiveTokenCount()
        {
            ExpressionParseOptions options = new ExpressionParseOptions { MaxTokenCount = 4 };

            ExpressionParseResult result = ExpressionParser.Parse("a + b + c", options);

            Assert.False(result.Success);
            Assert.Contains("expression.syntax.sourceTooLong", result.Diagnostics.Select(diagnostic => diagnostic.Code));
        }
    }
}
