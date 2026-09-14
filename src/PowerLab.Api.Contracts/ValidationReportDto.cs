using System;
using System.Collections.Generic;

namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Result of validating a workflow document.
    /// </summary>
    public sealed class ValidationReportDto
    {
        public bool Valid { get; set; }
        public DateTimeOffset ValidatedAt { get; set; }
        public IReadOnlyList<ValidationIssueDto> Issues { get; set; }
            = new List<ValidationIssueDto>();
    }
}
