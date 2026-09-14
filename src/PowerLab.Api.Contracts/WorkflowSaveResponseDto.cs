using PowerLab.Domain;

namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Metadata returned after a workflow document is saved.
    /// </summary>
    public sealed class WorkflowSaveResponseDto
    {
        public string WorkflowId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public System.DateTimeOffset UpdatedAt { get; set; }
        public WorkflowDocument Document { get; set; } = new WorkflowDocument();
    }
}
