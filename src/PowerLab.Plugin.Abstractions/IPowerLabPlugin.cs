namespace PowerLab.Plugin.Abstractions
{
    /// <summary>
    /// Defines the synchronous entry point used to configure a PowerLab plugin.
    /// </summary>
    public interface IPowerLabPlugin
    {
        /// <summary>
        /// Registers the plugin capabilities with the supplied controlled builder.
        /// </summary>
        void Configure(IPluginBuilder builder);
    }
}
