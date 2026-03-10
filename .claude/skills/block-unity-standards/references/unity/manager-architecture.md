# 管理器架构

## Manager系统设计原则

BlockGame使用基于SingletonBehaviour的Manager系统来组织游戏逻辑。

## Manager分类

### 1. 核心系统Manager
负责游戏核心系统功能，通常需要持久化。

```csharp
// GameManager - 游戏总控制器
public class GameManager : SingletonBehaviour<GameManager>
{
    protected override bool DontDestroyOnSceneChange => true;
    public override int InitPriority => 0; // 最高优先级
}

// StorageManager - 数据存储
public class StorageManager : SingletonBehaviour<StorageManager>
{
    protected override bool DontDestroyOnSceneChange => true;
    public override int InitPriority => 1;
}
```

### 2. 游戏逻辑Manager
处理具体游戏逻辑，通常随场景销毁。

```csharp
// LevelManager - 关卡管理
public class LevelManager : SingletonBehaviour<LevelManager>
{
    protected override bool DontDestroyOnSceneChange => false;
}

// FieldManager - 游戏场地管理
public class FieldManager : SingletonBehaviour<FieldManager>
{
    protected override bool DontDestroyOnSceneChange => false;
}
```

### 3. UI Manager
管理用户界面相关逻辑。

```csharp
public class UIManager : SingletonBehaviour<UIManager>
{
    protected override bool DontDestroyOnSceneChange => false;

    [Header("UI引用")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject gameplayPanel;
    [SerializeField] private GameObject pausePanel;
}
```

## Manager通信模式

### 1. 事件驱动通信

```csharp
public class ScoreManager : SingletonBehaviour<ScoreManager>
{
    // 定义事件
    public event Action<int> OnScoreChanged;
    public event Action<int> OnHighScoreChanged;

    private int currentScore;

    public void AddScore(int points)
    {
        currentScore += points;
        OnScoreChanged?.Invoke(currentScore);

        if (currentScore > GetHighScore())
        {
            SaveHighScore(currentScore);
            OnHighScoreChanged?.Invoke(currentScore);
        }
    }
}

// 订阅者
public class UIScoreDisplay : MonoBehaviour
{
    private void Start()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged += UpdateScoreDisplay;
        }
    }

    private void OnDestroy()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= UpdateScoreDisplay;
        }
    }
}
```

### 2. 直接调用模式

```csharp
public class GameplayController : MonoBehaviour
{
    public void OnBlockPlaced()
    {
        // 直接调用各Manager
        ScoreManager.Instance?.AddScore(10);
        AudioManager.Instance?.PlaySound("BlockPlaced");
        EffectManager.Instance?.PlayEffect("BlockPlaceEffect", transform.position);
    }
}
```

### 3. 命令模式

```csharp
// 命令接口
public interface IGameCommand
{
    void Execute();
    void Undo();
}

// 具体命令
public class PlaceBlockCommand : IGameCommand
{
    private Vector3 position;
    private BlockType blockType;

    public PlaceBlockCommand(Vector3 pos, BlockType type)
    {
        position = pos;
        blockType = type;
    }

    public void Execute()
    {
        FieldManager.Instance?.PlaceBlock(position, blockType);
        ScoreManager.Instance?.AddScore(10);
    }

    public void Undo()
    {
        FieldManager.Instance?.RemoveBlock(position);
        ScoreManager.Instance?.AddScore(-10);
    }
}

// 命令管理器
public class CommandManager : SingletonBehaviour<CommandManager>
{
    private Stack<IGameCommand> commandHistory = new Stack<IGameCommand>();

    public void ExecuteCommand(IGameCommand command)
    {
        command.Execute();
        commandHistory.Push(command);
    }

    public void UndoLastCommand()
    {
        if (commandHistory.Count > 0)
        {
            var command = commandHistory.Pop();
            command.Undo();
        }
    }
}
```

## Manager生命周期最佳实践

### 初始化流程

```csharp
public class ComplexManager : SingletonBehaviour<ComplexManager>
{
    private bool isInitialized = false;

    public override void Awake()
    {
        base.Awake(); // 必须调用
        // 只做最基本的初始化
        LoadConfiguration();
    }

    private void Start()
    {
        // 依赖其他Manager的初始化
        if (!CheckDependencies())
        {
            Debug.LogError("Dependencies not met!");
            enabled = false;
            return;
        }

        Initialize();
        isInitialized = true;
    }

    private bool CheckDependencies()
    {
        return StorageManager.Instance != null &&
               CurrencyManager.Instance != null;
    }

    private void Initialize()
    {
        // 复杂的初始化逻辑
        RegisterEvents();
        LoadData();
        SetupUI();
    }

    protected override void OnDestroy()
    {
        if (isInitialized)
        {
            UnregisterEvents();
            SaveData();
        }

        base.OnDestroy();
    }
}
```

## Manager依赖管理

### 依赖注入模式

```csharp
public class DependentManager : SingletonBehaviour<DependentManager>
{
    // 依赖的Manager
    private StorageManager storageManager;
    private CurrencyManager currencyManager;

    public override int InitPriority => 100; // 低优先级，后初始化

    private void Start()
    {
        // 获取依赖
        storageManager = StorageManager.Instance;
        currencyManager = CurrencyManager.Instance;

        if (storageManager == null || currencyManager == null)
        {
            Debug.LogError("Required dependencies not found!");
            enabled = false;
            return;
        }

        Initialize();
    }

    private void Initialize()
    {
        // 使用依赖进行初始化
        var saveData = storageManager.LoadData<GameData>("game_data");
        var currency = currencyManager.GetCurrency("coins");
    }
}
```

## 常见Manager模式

### 1. 资源管理器

```csharp
public class ResourceManager : SingletonBehaviour<ResourceManager>
{
    private Dictionary<string, UnityEngine.Object> resourceCache =
        new Dictionary<string, UnityEngine.Object>();

    public T LoadResource<T>(string path) where T : UnityEngine.Object
    {
        if (!resourceCache.TryGetValue(path, out var resource))
        {
            resource = Resources.Load<T>(path);
            if (resource != null)
            {
                resourceCache[path] = resource;
            }
        }

        return resource as T;
    }

    public void UnloadResource(string path)
    {
        if (resourceCache.ContainsKey(path))
        {
            resourceCache.Remove(path);
        }
    }

    protected override void OnDestroy()
    {
        resourceCache.Clear();
        Resources.UnloadUnusedAssets();
        base.OnDestroy();
    }
}
```

### 2. 对象池管理器

```csharp
public class PoolManager : SingletonBehaviour<PoolManager>
{
    private Dictionary<string, Queue<GameObject>> pools =
        new Dictionary<string, Queue<GameObject>>();

    public GameObject GetObject(string poolName, GameObject prefab)
    {
        if (!pools.ContainsKey(poolName))
        {
            pools[poolName] = new Queue<GameObject>();
        }

        GameObject obj;
        if (pools[poolName].Count > 0)
        {
            obj = pools[poolName].Dequeue();
            obj.SetActive(true);
        }
        else
        {
            obj = Instantiate(prefab);
        }

        return obj;
    }

    public void ReturnObject(string poolName, GameObject obj)
    {
        obj.SetActive(false);

        if (!pools.ContainsKey(poolName))
        {
            pools[poolName] = new Queue<GameObject>();
        }

        pools[poolName].Enqueue(obj);
    }
}
```

## Manager架构检查清单

- [ ] Manager继承自SingletonBehaviour
- [ ] 设置正确的InitPriority（依赖关系）
- [ ] 设置正确的DontDestroyOnSceneChange
- [ ] 在Start中访问其他Manager
- [ ] 实现完整的生命周期管理
- [ ] 事件正确订阅和取消订阅
- [ ] 资源正确加载和释放
- [ ] 依赖关系清晰明确
- [ ] 错误处理完善
- [ ] 避免循环依赖