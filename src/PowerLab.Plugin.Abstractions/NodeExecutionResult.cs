using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using PowerLab.Domain;

namespace PowerLab.Plugin.Abstractions
{
    /// <summary>
    /// Represents the successful outputs or structured failure of one node attempt.
    /// </summary>
    public sealed class NodeExecutionResult
    {
        private NodeExecutionResult(
            bool isSuccess,
            IReadOnlyDictionary<string, WorkflowValue> outputs,
            PluginError? error)
        {
            if (isSuccess && error != null)
            {
                throw new ArgumentException("A successful result cannot contain an error.", nameof(error));
            }

            if (!isSuccess && error == null)
            {
                throw new ArgumentNullException(nameof(error));
            }

            IsSuccess = isSuccess;
            Outputs = outputs;
            Error = error;
        }

        /// <summary>
        /// Creates a successful result with the supplied output values.
        /// </summary>
        public static NodeExecutionResult Success(
            IReadOnlyDictionary<string, WorkflowValue> outputs)
        {
            return new NodeExecutionResult(true, CopyOutputs(outputs), null);
        }

        /// <summary>
        /// Creates a failed result with a structured plugin error.
        /// </summary>
        public static NodeExecutionResult Failure(PluginError error)
        {
            if (error == null)
            {
                throw new ArgumentNullException(nameof(error));
            }

            return new NodeExecutionResult(false, CreateEmptyOutputs(), error);
        }

        /// <summary>
        /// Gets a value indicating whether this result represents success.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Gets a value indicating whether this result represents failure.
        /// </summary>
        public bool IsFailure
        {
            get { return !IsSuccess; }
        }

        /// <summary>
        /// Gets the defensive, read-only output snapshot.
        /// </summary>
        public IReadOnlyDictionary<string, WorkflowValue> Outputs { get; }

        /// <summary>
        /// Gets the structured error, or null for a successful result.
        /// </summary>
        public PluginError? Error { get; }

        private static IReadOnlyDictionary<string, WorkflowValue> CopyOutputs(
            IReadOnlyDictionary<string, WorkflowValue> values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            Dictionary<string, WorkflowValue> copy = new Dictionary<string, WorkflowValue>(StringComparer.Ordinal);
            foreach (KeyValuePair<string, WorkflowValue> pair in values)
            {
                if (pair.Key == null)
                {
                    throw new ArgumentException("The output collection cannot contain a null key.", nameof(values));
                }

                if (pair.Value == null)
                {
                    throw new ArgumentException("The output collection cannot contain a null value.", nameof(values));
                }

                copy.Add(pair.Key, pair.Value);
            }

            return new ReadOnlyDictionary<string, WorkflowValue>(copy);
        }

        private static IReadOnlyDictionary<string, WorkflowValue> CreateEmptyOutputs()
        {
            return new ReadOnlyDictionary<string, WorkflowValue>(
                new Dictionary<string, WorkflowValue>(StringComparer.Ordinal));
        }
    }
}
