# BlockGame Prefab Builder Skill

从 Prefab 结构文档自动生成 Unity Prefab，实现文档驱动的 UI 创建。

## 功能特性

- 📖 **文档解析** - 解析 Prefab Hierarchy 文档
- 🔍 **智能识别** - 根据命名规则自动推断组件类型
- 🔧 **代码生成** - 生成 Unity C# 创建脚本
- ⚡ **自动化执行** - 通过 Unity 命令行自动创建 Prefab

## 技能结构

```
blockgame-prefab-builder/
├── SKILL.md                           # 技能主文档（98行）
├── README.md                          # 本文件
├── scripts/                           # 核心脚本
│   ├── component_rules.py            # 组件映射规则
│   ├── parse_prefab_doc.py           # 文档解析器
│   ├── generate_prefab.py            # Prefab 生成器
│   └── auto_create_prefab.sh         # 自动化执行脚本
├── references/                        # 参考文档
│   └── component-mapping.md          # 详细组件映射规则
├── templates/                         # C# 脚本模板（未来）
└── examples/                          # 示例和最佳实践
```

---

## 使用方法

### 快速开始

#### 方法 1: 一键自动化创建（推荐）⭐

```bash
cd .claude/skills/blockgame-prefab-builder

./scripts/auto_create_prefab.sh \
    ../blockgame-scene-reference/references/Prefab_CommonRewardPopup_Hierarchy.md
```

**流程**：
1. ✅ 解析文档
2. ✅ 生成 C# 脚本
3. ✅ Unity 命令行自动执行
4. ✅ Prefab 创建完成

**输出**：
- `Assets/Editor/Generated/CreateCommonRewardPopup.cs` - C# 脚本
- `Assets/Generated/Prefabs/CommonRewardPopup.prefab` - 生成的 Prefab

---

#### 方法 2: 手动执行

**步骤 1** - 生成 C# 脚本：

```bash
python3 scripts/generate_prefab.py \
    ../blockgame-scene-reference/references/Prefab_Settings_Hierarchy.md
```

输出：`Assets/Editor/Generated/CreateSettings.cs`

**步骤 2** - 在 Unity Editor 中执行：

1. 打开 Unity Editor
2. 菜单: `Tools → Create Prefab → Settings`
3. Prefab 生成到 `Assets/Generated/Prefabs/Settings.prefab`

---

### 批量创建多个 Prefab

```bash
# 批量创建所有 Prefab
for doc in ../blockgame-scene-reference/references/Prefab_*.md; do
    ./scripts/auto_create_prefab.sh "$doc"
done
```

---

## 命名规则详解

### UI 元素命名规则

**优先级**: Text > TextMeshPro

| 前缀 | 组件类型 | 命名空间 | 示例 | 说明 |
|------|---------|---------|------|------|
| `txt_` | Text | UnityEngine.UI | txt_reward | 传统 UI 文本 |
| `tmp_` | TextMeshProUGUI | TMPro | tmp_reward | TextMeshPro 文本 |
| `img_` | Image | UnityEngine.UI | img_reward | 图片/精灵 |
| `btn_` | Button + Image | UnityEngine.UI | btn_claim | 按钮（含背景图） |
| `panel_` | CanvasGroup + Image | UnityEngine.UI | panel_main | 面板容器 |
| `scroll_` | ScrollRect + Image | UnityEngine.UI | scroll_list | 滚动视图 |
| `toggle_` | Toggle + Image | UnityEngine.UI | toggle_sound | 开关 |
| `slider_` | Slider | UnityEngine.UI | slider_volume | 滑动条 |
| `input_` | TMP_InputField | TMPro | input_name | TMP输入框 |

### 特殊节点

| 节点名称 | 组件类型 | 说明 |
|---------|---------|------|
| `Content` | RectTransform | 内容容器（ScrollView 等） |
| `Viewport` | RectMask2D | 滚动视图的视口 |
| `Scrollbar Horizontal` | Scrollbar | 水平滚动条 |
| `Scrollbar Vertical` | Scrollbar | 垂直滚动条 |

### 按钮的特殊处理

- `btn_` 开头的节点会自动添加 `Image` 和 `Button` 组件
- 如果文档中显示有子节点 `Text (TMP)`，会自动创建

---

## 工作流程

### 完整工作流

```
1. 有 Prefab 文档
   ↓
2. 运行生成脚本
   ↓
3. C# 脚本生成
   ↓
4. Unity 自动执行
   ↓
5. Prefab 创建完成
   ↓
6. 手动配置参数（sprite, text, 布局等）
   ↓
7. 连接脚本引用
   ↓
8. 完成
```

### 与 blockgame-scene-reference 的协作

**场景 1: 重建 Prefab**

```bash
# 1. 从现有 Prefab 生成文档（如果还没有）
cd .claude/skills/blockgame-scene-reference
python3 scripts/generate_scene_doc.py \
    Assets/Resources/Popups/OldPrefab.prefab

# 2. 从文档重建 Prefab
cd ../blockgame-prefab-builder
./scripts/auto_create_prefab.sh \
    ../blockgame-scene-reference/references/Prefab_OldPrefab_Hierarchy.md
```

**场景 2: 快速原型**

1. 手写简单的文档（参考示例）
2. 运行生成脚本
3. 在 Unity 中调整细节

---

## 示例

### 示例 1: 简单奖励弹窗

**文档内容** (`example_reward_popup.md`):

```markdown
● **SimpleRewardPopup** `[CanvasGroup]`
  ├─ **Content**
    ├─ **img_background** `[Image]`
    ├─ **tmp_title** `[TextMeshProUGUI]`
    ├─ **img_reward** `[Image]`
    ├─ **tmp_amount** `[TextMeshProUGUI]`
    ├─ **btn_claim** `[Button, Image]`
      ├─ **Text (TMP)** `[TextMeshProUGUI]`
```

**生成命令**:

```bash
python3 scripts/generate_prefab.py examples/example_reward_popup.md
```

**结果**: 包含 7 个 GameObject 的 Prefab

---

### 示例 2: 设置面板

**文档内容**:

```markdown
● **SettingsPanel** `[CanvasGroup]`
  ├─ **Content**
    ├─ **panel_header** `[CanvasGroup, Image]`
      ├─ **tmp_title** `[TextMeshProUGUI]`
      ├─ **btn_close** `[Button, Image]`
    ├─ **scroll_settings** `[ScrollRect, Image]`
      ├─ **Viewport** `[RectMask2D]`
        ├─ **Content**
          ├─ **toggle_sound** `[Toggle, Image]`
          ├─ **toggle_music** `[Toggle, Image]`
          ├─ **slider_volume** `[Slider]`
```

**结果**: 包含完整层级的设置面板

---

## 限制和注意事项

### 当前限制

#### 1. 组件参数不设置

**不包含**：
- Image 的 sprite
- TextMeshProUGUI 的 text, fontSize, color
- Button 的 onClick 事件
- RectTransform 的 position, size, anchor

**需要手动**：
- 在 Unity Editor 中配置所有参数

#### 2. 布局信息缺失

**不包含**：
- GameObject 的位置、大小
- 锚点和轴心点
- 相对布局关系

**需要手动**：
- 调整 RectTransform
- 设置锚点
- 配置布局组件（如 VerticalLayoutGroup）

#### 3. 脚本引用不自动连接

**不包含**：
- 脚本的序列化字段引用
- 例如 `CommonRewardPopup.cs` 中的 `btn_claim` 引用

**需要手动**：
- 拖拽连接字段引用
- 或使用代码查找（`transform.Find()`）

---

### 使用建议

#### ✅ 适合的场景

1. **快速原型** - 创建 UI 基础结构
2. **批量创建** - 多个相似的 UI 面板
3. **命名规范** - 强制团队遵循命名约定
4. **文档驱动** - 从设计文档生成实现

#### ⚠️ 不适合的场景

1. **复杂布局** - 需要精确定位的 UI
2. **完整配置** - 需要所有参数都设置好的 Prefab
3. **一次性工作** - 简单 UI，手动创建更快

#### 💡 最佳实践

1. **先生成结构** - 使用此技能创建骨架
2. **后配置参数** - 在 Unity 中调整细节
3. **保存模板** - 配置好后保存为 Prefab 模板
4. **文档同步** - 修改 Prefab 后重新生成文档

---

## 故障排除

### 问题 1: Python 脚本执行失败

**原因**: 缺少依赖或路径错误

**解决**:
```bash
# 检查 Python 版本（需要 3.6+）
python3 --version

# 检查路径
pwd
ls scripts/
```

### 问题 2: Unity 未找到

**原因**: Unity 路径不在预设位置

**解决**:
1. 手动执行方法 2
2. 或修改 `auto_create_prefab.sh` 中的 `UNITY_PATHS` 变量

### 问题 3: 生成的 Prefab 缺少组件

**原因**: 命名不符合规则或文档格式错误

**解决**:
1. 检查文档格式
2. 查看 `component_rules.py` 中的映射规则
3. 手动在 Unity 中补充组件

### 问题 4: Unity 命令行执行失败

**原因**: 项目未正确识别或编译错误

**解决**:
```bash
# 查看 Unity 日志
cat /tmp/unity_prefab_builder_*.log

# 手动执行 Unity
/Applications/Unity/Hub/Editor/*/Unity.app/Contents/MacOS/Unity \
    -quit -batchmode -projectPath $(pwd) \
    -executeMethod CreatePrefabName.CreatePrefab \
    -logFile /tmp/unity.log
```

---

## 技术细节

### 文档解析原理

1. **提取层级结构**：
   - 使用正则表达式匹配 `●` 和 `├─` 标记
   - 根据缩进计算层级深度
   - 构建 GameObject 树

2. **组件推断**：
   - 优先使用命名前缀规则
   - 其次使用文档中的组件列表
   - 最后使用默认规则

3. **代码生成**：
   - 递归遍历 GameObject 树
   - 生成 `new GameObject()` 和 `AddComponent<>()` 代码
   - 生成 `SetParent()` 建立层级关系

### Unity 自动化执行

**方式 1: Menu Item（手动触发）**
```csharp
[MenuItem("Tools/Create Prefab/PrefabName")]
public static void CreateFromMenu() { ... }
```

**方式 2: ExecuteMethod（命令行触发）**
```bash
Unity -executeMethod ClassName.MethodName
```

---

## 未来扩展

### P1: 增强参数配置

**目标**: 扩展文档格式包含参数信息

**示例**:
```markdown
● **img_reward** `[Image]` {sprite: "Sprites/coin", color: "#FFFF00"}
● **tmp_reward** `[TextMeshProUGUI]` {fontSize: 24, text: "1000"}
```

### P2: 自动布局计算

**目标**: 根据类型推断合理的布局

**规则**:
- 按钮: 居中，宽度 200
- 文本: 上方，宽度 300
- 图片: 中心，保持原始大小

### P3: 脚本引用自动连接

**目标**: 生成并执行序列化字段连接代码

**示例**:
```csharp
// 自动查找并连接
popup.btnClaim = root.transform.Find("Content/btn_claim").GetComponent<Button>();
```

---

## 维护建议

1. **更新组件映射** - 根据项目需要添加新的命名规则
2. **同步文档** - 修改 Prefab 后重新生成文档
3. **定期测试** - 确保生成脚本与 Unity 版本兼容

---

## 相关技能

- **blockgame-scene-reference** - 从 Prefab 生成文档（逆向操作）
- **block-unity-standards** - Unity 开发规范和命名约定

---

**创建时间**: 2026-03-11
**版本**: 1.0.0
**作者**: BlockGame Team
