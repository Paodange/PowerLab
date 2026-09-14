namespace PowerLab.Domain
{
    /// <summary>
    /// A UI-neutral hint for choosing an editor for a parameter.
    /// </summary>
    public sealed class ParameterEditorHint
    {
        public string Kind { get; set; } = string.Empty;
        public double? Step { get; set; }
    }
}
