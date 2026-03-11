# SimpleRewardPopup Prefab Example

**用途**: 演示如何从文档生成简单的奖励弹窗 Prefab

**节点数**: 7 GameObjects

---

## Prefab Hierarchy

```
● **SimpleRewardPopup** `[CanvasGroup]`
  ├─ **Content**
    ├─ **img_background** `[Image]`
    ├─ **tmp_title** `[TextMeshProUGUI]`
    ├─ **img_reward** `[Image]`
    ├─ **tmp_amount** `[TextMeshProUGUI]`
    ├─ **btn_claim** `[Button, Image]`
      ├─ **Text (TMP)** `[TextMeshProUGUI]`
```

---

## 生成命令

```bash
# 从技能目录运行
python3 scripts/generate_prefab.py examples/SimpleRewardPopup.md

# 生成的脚本位置
# Assets/Editor/Generated/CreateSimpleRewardPopup.cs
```

---

## Unity 执行

### 方法 1: 菜单（推荐新手）

1. 打开 Unity Editor
2. 菜单: `Tools → Create Prefab → SimpleRewardPopup`
3. 查看 `Assets/Generated/Prefabs/SimpleRewardPopup.prefab`

### 方法 2: 命令行（自动化）

```bash
/Applications/Unity/Hub/Editor/2021.3.45f2/Unity.app/Contents/MacOS/Unity \
    -quit \
    -batchmode \
    -projectPath /Users/lifan/BlockGameNew \
    -executeMethod CreateSimpleRewardPopup.CreatePrefab \
    -logFile /tmp/unity_create_prefab.log
```

---

## 生成的组件

| GameObject | 组件 | 推断规则 |
|-----------|------|---------|
| **SimpleRewardPopup** | CanvasGroup | 文档指定 |
| **Content** | RectTransform | 特殊节点 |
| **img_background** | Image | `img_` 前缀 |
| **tmp_title** | TextMeshProUGUI | `tmp_` 前缀 |
| **img_reward** | Image | `img_` 前缀 |
| **tmp_amount** | TextMeshProUGUI | `tmp_` 前缀 |
| **btn_claim** | Image + Button | `btn_` 前缀 |
| **Text (TMP)** | TextMeshProUGUI | 按钮子节点 |

---

## 后续配置

生成 Prefab 后，在 Unity 中需要手动配置：

### 1. 设置图片

- `img_background` - 设置 sprite 为背景图
- `img_reward` - 设置 sprite 为奖励图标

### 2. 设置文本

- `tmp_title` - 文本内容: "恭喜获得奖励！"
- `tmp_amount` - 文本内容: "1000"
- `btn_claim/Text (TMP)` - 文本内容: "领取"

### 3. 设置布局

- `Content` - 调整锚点为中心
- `btn_claim` - 设置大小和位置

### 4. 设置脚本引用

如果有 `SimpleRewardPopup.cs` 脚本：

```csharp
public class SimpleRewardPopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tmpTitle;
    [SerializeField] private Image imgReward;
    [SerializeField] private TextMeshProUGUI tmpAmount;
    [SerializeField] private Button btnClaim;

    // 手动连接这些字段
}
```

---

## 测试

生成后，在 Unity 中：

1. 将 Prefab 拖到场景中
2. 进入 Play 模式
3. 检查所有 GameObject 是否正确创建
4. 检查组件是否正确添加

---

**提示**: 这是一个最简示例，用于学习生成流程。实际项目中的 Prefab 通常更复杂。
