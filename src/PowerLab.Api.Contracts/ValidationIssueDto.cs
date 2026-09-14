namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// API projection of a structured workflow validation issue.
    /// </summary>
    public sealed class ValidationIssueDto
    {
        public ValidationIssueSeverity Severity { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public ValidationIssueLocationDto Location { get; set; }
            = new ValidationIssueLocationDto();
    }
}
