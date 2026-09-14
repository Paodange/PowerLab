using System.Collections.Generic;

namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Request to enqueue a run of an immutable workflow release.
    /// </summary>
    public sealed class CreateRunRequest
    {
        public string ReleaseId { get; set; } = string.Empty;
        public IReadOnlyDictionary<string, WorkflowValueDto> Inputs { get; set; }
            = new Dictionary<string, WorkflowValueDto>();
        public RunMode Mode { get; set; }
    }
}
