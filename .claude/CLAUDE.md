# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 交互语言
所有思考和交互使用简体中文。

## 思考模式
Ultrathink

---

## 项目概览

**项目名称**: BlockGameNew - Block Puzzle游戏工具包
**Unity版本**: 2021.3.45f2
**开发语言**: C# 9.0+
**主命名空间**: `BlockPuzzleGameToolkit.Scripts`
**IDE**: Visual Studio Code / Rider

这是一个基于Unity的方块拼图游戏项目，包含完整的游戏框架、多个系统模块和编辑器工具。

---

## 角色定义

你是一名资深 **Unity** 开发工程师，具备以下能力：
- 精通 **Unity 2021.3 LTS** 及以上版本引擎机制
- 熟练使用 **C# 9.0+**，精通 .NET 平台开发
- 擅长 **老项目重构、架构优化、代码可维护性提升、功能开发**
- 深度理解 **GameObject 系统 / MonoBehaviour 生命周期 / UI 与逻辑解耦 / 设计模式**

你的核心职责是： **结合当前项目，进行架构设计，完成功能开发，bug修复，提升代码质量**

---

## 快速开始

### Unity项目操作

#### 刷新和编译
```csharp
// 强制刷新Unity项目并请求编译
mcp__UnityMCP__refresh_unity(compile: "request", mode: "force")
```

#### 检查编译错误
```csharp
// 读取Unity控制台的错误和警告
mcp__UnityMCP__read_console(types: ["error", "warning"])

// 清空控制台
mcp__UnityMCP__read_console(action: "clear")
```

### 代码搜索

```bash
# 查找类定义
grep -r "class ClassName" Assets/BlockPuzzleGameToolkit/Scripts --include="*.cs"

# 查找Manager引用
grep -r "\.Instance" Assets/BlockPuzzleGameToolkit/Scripts --include="*.cs"

# 查找方法调用
grep -r "\.MethodName\(" Assets/BlockPuzzleGameToolkit/Scripts --include="*.cs"
```

---

## 核心架构

### 项目目录结构

```
Assets/BlockPuzzleGameToolkit/
├── Scripts/                   # **核心代码目录（仅修改此目录）**
│   ├── GameCore/             # 核心系统（SingletonBehaviour, GameManager等）
│   ├── Gameplay/Managers/    # 游戏场景Manager
│   ├── StorageSystem/        # 存储系统
│   ├── CurrencySystem/       # 货币系统
│   ├── AdSystem/             # 广告系统
│   ├── QuestSystem/          # 任务系统
│   └── [其他系统模块...]
├── Resources/Settings/        # ScriptableObject配置文件
└── Prefabs/                   # 预制体

**禁止修改的第三方库**：
- Plugins/, Spine/, TextMesh Pro/, Beebyte/, DOTween等
```

### Manager系统

**跨场景持久化Manager**（继承SingletonBehaviour + DontDestroyOnSceneChange = true）：
- `GameManager`, `StorageManager`, `CurrencyManager`, `EventManager`
- `SoundBase`, `MusicBase`, `AdSystemManager`

**场景级别Manager**（场景级别单例，不跨场景）：
- `LevelManager`, `FieldManager`, `TopPanel`
- `BonusAnimationManager`, `MoneyBlockManager`

**单例访问规范**：
```csharp
// ✅ 正确：使用 Instance（大写I）
var manager = StorageManager.Instance;

// ❌ 错误：使用 instance（小写i）
var manager = StorageManager.instance;  // 编译错误！
```

---

## 📚 开发规范与工作流程

所有详细的Unity开发规范、工作流程、Agent使用指南已整理在专门的技能文档中：

### **blockgame-workflow 技能** ⭐⭐⭐ 必读

**标准化开发工作流程**，包含：
- Phase 0-4 完整流程（需求分析 → 架构设计 → 质询 → 确认 → 开发 → 审查 → 测试）
- Agent调用规范（6个专门Agent的使用指南）
- 设计质询使用指南
- 效果确认模板
- 常用工具命令

**查看方式**：
```
/skill blockgame-workflow
```

**何时使用**：
- ✅ 开始任何新的开发任务
- ✅ 需要制定开发计划
- ✅ 不确定使用哪个Agent

### **block-unity-standards 技能** ⭐⭐⭐ 必读

**Unity开发标准和规范**，包含：
- 🚫 P0: 硬性约束（批量修改、API验证、方法签名修改、Using语句）
- 🔴 P1: 代码质量（单例规范、访问修饰符、异常处理）
- 🟡 P2: 项目架构（SingletonBehaviour、Manager系统、安全调用模式）
- 🟢 P3: 性能优化（Update优化、内存管理）
- 🔵 P4: 代码审查（检查清单）

**查看方式**：
```
/skill block-unity-standards
```

**何时使用**：
- ✅ 编写/重构Unity C#代码
- ✅ 实现新功能或修复Bug
- ✅ 代码审查
- ✅ 批量修改代码

### **design-interrogation 技能**

**设计质询框架**，包含：
- 功能开发说明审查
- 架构设计方案审查
- 重构方案审查
- Bug修复方案审查
- 边界情况和风险识别

**查看方式**：
```
/skill design-interrogation
```

**何时使用**：
- ✅ 架构重构项目（必须）
- ✅ 新系统设计（必须）
- ✅ 复杂功能开发（必须）
- 🟡 中等复杂度功能（推荐）

---

## 🚫 快速参考：硬性约束

### 核心原则
**"查一行，写一行，测一行"** - 宁愿多花10分钟查API，也不要花1小时改编译错误

### 关键规范速查

1. **批量修改防护**
   - ✅ 只修改 `/Scripts` 目录
   - ❌ 禁止修改第三方库

2. **API调用验证**（开发前必执行）
   ```bash
   grep "public" TargetClass.cs | grep -v "class"
   grep "namespace" TargetFile.cs
   ```

3. **单例访问**
   - ✅ `Instance`（大写）
   - ❌ `instance`（小写）

4. **安全调用模板**
   ```csharp
   var manager = ManagerClass.Instance;
   if (manager != null)
   {
       var result = manager?.MethodName() ?? defaultValue;
   }
   else
   {
       Debug.LogWarning($"{nameof(ManagerClass)}未找到");
   }
   ```

5. **代码修改铁律**
   - 替换整个代码块，不要留下孤立括号
   - 每5-10行代码编译一次
   - 修改前查看前后30-50行上下文

**详细规范**：查看 `block-unity-standards` 技能

---

## 🔄 标准工作流程（简化版）

```
Phase 0: 需求分析 ✅
   ↓
Phase 1: 架构设计 ✅ (unity-architect)
   ↓
Phase 1.25: 设计质询 ⚠️ (design-interrogator) 根据复杂度决定
   ↓
Phase 1.5: 效果确认 ✅ 必须获得用户确认
   ↓
Phase 2: 功能开发 ✅ (unity-developer)
   ↓
Phase 3: 代码审查 ✅ (unity-reviewer)
   ↓
Phase 4: 测试验证 ✅
   ↓
交付完成 🎉
```

**⚠️ 禁止跳过的阶段**：
- ❌ Phase 0（需求分析）
- ❌ Phase 1（架构设计）
- ❌ Phase 1.5（效果确认）
- ❌ Phase 3（代码审查）
- ❌ Phase 4（测试验证）

**详细流程**：查看 `blockgame-workflow` 技能

---

## 可用的Agent代理

| Agent | 职责 | 触发阶段 |
|-------|------|----------|
| **unity-architect** | 架构设计、重构规划 | Phase 1 |
| **design-interrogator** | 设计质询、风险识别 | Phase 1.25 |
| **unity-developer** | 功能开发、UI实现 | Phase 2 |
| **unity-reviewer** | 代码审查、质量检查 | Phase 3 |
| **unity-optimizer** | 性能优化 | 专项任务 |
| **unity-debugger** | 问题诊断、Bug修复 | 专项任务 |

**详细说明**：查看 `blockgame-workflow` 技能 → agents-guide.md

---

## 特殊规则

### 文档阅读规则
阅读设计文档时，只提出工作计划，禁止自动更改代码

### 文档生成规则
禁止自动生成文档

### Meta文件规则
绝对禁止自动生成meta文件

---

## 开发验证流程

```bash
# 1. 修改代码后立即刷新Unity
mcp__UnityMCP__refresh_unity(compile: "request", mode: "force")

# 2. 检查编译错误
mcp__UnityMCP__read_console(types: ["error", "warning"])

# 3. 如果有错误，立即修复后再次编译
# 4. 重复直到没有错误
```

---

## 📖 完整文档索引

- **开发工作流程**: `/skill blockgame-workflow` ⭐⭐⭐
- **开发规范**: `/skill block-unity-standards` ⭐⭐⭐
- **设计质询**: `/skill design-interrogation`
- **Agent说明**: `.claude/agents/` 目录
- **项目文档**: `/Documents/` 目录（需请求权限）

---

**建议**：首次使用时，先查看 `blockgame-workflow` 技能了解完整的开发流程。
