# 命名空间规范

**目的**：防止与系统命名空间冲突，确保命名空间清晰可辨

## 禁止使用的文件夹/命名空间名称

### 禁用列表

| 禁用名称 | 原因 | 冲突的系统命名空间 |
|---------|------|------------------|
| System | 与.NET System冲突 | System |
| Collections | 与集合冲突 | System.Collections |
| IO | 与输入输出冲突 | System.IO |
| Threading | 与线程冲突 | System.Threading |
| UI | 与Unity UI冲突 | UnityEngine.UI |
| Editor | 与Unity编辑器冲突 | UnityEditor |
| Linq | 与LINQ冲突 | System.Linq |
| Text | 与文本冲突 | System.Text |
| Net | 与网络冲突 | System.Net |

## 推荐的文件夹命名

### 替代方案

| 禁用名称 | 推荐替代 | 说明 |
|---------|---------|------|
| System | GameCore, CoreSystem | 游戏核心系统 |
| UI | GameUI, UISystem | 游戏UI系统 |
| Collections | DataStructures, Containers | 数据结构 |
| IO | FileSystem, Storage | 文件系统 |
| Threading | Concurrency, Async | 并发系统 |
| Editor | GameEditor, Tools | 编辑器工具 |

### 推荐命名模式

```
BlockPuzzleGameToolkit/
├── Scripts/
│   ├── GameCore/           ← 替代 System
│   ├── GameUI/             ← 替代 UI
│   ├── Gameplay/           ← 游戏逻辑
│   ├── Managers/           ← 管理器
│   ├── Utilities/          ← 工具类
│   ├── DataStructures/     ← 替代 Collections
│   └── Storage/            ← 替代 IO
```

## ❌ 错误示例

**错误1：使用System作为命名空间**
```csharp
// ❌ 错误：与.NET System冲突
namespace BlockPuzzleGameToolkit.Scripts.System
{
    public class GameManager { }
}

// 使用时会混淆
using BlockPuzzleGameToolkit.Scripts.System; // 哪个System？
using System; // 冲突！
```

**错误2：使用UI作为命名空间**
```csharp
// ❌ 错误：与UnityEngine.UI冲突
namespace BlockPuzzleGameToolkit.Scripts.UI
{
    public class Button { } // 与UnityEngine.UI.Button冲突！
}
```

## ✅ 正确示例

**正确1：使用GameCore替代System**
```csharp
// ✅ 正确：使用GameCore
namespace BlockPuzzleGameToolkit.Scripts.GameCore
{
    public class GameManager : SingletonBehaviour<GameManager>
    {
        // ...
    }
}

// 使用时清晰明确
using System; // .NET System
using BlockPuzzleGameToolkit.Scripts.GameCore; // 游戏核心
```

**正确2：使用GameUI替代UI**
```csharp
// ✅ 正确：使用GameUI
namespace BlockPuzzleGameToolkit.Scripts.GameUI
{
    public class CustomButton : UnityEngine.UI.Button
    {
        // ...
    }
}

// 使用时不会混淆
using UnityEngine.UI; // Unity UI
using BlockPuzzleGameToolkit.Scripts.GameUI; // 游戏UI
```

## 命名空间设计原则

### 1. 明确性
- 命名空间应该清楚表达其内容
- 避免模糊或通用的名称

### 2. 层次性
- 使用层次结构组织命名空间
- 从通用到具体

### 3. 一致性
- 整个项目使用一致的命名模式
- 遵循既定的命名约定

### 4. 可扩展性
- 预留扩展空间
- 避免过于具体的命名

## BlockGame项目命名空间规范

### 标准命名空间结构

```csharp
// 核心系统
namespace BlockPuzzleGameToolkit.Scripts.GameCore { }

// UI系统
namespace BlockPuzzleGameToolkit.Scripts.GameUI { }

// 游戏玩法
namespace BlockPuzzleGameToolkit.Scripts.Gameplay { }
namespace BlockPuzzleGameToolkit.Scripts.Gameplay.Managers { }
namespace BlockPuzzleGameToolkit.Scripts.Gameplay.Controllers { }

// 系统模块
namespace BlockPuzzleGameToolkit.Scripts.CurrencySystem { }
namespace BlockPuzzleGameToolkit.Scripts.Storage { }
namespace BlockPuzzleGameToolkit.Scripts.Settings { }

// 广告系统（项目特定）
namespace BlockPuzzle.AdSystem { }
namespace BlockPuzzle.AdSystem.Managers { }

// 原生桥接
namespace BlockPuzzle.NativeBridge { }
```

## 检查清单

**创建新命名空间前必查：**
- [ ] 名称是否在禁用列表中？
- [ ] 是否会与系统命名空间冲突？
- [ ] 是否遵循项目命名规范？
- [ ] 是否与现有命名空间保持一致？

**验证命令：**
```bash
# 检查是否使用了禁用的命名空间
grep -r "namespace.*\.System[^.]" Scripts --include="*.cs"
grep -r "namespace.*\.UI[^.]" Scripts --include="*.cs"
grep -r "namespace.*\.Collections[^.]" Scripts --include="*.cs"
```
