using System.Collections.Generic;

namespace PowerLab.Domain
{
    /// <summary>
    /// A node whose child branches can be scheduled in parallel.
    /// </summary>
    public sealed class ParallelNode : WorkflowNodeDefinition
    {
        public ParallelNode()
            : base("parallel")
        {
        }

        public IReadOnlyList<ParallelBranch> Branches { get; set; }
            = new List<ParallelBranch>();
    }

    /// <summary>
    /// A named branch and child scope of a parallel node.
    /// </summary>
    public sealed class ParallelBranch
    {
        public string Id { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public WorkflowScope Scope { get; set; } = new WorkflowScope();
    }
}
