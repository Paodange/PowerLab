using System.Collections.Generic;

namespace PowerLab.Domain
{
    /// <summary>
    /// Portable, persisted workflow document data.
    /// </summary>
    public sealed class WorkflowDocument
    {
        /// <summary>
        /// Gets or sets the document schema version.
        /// </summary>
        public string SchemaVersion { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the stable workflow ID.
        /// </summary>
        public string WorkflowId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the workflow name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the optional workflow description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets workflow input definitions.
        /// </summary>
        public IReadOnlyList<WorkflowInputDefinition> Inputs { get; set; }
            = new List<WorkflowInputDefinition>();

        /// <summary>
        /// Gets or sets workflow variable definitions.
        /// </summary>
        public IReadOnlyList<WorkflowVariableDefinition> Variables { get; set; }
            = new List<WorkflowVariableDefinition>();

        /// <summary>
        /// Gets or sets the root scope.
        /// </summary>
        public WorkflowScope RootScope { get; set; } = new WorkflowScope();
    }
}
