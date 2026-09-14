namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Optional audit comment for a manual fault ignore decision.
    /// </summary>
    public sealed class IgnoreFaultRequest
    {
        public string? Comment { get; set; }
    }
}
