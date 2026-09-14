using PowerLab.Domain;

namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Detailed workflow draft response containing the portable document.
    /// </summary>
    public sealed class WorkflowDetailDto : WorkflowSummaryDto
    {
        public WorkflowDocument Document { get; set; } = new WorkflowDocument();
    }
}
