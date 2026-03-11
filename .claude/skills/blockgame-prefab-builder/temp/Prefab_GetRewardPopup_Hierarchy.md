# GetRewardPopup Prefab Hierarchy

**用途**: 获奖弹窗，显示玩家获得的金钱奖励并播放飞币动画

**功能描述**:
- 黑色背景遮罩
- 转动发光粒子效果
- 金钱图标显示
- 奖励数值展示
- 弹窗缩放动画（从小到大弹出，从大到小收缩）

**弹出方式**: 由 MenuManager 调用弹出

**表现逻辑**:
1. 弹窗从小到大弹出
2. 展示 1 秒
3. 调用 FlyRewardManager 执行飞币动画
4. 在飞币动画第一阶段，弹窗从大到小迅速消失

---

## Prefab 节点结构

```
● **GetRewardPopup** `[Animator, CanvasGroup]`
  ├─ **Content**
    ├─ **img_background** `[Image]`
    ├─ **particle_glow** `[ParticleSystem]`
    ├─ **img_coin** `[Image]`
    ├─ **tmp_amount** `[TextMeshProUGUI]`
```

---

## 组件说明

| GameObject | 组件 | 说明 |
|-----------|------|------|
| **GetRewardPopup** | Animator, CanvasGroup | 根节点，控制弹窗动画和淡入淡出 |
| **Content** | RectTransform | 内容容器 |
| **img_background** | Image | 黑色背景遮罩 |
| **particle_glow** | ParticleSystem | 转动发光粒子效果 |
| **img_coin** | Image | 金钱图标 |
| **tmp_amount** | TextMeshProUGUI | 奖励数值文本 |

---

## 后续配置建议

### 1. Animator 配置
- 创建 GetRewardPopup.controller
- 添加 PopIn 动画（Scale: 0 → 1，Duration: 0.3s）
- 添加 PopOut 动画（Scale: 1 → 0，Duration: 0.2s）

### 2. Image 配置
- **img_background**: 设置黑色半透明图片（Alpha: 0.8）
- **img_coin**: 设置金币图标 Sprite

### 3. TextMeshProUGUI 配置
- **tmp_amount**: 字体大小 60，颜色金黄色，居中对齐

### 4. ParticleSystem 配置
- **particle_glow**:
  - Shape: Circle
  - Emission: 20 particles/sec
  - Start Lifetime: 1.5s
  - Start Speed: 50
  - Start Rotation: 旋转动画
  - Renderer: 发光材质

### 5. RectTransform 布局
- **GetRewardPopup**: Anchors = Center, Size = (600, 400)
- **Content**: Anchors = Stretch, Offsets = (0, 0, 0, 0)
- **img_background**: Anchors = Stretch
- **particle_glow**: Position = (0, 50, 0)
- **img_coin**: Position = (0, 0, 0), Size = (200, 200)
- **tmp_amount**: Position = (0, -120, 0)

---

## 相关脚本

建议创建 `GetRewardPopupController.cs`:

```csharp
public class GetRewardPopupController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tmpAmount;
    [SerializeField] private ParticleSystem particleGlow;
    [SerializeField] private Animator animator;

    public void Show(int rewardAmount)
    {
        tmpAmount.text = rewardAmount.ToString();
        animator.SetTrigger("PopIn");
        particleGlow.Play();

        // 1秒后触发飞币动画
        StartCoroutine(TriggerFlyReward());
    }

    private IEnumerator TriggerFlyReward()
    {
        yield return new WaitForSeconds(1f);

        // 调用 FlyRewardManager
        // FlyRewardManager.Instance?.PlayFlyAnimation(...);

        // 播放收缩动画
        animator.SetTrigger("PopOut");
    }
}
```

---

**生成日期**: 2025-01-27
**版本**: 1.0
