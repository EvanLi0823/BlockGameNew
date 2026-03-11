# 文档重新生成指南

## 概述

当场景或Prefab结构发生变化时，需要重新生成文档以保持同步。

---

## 重新生成场景文档

### Main场景

**命令**：
```bash
python3 .claude/skills/blockgame-scene-reference/scripts/generate_scene_doc.py \
    Assets/BlockPuzzleGameToolkit/Scenes/main.unity
```

**输出位置**：
- 自动保存到: `Documents/Scene_Main_Hierarchy.md`

**更新技能引用**：
```bash
cp Documents/Scene_Main_Hierarchy.md \
   .claude/skills/blockgame-scene-reference/references/
```

---

## 重新生成Prefab文档

### CommonRewardPopup Prefab

**命令**：
```bash
python3 .claude/skills/blockgame-scene-reference/scripts/generate_scene_doc.py \
    Assets/BlockPuzzleGameToolkit/Resources/Popups/CommonRewardPopup.prefab
```

**输出位置**：
- 自动保存到: `Documents/Prefab_CommonRewardPopup_Hierarchy.md`

**更新技能引用**：
```bash
cp Documents/Prefab_CommonRewardPopup_Hierarchy.md \
   .claude/skills/blockgame-scene-reference/references/
```

---

## 为新场景/Prefab生成文档

### 步骤1：运行生成脚本

**语法**：
```bash
python3 .claude/skills/blockgame-scene-reference/scripts/generate_scene_doc.py <unity_file_path> [output_file]
```

**示例1：场景文件**
```bash
python3 scripts/generate_scene_doc.py Assets/Scenes/Game.unity
```

**示例2：Prefab文件**
```bash
python3 scripts/generate_scene_doc.py Assets/Prefabs/MyPopup.prefab
```

**示例3：自定义输出路径**
```bash
python3 scripts/generate_scene_doc.py Assets/Scenes/Game.unity custom_output.md
```

### 步骤2：验证生成结果

脚本会输出统计信息：
```
✅ 场景文档已生成: Documents/Scene_Game_Hierarchy.md
📊 统计信息:
   - GameObject总数: 156
   - 根节点数量: 8
   - 文档大小: 12500 字符
```

### 步骤3：复制到技能目录（可选）

如果需要将文档纳入技能系统：
```bash
cp Documents/Scene_Game_Hierarchy.md \
   .claude/skills/blockgame-scene-reference/references/
```

### 步骤4：更新SKILL.md（可选）

在 SKILL.md 的相应部分添加新文档的引用和快速搜索示例。

---

## 脚本参数说明

### 必需参数

- `<unity_file_path>`: Unity场景文件(.unity)或Prefab文件(.prefab)的路径

### 可选参数

- `[output_file]`: 自定义输出文件路径
  - 如果未指定，场景文件默认输出到 `Documents/Scene_<name>_Hierarchy.md`
  - 如果未指定，Prefab文件默认输出到 `Documents/Prefab_<name>_Hierarchy.md`

---

## 支持的文件类型

### .unity 文件（场景）

**识别方式**：
- 文件扩展名为 `.unity`
- 位于 `Assets/` 目录下的场景文件

**生成内容**：
- 场景概览
- GameObject总数和根节点数量
- 完整Hierarchy层级结构
- 组件类型列表

### .prefab 文件（预制体）

**识别方式**：
- 文件扩展名为 `.prefab`
- 位于 `Assets/` 目录下的Prefab文件

**生成内容**：
- Prefab概览
- GameObject总数和根节点数量
- 完整Hierarchy层级结构
- 组件类型列表
- UI元素说明（如果是UI Prefab）

---

## 常见问题

### Q: 脚本运行失败怎么办？

**检查清单**：
1. 确认文件路径正确
2. 确认文件扩展名是 `.unity` 或 `.prefab`
3. 确认文件存在且可读
4. 确认 Python 3 已安装

**错误示例**：
```bash
❌ 错误: 文件不存在: Assets/Scenes/NotExist.unity
```

**解决方法**：
- 使用正确的文件路径
- 使用相对路径或绝对路径

### Q: 文档内容不完整怎么办？

**可能原因**：
- 场景/Prefab文件格式问题
- Unity版本不兼容
- 文件损坏

**解决方法**：
- 在Unity中重新保存场景/Prefab
- 确认Unity版本为 2021.3 LTS 或更高
- 检查Unity Console是否有错误

### Q: 如何更新多个场景/Prefab？

**批量生成脚本示例**：
```bash
#!/bin/bash
# bulk_generate.sh

# 生成所有场景文档
for scene in Assets/Scenes/*.unity; do
    python3 scripts/generate_scene_doc.py "$scene"
done

# 生成所有Popup Prefab文档
for prefab in Assets/BlockPuzzleGameToolkit/Resources/Popups/*.prefab; do
    python3 scripts/generate_scene_doc.py "$prefab"
done

# 复制到技能目录
cp Documents/Scene_*.md .claude/skills/blockgame-scene-reference/references/
cp Documents/Prefab_*.md .claude/skills/blockgame-scene-reference/references/
```

---

## 维护建议

### 何时需要重新生成文档？

**必须重新生成**：
- ✅ 添加/删除了GameObject
- ✅ 修改了GameObject名称
- ✅ 修改了Hierarchy层级结构
- ✅ 添加/删除了组件

**可选重新生成**：
- 🟡 修改了组件参数（不影响文档）
- 🟡 修改了GameObject位置（不影响文档）

### 建议的更新频率

**场景文档**：
- 大的结构调整后立即更新
- 每周定期检查一次

**Prefab文档**：
- 修改Prefab后立即更新
- 发布版本前检查所有Prefab文档

---

## Git版本管理

### 建议的Git工作流

**步骤1：修改场景/Prefab**
```bash
# 在Unity中修改场景或Prefab
```

**步骤2：重新生成文档**
```bash
python3 scripts/generate_scene_doc.py <modified_file>
cp Documents/<generated_doc> .claude/skills/blockgame-scene-reference/references/
```

**步骤3：提交更改**
```bash
git add Assets/<modified_file>
git add .claude/skills/blockgame-scene-reference/references/<generated_doc>
git commit -m "Update scene/prefab and regenerate documentation"
```

### .gitignore 建议

如果不想提交中间生成的文档：
```
# .gitignore
Documents/Scene_*.md
Documents/Prefab_*.md
```

但建议保留技能目录中的文档：
```
# 保留
.claude/skills/blockgame-scene-reference/references/Scene_*.md
.claude/skills/blockgame-scene-reference/references/Prefab_*.md
```

---

**参考**：查看 `scripts/generate_scene_doc.py` 获取脚本源代码和实现细节
