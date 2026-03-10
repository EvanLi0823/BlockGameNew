# 广告系统 (Ad System)

## 概述

广告系统是一个独立的模块，用于管理和播放原生广告。它通过调用 NativeBridge 接口与原生平台交互，支持多种广告入口配置和灵活的奖励倍数设置。

配置文件与 NativeBridge 配置集中管理在 `Scripts/Settings` 目录下。

## 特性

- ✅ **独立模块设计** - 与 NativeBridge 解耦，易于维护
- ✅ **灵活的广告入口配置** - 支持自定义广告入口和参数
- ✅ **奖励倍数系统** - 每个广告入口可配置不同的奖励倍数
- ✅ **自动降级处理** - 广告未准备好时自动调用成功
- ✅ **完整的事件系统** - 提供广告播放全流程的事件通知
- ✅ **ScriptableObject 配置** - 无需修改代码即可调整配置

## 快速开始

### 1. 配置广告系统

#### 通过编辑器菜单创建（推荐）
1. 在 Unity 编辑器中，选择菜单栏
2. 点击 `Tools > BlockPuzzleGameToolkit > Settings > Ad System settings`
3. 配置文件会自动创建并在 Inspector 中打开
4. 文件自动保存在 `Resources/Settings/AdSystemSettings.asset`

#### 或手动创建
1. 右键点击 Project 窗口
2. 选择 `Create > Block Puzzle > Settings > Ad System Settings`（如果有此选项）
3. 创建 `AdSystemSettings` 配置文件
4. 将配置文件放置在 `Resources/Settings/` 目录下

### 2. 配置广告入口

在 `AdSystemSettings` 中配置广告入口：

```
每个广告入口包含以下参数：
- Name: 广告入口唯一标识
- Type: 广告类型 (0=激励视频, 1=插屏, 2=AdMob)
- Multiple: 奖励倍数 (1-10)
- Active: 是否启用
- Description: 入口描述
```

### 3. 使用广告系统

```csharp
using BlockPuzzle.AdSystem.Managers;
using BlockPuzzle.AdSystem.Models;

// 播放广告
AdSystemManager.instance.PlayAd(AdEntryNames.LEVEL_COMPLETE, (success, multiple) =>
{
    if (success)
    {
        // 广告播放成功，应用奖励倍数
        int reward = baseReward * multiple;
        Debug.Log($"获得奖励: {reward}");
    }
    else
    {
        // 广告播放失败
        Debug.Log("广告播放失败");
    }
});

// 检查广告是否准备好
bool isReady = AdSystemManager.instance.IsAdReady(AdEntryNames.DAILY_TASK_REWARD);

// 获取广告奖励倍数
int multiple = AdSystemManager.instance.GetRewardMultiple(AdEntryNames.DOUBLE_COINS);
```

## 预定义的广告入口

系统预定义了常用的广告入口：

| 入口名称 | 说明 | 默认类型 | 默认倍数 |
|---------|------|---------|---------|
| `LEVEL_COMPLETE` | 关卡完成奖励 | 激励视频 | 3 |
| `REWARD_POPUP` | 奖励弹窗 | 激励视频 | 2 |
| `LEVEL_FAILED_REFRESH` | 失败刷新 | 激励视频 | 1 |
| `DAILY_TASK_REWARD` | 每日任务奖励 | 激励视频 | 2 |
| `JACKPOT_REWARD` | Jackpot奖励 | 激励视频 | 5 |
| `EXTRA_MOVES` | 额外步数 | 激励视频 | 1 |
| `DOUBLE_COINS` | 双倍金币 | 激励视频 | 2 |
| `UNLOCK_FEATURE` | 解锁功能 | 插屏 | 1 |
| `CONTINUE_GAME` | 继续游戏 | 激励视频 | 1 |
| `SKIP_LEVEL` | 跳过关卡 | 激励视频 | 1 |

## API 参考

### AdSystemManager

主要的广告管理器类，使用单例模式。

#### 方法

| 方法 | 说明 |
|------|------|
| `PlayAd(string entryName, Action<bool, int> onComplete)` | 播放指定入口的广告 |
| `IsAdReady(string entryName)` | 检查广告是否准备好 |
| `GetRewardMultiple(string entryName)` | 获取广告奖励倍数 |
| `GetAdEntry(string entryName)` | 获取广告入口配置 |
| `GetActiveEntries()` | 获取所有活跃的广告入口 |

#### 事件

| 事件 | 说明 |
|------|------|
| `OnAdStartPlaying` | 广告开始播放 |
| `OnAdPlaySuccess` | 广告播放成功 |
| `OnAdPlayFailed` | 广告播放失败 |
| `OnAdPlayComplete` | 广告播放完成（无论成功失败） |

### AdSystemSettings

ScriptableObject 配置类，位于 `Scripts/Settings` 目录，与 NativeBridgeSettings 统一管理。

#### 属性

| 属性 | 说明 |
|------|------|
| `EnableAdSystem` | 是否启用广告系统 |
| `TreatFailureAsSuccess` | 将失败当作成功处理 |
| `DebugMode` | 调试模式 |
| `AdEntries` | 广告入口配置列表 |

## 工作流程

1. **初始化**：AdSystemManager 在 Awake 时自动初始化，加载配置
2. **播放请求**：调用 `PlayAd` 方法请求播放广告
3. **准备检查**：通过 NativeBridge 检查广告是否准备好
4. **播放处理**：
   - 如果广告准备好：调用 NativeBridge 播放广告
   - 如果未准备好：触发失败回调
5. **结果回调**：接收原生平台回调，触发相应事件

## 配置示例

### 创建自定义广告入口

```csharp
// 在代码中创建
var customEntry = AdEntry.CreatePreset("custom_ad", 0, 3);

// 或在 Inspector 中配置
// 1. 选择 AdSystemSettings 资产
// 2. 在 Ad Entries 列表中添加新项
// 3. 设置 Name, Type, Multiple 等参数
```

### 监听广告事件

```csharp
void Start()
{
    // 订阅事件
    AdSystemManager.OnAdPlaySuccess += OnAdSuccess;
    AdSystemManager.OnAdPlayFailed += OnAdFailed;
}

void OnDestroy()
{
    // 取消订阅
    AdSystemManager.OnAdPlaySuccess -= OnAdSuccess;
    AdSystemManager.OnAdPlayFailed -= OnAdFailed;
}

void OnAdSuccess(string entryName, int multiple)
{
    Debug.Log($"广告 {entryName} 播放成功，倍数: {multiple}");
}

void OnAdFailed(string entryName, string error)
{
    Debug.Log($"广告 {entryName} 播放失败: {error}");
}
```

## 调试

### 编辑器测试

AdSystemManager 提供了编辑器测试方法：

1. 在 Hierarchy 中选择 AdSystemManager 对象
2. 在 Inspector 中右键点击组件标题
3. 选择测试方法：
   - `Test Level Complete Ad` - 测试关卡完成广告
   - `Test Daily Task Ad` - 测试每日任务广告

### 调试日志

启用调试模式以查看详细日志：

1. 打开 AdSystemSettings 配置文件
2. 勾选 `Debug Mode` 选项
3. 运行时将输出详细的调试信息

## 注意事项

1. **依赖关系**：广告系统依赖 NativeBridge 模块，请确保 NativeBridge 已正确配置
2. **配置文件位置**：AdSystemSettings 必须放在 `Resources/Settings/` 目录下，与 NativeBridgeSettings 在同一位置
3. **广告类型映射**：广告类型值必须与原生平台约定一致（0=激励视频, 1=插屏, 2=AdMob）
4. **入口名称唯一性**：每个广告入口的名称必须唯一

## 故障排除

### 广告不播放

1. 检查 AdSystemSettings 中 `EnableAdSystem` 是否启用
2. 确认广告入口的 `Active` 属性为 true
3. 验证 NativeBridge 是否初始化成功
4. 查看控制台是否有错误日志
5. 检查配置文件是否存在：
   - 如果不存在，通过菜单创建：`Tools > BlockPuzzleGameToolkit > Settings > Ad System settings`
   - 确认文件位置：`Resources/Settings/AdSystemSettings.asset`

### 回调未触发

1. 确认已正确订阅事件
2. 检查 NativeBridge 的 ADPlayResult 回调是否正常工作
3. 启用调试模式查看详细流程

### 倍数不正确

1. 检查 AdSystemSettings 中对应入口的 Multiple 配置
2. 确认回调中使用的是正确的倍数参数

## 更新日志

### v1.0.0 (2024-01-23)
- 初始版本发布
- 支持多入口广告配置
- 实现奖励倍数系统
- 集成 NativeBridge 接口
- 添加完整的事件系统
- 提供 ScriptableObject 配置支持