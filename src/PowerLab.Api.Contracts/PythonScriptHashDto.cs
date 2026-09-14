namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Published hash for one persisted Python script node.
    /// </summary>
    public sealed class PythonScriptHashDto
    {
        public string NodeId { get; set; } = string.Empty;
        public string Hash { get; set; } = string.Empty;
    }
}
