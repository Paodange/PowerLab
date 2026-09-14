# 自动化流程设计器方案总结

## 1. 总体方案

主流程采用 **“节点 + 连线”** 的方式表达，控制结构采用 **结构化容器节点（Structured Container Node）**，并辅以树形 Outline 进行层级导航。

核心原则：

- 不做完全自由的 Graph，而是做 **Structured Graph**
- 普通设备操作使用 `ActionNode`
- 条件、循环、并行使用带内部 Scope 的容器节点
- 子流程作为独立 Workflow 的调用，不与控制结构混淆
- 简单嵌套可原地展开，复杂嵌套进入独立子画布

---

## 2. 节点类型

| 类型 | 表达方式 |
|---|---|
| 普通设备动作 | `ActionNode` |
| 条件判断 | `IfNode` 容器 |
| 循环 | `LoopNode` 容器 |
| 并行 | `ParallelNode` 容器 |
| 子流程 | `SubWorkflowNode`，引用另一个 Workflow |
| 流程层级/导航 | Tree / Outline |

---

## 3. 控制结构设计

### 3.1 IfNode

```text
IfNode
├── Condition
├── TrueScope
│   └── Nodes + Edges
└── FalseScope
    └── Nodes + Edges
```

`IfNode` 本身负责条件判断语义，`TrueScope` 和 `FalseScope` 分别保存两个分支内部的子流程。

---

### 3.2 LoopNode

```text
LoopNode
├── Condition / Iterator
└── BodyScope
    └── Nodes + Edges
```

循环本身应该是明确的 Scope/Container，而不是依靠一条回路线来推导循环范围。

可以进一步支持：

- `While`
- `DoWhile`
- `Repeat`
- `ForEach`

对于实验室自动化，`ForEach Sample / Tube / Well / Column` 会比较实用。

---

### 3.3 ParallelNode

```text
ParallelNode
├── BranchScope 1
├── BranchScope 2
└── BranchScope N
```

每一个分支都是独立 Scope。

运行语义上类似多个分支同时执行，底层再结合设备资源锁和调度系统决定实际执行时机。

---

## 4. 统一的 ContainerNode 抽象

建议将 If、Loop、Parallel 统一抽象为：

```csharp
public abstract class ContainerNode : WorkflowNode
{
    public IReadOnlyList<WorkflowScope> Scopes { get; }
}
```

其中：

```text
ContainerNode
       │
       ├── Scope
       │      ├── Nodes[]
       │      └── Edges[]
       │
       └── Scope
              ├── Nodes[]
              └── Edges[]
```

不同节点的差异主要体现在控制语义以及 Scope 的数量上。

---

## 5. UI 表现

### 5.1 简单嵌套：Inline 展开

```text
A
│
▼
┌──── IF NeedWash ────────┐
│ Aspirate → Dispense     │
│              ↓          │
│             Mix         │
└─────────────────────────┘
│
▼
B
```

适合一层或两层、内容较少的结构。

---

### 5.2 复杂嵌套：Collapsed + Drill-down

主流程：

```text
A
↓
┌─────────────────────┐
│ IF NeedWash         │
│ 8 steps             │
│ Double click →      │
└─────────────────────┘
↓
B
```

双击进入该 Scope 的独立画布。

顶部通过 Breadcrumb 显示当前位置：

```text
Main Workflow
 > If NeedWash
 > Loop Samples
 > If HasCap
```

建议嵌套超过 **2～3 层** 后默认使用 Drill-down，而不是继续在同一画布中无限嵌套。

---

## 6. 数据模型

不建议将整个 Workflow 只保存成一个巨大的：

```text
Workflow
├── Nodes[]
└── Edges[]
```

建议：

```text
Workflow
└── RootScope
    ├── Nodes[]
    └── Edges[]
```

节点本身又可以拥有 Scope：

```text
RootScope
   │
   └── IfNode
         ├── TrueScope
         │     ├── Nodes[]
         │     └── Edges[]
         │
         └── FalseScope
               ├── Nodes[]
               └── Edges[]
```

整个模型本质上是：

> **Hierarchical Control Flow Graph**

也可以理解为：

> **Graph of Graphs**

---

## 7. SubWorkflow 与 ContainerNode 的区别

需要明确区分：

### If / Loop / Parallel

属于控制结构：

```text
If / Loop / Parallel
        ↓
ContainerNode + Scope
```

它们拥有自己的内部 Scope。

---

### SubWorkflow

属于流程调用：

```text
SubWorkflow
        ↓
CallNode + WorkflowReference
```

它本身不等于控制结构，而是引用另一个独立 Workflow。

类似代码中的：

```csharp
if (...)
{
    ...
}

while (...)
{
    ...
}
```

与：

```csharp
PrepareSample();
```

两者语义不同。

---

## 8. 推荐的最终模型

```text
Workflow
│
├── RootScope
│
├── ActionNode
│
├── IfNode
│    ├── TrueScope
│    └── FalseScope
│
├── LoopNode
│    └── BodyScope
│
├── ParallelNode
│    ├── BranchScope
│    └── BranchScope
│
└── SubWorkflowNode
     └── WorkflowReference
```

---

## 9. 推荐的设计器布局

```text
┌─────────────────────┬───────────────────────────────┬─────────────────────┐
│ Workflow / Scope    │ Node + Edge Designer          │ PropertyGrid        │
│ Tree / Outline      │                               │                     │
│                     │                               │                     │
│ Main                │        Main Workflow          │ Selected Node       │
│ ├ If NeedWash       │                               │ Properties          │
│ ├ Loop Samples      │                               │                     │
│ └ SubWorkflow       │                               │                     │
└─────────────────────┴───────────────────────────────┴─────────────────────┘
```

---

## 10. 一句话总结

> 使用结构化的节点图作为主设计器；`If`、`Loop`、`Parallel` 都是拥有内部 `Scope` 的 `ContainerNode`；简单结构原地展开，复杂结构双击进入子画布；`SubWorkflow` 单独作为流程调用处理，不与控制结构混淆。

---

# 节点参数表达式方案

## 11. 总体建议

节点参数需要支持动态计算时，不建议直接开放完整 Python 或完整 C# 代码。

推荐方案：

> **定义一套受控的 C#-like Expression DSL，用于参数计算；复杂逻辑则通过独立的 Python Script Node 处理。**

核心原则：

- 表达式只做“计算”，不做“动作”
- 表达式必须是 **Pure / Deterministic / Side-effect-free**
- 支持设计期类型检查、依赖分析和自动补全
- 表达式与 Workflow 控制流、脚本能力明确分层

整体能力分层：

```text
              Workflow
                  │
        ┌─────────┼─────────┐
        ↓         ↓         ↓
    Expression   Node    Python Script
        │         │          │
简单参数计算   流程控制    高级自定义逻辑
        │         │          │
无副作用      If/Loop     明确 Inputs/Outputs
强类型        Parallel
```

---

## 12. Python 表达式还是 C# 表达式

如果必须在 Python 表达式和 C# 表达式之间选择，推荐 **C# 风格表达式**。

原因：

| 维度 | Python 表达式 | C# 风格表达式 | 推荐判断 |
|---|---|---|---|
| 与 .NET Workflow Engine 集成 | 一般，需要 Python Runtime | 自然 | C# 更优 |
| 静态类型检查 | 较弱 | 较强 | C# 更优 |
| 参数类型验证 | 偏运行时 | 更适合设计期检查 | C# 更优 |
| 与 `ExpressionProperty<T>` 集成 | 可以 | 非常自然 | C# 更优 |
| 用户输入简洁度 | 较好 | 稍复杂 | Python 略优 |
| 安全性 | 完整语言风险高 | 完整语言风险同样高 | 都不应直接开放 |

因此推荐：

> **语法采用 C#-like，但实现上不是完整 C# Language，而是一套受控 DSL。**

例如：

```text
Source.Volume * Variables.DilutionRatio
```

设计器应该能够在运行前知道：

```text
Source.Volume                : double
Variables.DilutionRatio     : double
Expression Result           : double
Target Property             : double
```

从而在 Design-time 就完成类型校验。

---

## 13. 第一版建议支持的表达式能力

### 13.1 常量

```text
100
10.5
true
false
"ABC"
null
```

---

### 13.2 算术运算

```text
A + B
A - B
A * B
A / B
A % B
```

支持括号：

```text
(A + B) * C
```

---

### 13.3 比较运算

```text
A > B
A >= B
A < B
A <= B
A == B
A != B
```

---

### 13.4 逻辑运算

```text
A && B
A || B
!A
```

例如：

```text
Sample.Volume > 10 && MeasurePH.Output.PH < 7
```

---

### 13.5 条件表达式

建议支持 C# 风格三元表达式：

```text
condition ? value1 : value2
```

例如：

```text
Sample.Type == "Control" ? 50 : 100
```

参数级差异推荐使用表达式处理，而不是总是增加 `IfNode`。

推荐边界：

```text
参数级条件
    ↓
Expression

流程级条件
    ↓
IfNode
```

---

### 13.6 Null 处理

建议支持：

```text
A ?? DefaultValue
```

例如：

```text
Measure.Output.Value ?? 0
```

是否支持 `?.` 可以作为后续增强项。

---

## 14. 上下文和变量引用

表达式最核心的能力不是运算符，而是明确“允许引用什么”。

建议至少支持以下几类上下文。

### 14.1 Workflow Variables

```text
Variables.TargetVolume
Variables.DilutionRatio
Variables.SampleCount
```

---

### 14.2 当前执行上下文

```text
Context.SampleCount
Context.BatchId
```

具体暴露哪些字段由平台白名单定义。

---

### 14.3 当前循环上下文

```text
Loop.Index
Loop.Item
Loop.Item.Name
Loop.Item.Volume
```

例如：

```text
Loop.Index + 1
```

---

### 14.4 当前资源 / 当前对象

例如：

```text
CurrentSample.Volume
CurrentTube.Name
Source.Volume
Target.Position
```

---

### 14.5 前置节点输出

建议允许显式引用节点输出：

```text
MeasurePH.Output.PH
WeighNode.Output.Weight
ReadBarcode.Output.Value
```

例如：

```text
MeasurePH.Output.PH < 7.0
```

这种能力后续还可以用于建立节点之间的隐式数据依赖关系。

---

## 15. 白名单函数

建议提供一套受控的 Pure Function，而不是允许任意方法调用。

### 15.1 数学函数

建议第一版支持：

```text
min(a, b)
max(a, b)
abs(x)
round(x)
floor(x)
ceil(x)
clamp(x, min, max)
```

例如：

```text
min(Source.Volume, 100)
```

```text
round(TargetVolume / 8)
```

---

### 15.2 字符串函数

可以提供少量常用函数：

```text
contains(text, value)
startsWith(text, value)
endsWith(text, value)
toUpper(text)
toLower(text)
```

例如：

```text
contains(Sample.Name, "Control")
```

---

### 15.3 Collection 能力

第一版建议仅支持基础访问：

```text
Samples.Count
Samples[0]
Samples[index]
```

后续可以再增加：

```text
sum(...)
average(...)
any(...)
all(...)
```

不建议第一版直接支持 LINQ / Lambda，例如：

```csharp
Samples.Where(x => x.Volume > 10).Select(x => x.Name)
```

否则表达式系统会快速演变成一门完整编程语言。

---

## 16. 明确禁止的能力

### 16.1 禁止赋值

禁止：

```csharp
A = 10
```

允许：

```text
A + 10
```

表达式不能主动修改任何系统状态。

---

### 16.2 禁止流程语句

禁止：

```csharp
if (...) { }
for (...) { }
while (...) { }
```

对应能力分别交给：

```text
IfNode
LoopNode
```

表达式只保留：

```text
condition ? a : b
```

---

### 16.3 禁止任意对象创建

禁止：

```csharp
new SomeObject()
```

---

### 16.4 禁止任意方法调用

禁止：

```csharp
Device.Move(...)
Resource.Delete(...)
Database.Save(...)
Http.Get(...)
File.Read(...)
```

---

### 16.5 禁止 Reflection / Runtime 动态能力

禁止：

```csharp
GetType()
Assembly.Load(...)
Activator.CreateInstance(...)
```

---

### 16.6 禁止 IO / HTTP / DB 操作

表达式不能访问：

- 文件系统
- 网络
- 数据库
- 设备 API
- 系统进程
- Runtime Reflection

表达式应严格保持：

> **Pure + Deterministic + Side-effect-free**

---

## 17. Expression 与 Python Script Node 分层

当用户确实需要复杂算法或数据处理时，不应该继续扩展 Expression DSL，而应提供独立的 `Python Script Node`。

例如：

```text
┌───────────────────────────┐
│ Python Script             │
│                           │
│ Inputs                    │
│ ├ Samples                 │
│ └ Threshold               │
│                           │
│ Outputs                   │
│ └ Result                  │
└───────────────────────────┘
```

脚本可以显式声明：

```text
Input:
    samples: Sample[]
    threshold: double

Output:
    result: string[]
```

这样形成清晰边界：

| 能力 | 用途 |
|---|---|
| Expression | 简单参数计算 |
| Workflow Node | 控制流程和设备动作 |
| Python Script Node | 高级自定义算法 |

---

## 18. 表达式执行架构

不建议直接使用：

```csharp
Eval(Expression)
```

推荐建立完整的 Expression Pipeline：

```text
Expression String
      ↓
    Lexer
      ↓
    Parser
      ↓
     AST
      ↓
 Type Checking
      ↓
Dependency Analysis
      ↓
Compiled Expression / AST
      ↓
Evaluate(ExpressionContext)
```

核心原则：

> **Parse once, evaluate many times.**

流程加载或表达式修改时完成 Parse / Compile；运行过程中直接执行已经解析好的 AST 或编译结果。

---

## 19. 静态类型检查

对于：

```csharp
ExpressionProperty<double> Volume
```

表达式：

```text
Source.Volume * Variables.Ratio
```

设计期应完成：

```text
Source.Volume         : double
Variables.Ratio      : double
Expression Result    : double
Expected Type        : double

✓ Valid
```

如果用户输入：

```text
Sample.Name + 10
```

则设计器应该立即提示类型错误，而不是运行时才失败。

例如：

```text
Cannot apply '+' to string and double.
```

---

## 20. 静态依赖分析

建议 Expression Parser 在构建 AST 后，同时提取 Dependencies。

例如：

```text
Source.Volume * Variables.Ratio
```

得到：

```text
Dependencies:
- Source.Volume
- Variables.Ratio
```

例如：

```text
MeasurePH.Output.PH < 7
```

得到：

```text
Dependencies:
- MeasurePH.Output.PH
```

依赖信息可以用于：

- Design-time Validation
- Simulation
- Dependency Graph
- 变量重命名
- 自动补全
- 节点删除检查
- 节点输出可用性检查
- AI Workflow Generation
- 运行前预检查

例如可以检测：

```text
节点 B 使用 MeasurePH.Output.PH
但 MeasurePH 在某些执行路径上不一定先执行
```

并提示：

```text
MeasurePH.Output.PH may not be available
before node "AddBase" executes.
```

---

## 21. 与 Simulation 的关系

Expression 应该由 Simulation 与真实运行共同使用同一个 Evaluator。

即：

```text
Input State
    ↓
Expression Evaluator
    ↓
Result
```

由于表达式无副作用，因此 Simulation 中不会出现：

- 修改真实数据库
- 调用真实设备
- 修改实际资源状态
- 发起网络请求

从而更容易保证：

> **Simulation 与 Runtime 的表达式计算语义一致。**

---

## 22. ExpressionProperty<T> 建议模型

可以继续保持当前类似结构：

```csharp
public class ExpressionProperty<T>
{
    public bool UseExpression { get; set; }

    public T? Value { get; set; }

    public string? Expression { get; set; }
}
```

但建议内部增加编译结果和依赖缓存，例如概念上可以扩展为：

```text
ExpressionProperty<T>
├── UseExpression
├── Value
├── Expression
├── ParsedExpression / CompiledExpression
├── Dependencies
├── ValidationResult
└── ResultType
```

`Expression` 改变时重新 Parse / Validate，而不是每次运行时重新解析字符串。

---

## 23. UI 设计建议

PropertyGrid 中建议明确区分 Constant 和 Expression。

例如：

```text
Volume

○ Constant
● Expression

┌────────────────────────────────────────────┐
│ Source.Volume * Variables.Ratio            │
└────────────────────────────────────────────┘
```

设计器应支持智能提示。

输入：

```text
Source.
```

显示：

```text
Volume        double
Name          string
Position      Position
Remaining     double
```

输入：

```text
Variables.
```

显示：

```text
TargetVolume
DilutionRatio
SampleCount
```

同时实时显示校验状态：

```text
Expression Type: double
Expected Type:   double

✓ Valid
```

出现错误时在设计期直接提示。

---

## 24. 第一版推荐范围

第一版建议控制在以下范围：

| 能力 | 第一版 | 后续 |
|---|---:|---:|
| 常量 | ✅ | |
| `+ - * / %` | ✅ | |
| 比较运算 | ✅ | |
| `&& \|\| !` | ✅ | |
| 括号 | ✅ | |
| Workflow Variables | ✅ | |
| Runtime Context | ✅ | |
| Loop Context | ✅ | |
| Node Output | ✅ | |
| `?:` | ✅ | |
| `??` | ✅ | |
| 数学函数 | ✅ | |
| 少量字符串函数 | ✅ | 扩展 |
| Array Index | ✅ | |
| Collection Count | ✅ | |
| `sum / average / any / all` | | ✅ |
| Lambda | ❌ | 建议仍不支持 |
| Assignment | ❌ | ❌ |
| Statement | ❌ | ❌ |
| Loop Statement | ❌ | ❌ |
| Arbitrary Method | ❌ | ❌ |
| IO / HTTP / DB | ❌ | ❌ |
| Reflection | ❌ | ❌ |

第一版推荐的实际能力可以进一步压缩为：

> **变量引用 + 节点输出引用 + 算术 / 比较 / 逻辑 + `?:` + `??` + 10～20 个白名单函数。**

这一范围已经足够覆盖大多数自动化设备参数计算场景。

---

## 25. 表达式方案一句话总结

> **节点参数表达式采用受控的 C#-like Expression DSL，而不是完整 Python/C#；表达式只负责强类型、无副作用的参数计算，支持变量、运行上下文、节点输出、基础运算和白名单纯函数；复杂逻辑交给独立 Python Script Node；底层通过 AST、类型检查和依赖分析实现 Design-time Validation、Simulation 和 Runtime 共用。**
