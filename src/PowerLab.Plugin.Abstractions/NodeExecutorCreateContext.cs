namespace PowerLab.Plugin.Abstractions
{
    /// <summary>
    /// Identifies the stable node contract for which an executor is being created.
    /// </summary>
    public sealed class NodeExecutorCreateContext
    {
        /// <summary>
        /// Creates a node executor creation context.
        /// </summary>
        public NodeExecutorCreateContext(string nodeTypeId, int nodeVersion)
        {
            if (nodeTypeId == null)
            {
                throw new InvalidExecutorCreationRequestException("NodeTypeId cannot be null.");
            }

            if (nodeTypeId.Trim().Length == 0)
            {
                throw new InvalidExecutorCreationRequestException("NodeTypeId cannot be empty.");
            }

            if (nodeVersion <= 0)
            {
                throw new InvalidExecutorCreationRequestException("NodeVersion must be greater than zero.");
            }

            NodeTypeId = nodeTypeId;
            NodeVersion = nodeVersion;
        }

        /// <summary>
        /// Gets the stable node type identifier.
        /// </summary>
        public string NodeTypeId { get; }

        /// <summary>
        /// Gets the compatible node contract version.
        /// </summary>
        public int NodeVersion { get; }
    }
}
