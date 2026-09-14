using System;

namespace PowerLab.Domain
{
    /// <summary>
    /// A strongly typed scalar value used by workflow contracts.
    /// </summary>
    public sealed class WorkflowValue
    {
        private readonly object _value;

        /// <summary>
        /// Creates a workflow value after checking that the CLR value matches the declared type.
        /// </summary>
        public WorkflowValue(WorkflowValueType valueType, object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (!IsValueOfType(valueType, value))
            {
                throw new ArgumentException("The CLR value does not match the workflow value type.", nameof(value));
            }

            ValueType = valueType;
            _value = value;
        }

        /// <summary>
        /// Gets the declared workflow value type.
        /// </summary>
        public WorkflowValueType ValueType { get; }

        /// <summary>
        /// Gets the boxed scalar value.
        /// </summary>
        public object Value
        {
            get { return _value; }
        }

        /// <summary>
        /// Gets the integer value.
        /// </summary>
        public int IntegerValue
        {
            get { return GetValue<int>(WorkflowValueType.Integer); }
        }

        /// <summary>
        /// Gets the number value.
        /// </summary>
        public double NumberValue
        {
            get { return GetValue<double>(WorkflowValueType.Number); }
        }

        /// <summary>
        /// Gets the boolean value.
        /// </summary>
        public bool BooleanValue
        {
            get { return GetValue<bool>(WorkflowValueType.Boolean); }
        }

        /// <summary>
        /// Gets the string value.
        /// </summary>
        public string StringValue
        {
            get { return GetValue<string>(WorkflowValueType.String); }
        }

        /// <summary>
        /// Creates an integer workflow value.
        /// </summary>
        public static WorkflowValue FromInteger(int value)
        {
            return new WorkflowValue(WorkflowValueType.Integer, value);
        }

        /// <summary>
        /// Creates a number workflow value.
        /// </summary>
        public static WorkflowValue FromNumber(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Workflow numbers must be finite.");
            }

            return new WorkflowValue(WorkflowValueType.Number, value);
        }

        /// <summary>
        /// Creates a boolean workflow value.
        /// </summary>
        public static WorkflowValue FromBoolean(bool value)
        {
            return new WorkflowValue(WorkflowValueType.Boolean, value);
        }

        /// <summary>
        /// Creates a non-null string workflow value.
        /// </summary>
        public static WorkflowValue FromString(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return new WorkflowValue(WorkflowValueType.String, value);
        }

        /// <summary>
        /// Creates an integer workflow value. This alias is kept for SDK ergonomics.
        /// </summary>
        public static WorkflowValue FromInt(int value)
        {
            return FromInteger(value);
        }

        private static bool IsValueOfType(WorkflowValueType valueType, object value)
        {
            switch (valueType)
            {
                case WorkflowValueType.Integer:
                    return value is int;
                case WorkflowValueType.Number:
                    return value is double && !double.IsNaN((double)value) && !double.IsInfinity((double)value);
                case WorkflowValueType.Boolean:
                    return value is bool;
                case WorkflowValueType.String:
                    return value is string;
                default:
                    return false;
            }
        }

        private T GetValue<T>(WorkflowValueType expectedType)
        {
            if (ValueType != expectedType)
            {
                throw new InvalidOperationException("The workflow value has a different value type.");
            }

            return (T)_value;
        }
    }
}
