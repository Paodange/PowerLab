# PowerLab V1 需求与总体架构

> 状态：讨论基线草案  
> 本文记录 PowerLab 第一阶段已经确认的产品边界和运行语义。流程画布的基础方案参见 `workflow_designer_architecture.md`。

## 1. 产品目标

PowerLab V1 是一套面向自动化行业的通用流程设计与执行平台，提供：

- Windows WPF 流程设计器；
- 跨平台 RuntimeHost；
- 结构化流程编排；
- 插件化设备和动作节点；
- 工作流变量和受控表达式；
- 基于 Python.NET 的内嵌 Python 脚本节点；
- 流程运行、暂停、继续和单步调试；
- REST API 和 SignalR 实时事件接口。

第一阶段重点完成“设计、发布、执行、调试”闭环。

## 2. 第一阶段范围

### 2.1 包含

- `ActionNode` 插件节点；
- `IfNode`；
- `WhileNode`；
- `ParallelNode`；
- `BreakNode` 和 `ContinueNode`；
- `SetVariableNode`；
- `PythonScriptNode`；
- 工作流输入和工作流变量；
- 常量、变量和表达式参数绑定；
- 节点输出显式映射到变量；
- 设备插件、设备实例和设备互斥；
- 单活动运行和排队；
- 暂停、继续和调度波次单步；
- 草稿、发布版本、运行记录和日志持久化。

### 2.2 不包含

- 台面、器具、耗材、库存和位置模型；
- `ForEach`；
- `SubWorkflow`；
- Scope 局部变量；
- 节点输出直接表达式引用；
- 隐式机械臂或搬运模块依赖推导；
- 插件热更新；
- 插件自定义 WPF 控件；
- 自动重试策略；
- 断电恢复和运行中断点恢复；
- 远程运行；
- B/S 流程设计器；
- 3D 仿真。

## 3. 总体架构

```text
Windows
┌────────────────────────────┐
│ PowerLab.Designer.Wpf      │
│                            │
│ Canvas / Outline           │
│ Property Editor            │
│ Validation / Debug UI      │
└──────────────┬─────────────┘
               │ REST + SignalR
               ▼
┌─────────────────────────────────────────────┐
│ PowerLab.RuntimeHost                        │
│ Cross-platform ASP.NET Core Console Host    │
│                                             │
│ Application API                             │
│ Workflow Repository / Publisher             │
│ Plugin Catalog / Device Manager             │
│ Binding Resolver / Expression Engine        │
│ Run Scheduler / Node Scheduler              │
│ Execution Engine / Python Runtime           │
│ Persistence / Events / Logs                 │
└─────────────────────────────────────────────┘
```

### 3.1 Designer 职责

- 编辑和展示工作流；
- 节点参数编辑、动态显示和智能提示；
- 调用 RuntimeHost 完成保存、校验、发布和运行控制；
- 订阅并展示运行状态、节点状态、变量和日志；
- 不访问数据库；
- 不加载插件实现程序集；
- 不执行设备操作和 Python。

### 3.2 RuntimeHost 职责

- 作为所有业务数据的唯一访问入口；
- 管理插件、设备驱动和内嵌 Python 生命周期；
- 校验、编译和执行工作流；
- 调度运行和节点；
- 管理设备资源锁；
- 保存运行事件、变量和日志；
- 提供 REST、SignalR 和 OpenAPI。

RuntimeHost 第一阶段是独立控制台程序，不依赖 WPF，不安装为系统服务。关闭 Designer 不应自动结束 RuntimeHost。

## 4. 外部接口

RuntimeHost 的正式外部接口采用：

- REST：命令、查询、草稿和发布管理；
- SignalR：运行事件、日志和状态变化；
- OpenAPI：接口文档和客户端生成。

第一阶段监听本机回环地址。远程模式完成 TLS、身份认证和授权后再开放。

```text
/api/v1/system
/api/v1/plugins
/api/v1/node-types
/api/v1/device-types
/api/v1/devices
/api/v1/workflows
/api/v1/workflow-releases
/api/v1/runs
/api/v1/run-queue
/hubs/runtime
```

SignalR 不是事实来源。客户端断线重连后先通过 REST 获取快照和缺失事件，再恢复实时订阅。

## 5. 数据所有权与持久化

只有 RuntimeHost 可以打开业务数据库。Designer 只能保存派生缓存，不能绕过 API 修改数据。

建议第一阶段使用两个 SQLite 文件：

```text
data/
├── powerlab.db
│   ├── WorkflowDraft
│   ├── WorkflowRelease
│   ├── PluginCatalog
│   ├── DeviceInstance
│   └── ApplicationSettings
│
└── runs.db
    ├── WorkflowRun
    ├── NodeExecution
    ├── RunEvent
    ├── VariableValue
    └── RunLog
```

- 草稿以数据库为主；
- 支持导入和导出 JSON；
- 运行只能使用不可变的发布版本；
- 发布版本保存 Workflow Schema、插件、节点和脚本版本信息；
- 运行中的草稿修改不影响已经发布或正在运行的版本。

## 6. 工作流结构

```text
Workflow
├── Inputs
├── Variables
└── RootScope
    ├── Nodes
    └── ControlEdges
```

每个 Scope 是单入口、单出口的结构化区域：

- 主流程画布显示 `StartNode` 和 `EndNode`；
- If、While、Parallel 内部 Scope 使用隐式入口和出口；
- 孤立节点、不可达节点和无效连线阻止发布；
- 普通回边非法，循环只能由 While 表达；
- If 分支可以为空；
- Parallel 至少包含两个非空分支。

## 7. While、Break 和 Continue

While 使用前置条件语义：

```text
Evaluate Condition
├── false → Exit While
└── true  → Execute Body → Evaluate Condition Again
```

- 第一阶段不配置最大迭代次数；
- RuntimeHost 记录并展示当前迭代次数；
- 条件判断和循环体派发前都必须检查暂停、取消及错误状态；
- 设计器可以对明显不变化的循环条件发出警告，但不阻止发布。

Break 和 Continue 是框架内置控制节点：

- 作用于最近一层 While；
- 可以穿过 If 等普通 Scope；
- 不能跨越 Parallel 边界；
- 在循环外使用时禁止发布；
- 不具有普通后继控制连线；
- Break 退出 While；
- Continue 跳过剩余循环体并重新判断 While 条件。

## 8. 值、输入和变量

V1 只提供四种标量值类型：

```text
Integer  → System.Int32
Number   → System.Double
Boolean  → System.Boolean
String   → System.String
```

第一阶段不支持集合、自定义对象、单位值和 Scope 局部变量。

### 8.1 Workflow Inputs

- 在启动运行时提供；
- 运行期间只读；
- 发布时校验名称和类型。

### 8.2 Workflow Variables

- 在流程定义中声明名称、类型和初始 Binding；
- 初始值可以是常量或表达式；
- 使用内置 `SetVariableNode` 修改；
- 插件节点通过输出映射修改。

### 8.3 Output Mapping

插件声明强类型输出字段，设计者将其显式映射到变量：

```text
ReadTemperature.temperature
    → Variables.CurrentTemperature
```

- 输出类型必须与变量类型一致；
- 节点成功后一次性提交全部映射输出；
- 节点失败或被忽略时不提交输出；
- V1 表达式不能直接引用节点输出。

## 9. 参数绑定与表达式

每个节点输入参数使用统一 Binding：

```text
InputBinding
├── LiteralBinding
├── VariableBinding
└── ExpressionBinding
```

表达式由 RuntimeHost 的 `BindingResolver` 在节点即将执行时统一求值。插件执行器只接收求值和类型检查后的输入，不负责解析表达式或读取变量。

执行顺序：

```text
Check Run State
  → Resolve Bindings
  → Validate Resolved Values
  → Resolve Direct Device Requirements
  → Acquire Device Leases
  → Check Run State Again
  → Invoke Node Executor
  → Commit Outputs
  → Release Leases
```

### 9.1 V1 表达式上下文

```text
Inputs.xxx
Variables.xxx
Loop.Iteration
Context.RunId
```

### 9.2 V1 表达式能力

- 整数、浮点数、布尔和字符串常量；
- `+ - * / %`；
- `== != > >= < <=`；
- `&& || !`；
- `condition ? value1 : value2`；
- 受控的数学和字符串纯函数。

表达式不支持赋值、语句、对象创建、任意方法、IO、HTTP、数据库、设备调用和反射。

设计器和 RuntimeHost 使用同一个 Lexer、Parser、AST 和类型检查器。静态错误必须在编辑期提示并阻止发布；除零等动态错误由运行时 BindingResolver 捕获，节点执行器不会被调用。

## 10. 设备模型

设备插件提供设备类型、驱动和动作节点；设备实例是 RuntimeHost 保存的配置数据。

```text
DevicePlugin
├── DeviceTypeProvider
├── DeviceDriverFactory
└── NodeProvider

DeviceInstance
├── Id
├── DisplayName
├── PluginId
├── DeviceTypeId
├── ConnectionSettings
└── State
```

- 连接和释放由 RuntimeHost 的 DeviceManager 长期维护；
- 工作流只暴露设备业务能力，不提供 Connect/Disconnect 节点；
- 启动流程前扫描节点的直接设备依赖；
- 任一必需设备未 Ready 时拒绝启动；
- 启动检查不主动连接或初始化设备；
- 一个设备实例同一时间只能由一个节点持有；
- 隐式机械臂、能力依赖和台面路径推导暂不实现。

节点类型通过 `DeviceSlot` 声明设备类型要求，节点实例将 Slot 绑定到具体 DeviceInstance。设备绑定第一阶段不是表达式，运行中不能动态切换。

## 11. 插件和节点执行

- RuntimeHost 启动时扫描插件目录；
- 每个插件使用独立 AssemblyLoadContext；
- 运行期间不热加载或卸载插件；
- 安装、删除或升级插件后重启 RuntimeHost；
- Designer 只接收序列化 NodeDescriptor，不加载插件实现；
- 插件不能提供 WPF 控件；
- 参数界面由跨 UI 的声明式 Schema 生成；
- NodeExecutor 每次执行尝试创建新实例；
- 跨执行状态必须保存在 DeviceManager、DeviceDriver 或插件级服务中。

## 12. 两级调度

### 12.1 RunScheduler

负责运行队列和运行级控制。第一阶段：

```text
MaxConcurrentRuns = 1
```

可以排队多个运行，但同一时间只有一个活动运行。并发策略必须抽象，后续支持多个活动运行时不改变 Execution Engine 的基本接口。

### 12.2 NodeScheduler

负责：

- 计算就绪节点；
- Parallel 分支调度；
- 设备租约；
- 暂停安全点；
- 单步波次；
- 错误后停止派发。

设备互斥由 NodeScheduler 和 DeviceManager 保证，不能依赖各插件自行创建锁。节点执行器只能使用已经授予的设备租约。

## 13. 暂停、继续和单步

```text
Running → Pausing → Paused
Paused  → Running
Paused  → Stepping → Paused
```

- 暂停请求后不再派发新节点；
- 已执行节点默认运行到节点边界；
- `NodeBoundary` 节点在完成后暂停；
- `Cooperative` 节点可以在内部安全点等待 PauseToken；
- 只有全部活动节点到达安全点后，运行状态才变为 Paused。

Parallel 单步采用调度波次：

- 一次 Step 派发当前波次所有就绪节点；
- 获取到资源的节点进入 Running 并高亮；
- 资源不足的节点显示 WaitingForResource；
- 本波次运行期间不派发后继波次；
- 波次结束后重新进入 Paused。

## 14. Parallel 变量与错误语义

第一阶段所有 Parallel 分支共享同一个变量空间：

- 不创建分支变量快照；
- 输出映射成功后立即生效；
- 每次变量写入是原子的；
- 并行写同一个变量时最后写入者生效；
- 设计器应对可识别的并行写冲突给出警告；
- 一次表达式求值读取其依赖变量的一致性快照。

节点失败后：

```text
Running → FaultedAwaitingDecision
```

- 所有分支停止派发新节点；
- 已经执行的节点允许运行到自身结束点；
- 用户可以选择人工重试、忽略或终止；
- 忽略失败节点时不提交其输出；
- 终止时向仍执行的节点发送取消请求；
- 自动重试、退避和重试安全策略留到后续版本。

## 15. Python

- RuntimeHost 内嵌并管理唯一 Python 环境；
- 不寻找或允许选择系统 Python、虚拟环境；
- PythonEngine 由 `PowerLab.Python` 统一初始化和关闭；
- 插件不能自行初始化 PythonEngine；
- Python 源码第一阶段直接保存到 Workflow JSON；
- Python 节点声明四种标量类型的 Inputs 和 Outputs；
- Python 节点整体作为一个执行和单步单位；
- Python 通过受限 C# Context 写日志、读取输入和返回输出；
- Python 节点默认采用 NodeBoundary 暂停，主动配合时可以支持 Cooperative 暂停。

## 16. 建议项目结构

```text
src/
├── PowerLab.Domain
├── PowerLab.Plugin.Abstractions
├── PowerLab.Api.Contracts
├── PowerLab.Api.Client
├── PowerLab.Expressions
├── PowerLab.PluginSystem
├── PowerLab.Devices
├── PowerLab.Execution
├── PowerLab.Python
├── PowerLab.Persistence
├── PowerLab.RuntimeHost
├── PowerLab.Designer.Core
└── PowerLab.Designer.Wpf

plugins/
├── PowerLab.BuiltInNodes
└── PowerLab.SamplePlugin

tests/
├── PowerLab.Domain.Tests
├── PowerLab.Expressions.Tests
├── PowerLab.PluginSystem.Tests
├── PowerLab.Execution.Tests
└── PowerLab.IntegrationTests
```

依赖约束：

- Domain 不依赖 UI、数据库、网络、插件实现或 Python.NET；
- Plugin.Abstractions 不依赖 WPF；
- Execution 不依赖 WPF；
- Designer.Wpf 通过 Api.Client 与 RuntimeHost 通信；
- API DTO、领域对象和数据库实体保持分离；
- 业务逻辑位于 Application/Domain 服务中，不写入 API Controller 或 SignalR Hub。

## 17. 后续演进边界

- B/S Designer 使用相同 REST、SignalR 和 OpenAPI；
- RuntimeHost 可以托管 Web Designer 静态资源；
- 多运行并发通过替换 RunScheduler 并发策略实现；
- 设备能力路由和隐式依赖由 Execution Planner 扩展；
- 3D 仿真通过独立 Simulation Adapter/Worker 扩展；
- gRPC 仅在 RuntimeHost 与内部 Worker 之间确有高吞吐或双向流需求时引入；
- 断电恢复必须建立设备动作幂等性和人工恢复协议后再实现。

