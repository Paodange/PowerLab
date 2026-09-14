using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using PowerLab.Domain;

namespace PowerLab.Plugin.Abstractions
{
    /// <summary>
    /// Provides the read-only inputs and controlled capabilities for one node attempt.
    /// </summary>
    public sealed class NodeExecutionContext
    {
        private readonly IReadOnlyDictionary<string, WorkflowValue> _inputs;
        private readonly IReadOnlyDictionary<string, string> _metadata;

        /// <summary>
        /// Creates a node execution context from already evaluated inputs.
        /// </summary>
        public NodeExecutionContext(
            string runId,
            string nodeId,
            string attemptId,
            IReadOnlyDictionary<string, WorkflowValue> inputs,
            INodeDeviceAccessor devices,
            IPauseToken pauseToken,
            IPluginLogger logger,
            IReadOnlyDictionary<string, string>? metadata = null)
        {
            RunId = RequireIdentifier(runId, nameof(runId));
            NodeId = RequireIdentifier(nodeId, nameof(nodeId));
            AttemptId = RequireIdentifier(attemptId, nameof(attemptId));
            _inputs = CopyInputs(inputs, nameof(inputs));
            Devices = devices ?? throw new ArgumentNullException(nameof(devices));
            PauseToken = pauseToken ?? throw new ArgumentNullException(nameof(pauseToken));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _metadata = CopyMetadata(metadata, nameof(metadata));
        }

        /// <summary>
        /// Gets the run identifier.
        /// </summary>
        public string RunId { get; }

        /// <summary>
        /// Gets the workflow node identifier.
        /// </summary>
        public string NodeId { get; }

        /// <summary>
        /// Gets the execution attempt identifier.
        /// </summary>
        public string AttemptId { get; }

        /// <summary>
        /// Gets a defensive, read-only snapshot of evaluated node inputs.
        /// </summary>
        public IReadOnlyDictionary<string, WorkflowValue> Inputs
        {
            get { return _inputs; }
        }

        /// <summary>
        /// Gets the device capabilities granted for declared node slots.
        /// </summary>
        public INodeDeviceAccessor Devices { get; }

        /// <summary>
        /// Gets the cooperative pause signal for this node attempt.
        /// </summary>
        public IPauseToken PauseToken { get; }

        /// <summary>
        /// Gets the controlled plugin logging facade.
        /// </summary>
        public IPluginLogger Logger { get; }

        /// <summary>
        /// Gets defensive, read-only execution metadata.
        /// </summary>
        public IReadOnlyDictionary<string, string> Metadata
        {
            get { return _metadata; }
        }

        private static string RequireIdentifier(string value, string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            if (value.Trim().Length == 0)
            {
                throw new ArgumentException("The identifier cannot be empty.", parameterName);
            }

            return value;
        }

        private static IReadOnlyDictionary<string, WorkflowValue> CopyInputs(
            IReadOnlyDictionary<string, WorkflowValue> values,
            string parameterName)
        {
            if (values == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            Dictionary<string, WorkflowValue> copy = new Dictionary<string, WorkflowValue>(StringComparer.Ordinal);
            foreach (KeyValuePair<string, WorkflowValue> pair in values)
            {
                if (pair.Key == null)
                {
                    throw new ArgumentException("The input collection cannot contain a null key.", parameterName);
                }

                if (pair.Value == null)
                {
                    throw new ArgumentException("The input collection cannot contain a null value.", parameterName);
                }

                copy.Add(pair.Key, pair.Value);
            }

            return new ReadOnlyDictionary<string, WorkflowValue>(copy);
        }

        private static IReadOnlyDictionary<string, string> CopyMetadata(
            IReadOnlyDictionary<string, string>? values,
            string parameterName)
        {
            if (values == null)
            {
                return new ReadOnlyDictionary<string, string>(
                    new Dictionary<string, string>(StringComparer.Ordinal));
            }

            Dictionary<string, string> copy = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (KeyValuePair<string, string> pair in values)
            {
                if (pair.Key == null)
                {
                    throw new ArgumentException("The metadata collection cannot contain a null key.", parameterName);
                }

                if (pair.Value == null)
                {
                    throw new ArgumentException("The metadata collection cannot contain a null value.", parameterName);
                }

                copy.Add(pair.Key, pair.Value);
            }

            return new ReadOnlyDictionary<string, string>(copy);
        }
    }
}
