namespace PowerLab.Domain
{
    /// <summary>
    /// Severity of a workflow validation issue.
    /// </summary>
    public enum ValidationSeverity
    {
        Error,
        Warning,
        Info
    }

    /// <summary>
    /// A stable location within a workflow document.
    /// </summary>
    public sealed class ValidationIssueLocation
    {
        public string JsonPointer { get; set; } = string.Empty;
        public string? ScopeId { get; set; }
        public string? NodeId { get; set; }
        public string? ParameterId { get; set; }
        public string? OutputId { get; set; }
        public string? DeviceSlotId { get; set; }
        public string? Path { get; set; }
    }

    /// <summary>
    /// A structured, serializable workflow validation result item.
    /// </summary>
    public sealed class ValidationIssue
    {
        public ValidationSeverity Severity { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public ValidationIssueLocation Location { get; set; }
            = new ValidationIssueLocation();
    }

    /// <summary>
    /// Compatibility aliases for callers that name the severity after its containing DTO.
    /// </summary>
    public static class ValidationIssueSeverity
    {
        public const ValidationSeverity Error = ValidationSeverity.Error;
        public const ValidationSeverity Warning = ValidationSeverity.Warning;
        public const ValidationSeverity Info = ValidationSeverity.Info;
    }
}
