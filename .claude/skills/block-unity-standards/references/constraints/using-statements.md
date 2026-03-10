# Using语句管理规范

**目的**：防止类型未定义错误，确保所有类型引用都有正确的using语句

## 新增API调用时的Using检查流程

每次在文件中使用新的类型时，必须执行以下检查：

### 第1步：确认类型所在命名空间
```bash
# 查找类型定义
grep -r "class GameManager" Scripts --include="*.cs"

# 查看命名空间
grep "namespace" Scripts/GameCore/GameManager.cs
# 输出：namespace BlockPuzzleGameToolkit.Scripts.GameCore
```

### 第2步：检查当前文件的using语句
```bash
# 查看文件开头的using语句
head -20 TargetFile.cs | grep "using"
```

### 第3步：添加缺失的using语句
```csharp
// 如果使用了GameManager但缺少using
using BlockPuzzleGameToolkit.Scripts.GameCore;

// 如果使用了CurrencyManager
using BlockPuzzleGameToolkit.Scripts.CurrencySystem;
```

## 常见命名空间映射

| 类型 | 命名空间 | Using语句 |
|-----|---------|----------|
| GameManager | BlockPuzzleGameToolkit.Scripts.GameCore | using BlockPuzzleGameToolkit.Scripts.GameCore; |
| GameDataManager | BlockPuzzleGameToolkit.Scripts.Gameplay.Managers | using BlockPuzzleGameToolkit.Scripts.Gameplay.Managers; |
| CurrencyManager | BlockPuzzleGameToolkit.Scripts.CurrencySystem | using BlockPuzzleGameToolkit.Scripts.CurrencySystem; |
| StorageManager | BlockPuzzleGameToolkit.Scripts.Storage | using BlockPuzzleGameToolkit.Scripts.Storage; |
| EventManager | BlockPuzzleGameToolkit.Scripts.GameCore | using BlockPuzzleGameToolkit.Scripts.GameCore; |
| AdSystemManager | BlockPuzzle.AdSystem.Managers | using BlockPuzzle.AdSystem.Managers; |
| NativeBridgeManager | BlockPuzzle.NativeBridge | using BlockPuzzle.NativeBridge; |

## 编译错误预防检查清单

### 修改代码前必查：
- [ ] 是否修改了方法签名？→ 执行"方法签名修改规范"
- [ ] 是否新增了类型引用？→ 执行"Using语句检查流程"
- [ ] 是否调用了其他Manager？→ 检查using语句
- [ ] 是否使用了枚举类型？→ 检查枚举所在命名空间
- [ ] 是否跨文件修改？→ 确保所有文件都包含必要的using

### 修改代码后必做：
- [ ] 立即刷新Unity（不要等到全部修改完）
- [ ] 检查Console是否有编译错误
- [ ] 如果有错误，立即修复，不要继续修改其他文件
- [ ] 验证所有using语句都是必需的（移除冗余using）

## ❌ 常见错误场景

**错误示例1：使用了GameManager但缺少using**
```csharp
// FixedRewardPopup.cs
// 缺少：using BlockPuzzleGameToolkit.Scripts.GameCore;

private void OnAdFailed()
{
    GameManager.Instance.NextLevel(); // 编译错误：找不到GameManager
}
```

**错误示例2：修改方法签名但忘记检查using**
```csharp
// 基类修改了Action<bool>，子类跟着修改
// 但如果之前没有using System，现在也需要检查
protected override void ShowInterstitialAd(Action<bool> callback)
{
    // 编译错误：找不到Action<bool>
}
```

## ✅ 正确示例

**修改前先检查using**
```csharp
using System; // Action<T>需要
using BlockPuzzleGameToolkit.Scripts.GameCore; // GameManager需要
using BlockPuzzleGameToolkit.Scripts.Gameplay.Managers; // GameDataManager需要

namespace BlockPuzzleGameToolkit.Scripts.Popups.Reward
{
    public class FixedRewardPopup : RewardPopupBase
    {
        protected override void ShowInterstitialAd(Action<bool> callback)
        {
            // 现在可以安全使用
            if (GameDataManager.HasMoreLevels())
            {
                GameManager.Instance.NextLevel();
            }
        }
    }
}
```

## 快速验证命令

```bash
# 查找缺少using的文件
grep -l "GameManager" Scripts/**/*.cs | while read file; do
    if ! grep -q "using.*GameCore" "$file"; then
        echo "Missing using in: $file"
    fi
done
```
