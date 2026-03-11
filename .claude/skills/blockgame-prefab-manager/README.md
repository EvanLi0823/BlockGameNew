# BlockGame Scene Reference Skill

快速查询BlockGame项目Unity场景和Prefab结构的技能。

## 功能特性

- 📋 **场景 & Prefab 文档** - 完整的GameObject层级结构
- 🔍 **快速查询** - 通过grep快速查找GameObject和组件
- 🔄 **自动生成** - Python脚本自动解析Unity场景/Prefab文件
- 📦 **组件信息** - 显示每个GameObject挂载的组件类型
- 📑 **Prefab索引** - 28个Prefab的分类索引和快速搜索

## 文档统计

- **场景文档**: 1 个（Scene_Main_Hierarchy.md - 83 GameObjects）
- **Prefab文档**: 28 个（233 GameObjects总计）
- **平均复杂度**: 8.3 GameObjects/Prefab

## 技能结构

```
blockgame-scene-reference/
├── SKILL.md                           # 技能主文档（88行）
├── README.md                          # 本文件
├── CHANGELOG.md                       # 版本历史
├── references/                        # 场景/Prefab结构参考文档
│   ├── Scene_Main_Hierarchy.md       # Main场景（83 GameObjects）
│   ├── Prefab_Index.md               # Prefab索引（28个）
│   ├── Prefab_*.md                   # 各个Prefab的详细文档
│   ├── usage-guide.md                # 使用指南
│   └── regeneration-guide.md         # 文档生成指南
└── scripts/                           # 工具脚本
    └── generate_scene_doc.py         # 场景/Prefab文档生成脚本
```

## 使用方法

### 激活技能

当你需要查询场景结构时，Claude Code会自动激活此技能，或者手动调用：

```
/skill blockgame-scene-reference
```

### 查询示例

在技能激活后，可以使用以下命令查询：

**场景查询**：
```bash
# 查找特定GameObject
grep "TopPanel" references/Scene_Main_Hierarchy.md

# 查找所有Canvas对象
grep -i "canvas" references/Scene_Main_Hierarchy.md

# 查找所有Manager
grep -i "manager" references/Scene_Main_Hierarchy.md
```

**Prefab查询**：
```bash
# 查看Prefab索引
cat references/Prefab_Index.md

# 查找所有按钮
grep -r "btn_" references/Prefab_*.md

# 查找特定Prefab的文本元素
grep "tmp_\|txt_" references/Prefab_Settings_Hierarchy.md

# 查找最复杂的Prefab（GameObject > 20）
grep "GameObject总数.*[23][0-9]" references/Prefab_*.md
```

### 更新文档

**更新场景文档**：
```bash
# 重新生成场景文档
python3 scripts/generate_scene_doc.py Assets/BlockPuzzleGameToolkit/Scenes/main.unity

# 复制到技能目录
cp Documents/Scene_Main_Hierarchy.md .claude/skills/blockgame-scene-reference/references/
```

**更新单个Prefab文档**：
```bash
# 重新生成特定Prefab文档
python3 scripts/generate_scene_doc.py Assets/BlockPuzzleGameToolkit/Resources/Popups/Settings.prefab

# 复制到技能目录
cp Documents/Prefab_Settings_Hierarchy.md .claude/skills/blockgame-scene-reference/references/
```

**批量更新所有Prefab文档**：
```bash
# 批量生成所有Prefab文档
for prefab in Assets/BlockPuzzleGameToolkit/Resources/Popups/*.prefab; do
    python3 scripts/generate_scene_doc.py "$prefab"
done

# 复制到技能目录
cp Documents/Prefab_*.md .claude/skills/blockgame-scene-reference/references/

# 重新生成索引
python3 /tmp/generate_prefab_index.py
```

## 文档内容

### 场景文档

场景文档包含：

1. **场景概览** - GameObject总数、根节点数量
2. **完整Hierarchy** - 树状层级结构
3. **组件信息** - 每个GameObject挂载的组件列表
4. **使用说明** - 常见查询方法

### Prefab文档

Prefab文档包含：

1. **Prefab概览** - GameObject总数、根节点数量
2. **完整Hierarchy** - UI元素的树状层级结构
3. **组件信息** - 按钮、文本、图片等UI元素的组件列表
4. **使用说明** - 修改对应脚本时的参考信息

**Prefab索引** (`Prefab_Index.md`) 提供：
- 28个Prefab的完整列表（按复杂度排序）
- 按类型分类（Win/Failed/Reward/Settings等）
- 快速搜索指南和批量操作命令

## 示例

### Main场景结构

Main场景主要根节点：

- `---GamePlay---` - 游戏玩法核心逻辑
- `--Loading--` - 加载进度UI
- `---Main-------` - 主菜单Canvas
- `---Map--------` - 地图和关卡选择系统
- `CanvasBack` - 背景Canvas
- 各种Manager（TutorialManager, OrientationManager等）

### Prefab结构示例

**Settings Prefab**（最复杂，34 GameObjects）：
- 设置面板的完整UI结构
- 包含多个toggle、button、slider等UI控件

**CommonRewardPopup Prefab**（17 GameObjects）：
- 通用奖励弹窗
- 包含按钮：btn_claim, btn_claimAd, btn_noclaim
- 包含文本：tmp_reward, txt_multiple
- 包含图片：img_guang, img_reward, img_white

**Win_Slider/Win_Fixed Prefab**（17 GameObjects）：
- 胜利弹窗的两种变体
- 包含奖励显示和继续按钮

## 生成脚本说明

`generate_scene_doc.py` 脚本功能：

- 解析Unity场景/Prefab文件（YAML格式）
- 自动识别文件类型（.unity 或 .prefab）
- 提取GameObject和Transform/RectTransform信息
- 构建层级树结构
- 识别挂载的组件类型
- 生成Markdown格式文档

**脚本用法**：

```bash
python3 scripts/generate_scene_doc.py <unity文件路径> [输出文件路径]

# 场景示例
python3 scripts/generate_scene_doc.py Assets/Scenes/Main.unity

# Prefab示例
python3 scripts/generate_scene_doc.py Assets/Resources/Popups/Settings.prefab

# 自定义输出路径
python3 scripts/generate_scene_doc.py Assets/Scenes/Main.unity custom_output.md
```

**支持的文件类型**：
- `.unity` - Unity场景文件
- `.prefab` - Unity Prefab文件

## 维护建议

1. **定期更新** - 场景/Prefab修改后及时更新文档
2. **添加新文档** - 为新场景/Prefab生成文档并添加到references/
3. **保持同步** - 确保文档与实际结构一致
4. **更新索引** - 批量更新Prefab后重新生成Prefab_Index.md
5. **版本控制** - 将文档纳入Git，跟踪结构变化

## 技术细节

- **支持的文件** - .unity场景文件, .prefab预制体文件
- **支持的组件** - Transform, RectTransform, MonoBehaviour等所有Unity组件
- **文件格式** - Unity 2021.3 LTS YAML格式
- **文档格式** - Markdown with code blocks
- **脚本语言** - Python 3.x
- **文档数量** - 1个场景 + 28个Prefab = 29个文档

## 常见使用场景

1. **修改UI脚本时** - 查询对应Prefab的UI元素结构
2. **添加新UI元素时** - 了解现有命名规范和层级组织
3. **重构代码时** - 确认GameObject引用和组件依赖
4. **代码审查时** - 快速了解场景/Prefab结构变化
5. **新人上手时** - 快速熟悉项目UI结构

---

**创建时间**: 2026-03-11
**版本**: 2.0.0
**作者**: BlockGame Team
**更新日志**: 参见 [CHANGELOG.md](CHANGELOG.md)
