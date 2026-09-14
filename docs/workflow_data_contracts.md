# PowerLab V1 Workflow 与节点描述数据契约

> 状态：V1 契约草案  
> 本文定义可持久化 Workflow JSON、NodeDescriptor、ParameterSchema 和相关值对象。示例中的 C# 类型用于表达契约语义，不要求最终代码逐字一致。

## 1. 契约原则

- Workflow JSON 是可导入、导出和长期保存的公开文档格式；
- 数据库实体、REST DTO、领域对象和插件运行对象不得直接复用；
- 所有持久化多态对象都使用显式 `kind` 判别字段；
- ID 是稳定身份，显示名称允许修改；
- 插件参数、输出和设备槽位通过稳定 ID 绑定，不通过显示名称绑定；
- UI 布局信息可以保存在 Workflow JSON 中，但不参与执行语义；
- 解析后的 AST、编译委托、运行对象和 CLR Type 不进入 JSON；
- 读取方必须拒绝不支持的主版本，允许忽略新增的非关键响应字段。

## 2. 通用 JSON 约定

| 项目 | 约定 |
|---|---|
| 编码 | UTF-8 |
| 属性名 | camelCase |
| 枚举 | 小写 camelCase 字符串 |
| ID | 不透明字符串；生产实现默认生成小写 UUID，文档示例可使用可读 ID |
| 时间 | UTC RFC 3339，例如 `2026-09-14T08:30:00Z` |
| 数值 | Integer 为 Int32；Number 为 IEEE 754 Double |
| 未知字段 | 文档读取时保留兼容；请求校验时报告不支持的关键字段 |
| 缺失字段 | 表示未提供，不等同于 `null` |
| `null` | V1 工作流值不支持 `null` |

`schemaVersion` 使用 `major.minor`：

- major 变化表示不兼容；
- minor 变化只能新增可忽略字段或兼容能力；
- V1 首个版本为 `1.0`。

## 3. WorkflowDocument

概念模型：

```csharp
public sealed class WorkflowDocument
{
    public string SchemaVersion { get; set; } = string.Empty;
    public string WorkflowId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public IReadOnlyList<WorkflowInputDefinition> Inputs { get; set; }
        = Array.Empty<WorkflowInputDefinition>();
    public IReadOnlyList<WorkflowVariableDefinition> Variables { get; set; }
        = Array.Empty<WorkflowVariableDefinition>();
    public WorkflowScope RootScope { get; set; } = new WorkflowScope();
}
```

顶层 JSON：

```json
{
  "schemaVersion": "1.0",
  "workflowId": "f5415728-5115-451c-9dd3-80a068fe5153",
  "name": "Temperature Control",
  "description": "Read temperature until the target is reached.",
  "inputs": [],
  "variables": [],
  "rootScope": {}
}
```

不在可移植 WorkflowDocument 中保存：

- 数据库主键和行版本；
- 创建者、创建时间和更新时间；
- Draft/Release 状态；
- 运行状态；
- 编译产物；
- API 权限数据。

这些字段属于 RuntimeHost 的资源 DTO 或数据库记录。

## 4. 名称与 ID

所有输入、变量、参数和输出同时具有稳定 ID 与可读名称：

```text
id          用于持久化引用，不随重命名变化
name        用于表达式和代码，必须是合法标识符
displayName 用于 UI，可本地化，可重复
```

V1 名称规则：

```regex
^[A-Za-z_][A-Za-z0-9_]*$
```

- 同一命名空间不允许名称重复；
- 不允许只通过大小写区分两个名称；
- 表达式解析区分大小写；
- 重命名由 Designer 根据表达式依赖 AST 执行安全重写。
- 插件 Parameter/Output/DeviceSlot 的 `name` 在同一 NodeVersion 内属于兼容契约，不能随意修改；日常文案修改只改 `displayName`。

## 5. ValueType 与 WorkflowValue

V1 值类型：

```csharp
public enum WorkflowValueType
{
    Integer,
    Number,
    Boolean,
    String
}
```

JSON 字面量直接使用相应 JSON 类型。期望类型来自变量定义、ParameterSchema 或 OutputSchema。

约束：

- Integer 必须在 Int32 范围内且不能带小数；
- Number 使用 Double，默认拒绝 `NaN`、正无穷和负无穷；
- Boolean 只接受 JSON `true`/`false`；
- String 不接受 `null`；
- V1 不进行字符串与数值之间的隐式转换。

## 6. Workflow Inputs

```csharp
public sealed class WorkflowInputDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public WorkflowValueType ValueType { get; set; }
    public bool Required { get; set; }
    public LiteralBinding? DefaultValue { get; set; }
    public string? Description { get; set; }
}
```

JSON：

```json
{
  "id": "989fc012-5d6b-483f-9e13-9a9264b00e0a",
  "name": "TargetTemperature",
  "displayName": "Target temperature",
  "valueType": "number",
  "required": true,
  "description": "The target temperature for this run."
}
```

规则：

- Input 在运行开始时绑定，运行期间只读；
- Required Input 必须由启动请求提供或具有 DefaultValue；
- Input 默认值只能是 LiteralBinding；
- Input 值在创建 Run 时完成类型检查。

## 7. Workflow Variables

```csharp
public sealed class WorkflowVariableDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public WorkflowValueType ValueType { get; set; }
    public InputBinding InitialValue { get; set; } = new LiteralBinding();
    public string? Description { get; set; }
}
```

JSON：

```json
{
  "id": "5d66f27f-a447-43a3-bfd8-cd0290d230bb",
  "name": "CurrentTemperature",
  "displayName": "Current temperature",
  "valueType": "number",
  "initialValue": {
    "kind": "literal",
    "value": 0.0
  }
}
```

变量初始化规则：

- InitialValue 可以是 Literal、Variable 或 Expression；
- 初始化表达式可以读取 Inputs 和其他 Variables；
- RuntimeHost 根据依赖图按拓扑顺序求值；
- 循环依赖、自引用和未知引用阻止发布；
- 所有变量初始化成功后才能进入 Workflow 第一个节点。

## 8. InputBinding

```text
InputBinding
├── LiteralBinding
├── VariableBinding
└── ExpressionBinding
```

### 8.1 LiteralBinding

```json
{
  "kind": "literal",
  "value": 50.0
}
```

### 8.2 VariableBinding

```json
{
  "kind": "variable",
  "variableId": "5d66f27f-a447-43a3-bfd8-cd0290d230bb"
}
```

VariableBinding 只引用 Workflow Variable。读取 Workflow Input 使用 ExpressionBinding，例如 `Inputs.TargetTemperature`。

### 8.3 ExpressionBinding

```json
{
  "kind": "expression",
  "language": "powerExpression",
  "languageVersion": 1,
  "source": "Variables.BaseSpeed * 0.8"
}
```

不序列化 Parsed AST、依赖缓存和编译结果。它们是可以从 Source 重建的 RuntimeHost 派生数据。

## 9. WorkflowScope

```csharp
public sealed class WorkflowScope
{
    public string Id { get; set; } = string.Empty;
    public string EntryNodeId { get; set; } = string.Empty;
    public string ExitNodeId { get; set; } = string.Empty;
    public IReadOnlyList<WorkflowNodeDefinition> Nodes { get; set; }
        = Array.Empty<WorkflowNodeDefinition>();
    public IReadOnlyList<ControlEdge> Edges { get; set; }
        = Array.Empty<ControlEdge>();
    public ScopeLayout? Layout { get; set; }
}
```

`entryNodeId` 和 `exitNodeId` 是 Scope 的虚拟边界 ID：

- 不出现在 `nodes` 数组中；
- 可以作为 ControlEdge 的端点；
- RootScope 中由 Designer 显示为 Start/End；
- 容器子 Scope 中默认隐藏；
- 空 If 分支通过 Entry 直接连接 Exit 表达。

```json
{
  "id": "scope-root",
  "entryNodeId": "boundary-root-entry",
  "exitNodeId": "boundary-root-exit",
  "nodes": [],
  "edges": [],
  "layout": {
    "entry": { "x": 120.0, "y": 40.0 },
    "exit": { "x": 120.0, "y": 600.0 }
  }
}
```

所有 Node ID 和 Scope 边界 ID 在一个 WorkflowDocument 内必须全局唯一。

## 10. ControlEdge

```csharp
public sealed class ControlEdge
{
    public string Id { get; set; } = string.Empty;
    public ControlEndpoint Source { get; set; } = new ControlEndpoint();
    public ControlEndpoint Target { get; set; } = new ControlEndpoint();
}

public sealed class ControlEndpoint
{
    public string NodeId { get; set; } = string.Empty;
    public string PortId { get; set; } = string.Empty;
}
```

JSON：

```json
{
  "id": "edge-1",
  "source": {
    "nodeId": "boundary-root-entry",
    "portId": "out"
  },
  "target": {
    "nodeId": "node-read-temperature",
    "portId": "in"
  }
}
```

V1 的控制端口固定为：

```text
Scope Entry : out
Scope Exit  : in
普通节点    : in / out
Break       : in
Continue    : in
```

数据不通过 Edge 传递。

## 11. WorkflowNodeDefinition

所有节点共有：

```csharp
public abstract class WorkflowNodeDefinition
{
    public string Kind { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public NodeLayout? Layout { get; set; }
}
```

`layout` 仅用于 Designer，不参与执行计划 Hash。

V1 `kind`：

```text
action
if
while
parallel
break
continue
setVariable
pythonScript
```

### 11.1 ActionNode

```json
{
  "kind": "action",
  "id": "node-read-temperature",
  "displayName": "Read temperature",
  "nodeType": {
    "pluginId": "com.vendor.thermometer",
    "pluginVersion": "1.2.0",
    "nodeTypeId": "read-temperature",
    "nodeVersion": 1
  },
  "parameters": {
    "sampleCount": {
      "kind": "literal",
      "value": 3
    }
  },
  "deviceBindings": {
    "thermometer": "device-thermometer-a"
  },
  "outputMappings": {
    "temperature": "5d66f27f-a447-43a3-bfd8-cd0290d230bb"
  },
  "layout": {
    "x": 120.0,
    "y": 160.0
  }
}
```

映射对象的 Key 都是 Descriptor 中的稳定 ID：

- `parameters` Key：ParameterSchema.id；
- `deviceBindings` Key：DeviceSlotSchema.id；
- `outputMappings` Key：OutputSchema.id；
- `outputMappings` Value：WorkflowVariableDefinition.id。

`pluginVersion` 记录创建或发布该节点时使用的插件版本。RuntimeHost 仍根据 NodeVersion 兼容规则决定能否由更新插件执行。

可选的 `descriptorSnapshot` 可以保存显示和 Schema 快照，供插件缺失时展示；它不具有执行权威性，发布校验必须使用当前已加载插件的 Descriptor。

### 11.2 IfNode

```json
{
  "kind": "if",
  "id": "node-target-reached",
  "displayName": "Target reached?",
  "condition": {
    "kind": "expression",
    "language": "powerExpression",
    "languageVersion": 1,
    "source": "Variables.CurrentTemperature >= Inputs.TargetTemperature"
  },
  "trueScope": {},
  "falseScope": {},
  "layout": { "x": 120.0, "y": 260.0 }
}
```

- Condition 必须求值为 Boolean；
- TrueScope 或 FalseScope 可以为空；
- 分支完成后从 IfNode 的 `out` 端口继续；
- Break/Continue 控制信号可以提前终止当前分支并向上寻找 While。

### 11.3 WhileNode

```json
{
  "kind": "while",
  "id": "node-monitor-loop",
  "displayName": "Monitor loop",
  "condition": {
    "kind": "expression",
    "language": "powerExpression",
    "languageVersion": 1,
    "source": "Variables.AttemptCount < 100"
  },
  "bodyScope": {},
  "layout": { "x": 120.0, "y": 120.0 }
}
```

- V1 不保存或检查 MaxIterations；
- 第一次进入循环体时 `Loop.Iteration` 为 `0`；
- 一次循环体正常完成或执行 Continue 后递增 Iteration，再重新判断 Condition；
- Continue 回到 Condition；
- Break 从 WhileNode 的 `out` 端口继续。

### 11.4 ParallelNode

```json
{
  "kind": "parallel",
  "id": "node-parallel-prepare",
  "displayName": "Prepare devices",
  "branches": [
    {
      "id": "branch-a",
      "displayName": "Device A",
      "scope": {}
    },
    {
      "id": "branch-b",
      "displayName": "Device B",
      "scope": {}
    }
  ],
  "layout": { "x": 120.0, "y": 360.0 }
}
```

- 至少两个非空 Branch；
- 所有 Branch 正常完成后从 ParallelNode 的 `out` 端口继续；
- Break/Continue 不能跨越 Parallel 边界；
- 分支共享 Workflow Variable Store；
- 输出映射提交后其他分支立即可见。

### 11.5 BreakNode / ContinueNode

```json
{
  "kind": "break",
  "id": "node-break",
  "displayName": "Break",
  "layout": { "x": 120.0, "y": 80.0 }
}
```

```json
{
  "kind": "continue",
  "id": "node-continue",
  "displayName": "Continue",
  "layout": { "x": 120.0, "y": 80.0 }
}
```

- 只有 `in` 端口；
- 不保存 TargetLoopId；
- 编译器从 Scope 层级解析最近一层 While；
- 目标路径跨越 Parallel 时阻止发布。

### 11.6 SetVariableNode

```json
{
  "kind": "setVariable",
  "id": "node-increment-attempt",
  "displayName": "Increment attempt",
  "assignments": [
    {
      "variableId": "var-attempt-count",
      "value": {
        "kind": "expression",
        "language": "powerExpression",
        "languageVersion": 1,
        "source": "Variables.AttemptCount + 1"
      }
    }
  ],
  "layout": { "x": 120.0, "y": 360.0 }
}
```

一个 SetVariableNode 可以包含多个 Assignment。所有右值基于写入前的同一变量快照求值，全部成功后原子提交。

### 11.7 PythonScriptNode

```json
{
  "kind": "pythonScript",
  "id": "node-python-calculate",
  "displayName": "Calculate result",
  "script": {
    "language": "python",
    "source": "def main(context, inputs):\n    return {'result': inputs['value'] * 2}\n",
    "entryPoint": "main"
  },
  "inputs": [
    {
      "id": "value",
      "name": "value",
      "valueType": "number",
      "binding": {
        "kind": "variable",
        "variableId": "var-source-value"
      }
    }
  ],
  "outputs": [
    {
      "id": "result",
      "name": "result",
      "valueType": "number",
      "variableId": "var-result"
    }
  ],
  "layout": { "x": 120.0, "y": 420.0 }
}
```

- Source 直接保存在 Workflow JSON；
- EntryPoint 必须是合法 Python 标识符；
- Inputs 和 Outputs 名称在节点内唯一；
- 输出必须是包含已声明 Key 的字典；
- 发布版本记录脚本内容 Hash；
- Python 节点整体作为一个执行和单步单位。

## 12. NodeTypeReference

```csharp
public sealed class NodeTypeReference
{
    public string PluginId { get; set; } = string.Empty;
    public string PluginVersion { get; set; } = string.Empty;
    public string NodeTypeId { get; set; } = string.Empty;
    public int NodeVersion { get; set; }
}
```

兼容规则：

- PluginId 和 NodeTypeId 必须精确匹配；
- 已加载插件必须声明支持该 NodeVersion；
- 相同 NodeVersion 必须保持 Parameter、Output 和 DeviceSlot 的执行兼容；
- PluginVersion 用于诊断和发布依赖，不替代 NodeVersion；
- 不兼容变化必须增加 NodeVersion；
- V1 不自动迁移旧 NodeVersion。

## 13. NodeDescriptor

NodeDescriptor 是 RuntimeHost 从插件读取后，通过 REST 提供给 Designer 的可序列化契约。

```csharp
public sealed class NodeDescriptor
{
    public int DescriptorVersion { get; set; }
    public NodeTypeReference NodeType { get; set; } = new NodeTypeReference();
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public IconReference? Icon { get; set; }
    public IReadOnlyList<ParameterSchema> Parameters { get; set; }
        = Array.Empty<ParameterSchema>();
    public IReadOnlyList<OutputSchema> Outputs { get; set; }
        = Array.Empty<OutputSchema>();
    public IReadOnlyList<DeviceSlotSchema> DeviceSlots { get; set; }
        = Array.Empty<DeviceSlotSchema>();
    public PauseMode PauseMode { get; set; }
}
```

JSON 外形：

```json
{
  "descriptorVersion": 1,
  "nodeType": {
    "pluginId": "com.vendor.robot",
    "pluginVersion": "1.2.0",
    "nodeTypeId": "move",
    "nodeVersion": 1
  },
  "displayName": "Move robot",
  "description": "Moves a robot to a configured target.",
  "category": "Robot/Motion",
  "icon": {
    "kind": "pluginResource",
    "resourcePath": "icons/move.svg",
    "contentHash": "sha256:..."
  },
  "parameters": [],
  "outputs": [],
  "deviceSlots": [],
  "pauseMode": "nodeBoundary"
}
```

Descriptor 不得包含 WPF 控件、委托、CLR Type 名称或运行对象。

## 14. ParameterSchema

```csharp
public sealed class ParameterSchema
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public WorkflowValueType ValueType { get; set; }
    public bool Required { get; set; }
    public LiteralBinding? DefaultValue { get; set; }
    public IReadOnlySet<BindingKind> AllowedBindings { get; set; }
        = new HashSet<BindingKind>();
    public ParameterConstraints? Constraints { get; set; }
    public ParameterEditorHint? Editor { get; set; }
    public SchemaCondition? ApplicableWhen { get; set; }
    public SchemaCondition? VisibleWhen { get; set; }
    public SchemaCondition? EnabledWhen { get; set; }
    public string? Group { get; set; }
    public int Order { get; set; }
}
```

完整 JSON 示例：

```json
{
  "id": "speed",
  "name": "Speed",
  "displayName": "Speed",
  "description": "Movement speed in percent.",
  "valueType": "number",
  "required": true,
  "defaultValue": {
    "kind": "literal",
    "value": 50.0
  },
  "allowedBindings": ["literal", "variable", "expression"],
  "constraints": {
    "minimum": 0.0,
    "maximum": 100.0
  },
  "editor": {
    "kind": "number",
    "step": 1.0
  },
  "group": "Motion",
  "order": 20
}
```

### 14.1 Constraints

V1 支持：

```text
Integer/Number : minimum, maximum
String         : minLength, maxLength, pattern, allowedValues
Boolean        : 无额外约束
```

Constraints 在三个位置执行：

- Designer 编辑时；
- Workflow 发布时；
- 节点派发前对求值结果执行。

插件执行器仍应防御性检查设备业务边界。

### 14.2 Editor Hint

```text
integer
number
text
multilineText
toggle
select
code
```

Editor 只是跨 UI 的显示提示，不影响执行语义。未知 Editor 必须回退到对应 ValueType 的默认编辑器。

## 15. 动态参数规则

动态参数区分三个概念：

| 规则 | 是否属于执行语义 | 用途 |
|---|---:|---|
| `applicableWhen` | 是 | 决定参数是否参与必填校验、求值和传入执行器 |
| `visibleWhen` | 否 | 决定 Property Editor 是否显示 |
| `enabledWhen` | 否 | 决定编辑器是否可编辑 |

SchemaCondition：

```json
{
  "language": "parameterCondition",
  "languageVersion": 1,
  "source": "Parameters.Mode == \"namedPosition\""
}
```

规则：

- 只能读取同一节点的 Parameters；
- 不能访问 Workflow Variables、Inputs、设备或 IO；
- 条件必须返回 Boolean；
- 条件依赖图不能成环；
- `visibleWhen` 缺失时默认跟随 `applicableWhen`；
- `enabledWhen` 缺失时默认启用；
- 参数不适用时不执行 Binding、不检查 Required、也不传入 Executor；
- 隐藏但仍适用的参数继续求值并传入 Executor；
- 隐藏参数的已保存 Binding 不自动删除。

如果条件依赖的参数使用 Variable/Expression Binding，Designer 可能无法得到确定结果。此时采用三值状态 `true/false/unknown`：

- `unknown` 时 Designer 显示该参数并标记“运行时决定”；
- RuntimeHost 在所有前置参数求值后得到最终 applicable 状态；
- 依赖一个不适用参数的条件属于 Schema 错误。

## 16. OutputSchema

```csharp
public sealed class OutputSchema
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public WorkflowValueType ValueType { get; set; }
    public bool Required { get; set; }
}
```

V1 规则：

- 输出 ID 在 NodeType 内稳定且唯一；
- 所有 Required 输出必须存在且类型正确；
- 输出先完整校验，再原子提交 OutputMapping；
- 未映射输出仍进入 NodeExecution 诊断记录，但不进入 Workflow Variables；
- V1 表达式不能直接引用 Output。

## 17. DeviceSlotSchema

```csharp
public sealed class DeviceSlotSchema
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string RequiredDeviceTypeId { get; set; } = string.Empty;
    public bool Required { get; set; }
    public string? Description { get; set; }
}
```

JSON：

```json
{
  "id": "robot",
  "name": "Robot",
  "displayName": "Robot",
  "requiredDeviceTypeId": "com.vendor.robot/robot-v1",
  "required": true
}
```

- DeviceBinding 绑定具体 DeviceInstance ID；
- V1 不允许 DeviceBinding 使用变量或表达式；
- 未声明设备不能由 Executor 访问；
- V1 不处理隐式设备依赖。

## 18. ValidationIssue

Workflow 校验结果使用统一位置模型：

```json
{
  "severity": "error",
  "code": "expression.typeMismatch",
  "message": "Expression result is number but parameter expects integer.",
  "location": {
    "jsonPointer": "/rootScope/nodes/0/parameters/sampleCount",
    "scopeId": "scope-root",
    "nodeId": "node-read-temperature",
    "parameterId": "sampleCount"
  }
}
```

Severity：

```text
error   阻止发布
warning 允许发布但需要在 UI 中明确展示
info    仅提示
```

首批稳定错误码命名空间：

```text
document.*
graph.*
plugin.*
node.*
parameter.*
expression.*
variable.*
device.*
controlFlow.*
```

## 19. 发布版本与 Hash

WorkflowRelease 保存：

```text
releaseId
workflowId
releaseNumber
publishedAt
document
executionHash
requiredPlugins[]
pythonScriptHashes[]
```

`executionHash` 基于规范化后的执行语义计算，不包含：

- Node/Scope Layout；
- DisplayName 和 Description；
- DescriptorSnapshot 的展示信息。

Hash 包含：

- 控制图；
- 输入和变量定义；
- Binding；
- NodeTypeReference；
- DeviceBinding；
- OutputMapping；
- Python 源码和入口函数。

运行始终引用 ReleaseId 和 ExecutionHash，不能直接引用可变 Draft。

## 20. 完整示例

可执行结构示例见：

- `examples/workflow_v1.example.json`
- `examples/node_descriptor_v1.example.json`
