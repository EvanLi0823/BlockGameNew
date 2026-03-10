// // ©2015 - 2025 Candy Smith
// // All rights reserved
// // Redistribution of this software is strictly not allowed.
// // Copy of this software can be obtained from unity asset store only.
// // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// // IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// // FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// // AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// // LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// // OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// // THE SOFTWARE.

using System.Collections;
using System.Collections.Generic;
using BlockPuzzleGameToolkit.Scripts.LevelsData;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.MoneyBlockSystem;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Pool;

namespace BlockPuzzleGameToolkit.Scripts.Gameplay
{
    /// <summary>
    /// Cell 类表示游戏棋盘上的一个格子单元
    /// 负责管理格子状态、显示物品、处理消除动画等
    /// 每个Cell可以包含一个Item（方块），并支持对象池优化
    /// </summary>
    public class Cell : MonoBehaviour
    {
        // 自定义物品的对象池字典，按预制体名称索引，用于性能优化
        private static readonly Dictionary<string, ObjectPool<Item>> CustomItemPools = new();

        // 格子中显示的物品（方块）
        public Item item;

        // 控制物品透明度的CanvasGroup组件
        private CanvasGroup group;

        // 标记格子是否被占用
        public bool busy;

        // 保存临时的物品模板，用于高亮预览时的状态恢复
        private ItemTemplate saveTemplate;

        // 碰撞检测组件，用于判断格子的边界
        private BoxCollider2D _boxCollider2D;

        // 标记格子是否正在执行销毁动画
        private bool isDestroying;

        // 保存原始的Item引用，在使用自定义预制体时保留原始物品
        private Item originalItem;

        // 当前使用的自定义Item（从对象池获取）
        private Item customItem;

        // 格子背景图片，用于教学高亮等效果
        public Image image;

        // 判断格子是否为空（基于busy状态）
        private bool isEmpty => !busy;

        // 判断格子是否在预览状态（基于透明度）
        private bool IsEmptyPreview => group.alpha == 0;

        /// <summary>
        /// Unity生命周期 - 初始化组件引用
        /// </summary>
        private void Awake()
        {
            _boxCollider2D = GetComponent<BoxCollider2D>();
            group = item.GetComponent<CanvasGroup>();
            CustomItemPools.Clear();  // 清空对象池，避免内存泄漏
        }

        /// <summary>
        /// 获取或创建指定预制体的对象池
        /// 对象池用于优化性能，避免频繁的实例化和销毁
        /// </summary>
        /// <param name="prefab">Item预制体</param>
        /// <returns>对应的对象池实例</returns>
        private ObjectPool<Item> GetOrCreatePool(Item prefab)
        {
            if (!CustomItemPools.TryGetValue(prefab.name, out var pool))
            {
                pool = new ObjectPool<Item>(
                    // 创建新对象的函数
                    createFunc: () =>
                    {
                        var instantiate = Instantiate(prefab,transform);
                        instantiate.name = prefab.name;
                        return instantiate;
                    },
                    // 从池中获取对象时的处理
                    actionOnGet: item =>
                    {
                        if (GetValue(prefab, item, pool))
                        {
                            return;
                        }
                        item.transform.SetParent(transform);
                        item.gameObject.SetActive(true);
                    },
                    // 将对象返回池中时的处理
                    actionOnRelease: item =>
                    {
                        if (GetValue(prefab, item, pool))
                        {
                            return;
                        }
                        if (item?.gameObject != null)
                            item.gameObject.SetActive(false);
                    },
                    // 销毁对象时的处理
                    actionOnDestroy: item =>
                    {
                        if (GetValue(prefab, item, pool))
                        {
                            return;
                        }
                        if (item?.gameObject != null)
                            Destroy(item.gameObject);
                    }
                );
                CustomItemPools[prefab.name] = pool;
            }
            return pool;
        }

        /// <summary>
        /// 辅助方法 - 检查Item是否为空，如果为空则清理对象池
        /// </summary>
        /// <param name="prefab">预制体引用</param>
        /// <param name="item">待检查的Item</param>
        /// <param name="pool">对应的对象池</param>
        /// <returns>如果Item为空返回true，否则返回false</returns>
        private bool GetValue(Item prefab, Item item, ObjectPool<Item> pool)
        {
            if (item == null)
            {
                pool.Clear();
                CustomItemPools.Remove(prefab.name);
                ClearCell();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 使用自定义Item预制体替换当前的Item
        /// 用于支持特殊的视觉效果或不同类型的方块
        /// </summary>
        /// <param name="itemTemplate">包含自定义预制体的物品模板</param>
        private void ReplaceWithCustomItem(ItemTemplate itemTemplate)
        {
            // 首次替换时，保存原始Item的引用
            if (originalItem == null)
            {
                originalItem = item;
                originalItem.gameObject.SetActive(false);
            }

            // 如果已有自定义Item，先将其返回对象池
            if (customItem != null)
            {
                GetOrCreatePool(itemTemplate.customItemPrefab).Release(customItem);
            }

            // 从对象池获取新的自定义Item
            customItem = GetOrCreatePool(itemTemplate.customItemPrefab).Get();
            customItem.transform.SetParent(transform);
            customItem.transform.position = originalItem.transform.position;
            customItem.transform.localScale = originalItem.transform.localScale;

            // 同步RectTransform属性（UI布局）
            var rectTransform = customItem.GetComponent<RectTransform>();
            var originalRect = originalItem.GetComponent<RectTransform>();
            if (rectTransform != null && originalRect != null)
            {
                rectTransform.anchorMin = originalRect.anchorMin;
                rectTransform.anchorMax = originalRect.anchorMax;
                rectTransform.pivot = originalRect.pivot;
                rectTransform.sizeDelta = originalRect.sizeDelta;
                rectTransform.anchoredPosition = originalRect.anchoredPosition;
            }

            // 更新当前使用的Item引用
            item = customItem;
            group = item.GetComponent<CanvasGroup>();
        }

        /// <summary>
        /// 填充格子 - 将指定的物品模板应用到格子中
        /// 这是放置方块到格子的核心方法
        /// </summary>
        /// <param name="itemTemplate">要填充的物品模板</param>
        public void FillCell(ItemTemplate itemTemplate)
        {
            if (itemTemplate.customItemPrefab != null)
            {
                ReplaceWithCustomItem(itemTemplate);
            }
            else 
            {
                if (originalItem != null)
                {
                    if (customItem != null)
                    {
                        Destroy(customItem.gameObject);
                        customItem = null;
                    }
                    item = originalItem;
                    item.gameObject.SetActive(true);
                    originalItem = null;
                    group = item.GetComponent<CanvasGroup>();
                }
            }

            item.FillIcon(itemTemplate);
            group.alpha = 1;
            busy = true;
        }

        public void FillCellFailed(ItemTemplate itemTemplate)
        {
            item.FillIcon(itemTemplate);
            group.alpha = 1;
        }

        /// <summary>
        /// 判断格子是否为空
        /// </summary>
        /// <param name="preview">是否检查预览状态（基于透明度）</param>
        /// <returns>格子是否为空</returns>
        public bool IsEmpty(bool preview = false)
        {
            return preview ? IsEmptyPreview || isDestroying: isEmpty;
        }

        /// <summary>
        /// 清空格子 - 重置格子到初始状态
        /// 处理对象池回收和状态重置
        /// </summary>
        public void ClearCell()
        {
            // 如果有自定义Item，将其返回对象池
            if (customItem != null)
            {
                GetOrCreatePool(customItem.GetComponent<Item>()).Release(customItem);
                customItem = null;
            }

            // 如果有保存的原始Item，恢复使用原始Item
            if (originalItem != null)
            {
                item = originalItem;
                item.gameObject.SetActive(true);
                originalItem = null;
                group = item.GetComponent<CanvasGroup>();
            }

            // 重置缩放
            item.transform.localScale = Vector3.one;

            // 处理保存的模板（用于预览状态的恢复）
            if (saveTemplate == null && !busy)
            {
                group.alpha = 0;  // 设置为完全透明
                busy = false;
            }
            else if (saveTemplate != null && busy)
            {
                FillCell(saveTemplate);  // 恢复之前保存的状态
                saveTemplate = null;
            }
        }

        /// <summary>
        /// 高亮显示格子 - 用于拖拽预览效果
        /// 显示半透明的方块预览
        /// </summary>
        /// <param name="itemTemplate">要预览的物品模板</param>
        public void HighlightCell(ItemTemplate itemTemplate)
        {
            if (itemTemplate.customItemPrefab != null)
            {
                ReplaceWithCustomItem(itemTemplate);
            }
            else 
            {
                if (originalItem != null)
                {
                    if (customItem != null)
                    {
                        Destroy(customItem.gameObject);
                        customItem = null;
                    }
                    item = originalItem;
                    item.gameObject.SetActive(true);
                    originalItem = null;
                    group = item.GetComponent<CanvasGroup>();
                }
            }

            item.FillIcon(itemTemplate);
            group.alpha = 0.05f; // Make it semi-transparent to indicate it's a highlight
        }

        /// <summary>
        /// 高亮格子用于教学 - 显示教学引导的特殊颜色
        /// </summary>
        public void HighlightCellTutorial()
        {
            image.color = new Color(43f / 255f, 59f / 255f, 120f / 255f, 1f);  // 深蓝色高亮
        }

        /// <summary>
        /// 高亮填充格子 - 在预览放置时保存当前状态
        /// 用于处理已有物品的格子被预览覆盖的情况
        /// </summary>
        /// <param name="itemTemplate">要预览的物品模板</param>
        public void HighlightCellFill(ItemTemplate itemTemplate)
        {
            // 保存当前的物品模板，以便恢复
            saveTemplate = item.itemTemplate;
            // 如果没有奖励道具，则更新显示为新的模板
            if (!item.HasBonusItem())
            {
                item.FillIcon(itemTemplate);
            }

            group.alpha = 1f;  // 完全不透明显示
        }

        /// <summary>
        /// 销毁格子 - 播放消除动画并清空格子
        /// 通过DOTween动画实现缩放消失效果
        /// </summary>
        public void DestroyCell()
        {
            saveTemplate = null;
            busy = false;

            // 在动画播放前触发ItemDestroyed事件（传递item的GameObject，因为MoneyBlock组件在item上）
            EventManager.GetEvent<GameObject>(EGameEvent.ItemDestroyed).Invoke(item.gameObject);

            // 缩放动画：0.2秒内缩放到0
            item.transform.DOScale(Vector3.zero, 0.2f).OnComplete(() =>
            {
                isDestroying = false;
                ClearCell();
                item.ClearBonus();  // 清除奖励道具
            });
        }

        /// <summary>
        /// 获取格子的边界信息
        /// 用于碰撞检测和位置计算
        /// </summary>
        /// <returns>格子的Bounds信息</returns>
        public Bounds GetBounds()
        {
            return _boxCollider2D.bounds;
        }

        /// <summary>
        /// 初始化格子中的Item
        /// 设置Item的名称和位置
        /// </summary>
        public void InitItem()
        {
            item.name = "Item " + name;
            StartCoroutine(UpdateItem());
        }

        /// <summary>
        /// 延迟更新Item的位置和大小
        /// 确保布局系统已完成计算
        /// </summary>
        private IEnumerator UpdateItem()
        {
            yield return new WaitForSeconds(0.1f);  // 等待布局更新
            // 同步碰撞盒大小与RectTransform
            _boxCollider2D.size = transform.GetComponent<RectTransform>().sizeDelta;
            // 设置Item位置
            item.transform.position = transform.position;
        }

        /// <summary>
        /// 设置格子的奖励道具
        /// </summary>
        /// <param name="bonusItemTemplate">奖励道具模板</param>
        public void SetBonus(BonusItemTemplate bonusItemTemplate)
        {
            item.SetBonus(bonusItemTemplate);

            // 检查是否是金钱方块图标，如果是则添加MoneyBlock组件
            var moneyBlockSettings = Resources.Load<MoneyBlockSettings>("Settings/MoneyBlockSettings");
            if (moneyBlockSettings != null && bonusItemTemplate == moneyBlockSettings.moneyBonusTemplate)
            {
                // 添加MoneyBlock组件到Item的GameObject
                var moneyBlock = item.gameObject.GetComponent<MoneyBlock>();
                if (moneyBlock == null)
                {
                    moneyBlock = item.gameObject.AddComponent<MoneyBlock>();
                }
                moneyBlock.Initialize();
            }
        }

        /// <summary>
        /// 检查格子是否包含奖励道具
        /// </summary>
        /// <returns>是否有奖励道具</returns>
        public bool HasBonusItem()
        {
            return item.HasBonusItem();
        }

        /// <summary>
        /// 获取格子的奖励道具模板
        /// </summary>
        /// <returns>奖励道具模板</returns>
        public BonusItemTemplate GetBonusItem()
        {
            return item.bonusItemTemplate;
        }

        /// <summary>
        /// 播放填充动画 - 先缩小再放大的弹性效果
        /// </summary>
        public void AnimateFill()
        {
            item.transform.DOScale(Vector3.one * 0.5f, 0.1f).OnComplete(() => {
                item.transform.DOScale(Vector3.one, 0.1f);
            });
        }

        /// <summary>
        /// 禁用格子 - 使格子不可被选中或放置
        /// </summary>
        public void DisableCell()
        {
            _boxCollider2D.enabled = false;
        }

        /// <summary>
        /// 检查格子是否被禁用
        /// </summary>
        /// <returns>格子是否被禁用</returns>
        public bool IsDisabled()
        {
            return !_boxCollider2D.enabled;
        }

        /// <summary>
        /// 检查格子是否被高亮（实际检查是否未被禁用）
        /// </summary>
        /// <returns>格子是否被高亮</returns>
        public bool IsHighlighted()
        {
            return !IsDisabled();
        }

        /// <summary>
        /// 设置格子的销毁状态
        /// </summary>
        /// <param name="destroying">是否正在销毁</param>
        public void SetDestroying(bool destroying)
        {
            isDestroying = destroying;
        }

        /// <summary>
        /// 检查格子是否正在销毁动画中
        /// </summary>
        /// <returns>是否正在销毁</returns>
        public bool IsDestroying()
        {
            return isDestroying;
        }

        /// <summary>
        /// Unity生命周期 - 对象销毁时的清理
        /// 确保自定义Item返回对象池
        /// </summary>
        private void OnDestroy()
        {
            if (customItem != null)
            {
                GetOrCreatePool(customItem.GetComponent<Item>()).Release(customItem);
            }
        }
    }
}