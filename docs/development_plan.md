# PowerLab V1 开发计划

> 状态：执行基线  
> 目标：将当前需求和契约分解为可以逐项交给独立 Agent 完成、逐项审查和验证的开发任务。  
> 技术基线：.NET 10、C# 8.0、WPF、ASP.NET Core、SQLite、REST、SignalR、Python.NET。

## 1. 执行原则

### 1.1 单任务交付

每次只交给 Agent 一个任务。一个任务必须满足：

- 有明确的修改范围；
- 有可以独立运行的验证方式；
- 不预先实现后续任务；
- 不改变已经确认的数据契约和运行语义；
- 不修改与任务无关的用户文件；
- 完成后由主 Agent 审查，再发放下一个任务。

如果实现过程中发现契约矛盾，执行 Agent 应停止扩大实现，只记录问题和最小复现，由主 Agent与用户确认后再修改契约。

### 1.2 每个任务的完成流程

执行 Agent 完成任务后，需要报告：

1. 修改的文件和主要设计决定；
2. 实际执行的构建与测试命令；
3. 测试结果；
4. 尚未完成、刻意留给后续任务的内容；
5. 发现的风险或契约疑问。

主 Agent 负责：

1. 检查修改范围和依赖方向；
2. 检查 C# 8.0 兼容性；
3. 运行本任务的构建与测试；
4. 对高风险逻辑补充少量针对性验证；
5. 判断任务通过、要求返工或需要用户决策；
6. 任务通过后，根据实际代码状态生成下一个任务的提示词。

### 1.3 通用完成标准

除任务另有说明外，每项任务都必须满足：

- `dotnet build powerlab.slnx` 无错误；
- 本任务新增和受影响的测试全部通过；
- 手写与生成的 C# 源码均兼容 C# 8.0；
- 不出现 `record`、`init`、目标类型 `new()`、文件范围命名空间、`global using`、`required`、主构造函数和集合表达式等较新语法；
- 公共 API 带有必要的 XML 注释，内部实现不过度注释；
- 不捕获后无信息吞掉异常；
- 不提交密钥、本机绝对路径、构建输出、数据库文件或 Python 运行环境；
- DTO、领域模型、持久化实体和 WPF ViewModel 不互相替代；
- API Endpoint、Controller、SignalR Hub 不包含业务逻辑；
- 不引入与当前任务无关的第三方依赖。

## 2. 目标解决方案结构

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
├── PowerLab.Devices.Tests
├── PowerLab.Execution.Tests
├── PowerLab.Persistence.Tests
└── PowerLab.IntegrationTests
```

第一阶段保持上述项目数量，不为了每个接口额外创建微型项目。只有出现真实循环依赖时，才讨论增加新的 Abstractions 项目。

### 2.1 主要依赖方向

```text
PowerLab.Domain
    ↑
    ├── PowerLab.Plugin.Abstractions
    ├── PowerLab.Api.Contracts
    ├── PowerLab.Expressions
    ├── PowerLab.Persistence
    └── PowerLab.Designer.Core

PowerLab.Plugin.Abstractions
    ↑
    ├── PowerLab.PluginSystem
    ├── PowerLab.Devices
    ├── PowerLab.Execution
    ├── PowerLab.Python
    ├── PowerLab.BuiltInNodes
    └── PowerLab.SamplePlugin

PowerLab.Api.Contracts → PowerLab.Api.Client → PowerLab.Designer.Core → PowerLab.Designer.Wpf

PowerLab.Domain + Expressions + PluginSystem + Devices + Execution
    + Python + Persistence + Api.Contracts
    → PowerLab.RuntimeHost
```

关键限制：

- `Domain` 不依赖数据库、网络、WPF、插件实现或 Python.NET；
- `Plugin.Abstractions` 不依赖 WPF、ASP.NET Core、EF Core 或 RuntimeHost；
- `Execution` 不依赖 WPF、ASP.NET Core 或具体数据库；
- `Designer.Wpf` 不引用插件实现、Persistence、Execution、Devices 或 Python；
- `RuntimeHost` 是组合根，可以引用各实现项目；
- 插件项目只引用插件 SDK 及业务真正需要的稳定契约。

## 3. 里程碑概览

| 里程碑 | 目标 | 任务 |
| --- | --- | --- |
| M0 工程基础 | 建立可编译、可测试、依赖方向清晰的解决方案 | T01 |
| M1 契约与纯逻辑 | 落地工作流模型、API 契约、插件 SDK、表达式和静态校验 | T02–T07 |
| M2 宿主基础设施 | 完成插件、设备、持久化和 RuntimeHost 基础接口 | T08–T14 |
| M3 执行引擎 | 完成调度、控制结构、并行、暂停、故障处理和 Python | T15–T22 |
| M4 WPF 设计器 | 完成设计、保存、发布和运行调试闭环 | T23–T29 |
| M5 集成与交付 | 完成端到端验证、样例和开发者文档 | T30 |

## 4. 详细任务

## M0：工程基础

### T01 解决方案与项目骨架

**目标**

建立完整解决方案、项目目录、项目引用和最小测试基线，所有项目能够在 C# 8.0 下编译。

**范围**

- 创建第 2 节列出的所有项目；
- 将项目加入 `powerlab.slnx`；
- 按依赖方向添加最少的 ProjectReference；
- RuntimeHost 使用 ASP.NET Core 空宿主或 Web API 宿主，保持控制台启动方式；
- Designer.Wpf 使用 `net10.0-windows` 和 WPF；
- 其他项目使用 `net10.0`；
- 建立测试项目和最小冒烟测试；
- 删除模板自带的 `Class1`、WeatherForecast 等无关内容；
- 补充 `.gitignore` 中缺少的构建、数据库和嵌入式 Python 运行目录规则；
- 必要时增加 MSBuild 检查，防止子项目提高 `LangVersion`。

**不包含**

- 工作流数据类型；
- 插件接口；
- 数据库、API 端点、SignalR Hub；
- 任何执行逻辑和正式界面。

**验收**

- 解决方案中项目齐全；
- 项目引用与第 2.1 节一致且无循环引用；
- 全量 build 和 test 成功；
- RuntimeHost 可以启动并正常退出，但不要求提供业务端点；
- WPF 项目包含最小 App 和 MainWindow，能够编译；
- 抽查所有模板代码均已改为 C# 8.0。

### T02 Workflow Domain 数据契约

**依赖**：T01

**目标**

按照 `workflow_data_contracts.md` 落地工作流文档、节点、Scope、Binding、值类型、输出映射和校验问题等领域类型。

**范围**

- 实现 WorkflowDocument 及全部 V1 节点定义；
- 实现多态 JSON 序列化和反序列化；
- 对稳定 ID、JSON 字段名和枚举文本建立明确映射；
- 使用普通 C# 类，不使用 `record`；
- 为示例 JSON 增加往返序列化测试；
- 保证未知字段的兼容策略与文档一致。

**不包含**：结构校验、表达式解析、执行逻辑、数据库实体。

**验收**

- `docs/examples/workflow_v1.example.json` 可以反序列化、重新序列化并保持语义一致；
- Action、If、While、Parallel、Break、Continue、SetVariable、PythonScript 节点均有测试；
- Domain 不引入 EF Core、ASP.NET Core、WPF 或 Python.NET。

### T03 API 与实时事件契约

**依赖**：T01、T02

**目标**

按照 `runtime_api_contracts.md` 落地 REST DTO、ProblemDetails 扩展字段、运行快照和 SignalR 事件 DTO。

**范围**

- 实现请求、响应、分页、运行状态、节点状态、Fault、Event Envelope DTO；
- 确保 JSON 名称、枚举值和可空性稳定；
- 区分 API DTO 与领域对象；
- 提供显式映射扩展或 Mapper，但不引入自动映射框架；
- 增加序列化契约测试。

**不包含**：HTTP Endpoint、SignalR Hub、数据库实体和 API Client。

**验收**

- 文档中的代表性 JSON 能与 DTO 双向转换；
- RunStatus、NodeRuntimeStatus 和事件类型的文本值完全匹配文档；
- Api.Contracts 不依赖 RuntimeHost、EF Core 或 WPF。

### T04 插件 SDK 契约

**依赖**：T01、T02

**目标**

按照 `plugin_development_spec.md` 实现插件入口、节点提供者、执行器、设备类型、设备驱动、上下文和结果接口。

**范围**

- 定义插件 Manifest 对应模型；
- 定义 NodeDescriptor、ParameterSchema、OutputSchema、DeviceSlotSchema 所需的 SDK 边界；
- 定义 NodeExecutor 生命周期接口；
- 定义设备驱动工厂和业务能力访问边界；
- 定义 CancellationToken、PauseToken 或等效暂停协作抽象；
- 定义稳定错误对象，不跨插件边界抛送宿主实现类型；
- 添加 API 形状和最小假实现测试。

**不包含**：程序集扫描、插件加载、真实设备、调度和 WPF 编辑器。

**验收**

- 一个测试插件仅引用 Plugin.Abstractions 和 Domain 即可编译；
- SDK 不引用 WPF、ASP.NET Core、EF Core、RuntimeHost 或具体日志实现；
- 公共接口不使用默认接口实现，避免插件版本演进歧义。

### T05 表达式词法、语法与 AST

**依赖**：T01、T02

**目标**

实现受限表达式语言的 Lexer、Parser、AST 和带源码位置的语法诊断。

**范围**

- 四种标量字面量；
- `Inputs`、`Variables`、`Loop.Iteration`、`Context.RunId` 引用；
- 算术、比较、逻辑、条件运算和括号；
- 白名单函数调用的语法外形；
- 运算符优先级、结合性、转义和源码位置；
- 禁止赋值、语句、对象创建、任意成员调用等语法；
- 充分的正常、边界和错误测试。

**不包含**：类型检查、求值、BindingResolver。

**验收**

- 所有文档示例能解析为稳定 AST；
- 非法表达式返回结构化诊断而不是泄漏内部异常；
- Parser 不调用 Roslyn 动态编译、Python 或其他通用脚本引擎。

### T06 表达式类型检查、依赖分析与求值

**依赖**：T05

**目标**

在同一表达式 AST 上实现静态类型检查、变量依赖提取和纯求值器。

**范围**

- 四种标量类型规则；
- 白名单数学和字符串纯函数；
- 上下文符号表与未知符号诊断；
- 返回类型推导；
- 依赖集合提取；
- 一致性变量快照上的求值；
- 除零、溢出和函数参数错误的结构化运行时诊断；
- 结果不得执行 IO、反射或任意 CLR 方法。

**不包含**：节点派发和 UI 自动完成。

**验收**

- 表达式文档中的支持和禁止案例均有测试；
- 类型错误可在不执行表达式的情况下发现；
- 相同 AST 和上下文得到确定性结果。

### T07 工作流静态校验与执行计划编译

**依赖**：T02、T04、T06

**目标**

实现发布前的完整静态校验，并将合法 WorkflowDocument 编译为不包含 UI 布局信息的执行计划。

**范围**

- ID、引用、单入口单出口、可达性和非法回边校验；
- If/While/Parallel Scope 约束；
- Break/Continue 最近 While 和不得跨 Parallel 的校验；
- 参数 Binding、表达式类型、输出映射、设备 Slot 绑定校验；
- 并行变量写冲突警告；
- 插件和 NodeVersion 可用性校验接口；
- 生成规范化执行计划及稳定 Hash；
- 错误阻止发布，警告不阻止发布。

**不包含**：真正执行节点、读取数据库或加载插件程序集。

**验收**

- 每条发布阻断规则至少有正反测试；
- 布局坐标变化不改变执行计划 Hash；
- 非法 Break/Continue 和普通回边可靠阻止编译。

## M2：宿主基础设施

### T08 插件包清单与发现

**依赖**：T04

**目标**

实现插件目录扫描、Manifest 校验、重复标识诊断和插件目录状态投影，但暂不加载执行代码。

**范围**

- 插件包目录约定；
- Manifest JSON 解析和 Schema 校验；
- PluginId、版本、入口程序集、入口类型和文件存在性校验；
- 重复插件和不兼容契约版本诊断；
- 确定性扫描顺序；
- 不让单个坏插件阻止宿主发现其他插件。

**验收**：正常、缺文件、坏 JSON、重复 ID、错误版本均有测试，扫描结果稳定。

### T09 插件加载、隔离与 Catalog

**依赖**：T08

**目标**

使用独立 AssemblyLoadContext 加载插件，发现 NodeDescriptor 和 DeviceTypeDescriptor，并建立只读 Catalog。

**范围**

- 每插件独立 AssemblyLoadContext；
- 共享 SDK 程序集身份处理；
- 入口实例化和异常隔离；
- Descriptor 合规校验；
- NodeType 和 DeviceType 唯一索引；
- V1 仅启动时加载，不实现热更新或运行中卸载。

**验收**

- 两个有效插件可同时加载；
- 插件的私有依赖不会错误串用；
- 坏插件被标记失败且不影响其他插件；
- Catalog 可被并发只读访问。

### T10 示例插件与内置节点包

**依赖**：T04、T09

**目标**

提供一个真正通过插件机制加载的 SamplePlugin，以及框架内置节点的 Descriptor/注册包。

**范围**

- SamplePlugin 至少提供无设备动作节点和带设备 Slot 的动作节点；
- 内置节点覆盖 If、While、Parallel、Break、Continue、SetVariable、PythonScript 的设计器描述；
- 插件打包输出到测试插件目录；
- 示例展示参数动态显示、输出映射和暂停模式声明。

**不包含**：真实设备通信和 Python 执行。

**验收**：集成测试从磁盘发现并加载 SamplePlugin，Descriptor 与文档示例一致。

### T11 设备实例、DeviceManager 与设备租约

**依赖**：T04、T09

**目标**

实现设备实例配置、驱动长期生命周期、状态机、直接依赖解析和单实例互斥租约。

**范围**

- DeviceInstance 与连接配置的领域/应用模型；
- DeviceManager 启动、关闭和状态投影；
- 连接与初始化由 Manager/Driver 管理，不作为流程节点；
- Ready 检查不主动连接或初始化；
- 按稳定顺序一次获取节点所需的全部租约，避免死锁；
- 租约释放、取消和异常安全；
- 设备掉线事件上报接口。

**不包含**：隐式机械臂依赖、跨进程锁、真实厂商设备驱动。

**验收**

- 同一设备实例不被两个节点同时持有；
- 多设备租约不存在顺序反转死锁；
- 获取失败不会泄漏已获得租约；
- 未 Ready 设备导致启动前检查失败且不会触发连接。

### T12 SQLite 持久化基础

**依赖**：T02、T03

**目标**

实现 `powerlab.db` 与 `runs.db` 的持久化模型、DbContext、迁移和仓储边界。

**范围**

- WorkflowDraft、WorkflowRelease、PluginCatalog、DeviceInstance、ApplicationSettings；
- WorkflowRun、NodeExecution、RunEvent、VariableValue、RunLog；
- 数据库实体与外部 DTO/领域模型分离；
- 后台执行使用合适的 DbContext factory/scoping；
- SQLite 外键、索引、事务和并发令牌；
- 临时 SQLite 文件上的仓储测试；
- 草稿 JSON 和发布 JSON 的不可变语义。

**不包含**：断电恢复执行、API Endpoint 和运行调度。

**验收**

- 两个数据库可以从空目录迁移创建；
- CRUD、事务回滚、发布不可变性和事件序号均有测试；
- 测试不使用 EF Core InMemory Provider 替代 SQLite 语义。

### T13 RuntimeHost 组合根与运行基线

**依赖**：T03、T09、T11、T12

**目标**

搭建正式 ASP.NET Core RuntimeHost 的配置、依赖注入、错误处理、OpenAPI、健康检查和优雅关闭。

**范围**

- 使用现代 WebApplication hosting model，但代码必须为 C# 8.0；
- 结构化 Options 及启动时校验；
- 统一 ProblemDetails 和关联 ID；
- liveness/readiness 健康检查；
- 日志和本机回环地址默认配置；
- 插件、设备、数据库的启动和关闭顺序；
- Program 保持组合根，不承载业务逻辑；
- 为 WebApplicationFactory 集成测试开放可测试入口。

**不包含**：身份认证、远程监听、业务 API 和运行引擎。

**验收**

- 默认仅监听 loopback；
- 错误配置启动失败并给出明确诊断；
- 测试宿主可启动，健康检查和 ProblemDetails 生效；
- 停止宿主时设备与插件生命周期有序结束。

### T14 Catalog、设备和工作流管理 API

**依赖**：T07、T10、T11、T12、T13

**目标**

实现 System、Plugin、NodeType、Device、Workflow、WorkflowRelease REST API 和发布应用服务。

**范围**

- 按 `/api/v1` 路由组织；
- Endpoint 保持薄层；
- 草稿创建、查询、保存、导入、导出、校验和发布；
- 发布使用 T07 校验和执行计划 Hash；
- NodeDescriptor 与设备状态查询；
- ETag 或显式版本号用于单客户端误覆盖保护，但不实现协同编辑冲突合并；
- OpenAPI 描述和集成测试。

**不包含**：运行创建、运行控制和 SignalR 运行事件。

**验收**

- 完成“保存草稿 → 校验 → 发布 → 读取不可变 Release”闭环；
- 无效流程返回稳定 ProblemDetails；
- Designer 无需数据库访问即可取得全部设计期数据。

## M3：执行引擎

### T15 执行状态机、上下文与事件模型

**依赖**：T02、T03、T04、T07

**目标**

实现与 `runtime_api_contracts.md` 一致的 Run/Node 状态机、变量存储、运行上下文和有序事件流。

**范围**

- 合法状态迁移和非法迁移拒绝；
- Run、NodeAttempt、Fault 的运行时模型；
- Input 只读、Variable 原子读写和一致性快照；
- 单调 EventSequence；
- 运行日志接口；
- 领域事件到 API 事件 DTO 的映射。

**不包含**：节点调度、数据库写入和 SignalR 推送。

**验收**：每条状态迁移有测试，并发变量写保持原子，事件顺序严格递增。

### T16 RunScheduler 与顺序节点执行

**依赖**：T07、T11、T15

**目标**

实现可替换并发策略的 RunScheduler，以及能够执行单条顺序流程的 NodeScheduler。

**范围**

- 第一版 MaxConcurrentRuns=1；
- 多个 Run 可以 FIFO 排队；
- 启动前插件、节点版本和设备 Ready 检查；
- BindingResolver；
- ActionNode 和 SetVariableNode；
- 每次 Attempt 新建 NodeExecutor；
- 设备租约获取与释放；
- 输出完整校验后原子映射；
- 节点失败后停止派发。

**验收**

- 两个运行严格串行；
- 单条流程按控制边顺序执行；
- Binding 在节点调用前求值；
- 失败、取消或输出错误不会提交变量；
- 同一运行中不会重复派发同一节点 Attempt。

### T17 If、While、Break 与 Continue

**依赖**：T16

**目标**

执行结构化条件和循环控制。

**范围**

- If 只执行被选择的 Scope；
- While 使用前置条件；
- 记录 Loop.Iteration；
- Break 退出最近 While；
- Continue 跳过剩余循环体；
- Runtime 再次防御非法跨 Parallel 控制信号；
- 条件判断和下一迭代派发前检查运行状态。

**验收**：嵌套 If/While、空 If 分支、多层循环、Break、Continue 均有确定性测试。

### T18 Parallel、调度波次与设备竞争

**依赖**：T16、T17

**目标**

实现 Parallel 分支、Join、调度波次、共享变量和设备资源竞争语义。

**范围**

- 一次波次计算当前所有就绪节点；
- 分支并发运行和 Join；
- WaitingForResource 状态；
- 多节点设备租约公平性和无死锁；
- 共享变量立即生效，单次表达式使用一致性快照；
- 最后完成写入者生效；
- 任一分支失败后停止所有分支的新派发。

**验收**

- 无资源冲突节点可并发；
- 争用同一设备的节点不会并发；
- Join 等待所有未失败分支完成；
- 失败后已运行节点可收尾，但不会出现后继新派发。

### T19 暂停、继续与单步

**依赖**：T18

**目标**

实现 Pausing/Paused/Running/Stepping 状态和 NodeBoundary/Cooperative 暂停能力。

**范围**

- 暂停请求后停止新派发；
- 等待所有活动节点到达安全点后进入 Paused；
- PauseToken 协作点；
- Continue 恢复普通调度；
- Step 推进一个完整调度波次；
- Parallel 单步一次派发该波次所有就绪节点；
- 波次结束回到 Paused。

**验收**

- UI 可区分 Pausing 与 Paused；
- NodeBoundary 不会被强行中断；
- Cooperative 节点能在安全点暂停；
- Step 不会越过下一波次。

### T20 Fault 决策、人工重试、忽略与终止

**依赖**：T18、T19

**目标**

实现节点失败和设备掉线后的人工决策流程。

**范围**

- Faulting → FaultedAwaitingDecision；
- Retry 创建新的 AttemptNumber，不覆盖失败历史；
- Ignore 不提交失败输出并允许控制流继续；
- Terminate 向活动节点发送取消并进入终止状态；
- 命令 ID 幂等；
- 每项决策形成审计事件。

**不包含**：自动重试、退避、跨重启恢复。

**验收**：三种决策、重复命令、Parallel 中故障和设备掉线均有测试。

### T21 运行持久化与应用服务

**依赖**：T12、T15–T20

**目标**

将执行引擎接入 runs.db，并实现运行创建、队列、命令、快照和事件补取应用服务。

**范围**

- 运行只引用不可变 Release；
- 创建 Run 时验证 Inputs；
- 持久化状态、Attempt、Fault、变量、日志和事件；
- REST 快照所需查询投影；
- 按 Sequence 补取事件；
- 明确 V1 重启后活动运行标记 Interrupted，不恢复执行。

**验收**

- 执行记录可从新 DbContext 完整读取；
- 事件与快照最终一致；
- 宿主异常停止后再次启动，未完成运行标记为 Interrupted。

### T22 内嵌 Python 与 PythonScriptNode

**依赖**：T04、T15、T16、T21

**目标**

集成唯一内嵌 Python 环境和 Python.NET，实现受限 PythonScriptNode。

**范围**

- RuntimeHost 统一初始化和关闭 PythonEngine；
- 固定内嵌 Python 路径，不发现系统 Python；
- GIL 和线程边界；
- 标量 Inputs/Outputs 转换；
- 受限 Context：日志、输入、输出和可选暂停检查；
- 源码来自 Workflow JSON；
- 错误转换为结构化 Node Fault；
- 测试环境可在没有完整 Python 包时跳过明确标记的集成测试，但纯转换逻辑必须测试。

**验收**

- Python 代码可以读取输入并返回四种标量输出；
- 缺失输出、类型错误和 Python 异常不会提交变量；
- 多次节点执行不会重复初始化 PythonEngine；
- 插件不能自行控制 PythonEngine 生命周期。

### T23 Run REST API 与 SignalR RuntimeHub

**依赖**：T13、T21、T22

**目标**

公开运行创建、队列、快照、暂停、继续、单步、Fault 决策和实时事件接口。

**范围**

- 按 `runtime_api_contracts.md` 实现 Run API；
- 命令 ID 幂等；
- SignalR Hub 只负责订阅边界和组管理；
- 事件由后台发布器从有序运行事件流推送；
- 重连后通过 REST Snapshot + sinceSequence 补齐；
- Hub 慢客户端不得阻塞 Execution Engine；
- WebApplicationFactory 与 SignalR 客户端集成测试。

**验收**

- API 可以完成完整运行控制；
- 断开 SignalR 后通过 REST 恢复无事件缺口；
- 重复控制命令不会执行两次；
- Hub/Endpoint 中没有调度业务逻辑。

## M4：WPF 设计器

### T24 强类型 API Client

**依赖**：T03、T14、T23

**目标**

实现供 WPF 和未来 B/S 客户端参考的 REST/SignalR 客户端。

**范围**

- 使用 IHttpClientFactory 或可注入 HttpClient；
- typed methods、ProblemDetails 解析、取消和超时；
- SignalR 连接、重连、订阅和 Sequence 缺口通知；
- 不包含 WPF 类型；
- 使用测试 HTTP Handler 和测试宿主验证。

**验收**：API Client 可在非 WPF 测试中完成保存、发布、运行和事件订阅。

### T25 Designer.Core 文档与命令模型

**依赖**：T02、T03、T06、T07、T24

**目标**

实现与 WPF 无关的编辑会话、选择、Scope 导航、撤销重做、脏状态和设计期校验协调。

**范围**

- DesignerDocument/EditSession；
- 增删节点、连线、移动和参数修改命令；
- Undo/Redo；
- Root/Container Scope 导航；
- 本地快速校验与 RuntimeHost 正式校验结果合并；
- 不直接访问数据库或插件实现。

**验收**：所有编辑命令和 Undo/Redo 有纯单元测试，保存前后脏状态正确。

### T26 WPF 外壳、连接与 Node Catalog

**依赖**：T24、T25

**目标**

建立设计器主窗口、RuntimeHost 连接状态、节点分类列表、文档标签和基础 MVVM 设施。

**范围**

- 主窗口布局和导航；
- RuntimeHost 地址配置、连接和健康状态；
- 从 REST 加载 NodeDescriptor，不加载插件程序集；
- 节点搜索和分类；
- 可测试 ViewModel，不在 code-behind 放业务逻辑；
- 第一版可使用轻量自有 MVVM 基础，除非已有明确库选择。

**验收**：使用假 API Client 能显示分类节点列表并打开空白文档，断连状态清晰可见。

### T27 结构化流程画布

**依赖**：T25、T26

**目标**

实现节点、控制边、选择、拖放和结构化 Scope 的画布编辑能力。

**范围**

- Start/End、Action 和 Container 节点视觉；
- 节点拖放、选择、移动、删除和连线；
- If/While/Parallel inline 展开和 drill-down；
- Scope 面包屑；
- 非法连线即时反馈；
- 布局数据只影响 Designer；
- 暂不实现自动布局和 SubWorkflow。

**验收**：可以手工构造嵌套 If/While/Parallel 流程，序列化后结构正确。

### T28 动态属性编辑器与表达式体验

**依赖**：T06、T24、T27

**目标**

完全由 ParameterSchema 生成参数 UI，并支持常量、变量、表达式、设备绑定和输出映射。

**范围**

- 四种值类型编辑器；
- allowedBindings 切换；
- VisibleWhen、EnabledWhen、ApplicableWhen 三值规则；
- 分组、顺序、范围、枚举建议和基本 EditorHint；
- 变量/输入智能提示和表达式诊断；
- DeviceSlot 选择兼容设备实例；
- Output 显式映射到同类型变量；
- 插件不得注入 WPF 控件。

**验收**：使用 SamplePlugin Descriptor 驱动完整编辑，无需编写插件专属 WPF 代码。

### T29 保存、发布和运行调试 UI

**依赖**：T23–T28

**目标**

完成 Designer 到 RuntimeHost 的端到端用户闭环。

**范围**

- 新建、打开、保存、另存、导入、导出；
- 校验问题定位和发布；
- Workflow Inputs 填写和创建 Run；
- 队列和运行状态；
- 暂停、继续、单步、重试、忽略、终止；
- 节点多活动高亮、WaitingForResource、Pausing；
- 变量和日志面板；
- SignalR 重连后 REST 快照恢复。

**验收**：不直接访问数据库的 WPF Designer 可以完成设计、发布、运行和调试闭环。

## M5：集成与交付

### T30 端到端验收、样例和开发文档

**依赖**：T01–T29

**目标**

建立可重复的 V1 验收套件和开发者上手资料。

**范围**

- 示例流程：顺序、If、While+Break/Continue、Parallel+设备竞争、Fault 决策、Python；
- RuntimeHost + SQLite + SamplePlugin 集成测试；
- Designer 关键 ViewModel 和必要 UI 自动化冒烟测试；
- 插件开发、打包、安装、重启更新指南；
- 本地开发、数据库迁移、内嵌 Python 准备和故障排查；
- 验证第一阶段明确不包含的能力没有被误实现或公开承诺。

**验收**

- 新开发者按文档可以构建、启动 RuntimeHost、启动 Designer、安装样例插件并运行示例流程；
- 所有自动测试稳定通过；
- OpenAPI、JSON 示例和实现一致；
- 形成 V1 已知限制清单。

## 5. 每次主 Agent 的验证清单

主 Agent 收到任务完成结果后，至少执行以下检查：

```text
1. git status / git diff：确认修改范围
2. 项目引用检查：确认没有反向或循环依赖
3. C# 8.0 语法扫描与真实编译
4. 运行受影响的单元测试
5. 运行必要的集成测试
6. 对照任务验收条目逐项确认
7. 检查是否提前实现后续任务或引入无关包
8. 更新计划中的任务状态和必要备注
9. 生成下一任务提示词
```

任务状态使用：

```text
Pending → InProgress → Review → Done
                         └→ Rework
                         └→ DecisionRequired
```

默认不因为“代码可以编译”就判定任务完成；任务列出的行为验收和边界验收同样必须满足。

## 6. 当前执行入口

当前从 **T01 解决方案与项目骨架** 开始。T01 完成并通过主 Agent 验证后，再给出 T02 的最终提示词。后续提示词会以当时仓库的真实状态为依据，不提前固定，以避免计划和实现产生偏差。
