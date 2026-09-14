# PowerLab V1 运行状态机与 REST/SignalR 契约

> 状态：V1 契约草案  
> 本文定义 RuntimeHost 的运行状态、节点状态、HTTP API、错误响应和实时事件契约。

## 1. API 技术边界

RuntimeHost 使用 ASP.NET Core 10：

- Controller-based REST API；
- OpenAPI 文档；
- SignalR RuntimeHub；
- BackgroundService 承载 RunScheduler；
- EF Core/SQLite 仅位于 Persistence 层；
- Controller 和 Hub 只做协议适配，不承载业务逻辑。

选择 Controller API 的原因是接口面较大，需要统一 `[ApiController]` 行为、属性路由、过滤器、授权和 ProblemDetails。不要在同一业务功能中混用 Controller 与 Minimal API。

## 2. RunStatus

```csharp
public enum RunStatus
{
    Queued,
    Preparing,
    Running,
    Pausing,
    Paused,
    Stepping,
    Faulting,
    FaultedAwaitingDecision,
    Recovering,
    Terminating,
    Completed,
    Failed,
    Terminated,
    Interrupted
}
```

状态图：

```text
                          ┌──────────────┐
                          │    Queued    │
                          └──────┬───────┘
                                 ▼
                          ┌──────────────┐
                          │  Preparing   │
                          └───┬──────┬───┘
                              │      └──────────────→ Failed
                              ├──────── debug ─────→ Paused
                              ▼
                          ┌──────────────┐
                  ┌──────→│   Running    │───────→ Completed
                  │       └───┬──────┬───┘
                  │           │      │ node fault
                  │ pause     │      ▼
                  │           │   Faulting
                  │           ▼      │ wait active nodes
                  │        Pausing    ▼
                  │           │   FaultedAwaitingDecision
                  │           ▼      ├── retry → Recovering ──→ Paused
                  └──────── Paused ←─┤
                              │      ├── ignore ──────────────→ Paused
                              │ step └── terminate
                              ▼                  │
                           Stepping              ▼
                              │             Terminating
                              └──→ Paused         │
                                                 ▼
                                             Terminated

RuntimeHost 意外退出：任何非终态运行 → Interrupted
```

终态：

```text
Completed
Failed
Terminated
Interrupted
```

## 3. 状态语义

### Queued

- 已创建运行并进入 RunScheduler；
- 尚未占用活动运行槽；
- V1 同一时间只有一个活动运行；
- 可以直接终止。

### Preparing

- 校验 Release、插件和 NodeVersion；
- 绑定 Workflow Inputs；
- 初始化 Workflow Variables；
- 扫描所有直接设备依赖；
- 检查所有必需设备是否 Ready；
- 不主动连接或初始化设备。

Normal Run 在 Preparing 成功后进入 Running；Debug Run 在 Preparing 成功后直接进入 Paused，等待 Step 或 Resume。

创建 Run 前执行一次快速预检查；真正获得调度槽后必须再次执行完整 Preparing，避免排队期间环境变化。

### Running

- NodeScheduler 可以派发新的就绪节点；
- Parallel 可以同时存在多个 NodeAttempt；
- 派发前必须检查当前 RunStatus。

### Pausing / Paused

- 接受暂停命令后立即进入 Pausing；
- Pausing 时不派发新节点；
- NodeBoundary 节点执行到结束；
- Cooperative 节点可以进入内部安全暂停点；
- 所有活动节点均到达安全点后进入 Paused；
- Cooperative 节点暂停期间继续持有已获得的设备租约。

### Stepping

- 只能由 Paused 进入；
- 一次 Step 创建一个新的 `waveId`；
- 派发波次开始时所有就绪节点；
- 新产生的后继节点留给下一波次；
- 当前波次全部完成后回到 Paused；
- Parallel 中可有多个同属一个 waveId 的 Running NodeAttempt。

### Faulting / FaultedAwaitingDecision

- 任一节点失败后立即进入 Faulting；
- Faulting 时停止派发新节点；
- 已经 Running 的其他节点继续到自身结束或安全点；
- 所有活动节点收敛后进入 FaultedAwaitingDecision；
- 同一次 Faulting 期间发生的多个错误全部记录为未解决 Fault；
- 只有进入 FaultedAwaitingDecision 后才接受 Retry/Ignore/Terminate 决策。

### Recovering

- 人工 Retry 创建新的 NodeAttempt 和 NodeExecutor；
- 重新检查目标节点设备 Readiness 并重新获取设备租约；
- Retry 成功后解决对应 Fault；
- 所有 Fault 解决后进入 Paused，由用户显式 Resume 或 Step；
- Retry 再次失败时回到 FaultedAwaitingDecision；
- V1 不自动重试。

### Terminating / Terminated

- 停止派发新节点；
- 向活动节点发出 CancellationToken；
- 等待插件在安全点结束；
- 不强杀线程，不等同于设备急停；
- 全部活动节点结束后进入 Terminated。

### Failed

用于无法进入人工处理流程的系统级失败，例如损坏的 Release、插件契约违规或持久化故障。普通节点业务失败优先进入 FaultedAwaitingDecision。

### Interrupted

RuntimeHost 启动时将遗留的非终态 Run 标记为 Interrupted。V1 不提供断点恢复，也不会自动重放任何物理动作。

## 4. NodeRuntimeStatus

```csharp
public enum NodeRuntimeStatus
{
    Pending,
    Ready,
    WaitingForResource,
    Running,
    PauseRequested,
    Paused,
    Succeeded,
    Failed,
    Ignored,
    CancelRequested,
    Cancelled,
    Skipped
}
```

每次执行产生独立 NodeAttempt：

```csharp
public sealed class NodeAttemptDto
{
    public string AttemptId { get; set; } = string.Empty;
    public string RunId { get; set; } = string.Empty;
    public string NodeId { get; set; } = string.Empty;
    public int AttemptNumber { get; set; }
    public string? WaveId { get; set; }
    public NodeRuntimeStatus Status { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? FinishedAt { get; set; }
    public StructuredErrorDto? Error { get; set; }
    public IReadOnlyDictionary<string, WorkflowValueDto> Outputs { get; set; }
        = new Dictionary<string, WorkflowValueDto>();
}
```

规则：

- 人工 Retry 增加 AttemptNumber，不覆盖历史 Attempt；
- UI 根据 Running/Paused 状态高亮节点；
- WaitingForResource 使用独立视觉状态；
- 失败 Attempt 保持 Failed，Ignore 通过 FaultResolution 记录；
- Node 当前投影可以显示 Ignored，但审计记录不能抹除原始失败。

## 5. Fault 与人工决策

```csharp
public sealed class RunFaultDto
{
    public string FaultId { get; set; } = string.Empty;
    public string RunId { get; set; } = string.Empty;
    public string NodeId { get; set; } = string.Empty;
    public string AttemptId { get; set; } = string.Empty;
    public StructuredErrorDto Error { get; set; } = new StructuredErrorDto();
    public FaultStatus Status { get; set; }
    public DateTimeOffset RaisedAt { get; set; }
    public FaultResolutionDto? Resolution { get; set; }
}
```

```text
FaultStatus:
unresolved
retrying
resolvedByRetry
ignored
terminated
```

Ignore 规则：

- 不提交失败 Attempt 的输出；
- 将节点当前投影标记为 Ignored；
- 解决所有 Fault 后进入 Paused；
- 用户通过 Resume 或 Step 继续；
- 后续使用未赋值变量时，BindingResolver 产生新的错误。

## 6. Variable Store

- Input Store 在 Run 创建后只读；
- Variable Store 由 RuntimeHost 管理；
- 每次写入是原子的；
- SetVariable 多 Assignment 和单节点多 OutputMapping 都以一个事务提交；
- Parallel 分支不创建变量副本；
- 并行写同一变量时，提交序号较后的值生效；
- 一次表达式求值读取它所依赖变量的一致性快照；
- 每次提交产生 `variable.changed` RunEvent。

## 7. API 通用约定

### 7.1 Base URL

```text
http://127.0.0.1:<port>/api/v1
```

第一阶段只监听回环地址。未来远程模式必须使用 HTTPS、身份认证和授权。

### 7.2 Content Type

```text
application/json; charset=utf-8
```

Workflow 导出也使用 JSON。未来脚本或大型附件上传再引入 multipart。

### 7.3 响应外形

- 成功时直接返回资源或结果 DTO；
- 不使用统一 `{ success, data, error }` 包装；
- 创建资源返回 `201 Created` 和 Location；
- 已接受但异步完成的控制命令返回 `202 Accepted`；
- 删除成功返回 `204 No Content`；
- 错误统一返回 ProblemDetails。

### 7.4 幂等命令

以下操作接受 `Idempotency-Key` 请求头：

- 创建 Run；
- Pause/Resume/Step/Terminate；
- Retry/Ignore Fault；
- Publish Workflow。

RuntimeHost 在有限保留期内保存 Key 与结果。相同 Key、相同请求返回原结果；相同 Key、不同请求返回 `409 Conflict`。

## 8. ProblemDetails

```json
{
  "type": "https://powerlab.dev/problems/workflow-validation-failed",
  "title": "Workflow validation failed",
  "status": 422,
  "detail": "The workflow contains 2 errors.",
  "instance": "/api/v1/workflows/f5415728-5115-451c-9dd3-80a068fe5153/publish",
  "code": "workflow.validationFailed",
  "traceId": "00-...",
  "validationIssues": []
}
```

状态码约定：

| HTTP | 用途 |
|---:|---|
| 400 | JSON、字段或基本请求格式错误 |
| 401/403 | 未认证或无权限；远程模式启用 |
| 404 | 资源不存在 |
| 409 | 当前状态不允许操作、设备未 Ready、幂等键冲突 |
| 422 | Workflow 语义校验失败 |
| 500 | 未处理的 RuntimeHost 故障 |

## 9. System API

```http
GET /api/v1/system/info
GET /api/v1/system/health
```

`SystemInfoDto`：

```json
{
  "runtimeInstanceId": "runtime-local",
  "runtimeVersion": "1.0.0",
  "apiVersion": "1.0",
  "workflowSchemaVersion": "1.0",
  "pluginSdkVersion": "1.0",
  "pythonVersion": "3.x.x",
  "platform": "win-x64",
  "startedAt": "2026-09-14T08:00:00Z"
}
```

Health endpoint只表达进程和关键基础设施健康，不将具体设备故障等同于 RuntimeHost 不健康。

## 10. Plugin 与 Node Type API

```http
GET /api/v1/plugins
GET /api/v1/plugins/{pluginId}
GET /api/v1/node-types
GET /api/v1/node-types/{pluginId}/{nodeTypeId}/versions/{nodeVersion}
```

列表支持：

```text
?category=Robot
?pluginId=com.vendor.robot
?search=move
```

Node Type 详情返回 `NodeDescriptorDto`，其 JSON 结构由 `workflow_data_contracts.md` 定义。

## 11. Device API

```http
GET    /api/v1/device-types
GET    /api/v1/devices
POST   /api/v1/devices
GET    /api/v1/devices/{deviceId}
PUT    /api/v1/devices/{deviceId}
DELETE /api/v1/devices/{deviceId}
GET    /api/v1/devices/{deviceId}/status
```

V1 工作流 API 不暴露 Connect/Disconnect 节点。设备连接触发和初始化属于 DeviceManager 管理功能，具体管理端点在确定设备运维需求后补充。

`DeviceSummaryDto`：

```json
{
  "deviceId": "device-robot-a",
  "displayName": "Robot A",
  "pluginId": "com.vendor.robot",
  "deviceTypeId": "com.vendor.robot/robot-v1",
  "state": "ready",
  "diagnosticMessage": null
}
```

敏感连接参数使用写入 DTO，读取 DTO 不回传明文 Secret。

## 12. Workflow API

```http
GET    /api/v1/workflows
POST   /api/v1/workflows
GET    /api/v1/workflows/{workflowId}
PUT    /api/v1/workflows/{workflowId}
DELETE /api/v1/workflows/{workflowId}

POST /api/v1/workflows/{workflowId}/validate
POST /api/v1/workflows/{workflowId}/publish
GET  /api/v1/workflows/{workflowId}/releases
GET  /api/v1/workflow-releases/{releaseId}

POST /api/v1/workflows/import
GET  /api/v1/workflows/{workflowId}/export
```

### 12.1 CreateWorkflowRequest

```json
{
  "name": "Temperature Control",
  "description": "Optional description"
}
```

返回 `201 Created`：

```json
{
  "workflowId": "f5415728-5115-451c-9dd3-80a068fe5153",
  "name": "Temperature Control",
  "description": "Optional description",
  "updatedAt": "2026-09-14T08:00:00Z",
  "document": {}
}
```

### 12.2 Save Workflow

`PUT` 请求体直接使用 WorkflowDocument。V1 不做协同编辑和 Revision 冲突检查。

Route 中的 workflowId 必须与 Document 中的 workflowId 一致，否则返回 `400 Bad Request`。

RuntimeHost 必须忽略或拒绝客户端伪造的数据库元数据，保存时间等字段由服务端生成。

### 12.3 ValidationReportDto

Validate 始终返回 `200 OK`：

```json
{
  "valid": false,
  "validatedAt": "2026-09-14T08:20:00Z",
  "issues": [
    {
      "severity": "error",
      "code": "device.notBound",
      "message": "Required device slot 'robot' is not bound.",
      "location": {
        "nodeId": "node-move",
        "deviceSlotId": "robot"
      }
    }
  ]
}
```

Publish 在存在 Error 时返回 `422 ProblemDetails`；Warning 不阻止发布。

### 12.4 WorkflowReleaseDto

```json
{
  "releaseId": "release-001",
  "workflowId": "f5415728-5115-451c-9dd3-80a068fe5153",
  "releaseNumber": 1,
  "publishedAt": "2026-09-14T08:30:00Z",
  "executionHash": "sha256:...",
  "requiredPlugins": [
    {
      "pluginId": "com.vendor.robot",
      "pluginVersion": "1.2.0",
      "nodeVersions": [1]
    }
  ],
  "document": {}
}
```

## 13. Run API

```http
GET  /api/v1/runs
POST /api/v1/runs
GET  /api/v1/runs/{runId}
GET  /api/v1/runs/{runId}/snapshot
GET  /api/v1/runs/{runId}/events?afterSequence=100
GET  /api/v1/run-queue

POST /api/v1/runs/{runId}/pause
POST /api/v1/runs/{runId}/resume
POST /api/v1/runs/{runId}/step
POST /api/v1/runs/{runId}/terminate

POST /api/v1/runs/{runId}/faults/{faultId}/retry
POST /api/v1/runs/{runId}/faults/{faultId}/ignore
```

### 13.1 CreateRunRequest

```json
{
  "releaseId": "release-001",
  "inputs": {
    "989fc012-5d6b-483f-9e13-9a9264b00e0a": 37.5
  },
  "mode": "normal"
}
```

Input Key 使用 WorkflowInputDefinition.id。

`mode`：

```text
normal  获得调度槽后自动运行
debug   获得调度槽并完成 Preparing 后停在 Paused，等待 Step/Resume
```

返回 `201 Created` 和 RunDto。

### 13.2 RunDto

```json
{
  "runId": "run-001",
  "releaseId": "release-001",
  "workflowId": "f5415728-5115-451c-9dd3-80a068fe5153",
  "status": "queued",
  "mode": "debug",
  "queuePosition": 1,
  "createdAt": "2026-09-14T09:00:00Z",
  "startedAt": null,
  "finishedAt": null,
  "currentWaveId": null,
  "unresolvedFaultCount": 0,
  "lastEventSequence": 0
}
```

### 13.3 RunSnapshotDto

```json
{
  "run": {},
  "inputs": {},
  "variables": {},
  "nodes": [],
  "activeAttempts": [],
  "faults": [],
  "lastEventSequence": 125
}
```

Snapshot 是断线重连后的权威读模型，不要求客户端通过重放所有历史事件自行还原状态。

### 13.4 控制命令

请求体可以为空；Idempotency-Key 放在 Header。返回 `202 Accepted`：

```json
{
  "commandId": "01K...",
  "runId": "run-001",
  "command": "pause",
  "acceptedAt": "2026-09-14T09:05:00Z",
  "statusAtAcceptance": "running"
}
```

HTTP 接受只表示命令进入 RuntimeHost，最终状态以 REST Snapshot 或 SignalR 事件为准。

### 13.5 Fault 决策

Retry：

```json
{
  "comment": "Device was checked and is ready."
}
```

Ignore：

```json
{
  "comment": "The missing measurement is acceptable for this run."
}
```

Comment 可选并进入审计记录。存在多个未解决 Fault 时按 FaultId 分别处理；全部解决后 Run 进入 Paused。

## 14. SignalR RuntimeHub

端点：

```text
/hubs/runtime
```

Hub 只提供订阅，不处理 Pause、Resume、Step、Terminate 等业务命令。

客户端能力：

```text
SubscribeRun(runId, afterSequence)
UnsubscribeRun(runId)
SubscribeRuntime()
UnsubscribeRuntime()
```

服务端统一发送：

```text
RuntimeEvent(RuntimeEventEnvelope event)
```

## 15. RuntimeEventEnvelope

```json
{
  "eventId": "event-001",
  "streamId": "run-001",
  "sequence": 126,
  "eventType": "node.stateChanged",
  "eventVersion": 1,
  "occurredAt": "2026-09-14T09:05:02Z",
  "correlationId": "run-001",
  "payload": {}
}
```

规则：

- RunEvent 的 Sequence 在单个 Run Stream 内严格递增；
- 事务提交状态和事件后才允许广播；
- SignalR 广播失败不回滚运行状态；
- 客户端检测到 Sequence 缺口时通过 REST 补取；
- 客户端必须忽略未知 EventType 或更高的兼容 EventVersion；
- 不在 SignalR 中传输不可恢复的唯一状态。

V1 事件类型：

```text
run.statusChanged
run.queueChanged
run.faultRaised
run.faultResolved
node.statusChanged
node.attemptStarted
node.attemptCompleted
variable.changed
log.appended
device.statusChanged
```

### 15.1 node.statusChanged

```json
{
  "nodeId": "node-read-temperature",
  "attemptId": "attempt-001",
  "status": "running",
  "waveId": "wave-003"
}
```

### 15.2 variable.changed

```json
{
  "variableId": "5d66f27f-a447-43a3-bfd8-cd0290d230bb",
  "valueType": "number",
  "value": 36.8,
  "sourceNodeId": "node-read-temperature",
  "attemptId": "attempt-001"
}
```

### 15.3 log.appended

```json
{
  "level": "information",
  "message": "Temperature read completed.",
  "nodeId": "node-read-temperature",
  "attemptId": "attempt-001",
  "properties": {
    "temperature": 36.8
  }
}
```

日志 Properties 必须是可序列化、安全且已脱敏的值，不能包含设备密码、访问令牌或任意插件对象。

## 16. 列表与分页

可能增长的列表使用 Cursor 分页：

```text
?pageSize=50
?cursor=<opaque-token>
```

返回：

```json
{
  "items": [],
  "nextCursor": null
}
```

Cursor 是不透明字符串，客户端不得解析。V1 默认 PageSize 为 50，最大 200。

## 17. API 安全基线

第一阶段：

- 默认只监听 127.0.0.1；
- WPF 使用 RuntimeHost 生成的本地访问令牌；
- 令牌不写入日志或 Workflow JSON；
- RuntimeHost 拒绝 Host Header 和 Origin 不符合本地策略的请求；
- OpenAPI UI 只在开发配置中启用；
- Workflow 中的 Python 源码视为可信代码，但不得通过日志完整回显。

未来远程模式：

- 强制 HTTPS；
- 使用标准身份认证；
- Workflow 编辑、发布、运行控制和设备管理使用不同授权策略；
- SignalR Hub 使用与 REST 相同的身份和 Run 访问检查；
- 明确配置 CORS，不能使用带凭据的任意 Origin。

## 18. Controller、Hub 与应用层

建议按功能切片组织：

```text
Features/
├── Workflows/
│   ├── WorkflowController
│   ├── Requests
│   ├── Responses
│   └── WorkflowApplicationService
├── Runs/
├── Plugins/
└── Devices/
```

- Controller 负责 HTTP 绑定、认证、授权和状态码；
- Application Service 负责用例与事务；
- Domain/Execution 负责业务语义；
- Persistence 负责 EF Core/SQLite；
- RuntimeHub 只订阅已提交事件并广播；
- BackgroundService 使用 `IDbContextFactory` 创建短生命周期数据上下文，不能长期持有请求 Scoped DbContext。

## 19. 暂不锁定的实现细节

以下内容不影响 V1 外部契约，可以在编码阶段通过实验确定：

- Expression Parser 的具体实现库；
- OpenAPI C# 与 TypeScript Client 生成器；
- SignalR 本地重连退避参数；
- SQLite 表和索引的物理布局；
- RuntimeHost 本机端口发现方式；
- WPF 图形画布控件实现。
