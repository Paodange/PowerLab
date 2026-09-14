using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using PowerLab.Domain;

namespace PowerLab.Plugin.Abstractions
{
    /// <summary>
    /// Reports standard device readiness and safe scalar diagnostics.
    /// </summary>
    public sealed class DeviceReadiness
    {
        private readonly IReadOnlyDictionary<string, WorkflowValue> _details;

        /// <summary>
        /// Creates a readiness report.
        /// </summary>
        public DeviceReadiness(
            DeviceReadinessState state,
            string? message = null,
            IReadOnlyDictionary<string, WorkflowValue>? details = null)
        {
            State = state;
            if (message != null && message.Trim().Length == 0)
            {
                throw new ArgumentException("Message cannot be empty when supplied.", nameof(message));
            }

            Message = message;
            _details = CopyDetails(details);
        }

        /// <summary>
        /// Gets the standard readiness state.
        /// </summary>
        public DeviceReadinessState State { get; }

        /// <summary>
        /// Gets optional safe diagnostic text.
        /// </summary>
        public string? Message { get; }

        /// <summary>
        /// Gets safe scalar diagnostics associated with the readiness state.
        /// </summary>
        public IReadOnlyDictionary<string, WorkflowValue> Details
        {
            get { return _details; }
        }

        /// <summary>
        /// Creates a ready report.
        /// </summary>
        public static DeviceReadiness Ready
        {
            get { return new DeviceReadiness(DeviceReadinessState.Ready); }
        }

        /// <summary>
        /// Creates a not-ready report.
        /// </summary>
        public static DeviceReadiness NotReady
        {
            get { return new DeviceReadiness(DeviceReadinessState.NotReady); }
        }

        /// <summary>
        /// Creates a faulted report with a diagnostic message.
        /// </summary>
        public static DeviceReadiness Faulted(string message)
        {
            return new DeviceReadiness(DeviceReadinessState.Faulted, message);
        }

        private static IReadOnlyDictionary<string, WorkflowValue> CopyDetails(
            IReadOnlyDictionary<string, WorkflowValue>? details)
        {
            Dictionary<string, WorkflowValue> copy = new Dictionary<string, WorkflowValue>(StringComparer.Ordinal);
            if (details != null)
            {
                foreach (KeyValuePair<string, WorkflowValue> pair in details)
                {
                    if (pair.Key == null)
                    {
                        throw new ArgumentException("The details collection cannot contain a null key.", nameof(details));
                    }

                    if (pair.Value == null)
                    {
                        throw new ArgumentException("The details collection cannot contain a null value.", nameof(details));
                    }

                    copy.Add(pair.Key, pair.Value);
                }
            }

            return new ReadOnlyDictionary<string, WorkflowValue>(copy);
        }
    }
}
