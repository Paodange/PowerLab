using System.Collections.Generic;

namespace PowerLab.Api.Contracts
{
    /// <summary>
    /// A cursor-based page whose cursor is opaque to API clients.
    /// </summary>
    public sealed class CursorPageDto<T>
    {
        public IReadOnlyList<T> Items { get; set; } = new List<T>();
        public string? NextCursor { get; set; }
    }
}
