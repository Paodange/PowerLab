namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// Open string event type names defined by the V1 runtime stream.
    /// </summary>
    public static class RuntimeEventTypes
    {
        public const string RunStatusChanged = "run.statusChanged";
        public const string RunQueueChanged = "run.queueChanged";
        public const string RunFaultRaised = "run.faultRaised";
        public const string RunFaultResolved = "run.faultResolved";
        public const string NodeStatusChanged = "node.statusChanged";
        public const string NodeAttemptStarted = "node.attemptStarted";
        public const string NodeAttemptCompleted = "node.attemptCompleted";
        public const string VariableChanged = "variable.changed";
        public const string LogAppended = "log.appended";
        public const string DeviceStatusChanged = "device.statusChanged";

        public const string RunStatusChangedEvent = RunStatusChanged;
        public const string RunQueueChangedEvent = RunQueueChanged;
        public const string RunFaultRaisedEvent = RunFaultRaised;
        public const string RunFaultResolvedEvent = RunFaultResolved;
        public const string NodeStatusChangedEvent = NodeStatusChanged;
        public const string NodeAttemptStartedEvent = NodeAttemptStarted;
        public const string NodeAttemptCompletedEvent = NodeAttemptCompleted;
        public const string VariableChangedEvent = VariableChanged;
        public const string LogAppendedEvent = LogAppended;
        public const string DeviceStatusChangedEvent = DeviceStatusChanged;
    }
}
