namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Aggregate health state reported by the health query contract.
    /// </summary>
    public enum HealthStatus
    {
        Healthy,
        Degraded,
        Unhealthy
    }
}
