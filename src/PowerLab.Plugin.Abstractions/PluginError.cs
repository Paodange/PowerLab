using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using PowerLab.Domain;

namespace PowerLab.Plugin.Abstractions
{
    /// <summary>
    /// A stable, serializable summary of a plugin or node execution failure.
    /// </summary>
    public sealed class PluginError
    {
        private readonly IReadOnlyDictionary<string, WorkflowValue> _details;

        /// <summary>
        /// Creates a structured plugin error.
        /// </summary>
        public PluginError(
            string code,
            string message,
            string pluginId,
            string nodeTypeId,
            IReadOnlyDictionary<string, WorkflowValue>? details = null,
            string? deviceId = null)
        {
            Code = RequireText(code, nameof(code));
            Message = RequireText(message, nameof(message));
            PluginId = RequireText(pluginId, nameof(pluginId));
            NodeTypeId = RequireText(nodeTypeId, nameof(nodeTypeId));

            if (deviceId != null && deviceId.Trim().Length == 0)
            {
                throw new ArgumentException("DeviceId cannot be empty when supplied.", nameof(deviceId));
            }

            DeviceId = deviceId;
            _details = CopyDetails(details);
        }

        /// <summary>
        /// Gets the stable error code.
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Gets the user-displayable error summary.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets safe scalar diagnostic details.
        /// </summary>
        public IReadOnlyDictionary<string, WorkflowValue> Details
        {
            get { return _details; }
        }

        /// <summary>
        /// Gets the plugin identifier associated with the error.
        /// </summary>
        public string PluginId { get; }

        /// <summary>
        /// Gets the stable node type identifier associated with the error.
        /// </summary>
        public string NodeTypeId { get; }

        /// <summary>
        /// Gets the optional device instance identifier associated with the error.
        /// </summary>
        public string? DeviceId { get; }

        private static string RequireText(string value, string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            if (value.Trim().Length == 0)
            {
                throw new ArgumentException("The value cannot be empty.", parameterName);
            }

            return value;
        }

        private static IReadOnlyDictionary<string, WorkflowValue> CopyDetails(
            IReadOnlyDictionary<string, WorkflowValue>? details)
        {
            Dictionary<string, WorkflowValue> copy = new Dictionary<string, WorkflowValue>(StringComparer.Ordinal);
            if (details != null)
            {
                foreach (KeyValuePair<string, WorkflowValue> pair in details)
                {
                    if (pair.Key == null)
                    {
                        throw new ArgumentException("The details collection cannot contain a null key.", nameof(details));
                    }

                    if (pair.Value == null)
                    {
                        throw new ArgumentException("The details collection cannot contain a null value.", nameof(details));
                    }

                    copy.Add(pair.Key, pair.Value);
                }
            }

            return new ReadOnlyDictionary<string, WorkflowValue>(copy);
        }
    }
}
