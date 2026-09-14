using System;

namespace PowerLab.Plugin.Abstractions
{
    /// <summary>
    /// Indicates that a granted slot does not expose the requested capability type.
    /// </summary>
    public sealed class DeviceCapabilityMismatchException : PluginContractException
    {
        /// <summary>
        /// Creates a capability mismatch exception.
        /// </summary>
        public DeviceCapabilityMismatchException(string slotId, Type requestedCapabilityType)
            : base(CreateMessage(slotId, requestedCapabilityType))
        {
            SlotId = RequireSlotId(slotId);
            RequestedCapabilityType = requestedCapabilityType;
        }

        /// <summary>
        /// Gets the slot identifier whose capability was mismatched.
        /// </summary>
        public string SlotId { get; }

        /// <summary>
        /// Gets the capability type requested by the executor.
        /// </summary>
        public Type RequestedCapabilityType { get; }

        private static string CreateMessage(string slotId, Type requestedCapabilityType)
        {
            RequireSlotId(slotId);
            if (requestedCapabilityType == null)
            {
                throw new ArgumentNullException(nameof(requestedCapabilityType));
            }

            return "The device in slot '" + (slotId ?? "<null>")
                + "' does not provide capability '" + requestedCapabilityType.FullName + "'.";
        }

        private static string RequireSlotId(string slotId)
        {
            if (slotId == null)
            {
                throw new ArgumentNullException(nameof(slotId));
            }

            if (slotId.Trim().Length == 0)
            {
                throw new ArgumentException("SlotId cannot be empty.", nameof(slotId));
            }

            return slotId;
        }
    }
}
