namespace PowerLab.Domain
{
    /// <summary>
    /// Stores a declarative schema condition without evaluating it.
    /// </summary>
    public sealed class SchemaCondition
    {
        public string Language { get; set; } = string.Empty;
        public int LanguageVersion { get; set; }
        public string Source { get; set; } = string.Empty;
    }
}
