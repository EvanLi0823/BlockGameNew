# Unity性能优化

## 性能优化原则

优化前先测量，优化后要验证。使用Unity Profiler定位性能瓶颈。

## Update循环优化

### 避免Update中的内存分配

```csharp
// ❌ 错误：Update中分配内存
public class BadExample : MonoBehaviour
{
    void Update()
    {
        // 每帧分配新数组
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        // 每帧分配字符串
        string score = "Score: " + currentScore;

        // 每帧创建List
        List<int> numbers = new List<int>();
    }
}

// ✅ 正确：缓存和重用
public class GoodExample : MonoBehaviour
{
    private GameObject[] enemies;
    private List<int> numbers = new List<int>();
    private StringBuilder scoreBuilder = new StringBuilder();

    void Start()
    {
        enemies = GameObject.FindGameObjectsWithTag("Enemy");
    }

    void Update()
    {
        // 重用已分配的对象
        scoreBuilder.Clear();
        scoreBuilder.Append("Score: ").Append(currentScore);

        numbers.Clear();
        // 使用numbers...
    }
}
```

### 减少Update调用

```csharp
// ❌ 错误：每帧检查
public class BadTimer : MonoBehaviour
{
    private float timer = 0;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= 1.0f)
        {
            DoSomething();
            timer = 0;
        }
    }
}

// ✅ 正确：使用协程或InvokeRepeating
public class GoodTimer : MonoBehaviour
{
    void Start()
    {
        InvokeRepeating(nameof(DoSomething), 1.0f, 1.0f);
        // 或者
        StartCoroutine(TimedAction());
    }

    IEnumerator TimedAction()
    {
        var wait = new WaitForSeconds(1.0f);
        while (true)
        {
            yield return wait;
            DoSomething();
        }
    }
}
```

### 条件检查优化

```csharp
// ✅ 提前返回减少不必要的检查
void Update()
{
    if (!isActive) return;
    if (isPaused) return;
    if (!hasTarget) return;

    // 实际的更新逻辑
    UpdateMovement();
    UpdateAnimation();
}
```

## 组件访问优化

### 缓存组件引用

```csharp
// ❌ 错误：重复获取组件
public class BadComponentAccess : MonoBehaviour
{
    void Update()
    {
        GetComponent<Rigidbody>().velocity = newVelocity;
        GetComponent<Renderer>().material.color = newColor;
    }
}

// ✅ 正确：缓存组件引用
public class GoodComponentAccess : MonoBehaviour
{
    private Rigidbody rb;
    private Renderer rend;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        rb.velocity = newVelocity;
        rend.material.color = newColor;
    }
}
```

### 使用TryGetComponent

```csharp
// ❌ 错误：GetComponent + null检查
var health = target.GetComponent<Health>();
if (health != null)
{
    health.TakeDamage(10);
}

// ✅ 正确：TryGetComponent
if (target.TryGetComponent<Health>(out var health))
{
    health.TakeDamage(10);
}
```

## GameObject查找优化

### 避免运行时查找

```csharp
// ❌ 错误：运行时频繁查找
void Update()
{
    GameObject player = GameObject.Find("Player");
    GameObject manager = GameObject.FindWithTag("GameManager");
}

// ✅ 正确：启动时查找并缓存
public class CachedReferences : MonoBehaviour
{
    private GameObject player;
    private GameManager gameManager;

    void Start()
    {
        player = GameObject.Find("Player");
        gameManager = GameObject.FindWithTag("GameManager")?.GetComponent<GameManager>();
    }
}

// ✅ 更好：使用单例或依赖注入
void Start()
{
    var gameManager = GameManager.Instance;
}
```

## 内存优化

### 对象池模式

```csharp
public class ObjectPool<T> where T : Component
{
    private readonly Queue<T> pool = new Queue<T>();
    private readonly T prefab;
    private readonly Transform parent;

    public ObjectPool(T prefab, int initialSize, Transform parent = null)
    {
        this.prefab = prefab;
        this.parent = parent;

        // 预分配对象
        for (int i = 0; i < initialSize; i++)
        {
            var obj = Object.Instantiate(prefab, parent);
            obj.gameObject.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public T Get()
    {
        T obj;
        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
        }
        else
        {
            obj = Object.Instantiate(prefab, parent);
        }

        obj.gameObject.SetActive(true);
        return obj;
    }

    public void Return(T obj)
    {
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
    }
}

// 使用对象池
public class BulletManager : SingletonBehaviour<BulletManager>
{
    [SerializeField] private Bullet bulletPrefab;
    private ObjectPool<Bullet> bulletPool;

    void Start()
    {
        bulletPool = new ObjectPool<Bullet>(bulletPrefab, 50, transform);
    }

    public void FireBullet(Vector3 position, Vector3 direction)
    {
        var bullet = bulletPool.Get();
        bullet.transform.position = position;
        bullet.Fire(direction);
    }

    public void ReturnBullet(Bullet bullet)
    {
        bulletPool.Return(bullet);
    }
}
```

### 字符串优化

```csharp
// ❌ 错误：字符串拼接产生垃圾
void Update()
{
    scoreText.text = "Score: " + score + " / " + maxScore;
}

// ✅ 正确：使用StringBuilder
private StringBuilder sb = new StringBuilder();

void UpdateScore()
{
    sb.Clear();
    sb.Append("Score: ").Append(score).Append(" / ").Append(maxScore);
    scoreText.text = sb.ToString();
}

// ✅ 更好：使用字符串缓存
private static readonly string[] scoreCache = new string[1000];

static Constructor()
{
    for (int i = 0; i < scoreCache.Length; i++)
    {
        scoreCache[i] = i.ToString();
    }
}

void UpdateScore()
{
    if (score < scoreCache.Length)
        scoreText.text = scoreCache[score];
    else
        scoreText.text = score.ToString();
}
```

## LINQ性能注意事项

### 避免在热路径中使用LINQ

```csharp
// ❌ 错误：Update中使用LINQ
void Update()
{
    var nearestEnemy = enemies
        .Where(e => e.IsActive)
        .OrderBy(e => Vector3.Distance(transform.position, e.position))
        .FirstOrDefault();
}

// ✅ 正确：手动循环
void Update()
{
    Enemy nearestEnemy = null;
    float nearestDistance = float.MaxValue;

    foreach (var enemy in enemies)
    {
        if (!enemy.IsActive) continue;

        float distance = Vector3.Distance(transform.position, enemy.position);
        if (distance < nearestDistance)
        {
            nearestDistance = distance;
            nearestEnemy = enemy;
        }
    }
}
```

### LINQ优化技巧

```csharp
// ✅ 使用ToArray而非ToList（当不需要修改时）
var items = collection.Where(x => x.IsValid).ToArray();

// ✅ 缓存LINQ结果
private Item[] validItems;

void RefreshCache()
{
    validItems = allItems.Where(x => x.IsValid).ToArray();
}

// ✅ 使用Any()而非Count() > 0
if (items.Any(x => x.IsActive))  // 好
if (items.Count(x => x.IsActive) > 0)  // 差

// ✅ 使用FirstOrDefault而非Where().First()
var item = items.FirstOrDefault(x => x.Id == targetId);  // 好
var item = items.Where(x => x.Id == targetId).First();  // 差
```

## 物理优化

### 物理设置优化

```csharp
// ✅ 使用层级碰撞矩阵减少不必要的碰撞检测
// Edit -> Project Settings -> Physics -> Layer Collision Matrix

// ✅ 合理设置Fixed Timestep
// Time.fixedDeltaTime = 0.02f; // 50Hz，对大多数游戏足够

// ✅ 使用简单碰撞体
// 优先级：Sphere > Capsule > Box > Mesh
```

### Raycast优化

```csharp
// ❌ 错误：Raycast所有层
RaycastHit hit;
if (Physics.Raycast(ray, out hit))
{
    // ...
}

// ✅ 正确：指定层级
int layerMask = LayerMask.GetMask("Enemy", "Obstacle");
if (Physics.Raycast(ray, out hit, maxDistance, layerMask))
{
    // ...
}

// ✅ 使用NonAlloc版本
private RaycastHit[] hits = new RaycastHit[10];

void CheckCollisions()
{
    int count = Physics.RaycastNonAlloc(ray, hits, maxDistance, layerMask);
    for (int i = 0; i < count; i++)
    {
        ProcessHit(hits[i]);
    }
}
```

## UI优化

### Canvas优化

```csharp
// ✅ 分离动态和静态UI
// 静态UI（背景、框架）放在一个Canvas
// 动态UI（分数、生命值）放在另一个Canvas

// ✅ 避免Canvas重建
// 使用CanvasGroup控制显示隐藏而非SetActive

public class UIPanel : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Show()
    {
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}
```

## 性能检查清单

### 日常开发检查
- [ ] Update中无内存分配
- [ ] 组件引用已缓存
- [ ] 使用对象池管理频繁创建的对象
- [ ] 字符串使用StringBuilder
- [ ] 避免运行时Find操作
- [ ] LINQ不在热路径中使用

### 优化前检查
- [ ] 使用Profiler定位瓶颈
- [ ] 记录优化前的性能数据
- [ ] 确定优化目标（FPS、内存、加载时间）

### 优化后验证
- [ ] 性能确实提升
- [ ] 没有引入新的问题
- [ ] 代码可读性仍然良好