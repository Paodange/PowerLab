using System.Collections.Generic;

namespace PowerLab.Domain
{
    /// <summary>
    /// Declarative constraints for a parameter value.
    /// </summary>
    public sealed class ParameterConstraints
    {
        public double? Minimum { get; set; }
        public double? Maximum { get; set; }
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        public string? Pattern { get; set; }
        public IReadOnlyList<string> AllowedValues { get; set; }
            = new List<string>();
    }
}
