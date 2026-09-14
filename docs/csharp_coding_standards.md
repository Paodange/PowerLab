# PowerLab C# 编码规范

状态：V1 强制规范

## 1. 语言版本和运行时

- 项目可以面向 .NET 10，以使用当前运行时、标准库和 ASP.NET Core 能力。
- 所有手写与生成的 C# 源码必须使用 C# 8.0 语法。
- 根目录 `Directory.Build.props` 统一设置 `LangVersion=8.0`、启用可空引用类型并关闭隐式 `using`。
- 子项目不得自行提高 `LangVersion`。确需改变时，必须先形成架构决策记录（ADR）。

运行时版本和语言版本是两个独立概念：面向 .NET 10 不代表必须使用较新的 C# 语法。

## 2. 禁止使用的较新语法

不得使用 C# 9.0 及之后引入的语法，包括但不限于：

- `record`、`record struct`；
- `init` 访问器；
- 顶级语句；
- 目标类型推断的 `new()`；
- `with` 表达式；
- 文件范围命名空间；
- `global using`；
- `required` 成员；
- 原始字符串字面量；
- 列表模式和扩展属性模式；
- 主构造函数；
- 集合表达式。

例如：

```csharp
// 不允许
public sealed record StartRunRequest(string WorkflowId);

// 允许
public sealed class StartRunRequest
{
    public string WorkflowId { get; set; } = string.Empty;
}
```

```csharp
// 不允许
Dictionary<string, string> values = new();

// 允许
Dictionary<string, string> values = new Dictionary<string, string>();
```

## 3. 可以使用的 C# 8.0 能力

可以使用：

- 可空引用类型；
- switch 表达式；
- 属性模式；
- using 声明和 `await using`；
- 异步流；
- `Index`、`Range` 和 `??=`。

C# 8.0 支持默认接口实现，但公共插件契约不使用它。插件接口的演进通过新增接口或提高契约主版本完成，避免旧插件在运行时出现难以诊断的行为差异。

## 4. 文件与类型

- 使用块级命名空间，每个文件显式声明需要的 `using`。
- 一个文件原则上只放一个公开类型，文件名与公开类型名一致。
- DTO、Descriptor 和配置模型使用普通 `class`；不依赖 `record` 的值相等语义。
- 确实需要值相等时，显式实现 `IEquatable<T>`、`Equals` 和 `GetHashCode`。
- 默认将不需要继承的实现类声明为 `sealed`。

## 5. 命名

- 命名空间、类型、公开成员和常量使用 `PascalCase`。
- 参数和局部变量使用 `camelCase`。
- 私有实例字段使用 `_camelCase`。
- 异步方法使用 `Async` 后缀；事件处理器和接口规定的方法除外。
- 接口以 `I` 开头；抽象类型名称表达职责，不使用无含义的 `Base`。

## 6. 异步、取消和暂停

- 可能阻塞或执行 I/O 的操作使用异步 API，不以 `Task.Run` 包装同步 I/O。
- 可取消的异步方法接收 `CancellationToken`，通常作为最后一个参数。
- RuntimeHost、节点执行器和设备能力调用不得使用 `.Result`、`.Wait()` 等同步等待。
- 节点暂停使用框架提供的暂停令牌或检查点协议；不得把暂停伪装成取消。
- 库代码默认使用 `ConfigureAwait(false)`；界面代码根据 WPF 上下文决定是否返回 UI 线程。

## 7. 异常与结果

- 异常用于不可继续的失败，不用于普通分支控制。
- 捕获异常时保留原异常作为 `InnerException`，不得无信息吞掉异常。
- 跨 REST、SignalR 和插件边界传递稳定的错误码、用户可读消息和结构化详情，不直接暴露异常对象。
- 参数校验错误、设备未就绪、设备掉线和节点执行失败使用不同的错误码类别。

## 8. 契约和序列化

- 领域模型、持久化模型、REST DTO 和 SignalR 事件 DTO 分离；不要直接把数据库实体作为外部契约。
- JSON 字段使用既定的 `camelCase` 名称，枚举按契约约定序列化为字符串。
- 新增可选字段应保持向后兼容；删除或改变既有字段含义需要提高契约版本。
- 插件 SDK 的公共类型避免暴露具体容器、ORM、UI 或宿主实现类型。

## 9. 生成代码

OpenAPI 客户端、EF Core migration、SDK 模板和源生成器的输出也必须能在 `LangVersion=8.0` 下编译。引入生成工具前，应验证其是否可以关闭 `record`、`init`、文件范围命名空间等新语法；无法配置时，不能直接纳入构建。

## 10. 构建检查

- 本地构建和 CI 都从仓库根目录执行，确保自动导入 `Directory.Build.props`。
- 合并前至少执行一次完整编译；编译器是语言版本限制的最终检查。
- 使用 `dotnet format` 时遵循根目录 `.editorconfig`，但格式化不得改写用户未涉及的代码。
