using System;
using System.Linq;
using PowerLab.Expressions;
using Xunit;

namespace PowerLab.Expressions.Tests
{
    public sealed class LexerTests
    {
        [Fact]
        public void LexesAllSupportedTokenKinds()
        {
            LexerResult result = ExpressionLexer.Lex(
                "name 100 10.5 1.5e3 true false \"text\" + - * / % == != > >= < <= && || ! ? : ( ) , .");

            Assert.True(result.Success);
            Assert.Equal(
                new[]
                {
                    TokenKind.Identifier,
                    TokenKind.IntegerLiteral,
                    TokenKind.NumberLiteral,
                    TokenKind.NumberLiteral,
                    TokenKind.TrueKeyword,
                    TokenKind.FalseKeyword,
                    TokenKind.StringLiteral,
                    TokenKind.Plus,
                    TokenKind.Minus,
                    TokenKind.Star,
                    TokenKind.Slash,
                    TokenKind.Percent,
                    TokenKind.EqualsEquals,
                    TokenKind.BangEquals,
                    TokenKind.GreaterThan,
                    TokenKind.GreaterThanOrEqual,
                    TokenKind.LessThan,
                    TokenKind.LessThanOrEqual,
                    TokenKind.AmpersandAmpersand,
                    TokenKind.PipePipe,
                    TokenKind.Bang,
                    TokenKind.Question,
                    TokenKind.Colon,
                    TokenKind.OpenParen,
                    TokenKind.CloseParen,
                    TokenKind.Comma,
                    TokenKind.Dot,
                    TokenKind.EndOfFile
                },
                result.Tokens.Select(token => token.Kind));
        }

        [Fact]
        public void PreservesLiteralValuesAndRawText()
        {
            LexerResult result = ExpressionLexer.Lex("100 10.5 1.5e3 true false \"ABC\"");

            Assert.Equal(100, result.Tokens[0].IntegerValue);
            Assert.Equal(10.5, result.Tokens[1].NumberValue);
            Assert.Equal(1500.0, result.Tokens[2].NumberValue);
            Assert.Equal(true, result.Tokens[3].BooleanValue);
            Assert.Equal(false, result.Tokens[4].BooleanValue);
            Assert.Equal("ABC", result.Tokens[5].StringValue);
            Assert.Equal("\"ABC\"", result.Tokens[5].RawText);
        }

        [Fact]
        public void DecodesSupportedStringEscapes()
        {
            LexerResult result = ExpressionLexer.Lex("\"\\\\ \\\" \\n \\r \\t \\u0041\"");

            Assert.True(result.Success);
            Assert.Equal("\\ \" \n \r \t A", result.Tokens[0].StringValue);
        }

        [Fact]
        public void TracksZeroBasedOffsetsLinesAndColumnsAcrossCrLf()
        {
            LexerResult result = ExpressionLexer.Lex("a\r\n  Variables");

            Assert.Equal(new TextPosition(0, 0, 0), result.Tokens[0].Span.Start);
            Assert.Equal(new TextPosition(1, 0, 1), result.Tokens[0].Span.End);
            Assert.Equal(new TextPosition(5, 1, 2), result.Tokens[1].Span.Start);
            Assert.Equal(new TextPosition(14, 1, 11), result.Tokens[1].Span.End);
            Assert.Equal(new TextPosition(14, 1, 11), result.Tokens[2].Span.Start);
        }

        [Fact]
        public void EmitsEofTokenAtEndOfSource()
        {
            LexerResult result = ExpressionLexer.Lex("  a");
            Token eof = result.Tokens[result.Tokens.Count - 1];

            Assert.Equal(TokenKind.EndOfFile, eof.Kind);
            Assert.Equal(string.Empty, eof.Text);
            Assert.Equal(3, eof.Span.StartOffset);
            Assert.Equal(0, eof.Span.Length);
        }

        [Theory]
        [InlineData("@", "expression.syntax.unexpectedCharacter")]
        [InlineData("\"unterminated", "expression.syntax.unterminatedString")]
        [InlineData("\"bad\\q\"", "expression.syntax.invalidEscape")]
        [InlineData("\"bad\\u12G4\"", "expression.syntax.invalidUnicodeEscape")]
        [InlineData("1.2.3", "expression.syntax.invalidNumber")]
        [InlineData("1e", "expression.syntax.invalidNumber")]
        [InlineData("123abc", "expression.syntax.invalidNumber")]
        [InlineData("=", "expression.syntax.unsupportedSyntax")]
        [InlineData("&", "expression.syntax.unsupportedSyntax")]
        [InlineData("|", "expression.syntax.unsupportedSyntax")]
        public void ReportsInvalidLexemesWithoutDroppingThem(string source, string code)
        {
            LexerResult result = ExpressionLexer.Lex(source);

            Assert.False(result.Success);
            Assert.Contains(code, result.Diagnostics.Select(diagnostic => diagnostic.Code));
            Assert.Contains(result.Tokens, token => token.Kind == TokenKind.BadToken);
        }

        [Fact]
        public void RejectsCommentsAsUnsupportedSyntax()
        {
            LexerResult result = ExpressionLexer.Lex("a // comment");

            Assert.Contains("expression.syntax.unsupportedSyntax", result.Diagnostics.Select(diagnostic => diagnostic.Code));
            Assert.Contains(result.Tokens, token => token.Kind == TokenKind.BadToken);
        }
    }
}
