namespace PowerLab.Plugin.Abstractions
{
    /// <summary>
    /// Resolves only the device capabilities granted to a node's declared slots.
    /// </summary>
    public interface INodeDeviceAccessor
    {
        /// <summary>
        /// Gets the required capability for a granted slot.
        /// </summary>
        TDevice GetRequired<TDevice>(string slotId)
            where TDevice : class;

        /// <summary>
        /// Tries to get an optional capability for a granted slot.
        /// </summary>
        bool TryGet<TDevice>(string slotId, out TDevice? device)
            where TDevice : class;
    }
}
