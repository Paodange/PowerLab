namespace PowerLab.Domain
{
    /// <summary>
    /// A portable reference to a node icon resource.
    /// </summary>
    public sealed class IconReference
    {
        public string Kind { get; set; } = string.Empty;
        public string ResourcePath { get; set; } = string.Empty;
        public string? ContentHash { get; set; }
    }
}
