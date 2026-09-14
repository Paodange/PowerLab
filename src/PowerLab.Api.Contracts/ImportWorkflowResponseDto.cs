namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Result metadata returned after importing a workflow document.
    /// </summary>
    public sealed class ImportWorkflowResponseDto
    {
        public string WorkflowId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public System.DateTimeOffset UpdatedAt { get; set; }
    }
}
