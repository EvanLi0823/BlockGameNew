---
name: blockgame-prefab-builder
description: Automatically generate Unity Prefabs from Prefab structure documents. Parses naming conventions (txt_, img_, btn_, etc.) to create GameObjects with appropriate components. Supports automated generation via Unity command line or manual execution in Unity Editor.
version: 1.0.0
author: BlockGame Team
---

# BlockGame Prefab Builder

从 Prefab 结构文档自动生成 Unity Prefab，实现文档驱动的 UI 创建。

## Purpose

将 `blockgame-scene-reference` 生成的文档逆向转换为实际的 Unity Prefab：
- 解析 GameObject 层级结构
- 根据命名规则自动推断组件类型
- 生成 C# 创建脚本
- 自动化执行创建流程

## When to Use

激活此技能当：
- 需要从文档快速创建 Prefab 原型
- 批量创建多个相似结构的 UI
- 确保 UI 命名规范的一致性
- 重建丢失的 Prefab（有文档备份时）

## Naming Rules → Component Mapping

**常用前缀** (优先级: Text > TextMeshPro)：
- `txt_` → Text (UnityEngine.UI)
- `tmp_` → TextMeshProUGUI (TMPro)
- `img_` → Image
- `btn_` → Button + Image
- `panel_` → CanvasGroup + Image
- `scroll_` → ScrollRect
- `toggle_` → Toggle
- `slider_` → Slider
- `input_` → TMP_InputField

**特殊节点**: `Content` → RectTransform, `Viewport` → RectMask2D

**详细映射**: [component-mapping.md](references/component-mapping.md)

## Quick Start

### 方法 1: 自动化创建（推荐）

```bash
# 一键生成并创建 Prefab
./scripts/auto_create_prefab.sh \
    ../blockgame-scene-reference/references/Prefab_CommonRewardPopup_Hierarchy.md
```

**输出**：
- C# 脚本: `Assets/Editor/Generated/CreateCommonRewardPopup.cs`
- 弹窗Prefab生出路径: `Assets/BlockPuzzleGameToolkit/Resources/Popups/`

### 方法 2: 手动执行

**步骤 1** - 生成 C# 脚本：
```bash
python3 scripts/generate_prefab.py \
    ../blockgame-scene-reference/references/Prefab_Settings_Hierarchy.md
```

**步骤 2** - 在 Unity Editor 中执行：
- 菜单: `Tools → Create Prefab → Settings`
- Prefab 将创建到 `Assets/Generated/Prefabs/`

## Example

**输入**: `Prefab_CommonRewardPopup_Hierarchy.md` (17 GameObjects)

**生成**:
- 完整层级结构
- 自动推断组件（Image, TextMeshProUGUI, Button）
- 保持命名规范

## Limitations

当前版本限制：
- ⚠️ 不设置组件参数（如 Image 的 sprite，Text 的内容）
- ⚠️ 不包含布局信息（RectTransform 的位置、大小）
- ⚠️ 不自动连接脚本的序列化字段引用

**需要手动**：
- 配置 UI 元素的资源和参数
- 调整布局和锚点
- 连接脚本引用（如 CommonRewardPopup.cs 中的字段）

## Detailed Guides

- **组件映射规则**: [component-mapping.md](references/component-mapping.md)
- **示例和最佳实践**: [examples/](examples/)

---

**Note**: 此技能生成基础 Prefab 结构，完整配置需在 Unity Editor 中手动完成。
