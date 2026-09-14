namespace PowerLab.Plugin.Abstractions
{
    /// <summary>
    /// Describes the controlled registrations a plugin may contribute.
    /// </summary>
    public interface IPluginBuilder
    {
        /// <summary>
        /// Registers a node provider type.
        /// </summary>
        void AddNodeProvider<TProvider>()
            where TProvider : class, INodeProvider;

        /// <summary>
        /// Registers a device provider type.
        /// </summary>
        void AddDeviceProvider<TProvider>()
            where TProvider : class, IDeviceProvider;

        /// <summary>
        /// Registers a plugin-level service type and its implementation type.
        /// </summary>
        void AddService<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService;
    }
}
