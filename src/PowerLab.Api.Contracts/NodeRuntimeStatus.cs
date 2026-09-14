namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Current or historical state of one node attempt.
    /// </summary>
    public enum NodeRuntimeStatus
    {
        Pending,
        Ready,
        WaitingForResource,
        Running,
        PauseRequested,
        Paused,
        Succeeded,
        Failed,
        Ignored,
        CancelRequested,
        Cancelled,
        Skipped
    }
}
