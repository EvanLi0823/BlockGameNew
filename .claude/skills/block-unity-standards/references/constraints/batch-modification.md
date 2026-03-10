# 批量修改代码规范

**目的**：防止误改第三方库，确保修改范围可控

## 批量修改前必须遵守的规则

### 1. 严格限定修改范围
- 只能修改 `/Scripts` 目录下的文件
- 绝对禁止修改第三方库目录（如 Demigiant、Plugins、TextMeshPro 等）
- 使用命令：`find .../Scripts -name "*.cs"` 而非 `find .../Assets -name "*.cs"`

### 2. 使用排除模式保护第三方库
```bash
find ... -not -path "*/Demigiant/*" -not -path "*/Plugins/*" -not -path "*/ThirdParty/*"
```

### 3. 执行前预览
- 先使用 grep 查看将要修改的内容
- 确认没有第三方库文件后再执行替换

### 4. 精确匹配替换
- 优先使用具体类名匹配：`StorageManager.instance -> StorageManager.Instance`
- 避免使用过于宽泛的模式：`\.instance -> \.Instance`
- 只替换单例属性，不替换其他包含 instance 的变量名

### 5. 分步执行验证
- 步骤1：识别目标文件
- 步骤2：预览修改内容
- 步骤3：执行替换
- 步骤4：验证结果

## 示例工作流

```bash
# 步骤1：找到目标文件
find Assets/BlockPuzzleGameToolkit/Scripts -name "*.cs" -not -path "*/Plugins/*"

# 步骤2：预览修改内容
grep -r "StorageManager\.instance" Assets/BlockPuzzleGameToolkit/Scripts

# 步骤3：执行替换（使用sed或其他工具）
# ...

# 步骤4：验证结果
grep -r "StorageManager\.instance" Assets/BlockPuzzleGameToolkit/Scripts
# 应该返回0个结果
```

## ❌ 常见错误

**错误1：修改范围过大**
```bash
# ❌ 错误：会修改第三方库
find Assets -name "*.cs" | xargs sed -i 's/instance/Instance/g'
```

**错误2：模式过于宽泛**
```bash
# ❌ 错误：会修改所有instance，包括变量名
sed -i 's/\.instance/\.Instance/g'
```

## ✅ 正确做法

```bash
# ✅ 正确：精确匹配 + 限定范围
grep -r "StorageManager\.instance" Assets/BlockPuzzleGameToolkit/Scripts --include="*.cs"
# 然后逐个替换或使用精确的sed命令
```
