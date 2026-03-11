# Blockgame Prefab Builder - 快速开始

## 🚀 5分钟上手

### 步骤 1: 准备文档

使用现有的 Prefab 文档（由 `blockgame-scene-reference` 生成）：

```bash
ls -la ../blockgame-scene-reference/references/Prefab_*.md
```

**或者**手写简单文档（参考 `examples/SimpleRewardPopup.md`）

---

### 步骤 2: 一键生成

**自动化方式**（推荐）：

```bash
cd .claude/skills/blockgame-prefab-builder

./scripts/auto_create_prefab.sh \
    ../blockgame-scene-reference/references/Prefab_CommonRewardPopup_Hierarchy.md
```

**手动方式**：

```bash
# 生成 C# 脚本
python3 scripts/generate_prefab.py \
    ../blockgame-scene-reference/references/Prefab_CommonRewardPopup_Hierarchy.md

# 在 Unity Editor 中执行
# 菜单: Tools → Create Prefab → CommonRewardPopup
```

---

### 步骤 3: 查看结果

```bash
# 生成的 C# 脚本
cat Assets/Editor/Generated/CreateCommonRewardPopup.cs

# 在 Unity 中查看 Prefab
# Assets/Generated/Prefabs/CommonRewardPopup.prefab
```

---

## 📝 示例：创建简单弹窗

### 1. 创建文档 `my_popup.md`

```markdown
● **MyPopup** `[CanvasGroup]`
  ├─ **Content**
    ├─ **img_background** `[Image]`
    ├─ **tmp_title** `[TextMeshProUGUI]`
    ├─ **btn_close** `[Button, Image]`
      ├─ **Text (TMP)** `[TextMeshProUGUI]`
```

### 2. 生成 Prefab

```bash
python3 scripts/generate_prefab.py my_popup.md
```

### 3. Unity 中执行

菜单: `Tools → Create Prefab → MyPopup`

---

## 🎯 命名规则速记

**优先级**: Text > TextMeshPro

| 前缀 | 组件 |
|------|------|
| `txt_` | Text (传统 UI) |
| `tmp_` | TextMeshProUGUI (TMPro) |
| `img_` | Image |
| `btn_` | Button + Image |
| `panel_` | CanvasGroup + Image |
| `Content` | RectTransform（容器）|

**完整规则**: 参见 `references/component-mapping.md`

---

## 💡 常见用法

### 用法 1: 批量创建多个 Prefab

```bash
for doc in ../blockgame-scene-reference/references/Prefab_*.md; do
    ./scripts/auto_create_prefab.sh "$doc"
done
```

### 用法 2: 从现有 Prefab 重建

```bash
# 1. 生成文档（如果还没有）
cd ../blockgame-scene-reference
python3 scripts/generate_scene_doc.py Assets/Resources/Popups/OldPrefab.prefab

# 2. 从文档重建
cd ../blockgame-prefab-builder
./scripts/auto_create_prefab.sh ../blockgame-scene-reference/references/Prefab_OldPrefab_Hierarchy.md
```

### 用法 3: 快速原型

1. 手写简单文档（5-10个节点）
2. 运行生成脚本
3. Unity 中调整细节

---

## ⚠️ 注意事项

### 生成后需要手动配置

1. **设置图片** - Image 的 sprite
2. **设置文本** - Text 的内容、大小、颜色
3. **调整布局** - RectTransform 的位置、大小
4. **连接引用** - 脚本的序列化字段

### 命名规范

- ✅ 使用小写字母和下划线
- ✅ 前缀符合规则（`btn_`, `img_`, `tmp_`）
- ❌ 避免特殊字符（自动清理）

---

## 🔧 故障排除

### 问题: Python 脚本失败

**检查 Python 版本**:
```bash
python3 --version  # 需要 3.6+
```

### 问题: Unity 未找到

**手动执行方法 2**（在 Unity Editor 中点击菜单）

### 问题: 生成的 Prefab 缺少组件

**检查命名是否符合规则**，或手动在 Unity 中添加

---

## 📚 更多资源

- **详细文档**: `README.md`
- **组件映射**: `references/component-mapping.md`
- **示例**: `examples/SimpleRewardPopup.md`
- **技能文档**: `SKILL.md`

---

**开始使用吧！** 🎉
