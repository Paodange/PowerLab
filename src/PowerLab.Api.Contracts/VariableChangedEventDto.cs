using PowerLab.Domain;

namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Payload for a variable.changed event.
    /// </summary>
    public sealed class VariableChangedEventDto
    {
        public string VariableId { get; set; } = string.Empty;
        public WorkflowValueType ValueType { get; set; }
        public WorkflowValueDto Value { get; set; } = WorkflowValueDto.FromString(string.Empty);
        public string? SourceNodeId { get; set; }
        public string? AttemptId { get; set; }
    }
}
