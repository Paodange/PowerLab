namespace PowerLab.Plugin.Abstractions
{
    /// <summary>
    /// Indicates invalid plugin registration or configuration data.
    /// </summary>
    public sealed class PluginConfigurationException : PluginContractException
    {
        /// <summary>
        /// Creates a plugin configuration exception.
        /// </summary>
        public PluginConfigurationException(string message)
            : base(message)
        {
        }
    }
}
