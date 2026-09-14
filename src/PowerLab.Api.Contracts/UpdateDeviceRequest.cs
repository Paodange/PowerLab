using System.Collections.Generic;

namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Request to update a device instance and, when supplied, its connection settings.
    /// </summary>
    public sealed class UpdateDeviceRequest
    {
        public string? DisplayName { get; set; }
        public IReadOnlyDictionary<string, string>? ConnectionSettings { get; set; }
    }
}
