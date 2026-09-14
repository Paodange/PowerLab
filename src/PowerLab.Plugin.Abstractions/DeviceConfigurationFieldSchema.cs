using PowerLab.Domain;

namespace PowerLab.Plugin.Abstractions
{
    /// <summary>
    /// Describes one scalar field in a device connection configuration schema.
    /// </summary>
    public sealed class DeviceConfigurationFieldSchema
    {
        /// <summary>
        /// Gets or sets the stable field identifier.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the field name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the optional field description.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the scalar value type accepted by the field.
        /// </summary>
        public WorkflowValueType ValueType { get; set; }

        /// <summary>
        /// Gets or sets whether the field must be supplied.
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Gets or sets whether the field contains a secret and must be redacted by hosts.
        /// </summary>
        public bool Secret { get; set; }
    }
}
