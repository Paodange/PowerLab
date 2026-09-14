using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using PowerLab.Domain;

namespace PowerLab.Api.Contracts
{
    internal sealed class WorkflowValueDtoJsonConverter : JsonConverter<WorkflowValueDto>
    {
        public override bool HandleNull
        {
            get { return true; }
        }

        public override WorkflowValueDto Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            using (JsonDocument document = JsonDocument.ParseValue(ref reader))
            {
                JsonElement element = document.RootElement;
                switch (element.ValueKind)
                {
                    case JsonValueKind.Number:
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

                            return WorkflowValueDto.FromInteger(integer);
                        }

                        double number;
                        if (!element.TryGetDouble(out number)
                            || double.IsNaN(number)
                            || double.IsInfinity(number))
                        {
                            throw new JsonException("Workflow values must be finite numbers.");
                        }

                        return WorkflowValueDto.FromNumber(number);
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                        return WorkflowValueDto.FromBoolean(element.GetBoolean());
                    case JsonValueKind.String:
                        string? text = element.GetString();
                        if (text == null)
                        {
                            throw new JsonException("Workflow string values cannot be null.");
                        }

                        return WorkflowValueDto.FromString(text);
                    default:
                        throw new JsonException("Workflow values must be integer, number, boolean, or string JSON scalars.");
                }
            }
        }

        public override void Write(
            Utf8JsonWriter writer,
            WorkflowValueDto value,
            JsonSerializerOptions options)
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
                    if (double.IsNaN(value.NumberValue) || double.IsInfinity(value.NumberValue))
                    {
                        throw new JsonException("Workflow numbers must be finite.");
                    }

                    string numberText = value.NumberValue.ToString("R", CultureInfo.InvariantCulture);
                    if (numberText.IndexOf('.') < 0
                        && numberText.IndexOf('e') < 0
                        && numberText.IndexOf('E') < 0)
                    {
                        numberText += ".0";
                    }

                    writer.WriteRawValue(numberText, true);
                    break;
                case WorkflowValueType.Boolean:
                    writer.WriteBooleanValue(value.BooleanValue);
                    break;
                case WorkflowValueType.String:
                    if (value.StringValue == null)
                    {
                        throw new JsonException("Workflow string values cannot be null.");
                    }

                    writer.WriteStringValue(value.StringValue);
                    break;
                default:
                    throw new JsonException("Unsupported workflow value type.");
            }
        }
    }
}
