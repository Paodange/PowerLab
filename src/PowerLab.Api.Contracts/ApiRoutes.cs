namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Stable route fragments used by the RuntimeHost API.
    /// </summary>
    public static class ApiRoutes
    {
        public const string System = ApiContractConstants.BaseRoute + "/system";
        public const string Plugins = ApiContractConstants.BaseRoute + "/plugins";
        public const string NodeTypes = ApiContractConstants.BaseRoute + "/node-types";
        public const string DeviceTypes = ApiContractConstants.BaseRoute + "/device-types";
        public const string Devices = ApiContractConstants.BaseRoute + "/devices";
        public const string Workflows = ApiContractConstants.BaseRoute + "/workflows";
        public const string WorkflowReleases = ApiContractConstants.BaseRoute + "/workflow-releases";
        public const string Runs = ApiContractConstants.BaseRoute + "/runs";
        public const string RunQueue = ApiContractConstants.BaseRoute + "/run-queue";
        public const string RuntimeHub = ApiContractConstants.RuntimeHubPath;
    }
}
