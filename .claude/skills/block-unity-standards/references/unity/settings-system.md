# 配置系统

## ScriptableObject配置规范

BlockGame使用ScriptableObject作为配置系统的基础，提供可视化编辑和热更新能力。

## 基础配置类

### SettingsBase基类
```csharp
public abstract class SettingsBase : ScriptableObject
{
    // 所有配置类都应继承此基类
    // 提供通用功能和接口
}
```

### 创建配置类
```csharp
[CreateAssetMenu(fileName = "ShopSettings", menuName = "BlockPuzzle/Settings/Shop Settings")]
public class ShopSettings : SettingsBase
{
    [Header("基础配置")]
    [Tooltip("商店刷新间隔（小时）")]
    [Range(1, 24)]
    public int refreshIntervalHours = 6;

    [Header("道具配置")]
    [Tooltip("道具价格列表")]
    public List<PropPriceConfig> propPrices;

    [Header("折扣设置")]
    [Tooltip("是否启用每日折扣")]
    public bool enableDailyDiscount = true;

    [Tooltip("折扣比例")]
    [Range(0.1f, 1f)]
    public float discountRate = 0.8f;

    [System.Serializable]
    public class PropPriceConfig
    {
        public PropType propType;
        public int basePrice;
        public int vipPrice;
        public bool allowDiscount = true;
    }
}
```

## 配置文件组织

### 目录结构
```
Assets/
├── BlockPuzzleGameToolkit/
│   ├── Resources/
│   │   └── Settings/           # 配置文件存放位置
│   │       ├── GameSettings.asset
│   │       ├── PropSettings.asset
│   │       ├── ShopSettings.asset
│   │       └── ...
│   └── Scripts/
│       └── Settings/           # 配置类脚本
│           ├── GameSettings.cs
│           ├── PropSettings.cs
│           └── ...
```

### 创建配置资产
```csharp
// 在Editor中创建配置资产的菜单集成
public static class EditorMenu
{
    [MenuItem("BlockPuzzle/Create/Settings/Game Settings")]
    public static void CreateGameSettings()
    {
        CreateSettingsAsset<GameSettings>("GameSettings");
    }

    [MenuItem("BlockPuzzle/Create/Settings/Shop Settings")]
    public static void CreateShopSettings()
    {
        CreateSettingsAsset<ShopSettings>("ShopSettings");
    }

    private static void CreateSettingsAsset<T>(string name) where T : ScriptableObject
    {
        string path = "Assets/BlockPuzzleGameToolkit/Resources/Settings/";

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        T asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path + name + ".asset");
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }
}
```

## 配置加载和使用

### 单例配置模式
```csharp
public class ConfigManager : SingletonBehaviour<ConfigManager>
{
    private GameSettings gameSettings;
    private ShopSettings shopSettings;

    public GameSettings GameSettings => gameSettings;
    public ShopSettings ShopSettings => shopSettings;

    public override void Awake()
    {
        base.Awake();
        LoadAllSettings();
    }

    private void LoadAllSettings()
    {
        gameSettings = Resources.Load<GameSettings>("Settings/GameSettings");
        shopSettings = Resources.Load<ShopSettings>("Settings/ShopSettings");

        if (gameSettings == null)
        {
            Debug.LogError("GameSettings not found!");
        }
    }
}
```

### 直接加载模式
```csharp
public class ShopManager : SingletonBehaviour<ShopManager>
{
    private ShopSettings settings;

    private void Start()
    {
        // 直接加载配置
        settings = Resources.Load<ShopSettings>("Settings/ShopSettings");

        if (settings == null)
        {
            Debug.LogError("ShopSettings not found!");
            enabled = false;
            return;
        }

        Initialize();
    }

    private void Initialize()
    {
        // 使用配置
        SetRefreshTimer(settings.refreshIntervalHours);

        foreach (var price in settings.propPrices)
        {
            RegisterPropPrice(price.propType, price.basePrice);
        }
    }
}
```

## 高级配置模式

### 配置验证
```csharp
[CreateAssetMenu(fileName = "LevelSettings", menuName = "BlockPuzzle/Settings/Level Settings")]
public class LevelSettings : SettingsBase
{
    [Header("关卡配置")]
    [SerializeField] private List<LevelConfig> levels;

    // 在编辑器中验证配置
    private void OnValidate()
    {
        // 验证关卡配置
        for (int i = 0; i < levels.Count; i++)
        {
            var level = levels[i];

            // 确保关卡ID连续
            level.levelId = i + 1;

            // 验证分数阈值
            if (level.targetScore < 100)
            {
                Debug.LogWarning($"Level {level.levelId} target score too low!");
                level.targetScore = 100;
            }

            // 验证时间限制
            if (level.timeLimit < 30)
            {
                level.timeLimit = 30;
            }
        }
    }

    [System.Serializable]
    public class LevelConfig
    {
        [HideInInspector]
        public int levelId;

        public int targetScore;
        public int timeLimit;
        public int moveLimit;
        public List<BlockType> availableBlocks;
    }
}
```

### 配置继承
```csharp
// 基础配置
public abstract class BaseEnemySettings : SettingsBase
{
    [Header("基础属性")]
    public float baseHealth = 100;
    public float baseDamage = 10;
    public float moveSpeed = 5;
}

// 具体敌人配置
[CreateAssetMenu(fileName = "ZombieSettings", menuName = "BlockPuzzle/Settings/Enemies/Zombie")]
public class ZombieSettings : BaseEnemySettings
{
    [Header("僵尸特有属性")]
    public float infectionRadius = 2f;
    public float infectionChance = 0.3f;
}
```

### 配置预设系统
```csharp
[CreateAssetMenu(fileName = "DifficultyPreset", menuName = "BlockPuzzle/Settings/Difficulty Preset")]
public class DifficultyPreset : SettingsBase
{
    public enum Difficulty
    {
        Easy,
        Normal,
        Hard,
        Expert
    }

    [Header("难度设置")]
    public Difficulty difficulty;

    [Header("游戏参数")]
    public float scoreMultiplier = 1f;
    public float timeMultiplier = 1f;
    public int startingLives = 3;

    [Header("敌人参数")]
    public float enemyHealthMultiplier = 1f;
    public float enemySpeedMultiplier = 1f;
    public float spawnRateMultiplier = 1f;

    // 工厂方法创建预设
    public static DifficultyPreset CreatePreset(Difficulty diff)
    {
        var preset = CreateInstance<DifficultyPreset>();

        switch (diff)
        {
            case Difficulty.Easy:
                preset.scoreMultiplier = 0.8f;
                preset.timeMultiplier = 1.5f;
                preset.startingLives = 5;
                preset.enemyHealthMultiplier = 0.7f;
                preset.enemySpeedMultiplier = 0.8f;
                preset.spawnRateMultiplier = 0.6f;
                break;

            case Difficulty.Hard:
                preset.scoreMultiplier = 1.5f;
                preset.timeMultiplier = 0.8f;
                preset.startingLives = 2;
                preset.enemyHealthMultiplier = 1.5f;
                preset.enemySpeedMultiplier = 1.3f;
                preset.spawnRateMultiplier = 1.5f;
                break;
        }

        return preset;
    }
}
```

## 配置热更新

```csharp
public class HotReloadableSettings : SettingsBase
{
    // 配置变更事件
    public static event Action OnSettingsChanged;

    #if UNITY_EDITOR
    private void OnValidate()
    {
        // 编辑器中修改后立即生效
        OnSettingsChanged?.Invoke();
    }
    #endif
}

// 使用热更新配置
public class GameplayManager : MonoBehaviour
{
    private HotReloadableSettings settings;

    private void Start()
    {
        settings = Resources.Load<HotReloadableSettings>("Settings/GameplaySettings");
        HotReloadableSettings.OnSettingsChanged += RefreshSettings;
        RefreshSettings();
    }

    private void OnDestroy()
    {
        HotReloadableSettings.OnSettingsChanged -= RefreshSettings;
    }

    private void RefreshSettings()
    {
        // 重新应用配置
        ApplySettings();
    }
}
```

## 配置最佳实践

### DO（推荐）:
- ✅ 继承SettingsBase基类
- ✅ 使用CreateAssetMenu属性
- ✅ 添加Header和Tooltip属性说明
- ✅ 使用Range限制数值范围
- ✅ 配置文件放在Resources/Settings目录
- ✅ 使用OnValidate验证配置
- ✅ 使用[System.Serializable]标记嵌套类
- ✅ 提供默认值
- ✅ 配置分组使用[Header]

### DON'T（避免）:
- ❌ 在配置中存储运行时状态
- ❌ 在配置中使用非序列化类型
- ❌ 硬编码配置路径
- ❌ 忘记null检查
- ❌ 配置文件命名不规范
- ❌ 过度嵌套配置结构

## 配置系统检查清单

- [ ] 继承自SettingsBase
- [ ] 有CreateAssetMenu属性
- [ ] 字段有Tooltip说明
- [ ] 数值有Range限制
- [ ] 配置文件在正确目录
- [ ] 有OnValidate验证
- [ ] 嵌套类标记[Serializable]
- [ ] 有合理的默认值
- [ ] 使用Header分组
- [ ] 加载时有null检查