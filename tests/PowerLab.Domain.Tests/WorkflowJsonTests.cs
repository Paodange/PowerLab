using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Xunit;

namespace PowerLab.Domain.Tests
{
    public sealed class WorkflowJsonTests
    {
        private readonly JsonSerializerOptions _options = PowerLab.Domain.WorkflowJson.CreateSerializerOptions();

        [Fact]
        public void WorkflowExampleRoundTripsAllStructuredData()
        {
            string json = ReadExample("workflow_v1.example.json");

            PowerLab.Domain.WorkflowDocument document = JsonSerializer.Deserialize<PowerLab.Domain.WorkflowDocument>(json, _options);

            Assert.Equal("1.0", document.SchemaVersion);
            Assert.Equal("Temperature Control", document.Name);
            Assert.Single(document.Inputs);
            Assert.Equal(PowerLab.Domain.WorkflowValueType.Number, document.Inputs[0].ValueType);
            Assert.Equal(2, document.Variables.Count);
            Assert.Single(document.RootScope.Nodes);
            PowerLab.Domain.WhileNode loop = Assert.IsType<PowerLab.Domain.WhileNode>(document.RootScope.Nodes[0]);
            Assert.Equal("Variables.AttemptCount < 100", loop.Condition.Source);
            Assert.Equal(3, loop.BodyScope.Nodes.Count);
            Assert.IsType<PowerLab.Domain.ActionNode>(loop.BodyScope.Nodes[0]);
            Assert.IsType<PowerLab.Domain.IfNode>(loop.BodyScope.Nodes[1]);
            Assert.IsType<PowerLab.Domain.SetVariableNode>(loop.BodyScope.Nodes[2]);
            PowerLab.Domain.IfNode conditional = Assert.IsType<PowerLab.Domain.IfNode>(loop.BodyScope.Nodes[1]);
            Assert.IsType<PowerLab.Domain.BreakNode>(conditional.TrueScope.Nodes[0]);
            Assert.Empty(conditional.FalseScope.Nodes);
            Assert.Equal(120.0, document.RootScope.Layout.Entry.X);

            string roundTrippedJson = JsonSerializer.Serialize(document, _options);
            PowerLab.Domain.WorkflowDocument roundTripped = JsonSerializer.Deserialize<PowerLab.Domain.WorkflowDocument>(roundTrippedJson, _options);

            Assert.Equal(document.WorkflowId, roundTripped.WorkflowId);
            Assert.Equal(document.RootScope.Nodes.Count, roundTripped.RootScope.Nodes.Count);
            PowerLab.Domain.WhileNode roundTrippedLoop = Assert.IsType<PowerLab.Domain.WhileNode>(roundTripped.RootScope.Nodes[0]);
            Assert.Equal(loop.Condition.Source, roundTrippedLoop.Condition.Source);
            Assert.Equal(loop.BodyScope.Edges.Count, roundTrippedLoop.BodyScope.Edges.Count);
            Assert.Equal(120.0, roundTripped.RootScope.Layout.Entry.X);
            Assert.Contains("\"layout\"", roundTrippedJson);
            Assert.DoesNotContain("executionState", roundTrippedJson);
        }

        [Fact]
        public void NodeDescriptorExampleRoundTripsSchemaFeatures()
        {
            string json = ReadExample("node_descriptor_v1.example.json");

            PowerLab.Domain.NodeDescriptor descriptor = JsonSerializer.Deserialize<PowerLab.Domain.NodeDescriptor>(json, _options);

            Assert.Equal(1, descriptor.DescriptorVersion);
            Assert.Equal(PowerLab.Domain.PauseMode.NodeBoundary, descriptor.PauseMode);
            Assert.Equal("pluginResource", descriptor.Icon.Kind);
            Assert.Equal(5, descriptor.Parameters.Count);
            PowerLab.Domain.ParameterSchema speed = descriptor.Parameters[4];
            Assert.Equal(PowerLab.Domain.WorkflowValueType.Number, speed.ValueType);
            Assert.Contains(PowerLab.Domain.BindingKind.Expression, speed.AllowedBindings);
            Assert.Equal(0.0, speed.Constraints.Minimum);
            Assert.Equal(100.0, speed.Constraints.Maximum);
            Assert.Equal("number", speed.Editor.Kind);
            Assert.Equal(1.0, speed.Editor.Step);
            Assert.Equal("Parameters.Mode == \"coordinate\"", descriptor.Parameters[1].ApplicableWhen.Source);
            Assert.Single(descriptor.Outputs);
            Assert.Single(descriptor.DeviceSlots);

            string roundTrippedJson = JsonSerializer.Serialize(descriptor, _options);
            PowerLab.Domain.NodeDescriptor roundTripped = JsonSerializer.Deserialize<PowerLab.Domain.NodeDescriptor>(roundTrippedJson, _options);

            Assert.Equal(descriptor.NodeType.NodeTypeId, roundTripped.NodeType.NodeTypeId);
            Assert.Equal(descriptor.Parameters.Count, roundTripped.Parameters.Count);
            Assert.Equal(speed.Constraints.AllowedValues.Count, roundTripped.Parameters[4].Constraints.AllowedValues.Count);
            Assert.Equal(speed.Editor.Step, roundTripped.Parameters[4].Editor.Step);
            Assert.Equal(descriptor.Outputs[0].Id, roundTripped.Outputs[0].Id);
            Assert.Equal(descriptor.DeviceSlots[0].RequiredDeviceTypeId, roundTripped.DeviceSlots[0].RequiredDeviceTypeId);
        }

        [Fact]
        public void EveryV1NodeKindUsesItsConcreteType()
        {
            string[] kinds = new string[]
            {
                "action", "if", "while", "parallel", "break", "continue", "setVariable", "pythonScript"
            };
            Type[] types = new Type[]
            {
                typeof(PowerLab.Domain.ActionNode),
                typeof(PowerLab.Domain.IfNode),
                typeof(PowerLab.Domain.WhileNode),
                typeof(PowerLab.Domain.ParallelNode),
                typeof(PowerLab.Domain.BreakNode),
                typeof(PowerLab.Domain.ContinueNode),
                typeof(PowerLab.Domain.SetVariableNode),
                typeof(PowerLab.Domain.PythonScriptNode)
            };

            for (int i = 0; i < kinds.Length; i++)
            {
                PowerLab.Domain.WorkflowNodeDefinition node = JsonSerializer.Deserialize<PowerLab.Domain.WorkflowNodeDefinition>(
                    "{\"kind\":\"" + kinds[i] + "\",\"id\":\"node-" + i + "\",\"displayName\":\"Node\"}",
                    _options);

                Assert.IsType(types[i], node);
            }
        }

        [Fact]
        public void BindingKindsUsePolymorphicConcreteTypes()
        {
            PowerLab.Domain.InputBinding literal = JsonSerializer.Deserialize<PowerLab.Domain.InputBinding>(
                "{\"kind\":\"literal\",\"value\":3}", _options);
            PowerLab.Domain.InputBinding variable = JsonSerializer.Deserialize<PowerLab.Domain.InputBinding>(
                "{\"kind\":\"variable\",\"variableId\":\"var-1\"}", _options);
            PowerLab.Domain.InputBinding expression = JsonSerializer.Deserialize<PowerLab.Domain.InputBinding>(
                "{\"kind\":\"expression\",\"language\":\"powerExpression\",\"languageVersion\":1,\"source\":\"1 + 2\"}", _options);

            Assert.IsType<PowerLab.Domain.LiteralBinding>(literal);
            Assert.Equal(PowerLab.Domain.WorkflowValueType.Integer, ((PowerLab.Domain.LiteralBinding)literal).Value.ValueType);
            Assert.IsType<PowerLab.Domain.VariableBinding>(variable);
            Assert.Equal("var-1", ((PowerLab.Domain.VariableBinding)variable).VariableId);
            Assert.IsType<PowerLab.Domain.ExpressionBinding>(expression);
            Assert.Equal("1 + 2", ((PowerLab.Domain.ExpressionBinding)expression).Source);

            string serialized = JsonSerializer.Serialize(expression, _options);
            Assert.Contains("\"kind\":\"expression\"", serialized);
        }

        [Fact]
        public void WorkflowValuesRoundTripAndRejectInvalidScalars()
        {
            Assert.Equal(12, JsonSerializer.Deserialize<PowerLab.Domain.WorkflowValue>("12", _options).IntegerValue);
            Assert.Equal(12.5, JsonSerializer.Deserialize<PowerLab.Domain.WorkflowValue>("12.5", _options).NumberValue);
            Assert.True(JsonSerializer.Deserialize<PowerLab.Domain.WorkflowValue>("true", _options).BooleanValue);
            Assert.Equal("hello", JsonSerializer.Deserialize<PowerLab.Domain.WorkflowValue>("\"hello\"", _options).StringValue);

            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<PowerLab.Domain.WorkflowValue>("2147483648", _options));
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<PowerLab.Domain.WorkflowValue>("{\"value\":12}", _options));
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<PowerLab.Domain.WorkflowValue>("null", _options));
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<PowerLab.Domain.WorkflowValue>("[]", _options));

            string serialized = JsonSerializer.Serialize(PowerLab.Domain.WorkflowValue.FromInteger(12), _options);
            Assert.Equal("12", serialized);
            Assert.Equal(PowerLab.Domain.WorkflowValueType.Number,
                JsonSerializer.Deserialize<PowerLab.Domain.WorkflowValue>(JsonSerializer.Serialize(
                    PowerLab.Domain.WorkflowValue.FromNumber(12.0), _options), _options).ValueType);
        }

        [Fact]
        public void UnknownDiscriminatorsAndMissingDiscriminatorsFailClearly()
        {
            JsonException unknownNode = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<PowerLab.Domain.WorkflowNodeDefinition>(
                "{\"kind\":\"futureNode\"}", _options));
            Assert.Contains("Unknown workflow node kind", unknownNode.Message);

            JsonException missingNode = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<PowerLab.Domain.WorkflowNodeDefinition>(
                "{\"id\":\"node-1\"}", _options));
            Assert.Contains("discriminator 'kind' is required", missingNode.Message);

            JsonException unknownBinding = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<PowerLab.Domain.InputBinding>(
                "{\"kind\":\"futureBinding\"}", _options));
            Assert.Contains("Unknown input binding kind", unknownBinding.Message);

            JsonException missingBinding = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<PowerLab.Domain.InputBinding>(
                "{\"variableId\":\"var-1\"}", _options));
            Assert.Contains("discriminator 'kind' is required", missingBinding.Message);
        }

        [Fact]
        public void UnknownOrdinaryFieldsAreIgnoredAndEnumsUseContractText()
        {
            PowerLab.Domain.BreakNode node = Assert.IsType<PowerLab.Domain.BreakNode>(JsonSerializer.Deserialize<PowerLab.Domain.WorkflowNodeDefinition>(
                "{\"kind\":\"break\",\"id\":\"node-1\",\"futureField\":true}", _options));
            Assert.Equal("node-1", node.Id);

            PowerLab.Domain.NodeDescriptor descriptor = new PowerLab.Domain.NodeDescriptor
            {
                PauseMode = PowerLab.Domain.PauseMode.NodeBoundary,
                NodeType = new PowerLab.Domain.NodeTypeReference()
            };
            string json = JsonSerializer.Serialize(descriptor, _options);
            Assert.Contains("\"pauseMode\":\"nodeBoundary\"", json);
            Assert.Equal("\"number\"", JsonSerializer.Serialize(
                PowerLab.Domain.WorkflowValueType.Number, _options));
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<PowerLab.Domain.PauseMode>("1", _options));
        }

        [Fact]
        public void WorkflowSchemaRejectsUnknownMajorAndAllowsMinorVersions()
        {
            PowerLab.Domain.WorkflowDocument minorDocument = JsonSerializer.Deserialize<PowerLab.Domain.WorkflowDocument>(
                "{\"schemaVersion\":\"1.1\",\"workflowId\":\"workflow-1\",\"name\":\"Demo\",\"futureField\":true}",
                _options);
            Assert.Equal("1.1", minorDocument.SchemaVersion);

            JsonException exception = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<PowerLab.Domain.WorkflowDocument>(
                "{\"schemaVersion\":\"2.0\",\"workflowId\":\"workflow-1\",\"name\":\"Demo\"}",
                _options));
            Assert.Contains("Unsupported workflow document schema major version", exception.Message);
        }

        [Fact]
        public void PythonScriptNodeRoundTripsSourceEntrypointInputsAndOutputs()
        {
            const string json = "{\"kind\":\"pythonScript\",\"id\":\"python-1\",\"displayName\":\"Calculate\",\"script\":{\"language\":\"python\",\"source\":\"def main(context, inputs):\\n    return inputs['value']\",\"entryPoint\":\"main\"},\"inputs\":[{\"id\":\"value\",\"name\":\"value\",\"valueType\":\"number\",\"binding\":{\"kind\":\"variable\",\"variableId\":\"var-source\"}}],\"outputs\":[{\"id\":\"result\",\"name\":\"result\",\"valueType\":\"number\",\"variableId\":\"var-result\"}]}";

            PowerLab.Domain.PythonScriptNode node = Assert.IsType<PowerLab.Domain.PythonScriptNode>(
                JsonSerializer.Deserialize<PowerLab.Domain.WorkflowNodeDefinition>(json, _options));

            Assert.Equal("python", node.Script.Language);
            Assert.Contains("return inputs", node.Script.Source);
            Assert.Equal("main", node.Script.EntryPoint);
            Assert.Single(node.Inputs);
            Assert.IsType<PowerLab.Domain.VariableBinding>(node.Inputs[0].Binding);
            Assert.Single(node.Outputs);
            Assert.Equal("var-result", node.Outputs[0].VariableId);

            PowerLab.Domain.PythonScriptNode roundTripped = Assert.IsType<PowerLab.Domain.PythonScriptNode>(
                JsonSerializer.Deserialize<PowerLab.Domain.WorkflowNodeDefinition>(JsonSerializer.Serialize(node, _options), _options));
            Assert.Equal(node.Script.Source, roundTripped.Script.Source);
            Assert.Equal(node.Inputs[0].Name, roundTripped.Inputs[0].Name);
            Assert.Equal(node.Outputs[0].VariableId, roundTripped.Outputs[0].VariableId);
        }

        [Fact]
        public void ValidationIssueAndReleaseAreSerializablePureData()
        {
            PowerLab.Domain.ValidationIssue issue = new PowerLab.Domain.ValidationIssue
            {
                Severity = PowerLab.Domain.ValidationSeverity.Error,
                Code = "expression.typeMismatch",
                Message = "Expression result is number but parameter expects integer.",
                Location = new PowerLab.Domain.ValidationIssueLocation
                {
                    JsonPointer = "/rootScope/nodes/0/parameters/sampleCount",
                    ScopeId = "scope-root",
                    NodeId = "node-read-temperature",
                    ParameterId = "sampleCount"
                }
            };

            PowerLab.Domain.ValidationIssue roundTripped = JsonSerializer.Deserialize<PowerLab.Domain.ValidationIssue>(
                JsonSerializer.Serialize(issue, _options), _options);
            Assert.Equal(PowerLab.Domain.ValidationSeverity.Error, roundTripped.Severity);
            Assert.Equal(issue.Code, roundTripped.Code);
            Assert.Equal(issue.Location.NodeId, roundTripped.Location.NodeId);

            PowerLab.Domain.WorkflowRelease release = new PowerLab.Domain.WorkflowRelease
            {
                ReleaseId = "release-1",
                WorkflowId = "workflow-1",
                ReleaseNumber = 1,
                PublishedAt = new DateTimeOffset(2026, 9, 14, 8, 30, 0, TimeSpan.Zero),
                Document = new PowerLab.Domain.WorkflowDocument { SchemaVersion = "1.0", WorkflowId = "workflow-1", Name = "Demo" },
                ExecutionHash = "sha256:example",
                RequiredPlugins = new List<PowerLab.Domain.RequiredPluginReference>
                {
                    new PowerLab.Domain.RequiredPluginReference { PluginId = "com.vendor.robot", PluginVersion = "1.2.0" }
                },
                PythonScriptHashes = new List<PowerLab.Domain.PythonScriptHash>
                {
                    new PowerLab.Domain.PythonScriptHash { NodeId = "python-1", Hash = "sha256:script" }
                }
            };
            string releaseJson = JsonSerializer.Serialize(release, _options);
            PowerLab.Domain.WorkflowRelease releaseRoundTrip = JsonSerializer.Deserialize<PowerLab.Domain.WorkflowRelease>(releaseJson, _options);
            Assert.Equal(release.ExecutionHash, releaseRoundTrip.ExecutionHash);
            Assert.Single(releaseRoundTrip.RequiredPlugins);
            Assert.Single(releaseRoundTrip.PythonScriptHashes);
        }

        private static string ReadExample(string fileName)
        {
            DirectoryInfo directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                string candidate = Path.Combine(directory.FullName, "docs", "examples", fileName);
                if (File.Exists(candidate))
                {
                    return File.ReadAllText(candidate);
                }

                directory = directory.Parent;
            }

            throw new FileNotFoundException("Could not locate the repository example file.", fileName);
        }
    }
}
