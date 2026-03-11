# 安全调用模式

## 目的

防止NullReferenceException和访问未初始化对象的错误，确保代码健壮性。

## 单例Manager安全调用

### 标准模板

```csharp
// 所有Manager调用必须使用此模板
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

### 详细说明

#### 第1步：获取Instance

```csharp
var manager = ManagerClass.Instance;
```

**为什么不直接调用？**
```csharp
// ❌ 错误：如果Manager未初始化，会抛出异常
var result = ManagerClass.Instance.MethodName();
```

#### 第2步：Null检查

```csharp
if (manager != null)
{
    // 安全调用
}
else
{
    // 处理Manager不存在的情况
    Debug.LogWarning($"{nameof(ManagerClass)}未找到");
}
```

**为什么需要Null检查？**
- Manager可能未初始化
- Manager可能已被销毁
- 场景中可能没有Manager对象

#### 第3步：安全调用（可选：Null合并运算符）

```csharp
var result = manager?.MethodName() ?? defaultValue;
```

**等价于**：
```csharp
var result = manager.MethodName();
if (result == null)
{
    result = defaultValue;
}
```

### 使用示例

#### 示例1：调用void方法

```csharp
var storageManager = StorageManager.Instance;
if (storageManager != null)
{
    storageManager.SaveData("key", value);
}
else
{
    Debug.LogWarning($"{nameof(StorageManager)}未找到，数据未保存");
}
```

#### 示例2：调用返回值方法

```csharp
var currencyManager = CurrencyManager.Instance;
if (currencyManager != null)
{
    int coins = currencyManager?.GetCoins() ?? 0;
    Debug.Log($"当前金币: {coins}");
}
else
{
    Debug.LogWarning($"{nameof(CurrencyManager)}未找到，使用默认值0");
}
```

#### 示例3：链式调用

```csharp
var gameManager = GameManager.Instance;
if (gameManager != null)
{
    var levelData = gameManager?.GetCurrentLevel()?.GetLevelConfig();
    if (levelData != null)
    {
        // 使用levelData
    }
}
else
{
    Debug.LogWarning($"{nameof(GameManager)}未找到");
}
```

## 跨场景单例特殊情况

### DontDestroyOnLoad的Manager

**特点**：
- 跨场景持久化
- 全局唯一
- 场景切换不销毁

**安全调用**：
```csharp
// 跨场景Manager通常不会为null，但仍建议检查
var storageManager = StorageManager.Instance;
if (storageManager != null)
{
    storageManager.SaveData("key", value);
}
```

### 场景级别单例

**特点**：
- 场景切换时销毁
- 每个场景可能有不同实例
- 可能在某些场景中不存在

**安全调用**：
```csharp
// 场景级别单例更需要Null检查
var topPanel = TopPanel.Instance;
if (topPanel != null)
{
    topPanel.UpdateScore(score);
}
else
{
    // 当前场景可能没有TopPanel
    Debug.LogWarning("TopPanel不存在于当前场景");
}
```

## 生命周期安全访问

### Awake中的限制

**❌ 禁止在Awake中访问其他单例**：
```csharp
private void Awake()
{
    // ❌ 错误：其他单例可能还未初始化
    var manager = OtherManager.Instance;
}
```

**✅ 正确：在Start中访问**：
```csharp
private void Start()
{
    // ✅ 正确：所有Awake已完成，单例已初始化
    var manager = OtherManager.Instance;
    if (manager != null)
    {
        manager.Initialize();
    }
}
```

### OnDestroy中的限制

**❌ 禁止在OnDestroy中访问其他单例**：
```csharp
private void OnDestroy()
{
    // ❌ 错误：其他单例可能已被销毁
    var manager = OtherManager.Instance;
    manager.UnregisterThis();
}
```

**✅ 正确：在OnDisable中处理**：
```csharp
private void OnDisable()
{
    // ✅ 正确：对象还未销毁
    var manager = OtherManager.Instance;
    if (manager != null)
    {
        manager.UnregisterThis(this);
    }
}
```

## 事件订阅安全模式

### 订阅事件

```csharp
private void OnEnable()
{
    var eventManager = EventManager.Instance;
    if (eventManager != null)
    {
        eventManager.Subscribe<GameStartEvent>(OnGameStart);
    }
    else
    {
        Debug.LogWarning($"{nameof(EventManager)}未找到，无法订阅事件");
    }
}
```

### 取消订阅

```csharp
private void OnDisable()
{
    var eventManager = EventManager.Instance;
    if (eventManager != null)
    {
        eventManager.Unsubscribe<GameStartEvent>(OnGameStart);
    }
    // 如果EventManager为null，说明已被销毁，无需取消订阅
}
```

### 事件处理函数

```csharp
private void OnGameStart(GameStartEvent evt)
{
    // 事件触发时，访问其他Manager也需要Null检查
    var levelManager = LevelManager.Instance;
    if (levelManager != null)
    {
        levelManager.StartLevel(evt.LevelId);
    }
}
```

## Component引用安全模式

### GetComponent安全调用

```csharp
// ❌ 错误：可能返回null
var rigidbody = GetComponent<Rigidbody>();
rigidbody.AddForce(Vector3.up);  // NullReferenceException

// ✅ 正确：检查null
var rigidbody = GetComponent<Rigidbody>();
if (rigidbody != null)
{
    rigidbody.AddForce(Vector3.up);
}
else
{
    Debug.LogWarning($"{name}没有Rigidbody组件");
}
```

### 缓存组件引用

```csharp
public class MyComponent : MonoBehaviour
{
    private Rigidbody _rigidbody;

    private void Awake()
    {
        // Awake中缓存自身的组件引用
        _rigidbody = GetComponent<Rigidbody>();

        if (_rigidbody == null)
        {
            Debug.LogError($"{name}缺少Rigidbody组件");
        }
    }

    private void FixedUpdate()
    {
        // 使用前仍需检查
        if (_rigidbody != null)
        {
            _rigidbody.AddForce(Vector3.up);
        }
    }
}
```

### FindObjectOfType安全调用

```csharp
// ❌ 错误：可能返回null
var manager = FindObjectOfType<SomeManager>();
manager.DoSomething();  // NullReferenceException

// ✅ 正确：检查null
var manager = FindObjectOfType<SomeManager>();
if (manager != null)
{
    manager.DoSomething();
}
else
{
    Debug.LogWarning($"{nameof(SomeManager)}在场景中不存在");
}
```

## ScriptableObject安全模式

### 配置访问

```csharp
// ✅ 推荐：使用SingletonScriptableSettings
var settings = GameSettings.Instance;
if (settings != null)
{
    int maxLevel = settings.MaxLevel;
}
else
{
    Debug.LogError("GameSettings配置文件不存在");
}
```

### Resources.Load安全模式

```csharp
// ❌ 错误：可能返回null
var config = Resources.Load<LevelConfig>("Settings/LevelConfig");
int difficulty = config.Difficulty;  // NullReferenceException

// ✅ 正确：检查null
var config = Resources.Load<LevelConfig>("Settings/LevelConfig");
if (config != null)
{
    int difficulty = config.Difficulty;
}
else
{
    Debug.LogError("找不到配置文件: Settings/LevelConfig");
}
```

## 最佳实践总结

### ✅ 总是执行的检查

1. **单例Instance访问**：总是检查null
2. **GetComponent调用**：总是检查null
3. **FindObjectOfType调用**：总是检查null
4. **Resources.Load调用**：总是检查null
5. **事件订阅/取消订阅**：检查EventManager是否存在

### ⚠️ 生命周期限制

1. **Awake**：不访问其他单例
2. **OnDestroy**：不访问其他单例
3. **OnEnable**：可以订阅事件（需检查null）
4. **OnDisable**：取消订阅事件（需检查null）
5. **Start**：安全访问单例

### 📝 日志记录

```csharp
// ✅ 使用nameof避免硬编码
Debug.LogWarning($"{nameof(ManagerClass)}未找到");

// ✅ 提供上下文信息
Debug.LogWarning($"{name}的{nameof(ManagerClass)}未找到");

// ✅ 区分错误严重程度
Debug.LogError("致命错误");  // 严重错误
Debug.LogWarning("警告");    // 非致命问题
Debug.Log("信息");           // 调试信息
```

## 常见错误

### ❌ 错误1：不检查null就直接调用

```csharp
// ❌ 危险
StorageManager.Instance.SaveData("key", value);
```

**后果**：如果StorageManager未初始化，抛出NullReferenceException

### ❌ 错误2：在Awake中访问其他单例

```csharp
private void Awake()
{
    // ❌ 危险：OtherManager可能还未初始化
    var data = OtherManager.Instance.GetData();
}
```

**后果**：初始化顺序问题，可能获取到null

### ❌ 错误3：在OnDestroy中访问其他单例

```csharp
private void OnDestroy()
{
    // ❌ 危险：Manager可能已被销毁
    EventManager.Instance.Unsubscribe<Event>(Handler);
}
```

**后果**：访问已销毁的对象

### ❌ 错误4：不缓存GetComponent结果

```csharp
private void Update()
{
    // ❌ 性能问题：每帧都调用GetComponent
    GetComponent<Rigidbody>().AddForce(Vector3.up);
}
```

**后果**：性能问题，GC Alloc

## 安全调用检查清单

开发时使用此清单确保代码安全：

- [ ] 所有单例Instance访问都有null检查
- [ ] 所有GetComponent调用都有null检查
- [ ] 所有FindObjectOfType调用都有null检查
- [ ] 所有Resources.Load调用都有null检查
- [ ] Awake中不访问其他单例
- [ ] OnDestroy中不访问其他单例
- [ ] 事件订阅/取消订阅有null检查
- [ ] 组件引用已缓存
- [ ] 链式调用每一步都有null检查
- [ ] 使用nameof记录日志
