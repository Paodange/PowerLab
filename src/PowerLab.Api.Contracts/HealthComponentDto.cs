namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Protocol-level health information for one RuntimeHost component.
    /// </summary>
    public sealed class HealthComponentDto
    {
        public string Name { get; set; } = string.Empty;
        public HealthStatus Status { get; set; }
        public string? Description { get; set; }
    }
}
