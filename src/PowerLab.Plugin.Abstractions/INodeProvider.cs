using System.Collections.Generic;
using PowerLab.Domain;

namespace PowerLab.Plugin.Abstractions
{
    /// <summary>
    /// Provides serializable node descriptors and creates executors for node attempts.
    /// </summary>
    public interface INodeProvider
    {
        /// <summary>
        /// Gets the node descriptors exposed by this provider. An empty collection is valid.
        /// </summary>
        IReadOnlyList<NodeDescriptor> GetNodeDescriptors();

        /// <summary>
        /// Creates a new executor for the requested node type and contract version.
        /// </summary>
        INodeExecutor CreateExecutor(NodeExecutorCreateContext context);
    }
}
