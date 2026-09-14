using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using PowerLab.Domain;

namespace PowerLab.Plugin.Abstractions
{
    /// <summary>
    /// Supplies immutable device instance identity and connection settings to a provider.
    /// </summary>
    public sealed class DeviceCreateContext
    {
        private readonly IReadOnlyDictionary<string, WorkflowValue> _connectionSettings;

        /// <summary>
        /// Creates a device driver creation context.
        /// </summary>
        public DeviceCreateContext(
            string instanceId,
            string displayName,
            string deviceTypeId,
            IReadOnlyDictionary<string, WorkflowValue> connectionSettings)
        {
            InstanceId = RequireIdentifier(instanceId, nameof(instanceId));
            DisplayName = RequireIdentifier(displayName, nameof(displayName));
            DeviceTypeId = RequireIdentifier(deviceTypeId, nameof(deviceTypeId));
            _connectionSettings = CopySettings(connectionSettings);
        }

        /// <summary>
        /// Gets the configured device instance identifier.
        /// </summary>
        public string InstanceId { get; }

        /// <summary>
        /// Gets the configured display name.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Gets the stable device type identifier.
        /// </summary>
        public string DeviceTypeId { get; }

        /// <summary>
        /// Gets a defensive, read-only snapshot of scalar connection settings.
        /// </summary>
        public IReadOnlyDictionary<string, WorkflowValue> ConnectionSettings
        {
            get { return _connectionSettings; }
        }

        private static string RequireIdentifier(string value, string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            if (value.Trim().Length == 0)
            {
                throw new ArgumentException("The identifier cannot be empty.", parameterName);
            }

            return value;
        }

        private static IReadOnlyDictionary<string, WorkflowValue> CopySettings(
            IReadOnlyDictionary<string, WorkflowValue> settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            Dictionary<string, WorkflowValue> copy = new Dictionary<string, WorkflowValue>(StringComparer.Ordinal);
            foreach (KeyValuePair<string, WorkflowValue> pair in settings)
            {
                if (pair.Key == null)
                {
                    throw new ArgumentException("Connection settings cannot contain a null key.", nameof(settings));
                }

                if (pair.Value == null)
                {
                    throw new ArgumentException("Connection settings cannot contain a null value.", nameof(settings));
                }

                copy.Add(pair.Key, pair.Value);
            }

            return new ReadOnlyDictionary<string, WorkflowValue>(copy);
        }
    }
}
