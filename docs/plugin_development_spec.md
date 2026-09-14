# PowerLab V1 插件开发规范草案

> 状态：V1 设计草案  
> 目标：让设备供应商和业务开发者以稳定、可发现、与 UI 解耦的方式提供设备驱动和流程节点。

NodeDescriptor、ParameterSchema、Binding 和 Workflow JSON 的规范外形参见 `workflow_data_contracts.md`；运行状态与 REST DTO 参见 `runtime_api_contracts.md`。

插件及 SDK 源码必须遵守 `csharp_coding_standards.md`，使用 C# 8.0 语法。插件项目不得提高 `LangVersion`。

## 1. 插件边界

一个插件包可以提供以下一种或多种能力：

- 设备类型；
- 设备驱动；
- 流程动作节点；
- 插件级共享服务。

插件不能：

- 直接访问 RuntimeHost 数据库；
- 提供或引用 WPF 控件；
- 自行初始化 PythonEngine；
- 绕过调度器操作未声明设备；
- 在表达式中注册有副作用的行为；
- 修改工作流控制图或直接选择下一个节点。

## 2. 插件包结构

```text
com.vendor.robot/
├── plugin.json
├── lib/
│   └── net10.0/
│       └── Vendor.Robot.Plugin.dll
├── runtimes/
│   └── win-x64/native/
├── icons/
└── docs/
```

示例 Manifest：

```json
{
  "manifestVersion": 1,
  "pluginId": "com.vendor.robot",
  "displayName": "Vendor Robot",
  "version": "1.2.0",
  "sdkVersion": "1.0",
  "entryAssembly": "lib/net10.0/Vendor.Robot.Plugin.dll",
  "entryType": "Vendor.Robot.RobotPlugin",
  "supportedRuntimes": ["win-x64"],
  "capabilities": ["device", "workflow-nodes"]
}
```

要求：

- `pluginId` 全局稳定且不可随显示名称改变；
- 插件版本使用语义化版本；
- 必须声明支持的 Runtime Identifier；
- 原生依赖放入对应 `runtimes/<rid>`；
- Manifest 必须在加载程序集前完成校验。

## 3. 加载规则

- RuntimeHost 启动时扫描配置的插件目录；
- 一个插件包创建一个 AssemblyLoadContext；
- Plugin.Abstractions 等共享契约程序集由默认上下文提供；
- 插件不得私带另一份契约程序集造成类型隔离；
- 插件加载失败不应阻止 RuntimeHost 启动；
- 加载失败信息通过 Plugin API 和日志暴露；
- V1 不支持热更新，更新插件后必须重启 RuntimeHost。

AssemblyLoadContext 仅用于依赖隔离，不是安全沙箱。V1 插件均视为可信代码。

## 4. 插件入口

概念接口：

```csharp
public interface IPowerLabPlugin
{
    void Configure(IPluginBuilder builder);
}
```

插件通过受控 Builder 注册能力：

```csharp
public sealed class RobotPlugin : IPowerLabPlugin
{
    public void Configure(IPluginBuilder builder)
    {
        builder.AddDeviceProvider<RobotDeviceProvider>();
        builder.AddNodeProvider<RobotNodeProvider>();
    }
}
```

不向插件暴露 RuntimeHost 的根 `IServiceProvider`。插件只能通过稳定的 SDK facade 获取日志、配置、设备会话和执行上下文。

## 5. 稳定标识与版本

节点类型由以下字段唯一描述：

```text
PluginId
NodeTypeId
NodeVersion
```

示例：

```text
PluginId    = com.vendor.robot
NodeTypeId  = move-to-position
NodeVersion = 2
```

规则：

- `NodeTypeId` 不能使用 CLR 完整类名；
- 相同 NodeVersion 必须保持参数和输出兼容；
- 不兼容修改必须增加 NodeVersion；
- V1 不提供自动节点迁移；
- 工作流保存 ParameterSchemaSnapshot；
- 插件缺失或版本不兼容时节点仍可显示，但不能发布或执行。

## 6. NodeDescriptor

插件必须通过 NodeProvider 提供可序列化描述：

```csharp
public interface INodeProvider
{
    IReadOnlyList<NodeDescriptor> GetNodeDescriptors();
    INodeExecutor CreateExecutor(string nodeTypeId);
}
```

NodeDescriptor 概念字段：

```text
NodeTypeId
NodeVersion
DisplayName
Description
Category
IconResource
Parameters[]
Outputs[]
DeviceSlots[]
PauseMode
```

Descriptor 不能包含：

- WPF 类型；
- 委托；
- 运行期对象引用；
- 任意 CLR Type 序列化名称；
- 需要执行插件业务代码才能展示的 UI。

## 7. 参数类型

V1 节点参数和输出支持：

```text
Integer
Number
Boolean
String
```

SDK 在内部映射为：

```text
Integer  → Int32
Number   → Double
Boolean  → Boolean
String   → String
```

参数定义示例：

```json
{
  "name": "speed",
  "displayName": "Speed",
  "valueType": "number",
  "required": true,
  "defaultValue": 50.0,
  "minimum": 0.0,
  "maximum": 100.0,
  "editor": "number"
}
```

## 8. 参数动态显示

参数 UI 由 Schema 声明，不能由插件返回 WPF 控件。

支持的展示元数据：

- 显示名称和说明；
- 参数顺序和分组；
- 必填；
- 默认值；
- 数字范围；
- 可选值；
- `visibleWhen`；
- `enabledWhen`。

示例：

```json
{
  "name": "positionName",
  "valueType": "string",
  "visibleWhen": "Parameters.Mode == \"NamedPosition\""
}
```

动态显示规则：

- 使用受控的声明式条件表达式；
- 只能读取当前节点参数；
- 不能读取设备、数据库、文件或网络；
- 不能修改参数；
- Designer 和未来 Web Designer 使用相同规则；
- 隐藏参数仍保存在工作流中，除非用户显式清除。

动态显示只影响编辑体验，不改变 RuntimeHost 的参数验证。RuntimeHost 必须独立验证所有实际生效的参数。

## 9. 参数 Binding

节点参数保存为：

```text
LiteralBinding
VariableBinding
ExpressionBinding
```

插件不读取或解析 Binding。RuntimeHost 在执行前把所有 Binding 转换为强类型最终值。

```csharp
public interface INodeExecutor
{
    ValueTask<NodeExecutionResult> ExecuteAsync(
        NodeExecutionContext context,
        CancellationToken cancellationToken);
}
```

`NodeExecutionContext.Inputs` 只包含已求值的 Integer、Number、Boolean 和 String。

插件可以检查业务范围，但不能自行重新解释表达式。

## 10. 输出

插件在 Descriptor 中声明输出：

```json
{
  "name": "temperature",
  "displayName": "Temperature",
  "valueType": "number",
  "required": true
}
```

执行器返回输出值：

```csharp
return NodeExecutionResult.Success(new Dictionary<string, WorkflowValue>
{
    ["temperature"] = WorkflowValue.FromNumber(value)
});
```

规则：

- RuntimeHost 校验输出名称和类型；
- 所有必需输出通过校验后才一次性提交；
- 工作流设计者显式将输出映射到变量；
- 执行失败或被忽略时不提交输出；
- V1 表达式不能直接引用节点输出。

## 11. 设备类型与设备实例

一个设备插件可以声明一个或多个 DeviceType：

```csharp
public interface IDeviceProvider
{
    IReadOnlyList<DeviceTypeDescriptor> GetDeviceTypes();
    IDeviceDriver CreateDriver(DeviceCreateContext context);
}
```

DeviceTypeDescriptor 至少包含：

```text
DeviceTypeId
DisplayName
ConfigurationSchema
SupportedRuntimes
```

RuntimeHost 根据 DeviceType 创建 DeviceInstance。多个 DeviceInstance 可以使用同一 DeviceType，但具有不同名称和连接配置。

连接配置中的密码、令牌等敏感字段必须标记为 Secret，不能通过普通查询 API 原样返回。

## 12. DeviceDriver 生命周期

```csharp
public interface IDeviceDriver : IAsyncDisposable
{
    ValueTask ConnectAsync(CancellationToken cancellationToken);
    ValueTask DisconnectAsync(CancellationToken cancellationToken);
    ValueTask<DeviceReadiness> GetReadinessAsync(
        CancellationToken cancellationToken);
}
```

规则：

- Driver 实例由 DeviceManager 创建和持有；
- 工作流节点不能创建、连接或释放 Driver；
- 工作流不提供 Connect/Disconnect 节点；
- 运行前只查询 Readiness，不主动连接或初始化；
- Driver 必须将设备状态转换为标准状态和插件诊断信息；
- RuntimeHost 停止时由 DeviceManager 统一释放 Driver。

设备的连接触发策略属于 RuntimeHost 设备管理功能，不属于工作流语义。

## 13. 设备槽位与直接依赖

节点通过 DeviceSlot 声明要求：

```json
{
  "name": "robot",
  "displayName": "Robot",
  "requiredDeviceType": "com.vendor.robot/robot-v1",
  "required": true
}
```

节点实例绑定具体设备：

```json
{
  "deviceBindings": {
    "robot": "robot-a"
  }
}
```

V1 规则：

- DeviceBinding 不是表达式；
- 运行中不能动态切换设备；
- 流程启动前可以扫描出全部直接设备依赖；
- 所有必需设备必须处于 Ready；
- 节点实际访问的所有设备都必须声明；
- 隐式机械臂、能力依赖和关联设备展开不在 V1 范围。

## 14. 设备租约

节点执行器不能自行创建设备并发锁。NodeScheduler 在调用执行器前一次性获取全部 DeviceLease。

```text
Resolve Bindings
  → Resolve DeviceBindings
  → Acquire All DeviceLeases
  → Invoke Executor
  → Release DeviceLeases
```

V1 中每个设备租约都是 Exclusive。同一个设备不能被两个节点同时操作，包括同一 Parallel 的不同分支。

执行器通过 Context 获取设备：

```csharp
var robot = context.Devices.GetRequired<IRobotDevice>("robot");
```

如果声明的租约不存在，SDK 应立即抛出插件契约错误，而不是绕过调度器寻找设备实例。

## 15. NodeExecutor 生命周期

- 每个节点执行 Attempt 创建新的 NodeExecutor；
- 人工重试创建新 Executor；
- Executor 不保存跨 Attempt 状态；
- 长期状态保存在 Driver 或插件级服务；
- Executor 执行完成后不得留下后台线程或未追踪任务；
- Executor 必须尊重 CancellationToken；
- Executor 不得直接决定后继节点。

## 16. 暂停能力

节点声明：

```text
PauseMode.NodeBoundary
PauseMode.Cooperative
```

### NodeBoundary

- 收到暂停请求后允许当前执行完成；
- 完成后调度器不派发后继节点。

### Cooperative

- 插件可以在设备业务安全点调用 PauseToken；
- 插件负责保证暂停点不会让设备处于危险或不完整状态；
- 暂停不释放 DeviceManager 持有的设备连接；
- 是否在暂停期间继续持有当前节点的设备租约，由 V1 执行协议统一规定，插件不能自行决定。

## 17. 错误与人工处理

执行器失败时返回结构化错误：

```text
Code
Message
Details
PluginId
NodeTypeId
DeviceId
```

不要将异常堆栈直接作为用户消息。RuntimeHost 保存完整诊断信息，并通过 API 返回可展示错误。

节点失败后调度器停止派发新节点。用户可以：

- 人工重试；
- 忽略失败；
- 终止运行。

人工重试创建新的 Attempt 和 Executor。V1 不提供自动重试、退避或幂等策略。

## 18. 插件合规要求

插件发布前至少应通过 SDK 合规测试：

- Manifest 和稳定 ID 校验；
- NodeDescriptor 可序列化；
- 参数和输出 Schema 校验；
- 无 WPF 依赖；
- 不私带契约程序集；
- 输出名称和类型校验；
- CancellationToken 基础响应测试；
- NodeBoundary/Cooperative 声明一致性测试；
- 设备租约外访问检测；
- Executor 生命周期和后台任务泄漏检查；
- 不兼容 NodeVersion 检查。

后续可以通过 `PowerLab.PluginSdk.Testing` 提供测试 Fixture 和插件验证命令。
