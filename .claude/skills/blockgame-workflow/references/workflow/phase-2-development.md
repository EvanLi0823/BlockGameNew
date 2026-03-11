# Phase 2: 功能开发

## 阶段目标

按照Phase 1的架构设计和Phase 1.5的用户确认，实现功能代码。

## 必须使用的Agent

**unity-developer** - 功能开发工程师

调用方式：
```
"请使用 unity-developer 代理实现这个功能"
```

## 开发顺序

**必须按照以下顺序开发**：

### 1. 数据模型（Data Layer）

**ScriptableObject配置**：
```csharp
// 文件路径：Assets/BlockPuzzleGameToolkit/Scripts/Settings/FeatureSettings.cs
using UnityEngine;
using System.Collections.Generic;
using BlockPuzzleGameToolkit.Scripts.GameCore;

[CreateAssetMenu(fileName = "FeatureSettings", menuName = "Settings/Feature Settings")]
public class FeatureSettings : SingletonScriptableSettings<FeatureSettings>
{
    [Header("基础配置")]
    public int maxCount = 10;
    public float duration = 5f;

    [Header("高级配置")]
    public bool enableDebug = false;
    public List<FeatureData> featureList = new List<FeatureData>();
}
```

**数据类**：
```csharp
// 文件路径：Assets/BlockPuzzleGameToolkit/Scripts/FeatureSystem/Data/FeatureData.cs
using System;

namespace BlockPuzzleGameToolkit.Scripts.FeatureSystem.Data
{
    [Serializable]
    public class FeatureData
    {
        public string id;
        public int value;

        public FeatureData(string id, int value)
        {
            this.id = id;
            this.value = value;
        }
    }
}
```

**开发后立即验证**：
```csharp
// 刷新Unity
mcp__UnityMCP__refresh_unity(compile: "request", mode: "force")

// 检查错误
mcp__UnityMCP__read_console(types: ["error"])
```

### 2. 核心逻辑（Manager Layer）

**Manager实现**：
```csharp
// 文件路径：Assets/BlockPuzzleGameToolkit/Scripts/FeatureSystem/Core/FeatureManager.cs
using UnityEngine;
using System.Collections.Generic;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using BlockPuzzleGameToolkit.Scripts.FeatureSystem.Data;

namespace BlockPuzzleGameToolkit.Scripts.FeatureSystem.Core
{
    public class FeatureManager : SingletonBehaviour<FeatureManager>
    {
        protected override bool DontDestroyOnSceneChange => true;
        protected override int InitializationOrder => 20;

        private FeatureSettings _settings;
        private List<FeatureData> _activeFeatures = new List<FeatureData>();

        protected override void Awake()
        {
            base.Awake();
            // 不在Awake中访问其他单例
        }

        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            // 加载配置（安全调用模板）
            _settings = FeatureSettings.Instance;
            if (_settings == null)
            {
                Debug.LogError($"{nameof(FeatureSettings)}配置文件不存在");
                return;
            }

            // 加载存储的数据
            LoadData();
        }

        private void LoadData()
        {
            var storageManager = StorageManager.Instance;
            if (storageManager != null)
            {
                var savedData = storageManager.LoadData<List<FeatureData>>("feature_data");
                if (savedData != null)
                {
                    _activeFeatures = savedData;
                }
            }
            else
            {
                Debug.LogWarning($"{nameof(StorageManager)}未找到");
            }
        }

        // 公共API
        public void AddFeature(string id, int value)
        {
            if (HasFeature(id))
            {
                Debug.LogWarning($"Feature {id} already exists");
                return;
            }

            if (_activeFeatures.Count >= _settings.maxCount)
            {
                Debug.LogWarning($"达到最大数量限制: {_settings.maxCount}");
                return;
            }

            var feature = new FeatureData(id, value);
            _activeFeatures.Add(feature);

            SaveData();
            NotifyFeatureAdded(feature);
        }

        public void RemoveFeature(string id)
        {
            var feature = _activeFeatures.Find(f => f.id == id);
            if (feature != null)
            {
                _activeFeatures.Remove(feature);
                SaveData();
                NotifyFeatureRemoved(id);
            }
        }

        public FeatureData GetFeature(string id)
        {
            return _activeFeatures.Find(f => f.id == id);
        }

        public bool HasFeature(string id)
        {
            return _activeFeatures.Exists(f => f.id == id);
        }

        public int GetFeatureCount()
        {
            return _activeFeatures.Count;
        }

        private void SaveData()
        {
            var storageManager = StorageManager.Instance;
            if (storageManager != null)
            {
                storageManager.SaveData("feature_data", _activeFeatures);
            }
        }

        private void NotifyFeatureAdded(FeatureData feature)
        {
            var eventManager = EventManager.Instance;
            if (eventManager != null)
            {
                eventManager.Publish(new FeatureAddedEvent { FeatureData = feature });
            }
        }

        private void NotifyFeatureRemoved(string id)
        {
            var eventManager = EventManager.Instance;
            if (eventManager != null)
            {
                eventManager.Publish(new FeatureRemovedEvent { FeatureId = id });
            }
        }
    }
}
```

**事件定义**：
```csharp
// 文件路径：Assets/BlockPuzzleGameToolkit/Scripts/FeatureSystem/Data/FeatureEvents.cs
namespace BlockPuzzleGameToolkit.Scripts.FeatureSystem.Data
{
    public class FeatureAddedEvent
    {
        public FeatureData FeatureData;
    }

    public class FeatureRemovedEvent
    {
        public string FeatureId;
    }
}
```

**开发后立即验证**：
```csharp
// 刷新Unity
mcp__UnityMCP__refresh_unity(compile: "request", mode: "force")

// 检查错误
mcp__UnityMCP__read_console(types: ["error"])
```

### 3. UI界面（UI Layer）

**UI MonoBehaviour**：
```csharp
// 文件路径：Assets/BlockPuzzleGameToolkit/Scripts/FeatureSystem/UI/FeaturePanel.cs
using UnityEngine;
using UnityEngine.UI;
using BlockPuzzleGameToolkit.Scripts.FeatureSystem.Core;
using BlockPuzzleGameToolkit.Scripts.FeatureSystem.Data;
using BlockPuzzleGameToolkit.Scripts.GameCore;

namespace BlockPuzzleGameToolkit.Scripts.FeatureSystem.UI
{
    public class FeaturePanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button addButton;
        [SerializeField] private Text countText;

        private void OnEnable()
        {
            // 订阅事件
            var eventManager = EventManager.Instance;
            if (eventManager != null)
            {
                eventManager.Subscribe<FeatureAddedEvent>(OnFeatureAdded);
                eventManager.Subscribe<FeatureRemovedEvent>(OnFeatureRemoved);
            }

            UpdateUI();
        }

        private void OnDisable()
        {
            // 取消订阅
            var eventManager = EventManager.Instance;
            if (eventManager != null)
            {
                eventManager.Unsubscribe<FeatureAddedEvent>(OnFeatureAdded);
                eventManager.Unsubscribe<FeatureRemovedEvent>(OnFeatureRemoved);
            }
        }

        private void Start()
        {
            // 绑定按钮事件
            if (addButton != null)
            {
                addButton.onClick.AddListener(OnAddButtonClick);
            }
        }

        private void OnAddButtonClick()
        {
            var featureManager = FeatureManager.Instance;
            if (featureManager != null)
            {
                string id = $"feature_{System.Guid.NewGuid()}";
                featureManager.AddFeature(id, 100);
            }
            else
            {
                Debug.LogWarning($"{nameof(FeatureManager)}未找到");
            }
        }

        private void OnFeatureAdded(FeatureAddedEvent evt)
        {
            UpdateUI();
        }

        private void OnFeatureRemoved(FeatureRemovedEvent evt)
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            var featureManager = FeatureManager.Instance;
            if (featureManager != null && countText != null)
            {
                int count = featureManager.GetFeatureCount();
                countText.text = $"Features: {count}";
            }
        }
    }
}
```

**开发后立即验证**：
```csharp
// 刷新Unity
mcp__UnityMCP__refresh_unity(compile: "request", mode: "force")

// 检查错误和警告
mcp__UnityMCP__read_console(types: ["error", "warning"])
```

## 增量开发原则

### 每5-10行代码编译一次

```
编写数据类 → 编译 → 检查错误
编写Manager → 编译 → 检查错误
编写UI → 编译 → 检查错误
```

### 每完成一个文件立即验证

```bash
# 1. 保存文件
# 2. 刷新Unity
mcp__UnityMCP__refresh_unity(compile: "request", mode: "force")

# 3. 检查错误
mcp__UnityMCP__read_console(types: ["error"])

# 4. 如果有错误，立即修复
# 5. 修复后再次编译
# 6. 确认无错误后再继续下一个文件
```

## 开发规范检查

**P0: 硬性约束**（必须遵守）：
- [ ] 只修改 `/Scripts` 目录
- [ ] Instance（大写）
- [ ] Using语句完整
- [ ] 命名空间正确

**P1: 代码质量**：
- [ ] 明确访问修饰符（private/public）
- [ ] 使用nameof避免硬编码字符串
- [ ] 异常处理（try-catch外部调用）
- [ ] 安全调用模板（所有单例访问都检查null）

**P2: 项目架构**：
- [ ] 正确使用SingletonBehaviour
- [ ] 事件订阅/取消订阅
- [ ] 在Start中访问其他单例（不在Awake中）
- [ ] 在OnDisable中取消订阅（不在OnDestroy中）

**P3: 性能优化**：
- [ ] 组件引用已缓存
- [ ] Update中无GC Alloc
- [ ] 字符串拼接使用StringBuilder（如有大量拼接）

**详细规范**：参考 `block-unity-standards` 技能

## 参考质询报告

如果执行了Phase 1.25设计质询，开发时要注意质询报告中的问题：

**高优先级问题**：
- 必须在实现中解决
- 参考质询报告的建议方案

**中优先级问题**：
- 在实现时尽量处理
- 如果成本过高，记录技术债

**边界情况**：
- 实现异常处理
- 添加参数验证
- 处理极端情况

## 常见错误

### ❌ 错误1：跳过增量编译

**症状**：
- 一次修改多个文件
- 积累大量编译错误
- 不知道哪个修改导致错误

**解决**：
- 每完成一个文件立即编译
- 每5-10行代码编译一次

### ❌ 错误2：在Awake中访问其他单例

```csharp
// ❌ 错误
protected override void Awake()
{
    base.Awake();
    var manager = OtherManager.Instance; // 可能还未初始化
}

// ✅ 正确
private void Start()
{
    var manager = OtherManager.Instance;
    if (manager != null)
    {
        // 安全访问
    }
}
```

### ❌ 错误3：不检查null就直接调用

```csharp
// ❌ 错误
StorageManager.Instance.SaveData("key", value);

// ✅ 正确
var storageManager = StorageManager.Instance;
if (storageManager != null)
{
    storageManager.SaveData("key", value);
}
else
{
    Debug.LogWarning($"{nameof(StorageManager)}未找到");
}
```

### ❌ 错误4：在OnDestroy中取消订阅

```csharp
// ❌ 错误
private void OnDestroy()
{
    EventManager.Instance.Unsubscribe<Event>(Handler);
}

// ✅ 正确
private void OnDisable()
{
    var eventManager = EventManager.Instance;
    if (eventManager != null)
    {
        eventManager.Unsubscribe<Event>(Handler);
    }
}
```

## 开发检查清单

- [ ] 按顺序开发（数据模型 → 核心逻辑 → UI界面）
- [ ] 每个文件完成后立即编译验证
- [ ] 所有单例访问都使用安全调用模板
- [ ] 在Start中访问其他单例（不在Awake）
- [ ] 在OnDisable中取消订阅（不在OnDestroy）
- [ ] 使用nameof记录日志
- [ ] 明确访问修饰符
- [ ] Using语句完整
- [ ] 命名空间正确
- [ ] 无编译错误和警告

## 下一步

完成开发后，进入 **Phase 3: 代码审查**
