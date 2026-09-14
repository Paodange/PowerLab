namespace PowerLab.Plugin.Abstractions
{
    /// <summary>
    /// Indicates that an executor creation request lacks a valid stable node identity.
    /// </summary>
    public sealed class InvalidExecutorCreationRequestException : PluginContractException
    {
        /// <summary>
        /// Creates an invalid executor creation request exception.
        /// </summary>
        public InvalidExecutorCreationRequestException(string message)
            : base(message)
        {
        }
    }
}
