using System;
using System.Threading;
using System.Threading.Tasks;

namespace PowerLab.Plugin.Abstractions
{
    /// <summary>
    /// Defines the device lifecycle boundary managed by the future DeviceManager.
    /// </summary>
    public interface IDeviceDriver : IAsyncDisposable
    {
        /// <summary>
        /// Connects the driver to its configured device.
        /// </summary>
        ValueTask ConnectAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Disconnects the driver from its configured device.
        /// </summary>
        ValueTask DisconnectAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Reads readiness without connecting or initializing the device.
        /// </summary>
        ValueTask<DeviceReadiness> GetReadinessAsync(CancellationToken cancellationToken);
    }
}
