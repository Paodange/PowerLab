using PowerLab.Domain;

namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Request to import a portable workflow document.
    /// </summary>
    public sealed class ImportWorkflowRequest
    {
        public WorkflowDocument Document { get; set; } = new WorkflowDocument();
    }
}
