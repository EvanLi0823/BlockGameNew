# PropPurchase Prefab 结构文档

**生成时间**: 2026-03-11 13:18:00
**文件路径**: `Assets/BlockPuzzleGameToolkit/Resources/Popups/PropPurchase.prefab`
**文件类型**: Prefab
**GameObject总数**: 13
**根节点数量**: 1

---

## Prefab概览

PropPurchase Prefab 包含以下主要组件。

---

## 完整Hierarchy层级结构


### 根节点 1: PropPurchase

```
● **PropPurchase** `[Animator, CanvasGroup, MonoBehaviour]`
  ├─ **Content** `[Animator, CanvasGroup]`
    ├─ **img_bg** `[CanvasRenderer, MonoBehaviour]`
    ├─ **img_title** `[CanvasRenderer, MonoBehaviour]`
    ├─ **img_title_1** `[CanvasRenderer, MonoBehaviour]`
    ├─ **content**
      ├─ **img_propIcon** `[CanvasRenderer, MonoBehaviour]`
      ├─ **tmp_propCount** `[CanvasRenderer, MonoBehaviour]`
    ├─ **btn_ad** `[CanvasRenderer, MonoBehaviour, MonoBehaviour]`
      ├─ **tmp_adBtn** `[CanvasRenderer, MonoBehaviour]`
      ├─ **img_ad** `[CanvasRenderer, MonoBehaviour]`
    ├─ **btn_close** `[CanvasRenderer, Animator, MonoBehaviour]`
      ├─ **img_ad** `[CanvasRenderer, MonoBehaviour]`
```


---

## 使用说明

### 如何查找GameObject

1. **使用Ctrl+F搜索GameObject名称**
2. **查看层级结构了解父子关系**
3. **查看组件列表了解功能**

### 常见查询

**查找UI面板**:
- 搜索 "Panel", "Canvas", "Menu"

**查找Manager**:
- 搜索 "Manager"

**查找Button**:
- 搜索 "Button", "Btn"

---

**注意**: 此文档由脚本自动生成，场景结构可能会随开发变化。建议定期更新此文档。
