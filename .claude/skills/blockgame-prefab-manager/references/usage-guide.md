# BlockGame Scene & Prefab Reference - 使用指南

## 常见查询

### 查找特定GameObject

**跨所有文件搜索**：
```bash
grep -r "GameObject_Name" .claude/skills/blockgame-scene-reference/references/
```

### 查找UI元素

**在场景中查找Canvas对象**：
```bash
grep "Canvas" .claude/skills/blockgame-scene-reference/references/Scene_*.md
```

**在Prefab中查找按钮**：
```bash
grep "btn_" .claude/skills/blockgame-scene-reference/references/Prefab_*.md
```

**在Prefab中查找文本字段**：
```bash
grep "tmp_\|txt_" .claude/skills/blockgame-scene-reference/references/Prefab_*.md
```

### 查找Manager组件

**在Main场景中查找所有Manager**：
```bash
grep -i "manager" .claude/skills/blockgame-scene-reference/references/Scene_Main_Hierarchy.md
```

### 查找UI面板

**在Main场景中查找所有Panel**：
```bash
grep -i "panel" .claude/skills/blockgame-scene-reference/references/Scene_Main_Hierarchy.md
```

---

## 示例工作流

### 场景1：修改CommonRewardPopup.cs添加新按钮

当你需要修改 CommonRewardPopup.cs 脚本添加新功能时：

**步骤1：检查当前Prefab结构**
```bash
cat .claude/skills/blockgame-scene-reference/references/Prefab_CommonRewardPopup_Hierarchy.md
```

**步骤2：查找现有按钮模式**
```bash
grep "btn_" .claude/skills/blockgame-scene-reference/references/Prefab_CommonRewardPopup_Hierarchy.md
```

**输出示例**：
```
├─ **btn_claim** `[CanvasRenderer, MonoBehaviour, MonoBehaviour, MonoBehaviour]`
├─ **btn_claimAd** `[CanvasRenderer, MonoBehaviour, MonoBehaviour, MonoBehaviour]`
├─ **btn_noclaim** `[CanvasRenderer, MonoBehaviour, MonoBehaviour, MonoBehaviour]`
```

**步骤3：理解Hierarchy层级**
- 所有按钮都是 `Content` 的子节点
- 每个按钮都有 `Text (TMP)` 子节点用于显示文本
- 按钮包含 Button, CanvasRenderer, MonoBehaviour 组件

**步骤4：在Unity中更新Prefab**
- 在 `Content` 节点下添加新按钮
- 遵循现有命名规范（如 `btn_xxx`）
- 添加必要的组件和子节点

**步骤5：更新脚本引用**
```csharp
// 在 CommonRewardPopup.cs 中添加字段
[SerializeField] private Button btn_newButton;
[SerializeField] private TextMeshProUGUI tmp_newButtonText;
```

**步骤6：重新生成文档**
```bash
python3 .claude/skills/blockgame-scene-reference/scripts/generate_scene_doc.py \
    Assets/BlockPuzzleGameToolkit/Resources/Popups/CommonRewardPopup.prefab

# 复制到技能目录
cp Documents/Prefab_CommonRewardPopup_Hierarchy.md \
   .claude/skills/blockgame-scene-reference/references/
```

---

### 场景2：查找Main场景中的TopPanel

**步骤1：搜索GameObject**
```bash
grep "TopPanel" .claude/skills/blockgame-scene-reference/references/Scene_Main_Hierarchy.md
```

**输出示例**：
```
  ├─ **TopPanel** `[Canvas, MonoBehaviour]`
```

**步骤2：查看完整层级**
打开 Scene_Main_Hierarchy.md 找到 TopPanel 的完整层级树，了解其父节点和子节点

**步骤3：了解组件挂载**
从输出可知 TopPanel 挂载了：
- Canvas 组件
- MonoBehaviour（自定义脚本，可能是 TopPanel.cs）

---

### 场景3：为新Prefab生成文档

假设你创建了一个新的 Prefab `NewRewardPopup.prefab`：

**步骤1：生成文档**
```bash
python3 .claude/skills/blockgame-scene-reference/scripts/generate_scene_doc.py \
    Assets/BlockPuzzleGameToolkit/Resources/Popups/NewRewardPopup.prefab
```

**步骤2：复制到技能目录**
```bash
cp Documents/Prefab_NewRewardPopup_Hierarchy.md \
   .claude/skills/blockgame-scene-reference/references/
```

**步骤3：更新SKILL.md**
在 SKILL.md 的 "Prefabs" 部分添加新Prefab的说明和快速搜索示例

---

## 快速参考：常用grep模式

### 搜索模式速查

| 目标 | 命令 |
|------|------|
| 查找所有按钮 | `grep -i "btn_" references/` |
| 查找所有文本 | `grep -i "tmp_\|txt_" references/` |
| 查找所有图片 | `grep -i "img_" references/` |
| 查找所有面板 | `grep -i "panel" references/` |
| 查找所有Canvas | `grep -i "canvas" references/` |
| 查找所有Manager | `grep -i "manager" references/` |

### 组合搜索示例

**查找包含Button组件的GameObject**：
```bash
grep "MonoBehaviour, MonoBehaviour" references/Prefab_*.md
```

**查找特定层级的GameObject**（如Content的直接子节点）：
```bash
grep "  ├─" references/Prefab_CommonRewardPopup_Hierarchy.md
```

**统计GameObject数量**：
```bash
grep -c "├─\|●" references/Scene_Main_Hierarchy.md
```

---

## 提示与技巧

### Tip 1: 使用 -A 和 -B 查看上下文

查看GameObject的父节点和子节点：
```bash
grep -B 2 -A 5 "btn_claim" references/Prefab_CommonRewardPopup_Hierarchy.md
```

### Tip 2: 使用 head 限制输出

只查看前10个匹配项：
```bash
grep "├─" references/Scene_Main_Hierarchy.md | head -10
```

### Tip 3: 组合多个条件

查找同时包含Button和TMP的节点：
```bash
grep "btn_" references/*.md | grep "TMP"
```

### Tip 4: 导出搜索结果

将搜索结果保存到文件：
```bash
grep -r "Manager" references/ > manager_list.txt
```

---

**参考**：查看各个文档文件获取完整的Hierarchy树状结构
