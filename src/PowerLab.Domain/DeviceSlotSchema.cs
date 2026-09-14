namespace PowerLab.Domain
{
    /// <summary>
    /// Describes a device slot required by a plugin node type.
    /// </summary>
    public sealed class DeviceSlotSchema
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string RequiredDeviceTypeId { get; set; } = string.Empty;
        public bool Required { get; set; }
        public string? Description { get; set; }
    }
}
