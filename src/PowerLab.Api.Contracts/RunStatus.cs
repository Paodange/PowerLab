namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Lifecycle state of a workflow run.
    /// </summary>
    public enum RunStatus
    {
        Queued,
        Preparing,
        Running,
        Pausing,
        Paused,
        Stepping,
        Faulting,
        FaultedAwaitingDecision,
        Recovering,
        Terminating,
        Completed,
        Failed,
        Terminated,
        Interrupted
    }
}
