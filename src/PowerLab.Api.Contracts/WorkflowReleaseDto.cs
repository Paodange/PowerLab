using System;
using System.Collections.Generic;
using PowerLab.Domain;

namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Immutable published workflow release response.
    /// </summary>
    public sealed class WorkflowReleaseDto
    {
        public string ReleaseId { get; set; } = string.Empty;
        public string WorkflowId { get; set; } = string.Empty;
        public int ReleaseNumber { get; set; }
        public DateTimeOffset PublishedAt { get; set; }
        public string ExecutionHash { get; set; } = string.Empty;
        public IReadOnlyList<RequiredPluginDto> RequiredPlugins { get; set; }
            = new List<RequiredPluginDto>();
        public IReadOnlyList<PythonScriptHashDto> PythonScriptHashes { get; set; }
            = new List<PythonScriptHashDto>();
        public WorkflowDocument Document { get; set; } = new WorkflowDocument();
    }
}
