using System;
using System.Collections.Generic;
using System.Text.Json;
using PowerLab.Api.Contracts;
using PowerLab.Domain;
using Xunit;
using ApiValidationIssueSeverity = PowerLab.Api.Contracts.ValidationIssueSeverity;

namespace PowerLab.IntegrationTests.Contracts
{
    public sealed class ApiContractsSerializationTests
    {
        private static readonly DateTimeOffset FixedTime =
            new DateTimeOffset(2026, 9, 14, 8, 0, 0, TimeSpan.Zero);

        [Fact]
        public void SystemInfoUsesDocumentedJsonShape()
        {
            SystemInfoDto value = new SystemInfoDto
            {
                RuntimeInstanceId = "runtime-local",
                RuntimeVersion = "1.0.0",
                ApiVersion = "1.0",
                WorkflowSchemaVersion = "1.0",
                PluginSdkVersion = "1.0",
                PythonVersion = "3.12.0",
                Platform = "win-x64",
                StartedAt = FixedTime
            };

            string json = ApiJson.Serialize(value);

            Assert.Contains("\"runtimeInstanceId\":\"runtime-local\"", json);
            Assert.Contains("\"startedAt\":\"2026-09-14T08:00:00+00:00\"", json);
            Assert.DoesNotContain("RuntimeInstanceId", json);
        }

        [Fact]
        public void ProblemDetailsIncludesStandardAndPowerLabExtensions()
        {
            PowerLabProblemDetailsDto value = new PowerLabProblemDetailsDto
            {
                Type = "https://powerlab.dev/problems/workflow-validation-failed",
                Title = "Workflow validation failed",
                Status = 422,
                Detail = "The workflow contains 1 error.",
                Instance = "/api/v1/workflows/workflow-1/publish",
                Code = "workflow.validationFailed",
                TraceId = "00-trace"
            };
            value.WithValidationIssues(new List<ValidationIssueDto>
            {
                new ValidationIssueDto
                {
                    Severity = ApiValidationIssueSeverity.Error,
                    Code = "device.notBound",
                    Message = "Required device slot 'robot' is not bound."
                }
            });

            string json = ApiJson.Serialize(value);

            Assert.Contains("\"type\":\"https://powerlab.dev/problems/workflow-validation-failed\"", json);
            Assert.Contains("\"status\":422", json);
            Assert.Contains("\"code\":\"workflow.validationFailed\"", json);
            Assert.Contains("\"traceId\":\"00-trace\"", json);
            Assert.Contains("\"validationIssues\":[{", json);
            Assert.Contains("\"severity\":\"error\"", json);
        }

        [Fact]
        public void CreateWorkflowRequestUsesCamelCaseAndRoundTrips()
        {
            CreateWorkflowRequest value = new CreateWorkflowRequest
            {
                Name = "Temperature Control",
                Description = "Optional description"
            };

            CreateWorkflowRequest roundTrip = ApiJson.Deserialize<CreateWorkflowRequest>(ApiJson.Serialize(value));

            Assert.Equal("Temperature Control", roundTrip.Name);
            Assert.Equal("Optional description", roundTrip.Description);
            Assert.Equal("{\"name\":\"Temperature Control\",\"description\":\"Optional description\"}", ApiJson.Serialize(value));
        }

        [Fact]
        public void ValidationReportAndReleasePreserveDomainDocument()
        {
            ValidationReportDto report = new ValidationReportDto
            {
                Valid = false,
                ValidatedAt = FixedTime,
                Issues = new List<ValidationIssueDto>
                {
                    new ValidationIssueDto
                    {
                        Severity = ApiValidationIssueSeverity.Warning,
                        Code = "graph.unreachable",
                        Message = "Node is unreachable.",
                        Location = new ValidationIssueLocationDto { NodeId = "node-1" }
                    }
                }
            };
            WorkflowReleaseDto release = new WorkflowReleaseDto
            {
                ReleaseId = "release-001",
                WorkflowId = "workflow-1",
                ReleaseNumber = 1,
                PublishedAt = FixedTime,
                ExecutionHash = "sha256:example",
                RequiredPlugins = new List<RequiredPluginDto>
                {
                    new RequiredPluginDto
                    {
                        PluginId = "com.vendor.robot",
                        PluginVersion = "1.2.0",
                        NodeVersions = new List<int> { 1 }
                    }
                },
                Document = NewDocument()
            };

            string reportJson = ApiJson.Serialize(report);
            string releaseJson = ApiJson.Serialize(release);
            ValidationReportDto reportRoundTrip = ApiJson.Deserialize<ValidationReportDto>(reportJson);
            WorkflowReleaseDto releaseRoundTrip = ApiJson.Deserialize<WorkflowReleaseDto>(releaseJson);

            Assert.Contains("\"validatedAt\":\"2026-09-14T08:00:00+00:00\"", reportJson);
            Assert.Equal("graph.unreachable", reportRoundTrip.Issues[0].Code);
            Assert.Equal("release-001", releaseRoundTrip.ReleaseId);
            Assert.Equal(1, releaseRoundTrip.RequiredPlugins[0].NodeVersions[0]);
            Assert.Equal("workflow-1", releaseRoundTrip.Document.WorkflowId);
        }

        [Fact]
        public void DeviceSummaryAndRunRequestsUseStableEnumText()
        {
            DeviceSummaryDto device = new DeviceSummaryDto
            {
                DeviceId = "device-robot-a",
                DisplayName = "Robot A",
                PluginId = "com.vendor.robot",
                DeviceTypeId = "com.vendor.robot/robot-v1",
                State = DeviceState.Ready
            };
            CreateRunRequest request = new CreateRunRequest
            {
                ReleaseId = "release-001",
                Inputs = new Dictionary<string, WorkflowValueDto>
                {
                    { "input-temperature", WorkflowValueDto.FromNumber(37.5) }
                },
                Mode = RunMode.Normal
            };

            string deviceJson = ApiJson.Serialize(device);
            string requestJson = ApiJson.Serialize(request);
            CreateRunRequest roundTrip = ApiJson.Deserialize<CreateRunRequest>(requestJson);

            Assert.Contains("\"state\":\"ready\"", deviceJson);
            Assert.Contains("\"inputs\":{\"input-temperature\":37.5}", requestJson);
            Assert.Contains("\"mode\":\"normal\"", requestJson);
            Assert.Equal(WorkflowValueType.Number, roundTrip.Inputs["input-temperature"].ValueType);
            Assert.Equal(37.5, roundTrip.Inputs["input-temperature"].NumberValue);
            Assert.Equal(WorkflowValueType.Number, ApiJson.Deserialize<WorkflowValueDto>(ApiJson.Serialize(
                WorkflowValueDto.FromNumber(12.0))).ValueType);
        }

        [Fact]
        public void RunAndSnapshotPreserveTimeStatusValuesAndScalarMaps()
        {
            RunDto run = new RunDto
            {
                RunId = "run-001",
                ReleaseId = "release-001",
                WorkflowId = "workflow-1",
                Status = RunStatus.Queued,
                Mode = RunMode.Debug,
                QueuePosition = 1,
                CreatedAt = FixedTime,
                LastEventSequence = 0
            };
            RunSnapshotDto snapshot = new RunSnapshotDto
            {
                Run = run,
                Inputs = new Dictionary<string, WorkflowValueDto>
                {
                    { "input-1", WorkflowValueDto.FromInteger(3) }
                },
                Variables = new Dictionary<string, WorkflowValueDto>
                {
                    { "variable-1", WorkflowValueDto.FromBoolean(true) }
                },
                Nodes = new List<NodeRuntimeStateDto>
                {
                    new NodeRuntimeStateDto { NodeId = "node-1", Status = NodeRuntimeStatus.Running }
                },
                LastEventSequence = 125
            };

            string json = ApiJson.Serialize(snapshot);
            RunSnapshotDto roundTrip = ApiJson.Deserialize<RunSnapshotDto>(json);

            Assert.Contains("\"status\":\"queued\"", json);
            Assert.Contains("\"startedAt\":null", json);
            Assert.Contains("\"input-1\":3", json);
            Assert.Equal(125, roundTrip.LastEventSequence);
            Assert.Equal(NodeRuntimeStatus.Running, roundTrip.Nodes[0].Status);
            Assert.Equal(WorkflowValueType.Boolean, roundTrip.Variables["variable-1"].ValueType);
        }

        [Fact]
        public void AttemptsFaultsResolutionAndCommandResultRoundTrip()
        {
            NodeAttemptDto attempt = new NodeAttemptDto
            {
                AttemptId = "attempt-001",
                RunId = "run-001",
                NodeId = "node-1",
                AttemptNumber = 2,
                Status = NodeRuntimeStatus.Failed,
                FinishedAt = FixedTime,
                Error = new StructuredErrorDto { Code = "device.offline", Message = "Device is offline." }
            };
            RunFaultDto fault = new RunFaultDto
            {
                FaultId = "fault-001",
                RunId = "run-001",
                NodeId = "node-1",
                AttemptId = "attempt-001",
                Error = attempt.Error,
                Status = FaultStatus.Ignored,
                RaisedAt = FixedTime,
                Resolution = new FaultResolutionDto
                {
                    Action = FaultResolutionAction.Ignore,
                    ResolvedAt = FixedTime,
                    Comment = "Accepted for this run."
                }
            };
            CommandAcceptedDto command = new CommandAcceptedDto
            {
                CommandId = "command-001",
                RunId = "run-001",
                Command = "pause",
                AcceptedAt = FixedTime,
                StatusAtAcceptance = RunStatus.Running
            };

            NodeAttemptDto attemptRoundTrip = ApiJson.Deserialize<NodeAttemptDto>(ApiJson.Serialize(attempt));
            RunFaultDto faultRoundTrip = ApiJson.Deserialize<RunFaultDto>(ApiJson.Serialize(fault));
            CommandAcceptedDto commandRoundTrip = ApiJson.Deserialize<CommandAcceptedDto>(ApiJson.Serialize(command));

            Assert.Equal(2, attemptRoundTrip.AttemptNumber);
            Assert.NotNull(attemptRoundTrip.Error);
            Assert.Equal("device.offline", attemptRoundTrip.Error!.Code);
            Assert.Equal(FaultStatus.Ignored, faultRoundTrip.Status);
            Assert.NotNull(faultRoundTrip.Resolution);
            Assert.Equal(FaultResolutionAction.Ignore, faultRoundTrip.Resolution!.Action);
            Assert.Equal(RunStatus.Running, commandRoundTrip.StatusAtAcceptance);
        }

        [Fact]
        public void CursorPageAndEventEnvelopeKeepOpaqueCursorAndUnknownType()
        {
            CursorPageDto<DeviceSummaryDto> page = new CursorPageDto<DeviceSummaryDto>
            {
                Items = new List<DeviceSummaryDto>
                {
                    new DeviceSummaryDto { DeviceId = "device-1", State = DeviceState.Ready }
                },
                NextCursor = "opaque-token/abc"
            };
            RuntimeEventEnvelopeDto envelope = new RuntimeEventEnvelopeDto
            {
                EventId = "event-001",
                StreamId = "run-001",
                Sequence = 126,
                EventType = "future.event",
                EventVersion = 7,
                OccurredAt = FixedTime,
                CorrelationId = "run-001",
                Payload = JsonSerializer.SerializeToElement(new { value = 1 })
            };

            string pageJson = ApiJson.Serialize(page);
            RuntimeEventEnvelopeDto roundTrip = ApiJson.Deserialize<RuntimeEventEnvelopeDto>(ApiJson.Serialize(envelope));

            Assert.Contains("\"nextCursor\":\"opaque-token/abc\"", pageJson);
            Assert.Equal("future.event", roundTrip.EventType);
            Assert.Equal(7, roundTrip.EventVersion);
            Assert.Equal(1, roundTrip.Payload.GetProperty("value").GetInt32());
        }

        [Fact]
        public void AllV1EventPayloadsUseDocumentedNamesAndEnums()
        {
            RunStatusChangedEventDto runStatus = new RunStatusChangedEventDto
            {
                RunId = "run-1",
                Status = RunStatus.FaultedAwaitingDecision
            };
            RunQueueChangedEventDto queue = new RunQueueChangedEventDto { RunId = "run-1", QueuePosition = 2 };
            RunFaultRaisedEventDto raised = new RunFaultRaisedEventDto
            {
                FaultId = "fault-1",
                RunId = "run-1",
                NodeId = "node-1",
                AttemptId = "attempt-1",
                Error = new StructuredErrorDto { Code = "node.failed", Message = "failed" },
                Status = FaultStatus.Unresolved,
                RaisedAt = FixedTime
            };
            RunFaultResolvedEventDto resolved = new RunFaultResolvedEventDto
            {
                FaultId = "fault-1",
                RunId = "run-1",
                Status = FaultStatus.ResolvedByRetry,
                Resolution = new FaultResolutionDto { Action = FaultResolutionAction.Retry, ResolvedAt = FixedTime }
            };
            NodeStatusChangedEventDto nodeStatus = new NodeStatusChangedEventDto
            {
                NodeId = "node-1", AttemptId = "attempt-1", Status = NodeRuntimeStatus.WaitingForResource, WaveId = "wave-1"
            };
            NodeAttemptStartedEventDto started = new NodeAttemptStartedEventDto
            {
                AttemptId = "attempt-1", RunId = "run-1", NodeId = "node-1", AttemptNumber = 1, StartedAt = FixedTime
            };
            NodeAttemptCompletedEventDto completed = new NodeAttemptCompletedEventDto
            {
                AttemptId = "attempt-1", RunId = "run-1", NodeId = "node-1", AttemptNumber = 1,
                Status = NodeRuntimeStatus.Succeeded, FinishedAt = FixedTime
            };
            VariableChangedEventDto variable = new VariableChangedEventDto
            {
                VariableId = "variable-1", ValueType = WorkflowValueType.Number, Value = WorkflowValueDto.FromNumber(36.8),
                SourceNodeId = "node-1", AttemptId = "attempt-1"
            };
            LogAppendedEventDto log = new LogAppendedEventDto
            {
                Level = RunLogLevel.Information,
                Message = "Temperature read completed.",
                Properties = new Dictionary<string, WorkflowValueDto>
                {
                    { "temperature", WorkflowValueDto.FromNumber(36.8) }
                }
            };
            DeviceStatusChangedEventDto device = new DeviceStatusChangedEventDto
            {
                DeviceId = "device-1", State = DeviceState.Ready
            };

            Assert.Contains("\"status\":\"faultedAwaitingDecision\"", ApiJson.Serialize(runStatus));
            Assert.Contains("\"queuePosition\":2", ApiJson.Serialize(queue));
            Assert.Contains("\"status\":\"unresolved\"", ApiJson.Serialize(raised));
            Assert.Contains("\"action\":\"retry\"", ApiJson.Serialize(resolved));
            Assert.Contains("\"status\":\"waitingForResource\"", ApiJson.Serialize(nodeStatus));
            Assert.Contains("\"attemptNumber\":1", ApiJson.Serialize(started));
            Assert.Contains("\"status\":\"succeeded\"", ApiJson.Serialize(completed));
            Assert.Contains("\"valueType\":\"number\"", ApiJson.Serialize(variable));
            Assert.Contains("\"level\":\"information\"", ApiJson.Serialize(log));
            Assert.Contains("\"state\":\"ready\"", ApiJson.Serialize(device));
        }

        [Fact]
        public void EnumTextMatchesEveryRunContractEnumAndNumericValuesAreRejected()
        {
            Assert.Contains("\"cancelRequested\"", ApiJson.Serialize(NodeRuntimeStatus.CancelRequested));
            Assert.Contains("\"faultedAwaitingDecision\"", ApiJson.Serialize(RunStatus.FaultedAwaitingDecision));
            Assert.Contains("\"resolvedByRetry\"", ApiJson.Serialize(FaultStatus.ResolvedByRetry));
            Assert.Contains("\"debug\"", ApiJson.Serialize(RunMode.Debug));
            Assert.Throws<JsonException>(() => ApiJson.Deserialize<RunStatus>("3"));
            Assert.Throws<JsonException>(() => ApiJson.Deserialize<NodeRuntimeStatus>("1"));
            Assert.Throws<JsonException>(() => ApiJson.Deserialize<FaultStatus>("2"));
            Assert.Throws<JsonException>(() => ApiJson.Deserialize<RunMode>("1"));
        }

        [Fact]
        public void UnknownOrdinaryFieldsAreIgnoredAndDomainPolymorphismIsPreserved()
        {
            string requestJson = "{\"name\":\"Demo\",\"futureField\":true}";
            CreateWorkflowRequest request = ApiJson.Deserialize<CreateWorkflowRequest>(requestJson);
            ActionNode action = new ActionNode
            {
                Id = "node-action",
                DisplayName = "Action",
                NodeType = new NodeTypeReference
                {
                    PluginId = "plugin-1",
                    PluginVersion = "1.0.0",
                    NodeTypeId = "action-1",
                    NodeVersion = 1
                }
            };
            WorkflowDocument document = NewDocument();
            document.RootScope.Nodes = new List<WorkflowNodeDefinition> { action };

            WorkflowDocument roundTrip = ApiJson.Deserialize<WorkflowDocument>(ApiJson.Serialize(document));
            WorkflowNodeDefinition node = roundTrip.RootScope.Nodes[0];

            Assert.Equal("Demo", request.Name);
            Assert.IsType<ActionNode>(node);
            Assert.Equal("plugin-1", ((ActionNode)node).NodeType.PluginId);
        }

        [Fact]
        public void NodeDescriptorDtoMapsToDomainWithoutCopyingSchemaTypes()
        {
            NodeDescriptor descriptor = new NodeDescriptor
            {
                DescriptorVersion = 1,
                NodeType = new NodeTypeReference { NodeTypeId = "move", NodeVersion = 1 },
                DisplayName = "Move",
                Parameters = new List<ParameterSchema>
                {
                    new ParameterSchema { Id = "speed", ValueType = WorkflowValueType.Number }
                },
                PauseMode = PauseMode.NodeBoundary
            };

            NodeDescriptorDto dto = descriptor.ToDto();
            NodeDescriptor roundTrip = ApiJson.Deserialize<NodeDescriptorDto>(ApiJson.Serialize(dto)).ToDomain();

            Assert.Equal("move", roundTrip.NodeType.NodeTypeId);
            Assert.Same(descriptor.Parameters[0].GetType(), roundTrip.Parameters[0].GetType());
            Assert.Equal(WorkflowValueType.Number, roundTrip.Parameters[0].ValueType);
        }

        private static WorkflowDocument NewDocument()
        {
            return new WorkflowDocument
            {
                SchemaVersion = "1.0",
                WorkflowId = "workflow-1",
                Name = "Demo",
                RootScope = new WorkflowScope
                {
                    Id = "scope-root",
                    EntryNodeId = "entry",
                    ExitNodeId = "exit"
                }
            };
        }
    }
}
