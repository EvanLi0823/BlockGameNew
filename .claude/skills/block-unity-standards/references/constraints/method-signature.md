# 方法签名修改规范

**目的**：防止修改方法签名时遗漏子类或调用处，确保修改完整性

## 修改基类方法签名的完整工作流

当需要修改基类的虚方法/抽象方法/protected方法签名时，必须执行以下步骤：

### 第1步：影响范围分析
```bash
# 找到所有子类的override实现
grep -r "override.*MethodName" Scripts --include="*.cs"

# 找到所有直接调用此方法的地方
grep -r "\.MethodName\(" Scripts --include="*.cs"

# 找到所有该类的子类
grep -r "class.*:.*BaseClassName" Scripts --include="*.cs"
```

### 第2步：记录所有需要修改的文件
```
受影响的文件清单：
- BaseClass.cs:123 (方法定义)
- SubClassA.cs:456 (override)
- SubClassB.cs:789 (override)
- CallerClass.cs:234 (调用处)
```

### 第3步：按顺序修改
1. 修改基类方法签名
2. 修改所有子类的override方法签名
3. 修改所有调用处的参数传递
4. 检查所有文件的using语句（见using-statements.md）

### 第4步：验证完整性
```bash
# 再次搜索旧的方法签名，确保没有遗漏
grep -r "Action callback" Scripts --include="*.cs"
# 应该返回0个结果（如果全部修改完成）
```

## 关键原则

### 1. 一次性修改原则
- 修改基类方法签名时，必须在同一次提交中修改所有相关文件
- 不能分多次提交，否则中间状态会导致编译错误

### 2. 影响范围必须全覆盖
- 不仅要修改override，还要修改所有调用处
- 使用grep确保没有遗漏

### 3. 编译验证必须及时
- 修改完成后立即刷新Unity并检查编译错误
- 不要等到所有文件都修改完才验证

## ❌ 常见错误场景

**错误示例：只修改了基类和部分子类**
```csharp
// BaseClass.cs - 已修改
protected virtual void ShowInterstitialAd(Action<bool> callback) { }

// SubClassA.cs - 已修改
protected override void ShowInterstitialAd(Action<bool> callback) { }

// SubClassB.cs - 遗漏未修改！编译错误！
protected override void ShowInterstitialAd(Action callback) { }
```

## ✅ 正确做法

```bash
# 步骤1：找到所有需要修改的地方
grep -r "ShowInterstitialAd" Scripts --include="*.cs"

# 步骤2：记录所有文件路径
# - RewardPopupBase.cs
# - FixedRewardPopup.cs
# - SlidingRewardPopup.cs

# 步骤3：逐个修改，确保全部覆盖
# 步骤4：验证没有遗漏
grep -r "ShowInterstitialAd.*Action callback[^<]" Scripts --include="*.cs"
# 应该返回0个结果
```

## 修改清单

**修改前必查：**
- [ ] 找到所有override实现
- [ ] 找到所有调用处
- [ ] 记录所有需要修改的文件
- [ ] 确认修改顺序

**修改后必查：**
- [ ] 所有子类已修改
- [ ] 所有调用处已修改
- [ ] 所有文件using语句正确
- [ ] Unity编译无错误
- [ ] grep验证无旧签名残留
