using System.Threading;
using System.Threading.Tasks;

namespace PowerLab.Plugin.Abstractions
{
    /// <summary>
    /// Exposes cooperative pause requests without deciding runtime state transitions.
    /// </summary>
    public interface IPauseToken
    {
        /// <summary>
        /// Gets a value indicating whether a pause has been requested.
        /// </summary>
        bool IsPauseRequested { get; }

        /// <summary>
        /// Waits asynchronously for a requested pause to be released.
        /// </summary>
        ValueTask WaitForResumeAsync(CancellationToken cancellationToken);
    }
}
