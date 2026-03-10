# NativeBridge - Unity与原生平台交互桥接器

## 概述

NativeBridgeManager 是一个采用 SingletonBehaviour 单例模式的管理器，用于Unity与Android/iOS原生平台交互，实现了Unity C#代码与原生代码的双向通信。与 LocalizationManager 类似，作为独立的系统组件运行。

## 目录结构

```
Assets/BlockPuzzleGameToolkit/
├── Resources/
│   └── Settings/
│       └── NativeBridgeSettings.asset    # 配置文件（通过菜单创建）
├── Scripts/
│   ├── Settings/
│   │   └── NativeBridgeSettings.cs       # 配置类定义
│   ├── Editor/
│   │   └── EditorMenu.cs                 # 统一的编辑器菜单（包含 NativeBridge 设置）
│   └── NativeBridge/
│       ├── Enums/
│       │   └── BridgeMessageType.cs      # 消息类型枚举
│       ├── Models/
│       │   └── BridgeModels.cs           # 数据模型
│       ├── NativeBridgeManager.cs        # 核心管理器（单例）
│       ├── NativeBridgeExample.cs        # 使用示例
│       └── README.md                     # 本文档
```

## 功能特点

- ✅ 支持20种预定义的消息类型
- ✅ Unity与原生平台双向通信
- ✅ Android和iOS平台统一接口
- ✅ 使用 Newtonsoft.Json 进行JSON序列化
- ✅ 事件系统支持
- ✅ 编辑器模式下的模拟响应
- ✅ 单例模式自动管理

## 支持的消息类型

| 消息类型 | 功能描述 | 参数 |
|---------|---------|------|
| CommonParam | 获取公共参数 | 无 |
| PrivacyPolicy | 隐私政策 | 无 |
| TermsOfUse | 使用条款 | 无 |
| PlayMergeAudio | 播放合并音频 | 音频参数 |
| ShowWithdraw | 显示提现界面 | 无 |
| GetMergeReward | 获取合并奖励 | 无 |
| BuryPoint | 埋点统计 | 事件名, 事件值 |
| ShowVideo | 显示视频广告 | 无 |
| RequestIsWhiteBao | 请求是否白包 | 无 |
| GetUnifyCurrency | 获取统一货币符号 | 无 |
| FeedBack | 用户反馈 | 无 |
| ShowPromotion | 显示促销活动 | 无 |
| EnterGame | 进入游戏 | 无 |
| UpdateLevel | 更新等级 | 等级值 |
| UserAmount | 用户金额 | 无 |
| IsInterADReady | 插屏广告是否就绪 | 无 |
| IsRewardADReady | 激励视频广告是否就绪 | 无 |
| IsAdMobADReady | AdMob广告是否就绪 | 无 |
| ShowWithdrawGuide | 显示提现引导 | 无 |
| IsWithdrawReward | 是否有提现奖励 | 无 |

## 快速开始

### 1. 基础初始化

```csharp
using BlockPuzzle.NativeBridge;

public class MyController : MonoBehaviour
{
    void Start()
    {
        // NativeBridgeManager会自动初始化（单例）
        if (NativeBridgeManager.instance.IsInitSuccess())
        {
            Debug.Log("NativeBridge初始化成功");
        }
    }
}
```

### 2. 发送消息到原生平台

```csharp
// 显示视频广告
NativeBridgeManager.instance.SendMessageToPlatform(BridgeMessageType.ShowVideo);

// 埋点统计（带参数）
NativeBridgeManager.instance.SendMessageToPlatform(
    BridgeMessageType.BuryPoint,
    "level_complete",
    "level_5"
);

// 更新等级
NativeBridgeManager.instance.SendMessageToPlatform(
    BridgeMessageType.UpdateLevel,
    10
);
```

### 3. 检查广告状态

```csharp
// 检查激励视频是否就绪
if (NativeBridgeManager.instance.IsADReady(AdType.RewardVideo))
{
    // 显示广告
    NativeBridgeManager.instance.SendMessageToPlatform(BridgeMessageType.ShowVideo);
}

// 检查插屏广告
bool interReady = NativeBridgeManager.instance.IsADReady(AdType.Interstitial);

// 检查AdMob广告
bool admobReady = NativeBridgeManager.instance.IsADReady(AdType.AdMob);
```

### 4. 监听事件

```csharp
void OnEnable()
{
    // 订阅事件
    NativeBridgeManager.OnVideoPlayEnd += OnVideoPlayEnd;
    NativeBridgeManager.OnCommonParamReceived += OnCommonParamReceived;
    NativeBridgeManager.OnCurrencySymbolReceived += OnCurrencySymbolReceived;
}

void OnDisable()
{
    // 取消订阅
    NativeBridgeManager.OnVideoPlayEnd -= OnVideoPlayEnd;
    NativeBridgeManager.OnCommonParamReceived -= OnCommonParamReceived;
    NativeBridgeManager.OnCurrencySymbolReceived -= OnCurrencySymbolReceived;
}

private void OnVideoPlayEnd()
{
    Debug.Log("视频播放完成，发放奖励");
}

private void OnCommonParamReceived(CommonParamResponse param)
{
    Debug.Log($"语言: {param.language}, 国家: {param.country}");
}

private void OnCurrencySymbolReceived(string symbol)
{
    Debug.Log($"货币符号: {symbol}");
}
```

### 5. 获取缓存数据

```csharp
// 获取公共参数
CommonParamResponse commonParam = NativeBridgeManager.instance.GetCommonParam();
if (commonParam != null)
{
    string language = commonParam.language;
    string country = commonParam.country;
    int numberGK = commonParam.numberGK;
}

// 获取货币符号
string currencySymbol = NativeBridgeManager.instance.GetCurrencySymbol();

// 检查H5状态
if (NativeBridgeManager.instance.CheckCanShowH5())
{
    int userType = NativeBridgeManager.instance.GetH5UserType();
}
```

## 原生平台集成

### Android平台

原生开发者需要实现以下Java类：

```java
package com.blockpuzzle.game;

public class NativeBridge {
    private static NativeBridge instance;

    public static NativeBridge getInstance() {
        // 返回单例实例
    }

    public String callUnity(String jsonData) {
        // 处理Unity发来的消息
        // 返回JSON格式的响应
    }
}
```

### iOS平台

原生开发者需要实现以下C函数：

```c
extern "C" {
    const char* callNative(const char* msg) {
        // 处理Unity发来的消息
        // 返回JSON格式的响应
    }
}
```

### 消息格式

Unity发送给原生的消息格式：
```json
{
    "m": "methodName",
    "p1": "parameter1",
    "p2": "parameter2"
}
```

原生返回给Unity的消息格式（示例）：
```json
{
    "language": "en",
    "country": "US",
    "numberGK": 1
}
```

## 配置说明

### 创建配置文件

1. **通过菜单创建**：
   - 菜单栏：`Tools > BlockPuzzleGameToolkit > Settings > Native Bridge settings`
   - 如果配置文件不存在，会自动在 `Assets/BlockPuzzleGameToolkit/Resources/Settings/` 目录下创建 `NativeBridgeSettings.asset`
   - 如果配置文件已存在，会直接在 Inspector 中打开

2. **手动创建**：
   - 在 Project 窗口右键
   - 选择 `Create > Block Puzzle > Settings > Native Bridge Settings`
   - 将文件保存到 `Assets/BlockPuzzleGameToolkit/Resources/Settings/` 目录

### 配置选项

| 设置项 | 说明 | 默认值 |
|--------|------|--------|
| Enable Native Bridge | 是否启用 Native Bridge | true |
| Android Package Name | Android 原生类的包名 | com.blockpuzzle.game.NativeBridge |
| Android Method Name | Android 原生方法名 | callUnity |
| iOS Method Name | iOS 原生方法名 | callNative |
| Enable Debug Logs | 是否输出调试日志 | true |
| Mock Response In Editor | 编辑器中模拟原生响应 | true |
| Auto Initialize | 启动时自动初始化 | true |
| Auto Request Common Params | 自动请求公共参数 | true |
| Auto Request White Bao | 自动请求白包状态 | true |
| Auto Request Currency | 自动请求货币符号 | true |
| Native Call Timeout | 原生调用超时时间（秒） | 5 |

### 管理配置

- **打开配置**：`Tools > BlockPuzzleGameToolkit > Settings > Native Bridge settings`
- **修改配置**：在 Inspector 中直接编辑各项参数
- **重置默认值**：在 Inspector 中右键点击任意字段，选择 "Reset" 恢复默认值

## 编辑器模式

在Unity编辑器中运行时：
- 大部分功能会返回模拟数据
- `IsInitSuccess()` 默认返回 true
- `IsADReady()` 默认返回 true
- 适合用于UI开发和基础逻辑测试

## 注意事项

1. **依赖项**: 需要先安装 Newtonsoft.Json 包 (com.unity.nuget.newtonsoft-json)
2. **初始化时机**: NativeBridge会在首次访问Instance时自动初始化
3. **线程安全**: 原生回调Unity的方法需要在主线程执行
4. **JSON格式**: 所有数据交互必须使用正确的JSON格式
5. **内存管理**: iOS端注意字符串内存的分配和释放
6. **错误处理**: 建议在发送消息前检查初始化状态

## 扩展开发

### 添加新的消息类型

1. 在 `BridgeMessageType` 枚举中添加新类型
2. 在 `_methodNameMap` 字典中添加方法名映射
3. 在 `_messageHandlers` 中添加处理器
4. 实现对应的处理方法

```csharp
// 1. 添加枚举
public enum BridgeMessageType
{
    // ...
    MyNewFeature = 21
}

// 2. 添加映射
_methodNameMap.Add(BridgeMessageType.MyNewFeature, "myNewFeature");

// 3. 添加处理器
_messageHandlers.Add(BridgeMessageType.MyNewFeature, HandleMyNewFeature);

// 4. 实现处理方法
private void HandleMyNewFeature(Dictionary<string, object> data)
{
    // 处理逻辑
}
```

## 调试技巧

1. 启用详细日志：所有关键操作都有Debug.Log输出
2. 使用NativeBridgeExample测试各项功能
3. 在编辑器中先测试基础流程
4. 使用Android Logcat或Xcode Console查看原生日志

## 常见问题

**Q: 初始化失败怎么办？**
A: 检查Android的packageName配置是否正确，确认原生类已正确实现。

**Q: 消息没有响应？**
A: 检查JSON格式是否正确，确认方法名映射是否匹配。

**Q: 广告相关功能不工作？**
A: 先使用IsADReady()检查状态，监听OnVideoPlayEnd事件确认播放结果。

## 版本历史

- v2.2.0 - 将编辑器菜单功能整合到 EditorMenu.cs，统一菜单管理
- v2.1.0 - 将配置移到 ScriptableObject 配置文件，支持通过编辑器工具管理
- v2.0.0 - 采用 SingletonBehaviour 单例模式，独立于 GameManager
- v1.1.0 - 使用 Newtonsoft.Json 替代 MiniJSON，提供更强大的序列化功能
- v1.0.0 - 初始版本，支持20种消息类型

## 联系方式

如有问题或建议，请联系开发团队。