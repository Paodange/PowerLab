namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Optional location details for a validation issue.
    /// </summary>
    public sealed class ValidationIssueLocationDto
    {
        public string? JsonPointer { get; set; }
        public string? ScopeId { get; set; }
        public string? NodeId { get; set; }
        public string? ParameterId { get; set; }
        public string? OutputId { get; set; }
        public string? DeviceSlotId { get; set; }
        public string? Path { get; set; }
    }
}
