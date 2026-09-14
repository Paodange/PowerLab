namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Decision recorded when a run fault is resolved or terminated.
    /// </summary>
    public enum FaultResolutionAction
    {
        Retry,
        Ignore,
        Terminate
    }
}
