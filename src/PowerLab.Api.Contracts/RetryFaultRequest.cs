namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Optional audit comment for a manual fault retry decision.
    /// </summary>
    public sealed class RetryFaultRequest
    {
        public string? Comment { get; set; }
    }
}
