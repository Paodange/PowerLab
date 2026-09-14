using System.Collections.Generic;

namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Plugin dependency recorded by a published workflow release.
    /// </summary>
    public sealed class RequiredPluginDto
    {
        public string PluginId { get; set; } = string.Empty;
        public string PluginVersion { get; set; } = string.Empty;
        public IReadOnlyList<int> NodeVersions { get; set; }
            = new List<int>();
    }
}
