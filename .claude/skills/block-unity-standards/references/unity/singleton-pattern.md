# 单例模式规范

## SingletonBehaviour使用指南

BlockGame项目使用统一的SingletonBehaviour基类实现单例模式，确保全局访问点的一致性和生命周期管理。

## 核心规范

### 🚫 强制规范（违反将导致错误）

1. **必须使用Instance属性（大写）**
```csharp
// ✅ 正确
var manager = GameManager.Instance;

// ❌ 错误 - instance不存在
var manager = GameManager.instance; // 编译错误
```

2. **必须继承SingletonBehaviour<T>**
```csharp
// ✅ 正确
public class ShopManager : SingletonBehaviour<ShopManager>
{
    // 实现
}

// ❌ 错误 - 不要自己实现单例
public class BadManager : MonoBehaviour
{
    private static BadManager instance; // 不推荐
}
```

## SingletonBehaviour功能详解

### 基本实现

```csharp
public class ExampleManager : SingletonBehaviour<ExampleManager>
{
    // 可选：控制是否在场景切换时保持
    protected override bool DontDestroyOnSceneChange => true;

    // 可选：设置初始化优先级（数值越小越先初始化）
    public override int InitPriority => 50;

    public override void Awake()
    {
        // 必须调用基类Awake
        base.Awake();

        // 自定义初始化逻辑
        InitializeManager();
    }
}
```

### 生命周期管理

#### 1. 初始化顺序控制

```csharp
// 通过InitPriority控制初始化顺序
public class StorageManager : SingletonBehaviour<StorageManager>
{
    public override int InitPriority => 0; // 最先初始化
}

public class CurrencyManager : SingletonBehaviour<CurrencyManager>
{
    public override int InitPriority => 10; // 依赖StorageManager
}

public class RewardCalculator : SingletonBehaviour<RewardCalculator>
{
    public override int InitPriority => 20; // 依赖CurrencyManager
}
```

#### 2. 场景持久化控制

```csharp
public class PersistentManager : SingletonBehaviour<PersistentManager>
{
    // 场景切换时保持此Manager
    protected override bool DontDestroyOnSceneChange => true;

    public override void Awake()
    {
        base.Awake();
        // DontDestroyOnLoad会自动设置
    }
}

public class SceneSpecificManager : SingletonBehaviour<SceneSpecificManager>
{
    // 场景切换时销毁（默认行为）
    protected override bool DontDestroyOnSceneChange => false;
}
```

### 安全访问模式

#### 1. 基本访问检查

```csharp
public class GameController : MonoBehaviour
{
    private void Start()
    {
        // ✅ 安全访问：先检查是否存在
        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.OpenShop();
        }
        else
        {
            Debug.LogWarning("ShopManager not found in scene");
        }
    }
}
```

#### 2. 使用空合并操作符

```csharp
public class UIController : MonoBehaviour
{
    public void UpdateScore()
    {
        // ✅ 使用?.和??提供默认值
        int score = ScoreManager.Instance?.GetCurrentScore() ?? 0;
        scoreText.text = $"Score: {score}";
    }
}
```

#### 3. 依赖检查模式

```csharp
public class DependentManager : SingletonBehaviour<DependentManager>
{
    private bool CheckDependencies()
    {
        if (StorageManager.Instance == null)
        {
            Debug.LogError($"{nameof(StorageManager)} is required but not found!");
            return false;
        }

        if (CurrencyManager.Instance == null)
        {
            Debug.LogError($"{nameof(CurrencyManager)} is required but not found!");
            return false;
        }

        return true;
    }

    private void Start()
    {
        if (!CheckDependencies())
        {
            enabled = false; // 禁用此Manager
            return;
        }

        // 正常初始化
        Initialize();
    }
}
```

## Manager之间的通信

### 1. 事件订阅模式

```csharp
public class EventManager : SingletonBehaviour<EventManager>
{
    public event Action<int> OnScoreChanged;

    public void UpdateScore(int newScore)
    {
        OnScoreChanged?.Invoke(newScore);
    }
}

public class UIManager : SingletonBehaviour<UIManager>
{
    private void Start()
    {
        // 订阅事件
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnScoreChanged += HandleScoreChanged;
        }
    }

    protected override void OnDestroy()
    {
        // 必须取消订阅
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnScoreChanged -= HandleScoreChanged;
        }

        base.OnDestroy();
    }

    private void HandleScoreChanged(int score)
    {
        // 处理分数变化
    }
}
```

### 2. 直接调用模式

```csharp
public class GameManager : SingletonBehaviour<GameManager>
{
    public void StartNewGame()
    {
        // 重置各个系统
        ScoreManager.Instance?.ResetScore();
        LevelManager.Instance?.LoadLevel(1);
        UIManager.Instance?.ShowGameplayUI();
    }
}
```

## 特殊情况处理

### GameManager特殊规则

```csharp
// GameManager必须在场景中预先配置，不会自动创建
[DefaultExecutionOrder(-1000)] // 最高优先级
public class GameManager : SingletonBehaviour<GameManager>
{
    public override void Awake()
    {
        // 防止重复实例的特殊处理
        if (Instance != null && Instance != this)
        {
            Debug.LogError($"Duplicate GameManager detected!");
            MigrateChildrenToExistingInstance();
            Destroy(gameObject);
            return;
        }

        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    private void MigrateChildrenToExistingInstance()
    {
        // 迁移子节点到现有实例
        while (transform.childCount > 0)
        {
            transform.GetChild(0).SetParent(Instance.transform, false);
        }
    }
}
```

### 自动创建实例

```csharp
// 大多数Manager可以自动创建（除了GameManager）
public class AutoCreateManager : SingletonBehaviour<AutoCreateManager>
{
    // 如果场景中没有，会自动创建在GameManagers节点下
    // 无需特殊处理
}
```

## 常见错误和解决方案

### 错误1：在Awake中访问其他单例

```csharp
// ❌ 错误：Awake执行顺序不确定
public override void Awake()
{
    base.Awake();
    var data = DataManager.Instance.GetData(); // 可能为null
}

// ✅ 正确：在Start中访问
private void Start()
{
    if (DataManager.Instance != null)
    {
        var data = DataManager.Instance.GetData();
    }
}
```

### 错误2：循环依赖

```csharp
// ❌ 避免循环依赖
public class ManagerA : SingletonBehaviour<ManagerA>
{
    private void Start()
    {
        ManagerB.Instance.RegisterA(this);
    }
}

public class ManagerB : SingletonBehaviour<ManagerB>
{
    private void Start()
    {
        ManagerA.Instance.RegisterB(this); // 循环依赖
    }
}

// ✅ 使用事件解耦
public class ManagerA : SingletonBehaviour<ManagerA>
{
    public event Action OnReady;

    private void Start()
    {
        OnReady?.Invoke();
    }
}
```

### 错误3：忘记调用基类方法

```csharp
// ❌ 错误：没有调用base.Awake()
public override void Awake()
{
    // 忘记调用base.Awake()
    Initialize();
}

// ✅ 正确：必须调用基类方法
public override void Awake()
{
    base.Awake(); // 必须调用
    Initialize();
}

protected override void OnDestroy()
{
    Cleanup();
    base.OnDestroy(); // 必须调用
}
```

## 最佳实践总结

### DO（应该做）:
- ✅ 使用Instance属性（大写）访问单例
- ✅ 在Start而不是Awake中访问其他单例
- ✅ 检查null后再访问Instance
- ✅ 在OnDestroy中取消事件订阅
- ✅ 调用基类的Awake和OnDestroy
- ✅ 使用InitPriority控制初始化顺序
- ✅ 使用DontDestroyOnSceneChange控制持久化

### DON'T（不要做）:
- ❌ 使用instance（小写）
- ❌ 自己实现单例模式
- ❌ 在Awake中访问其他单例
- ❌ 忘记null检查
- ❌ 忘记取消事件订阅
- ❌ 创建循环依赖
- ❌ 忘记调用基类方法

## 单例使用检查清单

提交代码前检查：

- [ ] 继承自SingletonBehaviour<T>
- [ ] 使用Instance（大写）访问
- [ ] 在Start中访问其他单例
- [ ] 访问前进行null检查
- [ ] 事件在OnDestroy中取消订阅
- [ ] 调用了base.Awake()和base.OnDestroy()
- [ ] 设置了正确的InitPriority（如有依赖）
- [ ] 设置了正确的DontDestroyOnSceneChange