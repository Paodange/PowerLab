namespace PowerLab.Expressions
{
    /// <summary>
    /// Token kinds in the PowerLab V1 expression language.
    /// </summary>
    public enum TokenKind
    {
        Identifier,
        IntegerLiteral,
        NumberLiteral,
        StringLiteral,
        TrueKeyword,
        FalseKeyword,
        Plus,
        Minus,
        Star,
        Slash,
        Percent,
        EqualsEquals,
        BangEquals,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual,
        AmpersandAmpersand,
        PipePipe,
        Bang,
        Question,
        Colon,
        OpenParen,
        CloseParen,
        Comma,
        Dot,
        BadToken,
        EndOfFile,
        True = TrueKeyword,
        False = FalseKeyword,
        End = EndOfFile
    }
}
