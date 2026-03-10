# 内存管理

## Unity内存管理原则

合理管理内存分配和释放，避免内存泄漏和频繁GC。

## 内存分配优化

### 避免频繁分配

```csharp
// ❌ 错误：频繁创建临时对象
void Update()
{
    // 每帧new一个数组
    int[] numbers = new int[100];

    // 每帧创建List
    List<Enemy> enemies = new List<Enemy>();

    // 每帧创建字符串
    string info = "HP: " + health + " MP: " + mana;
}

// ✅ 正确：重用对象
public class MemoryOptimized : MonoBehaviour
{
    // 预分配并重用
    private int[] numbers = new int[100];
    private List<Enemy> enemies = new List<Enemy>(50);
    private StringBuilder stringBuilder = new StringBuilder();

    void Update()
    {
        // 清空后重用
        enemies.Clear();

        // 使用StringBuilder
        stringBuilder.Clear();
        stringBuilder.Append("HP: ").Append(health).Append(" MP: ").Append(mana);
    }
}
```

### 数组vs List选择

```csharp
// ✅ 固定大小用数组
private Enemy[] fixedEnemies = new Enemy[10];

// ✅ 动态大小用List，但要预设容量
private List<Enemy> dynamicEnemies = new List<Enemy>(100);

// ❌ 避免频繁扩容
private List<Enemy> badList = new List<Enemy>(); // 默认容量太小
```

## 对象池实现

### 通用对象池

```csharp
public class ObjectPool<T> where T : Component
{
    private readonly Stack<T> pool = new Stack<T>();
    private readonly T prefab;
    private readonly Transform container;
    private readonly int maxSize;

    public ObjectPool(T prefab, int initialSize, int maxSize = 100)
    {
        this.prefab = prefab;
        this.maxSize = maxSize;

        // 创建容器
        container = new GameObject($"{typeof(T).Name}Pool").transform;

        // 预创建对象
        for (int i = 0; i < initialSize; i++)
        {
            CreateNewObject();
        }
    }

    private T CreateNewObject()
    {
        var obj = Object.Instantiate(prefab, container);
        obj.gameObject.SetActive(false);
        return obj;
    }

    public T Get()
    {
        T obj = pool.Count > 0 ? pool.Pop() : CreateNewObject();
        obj.gameObject.SetActive(true);
        return obj;
    }

    public void Return(T obj)
    {
        if (pool.Count >= maxSize)
        {
            Object.Destroy(obj.gameObject);
            return;
        }

        obj.gameObject.SetActive(false);
        obj.transform.SetParent(container);
        pool.Push(obj);
    }

    public void Clear()
    {
        while (pool.Count > 0)
        {
            var obj = pool.Pop();
            if (obj != null)
                Object.Destroy(obj.gameObject);
        }
    }
}
```

### 特效对象池

```csharp
public class ParticlePoolManager : SingletonBehaviour<ParticlePoolManager>
{
    [System.Serializable]
    public class PoolConfig
    {
        public string poolName;
        public GameObject prefab;
        public int initialSize = 10;
        public int maxSize = 50;
    }

    [SerializeField] private List<PoolConfig> poolConfigs;
    private Dictionary<string, ObjectPool<ParticleSystem>> pools;

    protected override void Awake()
    {
        base.Awake();
        InitializePools();
    }

    private void InitializePools()
    {
        pools = new Dictionary<string, ObjectPool<ParticleSystem>>();

        foreach (var config in poolConfigs)
        {
            var particleSystem = config.prefab.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                var pool = new ObjectPool<ParticleSystem>(
                    particleSystem,
                    config.initialSize,
                    config.maxSize
                );
                pools[config.poolName] = pool;
            }
        }
    }

    public void PlayEffect(string effectName, Vector3 position)
    {
        if (pools.TryGetValue(effectName, out var pool))
        {
            var effect = pool.Get();
            effect.transform.position = position;
            effect.Play();

            // 自动回收
            StartCoroutine(ReturnToPool(effect, pool));
        }
    }

    private IEnumerator ReturnToPool(ParticleSystem effect, ObjectPool<ParticleSystem> pool)
    {
        yield return new WaitForSeconds(effect.main.duration + effect.main.startLifetime.constantMax);
        pool.Return(effect);
    }
}
```

## 资源管理

### Resources管理

```csharp
public class ResourceManager : SingletonBehaviour<ResourceManager>
{
    private Dictionary<string, UnityEngine.Object> cache = new Dictionary<string, UnityEngine.Object>();

    public T Load<T>(string path) where T : UnityEngine.Object
    {
        string key = $"{typeof(T).Name}_{path}";

        if (!cache.TryGetValue(key, out var resource))
        {
            resource = Resources.Load<T>(path);
            if (resource != null)
                cache[key] = resource;
        }

        return resource as T;
    }

    public void Unload(string path)
    {
        var keysToRemove = cache.Keys.Where(k => k.Contains(path)).ToList();
        foreach (var key in keysToRemove)
        {
            cache.Remove(key);
        }
    }

    public void UnloadAll()
    {
        cache.Clear();
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
    }

    protected override void OnDestroy()
    {
        UnloadAll();
        base.OnDestroy();
    }
}
```

### Texture内存优化

```csharp
public class TextureManager : SingletonBehaviour<TextureManager>
{
    [System.Serializable]
    public class TextureSettings
    {
        public int maxTextureSize = 1024;
        public TextureFormat format = TextureFormat.RGBA32;
        public bool generateMipmaps = false;
    }

    [SerializeField] private TextureSettings settings;

    public Texture2D LoadAndOptimizeTexture(string path)
    {
        var texture = Resources.Load<Texture2D>(path);
        if (texture == null) return null;

        // 如果纹理太大，缩小它
        if (texture.width > settings.maxTextureSize || texture.height > settings.maxTextureSize)
        {
            texture = ResizeTexture(texture, settings.maxTextureSize);
        }

        // 压缩纹理
        texture.Compress(true);

        return texture;
    }

    private Texture2D ResizeTexture(Texture2D source, int maxSize)
    {
        float scale = Mathf.Min((float)maxSize / source.width, (float)maxSize / source.height);
        int newWidth = Mathf.RoundToInt(source.width * scale);
        int newHeight = Mathf.RoundToInt(source.height * scale);

        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        Graphics.Blit(source, rt);

        Texture2D result = new Texture2D(newWidth, newHeight, settings.format, settings.generateMipmaps);

        RenderTexture.active = rt;
        result.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        result.Apply();

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        return result;
    }
}
```

## 内存泄漏防范

### 事件订阅管理

```csharp
public class SafeEventManager : SingletonBehaviour<SafeEventManager>
{
    private Dictionary<object, List<System.Delegate>> subscriptions =
        new Dictionary<object, List<System.Delegate>>();

    public void Subscribe<T>(object subscriber, Action<T> handler) where T : struct
    {
        if (!subscriptions.ContainsKey(subscriber))
            subscriptions[subscriber] = new List<System.Delegate>();

        subscriptions[subscriber].Add(handler);
        EventBus<T>.Subscribe(handler);
    }

    public void UnsubscribeAll(object subscriber)
    {
        if (subscriptions.TryGetValue(subscriber, out var handlers))
        {
            foreach (var handler in handlers)
            {
                // 使用反射取消订阅
                var method = typeof(EventBus<>)
                    .MakeGenericType(handler.Method.GetParameters()[0].ParameterType)
                    .GetMethod("Unsubscribe");

                method?.Invoke(null, new object[] { handler });
            }

            subscriptions.Remove(subscriber);
        }
    }
}

// 使用示例
public class SafeSubscriber : MonoBehaviour
{
    void Start()
    {
        SafeEventManager.Instance.Subscribe<GameStartEvent>(this, OnGameStart);
    }

    void OnDestroy()
    {
        SafeEventManager.Instance?.UnsubscribeAll(this);
    }

    void OnGameStart(GameStartEvent evt)
    {
        // 处理事件
    }
}
```

### 协程管理

```csharp
public class CoroutineManager : SingletonBehaviour<CoroutineManager>
{
    private Dictionary<object, List<Coroutine>> runningCoroutines =
        new Dictionary<object, List<Coroutine>>();

    public Coroutine StartManagedCoroutine(object owner, IEnumerator routine)
    {
        if (!runningCoroutines.ContainsKey(owner))
            runningCoroutines[owner] = new List<Coroutine>();

        var coroutine = StartCoroutine(routine);
        runningCoroutines[owner].Add(coroutine);

        return coroutine;
    }

    public void StopManagedCoroutines(object owner)
    {
        if (runningCoroutines.TryGetValue(owner, out var coroutines))
        {
            foreach (var coroutine in coroutines)
            {
                if (coroutine != null)
                    StopCoroutine(coroutine);
            }

            runningCoroutines.Remove(owner);
        }
    }

    public void StopAllManagedCoroutines()
    {
        foreach (var kvp in runningCoroutines)
        {
            foreach (var coroutine in kvp.Value)
            {
                if (coroutine != null)
                    StopCoroutine(coroutine);
            }
        }

        runningCoroutines.Clear();
    }
}
```

## 内存分析工具使用

### Profiler内存分析

```csharp
// 标记内存分配区域
using (Profiler.BeginSample("MyCustomAllocation"))
{
    // 需要分析的代码
    var largeArray = new int[10000];
}

// 条件性分析
[System.Diagnostics.Conditional("ENABLE_PROFILER")]
void ProfileMemory()
{
    var memoryBefore = System.GC.GetTotalMemory(false);

    // 执行操作
    PerformOperation();

    var memoryAfter = System.GC.GetTotalMemory(false);
    Debug.Log($"Memory allocated: {memoryAfter - memoryBefore} bytes");
}
```

## 内存管理检查清单

### 开发时检查
- [ ] 对象池用于频繁创建/销毁的对象
- [ ] Update中无内存分配
- [ ] 集合预设合适容量
- [ ] 使用StringBuilder处理字符串
- [ ] 事件正确订阅/取消订阅
- [ ] 协程正确停止
- [ ] 资源正确加载/卸载

### 优化时检查
- [ ] 使用Profiler分析内存
- [ ] 识别内存热点
- [ ] 检查内存泄漏
- [ ] 优化纹理大小
- [ ] 压缩音频资源
- [ ] 清理未使用资源

### 发布前检查
- [ ] 所有对象池正常工作
- [ ] 场景切换无内存泄漏
- [ ] 长时间运行内存稳定
- [ ] Resources.UnloadUnusedAssets调用时机正确