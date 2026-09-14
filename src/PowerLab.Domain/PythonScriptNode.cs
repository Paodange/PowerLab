using System.Collections.Generic;

namespace PowerLab.Domain
{
    /// <summary>
    /// A workflow node that executes a persisted Python script as one unit.
    /// </summary>
    public sealed class PythonScriptNode : WorkflowNodeDefinition
    {
        public PythonScriptNode()
            : base("pythonScript")
        {
        }

        public PythonScriptDefinition Script { get; set; } = new PythonScriptDefinition();
        public IReadOnlyList<PythonInputDefinition> Inputs { get; set; }
            = new List<PythonInputDefinition>();
        public IReadOnlyList<PythonOutputDefinition> Outputs { get; set; }
            = new List<PythonOutputDefinition>();
    }

    /// <summary>
    /// The source and entry point of a Python script node.
    /// </summary>
    public class PythonScriptDefinition
    {
        public string Language { get; set; } = "python";
        public string Source { get; set; } = string.Empty;
        public string EntryPoint { get; set; } = "main";
    }

    /// <summary>
    /// Declares one named input to a Python script node.
    /// </summary>
    public class PythonInputDefinition
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public WorkflowValueType ValueType { get; set; }
        public InputBinding Binding { get; set; } = new LiteralBinding();
    }

    /// <summary>
    /// Declares one named output from a Python script node.
    /// </summary>
    public class PythonOutputDefinition
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public WorkflowValueType ValueType { get; set; }
        public string VariableId { get; set; } = string.Empty;
    }
}
