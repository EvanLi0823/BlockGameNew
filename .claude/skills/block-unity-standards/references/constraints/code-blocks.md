# 代码块修改规范

**目的**：防止代码修改时留下孤立括号，确保代码块完整性

## 完整代码块替换原则

### ❌ 错误做法

**只删除条件，留下括号**
```csharp
// 原始代码
if (count > 0)
{
    OnPropCountChanged?.Invoke(type, count);
}

// ❌ 错误：只替换了条件
old_string: if (count > 0)
new_string: // 删除了条件

// 结果：留下孤立的括号
{
    OnPropCountChanged?.Invoke(type, count);
}
// 编译错误：CS1022 类型或命名空间定义，或文件尾所需
```

### ✅ 正确做法

**替换整个代码块**
```csharp
// ✅ 正确：替换整个代码块
old_string: |
    if (count > 0)
    {
        OnPropCountChanged?.Invoke(type, count);
    }

new_string: |
    OnPropCountChanged?.Invoke(type, count);

// 结果：代码结构完整
OnPropCountChanged?.Invoke(type, count);
```

## 修改规范

### 1. 删除条件语句
- 必须删除整个if/for/while块，包括括号
- 保留块内代码时需调整缩进

### 2. 修改循环语句
- 替换整个循环，包括条件和循环体
- 不要只修改循环条件

### 3. 修改try-catch块
- 替换整个try-catch-finally块
- 不要留下孤立的catch或finally

## ❌ 更多常见错误

**错误1：只删除循环条件**
```csharp
// ❌ 错误
old_string: for (int i = 0; i < count; i++)
new_string: // 删除

// 结果：留下孤立的循环体括号
{
    ProcessItem(i);
}
```

**错误2：只删除try**
```csharp
// ❌ 错误
old_string: try
new_string: // 删除

// 结果：留下孤立的catch
{
    DoSomething();
}
catch (Exception e)
{
    HandleError(e);
}
```

## ✅ 正确的修改方式

**删除整个控制流块**
```csharp
// ✅ 正确方式1：删除整个if块
old_string: |
    if (count > 0)
    {
        OnPropCountChanged?.Invoke(type, count);
    }
new_string: |
    OnPropCountChanged?.Invoke(type, count);
```

```csharp
// ✅ 正确方式2：删除整个for循环
old_string: |
    for (int i = 0; i < count; i++)
    {
        ProcessItem(i);
    }
new_string: |
    // 循环已移除
```

```csharp
// ✅ 正确方式3：替换整个try-catch
old_string: |
    try
    {
        DoSomething();
    }
    catch (Exception e)
    {
        HandleError(e);
    }
new_string: |
    DoSomething();
```

## 修改前检查清单

- [ ] 确认要修改的代码块边界（开始和结束）
- [ ] 包含所有相关的括号
- [ ] 确认缩进正确
- [ ] 验证修改后的代码块完整

## 验证方法

```bash
# 修改后立即验证括号匹配
# 使用IDE或编辑器的括号匹配功能
# 或使用简单的bash命令
grep -c '{' file.cs
grep -c '}' file.cs
# 两个数字应该相等
```
