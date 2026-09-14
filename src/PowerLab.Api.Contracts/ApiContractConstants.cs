namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Version and size constants that are part of the public API contract.
    /// </summary>
    public static class ApiContractConstants
    {
        public const string ApiVersion = "1.0";
        public const string WorkflowSchemaVersion = "1.0";
        public const string PluginSdkVersion = "1.0";
        public const string BaseRoute = "/api/v1";
        public const string RuntimeHubPath = "/hubs/runtime";
        public const string JsonContentType = "application/json; charset=utf-8";
        public const int DefaultPageSize = 50;
        public const int MaxPageSize = 200;
    }
}
