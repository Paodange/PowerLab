using System.Collections.Generic;

namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Safe structured diagnostic information for plugin discovery and loading.
    /// </summary>
    public sealed class PluginDiagnosticDto
    {
        public PluginDiagnosticSeverity Severity { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public IReadOnlyDictionary<string, string>? Details { get; set; }
    }
}
