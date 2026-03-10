# 边界情况清单

## 1. 数据边界

### 1.1 Null值
**场景**:
- 对象引用为null
- 字符串为null
- 数组/列表为null

**检查**:
- [ ] 所有引用类型是否检查null?
- [ ] 是否使用?.运算符?
- [ ] 是否提供默认值?

**示例**:
```csharp
// ❌ 危险
var result = manager.GetData().value;

// ✅ 安全
var data = manager?.GetData();
var result = data?.value ?? defaultValue;
```

### 1.2 空集合
**场景**:
- List.Count == 0
- Array.Length == 0
- Dictionary为空

**检查**:
- [ ] 遍历前是否检查Count?
- [ ] [0]访问前是否检查Length?
- [ ] 空集合时UI如何展示?

**示例**:
```csharp
// ❌ 危险
var first = list[0];

// ✅ 安全
if (list != null && list.Count > 0)
{
    var first = list[0];
}
```

### 1.3 极端数值
**场景**:
- int.MaxValue / int.MinValue
- float.PositiveInfinity / NaN
- 0 / 负数

**检查**:
- [ ] 数值运算是否可能溢出?
- [ ] 除法是否检查除数为0?
- [ ] 负数是否有意义?

**示例**:
```csharp
// ❌ 危险
int result = a + b; // 可能溢出

// ✅ 安全
if (b > 0 && a > int.MaxValue - b)
    return int.MaxValue;
```

### 1.4 字符串边界
**场景**:
- 空字符串("")
- 空白字符串("   ")
- 特殊字符

**检查**:
- [ ] 是否使用string.IsNullOrEmpty?
- [ ] 是否使用string.IsNullOrWhiteSpace?
- [ ] 是否处理特殊字符(换行、引号等)?

## 2. 时序边界

### 2.1 组件未初始化
**场景**:
- Awake中访问其他组件
- Start前调用方法
- 依赖组件未加载

**检查**:
- [ ] 是否在Awake/Start正确初始化?
- [ ] 是否检查依赖组件是否存在?
- [ ] 是否使用Script Execution Order?

**示例**:
```csharp
// ❌ 危险
void Awake() {
    OtherManager.Instance.DoSomething();
}

// ✅ 安全
void Start() {
    if (OtherManager.Instance != null)
        OtherManager.Instance.DoSomething();
}
```

### 2.2 场景切换
**场景**:
- 场景加载过程中
- DontDestroyOnLoad对象
- 场景卸载时的清理

**检查**:
- [ ] 场景切换时是否清理事件订阅?
- [ ] DontDestroyOnLoad对象是否正确管理?
- [ ] 场景卸载时是否停止协程?

### 2.3 异步操作
**场景**:
- 协程执行中对象被销毁
- 异步回调时对象已销毁
- 多个异步操作竞争

**检查**:
- [ ] OnDestroy中是否停止协程?
- [ ] 异步回调中是否检查对象有效性?
- [ ] 是否有并发控制?

**示例**:
```csharp
// ❌ 危险
IEnumerator Load() {
    yield return www.SendWebRequest();
    UpdateUI(www.downloadHandler.text); // 可能已销毁
}

// ✅ 安全
IEnumerator Load() {
    yield return www.SendWebRequest();
    if (this != null && !string.IsNullOrEmpty(www.downloadHandler.text))
        UpdateUI(www.downloadHandler.text);
}
```

### 2.4 快速重复操作
**场景**:
- 按钮连续点击
- 事件快速触发
- Update中频繁调用

**检查**:
- [ ] 是否需要防抖(Debounce)?
- [ ] 是否需要节流(Throttle)?
- [ ] 是否需要冷却时间?

## 3. 状态边界

### 3.1 对象已销毁
**场景**:
- GameObject被Destroy
- 组件被移除
- 场景卸载

**检查**:
- [ ] 缓存引用前是否检查有效性?
- [ ] 事件回调中是否检查this != null?
- [ ] 是否及时清理引用?

### 3.2 GameObject禁用
**场景**:
- gameObject.SetActive(false)
- 父对象禁用
- 组件enabled = false

**检查**:
- [ ] 禁用状态下逻辑是否正确?
- [ ] OnEnable/OnDisable是否配对?
- [ ] 是否影响数据状态?

### 3.3 暂停/恢复
**场景**:
- Time.timeScale = 0
- 应用进入后台
- 焦点丢失

**检查**:
- [ ] 暂停时是否停止逻辑更新?
- [ ] 恢复时是否重新初始化?
- [ ] 是否使用Time.deltaTime vs Time.unscaledDeltaTime?

## 4. 平台边界

### 4.1 分辨率/屏幕比例
**场景**:
- 不同设备分辨率
- 横竖屏切换
- 刘海屏/安全区域

**检查**:
- [ ] UI是否适配不同分辨率?
- [ ] Canvas Scaler设置是否合理?
- [ ] 是否处理安全区域?

### 4.2 设备性能
**场景**:
- 低端设备
- 内存不足
- 帧率波动

**检查**:
- [ ] 是否有性能降级策略?
- [ ] 是否有内存警告处理?
- [ ] 是否根据设备调整质量?

### 4.3 Unity版本
**场景**:
- API变化
- 行为变化
- 弃用API

**检查**:
- [ ] 是否使用了版本特定API?
- [ ] 是否标记了最低支持版本?
- [ ] 是否处理了API兼容性?

## 5. 输入边界

### 5.1 用户输入
**场景**:
- 非法输入
- 超长输入
- 特殊字符

**检查**:
- [ ] 是否验证输入格式?
- [ ] 是否限制输入长度?
- [ ] 是否过滤特殊字符?

### 5.2 触摸/点击
**场景**:
- 多点触摸
- 快速连续点击
- 点击在UI外

**检查**:
- [ ] 是否处理多点触摸?
- [ ] 是否防止连续点击?
- [ ] 是否检测UI遮挡?

## 快速检查模板

对于每个功能,问自己:

**数据边界**:
- null时怎么办?
- 空集合怎么办?
- 极值怎么办?

**时序边界**:
- 未初始化时怎么办?
- 场景切换时怎么办?
- 异步执行中对象销毁怎么办?

**状态边界**:
- 对象已销毁怎么办?
- GameObject禁用怎么办?
- 游戏暂停怎么办?

**平台边界**:
- 不同分辨率怎么办?
- 低性能设备怎么办?
- 不同Unity版本怎么办?
