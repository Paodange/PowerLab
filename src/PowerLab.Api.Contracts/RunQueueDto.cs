using System.Collections.Generic;

namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Current run scheduler queue projection.
    /// </summary>
    public sealed class RunQueueDto
    {
        public IReadOnlyList<RunQueueItemDto> Items { get; set; }
            = new List<RunQueueItemDto>();
    }
}
