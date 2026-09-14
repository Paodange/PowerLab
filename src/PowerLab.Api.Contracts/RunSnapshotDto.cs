using System.Collections.Generic;

namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Authoritative read model used to recover a run after reconnect.
    /// </summary>
    public sealed class RunSnapshotDto
    {
        public RunDto Run { get; set; } = new RunDto();
        public IReadOnlyDictionary<string, WorkflowValueDto> Inputs { get; set; }
            = new Dictionary<string, WorkflowValueDto>();
        public IReadOnlyDictionary<string, WorkflowValueDto> Variables { get; set; }
            = new Dictionary<string, WorkflowValueDto>();
        public IReadOnlyList<NodeRuntimeStateDto> Nodes { get; set; }
            = new List<NodeRuntimeStateDto>();
        public IReadOnlyList<NodeAttemptDto> ActiveAttempts { get; set; }
            = new List<NodeAttemptDto>();
        public IReadOnlyList<RunFaultDto> Faults { get; set; }
            = new List<RunFaultDto>();
        public long LastEventSequence { get; set; }
    }
}
