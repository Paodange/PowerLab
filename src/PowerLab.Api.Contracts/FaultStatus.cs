namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Resolution state of a run fault.
    /// </summary>
    public enum FaultStatus
    {
        Unresolved,
        Retrying,
        ResolvedByRetry,
        Ignored,
        Terminated
    }
}
