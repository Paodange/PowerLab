namespace PowerLab.Plugin.Abstractions
{
    /// <summary>
    /// Indicates that a node attempted to access a slot without a granted lease.
    /// </summary>
    public sealed class DeviceSlotNotGrantedException : PluginContractException
    {
        /// <summary>
        /// Creates a missing or ungranted slot exception.
        /// </summary>
        public DeviceSlotNotGrantedException(string slotId)
            : base(CreateMessage(slotId))
        {
            SlotId = RequireSlotId(slotId);
        }

        /// <summary>
        /// Gets the slot identifier that was not granted.
        /// </summary>
        public string SlotId { get; }

        private static string CreateMessage(string slotId)
        {
            return "No device capability was granted for slot '" + RequireSlotId(slotId) + "'.";
        }

        private static string RequireSlotId(string slotId)
        {
            if (slotId == null)
            {
                throw new System.ArgumentNullException(nameof(slotId));
            }

            if (slotId.Trim().Length == 0)
            {
                throw new System.ArgumentException("SlotId cannot be empty.", nameof(slotId));
            }

            return slotId;
        }
    }
}
