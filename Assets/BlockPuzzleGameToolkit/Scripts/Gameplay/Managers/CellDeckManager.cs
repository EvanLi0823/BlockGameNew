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
using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.Gameplay.Pool;
using BlockPuzzleGameToolkit.Scripts.LevelsData;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using BlockPuzzleGameToolkit.Scripts.MoneyBlockSystem;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Gameplay
{
    /// <summary>
    /// CellDeckManager - 形状队列管理器
    /// 负责管理游戏底部的3个待放置形状槽位
    /// 核心功能：确保游戏的可玩性（至少2个形状可放置）
    /// </summary>
    public class CellDeckManager : MonoBehaviour, Managers.ILevelLoadable
    {
        /// <summary>
        /// 形状槽位数组（通常是3个）
        /// </summary>
        public CellDeck[] cellDecks;

        /// <summary>
        /// 棋盘管理器引用
        /// </summary>
        [SerializeField]
        private FieldManager field;

        /// <summary>
        /// 物品工厂引用
        /// </summary>
        [SerializeField]
        private ItemFactory itemFactory;

        /// <summary>
        /// 形状预制体
        /// </summary>
        [SerializeField]
        public Shape shapePrefab;

        /// <summary>
        /// 已使用的形状模板集合（用于避免重复）
        /// </summary>
        private HashSet<ShapeTemplate> usedShapes = new HashSet<ShapeTemplate>();

        /// <summary>
        /// 当前关卡引用（用于获取初始方块配置）
        /// </summary>
        private Level currentLevel;

        /// <summary>
        /// 刷新计数器（用于追踪第几次刷新）
        /// </summary>
        private int refreshCount = 0;

        /// <summary>
        /// Unity生命周期 - 启用时订阅事件
        /// </summary>
        private void OnEnable()
        {
            // 订阅形状放置事件
            EventManager.GetEvent<Shape>(EGameEvent.ShapePlaced).Subscribe(FillCellDecks);
        }

        /// <summary>
        /// Unity生命周期 - 禁用时清理
        /// </summary>
        private void OnDisable()
        {
            ClearCellDecks();
            // 取消订阅事件
            EventManager.GetEvent<Shape>(EGameEvent.ShapePlaced).Unsubscribe(FillCellDecks);
        }

        /// <summary>
        /// 填充形状槽位 - 核心方法
        /// 当所有槽位都空时，生成3个新形状
        /// 保证至少2个形状可以放置到棋盘上
        /// 支持从Level配置读取初始几次刷新的方块类型
        /// </summary>
        /// <param name="shape">刚放置的形状（可选）</param>
        public void FillCellDecks(Shape shape = null)
        {
            // 移除已使用的形状
            RemoveUsedShapes(shape);

            // 教学模式下不自动填充
            if (GameManager.Instance.IsTutorialMode())
            {
                return;
            }

            // 如果还有形状未使用，不需要填充
            bool hasNonEmptyDeck = false;
            foreach (var deck in cellDecks)
            {
                if (!deck.IsEmpty)
                {
                    hasNonEmptyDeck = true;
                    break;
                }
            }

            if (hasNonEmptyDeck)
            {
                return;
            }

            // 检查是否使用配置的初始方块
            if (TryFillFromConfiguration())
            {
                // 使用配置成功，增加刷新计数
                refreshCount++;
                return;
            }

            // 没有配置或配置已用完，使用原有随机逻辑
            FillCellDecksRandom();
        }

        /// <summary>
        /// 尝试从Level配置填充方块
        /// </summary>
        /// <returns>是否成功使用配置</returns>
        private bool TryFillFromConfiguration()
        {
            // 检查配置有效性
            if (currentLevel == null ||
                currentLevel.initialShapeRefreshes == null ||
                refreshCount >= currentLevel.initialShapeRefreshes.Count)
            {
                return false;
            }

            var config = currentLevel.initialShapeRefreshes[refreshCount];
            if (config == null || config.slots == null || config.slots.Length == 0)
            {
                return false;
            }

            // 使用配置填充
            for (var index = 0; index < cellDecks.Length && index < config.slots.Length; index++)
            {
                var cellDeck = cellDecks[index];
                var slotConfig = config.slots[index];

                if (cellDeck.IsEmpty && slotConfig != null)
                {
                    Shape newShape;

                    // 1. 创建Shape
                    if (slotConfig.shape != null)
                    {
                        // 使用配置的ShapeTemplate
                        var shapeObject = PoolObject.GetObject(shapePrefab.gameObject);
                        newShape = shapeObject.GetComponent<Shape>();
                        newShape.UpdateShape(slotConfig.shape);
                    }
                    else
                    {
                        // 配置为null，使用随机生成
                        var shapeObject = PoolObject.GetObject(shapePrefab.gameObject);
                        newShape = itemFactory.CreateRandomShape(new HashSet<ShapeTemplate>(), shapeObject);
                    }

                    // 2. 应用颜色
                    if (slotConfig.color != null)
                    {
                        // 使用配置的颜色
                        newShape.UpdateColor(slotConfig.color);
                    }
                    else
                    {
                        // 配置为null，使用随机颜色
                        var colorTemplate = itemFactory?.GetColor();
                        if (colorTemplate != null)
                        {
                            newShape.UpdateColor(colorTemplate);
                        }
                        else
                        {
                            Debug.LogWarning($"[CellDeckManager] 无法获取颜色模板，形状将使用默认颜色");
                        }
                    }

                    // 3. 应用BonusItem
                    if (slotConfig.bonusItems != null && slotConfig.bonusItems.Count > 0)
                    {
                        ApplyBonusItems(newShape, slotConfig.bonusItems, index);
                    }

                    cellDeck.FillCell(newShape);
                }
            }

            Debug.Log($"[CellDeckManager] 使用初始配置填充第 {refreshCount + 1} 次刷新");

            // 3个shape生成完成后，统一处理金钱方块生成
            AddMoneyBlockToRandomShape();

            return true;
        }

        /// <summary>
        /// 使用随机逻辑填充方块（原有逻辑）
        /// </summary>
        private void FillCellDecksRandom()
        {
            // 收集当前的形状模板，用于去重
            var usedShapeTemplates = new HashSet<ShapeTemplate>();
            var shapes = GetShapes();
            foreach (var shape in shapes)
            {
                if (shape?.shapeTemplate != null)
                {
                    usedShapeTemplates.Add(shape.shapeTemplate);
                }
            }

            // 可放置形状计数器
            var fitShapesCount = 0;

            // 为每个槽位生成形状
            for (var index = 0; index < cellDecks.Length; index++)
            {
                var cellDeck = cellDecks[index];
                if (cellDeck.IsEmpty)
                {
                    // 从对象池获取形状对象
                    var shapeObject = PoolObject.GetObject(shapePrefab.gameObject);
                    Shape randomShape = null;

                    // 关键逻辑：如果还没有2个可放置的形状，且已到最后两个槽位
                    // 强制生成可放置的形状，确保游戏可以继续
                    if (fitShapesCount < 2 && index >= cellDecks.Length - 2)
                    {
                        randomShape = itemFactory.CreateRandomShapeFits(shapeObject);
                    }
                    else
                    {
                        // 否则生成随机形状（可能无法放置）
                        randomShape = itemFactory.CreateRandomShape(usedShapeTemplates, shapeObject);
                    }

                    // 检查形状是否可以放置
                    if (field.CanPlaceShape(randomShape))
                    {
                        fitShapesCount++;
                    }

                    // 填充到槽位
                    cellDeck.FillCell(randomShape);
                }
            }

            // 3个shape生成完成后，统一处理金钱方块生成
            AddMoneyBlockToRandomShape();
        }

        /// <summary>
        /// 使用指定的形状模板填充槽位
        /// 用于特定场景（如教学模式）
        /// </summary>
        /// <param name="shapes">形状模板数组</param>
        public void FillCellDecksWithShapes(ShapeTemplate[] shapes)
        {
            if (shapes == null || shapes.Length == 0)
            {
                return;
            }

            // 清空现有形状
            ClearCellDecks();

            // 填充指定的形状
            for (var index = 0; index < cellDecks.Length && index < shapes.Length; index++)
            {
                var cellDeck = cellDecks[index];
                if (cellDeck.IsEmpty)
                {
                    // 从对象池获取形状
                    var shapeObject = PoolObject.GetObject(shapePrefab.gameObject);
                    var shape = shapeObject.GetComponent<Shape>();

                    // 应用形状模板和颜色
                    var shapeTemplate = shapes[index];
                    shape.UpdateShape(shapeTemplate);

                    // 获取颜色模板并检查是否为null
                    var colorTemplate = itemFactory.GetColor();
                    if (colorTemplate != null)
                    {
                        shape.UpdateColor(colorTemplate);
                    }
                    else
                    {
                        Debug.LogWarning($"[CellDeckManager] 无法获取颜色模板，形状将使用默认颜色");
                    }

                    cellDeck.FillCell(shape);
                }
            }
        }

        /// <summary>
        /// 移除已使用的形状
        /// 将形状从槽位移除并返回对象池
        /// </summary>
        /// <param name="shape">要移除的形状</param>
        private void RemoveUsedShapes(Shape shape)
        {
            if (shape == null)
            {
                return;
            }

            foreach (var cellDeck in cellDecks)
            {
                if (cellDeck.shape == shape)
                {
                    cellDeck.FillCell(null);  // 清空槽位
                    PoolObject.Return(shape.gameObject);  // 返回对象池
                }
            }
        }

        /// <summary>
        /// 清空所有槽位
        /// </summary>
        public void ClearCellDecks()
        {
            foreach (var cellDeck in cellDecks)
            {
                cellDeck.ClearCell();
            }
        }

        /// <summary>
        /// 获取当前所有形状
        /// </summary>
        /// <returns>形状数组</returns>
        public Shape[] GetShapes()
        {
            var shapes = new List<Shape>();
            foreach (var deck in cellDecks)
            {
                if (deck.shape != null)
                {
                    shapes.Add(deck.shape);
                }
            }
            return shapes.ToArray();
        }

        /// <summary>
        /// 游戏失败后更新槽位
        /// 生成所有可放置的形状，让玩家可以继续
        /// </summary>
        public void UpdateCellDeckAfterFail()
        {
            foreach (var cellDeck in cellDecks)
            {
                cellDeck.ClearCell();
                // 强制生成可放置的形状
                cellDeck.FillCell(itemFactory.CreateRandomShapeFits(PoolObject.GetObject(shapePrefab.gameObject)));
            }
        }

        /// <summary>
        /// 关卡加载时的处理（实现ILevelLoadable接口）
        /// 用于关卡开始时的初始化
        /// </summary>
        /// <param name="level">关卡数据</param>
        public void OnLevelLoaded(Level level)
        {
            // 保存关卡引用，用于获取初始方块配置
            currentLevel = level;
            // 重置刷新计数器
            refreshCount = 0;

            // 注意：不在这里清空槽位，而是在 FillFitShapesOnly 中清空
            // 这样可以避免清空后到填充前的时间间隙导致游戏检测到失败

            if (!GameManager.Instance.IsTutorialMode())
            {
                // 延迟填充形状，等待场景完全加载
                StartCoroutine(DelayedFillFitShapesOnly());
            }
        }

        /// <summary>
        /// 延迟填充可放置的形状
        /// </summary>
        private IEnumerator DelayedFillFitShapesOnly()
        {
            // 等待0.2秒，确保场景初始化完成
            yield return new WaitForSeconds(0.2f);
            FillFitShapesOnly();
        }

        /// <summary>
        /// 只填充可放置的形状
        /// 用于游戏开始时，确保玩家有形状可用
        /// </summary>
        private void FillFitShapesOnly()
        {
            // 首先清空所有槽位，确保不会有残留方块
            // 这里清空是为了确保关卡切换时旧方块不会残留
            ClearCellDecks();

            // 然后尝试使用配置的初始方块
            if (TryFillFromConfiguration())
            {
                // 使用配置成功，增加刷新计数并返回
                refreshCount++;
                return;
            }

            // 配置不存在或已用完，使用原有随机逻辑

            // 如果所有槽位都空，清除已使用形状记录（允许重新使用）
            bool allEmpty = true;
            foreach (var deck in cellDecks)
            {
                if (!deck.IsEmpty)
                {
                    allEmpty = false;
                    break;
                }
            }

            if (allEmpty)
            {
                usedShapes.Clear();
            }

            for (var index = 0; index < cellDecks.Length; index++)
            {
                var cellDeck = cellDecks[index];
                cellDeck.ClearCell();

                // 尝试创建可放置的形状
                var shapeObject = PoolObject.GetObject(shapePrefab.gameObject);
                var shape = itemFactory.CreateRandomShapeFits(shapeObject, usedShapes);

                // 使用找到的形状
                if (shape != null)
                {
                    cellDeck.FillCell(shape);
                    if (shape.shapeTemplate != null)
                    {
                        usedShapes.Add(shape.shapeTemplate);  // 记录已使用
                    }
                }
                else
                {
                    // 如果找不到可放置的形状，退而使用随机形状
                    shapeObject = PoolObject.GetObject(shapePrefab.gameObject);
                    shape = itemFactory.CreateRandomShape(usedShapes, shapeObject);
                    cellDeck.FillCell(shape);
                    if (shape.shapeTemplate != null)
                    {
                        usedShapes.Add(shape.shapeTemplate);
                    }
                }
            }
        }

        /// <summary>
        /// 添加形状到空闲槽位
        /// 用于特殊情况下手动添加形状
        /// </summary>
        /// <param name="shapeTemplate">形状模板</param>
        public void AddShapeToFreeCell(ShapeTemplate shapeTemplate)
        {
            foreach (var cellDeck in cellDecks)
            {
                if (cellDeck.IsEmpty)
                {
                    // 创建并配置形状
                    var shapeObject = PoolObject.GetObject(shapePrefab.gameObject);
                    var shape = shapeObject.GetComponent<Shape>();
                    shape.UpdateShape(shapeTemplate);
                    shape.UpdateColor(itemFactory.GetColor());
                    cellDeck.FillCell(shape);
                    return;  // 只填充第一个空槽位
                }
            }
        }

        /// <summary>
        /// 刷新方块用于复活功能
        /// 清空所有槽位并生成新的方块，保证至少有一个可放置
        /// </summary>
        /// <param name="shapeCount">要生成的方块数量</param>
        /// <param name="guaranteePlaceable">是否保证至少一个可放置</param>
        public void RefreshShapesForRevive(int shapeCount, bool guaranteePlaceable = true)
        {
            // 清空所有槽位
            ClearCellDecks();

            // 清除已使用形状记录，允许重新使用所有形状
            usedShapes.Clear();

            // 确保数量不超过槽位数
            shapeCount = Mathf.Min(shapeCount, cellDecks.Length);

            // 记录已生成的可放置方块数量
            int placeableCount = 0;

            // 用于去重的集合
            var generatedTemplates = new HashSet<ShapeTemplate>();

            for (int i = 0; i < shapeCount; i++)
            {
                var cellDeck = cellDecks[i];
                Shape newShape = null;
                var shapeObject = PoolObject.GetObject(shapePrefab.gameObject);

                // 如果需要保证至少一个可放置，且还没有生成可放置的方块
                if (guaranteePlaceable && placeableCount == 0 && i == shapeCount - 1)
                {
                    // 最后一个位置强制生成可放置的方块
                    newShape = itemFactory.GeneratePlaceableShape(shapeObject, field, generatedTemplates);

                    // 如果生成失败，退而使用CreateRandomShapeFits
                    if (newShape == null)
                    {
                        newShape = itemFactory.CreateRandomShapeFits(shapeObject, generatedTemplates);
                    }

                    if (newShape != null && field.CanPlaceShape(newShape))
                    {
                        placeableCount++;
                    }
                }
                else
                {
                    // 优先尝试生成小型方块（如果配置中启用）
                    var settings = BlockPuzzleGameToolkit.Scripts.Settings.FailedPopupSettings.Instance;
                    bool preferSmall = settings != null && Random.Range(0f, 1f) < settings.smallShapePriority;

                    if (preferSmall)
                    {
                        // 尝试生成小型方块（1-3个格子）
                        newShape = itemFactory.CreateSmallShape(shapeObject, generatedTemplates);
                    }

                    // 如果小型方块生成失败，或不需要优先生成小型方块
                    if (newShape == null)
                    {
                        // 生成随机方块
                        newShape = itemFactory.CreateRandomShape(generatedTemplates, shapeObject);
                    }

                    // 检查是否可放置
                    if (newShape != null && field.CanPlaceShape(newShape))
                    {
                        placeableCount++;
                    }
                }

                // 填充到槽位
                if (newShape != null)
                {
                    cellDeck.FillCell(newShape);

                    // 记录已使用的模板
                    if (newShape.shapeTemplate != null)
                    {
                        generatedTemplates.Add(newShape.shapeTemplate);
                        usedShapes.Add(newShape.shapeTemplate);
                    }
                }
                else
                {
                    // 如果生成失败，返回对象到池
                    PoolObject.Return(shapeObject);
                }
            }

            Debug.Log($"[CellDeckManager] 复活刷新完成：生成了 {shapeCount} 个方块，其中 {placeableCount} 个可放置");
        }

        /// <summary>
        /// 为单个Shape应用BonusItem配置
        /// </summary>
        /// <param name="shape">目标形状</param>
        /// <param name="bonusConfigs">BonusItem配置列表</param>
        /// <param name="slotIndex">槽位索引（用于日志）</param>
        private void ApplyBonusItems(Shape shape, List<BonusItemConfig> bonusConfigs, int slotIndex)
        {
            // 构建BonusItem池
            var bonusPool = new List<BonusItemTemplate>();
            foreach (var config in bonusConfigs)
            {
                if (config.bonusItem != null && config.count > 0)
                {
                    for (int i = 0; i < config.count; i++)
                    {
                        bonusPool.Add(config.bonusItem);
                    }
                }
            }

            if (bonusPool.Count == 0)
            {
                return;
            }

            // 获取Shape的所有激活Item
            var activeItems = shape.GetActiveItems();
            int itemCount = activeItems.Count;

            // 限制BonusItem数量不超过Item数量
            int maxBonusCount = Mathf.Min(bonusPool.Count, itemCount);

            // 警告：如果配置的BonusItem超出限制
            if (bonusPool.Count > itemCount)
            {
                Debug.LogWarning($"[CellDeckManager] 槽位{slotIndex + 1}配置了{bonusPool.Count}个BonusItem，" +
                                $"但Shape只有{itemCount}个Item，已限制为{maxBonusCount}个");
            }

            // 随机选择Item并分配BonusItem
            var availableItems = new List<Item>(activeItems);
            for (int i = 0; i < maxBonusCount; i++)
            {
                if (availableItems.Count == 0)
                {
                    break;
                }

                // 随机选择一个未分配的Item
                int randomIndex = Random.Range(0, availableItems.Count);
                var selectedItem = availableItems[randomIndex];

                // 分配BonusItem
                selectedItem.SetBonus(bonusPool[i]);

                // 从可用列表中移除（避免重复分配）
                availableItems.RemoveAt(randomIndex);
            }

            Debug.Log($"[CellDeckManager] 槽位{slotIndex + 1}：为Shape分配了{maxBonusCount}个BonusItem");
        }

        /// <summary>
        /// 在当前3个shape中随机选择一个，添加金钱方块
        /// 在生成完3个shape后统一调用
        /// </summary>
        private void AddMoneyBlockToRandomShape()
        {
            // 获取MoneyBlockManager
            var moneyBlockManager = MoneyBlockManager.Instance;
            if (moneyBlockManager == null)
                return;

            // 检查是否满足刷新条件（Manager内部判断）
            var moneySpawner = moneyBlockManager.GetSpawner();
            if (moneySpawner == null)
                return;

            // 收集所有非空的shape
            var availableShapes = new List<Shape>();
            foreach (var deck in cellDecks)
            {
                if (!deck.IsEmpty && deck.shape != null)
                {
                    availableShapes.Add(deck.shape);
                }
            }

            if (availableShapes.Count == 0)
            {
                Debug.LogWarning("[CellDeckManager] 没有可用的shape，跳过金钱方块生成");
                return;
            }

            // 打乱shape列表，确保随机性
            for (int i = availableShapes.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                var temp = availableShapes[i];
                availableShapes[i] = availableShapes[j];
                availableShapes[j] = temp;
            }

            // 遍历shape列表，找到第一个有合适格子的shape
            foreach (var shape in availableShapes)
            {
                // 获取所有激活的Item
                var activeItems = shape.GetActiveItems();
                if (activeItems == null || activeItems.Count == 0)
                    continue;

                // 过滤掉已有宝石和其他道具的Item
                var eligibleItems = new List<Item>();
                foreach (var item in activeItems)
                {
                    // 检查是否已有宝石或其他道具
                    if (!item.HasBonusItem())
                    {
                        eligibleItems.Add(item);
                    }
                }

                if (eligibleItems.Count == 0)
                {
                    // 这个shape所有格子都有道具，尝试下一个shape
                    continue;
                }

                // 随机选择一个Item
                int randomIndex = Random.Range(0, eligibleItems.Count);
                var selectedItem = eligibleItems[randomIndex];

                // 添加金钱图标
                moneySpawner.AddMoneyIconToCell(selectedItem.gameObject);

                // 生成成功，通知Manager更新计数
                moneyBlockManager.NotifyMoneyBlockSpawned();

                Debug.Log($"[CellDeckManager] 在形状上生成金钱方块，位置: 行{selectedItem.GetPosition().y}, 列{selectedItem.GetPosition().x}");

                // 成功生成一个金钱方块后立即返回，确保只生成一个
                return;
            }

            // 如果遍历完所有shape都没有合适的格子，设置pending标志，下次继续尝试
            Debug.LogWarning("[CellDeckManager] 所有shape的格子都已有道具，无法生成金钱方块，设置pending标志");
            moneyBlockManager.SetPendingSpawn();
        }
    }
}