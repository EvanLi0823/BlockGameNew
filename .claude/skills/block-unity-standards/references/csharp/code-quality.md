# 代码质量规范

## 目的
确保BlockGame项目代码的健壮性、可维护性和一致性。

## 🚫 硬性约束（违反将导致编译错误）

### 单例模式规范

**必须使用Instance（大写）作为单例属性名：**

```csharp
// ✅ 正确
public class ExampleManager : SingletonBehaviour<ExampleManager>
{
    // 使用Instance访问单例
    var manager = ExampleManager.Instance;
}

// ❌ 错误 - 会导致编译错误
ExampleManager.instance; // 小写instance不存在
```

### 批量修改单例引用时的规范

修改单例引用必须：
1. 只修改`/Scripts`目录下的文件
2. 排除第三方库目录（Demigiant、Plugins、TextMeshPro等）
3. 使用精确匹配（如`StorageManager.instance -> StorageManager.Instance`）
4. 执行前预览修改内容

```bash
# ✅ 正确的批量修改
find /Users/lifan/BlockGame/Assets/BlockPuzzleGameToolkit/Scripts -name "*.cs" \
    -not -path "*/Demigiant/*" -not -path "*/Plugins/*" | \
    xargs sed -i '' 's/StorageManager\.instance/StorageManager.Instance/g'

# ❌ 错误 - 会影响第三方库
find /Users/lifan/BlockGame/Assets -name "*.cs" | \
    xargs sed -i '' 's/\.instance/\.Instance/g'
```

## 🔴 必须遵守的质量标准

### 1. 访问修饰符规范

```csharp
public class ExampleClass
{
    // ✅ 正确：明确的访问修饰符
    private int count;
    public string Name { get; private set; }
    protected virtual void OnInit() { }

    // ❌ 错误：缺少访问修饰符
    int value;  // 应该是 private int value;
    void DoSomething() { } // 应该是 private void DoSomething()
}
```

### 2. 只读字段和常量

```csharp
public class Configuration
{
    // ✅ 正确：使用readonly标记不会重新赋值的字段
    private readonly List<Item> items = new List<Item>();
    private readonly string configPath;

    // ✅ 正确：使用const定义常量
    private const int MAX_COUNT = 100;
    private const string DEFAULT_NAME = "Player";

    // ❌ 错误：应该使用readonly
    private List<Item> itemList = new List<Item>(); // 不会重新赋值

    // ❌ 错误：应该使用const
    private readonly int MAX_SIZE = 50; // 编译时常量应该用const
}
```

### 3. 异常处理规范

```csharp
public class DataManager : SingletonBehaviour<DataManager>
{
    // ✅ 正确：参数验证抛出异常
    public void SaveData(string key, object data)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Key cannot be null or empty", nameof(key));

        if (data == null)
            throw new ArgumentNullException(nameof(data));

        // 保存逻辑
    }

    // ✅ 正确：外部调用使用try-catch
    public bool LoadDataFromFile(string path)
    {
        try
        {
            var content = File.ReadAllText(path);
            ProcessData(content);
            return true;
        }
        catch (FileNotFoundException e)
        {
            Debug.LogError($"文件未找到: {path}, 错误: {e.Message}");
            return false;
        }
        catch (Exception e)
        {
            Debug.LogError($"加载数据失败: {e.Message}");
            return false;
        }
    }

    // ❌ 错误：不应该吞掉异常
    public void BadMethod()
    {
        try
        {
            // 某些操作
        }
        catch
        {
            // 静默失败，没有任何处理
        }
    }
}
```

### 4. 使用nameof操作符

```csharp
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed;

    // ✅ 正确：使用nameof
    public void SetSpeed(float value)
    {
        if (value < 0)
            throw new ArgumentException($"{nameof(value)} cannot be negative");

        speed = value;
        OnPropertyChanged(nameof(speed));
    }

    // ❌ 错误：硬编码字符串
    public void BadSetSpeed(float value)
    {
        if (value < 0)
            throw new ArgumentException("value cannot be negative"); // 应该用nameof(value)

        speed = value;
        OnPropertyChanged("speed"); // 应该用nameof(speed)
    }
}
```

### 5. 空值检查规范

```csharp
public class GameController : SingletonBehaviour<GameController>
{
    // ✅ 正确：安全的Manager访问
    public void InitializeGame()
    {
        // 检查依赖的Manager
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.LoadLevel(1);
        }
        else
        {
            Debug.LogError($"{nameof(LevelManager)} not found!");
        }
    }

    // ✅ 正确：使用?.操作符
    public int GetPlayerScore()
    {
        return ScoreManager.Instance?.GetScore() ?? 0;
    }

    // ❌ 错误：没有null检查
    public void BadMethod()
    {
        LevelManager.Instance.LoadLevel(1); // 可能空引用异常
    }
}
```

### 6. 事件订阅管理 ⚠️ 严重内存泄漏风险

**核心原则**：所有 `+=` 订阅都必须有对应的 `-=` 取消订阅。

#### 6.1 MonoBehaviour 生命周期中的事件订阅

```csharp
public class UIManager : SingletonBehaviour<UIManager>
{
    private Action<int> scoreChangedHandler;

    // ✅ 正确：在Start订阅，OnDestroy取消订阅
    private void Start()
    {
        if (ScoreManager.Instance != null)
        {
            scoreChangedHandler = OnScoreChanged;
            ScoreManager.Instance.OnScoreChanged += scoreChangedHandler;
        }
    }

    protected override void OnDestroy()
    {
        // 取消订阅防止内存泄漏
        if (ScoreManager.Instance != null && scoreChangedHandler != null)
        {
            ScoreManager.Instance.OnScoreChanged -= scoreChangedHandler;
        }

        base.OnDestroy();
    }

    private void OnScoreChanged(int score)
    {
        // 处理分数变化
    }

    // ❌ 错误：忘记取消订阅（导致内存泄漏）
    private void BadSubscription()
    {
        ScoreManager.Instance.OnScoreChanged += OnScoreChanged;
        // 没有在OnDestroy中取消订阅 → 即使GameObject销毁，回调仍会执行
    }
}
```

#### 6.2 匿名 Lambda 订阅 - 严重内存泄漏源

**⚠️ 绝对禁止使用无法取消订阅的匿名 lambda！**

```csharp
public class AdSystemManager : SingletonBehaviour<AdSystemManager>
{
    public event Action<AdResult> OnAdComplete;

    // ❌ 严重错误：匿名lambda永不清理（内存泄漏）
    public void PlayAdBad(string entryName, Action<bool> callback)
    {
        OnAdComplete += (result) =>  // ← 匿名lambda，无法使用 -= 移除
        {
            if (result.entryName == entryName)
            {
                callback(result.success);
            }
        };
        // 问题：每次调用都添加新的lambda，永不清理
        // 结果：内存泄漏 + 性能下降 + 重复执行
    }

    // ✅ 正确：使用可自我取消订阅的handler
    public void PlayAdGood(string entryName, Action<bool> callback)
    {
        Action<AdResult> handler = null;
        handler = (result) =>
        {
            if (result.entryName == entryName)
            {
                try
                {
                    callback(result.success);
                }
                finally
                {
                    // 执行完成后立即取消订阅
                    OnAdComplete -= handler;
                }
            }
        };
        OnAdComplete += handler;
    }
}
```

**为什么匿名lambda是严重问题？**

```
第1次调用: OnAdComplete += lambda1  (订阅者: 1)
第2次调用: OnAdComplete += lambda2  (订阅者: 2)
第3次调用: OnAdComplete += lambda3  (订阅者: 3)
...
第100次调用: OnAdComplete += lambda100  (订阅者: 100)

每次触发事件:
  OnAdComplete?.Invoke(result)
  ├─ lambda1 执行 (检查 entryName)
  ├─ lambda2 执行 (检查 entryName)
  ├─ ...
  └─ lambda100 执行 (检查 entryName)

结果：
- 内存泄漏：每个lambda持有闭包引用，永不释放
- 性能下降：订阅者越多，事件触发越慢
- 潜在重复执行：同一entryName可能匹配多个lambda
```

#### 6.3 动态订阅的最佳实践

**规则**：如果在方法中动态订阅事件，必须确保订阅者能被清理。

```csharp
public class GameManager : SingletonBehaviour<GameManager>
{
    // ✅ 方案1：使用命名handler + finally确保清理
    public void WaitForLevelComplete(Action onComplete)
    {
        Action<int> handler = null;
        handler = (levelId) =>
        {
            try
            {
                onComplete?.Invoke();
            }
            finally
            {
                // 无论成功或异常，都取消订阅
                LevelManager.Instance.OnLevelComplete -= handler;
            }
        };
        LevelManager.Instance.OnLevelComplete += handler;
    }

    // ✅ 方案2：返回IDisposable，使用using语句管理生命周期
    public IDisposable SubscribeToEvent(Action callback)
    {
        Action handler = () => callback?.Invoke();
        SomeManager.Instance.OnEvent += handler;

        return new ActionDisposable(() =>
        {
            SomeManager.Instance.OnEvent -= handler;
        });
    }

    // 使用方式
    public void Example()
    {
        using (var subscription = SubscribeToEvent(() => Debug.Log("Event fired")))
        {
            // subscription会在离开作用域时自动取消订阅
        }
    }

    // ✅ 方案3：保存handler引用，在合适时机取消
    private Action<int> currentHandler;

    public void StartListening(Action<int> callback)
    {
        StopListening(); // 先清理旧订阅

        currentHandler = callback;
        EventSource.Instance.OnEvent += currentHandler;
    }

    public void StopListening()
    {
        if (currentHandler != null)
        {
            EventSource.Instance.OnEvent -= currentHandler;
            currentHandler = null;
        }
    }
}

// 辅助类：IDisposable实现
public class ActionDisposable : IDisposable
{
    private Action action;

    public ActionDisposable(Action action)
    {
        this.action = action;
    }

    public void Dispose()
    {
        action?.Invoke();
        action = null;
    }
}
```

#### 6.4 事件订阅检查清单

**在代码审查时必须检查**：

```csharp
// 1. 搜索所有 += 操作
// 2. 确认每个 += 都有对应的 -=
// 3. 检查取消订阅的时机是否合理

// ✅ 好的模式
void Start() { EventSource.OnEvent += Handler; }
void OnDestroy() { EventSource.OnEvent -= Handler; }

// ✅ 好的模式（一次性订阅）
void Method()
{
    Action handler = null;
    handler = () => {
        // do something
        EventSource.OnEvent -= handler; // 自我清理
    };
    EventSource.OnEvent += handler;
}

// ❌ 危险模式（永不清理）
void Method()
{
    EventSource.OnEvent += () => { }; // 匿名lambda，无法清理
}

// ❌ 危险模式（忘记取消订阅）
void Start() { EventSource.OnEvent += Handler; }
// OnDestroy中没有取消订阅
```

#### 6.5 常见内存泄漏场景

**场景1：广告回调**
```csharp
// ❌ 错误：每次播放广告都累积订阅者
public void PlayAd(string adId, Action<bool> callback)
{
    AdSystem.OnAdComplete += (result) => // 永不清理
    {
        if (result.id == adId) callback(result.success);
    };
}

// ✅ 正确：自动清理
public void PlayAd(string adId, Action<bool> callback)
{
    Action<AdResult> handler = null;
    handler = (result) =>
    {
        if (result.id == adId)
        {
            try { callback(result.success); }
            finally { AdSystem.OnAdComplete -= handler; }
        }
    };
    AdSystem.OnAdComplete += handler;
}
```

**场景2：UI监听游戏事件**
```csharp
// ❌ 错误：UI销毁后仍然响应事件
public class GameUI : MonoBehaviour
{
    void Start()
    {
        GameManager.Instance.OnScoreChanged += UpdateScore;
        // 忘记在OnDestroy取消订阅
    }

    void UpdateScore(int score) { }
}

// ✅ 正确：始终配对订阅/取消订阅
public class GameUI : MonoBehaviour
{
    void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnScoreChanged += UpdateScore;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnScoreChanged -= UpdateScore;
    }

    void UpdateScore(int score) { }
}
```

#### 6.6 事件订阅调试技巧

```csharp
// 调试：检查事件订阅者数量
public class EventDebugger
{
    public static int GetSubscriberCount(Delegate eventDelegate)
    {
        return eventDelegate?.GetInvocationList().Length ?? 0;
    }
}

// 使用示例
Debug.Log($"OnAdComplete订阅者数量: {EventDebugger.GetSubscriberCount(AdSystem.OnAdComplete)}");

// 如果这个数量持续增长，说明存在内存泄漏
```

## 编译器警告处理

### 必须修复的警告类型

1. **CS0649**: 字段从未被赋值
```csharp
// ❌ 产生警告
private GameObject target;

// ✅ 修复方案1：初始化
private GameObject target = null;

// ✅ 修复方案2：SerializeField
[SerializeField] private GameObject target;

// ✅ 修复方案3：明确赋值为null
private GameObject target = default;
```

2. **CS0067**: 事件从未使用
```csharp
// ❌ 产生警告
public event Action OnSomething;

// ✅ 修复：实际触发事件
public event Action OnSomething;
private void TriggerEvent()
{
    OnSomething?.Invoke();
}
```

3. **CS0219**: 变量已赋值但从未使用
```csharp
// ❌ 产生警告
int unusedValue = 10;

// ✅ 修复：删除未使用的变量
// 或者实际使用它
```

## 日志输出管理规范

### 核心原则

所有模块必须实现统一的日志管理，通过开关控制日志输出，避免生产环境日志污染。

### 规范要求

1. **每个Manager/Controller必须提供日志开关**
   - 在Inspector中可配置（开发时开启，发布时关闭）
   - 所有Debug.Log调用必须通过统一方法
   - 支持分级控制（Log/Warning/Error）

2. **统一日志方法模板**

```csharp
public class ExampleManager : SingletonBehaviour<ExampleManager>
{
    [Header("Debug Settings")]
    [Tooltip("是否输出调试日志")]
    [SerializeField] private bool enableDebugLog = false;

    #region Log Methods

    /// <summary>
    /// 统一的日志输出方法（普通日志）
    /// </summary>
    private void Log(string message)
    {
        if (enableDebugLog)
        {
            Debug.Log(message);
        }
    }

    /// <summary>
    /// 统一的日志输出方法（警告）
    /// </summary>
    private void LogWarning(string message)
    {
        if (enableDebugLog)
        {
            Debug.LogWarning(message);
        }
    }

    /// <summary>
    /// 统一的日志输出方法（错误）
    /// </summary>
    private void LogError(string message)
    {
        if (enableDebugLog)
        {
            Debug.LogError(message);
        }
    }

    #endregion

    // 使用示例
    public override void OnInit()
    {
        base.OnInit();

        Log("[ExampleManager] 初始化开始"); // ✅ 正确

        // Debug.Log("[ExampleManager] xxx"); // ❌ 错误：不要直接使用Debug.Log
    }

    public void ProcessData(int count)
    {
        if (count < 0)
        {
            LogError($"[ExampleManager] 无效参数: count={count}"); // ✅ 正确
            return;
        }

        Log($"[ExampleManager] 处理数据: {count}条"); // ✅ 正确
    }
}
```

### 3. 日志格式规范

```csharp
// ✅ 正确：包含模块名和关键信息
Log($"[MoneyBlockManager] 形状放置计数: {saveData.shapePlacementCount}");
Log($"[DynamicDifficultyController] 🎯 开始选择方块 | 关卡={level?.name}");

// ✅ 正确：使用emoji增强可读性（可选）
Log("🔧 [DynamicDifficultyController] OnInit() 开始执行");
Log("✅ [DynamicDifficultyController] 成功加载 layer1");
Log("⚠️ [DynamicDifficultyController] 未找到LevelManager");

// ❌ 错误：缺少模块名
Log("开始初始化"); // 无法定位是哪个模块

// ❌ 错误：信息不足
Log("count=5"); // 不知道是什么count
```

### 4. 开关命名规范

```csharp
// ✅ 推荐的开关名称
private bool enableDebugLog;      // 通用日志开关
private bool showDebugInfo;       // 调试信息开关
private bool enableLog;           // 简洁版

// ❌ 不推荐
private bool debug;               // 太泛化
private bool log;                 // 不清晰
private bool showLog;             // 容易混淆
```

### 5. 实际案例：DynamicDifficultyController

```csharp
public class DynamicDifficultyController : SingletonBehaviour<DynamicDifficultyController>
{
    [Header("Debug Info")]
    [Tooltip("显示调试信息")]
    public bool showDebugInfo = false;

    #region Log Methods

    private void Log(string message)
    {
        if (showDebugInfo)
        {
            Debug.Log(message);
        }
    }

    private void LogWarning(string message)
    {
        if (showDebugInfo)
        {
            Debug.LogWarning(message);
        }
    }

    private void LogError(string message)
    {
        if (showDebugInfo)
        {
            Debug.LogError(message);
        }
    }

    #endregion

    public override void OnInit()
    {
        base.OnInit();

        Log("🔧 [DynamicDifficultyController] OnInit() 开始执行");

        if (layer1 == null)
        {
            LogWarning("DynamicDifficultyController: layer1未设置");
            // 加载逻辑...
        }
    }

    public ShapeTemplate SelectNextShape(FieldManager field, Level level)
    {
        Log($"[DynamicDifficultyController] 🎯 开始选择方块 | 关卡={level?.name}");

        if (field == null || level == null)
        {
            LogWarning("DynamicDifficultyController: FieldManager或Level为null");
            return null;
        }

        // ... 业务逻辑
    }
}
```

### 6. 优势总结

✅ **统一管理** - 所有日志通过3个方法集中控制
✅ **性能优化** - 日志关闭时无任何输出开销
✅ **易于维护** - 修改日志行为只需修改3个方法
✅ **灵活扩展** - 可轻松添加日志分级、文件输出等功能
✅ **发布控制** - 发布前统一关闭所有日志，避免性能损耗

### 7. 检查清单

提交代码前必须检查：

- [ ] 模块是否添加了日志开关（enableDebugLog/showDebugInfo）
- [ ] 是否实现了Log/LogWarning/LogError三个方法
- [ ] 所有Debug.Log调用是否替换为统一方法
- [ ] 日志格式是否包含模块名
- [ ] Inspector中的日志开关默认值是否设置为false（发布版）

### 8. 常见问题

**Q: 为什么不直接使用Debug.Log？**
A: 直接使用Debug.Log无法统一控制，发布版会产生性能损耗和日志污染。

**Q: Error级别的日志也需要开关控制吗？**
A: 建议根据实际情况决定。关键错误可以不受开关控制，但调试性质的错误应该受控。

**Q: 如何在多个模块间共享日志配置？**
A: 可以创建全局LogSettings ScriptableObject，各模块引用统一配置。

```csharp
[CreateAssetMenu(fileName = "LogSettings", menuName = "Settings/LogSettings")]
public class LogSettings : ScriptableObject
{
    [Header("模块日志开关")]
    public bool enableDifficultyLog = false;
    public bool enableMoneyBlockLog = false;
    public bool enableAdSystemLog = false;
    // ... 其他模块

    private static LogSettings instance;
    public static LogSettings Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<LogSettings>("Settings/LogSettings");
            }
            return instance;
        }
    }
}

// 使用方式
public class DynamicDifficultyController : SingletonBehaviour<DynamicDifficultyController>
{
    private void Log(string message)
    {
        if (LogSettings.Instance?.enableDifficultyLog ?? false)
        {
            Debug.Log(message);
        }
    }
}
```

## 代码组织规范

### 使用#region组织代码

```csharp
public class ExampleManager : SingletonBehaviour<ExampleManager>
{
    #region 字段和属性

    private readonly List<Item> items = new List<Item>();
    public int ItemCount => items.Count;

    #endregion

    #region 事件

    public event Action<Item> OnItemAdded;
    public event Action<Item> OnItemRemoved;

    #endregion

    #region Unity生命周期

    public override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        Initialize();
    }

    #endregion

    #region 公共方法

    public void AddItem(Item item)
    {
        items.Add(item);
        OnItemAdded?.Invoke(item);
    }

    #endregion

    #region 私有方法

    private void Initialize()
    {
        // 初始化逻辑
    }

    #endregion
}
```

## 质量检查清单

提交代码前必须检查：

- [ ] **单例属性**: 所有单例使用Instance（大写）
- [ ] **访问修饰符**: 所有成员都有明确的访问修饰符
- [ ] **只读标记**: 不可变字段使用readonly
- [ ] **常量定义**: 编译时常量使用const
- [ ] **nameof使用**: 没有硬编码的属性/参数名字符串
- [ ] **异常处理**: 参数验证和外部调用有适当的异常处理
- [ ] **空值检查**: Manager调用前进行null检查
- [ ] **事件订阅管理** ⚠️ **重点检查**:
  - [ ] 所有 `+=` 都有对应的 `-=`
  - [ ] MonoBehaviour的订阅在OnDestroy中取消
  - [ ] 动态订阅使用可清理的handler（禁止匿名lambda）
  - [ ] 使用finally确保订阅清理的可靠性
  - [ ] 检查事件订阅者数量是否持续增长
- [ ] **编译器警告**: 零警告政策，所有警告已修复
- [ ] **代码组织**: 使用#region合理组织代码结构

## 常见问题修复指南

### 问题1：单例访问错误
```csharp
// 错误信息：'instance' does not exist
// 原因：使用了小写的instance
// 修复：改为Instance（大写）
var manager = SomeManager.Instance; // 不是instance
```

### 问题2：空引用异常
```csharp
// 错误信息：NullReferenceException
// 原因：没有检查Manager是否存在
// 修复：添加null检查
if (TargetManager.Instance != null)
{
    TargetManager.Instance.DoSomething();
}
```

### 问题3：内存泄漏（事件订阅未清理）

```csharp
// 错误：事件订阅后没有取消订阅
// 症状：内存占用持续上升，事件响应越来越慢
// 修复：在OnDestroy中取消所有事件订阅
protected override void OnDestroy()
{
    // 取消所有事件订阅
    if (EventSource.Instance != null)
    {
        EventSource.Instance.OnEvent -= HandleEvent;
    }
    base.OnDestroy();
}
```

### 问题4：匿名Lambda导致的严重内存泄漏

```csharp
// 错误信息：内存占用持续增长，GC频繁触发
// 原因：使用了无法取消订阅的匿名lambda
// 症状：
//   - 事件订阅者数量持续增长
//   - 相同的回调被执行多次
//   - 内存Profiler显示大量委托对象

// ❌ 错误代码
OnAdComplete += (result) => { callback(result); };

// ✅ 修复方案
Action<AdResult> handler = null;
handler = (result) => {
    try { callback(result); }
    finally { OnAdComplete -= handler; }
};
OnAdComplete += handler;
```

### 问题5：对象销毁后事件仍触发

```csharp
// 错误信息：MissingReferenceException 或 NullReferenceException
// 原因：对象已销毁但事件订阅未清理，回调仍被触发
// 修复：在回调开始处检查对象是否存活

adManager.PlayAd("ad_id", (success) =>
{
    // ✅ 添加对象存活检查
    if (this == null)
    {
        Debug.LogWarning("对象已销毁，忽略回调");
        return;
    }

    // 处理广告结果
    HandleAdResult(success);
});
```