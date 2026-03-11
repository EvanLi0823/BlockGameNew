# 常用工具命令参考

## Unity MCP工具

### 刷新和编译

```csharp
// 强制刷新Unity项目并请求编译
mcp__UnityMCP__refresh_unity(compile: "request", mode: "force")
```

**参数说明**：
- `compile`: `"request"` - 请求编译
- `mode`: `"force"` - 强制刷新

**使用场景**：
- 修改代码后立即编译验证
- 添加新文件后刷新项目
- 修复编译错误后重新编译

### 读取控制台

```csharp
// 读取Unity控制台的错误和警告
mcp__UnityMCP__read_console(types: ["error", "warning"])

// 只读取错误
mcp__UnityMCP__read_console(types: ["error"])

// 读取所有消息
mcp__UnityMCP__read_console(types: ["error", "warning", "log"])
```

**参数说明**：
- `types`: 消息类型数组
  - `"error"` - 错误消息
  - `"warning"` - 警告消息
  - `"log"` - 普通日志

**使用场景**：
- 编译后检查错误
- 验证警告是否修复
- 查看运行时日志

### 清空控制台

```csharp
// 清空Unity控制台
mcp__UnityMCP__read_console(action: "clear")
```

**使用场景**：
- 开始新一轮测试前清空日志
- 编译前清空旧消息

### 标准开发验证流程

```bash
# 1. 修改代码后立即刷新Unity
mcp__UnityMCP__refresh_unity(compile: "request", mode: "force")

# 2. 检查编译错误
mcp__UnityMCP__read_console(types: ["error", "warning"])

# 3. 如果有错误，立即修复后再次编译
# 4. 重复直到没有错误

# 5. 清空控制台，准备下一轮
mcp__UnityMCP__read_console(action: "clear")
```

## 代码搜索命令

### 查找类定义

```bash
# 查找指定类的定义
grep -r "class ClassName" Assets/BlockPuzzleGameToolkit/Scripts --include="*.cs"

# 查找继承特定基类的类
grep -r "class.*: .*BaseClass" Assets/BlockPuzzleGameToolkit/Scripts --include="*.cs"

# 查找所有Manager类
grep -r "class.*Manager" Assets/BlockPuzzleGameToolkit/Scripts --include="*.cs"
```

### 查找命名空间

```bash
# 查找类型所在的命名空间
grep "namespace" TargetFile.cs

# 查找所有使用特定命名空间的文件
grep -r "using GameCore" Assets/BlockPuzzleGameToolkit/Scripts --include="*.cs"
```

### 查找方法和属性

```bash
# 查找公共API
grep "public" TargetClass.cs | grep -v "class"

# 查找方法调用
grep -r "\.MethodName\(" Assets/BlockPuzzleGameToolkit/Scripts --include="*.cs"

# 查找属性访问
grep -r "\.PropertyName" Assets/BlockPuzzleGameToolkit/Scripts --include="*.cs"
```

### 查找单例Instance

```bash
# 查找所有单例引用
grep -r "\.Instance" Assets/BlockPuzzleGameToolkit/Scripts --include="*.cs"

# 查找特定Manager的Instance使用
grep -r "ManagerName\.Instance" Assets/BlockPuzzleGameToolkit/Scripts --include="*.cs"
```

### 查找方法重写

```bash
# 查找重写的方法
grep -r "override.*MethodName" Assets/BlockPuzzleGameToolkit/Scripts --include="*.cs"

# 查找所有重写的Awake方法
grep -r "override.*Awake" Assets/BlockPuzzleGameToolkit/Scripts --include="*.cs"
```

### 查找事件订阅

```bash
# 查找事件订阅
grep -r "EventManager.Subscribe" Assets/BlockPuzzleGameToolkit/Scripts --include="*.cs"

# 查找事件发布
grep -r "EventManager.Publish" Assets/BlockPuzzleGameToolkit/Scripts --include="*.cs"
```

## 文件和目录操作

### 列出目录结构

```bash
# 列出Scripts目录的第一层
ls Assets/BlockPuzzleGameToolkit/Scripts

# 列出所有子目录
find Assets/BlockPuzzleGameToolkit/Scripts -type d -maxdepth 2

# 使用tree命令（如果可用）
tree Assets/BlockPuzzleGameToolkit/Scripts -L 2
```

### 查找文件

```bash
# 按名称查找文件
find Assets/BlockPuzzleGameToolkit/Scripts -name "ClassName.cs"

# 查找所有Manager文件
find Assets/BlockPuzzleGameToolkit/Scripts -name "*Manager.cs"

# 查找配置文件
find Assets/BlockPuzzleGameToolkit/Resources/Settings -name "*.asset"
```

### 统计代码行数

```bash
# 统计C#代码行数
find Assets/BlockPuzzleGameToolkit/Scripts -name "*.cs" | xargs wc -l

# 统计特定目录的代码行数
wc -l Assets/BlockPuzzleGameToolkit/Scripts/GameCore/*.cs
```

## Git操作

### 查看状态

```bash
# 查看当前状态
git status

# 查看修改的文件
git diff --name-only

# 查看具体修改
git diff FileName.cs
```

### 查看提交历史

```bash
# 查看最近的提交
git log -10 --oneline

# 查看特定文件的历史
git log --oneline -- FileName.cs

# 查看提交详情
git show CommitHash
```

### 分支操作

```bash
# 查看当前分支
git branch

# 创建新分支
git checkout -b feature/new-feature

# 切换分支
git checkout main
```

## 常用组合命令

### 验证API调用

```bash
# 步骤1：查找类定义
grep -r "class TargetClass" Assets/BlockPuzzleGameToolkit/Scripts --include="*.cs"

# 步骤2：查看公共API
grep "public" TargetFile.cs | grep -v "class"

# 步骤3：查找命名空间
grep "namespace" TargetFile.cs

# 步骤4：查找所有调用处
grep -r "TargetClass\.MethodName" Assets/BlockPuzzleGameToolkit/Scripts --include="*.cs"
```

### 修改方法签名

```bash
# 步骤1：找到所有重写的方法
grep -r "override.*MethodName" Assets/BlockPuzzleGameToolkit/Scripts --include="*.cs"

# 步骤2：找到所有调用处
grep -r "\.MethodName\(" Assets/BlockPuzzleGameToolkit/Scripts --include="*.cs"

# 步骤3：查看每个文件的命名空间
grep "namespace" TargetFile.cs

# 步骤4：修改所有文件后立即编译验证
mcp__UnityMCP__refresh_unity(compile: "request", mode: "force")
```

### 分析Manager依赖

```bash
# 步骤1：找到所有Manager类
grep -r "class.*Manager.*:" Assets/BlockPuzzleGameToolkit/Scripts --include="*.cs"

# 步骤2：查找Manager之间的相互引用
grep -r "Manager\.Instance" Assets/BlockPuzzleGameToolkit/Scripts --include="*.cs"

# 步骤3：查找初始化顺序
grep -r "InitializationOrder" Assets/BlockPuzzleGameToolkit/Scripts --include="*.cs"
```

## 工具使用最佳实践

### 1. 增量开发流程

```
修改代码 → 刷新Unity → 检查错误 → 修复错误 → 重复
```

**每5-10行代码编译一次**：
```bash
# 修改文件后
mcp__UnityMCP__refresh_unity(compile: "request", mode: "force")

# 立即检查
mcp__UnityMCP__read_console(types: ["error"])

# 如果有错误，立即修复
# 修复后再次编译
```

### 2. 批量修改验证

```bash
# 修改前：查找所有需要修改的地方
grep -r "OldName" Assets/BlockPuzzleGameToolkit/Scripts --include="*.cs"

# 修改后：验证是否还有遗漏
grep -r "OldName" Assets/BlockPuzzleGameToolkit/Scripts --include="*.cs"

# 立即编译验证
mcp__UnityMCP__refresh_unity(compile: "request", mode: "force")
```

### 3. API调用验证

**开发前必执行**：
```bash
grep "public" TargetClass.cs | grep -v "class"
grep "namespace" TargetFile.cs
```

**确保**：
- API签名正确
- 命名空间已导入
- 参数类型匹配

### 4. 错误诊断流程

```bash
# 1. 读取错误
mcp__UnityMCP__read_console(types: ["error"])

# 2. 定位错误文件和行号
# 从错误消息中找到文件名和行号

# 3. 查看上下文
# 使用Read工具读取文件，查看前后30-50行

# 4. 查找相关代码
grep -n "ErrorKeyword" TargetFile.cs

# 5. 修复后验证
mcp__UnityMCP__refresh_unity(compile: "request", mode: "force")
mcp__UnityMCP__read_console(types: ["error"])
```

## 快速参考表

| 任务 | 命令 |
|------|------|
| **刷新Unity** | `mcp__UnityMCP__refresh_unity(compile: "request", mode: "force")` |
| **检查错误** | `mcp__UnityMCP__read_console(types: ["error"])` |
| **清空控制台** | `mcp__UnityMCP__read_console(action: "clear")` |
| **查找类** | `grep -r "class ClassName" Scripts --include="*.cs"` |
| **查找方法** | `grep -r "\.MethodName\(" Scripts --include="*.cs"` |
| **查找命名空间** | `grep "namespace" TargetFile.cs` |
| **查找单例** | `grep -r "\.Instance" Scripts --include="*.cs"` |
| **查找重写** | `grep -r "override.*MethodName" Scripts --include="*.cs"` |
| **查看Git状态** | `git status` |
| **查看修改** | `git diff FileName.cs` |
