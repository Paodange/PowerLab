namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Request to create a workflow draft.
    /// </summary>
    public sealed class CreateWorkflowRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
