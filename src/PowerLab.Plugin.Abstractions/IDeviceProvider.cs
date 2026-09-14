using System.Collections.Generic;

namespace PowerLab.Plugin.Abstractions
{
    /// <summary>
    /// Provides device type descriptors and creates drivers for configured instances.
    /// </summary>
    public interface IDeviceProvider
    {
        /// <summary>
        /// Gets the device types exposed by this provider. An empty collection is valid.
        /// </summary>
        IReadOnlyList<DeviceTypeDescriptor> GetDeviceTypes();

        /// <summary>
        /// Creates a driver for one configured device instance.
        /// </summary>
        IDeviceDriver CreateDriver(DeviceCreateContext context);
    }
}
