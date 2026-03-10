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

using System;
using System.Collections.Generic;
using System.Linq;
using BlockPuzzleGameToolkit.Scripts.LevelsData;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace BlockPuzzleGameToolkit.Scripts.Gameplay
{
    /// <summary>
    /// ShapeRow - 表示Shape中的一行
    /// 每行包含5个Item格子
    /// </summary>
    [Serializable]
    public class ShapeRow
    {
        public Item[] cells = new Item[5];  // 固定5个格子的数组
    }

    /// <summary>
    /// Shape 类表示可拖拽的方块组
    /// 采用5x5的网格结构，根据ShapeTemplate定义哪些格子被激活
    /// 支持自定义Item和对象池优化
    /// </summary>
    public class Shape : MonoBehaviour
    {
        // 形状的数据模板，定义哪些格子被激活
        public ShapeTemplate shapeTemplate;

        // 5x5的网格结构，存储所有Item
        public ShapeRow[] row;

        // 形状更新时触发的事件
        public Action OnShapeUpdated;

        // 左上角的第一个激活的Item（用于对齐计算）
        public Item topLeftItem;

        // 当前激活的Item列表（实际显示的方块）
        private readonly List<Item> activeItems = new();

        // 初始模板（用于奖励道具的背景显示）
        private ItemTemplate initialTemplate;

        // DOTween动画序列
        private Sequence _sequence;

        // 原始Item字典（保存被自定义Item替换前的原始Item）
        private readonly Dictionary<Vector2Int, Item> originalItems = new();

        // 自定义Item字典（从对象池获取的自定义Item）
        private readonly Dictionary<Vector2Int, Item> customItems = new();

        // 自定义Item的静态对象池
        private static readonly Dictionary<string, ObjectPool<Item>> CustomItemPools = new();

        /// <summary>
        /// Unity生命周期 - 初始化
        /// </summary>
        private void Awake()
        {
            // 加载默认的初始模板（用于奖励道具）
            initialTemplate = Resources.Load<ItemTemplate>("Items/ItemTemplate 0");
            // 更新形状显示
            UpdateShape(shapeTemplate);
        }

        /// <summary>
        /// Unity生命周期 - 对象禁用时的清理
        /// 将自定义Item返回对象池，恢复原始Item
        /// </summary>
        private void OnDisable()
        {
            // 将所有自定义Item返回对象池
            foreach (var kvp in customItems)
            {
                if (kvp.Value != null)
                {
                    GetOrCreatePool(kvp.Value.GetComponent<Item>()).Release(kvp.Value);
                }
            }

            // 恢复所有原始Item到正确位置
            foreach (var kvp in originalItems)
            {
                var position = kvp.Key;
                var i = position.y;
                var j = position.x;

                // 恢复到网格中的原始位置
                row[i].cells[j] = kvp.Value;
                kvp.Value.gameObject.SetActive(true);

                // 更新activeItems列表中的引用
                if (activeItems.Contains(customItems[position]))
                {
                    activeItems[activeItems.IndexOf(customItems[position])] = kvp.Value;
                }
            }

            // 清空字典
            customItems.Clear();
            originalItems.Clear();
        }

        /// <summary>
        /// 更新形状 - 根据ShapeTemplate配置激活/禁用格子
        /// 这是Shape的核心配置方法
        /// </summary>
        /// <param name="shapeTemplate">形状模板数据</param>
        public void UpdateShape(ShapeTemplate shapeTemplate)
        {
            activeItems.Clear();

            // 遍历5x5网格，根据模板激活对应的Item
            for (var i = 0; i < row.Length; i++)
            {
                for (var j = 0; j < row[i].cells.Length; j++)
                {
                    var item = row[i].cells[j];
                    // 根据模板的布尔值决定是否激活
                    item.gameObject.SetActive(shapeTemplate.rows[i].cells[j]);
                    // 设置Item在网格中的位置
                    item.SetPosition(new Vector2Int(j, i));
                    // 清除奖励道具
                    item.ClearBonus();
                    // 将激活的Item添加到列表
                    if (item.gameObject.activeSelf)
                    {
                        activeItems.Add(item);
                    }
                }
            }

            // 计算形状的重心，用于居中显示
            var centroid = CalculateCentroid();
            transform.GetChild(0).localPosition -= centroid;

            // 获取左上角的第一个Item
            topLeftItem = GetTopLeftItem();

            // 触发形状更新事件
            OnShapeUpdated?.Invoke();
        }

        /// <summary>
        /// 获取左上角第一个激活的Item
        /// 用于对齐和定位计算
        /// </summary>
        /// <returns>左上角的Item，如果没有则返回null</returns>
        private Item GetTopLeftItem()
        {
            // 从左上角开始遍历
            for (var i = 0; i < row.Length; i++)
            {
                for (var j = 0; j < row[i].cells.Length; j++)
                {
                    var item = row[i].cells[j];
                    if (item.gameObject.activeSelf)
                    {
                        return item;  // 返回第一个找到的激活Item
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 更新形状的颜色 - 为所有激活的Item应用新的颜色模板
        /// 支持自定义Item预制体的替换
        /// </summary>
        /// <param name="itemTemplate">要应用的物品模板</param>
        public void UpdateColor(ItemTemplate itemTemplate)
        {
            for (var i = 0; i < row.Length; i++)
            {
                for (var j = 0; j < row[i].cells.Length; j++)
                {
                    var item = row[i].cells[j];
                    if (!item.HasBonusItem())
                    {
                        var position = new Vector2Int(j, i);
                        if (itemTemplate.customItemPrefab != null)
                        {
                            if (!originalItems.ContainsKey(position))
                            {
                                originalItems[position] = item;
                            }

                            if (customItems.TryGetValue(position, out var existingCustomItem))
                            {
                                GetOrCreatePool(existingCustomItem.GetComponent<Item>()).Release(existingCustomItem);
                            }

                            var newItem = GetOrCreatePool(itemTemplate.customItemPrefab).Get();
                            newItem.transform.SetParent(item.transform.parent);
                            newItem.transform.position = item.transform.position;
                            newItem.transform.localScale = item.transform.localScale;
                            newItem.SetPosition(position);
                            newItem.gameObject.SetActive(item.gameObject.activeSelf);
                            customItems[position] = newItem;
                            row[i].cells[j] = newItem;
                            item.gameObject.SetActive(false);

                            if (activeItems.Contains(item))
                            {
                                activeItems[activeItems.IndexOf(item)] = newItem;
                            }
                        }
                        else
                        {
                            item.UpdateColor(itemTemplate);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取或创建指定预制体的对象池
        /// 性能优化：避免频繁实例化和销毁
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
        /// 辅助方法 - 检查Item是否为空
        /// </summary>
        private bool GetValue(Item prefab, Item item, ObjectPool<Item> pool)
        {
            if (item == null)
            {
                pool.Clear();
                CustomItemPools.Remove(prefab.name);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 获取当前激活的Item列表
        /// </summary>
        /// <returns>激活的Item列表</returns>
        public List<Item> GetActiveItems()
        {
            return activeItems;
        }

        /// <summary>
        /// 为形状设置奖励道具
        /// 随机选择部分Item添加奖励道具
        /// </summary>
        /// <param name="bonus">奖励道具模板</param>
        /// <param name="maxValue">最大奖励道具数量</param>
        public void SetBonus(BonusItemTemplate bonus, int maxValue)
        {
            // 限制最多2个奖励道具
            maxValue = Mathf.Min(maxValue, 2);
            var bonusesAssigned = 0;
            var lastBonusIndex = -2; // 初始化为-2，确保第一个Item可以被分配奖励
            var bonusAssigned = false;

            for (var i = 0; i < activeItems.Count; i++)
            {
                if (bonusesAssigned >= maxValue)
                {
                    break;
                }

                // 确保奖励道具之间至少间隔1个Item
                if (i - lastBonusIndex > 1 && Random.Range(0, 3) == 0)
                {
                    SetBonus(activeItems[i], bonus);
                    bonusesAssigned++;
                    lastBonusIndex = i;
                    bonusAssigned = true;
                }
            }

            // 确保至少有一个Item获得奖励（如果没有分配的话）
            if (!bonusAssigned && activeItems.Count > 0)
            {
                SetBonus(activeItems[0], bonus);
            }
        }

        /// <summary>
        /// 为单个Item设置奖励道具
        /// </summary>
        private void SetBonus(Item item, BonusItemTemplate bonus)
        {
            item.UpdateColor(initialTemplate);  // 使用初始模板作为背景
            item.SetBonus(bonus);
        }

        /// <summary>
        /// 检查形状是否包含奖励道具
        /// </summary>
        /// <returns>是否包含奖励道具</returns>
        public bool HasBonusItem()
        {
            return activeItems.Any(item => item.HasBonusItem());
        }

        /// <summary>
        /// 计算形状的重心
        /// 用于居中显示形状
        /// </summary>
        /// <returns>重心位置（本地坐标）</returns>
        public Vector3 CalculateCentroid()
        {
            var centroid = Vector3.zero;
            var items = GetActiveItems();

            // 累加所有激活Item的位置
            foreach (var item in items)
            {
                centroid += item.transform.position;
            }

            // 计算平均位置（重心）
            if (items.Count > 0)
            {
                centroid /= items.Count;
            }

            // 转换为本地坐标
            return transform.InverseTransformPoint(centroid);
        }
    }
}