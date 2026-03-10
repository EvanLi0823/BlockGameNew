# 代码审查指南

## 代码审查目的

确保代码质量、发现潜在问题、分享知识、保持代码一致性。

## 审查优先级

### 🚫 P0 - 阻塞级（必须修复）
会导致编译错误、运行时崩溃或数据丢失的问题。

### 🔴 P1 - 严重（必须修复）
影响功能正常工作或存在严重性能问题。

### 🟡 P2 - 重要（应该修复）
代码质量问题，影响可维护性。

### 🟢 P3 - 建议（可选）
代码风格、可读性改进建议。

## 审查检查清单

### 🚫 硬性约束检查

#### 单例模式检查
```csharp
// 🚫 P0: 必须使用Instance（大写）
// ❌ 错误
var manager = GameManager.instance;  // 编译错误

// ✅ 正确
var manager = GameManager.Instance;
```

#### API调用检查
```csharp
// 🚫 P0: 调用不存在的方法
// 审查时必须验证：
// 1. 方法确实存在
// 2. 访问权限正确（public/private）
// 3. 参数类型匹配
```

#### 括号匹配检查
```csharp
// 🚫 P0: 括号不匹配
// 审查重点：
// 1. 每个 { 有对应的 }
// 2. 缩进正确反映嵌套层级
// 3. 没有孤立的括号
```

### 🔴 功能正确性检查

#### 空引用检查
```csharp
// 🔴 P1: 可能的空引用异常
// ❌ 问题代码
public void ProcessData()
{
    DataManager.Instance.SaveData();  // Instance可能为null
}

// ✅ 修复建议
public void ProcessData()
{
    if (DataManager.Instance != null)
    {
        DataManager.Instance.SaveData();
    }
    else
    {
        Debug.LogError("DataManager not found!");
    }
}
```

#### 资源泄漏检查
```csharp
// 🔴 P1: 事件未取消订阅
// ❌ 问题代码
void Start()
{
    EventManager.Instance.OnEvent += HandleEvent;
}
// 缺少OnDestroy中的取消订阅

// ✅ 修复建议
void Start()
{
    if (EventManager.Instance != null)
        EventManager.Instance.OnEvent += HandleEvent;
}

void OnDestroy()
{
    if (EventManager.Instance != null)
        EventManager.Instance.OnEvent -= HandleEvent;
}
```

#### 异常处理检查
```csharp
// 🔴 P1: 缺少异常处理
// ❌ 问题代码
public void LoadFile(string path)
{
    var content = File.ReadAllText(path);  // 可能抛出异常
}

// ✅ 修复建议
public void LoadFile(string path)
{
    try
    {
        var content = File.ReadAllText(path);
        ProcessContent(content);
    }
    catch (FileNotFoundException e)
    {
        Debug.LogError($"File not found: {path}");
    }
    catch (Exception e)
    {
        Debug.LogError($"Failed to load file: {e.Message}");
    }
}
```

### 🟡 代码质量检查

#### 访问修饰符
```csharp
// 🟡 P2: 缺少访问修饰符
// ❌ 问题
int count;  // 应该明确指定private

// ✅ 改进
private int count;
```

#### 只读字段
```csharp
// 🟡 P2: 应该使用readonly
// ❌ 问题
private List<Item> items = new List<Item>();  // 从不重新赋值

// ✅ 改进
private readonly List<Item> items = new List<Item>();
```

#### 魔法数字
```csharp
// 🟡 P2: 使用魔法数字
// ❌ 问题
if (health < 20)  // 20是什么？

// ✅ 改进
private const int LOW_HEALTH_THRESHOLD = 20;
if (health < LOW_HEALTH_THRESHOLD)
```

### 🟢 性能检查

#### Update循环
```csharp
// 🟢 P3: Update中的性能问题
// ⚠️ 需要注意
void Update()
{
    // LINQ在Update中
    var nearest = enemies.OrderBy(e => e.distance).First();

    // 字符串拼接
    text.text = "Score: " + score;

    // 频繁的Find
    var player = GameObject.Find("Player");
}

// 💡 建议改进
private GameObject player;
private StringBuilder sb = new StringBuilder();

void Start()
{
    player = GameObject.Find("Player");
}

void Update()
{
    // 手动查找最近的
    Enemy nearest = FindNearestEnemy();

    // 使用StringBuilder
    sb.Clear();
    sb.Append("Score: ").Append(score);
    text.text = sb.ToString();
}
```

## 代码审查模板

### PR描述检查
```markdown
## 变更说明
- [ ] 清晰描述了改动内容
- [ ] 说明了改动原因
- [ ] 列出了相关Issue编号

## 测试说明
- [ ] 描述了如何测试
- [ ] 列出了测试用例
- [ ] 确认测试通过
```

### 代码检查评论模板

#### 🚫 阻塞级问题
```
🚫 [P0-阻塞] 单例访问错误

这里使用了 `GameManager.instance`（小写），应该改为 `GameManager.Instance`（大写）。

```diff
- var manager = GameManager.instance;
+ var manager = GameManager.Instance;
```

这会导致编译错误，必须修复。
```

#### 🔴 严重问题
```
🔴 [P1-严重] 可能的空引用异常

`ShopManager.Instance` 可能为null，需要添加空检查。

```diff
- ShopManager.Instance.OpenShop();
+ if (ShopManager.Instance != null)
+ {
+     ShopManager.Instance.OpenShop();
+ }
```
```

#### 🟡 重要建议
```
🟡 [P2-重要] 字段应该标记为readonly

`itemList` 在构造后从未重新赋值，建议添加readonly修饰符。

```diff
- private List<Item> itemList = new List<Item>();
+ private readonly List<Item> itemList = new List<Item>();
```

这样可以防止意外的重新赋值，提高代码安全性。
```

#### 🟢 改进建议
```
🟢 [P3-建议] 可以使用LINQ简化代码

这段代码可以使用LINQ简化：

```diff
- List<Enemy> activeEnemies = new List<Enemy>();
- foreach (var enemy in allEnemies)
- {
-     if (enemy.IsActive)
-         activeEnemies.Add(enemy);
- }
+ var activeEnemies = allEnemies.Where(e => e.IsActive).ToList();
```

更简洁易读。
```

## 审查工具和自动化

### 使用grep验证API调用
```bash
# 审查前验证方法是否存在
grep "public.*MethodName" TargetClass.cs

# 检查访问权限
grep "private.*MethodName" TargetClass.cs
```

### 批量检查单例使用
```bash
# 查找所有错误的单例访问
grep -r "\.instance" --include="*.cs" Scripts/

# 查找正确的单例访问
grep -r "\.Instance" --include="*.cs" Scripts/
```

## 审查最佳实践

### DO（应该做）:
- ✅ 先检查硬性约束（P0级）
- ✅ 使用模板格式化评论
- ✅ 提供具体的修改建议
- ✅ 解释为什么需要修改
- ✅ 表扬好的代码实践
- ✅ 保持专业和友善
- ✅ 关注代码的正确性和可维护性

### DON'T（不要做）:
- ❌ 人身攻击或贬低
- ❌ 只指出问题不给建议
- ❌ 过度纠结代码风格
- ❌ 忽略严重问题
- ❌ 批准有P0/P1问题的代码
- ❌ 审查时不运行代码

## 快速审查清单

### 必查项（P0/P1）
- [ ] **单例**: Instance使用正确（大写）
- [ ] **空检查**: Manager访问前判空
- [ ] **事件**: OnDestroy中取消订阅
- [ ] **异常**: 外部调用有try-catch
- [ ] **资源**: 正确释放（Dispose）
- [ ] **API**: 调用的方法确实存在

### 应查项（P2）
- [ ] **访问修饰符**: 明确指定
- [ ] **Readonly**: 不可变字段标记
- [ ] **Const**: 常量使用const
- [ ] **命名**: 符合规范
- [ ] **注释**: 复杂逻辑有说明
- [ ] **Warning**: 无编译器警告

### 建议项（P3）
- [ ] **LINQ**: 合理使用简化代码
- [ ] **性能**: Update无过度开销
- [ ] **缓存**: 组件引用已缓存
- [ ] **代码组织**: 使用region分组
- [ ] **可读性**: 代码清晰易懂

## 审查流程

1. **快速浏览**: 了解改动范围
2. **检查P0**: 查找阻塞级问题
3. **检查P1**: 查找严重问题
4. **检查P2**: 查找质量问题
5. **提供P3建议**: 改进建议
6. **运行测试**: 验证功能
7. **总结反馈**: 整体评价

## 审查决定

- **Request Changes**: 有P0或P1问题
- **Comment**: 有P2问题或P3建议
- **Approve**: 仅有P3建议或无问题