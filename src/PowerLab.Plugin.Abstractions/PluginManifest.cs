using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PowerLab.Plugin.Abstractions
{
    /// <summary>
    /// Describes the data required to identify and load a PowerLab plugin.
    /// </summary>
    public sealed class PluginManifest
    {
        private IReadOnlyList<string> _supportedRuntimes = Array.Empty<string>();
        private IReadOnlyList<string> _capabilities = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the manifest contract version.
        /// </summary>
        public int ManifestVersion { get; set; }

        /// <summary>
        /// Gets or sets the globally stable plugin identifier.
        /// </summary>
        public string PluginId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the display name shown to users.
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the semantic plugin version.
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the SDK contract version required by the plugin.
        /// </summary>
        public string SdkVersion { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the path of the plugin entry assembly within the package.
        /// </summary>
        public string EntryAssembly { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the entry type name within the entry assembly.
        /// </summary>
        public string EntryType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Runtime Identifiers supported by the plugin.
        /// </summary>
        public IReadOnlyList<string> SupportedRuntimes
        {
            get { return _supportedRuntimes; }
            set { _supportedRuntimes = CopyStrings(value, nameof(SupportedRuntimes)); }
        }

        /// <summary>
        /// Gets or sets extensible capability identifiers declared by the plugin.
        /// </summary>
        public IReadOnlyList<string> Capabilities
        {
            get { return _capabilities; }
            set { _capabilities = CopyStrings(value, nameof(Capabilities)); }
        }

        private static IReadOnlyList<string> CopyStrings(
            IReadOnlyList<string> values,
            string parameterName)
        {
            if (values == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            List<string> copy = new List<string>(values.Count);
            foreach (string value in values)
            {
                if (value == null)
                {
                    throw new ArgumentException("The collection cannot contain null values.", parameterName);
                }

                copy.Add(value);
            }

            return new ReadOnlyCollection<string>(copy);
        }
    }
}
