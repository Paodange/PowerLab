using System.Collections.Generic;

namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Safe, structured error information crossing the RuntimeHost boundary.
    /// </summary>
    public sealed class StructuredErrorDto
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Category { get; set; }
        public bool? IsTransient { get; set; }
        public IReadOnlyDictionary<string, string>? Details { get; set; }
    }
}
