using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PowerLab.Domain
{
    /// <summary>
    /// Creates the standalone JSON configuration used by workflow contracts.
    /// </summary>
    public static class WorkflowJson
    {
        /// <summary>
        /// Creates serializer options for workflow and node descriptor JSON.
        /// </summary>
        public static JsonSerializerOptions CreateSerializerOptions()
        {
            return CreateSerializerOptions(false);
        }

        /// <summary>
        /// Creates serializer options and optionally enables indented output.
        /// </summary>
        public static JsonSerializerOptions CreateSerializerOptions(bool writeIndented)
        {
            JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.General)
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
                NumberHandling = JsonNumberHandling.Strict,
                WriteIndented = writeIndented
            };

            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, false));
            options.Converters.Add(new WorkflowValueJsonConverter());
            options.Converters.Add(new InputBindingJsonConverter());
            options.Converters.Add(new WorkflowNodeDefinitionJsonConverter());
            options.Converters.Add(new BindingKindSetJsonConverter());
            options.Converters.Add(new WorkflowDocumentJsonConverter());
            return options;
        }

        /// <summary>
        /// Serializes a contract value with the workflow JSON configuration.
        /// </summary>
        public static string Serialize<T>(T value, bool writeIndented = false)
        {
            return JsonSerializer.Serialize(value, CreateSerializerOptions(writeIndented));
        }

        /// <summary>
        /// Deserializes a contract value with the workflow JSON configuration.
        /// </summary>
        public static T Deserialize<T>(string json)
        {
            if (json == null)
            {
                throw new ArgumentNullException(nameof(json));
            }

            T result = JsonSerializer.Deserialize<T>(json, CreateSerializerOptions())!;
            if (result == null)
            {
                throw new JsonException("The JSON value cannot be null for the requested workflow contract.");
            }

            return result;
        }
    }

    internal sealed class WorkflowValueJsonConverter : JsonConverter<WorkflowValue>
    {
        public override bool HandleNull
        {
            get { return true; }
        }

        public override WorkflowValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (JsonDocument document = JsonDocument.ParseValue(ref reader))
            {
                JsonElement element = document.RootElement;
                switch (element.ValueKind)
                {
                    case JsonValueKind.Number:
                        return ReadNumber(element);
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                        return WorkflowValue.FromBoolean(element.GetBoolean());
                    case JsonValueKind.String:
                        string? stringValue = element.GetString();
                        if (stringValue == null)
                        {
                            throw new JsonException("Workflow string values cannot be null.");
                        }

                        return WorkflowValue.FromString(stringValue);
                    case JsonValueKind.Null:
                        throw new JsonException("Workflow values do not support null.");
                    default:
                        throw new JsonException("Workflow values must be JSON scalar values.");
                }
            }
        }

        public override void Write(Utf8JsonWriter writer, WorkflowValue value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            switch (value.ValueType)
            {
                case WorkflowValueType.Integer:
                    writer.WriteNumberValue(value.IntegerValue);
                    break;
                case WorkflowValueType.Number:
                    WriteNumber(writer, value.NumberValue);
                    break;
                case WorkflowValueType.Boolean:
                    writer.WriteBooleanValue(value.BooleanValue);
                    break;
                case WorkflowValueType.String:
                    writer.WriteStringValue(value.StringValue);
                    break;
                default:
                    throw new JsonException("Unsupported workflow value type.");
            }
        }

        private static WorkflowValue ReadNumber(JsonElement element)
        {
            string rawNumber = element.GetRawText();
            bool hasFractionOrExponent = rawNumber.IndexOf('.') >= 0
                || rawNumber.IndexOf('e') >= 0
                || rawNumber.IndexOf('E') >= 0;

            if (!hasFractionOrExponent)
            {
                int integer;
                if (!element.TryGetInt32(out integer))
                {
                    throw new JsonException("Integer workflow values must be within the Int32 range.");
                }

                return WorkflowValue.FromInteger(integer);
            }

            double number;
            if (!element.TryGetDouble(out number)
                || double.IsNaN(number)
                || double.IsInfinity(number))
            {
                throw new JsonException("Number workflow values must be finite IEEE 754 doubles.");
            }

            return WorkflowValue.FromNumber(number);
        }

        private static void WriteNumber(Utf8JsonWriter writer, double value)
        {
            string text = value.ToString("R", CultureInfo.InvariantCulture);
            if (text.IndexOf('.') < 0 && text.IndexOf('e') < 0 && text.IndexOf('E') < 0)
            {
                text += ".0";
            }

            writer.WriteRawValue(text, true);
        }
    }

    internal sealed class InputBindingJsonConverter : JsonConverter<InputBinding>
    {
        public override bool HandleNull
        {
            get { return true; }
        }

        public override InputBinding Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (JsonDocument document = JsonDocument.ParseValue(ref reader))
            {
                JsonElement element = document.RootElement;
                if (element.ValueKind != JsonValueKind.Object)
                {
                    throw new JsonException("An input binding must be a JSON object.");
                }

                string kind = ReadDiscriminator(element, "binding");
                Type bindingType;
                switch (kind)
                {
                    case "literal":
                        bindingType = typeof(LiteralBinding);
                        break;
                    case "variable":
                        bindingType = typeof(VariableBinding);
                        break;
                    case "expression":
                        bindingType = typeof(ExpressionBinding);
                        break;
                    default:
                        throw new JsonException("Unknown input binding kind '" + kind + "'.");
                }

                InputBinding binding = (InputBinding)JsonSerializer.Deserialize(
                    element.GetRawText(), bindingType, options)!;
                if (binding == null)
                {
                    throw new JsonException("The input binding JSON produced no binding.");
                }

                return binding;
            }
        }

        public override void Write(Utf8JsonWriter writer, InputBinding value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            Type bindingType = value.GetType();
            if (bindingType != typeof(LiteralBinding)
                && bindingType != typeof(VariableBinding)
                && bindingType != typeof(ExpressionBinding))
            {
                throw new JsonException("Unsupported input binding type '" + bindingType.FullName + "'.");
            }

            JsonSerializer.Serialize(writer, value, bindingType, options);
        }

        private static string ReadDiscriminator(JsonElement element, string objectName)
        {
            JsonElement discriminator;
            if (!element.TryGetProperty("kind", out discriminator))
            {
                throw new JsonException("The " + objectName + " discriminator 'kind' is required.");
            }

            if (discriminator.ValueKind != JsonValueKind.String)
            {
                throw new JsonException("The " + objectName + " discriminator 'kind' must be a string.");
            }

            string? value = discriminator.GetString();
            if (value == null || value.Length == 0)
            {
                throw new JsonException("The " + objectName + " discriminator 'kind' cannot be empty.");
            }

            return value;
        }
    }

    internal sealed class WorkflowNodeDefinitionJsonConverter : JsonConverter<WorkflowNodeDefinition>
    {
        public override bool HandleNull
        {
            get { return true; }
        }

        public override WorkflowNodeDefinition Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            using (JsonDocument document = JsonDocument.ParseValue(ref reader))
            {
                JsonElement element = document.RootElement;
                if (element.ValueKind != JsonValueKind.Object)
                {
                    throw new JsonException("A workflow node must be a JSON object.");
                }

                JsonElement discriminator;
                if (!element.TryGetProperty("kind", out discriminator))
                {
                    throw new JsonException("The workflow node discriminator 'kind' is required.");
                }

                if (discriminator.ValueKind != JsonValueKind.String)
                {
                    throw new JsonException("The workflow node discriminator 'kind' must be a string.");
                }

                string? kind = discriminator.GetString();
                if (kind == null || kind.Length == 0)
                {
                    throw new JsonException("The workflow node discriminator 'kind' cannot be empty.");
                }

                Type nodeType;
                switch (kind)
                {
                    case "action":
                        nodeType = typeof(ActionNode);
                        break;
                    case "if":
                        nodeType = typeof(IfNode);
                        break;
                    case "while":
                        nodeType = typeof(WhileNode);
                        break;
                    case "parallel":
                        nodeType = typeof(ParallelNode);
                        break;
                    case "break":
                        nodeType = typeof(BreakNode);
                        break;
                    case "continue":
                        nodeType = typeof(ContinueNode);
                        break;
                    case "setVariable":
                        nodeType = typeof(SetVariableNode);
                        break;
                    case "pythonScript":
                        nodeType = typeof(PythonScriptNode);
                        break;
                    default:
                        throw new JsonException("Unknown workflow node kind '" + kind + "'.");
                }

                WorkflowNodeDefinition node = (WorkflowNodeDefinition)JsonSerializer.Deserialize(
                    element.GetRawText(), nodeType, options)!;
                if (node == null)
                {
                    throw new JsonException("The workflow node JSON produced no node.");
                }

                return node;
            }
        }

        public override void Write(
            Utf8JsonWriter writer,
            WorkflowNodeDefinition value,
            JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            Type nodeType = value.GetType();
            if (!IsKnownNodeType(nodeType))
            {
                throw new JsonException("Unsupported workflow node type '" + nodeType.FullName + "'.");
            }

            JsonSerializer.Serialize(writer, value, nodeType, options);
        }

        private static bool IsKnownNodeType(Type nodeType)
        {
            return nodeType == typeof(ActionNode)
                || nodeType == typeof(IfNode)
                || nodeType == typeof(WhileNode)
                || nodeType == typeof(ParallelNode)
                || nodeType == typeof(BreakNode)
                || nodeType == typeof(ContinueNode)
                || nodeType == typeof(SetVariableNode)
                || nodeType == typeof(PythonScriptNode);
        }
    }

    internal sealed class BindingKindSetJsonConverter : JsonConverter<IReadOnlySet<BindingKind>>
    {
        public override IReadOnlySet<BindingKind> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            List<BindingKind> values = JsonSerializer.Deserialize<List<BindingKind>>(ref reader, options)!;
            if (values == null)
            {
                throw new JsonException("Allowed bindings must be a JSON array.");
            }

            return new HashSet<BindingKind>(values);
        }

        public override void Write(
            Utf8JsonWriter writer,
            IReadOnlySet<BindingKind> value,
            JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();
            foreach (BindingKind bindingKind in value)
            {
                JsonSerializer.Serialize(writer, bindingKind, options);
            }

            writer.WriteEndArray();
        }
    }

    internal sealed class WorkflowDocumentJsonConverter : JsonConverter<WorkflowDocument>
    {
        public override bool HandleNull
        {
            get { return true; }
        }

        public override WorkflowDocument Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            using (JsonDocument document = JsonDocument.ParseValue(ref reader))
            {
                JsonElement element = document.RootElement;
                if (element.ValueKind != JsonValueKind.Object)
                {
                    throw new JsonException("A workflow document must be a JSON object.");
                }

                ValidateSchemaVersion(element);
                JsonSerializerOptions optionsWithoutThisConverter = WithoutThisConverter(options);
                WorkflowDocument result = JsonSerializer.Deserialize<WorkflowDocument>(
                    element.GetRawText(), optionsWithoutThisConverter)!;
                if (result == null)
                {
                    throw new JsonException("The workflow document JSON produced no document.");
                }

                return result;
            }
        }

        public override void Write(
            Utf8JsonWriter writer,
            WorkflowDocument value,
            JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            JsonSerializer.Serialize(writer, value, WithoutThisConverter(options));
        }

        private static void ValidateSchemaVersion(JsonElement document)
        {
            JsonElement schemaVersionElement;
            if (!document.TryGetProperty("schemaVersion", out schemaVersionElement)
                || schemaVersionElement.ValueKind != JsonValueKind.String)
            {
                throw new JsonException("The workflow document schemaVersion is required and must be a string.");
            }

            string? schemaVersion = schemaVersionElement.GetString();
            if (schemaVersion == null)
            {
                throw new JsonException("The workflow document schemaVersion cannot be null.");
            }

            int separator = schemaVersion.IndexOf('.');
            int major;
            int minor;
            if (separator <= 0
                || separator == schemaVersion.Length - 1
                || schemaVersion.IndexOf('.', separator + 1) >= 0
                || !int.TryParse(schemaVersion.Substring(0, separator), NumberStyles.None, CultureInfo.InvariantCulture, out major)
                || !int.TryParse(schemaVersion.Substring(separator + 1), NumberStyles.None, CultureInfo.InvariantCulture, out minor))
            {
                throw new JsonException("The workflow document schemaVersion must use major.minor format.");
            }

            if (major != 1)
            {
                throw new JsonException("Unsupported workflow document schema major version " + major + ".");
            }
        }

        private static JsonSerializerOptions WithoutThisConverter(JsonSerializerOptions options)
        {
            JsonSerializerOptions clone = new JsonSerializerOptions(options);
            for (int index = clone.Converters.Count - 1; index >= 0; index--)
            {
                if (clone.Converters[index] is WorkflowDocumentJsonConverter)
                {
                    clone.Converters.RemoveAt(index);
                }
            }

            return clone;
        }
    }
}
