using System.Collections.Generic;

namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Framework-neutral ProblemDetails payload returned by RuntimeHost.
    /// </summary>
    public sealed class PowerLabProblemDetailsDto
    {
        public string? Type { get; set; }
        public string? Title { get; set; }
        public int? Status { get; set; }
        public string? Detail { get; set; }
        public string? Instance { get; set; }
        public string? Code { get; set; }
        public string? TraceId { get; set; }
        public IReadOnlyList<ValidationIssueDto>? ValidationIssues { get; set; }
    }
}
