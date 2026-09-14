using PowerLab.Domain;

namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Workflow resource response used after draft creation or saving.
    /// </summary>
    public sealed class WorkflowDto : WorkflowSummaryDto
    {
        public WorkflowDocument Document { get; set; } = new WorkflowDocument();
    }
}
