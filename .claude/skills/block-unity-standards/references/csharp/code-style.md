# 代码风格指南

## 命名规范

### 类和接口
```csharp
// ✅ 正确：PascalCase
public class PlayerController { }
public interface IDataProvider { }
public enum GameState { }
public struct PlayerData { }

// ❌ 错误
public class playerController { }  // 应该用PascalCase
public interface DataProvider { }  // 接口应该以I开头
```

### 方法和属性
```csharp
public class Example
{
    // ✅ 正确：PascalCase
    public void CalculateScore() { }
    public int MaxHealth { get; set; }

    // ❌ 错误
    public void calculate_score() { }  // 应该用PascalCase
    public int maxHealth { get; set; } // 属性应该用PascalCase
}
```

### 字段和变量
```csharp
public class Example
{
    // ✅ 正确：私有字段用camelCase
    private int currentScore;
    private readonly List<Item> itemList;

    // ✅ 正确：常量用大写蛇形命名
    private const int MAX_RETRY_COUNT = 3;
    private const string DEFAULT_NAME = "Player";

    // ✅ 正确：局部变量用camelCase
    public void Method()
    {
        int localVariable = 0;
        string userName = "test";
    }

    // ❌ 错误
    private int CurrentScore;     // 私有字段应该用camelCase
    private const int MaxRetry = 3; // 常量应该用大写
}
```

### Unity特定命名
```csharp
public class UnityExample : MonoBehaviour
{
    // ✅ Unity序列化字段用camelCase
    [SerializeField] private GameObject targetObject;
    [SerializeField] private float moveSpeed = 5f;

    // ✅ 公开字段（仅在必要时）
    public Transform playerTransform; // Unity Inspector显示

    // ❌ 错误
    [SerializeField] private GameObject TargetObject; // 应该用camelCase
}
```

## 注释规范

### XML文档注释
```csharp
/// <summary>
/// 道具管理器 - 管理所有道具的使用、购买和存储
/// </summary>
public class PropManager : SingletonBehaviour<PropManager>
{
    /// <summary>
    /// 添加道具到背包
    /// </summary>
    /// <param name="propType">道具类型</param>
    /// <param name="count">道具数量</param>
    /// <returns>是否添加成功</returns>
    public bool AddProp(PropType propType, int count)
    {
        // 实现
    }
}
```

### 代码注释
```csharp
public class GameLogic
{
    public void ComplexMethod()
    {
        // 第一步：验证输入数据
        ValidateInput();

        // 第二步：执行核心算法
        // 这里使用了改进的匹配算法，相比原版提升30%性能
        PerformMatching();

        // 第三步：更新游戏状态
        UpdateGameState();
    }

    // TODO: 优化这个方法的性能
    // FIXME: 修复边界条件的bug
    // HACK: 临时解决方案，后续需要重构
}
```

## 代码组织

### 使用#region组织代码
```csharp
public class WellOrganizedClass : SingletonBehaviour<WellOrganizedClass>
{
    #region 常量

    private const int MAX_ITEMS = 100;
    private const string SAVE_KEY = "GameData";

    #endregion

    #region 字段和属性

    private readonly List<Item> items = new List<Item>();
    private int currentIndex;

    public int ItemCount => items.Count;
    public bool IsEmpty => items.Count == 0;

    #endregion

    #region 事件

    public event Action<Item> OnItemAdded;
    public event Action<Item> OnItemRemoved;

    #endregion

    #region Unity生命周期

    public override void Awake()
    {
        base.Awake();
        Initialize();
    }

    private void Start()
    {
        LoadData();
    }

    protected override void OnDestroy()
    {
        SaveData();
        base.OnDestroy();
    }

    #endregion

    #region 公共方法

    public void AddItem(Item item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        items.Add(item);
        OnItemAdded?.Invoke(item);
    }

    public void RemoveItem(Item item)
    {
        if (items.Remove(item))
        {
            OnItemRemoved?.Invoke(item);
        }
    }

    #endregion

    #region 私有方法

    private void Initialize()
    {
        // 初始化逻辑
    }

    private void LoadData()
    {
        // 加载数据
    }

    private void SaveData()
    {
        // 保存数据
    }

    #endregion
}
```

### 文件组织
```
Scripts/
├── GameCore/               # 核心系统
│   ├── SingletonBehaviour.cs
│   ├── GameManager.cs
│   └── ...
├── Gameplay/              # 游戏逻辑
│   ├── Managers/
│   │   ├── LevelManager.cs
│   │   ├── FieldManager.cs
│   │   └── ...
│   ├── Controllers/
│   └── ...
├── UI/                    # 用户界面
│   ├── Panels/
│   ├── Components/
│   └── ...
├── Settings/             # 配置文件
│   ├── GameSettings.cs
│   └── ...
└── Utils/               # 工具类
    └── ...
```

## 格式化规范

### 缩进和空格
```csharp
// ✅ 正确：使用4个空格缩进
public class Example
{
    public void Method()
    {
        if (condition)
        {
            DoSomething();
        }
    }
}

// ✅ 正确：操作符两边加空格
int result = a + b * c;
bool isValid = value > 0 && value < 100;

// ❌ 错误：缺少空格
int result=a+b*c;  // 操作符两边应该有空格
```

### 大括号风格
```csharp
// ✅ 推荐：大括号独占一行
public class Example
{
    public void Method()
    {
        if (condition)
        {
            DoSomething();
        }
        else
        {
            DoSomethingElse();
        }
    }
}

// ✅ 简单语句可以单行
public int Count { get; private set; }
public bool IsEmpty => items.Count == 0;

// ❌ 避免省略大括号
if (condition)
    DoSomething();  // 容易出错，建议加大括号
```

### 行长度限制
```csharp
// ✅ 保持每行不超过120字符
public void ProcessData(
    string userName,
    int userId,
    DateTime createdDate,
    bool isActive)
{
    // 方法体
}

// ✅ 长条件语句换行
if (player.IsAlive &&
    player.Health > 0 &&
    player.HasWeapon &&
    !player.IsStunned)
{
    player.Attack();
}
```

## LINQ使用规范

```csharp
// ✅ 推荐：使用LINQ简化代码
var activeEnemies = enemies
    .Where(e => e.IsActive && e.Health > 0)
    .OrderBy(e => e.Distance)
    .Take(5)
    .ToList();

// ✅ 复杂查询分行
var result = items
    .Where(item => item.Type == ItemType.Weapon)
    .Where(item => item.Level >= minLevel)
    .OrderByDescending(item => item.Power)
    .ThenBy(item => item.Name)
    .Select(item => new WeaponInfo
    {
        Id = item.Id,
        Name = item.Name,
        Damage = item.Power * multiplier
    })
    .ToList();

// ❌ 避免过度使用LINQ在性能关键路径
void Update()
{
    // 不要在Update中使用LINQ
    var nearest = enemies.OrderBy(e => e.Distance).First();
}
```

## 异步编程规范

```csharp
// ✅ 异步方法命名以Async结尾
public async Task<bool> LoadDataAsync()
{
    try
    {
        var data = await ReadFileAsync();
        return ProcessData(data);
    }
    catch (Exception e)
    {
        Debug.LogError($"Failed to load data: {e.Message}");
        return false;
    }
}

// ✅ 使用ConfigureAwait(false)在库代码中
public async Task<string> GetDataAsync()
{
    var result = await HttpClient.GetStringAsync(url)
        .ConfigureAwait(false);
    return result;
}
```

## 代码简洁性

### 使用现代C#特性
```csharp
// ✅ 使用表达式体
public int Count => items.Count;
public bool IsValid => value > 0 && value < 100;

// ✅ 使用模式匹配
if (obj is Player player)
{
    player.TakeDamage(10);
}

// ✅ 使用空合并操作符
string name = userName ?? "Unknown";
int score = GetScore() ?? 0;

// ✅ 使用字符串插值
string message = $"Player {playerName} scored {score} points";
```

### 避免冗余代码
```csharp
// ❌ 冗余
if (isActive == true) { }
if (count > 0)
    return true;
else
    return false;

// ✅ 简洁
if (isActive) { }
return count > 0;
```

## 风格检查清单

- [ ] 类名使用PascalCase
- [ ] 接口以I开头
- [ ] 私有字段使用camelCase
- [ ] 常量使用大写蛇形命名
- [ ] 方法和属性使用PascalCase
- [ ] 有完整的XML文档注释
- [ ] 使用#region组织代码
- [ ] 缩进使用4个空格
- [ ] 操作符两边有空格
- [ ] 行长度不超过120字符
- [ ] 异步方法以Async结尾
- [ ] 使用现代C#特性简化代码
- [ ] 避免冗余代码
- [ ] LINQ查询格式清晰