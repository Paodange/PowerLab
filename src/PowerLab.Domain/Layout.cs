namespace PowerLab.Domain
{
    /// <summary>
    /// Designer position for a workflow node.
    /// </summary>
    public sealed class NodeLayout
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    /// <summary>
    /// Designer positions for a scope's virtual entry and exit boundaries.
    /// </summary>
    public sealed class ScopeLayout
    {
        public LayoutPoint Entry { get; set; } = new LayoutPoint();
        public LayoutPoint Exit { get; set; } = new LayoutPoint();
    }

    /// <summary>
    /// A two-dimensional designer layout point.
    /// </summary>
    public sealed class LayoutPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
    }
}
