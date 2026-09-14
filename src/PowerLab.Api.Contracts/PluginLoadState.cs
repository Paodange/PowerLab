namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Public projection of a plugin's catalog loading state.
    /// </summary>
    public enum PluginLoadState
    {
        Loaded,
        Failed,
        Disabled,
        Unloaded,
        Unknown
    }
}
