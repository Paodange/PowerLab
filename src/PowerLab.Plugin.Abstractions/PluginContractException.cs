using System;

namespace PowerLab.Plugin.Abstractions
{
    /// <summary>
    /// Base exception for plugin SDK programming and contract violations.
    /// </summary>
    public class PluginContractException : Exception
    {
        /// <summary>
        /// Creates a plugin contract exception.
        /// </summary>
        public PluginContractException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates a plugin contract exception with an inner exception.
        /// </summary>
        public PluginContractException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
