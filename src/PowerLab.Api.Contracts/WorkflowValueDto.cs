using System;
using PowerLab.Domain;

namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// A four-type scalar value used by run inputs, variables, outputs, and logs.
    /// </summary>
    public sealed class WorkflowValueDto
    {
        public WorkflowValueType ValueType { get; set; }
        public int IntegerValue { get; set; }
        public double NumberValue { get; set; }
        public bool BooleanValue { get; set; }
        public string? StringValue { get; set; }

        public static WorkflowValueDto FromInteger(int value)
        {
            return new WorkflowValueDto { ValueType = WorkflowValueType.Integer, IntegerValue = value };
        }

        public static WorkflowValueDto FromNumber(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Workflow numbers must be finite.");
            }

            return new WorkflowValueDto { ValueType = WorkflowValueType.Number, NumberValue = value };
        }

        public static WorkflowValueDto FromBoolean(bool value)
        {
            return new WorkflowValueDto { ValueType = WorkflowValueType.Boolean, BooleanValue = value };
        }

        public static WorkflowValueDto FromString(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return new WorkflowValueDto { ValueType = WorkflowValueType.String, StringValue = value };
        }

        public static WorkflowValueDto FromDomain(WorkflowValue value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            switch (value.ValueType)
            {
                case WorkflowValueType.Integer:
                    return FromInteger(value.IntegerValue);
                case WorkflowValueType.Number:
                    return FromNumber(value.NumberValue);
                case WorkflowValueType.Boolean:
                    return FromBoolean(value.BooleanValue);
                case WorkflowValueType.String:
                    return FromString(value.StringValue);
                default:
                    throw new ArgumentException("Unsupported workflow value type.", nameof(value));
            }
        }

        public WorkflowValue ToDomain()
        {
            switch (ValueType)
            {
                case WorkflowValueType.Integer:
                    return WorkflowValue.FromInteger(IntegerValue);
                case WorkflowValueType.Number:
                    return WorkflowValue.FromNumber(NumberValue);
                case WorkflowValueType.Boolean:
                    return WorkflowValue.FromBoolean(BooleanValue);
                case WorkflowValueType.String:
                    if (StringValue == null)
                    {
                        throw new InvalidOperationException("Workflow string values cannot be null.");
                    }

                    return WorkflowValue.FromString(StringValue);
                default:
                    throw new InvalidOperationException("Unsupported workflow value type.");
            }
        }
    }
}
