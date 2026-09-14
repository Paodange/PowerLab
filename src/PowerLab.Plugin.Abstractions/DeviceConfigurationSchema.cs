using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PowerLab.Plugin.Abstractions
{
    /// <summary>
    /// Describes the scalar connection settings accepted by a device type.
    /// </summary>
    public sealed class DeviceConfigurationSchema
    {
        private IReadOnlyList<DeviceConfigurationFieldSchema> _fields
            = Array.Empty<DeviceConfigurationFieldSchema>();

        /// <summary>
        /// Gets or sets the declared connection configuration fields.
        /// </summary>
        public IReadOnlyList<DeviceConfigurationFieldSchema> Fields
        {
            get { return _fields; }
            set { _fields = CopyFields(value); }
        }

        private static IReadOnlyList<DeviceConfigurationFieldSchema> CopyFields(
            IReadOnlyList<DeviceConfigurationFieldSchema> fields)
        {
            if (fields == null)
            {
                throw new ArgumentNullException(nameof(fields));
            }

            List<DeviceConfigurationFieldSchema> copy
                = new List<DeviceConfigurationFieldSchema>(fields.Count);
            foreach (DeviceConfigurationFieldSchema field in fields)
            {
                if (field == null)
                {
                    throw new ArgumentException("The field collection cannot contain null values.", nameof(fields));
                }

                copy.Add(field);
            }

            return new ReadOnlyCollection<DeviceConfigurationFieldSchema>(copy);
        }
    }
}
