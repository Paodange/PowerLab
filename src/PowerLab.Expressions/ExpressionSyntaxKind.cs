namespace PowerLab.Expressions
{
    /// <summary>
    /// Kinds of nodes in the expression syntax tree.
    /// </summary>
    public enum ExpressionSyntaxKind
    {
        Literal,
        Reference,
        Unary,
        Binary,
        Conditional,
        FunctionCall,
        Parenthesized
    }
}
