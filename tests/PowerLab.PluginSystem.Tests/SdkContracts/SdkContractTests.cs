using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using PowerLab.Domain;
using PowerLab.Plugin.Abstractions;
using Xunit;

namespace PowerLab.PluginSystem.Tests.SdkContracts
{
    public sealed class SdkContractTests
    {
        [Fact]
        public void MinimalPluginRegistersNodeProvider()
        {
            RecordingPluginBuilder builder = new RecordingPluginBuilder();

            new MinimalPlugin().Configure(builder);

            Assert.Contains(typeof(FakeNodeProvider), builder.NodeProviderTypes);
        }

        [Fact]
        public void MinimalPluginRegistersDeviceProvider()
        {
            RecordingPluginBuilder builder = new RecordingPluginBuilder();

            new MinimalPlugin().Configure(builder);

            Assert.Contains(typeof(FakeDeviceProvider), builder.DeviceProviderTypes);
        }

        [Fact]
        public void MinimalPluginDoesNotReferenceRuntimeHost()
        {
            IEnumerable<string?> references = typeof(MinimalPlugin).Assembly
                .GetReferencedAssemblies()
                .Select(reference => reference.Name);

            Assert.DoesNotContain("PowerLab.RuntimeHost", references);
            Assert.DoesNotContain("PowerLab.Execution", references);
            Assert.DoesNotContain("Microsoft.EntityFrameworkCore", references);
        }

        [Fact]
        public void NodeProviderReturnsDomainDescriptor()
        {
            FakeNodeProvider provider = new FakeNodeProvider();

            IReadOnlyList<NodeDescriptor> descriptors = provider.GetNodeDescriptors();

            NodeDescriptor descriptor = Assert.Single(descriptors);
            Assert.Equal("move", descriptor.NodeType.NodeTypeId);
            Assert.Equal(2, descriptor.NodeType.NodeVersion);
            Assert.Equal(PauseMode.Cooperative, descriptor.PauseMode);
        }

        [Fact]
        public void ExecutorCreationRequestContainsNodeTypeIdAndVersion()
        {
            NodeExecutorCreateContext request = new NodeExecutorCreateContext("move", 2);

            Assert.Equal("move", request.NodeTypeId);
            Assert.Equal(2, request.NodeVersion);
        }

        [Fact]
        public void ProviderCreatesFreshExecutorPerAttempt()
        {
            FakeNodeProvider provider = new FakeNodeProvider();
            NodeExecutorCreateContext request = new NodeExecutorCreateContext("move", 2);

            INodeExecutor first = provider.CreateExecutor(request);
            INodeExecutor second = provider.CreateExecutor(request);

            Assert.NotSame(first, second);
        }

        [Fact]
        public void NodeExecutionContextSnapshotsInputs()
        {
            Dictionary<string, WorkflowValue> inputs = new Dictionary<string, WorkflowValue>();
            inputs.Add("speed", WorkflowValue.FromInteger(10));
            NodeExecutionContext context = CreateContext(inputs);

            inputs["speed"] = WorkflowValue.FromInteger(20);

            Assert.Equal(10, context.Inputs["speed"].IntegerValue);
            IDictionary<string, WorkflowValue> readOnlyInputs
                = Assert.IsAssignableFrom<IDictionary<string, WorkflowValue>>(context.Inputs);
            Assert.Throws<NotSupportedException>(
                () => readOnlyInputs.Add("newInput", WorkflowValue.FromString("blocked")));
        }

        [Fact]
        public void NodeExecutionContextDoesNotExposeWorkflowVariableStore()
        {
            PropertyInfo[] properties = typeof(NodeExecutionContext).GetProperties();

            Assert.Null(typeof(NodeExecutionContext).GetProperty("Variables"));
            Assert.Null(typeof(NodeExecutionContext).GetProperty("WorkflowVariables"));
            Assert.DoesNotContain(typeof(IServiceProvider), properties.Select(property => property.PropertyType));
        }

        [Fact]
        public void SuccessfulResultStoresOutputs()
        {
            Dictionary<string, WorkflowValue> outputs = new Dictionary<string, WorkflowValue>();
            outputs.Add("temperature", WorkflowValue.FromNumber(21.5));

            NodeExecutionResult result = NodeExecutionResult.Success(outputs);

            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
            Assert.Equal(21.5, result.Outputs["temperature"].NumberValue);
            Assert.Null(result.Error);
        }

        [Fact]
        public void FailedResultStoresPluginError()
        {
            PluginError error = new PluginError(
                "device.notReady",
                "The device is not ready.",
                "com.example.device",
                "measure",
                null,
                "device-a");

            NodeExecutionResult result = NodeExecutionResult.Failure(error);

            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
            Assert.Same(error, result.Error);
            Assert.Empty(result.Outputs);
        }

        [Fact]
        public void ResultSuccessAndFailureStatesCannotContradict()
        {
            NodeExecutionResult success = NodeExecutionResult.Success(
                new Dictionary<string, WorkflowValue>());
            NodeExecutionResult failure = NodeExecutionResult.Failure(
                new PluginError("failure", "Execution failed.", "plugin", "node"));

            Assert.True(success.IsSuccess && success.Error == null);
            Assert.True(!failure.IsSuccess && failure.Error != null);
        }

        [Fact]
        public void ResultCopiesCallerOutputDictionary()
        {
            Dictionary<string, WorkflowValue> outputs = new Dictionary<string, WorkflowValue>();
            outputs.Add("value", WorkflowValue.FromInteger(1));
            NodeExecutionResult result = NodeExecutionResult.Success(outputs);

            outputs["value"] = WorkflowValue.FromInteger(2);
            outputs.Add("other", WorkflowValue.FromBoolean(true));

            Assert.Equal(1, result.Outputs["value"].IntegerValue);
            Assert.DoesNotContain("other", result.Outputs.Keys);
        }

        [Fact]
        public async Task PauseTokenReportsRequestAndHonorsCancellation()
        {
            ManualPauseToken pauseToken = new ManualPauseToken();
            Assert.False(pauseToken.IsPauseRequested);
            await pauseToken.WaitForResumeAsync(CancellationToken.None);

            pauseToken.RequestPause();
            Assert.True(pauseToken.IsPauseRequested);

            using (CancellationTokenSource cancellation = new CancellationTokenSource())
            {
                cancellation.Cancel();
                await Assert.ThrowsAnyAsync<OperationCanceledException>(async delegate
                {
                    await pauseToken.WaitForResumeAsync(cancellation.Token);
                });
            }
        }

        [Fact]
        public void MissingDeviceSlotThrowsContractException()
        {
            FakeDeviceAccessor accessor = new FakeDeviceAccessor();

            DeviceSlotNotGrantedException exception = Assert.Throws<DeviceSlotNotGrantedException>(
                () => accessor.GetRequired<IRobotDevice>("robot"));

            Assert.Equal("robot", exception.SlotId);
            Assert.Contains("robot", exception.Message);
        }

        [Fact]
        public void DeviceCapabilityMismatchThrowsContractException()
        {
            FakeDeviceAccessor accessor = new FakeDeviceAccessor();
            accessor.Add("robot", new NonRobotDevice());

            DeviceCapabilityMismatchException exception = Assert.Throws<DeviceCapabilityMismatchException>(
                () => accessor.GetRequired<IRobotDevice>("robot"));

            Assert.Equal("robot", exception.SlotId);
            Assert.Equal(typeof(IRobotDevice), exception.RequestedCapabilityType);
        }

        [Fact]
        public void DeviceCreateContextSnapshotsConnectionSettings()
        {
            Dictionary<string, WorkflowValue> settings = new Dictionary<string, WorkflowValue>();
            settings.Add("port", WorkflowValue.FromInteger(502));
            DeviceCreateContext context = new DeviceCreateContext(
                "device-a",
                "Device A",
                "com.example/device",
                settings);

            settings["port"] = WorkflowValue.FromInteger(503);

            Assert.Equal("device-a", context.InstanceId);
            Assert.Equal("Device A", context.DisplayName);
            Assert.Equal("com.example/device", context.DeviceTypeId);
            Assert.Equal(502, context.ConnectionSettings["port"].IntegerValue);
            Assert.Null(typeof(DeviceCreateContext).GetProperty("ServiceProvider"));
        }

        [Fact]
        public void DeviceTypeDescriptorDeclaresSecretConfigurationField()
        {
            DeviceConfigurationFieldSchema secretField = new DeviceConfigurationFieldSchema
            {
                Id = "token",
                Name = "Token",
                DisplayName = "Access token",
                ValueType = WorkflowValueType.String,
                Required = true,
                Secret = true
            };
            DeviceTypeDescriptor descriptor = new DeviceTypeDescriptor
            {
                DeviceTypeId = "com.example/device",
                DisplayName = "Example device",
                ConfigurationSchema = new DeviceConfigurationSchema
                {
                    Fields = new List<DeviceConfigurationFieldSchema> { secretField }
                }
            };

            DeviceConfigurationFieldSchema field = Assert.Single(descriptor.ConfigurationSchema.Fields);

            Assert.True(field.Secret);
            Assert.Equal(WorkflowValueType.String, field.ValueType);
            Assert.True(field.Required);
        }

        [Fact]
        public void DeviceReadinessDistinguishesStandardStates()
        {
            Assert.Equal(DeviceReadinessState.Ready, DeviceReadiness.Ready.State);
            Assert.Equal(DeviceReadinessState.NotReady, DeviceReadiness.NotReady.State);
            Assert.Equal(DeviceReadinessState.Faulted, DeviceReadiness.Faulted("fault").State);
            Assert.Equal(DeviceReadinessState.Unknown, new DeviceReadiness(DeviceReadinessState.Unknown).State);
        }

        [Fact]
        public void PluginManifestRoundTripsSampleJson()
        {
            const string json = "{"
                + "\"manifestVersion\":1,"
                + "\"pluginId\":\"com.vendor.robot\","
                + "\"displayName\":\"Vendor Robot\","
                + "\"version\":\"1.2.0\","
                + "\"sdkVersion\":\"1.0\","
                + "\"entryAssembly\":\"lib/net10.0/Vendor.Robot.Plugin.dll\","
                + "\"entryType\":\"Vendor.Robot.RobotPlugin\","
                + "\"supportedRuntimes\":[\"win-x64\"],"
                + "\"capabilities\":[\"device\",\"workflow-nodes\"]"
                + "}";

            PluginManifest manifest = PluginManifestJson.Deserialize(json);
            string serialized = PluginManifestJson.Serialize(manifest);
            PluginManifest roundTripped = PluginManifestJson.Deserialize(serialized);

            Assert.Equal(1, roundTripped.ManifestVersion);
            Assert.Equal("com.vendor.robot", roundTripped.PluginId);
            Assert.Equal("lib/net10.0/Vendor.Robot.Plugin.dll", roundTripped.EntryAssembly);
            Assert.Equal(new[] { "win-x64" }, roundTripped.SupportedRuntimes);
            Assert.Equal(new[] { "device", "workflow-nodes" }, roundTripped.Capabilities);
        }

        [Fact]
        public void PluginManifestPreservesUnknownCapabilityStrings()
        {
            PluginManifest manifest = PluginManifestJson.Deserialize(
                "{\"manifestVersion\":1,\"pluginId\":\"plugin\","
                + "\"capabilities\":[\"future-capability\"]}");

            Assert.Contains("future-capability", manifest.Capabilities);
            Assert.Contains("future-capability", PluginManifestJson.Serialize(manifest));
        }

        [Fact]
        public void PluginAssemblyAvoidsForbiddenImplementationDependencies()
        {
            HashSet<string> forbidden = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "PowerLab.RuntimeHost",
                "PowerLab.PluginSystem",
                "PowerLab.Execution",
                "PowerLab.Devices",
                "PowerLab.Persistence",
                "Microsoft.EntityFrameworkCore",
                "Microsoft.AspNetCore.Core",
                "PresentationFramework",
                "Python.Runtime"
            };
            IEnumerable<string?> references = typeof(IPowerLabPlugin).Assembly
                .GetReferencedAssemblies()
                .Select(reference => reference.Name);

            Assert.DoesNotContain(references, reference => reference != null && forbidden.Contains(reference));
        }

        [Fact]
        public void LoggerFacadeAcceptsOnlyScalarStructuredProperties()
        {
            MethodInfo? log = typeof(IPluginLogger).GetMethod("Log");
            Assert.NotNull(log);
            ParameterInfo[] parameters = log!.GetParameters();

            Assert.Equal(typeof(PluginLogLevel), parameters[0].ParameterType);
            Assert.Equal(typeof(string), parameters[1].ParameterType);
            Assert.Equal(typeof(Exception), parameters[3].ParameterType);
            Assert.Contains("WorkflowValue", parameters[2].ParameterType.FullName);
        }

        private static NodeExecutionContext CreateContext(
            IReadOnlyDictionary<string, WorkflowValue> inputs)
        {
            return new NodeExecutionContext(
                "run-1",
                "node-1",
                "attempt-1",
                inputs,
                new FakeDeviceAccessor(),
                new ManualPauseToken(),
                new FakeLogger());
        }

        private sealed class MinimalPlugin : IPowerLabPlugin
        {
            public void Configure(IPluginBuilder builder)
            {
                builder.AddNodeProvider<FakeNodeProvider>();
                builder.AddDeviceProvider<FakeDeviceProvider>();
                builder.AddService<ISharedPluginService, SharedPluginService>();
            }
        }

        private interface ISharedPluginService
        {
        }

        private sealed class SharedPluginService : ISharedPluginService
        {
        }

        private sealed class RecordingPluginBuilder : IPluginBuilder
        {
            public List<Type> NodeProviderTypes { get; } = new List<Type>();
            public List<Type> DeviceProviderTypes { get; } = new List<Type>();
            public List<Type> ServiceTypes { get; } = new List<Type>();

            public void AddNodeProvider<TProvider>()
                where TProvider : class, INodeProvider
            {
                NodeProviderTypes.Add(typeof(TProvider));
            }

            public void AddDeviceProvider<TProvider>()
                where TProvider : class, IDeviceProvider
            {
                DeviceProviderTypes.Add(typeof(TProvider));
            }

            public void AddService<TService, TImplementation>()
                where TService : class
                where TImplementation : class, TService
            {
                ServiceTypes.Add(typeof(TService));
            }
        }

        private sealed class FakeNodeProvider : INodeProvider
        {
            public IReadOnlyList<NodeDescriptor> GetNodeDescriptors()
            {
                return new List<NodeDescriptor>
                {
                    new NodeDescriptor
                    {
                        DescriptorVersion = 1,
                        NodeType = new NodeTypeReference
                        {
                            PluginId = "com.example",
                            PluginVersion = "1.0.0",
                            NodeTypeId = "move",
                            NodeVersion = 2
                        },
                        DisplayName = "Move",
                        Category = "Motion",
                        PauseMode = PauseMode.Cooperative
                    }
                };
            }

            public INodeExecutor CreateExecutor(NodeExecutorCreateContext context)
            {
                if (context.NodeTypeId != "move" || context.NodeVersion != 2)
                {
                    throw new InvalidExecutorCreationRequestException("Unexpected node request.");
                }

                return new FakeExecutor();
            }
        }

        private sealed class FakeExecutor : INodeExecutor
        {
            public ValueTask<NodeExecutionResult> ExecuteAsync(
                NodeExecutionContext context,
                CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                return new ValueTask<NodeExecutionResult>(
                    NodeExecutionResult.Success(new Dictionary<string, WorkflowValue>()));
            }
        }

        private sealed class FakeDeviceProvider : IDeviceProvider
        {
            public IReadOnlyList<DeviceTypeDescriptor> GetDeviceTypes()
            {
                return new List<DeviceTypeDescriptor>
                {
                    new DeviceTypeDescriptor
                    {
                        DeviceTypeId = "com.example/device",
                        DisplayName = "Example device"
                    }
                };
            }

            public IDeviceDriver CreateDriver(DeviceCreateContext context)
            {
                return new FakeDriver();
            }
        }

        private sealed class FakeDriver : IDeviceDriver
        {
            public ValueTask ConnectAsync(CancellationToken cancellationToken)
            {
                return new ValueTask();
            }

            public ValueTask DisconnectAsync(CancellationToken cancellationToken)
            {
                return new ValueTask();
            }

            public ValueTask<DeviceReadiness> GetReadinessAsync(CancellationToken cancellationToken)
            {
                return new ValueTask<DeviceReadiness>(DeviceReadiness.Ready);
            }

            public ValueTask DisposeAsync()
            {
                return new ValueTask();
            }
        }

        private interface IRobotDevice
        {
        }

        private sealed class NonRobotDevice
        {
        }

        private sealed class FakeDeviceAccessor : INodeDeviceAccessor
        {
            private readonly Dictionary<string, object> _devices
                = new Dictionary<string, object>(StringComparer.Ordinal);

            public void Add(string slotId, object device)
            {
                _devices.Add(slotId, device);
            }

            public TDevice GetRequired<TDevice>(string slotId)
                where TDevice : class
            {
                object? device;
                if (!_devices.TryGetValue(slotId, out device))
                {
                    throw new DeviceSlotNotGrantedException(slotId);
                }

                TDevice? typedDevice = device as TDevice;
                if (typedDevice == null)
                {
                    throw new DeviceCapabilityMismatchException(slotId, typeof(TDevice));
                }

                return typedDevice;
            }

            public bool TryGet<TDevice>(string slotId, out TDevice? device)
                where TDevice : class
            {
                device = GetRequired<TDevice>(slotId);
                return true;
            }
        }

        private sealed class ManualPauseToken : IPauseToken
        {
            public bool IsPauseRequested { get; private set; }

            public void RequestPause()
            {
                IsPauseRequested = true;
            }

            public ValueTask WaitForResumeAsync(CancellationToken cancellationToken)
            {
                if (!IsPauseRequested)
                {
                    return new ValueTask();
                }

                return new ValueTask(Task.Delay(Timeout.Infinite, cancellationToken));
            }
        }

        private sealed class FakeLogger : IPluginLogger
        {
            public void Log(
                PluginLogLevel level,
                string message,
                IReadOnlyDictionary<string, WorkflowValue>? properties = null,
                Exception? exception = null)
            {
            }
        }
    }
}
