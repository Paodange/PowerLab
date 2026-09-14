namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Detailed node type response, including its portable descriptor.
    /// </summary>
    public sealed class NodeTypeDetailDto : NodeTypeSummaryDto
    {
        public NodeDescriptorDto Descriptor { get; set; } = new NodeDescriptorDto();
    }
}
