using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PowerLab.Plugin.Abstractions
{
    /// <summary>
    /// Describes a device type exposed by a plugin.
    /// </summary>
    public sealed class DeviceTypeDescriptor
    {
        private DeviceConfigurationSchema _configurationSchema = new DeviceConfigurationSchema();
        private IReadOnlyList<string> _supportedRuntimes = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the stable device type identifier.
        /// </summary>
        public string DeviceTypeId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the scalar connection configuration schema.
        /// </summary>
        public DeviceConfigurationSchema ConfigurationSchema
        {
            get { return _configurationSchema; }
            set { _configurationSchema = value ?? throw new ArgumentNullException(nameof(value)); }
        }

        /// <summary>
        /// Gets or sets the Runtime Identifiers supported by this device type.
        /// </summary>
        public IReadOnlyList<string> SupportedRuntimes
        {
            get { return _supportedRuntimes; }
            set { _supportedRuntimes = CopyRuntimes(value); }
        }

        private static IReadOnlyList<string> CopyRuntimes(IReadOnlyList<string> runtimes)
        {
            if (runtimes == null)
            {
                throw new ArgumentNullException(nameof(runtimes));
            }

            List<string> copy = new List<string>(runtimes.Count);
            foreach (string runtime in runtimes)
            {
                if (runtime == null)
                {
                    throw new ArgumentException("The runtime collection cannot contain null values.", nameof(runtimes));
                }

                copy.Add(runtime);
            }

            return new ReadOnlyCollection<string>(copy);
        }
    }
}
