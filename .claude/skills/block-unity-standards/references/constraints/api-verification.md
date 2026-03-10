# API调用验证规范

**目的**：防止调用不存在的方法，确保API调用正确

## 开发前必须执行的检查

```bash
# 必须执行的三个命令
# 1. 查看目标类的所有公共方法
grep "public" TargetClass.cs | grep -v "class"

# 2. 查找类的实际位置
find . -name "*.cs" | xargs grep "class TargetClassName"

# 3. 检查命名空间
grep "namespace" TargetFile.cs
```

## 铁律

**没有通过grep确认的方法，绝对不要使用**

## 验证工作流

### 步骤1：查找类定义
```bash
find Assets/BlockPuzzleGameToolkit/Scripts -name "*.cs" | xargs grep "class GameManager"
# 输出：Assets/BlockPuzzleGameToolkit/Scripts/GameCore/GameManager.cs:public class GameManager
```

### 步骤2：查看公共方法
```bash
grep "public" Assets/BlockPuzzleGameToolkit/Scripts/GameCore/GameManager.cs | grep -v "class"
# 输出所有公共方法、属性、字段
```

### 步骤3：检查命名空间
```bash
grep "namespace" Assets/BlockPuzzleGameToolkit/Scripts/GameCore/GameManager.cs
# 输出：namespace BlockPuzzleGameToolkit.Scripts.GameCore
```

### 步骤4：确认方法签名
```bash
grep -A 2 "public.*NextLevel" Assets/BlockPuzzleGameToolkit/Scripts/GameCore/GameManager.cs
# 查看方法的完整签名
```

## ❌ 常见错误

**错误1：未验证方法存在**
```csharp
// ❌ 错误：直接调用未验证的方法
GameManager.Instance.LoadNextLevel(); // 方法可能不存在！
```

**错误2：方法签名错误**
```csharp
// ❌ 错误：参数类型不匹配
GameManager.Instance.ShowReward("coins"); // 实际需要int参数
```

## ✅ 正确做法

```bash
# 1. 先验证方法存在
grep "public.*LoadNextLevel" GameManager.cs

# 2. 查看方法签名
grep -A 3 "public.*LoadNextLevel" GameManager.cs

# 3. 确认参数类型
# 然后再调用
```

```csharp
// ✅ 正确：验证后调用
GameManager.Instance.NextLevel(); // 已通过grep确认存在
```

## 安全调用模板

```csharp
// 所有Manager调用必须使用此模板
var manager = TargetManager.Instance;
if (manager != null)
{
    var result = manager.MethodName();
}
else
{
    Debug.LogWarning($"{nameof(TargetManager)} 未找到");
}
```
