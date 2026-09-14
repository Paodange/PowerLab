using System.Collections.Generic;

namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Request to create a configured device instance.
    /// </summary>
    public sealed class CreateDeviceRequest
    {
        public string DisplayName { get; set; } = string.Empty;
        public string DeviceTypeId { get; set; } = string.Empty;
        public IReadOnlyDictionary<string, string> ConnectionSettings { get; set; }
            = new Dictionary<string, string>();
    }
}
