using System;

namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// A control command accepted for asynchronous processing.
    /// </summary>
    public class CommandAcceptedDto
    {
        public string CommandId { get; set; } = string.Empty;
        public string RunId { get; set; } = string.Empty;
        public string Command { get; set; } = string.Empty;
        public DateTimeOffset AcceptedAt { get; set; }
        public RunStatus StatusAtAcceptance { get; set; }
    }
}
