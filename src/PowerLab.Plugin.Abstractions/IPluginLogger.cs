using System;
using System.Collections.Generic;
using System.Threading;
using PowerLab.Domain;

namespace PowerLab.Plugin.Abstractions
{
    /// <summary>
    /// Provides controlled, framework-independent plugin logging.
    /// </summary>
    public interface IPluginLogger
    {
        /// <summary>
        /// Writes a structured diagnostic entry through the host facade.
        /// </summary>
        void Log(
            PluginLogLevel level,
            string message,
            IReadOnlyDictionary<string, WorkflowValue>? properties = null,
            Exception? exception = null);
    }
}
