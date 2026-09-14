using System;
using System.Collections.Generic;

namespace PowerLab.Domain
{
    /// <summary>
    /// A pure data snapshot of a published workflow release.
    /// </summary>
    public sealed class WorkflowRelease
    {
        public string ReleaseId { get; set; } = string.Empty;
        public string WorkflowId { get; set; } = string.Empty;
        public int ReleaseNumber { get; set; }
        public DateTimeOffset PublishedAt { get; set; }
        public WorkflowDocument Document { get; set; } = new WorkflowDocument();
        public string ExecutionHash { get; set; } = string.Empty;
        public IReadOnlyList<RequiredPluginReference> RequiredPlugins { get; set; }
            = new List<RequiredPluginReference>();
        public IReadOnlyList<PythonScriptHash> PythonScriptHashes { get; set; }
            = new List<PythonScriptHash>();
    }

    /// <summary>
    /// Identifies a plugin dependency recorded by a release.
    /// </summary>
    public sealed class RequiredPluginReference
    {
        public string PluginId { get; set; } = string.Empty;
        public string PluginVersion { get; set; } = string.Empty;
    }

    /// <summary>
    /// Associates a Python script node with its published source hash.
    /// </summary>
    public sealed class PythonScriptHash
    {
        public string NodeId { get; set; } = string.Empty;
        public string Hash { get; set; } = string.Empty;
    }
}
