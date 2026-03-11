# 组件映射规则详解

## 概述

此文档详细说明了 GameObject 命名规则到 Unity 组件的映射关系。

---

## UI 元素命名规则

### 文本元素

**优先级**: Text > TextMeshPro

#### txt_ → Text

**命名空间**: `UnityEngine.UI`

**用途**: 传统 UI Text 文本显示

**示例**:
- `txt_reward` - 奖励数量文本
- `txt_score` - 分数显示
- `txt_level` - 等级文本

**组件配置**（默认）:
```csharp
AddComponent<Text>();
// fontSize: 24
// alignment: Center
// color: White
```

#### tmp_ → TextMeshProUGUI

**命名空间**: `TMPro`

**用途**: TextMeshPro 高级文本显示

**示例**:
- `tmp_title` - 标题文本（支持富文本效果）
- `tmp_amount` - 数值显示（支持更好的字体渲染）
- `tmp_description` - 描述文本

**组件配置**（默认）:
```csharp
AddComponent<TextMeshProUGUI>();
// fontSize: 24
// alignment: Center
// color: White
```

---

### 图片元素

#### img_ → Image

**命名空间**: `UnityEngine.UI`

**用途**: 显示图片/精灵

**示例**:
- `img_reward` - 奖励图标
- `img_background` - 背景图片
- `img_icon` - 图标

**组件配置**（默认）:
```csharp
AddComponent<Image>();
// raycastTarget: false（普通图片不响应点击）
```

---

### 按钮元素

#### btn_ → Button + Image

**命名空间**: `UnityEngine.UI`

**用途**: 可点击的按钮

**示例**:
- `btn_claim` - 领取按钮
- `btn_close` - 关闭按钮
- `btn_start` - 开始按钮

**组件配置**（默认）:
```csharp
AddComponent<Image>();     // 按钮背景
AddComponent<Button>();    // 按钮逻辑
// image.raycastTarget: true
// button.interactable: true
```

**子节点**: 自动创建 `Text (TMP)` 文本子节点（如果文档中有）

---

### 面板容器

#### panel_ → CanvasGroup + Image

**命名空间**: `UnityEngine.UI`

**用途**: 面板容器，可整体控制透明度和交互

**示例**:
- `panel_main` - 主面板
- `panel_settings` - 设置面板
- `panel_header` - 头部面板

**组件配置**（默认）:
```csharp
AddComponent<CanvasGroup>();
AddComponent<Image>();
// canvasGroup.alpha: 1.0
// canvasGroup.blocksRaycasts: true
```

---

### 滚动视图

#### scroll_ → ScrollRect + Image

**命名空间**: `UnityEngine.UI`

**用途**: 可滚动的列表或区域

**示例**:
- `scroll_list` - 列表滚动视图
- `scroll_content` - 内容滚动区域

**组件配置**（默认）:
```csharp
AddComponent<ScrollRect>();
AddComponent<Image>();
```

**常见子节点**:
- `Viewport` - 视口（RectMask2D）
  - `Content` - 内容容器

---

### 开关元素

#### toggle_ → Toggle + Image

**命名空间**: `UnityEngine.UI`

**用途**: 开关/复选框

**示例**:
- `toggle_sound` - 音效开关
- `toggle_music` - 音乐开关
- `toggle_vibrate` - 震动开关

**组件配置**（默认）:
```csharp
AddComponent<Toggle>();
AddComponent<Image>();
```

---

### 滑动条

#### slider_ → Slider

**命名空间**: `UnityEngine.UI`

**用途**: 滑动条控制

**示例**:
- `slider_volume` - 音量滑动条
- `slider_brightness` - 亮度滑动条

**组件配置**（默认）:
```csharp
AddComponent<Slider>();
```

---

### 输入框

#### input_ → TMP_InputField

**命名空间**: `TMPro`

**用途**: 文本输入框

**示例**:
- `input_name` - 名称输入
- `input_search` - 搜索框

**组件配置**（默认）:
```csharp
AddComponent<TMP_InputField>();
```

---

## 特殊节点

### Content → RectTransform

**用途**: 通用内容容器

**说明**:
- 不添加额外组件
- 只有默认的 RectTransform
- 通常作为 ScrollView 或 Panel 的子节点

**示例**:
```
● **Panel**
  ├─ **Content**  ← 只有 RectTransform
    ├─ **item_1**
    ├─ **item_2**
```

---

### Viewport → RectMask2D

**命名空间**: `UnityEngine.UI`

**用途**: ScrollView 的视口，裁剪超出部分

**说明**:
- 自动添加 RectMask2D 组件
- 用于 ScrollRect 的视口

**示例**:
```
● **scroll_list** `[ScrollRect, Image]`
  ├─ **Viewport** `[RectMask2D]`  ← 视口
    ├─ **Content**
```

---

### Scrollbar Horizontal / Vertical → Scrollbar

**命名空间**: `UnityEngine.UI`

**用途**: 滚动条

**说明**:
- 自动添加 Scrollbar 组件
- 通常作为 ScrollRect 的子节点

---

## 文档组件优先级

### 组件推断顺序

1. **特殊节点名称**（最高优先级）
   - 例如：`Content`, `Viewport`

2. **命名前缀规则**
   - 例如：`btn_`, `img_`, `tmp_`

3. **文档组件列表**
   - 例如：`[Animator, CanvasGroup]`

4. **默认规则**（最低优先级）
   - 只有 RectTransform

### 示例

**输入**:
```markdown
● **btn_claim** `[Animator]`
```

**组件推断**:
- ✅ 前缀规则: `btn_` → Button + Image
- ✅ 文档组件: `Animator`
- **最终**: `Image`, `Button`, `Animator`

---

## 命名空间管理

### 自动引入的命名空间

根据组件类型自动引入：

```csharp
using UnityEngine;              // 总是引入
using UnityEditor;              // 总是引入
using UnityEngine.UI;           // 当有 UI 组件时
using TMPro;                    // 当有 TMP 组件时
```

### 组件到命名空间的映射

| 组件 | 命名空间 |
|------|---------|
| Image, Button, Toggle, Slider | UnityEngine.UI |
| ScrollRect, CanvasGroup | UnityEngine.UI |
| CanvasScaler, GraphicRaycaster | UnityEngine.UI |
| RectMask2D, Scrollbar | UnityEngine.UI |
| TextMeshProUGUI, TMP_InputField | TMPro |
| Animator, CanvasRenderer | UnityEngine |

---

## 自定义规则

### 添加新的命名规则

编辑 `scripts/component_rules.py`:

```python
COMPONENT_MAPPING = {
    # 添加新规则
    "icon_": {
        "components": ["Image"],
        "namespace": "UnityEngine.UI",
        "description": "图标元素"
    },

    "label_": {
        "components": ["TextMeshProUGUI"],
        "namespace": "TMPro",
        "description": "标签文本"
    },
}
```

### 修改现有规则

```python
# 修改按钮规则，添加 Shadow 组件
"btn_": {
    "components": ["Image", "Button", "Shadow"],
    "namespace": "UnityEngine.UI",
    "description": "按钮组件（带阴影）"
},
```

---

## 常见问题

### Q: 为什么按钮需要 Image + Button？

**A**: Unity UI 的 Button 组件需要一个图形组件（如 Image）来响应点击事件。

### Q: 可以自定义组件参数吗？

**A**: 当前版本不支持。未来版本计划支持在文档中指定参数。

### Q: 如何处理不符合规则的命名？

**A**:
1. 修改命名符合规则
2. 或在 `component_rules.py` 中添加新规则
3. 或在生成后手动添加组件

---

**维护**: 根据项目需要更新映射规则

**版本**: 1.0.0
