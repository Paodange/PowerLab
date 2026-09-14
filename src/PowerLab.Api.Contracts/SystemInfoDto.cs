namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Runtime and protocol versions reported by the local RuntimeHost.
    /// </summary>
    public sealed class SystemInfoDto
    {
        public string RuntimeInstanceId { get; set; } = string.Empty;
        public string RuntimeVersion { get; set; } = string.Empty;
        public string ApiVersion { get; set; } = string.Empty;
        public string WorkflowSchemaVersion { get; set; } = string.Empty;
        public string PluginSdkVersion { get; set; } = string.Empty;
        public string PythonVersion { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public System.DateTimeOffset StartedAt { get; set; }
    }
}
