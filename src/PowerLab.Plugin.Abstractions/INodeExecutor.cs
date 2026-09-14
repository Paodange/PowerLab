using System.Threading;
using System.Threading.Tasks;

namespace PowerLab.Plugin.Abstractions
{
    /// <summary>
    /// Executes one node attempt using only the supplied controlled context.
    /// </summary>
    public interface INodeExecutor
    {
        /// <summary>
        /// Executes the node and returns either outputs or a structured plugin error.
        /// </summary>
        ValueTask<NodeExecutionResult> ExecuteAsync(
            NodeExecutionContext context,
            CancellationToken cancellationToken);
    }
}
