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

using System.Collections.Generic;
using System.Linq;
using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.Gameplay.Pool;
using BlockPuzzleGameToolkit.Scripts.LevelsData;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Gameplay
{
    /// <summary>
    /// ItemFactory - 物品工厂类
    /// 负责生成游戏中的形状和颜色
    /// 控制难度递进系统，根据分数或关卡解锁新形状
    /// </summary>
    public class ItemFactory : MonoBehaviour
    {
        // ========== 静态缓存 ==========
        /// <summary>
        /// 经典模式处理器缓存
        /// </summary>
        private static ClassicModeHandler classicModeHandlerCached;

        /// <summary>
        /// 计时模式处理器缓存
        /// </summary>
        private static TimedModeHandler timeModeHandlerCached;

        // ========== 资源数组 ==========
        /// <summary>
        /// 所有可用的形状模板数组
        /// </summary>
        private ShapeTemplate[] shapes;

        /// <summary>
        /// 所有可用的物品（颜色）模板数组
        /// </summary>
        protected ItemTemplate[] items;

        // ========== 游戏数据 ==========
        /// <summary>
        /// 当前关卡数据
        /// </summary>
        private Level level;

        /// <summary>
        /// 预测的目标字典（用于奖励道具生成）
        /// </summary>
        private Dictionary<BonusItemTemplate, int> predictedTargets;

        /// <summary>
        /// 是否为单色模式
        /// </summary>
        public bool _oneColorMode;

        /// <summary>
        /// 单色模式下使用的颜色索引
        /// </summary>
        protected int _oneColor;

        // ========== 管理器引用 ==========
        [SerializeField]
        private FieldManager field;

        [SerializeField]
        private CellDeckManager cellDeck;

        [SerializeField]
        private TargetManager targetManager;

        [SerializeField]
        private LevelManager levelManager;

        /// <summary>
        /// Unity生命周期 - 初始化
        /// 从Resources文件夹加载所有形状和物品模板
        /// </summary>
        protected virtual void Awake()
        {
            // 从Resources加载所有形状模板
            shapes = Resources.LoadAll<ShapeTemplate>("Shapes");
            Debug.Log($"[ItemFactory] 加载了 {shapes?.Length ?? 0} 个形状模板");

            // 从Resources加载所有物品（颜色）模板
            items = Resources.LoadAll<ItemTemplate>("Items");
            Debug.Log($"[ItemFactory] 加载了 {items?.Length ?? 0} 个颜色模板");

            // 如果没有加载到颜色模板，输出警告
            if (items == null || items.Length == 0)
            {
                Debug.LogError("[ItemFactory] 警告：没有在Resources/Items文件夹中找到ItemTemplate资源！");
            }
        }

        /// <summary>
        /// 【方案C：双策略防重复】根据策略获取随机形状
        /// 优先尝试返回不重复的方块，达到上限后根据策略决定是否允许重复
        /// </summary>
        /// <param name="usedShapeTemplates">已使用的形状模板集合</param>
        /// <param name="allowDuplicateIfNecessary">是否允许在无法找到不重复方块时返回重复方块（默认true）</param>
        /// <returns>形状模板，如果不允许重复且找不到则返回null</returns>
        private ShapeTemplate GetRandomShapeWithPolicy(
            HashSet<ShapeTemplate> usedShapeTemplates,
            bool allowDuplicateIfNecessary = true)
        {
            // 如果没有已使用集合或集合为空，直接返回随机方块
            if (usedShapeTemplates == null || usedShapeTemplates.Count == 0)
            {
                return GetRandomShape();
            }

            ShapeTemplate shapeTemplate = null;
            const int maxAttempts = 10;  // 优化：降低尝试次数到10次（性能优化）

            // 尝试找到不重复的方块
            for (int i = 0; i < maxAttempts; i++)
            {
                shapeTemplate = GetRandomShape();

                // 找到不重复的方块，立即返回
                if (!usedShapeTemplates.Contains(shapeTemplate))
                {
                    usedShapeTemplates.Add(shapeTemplate);
                    Debug.Log($"[ItemFactory] ✅ 成功找到不重复方块: {shapeTemplate?.name} (尝试{i + 1}次)");
                    return shapeTemplate;
                }
            }

            // 达到上限后的处理策略
            if (allowDuplicateIfNecessary)
            {
                // 🔄 宽松模式：允许重复（Layer4反死局优先级更高）
                Debug.Log($"[ItemFactory] 🔄 无法找到不重复方块（已尝试{maxAttempts}次），允许重复方块: {shapeTemplate?.name}。" +
                          $"原因：可能触发了Layer4反死局或可用方块数量不足（已使用={usedShapeTemplates.Count}，总数={shapes?.Length}）");
                return shapeTemplate;
            }
            else
            {
                // ⚠️ 严格模式：返回null，让调用方处理
                Debug.LogWarning($"[ItemFactory] ⚠️ 无法找到不重复方块（已尝试{maxAttempts}次），严格模式返回null。" +
                                 $"已使用={usedShapeTemplates.Count}，总数={shapes?.Length}");
                return null;
            }
        }

        /// <summary>
        /// 获取不重复的形状模板（兼容性方法）
        /// 内部调用GetRandomShapeWithPolicy，默认允许重复
        /// </summary>
        /// <param name="usedShapeTemplates">已使用的形状模板集合</param>
        /// <returns>不重复的形状模板</returns>
        private ShapeTemplate GetNonRepeatedShapeTemplate(HashSet<ShapeTemplate> usedShapeTemplates)
        {
            // 调用新的双策略方法，默认允许重复（Layer4优先级更高）
            return GetRandomShapeWithPolicy(usedShapeTemplates, allowDuplicateIfNecessary: true);
        }

        /// <summary>
        /// 获取随机形状模板 - 核心难度递进逻辑
        /// 优先使用 DynamicDifficultyController（四层算法）
        /// 如果不可用，则使用旧的权重系统作为 fallback
        /// </summary>
        /// <returns>随机选择的形状模板</returns>
        private ShapeTemplate GetRandomShape()
        {
            ShapeTemplate shapeTemplate = null;

            // ⭐ 优先使用动态难度控制器（四层算法）
            var difficultyController = global::GameCore.DifficultySystem.DynamicDifficultyController.Instance;
            var currentLevel = levelManager?.GetCurrentLevel();

            if (difficultyController != null && currentLevel != null && field != null)
            {
                Debug.Log($"[ItemFactory] ✅ 调用四层算法 | 关卡={currentLevel.name}, ShapeWeightLevel={currentLevel.shapeWeightLevel}, UseCustom={currentLevel.useCustomProbabilities}");

                // 使用四层算法选择方块
                // 传入 null 让 DynamicDifficultyController 使用内部维护的 currentGameState
                var selectedShapeTemplate = difficultyController.SelectNextShape(field, currentLevel, null);
                if (selectedShapeTemplate != null)
                {
                    Debug.Log($"[ItemFactory] ✅ 四层算法返回: {selectedShapeTemplate.name} (Category={selectedShapeTemplate.category}, CellCount={selectedShapeTemplate.cellCount})");
                    return selectedShapeTemplate;
                }
                else
                {
                    Debug.LogWarning("[ItemFactory] ⚠️ 四层算法返回null，使用fallback");
                }
            }
            else
            {
                Debug.LogWarning($"[ItemFactory] ⚠️ 四层算法不可用 | Controller={difficultyController != null}, Level={currentLevel != null}, Field={field != null}");
            }

            // ⚠️ Fallback: 如果动态难度控制器不可用，使用随机选择
            Debug.Log("[ItemFactory] 使用Fallback随机选择");

            // 根据游戏模式筛选可用形状
            var shapesToConsider = levelManager.GetGameMode() == EGameMode.Adventure
                ? shapes.Where(shape => shape.spawnFromLevel <= levelManager.currentLevel).ToArray()  // 冒险模式：根据关卡解锁
                : shapes.Where(shape => shape.scoreForSpawn <= GetClassicScore()).ToArray();          // 经典/计时模式：根据分数解锁

            // 随机选择形状
            if (shapesToConsider.Length > 0)
            {
                shapeTemplate = shapesToConsider[Random.Range(0, shapesToConsider.Length)];
            }

            Debug.Log($"[ItemFactory] Fallback返回: {shapeTemplate?.name ?? "null"}");
            return shapeTemplate;
        }

        /// <summary>
        /// 获取经典/计时模式的当前分数
        /// </summary>
        /// <returns>当前分数</returns>
        private static int GetClassicScore()
        {
            // 尝试获取经典模式分数
            classicModeHandlerCached ??= FindObjectOfType<ClassicModeHandler>();
            var classicHandler = classicModeHandlerCached;
            if (classicHandler != null)
                return classicHandler.score;

            // 尝试获取计时模式分数
            timeModeHandlerCached ??= FindObjectOfType<TimedModeHandler>();
            var timedHandler = timeModeHandlerCached;
            if (timedHandler != null)
                return timedHandler.score;

            return 0;
        }

        /// <summary>
        /// 创建随机形状 - 公开接口
        /// 生成一个随机形状，可能包含奖励道具
        /// </summary>
        /// <param name="usedShapeTemplates">已使用的形状模板集合</param>
        /// <param name="shapeObject">形状GameObject</param>
        /// <returns>配置好的形状组件</returns>
        public Shape CreateRandomShape(HashSet<ShapeTemplate> usedShapeTemplates, GameObject shapeObject)
        {
            var shape = shapeObject.GetComponent<Shape>();
            shape.transform.localScale = Vector3.one;

            // 设置不重复的形状模板
            shape.UpdateShape(GetNonRepeatedShapeTemplate(usedShapeTemplates));

            // 如果有奖励目标，尝试生成奖励道具
            var currentTargets = targetManager.GetTargets();
            if (currentTargets != null && currentTargets.Any(i => i.targetScriptable.bonusItem != null))
            {
                GenerateBonus(shape, currentTargets);
            }

            // 设置颜色
            shape.UpdateColor(GetColor());

            return shape;
        }

        /// <summary>
        /// 创建可放置的随机形状 - 确保游戏可玩性
        /// 尝试找到一个可以放置到当前棋盘的形状
        /// </summary>
        /// <param name="shapeObject">形状GameObject</param>
        /// <param name="usedShapes">已使用的形状集合（可选）</param>
        /// <returns>可放置的形状，如果没找到返回null</returns>
        public Shape CreateRandomShapeFits(GameObject shapeObject, HashSet<ShapeTemplate> usedShapes = null)
        {
            Debug.Log($"[ItemFactory] 🎯 CreateRandomShapeFits 被调用（保证可放置）");

            var shape = shapeObject.GetComponent<Shape>();

            // ⭐ 优先使用动态难度控制器（四层算法）
            var difficultyController = global::GameCore.DifficultySystem.DynamicDifficultyController.Instance;
            var currentLevel = levelManager?.GetCurrentLevel();

            if (difficultyController != null && currentLevel != null && field != null)
            {
                Debug.Log($"[ItemFactory] ✅ CreateRandomShapeFits 调用四层算法");

                // 尝试多次生成，直到找到可放置的方块
                const int maxAttempts = 10;
                for (int attempt = 0; attempt < maxAttempts; attempt++)
                {
                    var selectedShapeTemplate = difficultyController.SelectNextShape(field, currentLevel, null);
                    if (selectedShapeTemplate != null && (usedShapes == null || !usedShapes.Contains(selectedShapeTemplate)))
                    {
                        shape.UpdateShape(selectedShapeTemplate);
                        shape.UpdateColor(GetColor());

                        // 检查形状是否可以放置
                        if (field.CanPlaceShape(shape))
                        {
                            Debug.Log($"[ItemFactory] ✅ CreateRandomShapeFits 四层算法找到可放置方块: {selectedShapeTemplate.name} (尝试次数={attempt + 1})");

                            // 生成奖励道具（如果需要）
                            var currentTargets = targetManager.GetTargets();
                            if (currentTargets != null && currentTargets.Any(i => i.targetScriptable.bonusItem != null))
                            {
                                GenerateBonus(shape, currentTargets);
                            }
                            return shape;
                        }
                    }
                }

                Debug.LogWarning($"[ItemFactory] ⚠️ CreateRandomShapeFits 四层算法尝试{maxAttempts}次未找到可放置方块，使用fallback");
            }

            // ⚠️ Fallback: 旧的遍历所有方块的方式
            Debug.Log("[ItemFactory] CreateRandomShapeFits 使用旧遍历方式（Fallback）");

            // 筛选符合条件的形状（排除已使用的）
            var eligibleShapes = levelManager.GetGameMode() == EGameMode.Adventure
                ? shapes.Where(s => s.spawnFromLevel <= levelManager.currentLevel && (usedShapes == null || !usedShapes.Contains(s))).ToArray()
                : shapes.Where(s => s.scoreForSpawn <= GetClassicScore() && (usedShapes == null || !usedShapes.Contains(s))).ToArray();

            // 如果没有未使用的形状，允许重复使用
            if (eligibleShapes.Length == 0)
            {
                eligibleShapes = levelManager.GetGameMode() == EGameMode.Adventure
                    ? shapes.Where(s => s.spawnFromLevel <= levelManager.currentLevel).ToArray()
                    : shapes.Where(s => s.scoreForSpawn <= GetClassicScore()).ToArray();
            }

            // 随机打乱形状顺序
            var shapes_random = eligibleShapes.OrderBy(x => Random.value).ToList();

            // 尝试每个形状，直到找到可放置的
            foreach (var shapeTemplate in shapes_random)
            {
                shape.UpdateShape(shapeTemplate);
                shape.UpdateColor(GetColor());

                // 检查形状是否可以放置
                if (field.CanPlaceShape(shape))
                {
                    Debug.Log($"[ItemFactory] Fallback找到可放置方块: {shapeTemplate?.name}");

                    // 生成奖励道具（如果需要）
                    var currentTargets = targetManager.GetTargets();
                    if (currentTargets != null && currentTargets.Any(i => i.targetScriptable.bonusItem != null))
                    {
                        GenerateBonus(shape, currentTargets);
                    }
                    return shape;
                }
            }

            // 没有找到可放置的形状，返回对象到池并返回null
            Debug.LogWarning("[ItemFactory] ⚠️ CreateRandomShapeFits 未找到任何可放置的方块，返回null");
            PoolObject.Return(shapeObject);
            return null;
        }

        /// <summary>
        /// 获取颜色模板
        /// 根据是否为单色模式返回相应的颜色
        /// </summary>
        /// <returns>颜色模板</returns>
        public ItemTemplate GetColor()
        {
            // 检查items数组是否已初始化且有内容
            if (items == null || items.Length == 0)
            {
                Debug.LogWarning("[ItemFactory] GetColor: items数组为空，尝试重新加载");
                items = Resources.LoadAll<ItemTemplate>("Items");

                // 如果还是为空，返回null或创建默认颜色
                if (items == null || items.Length == 0)
                {
                    Debug.LogError("[ItemFactory] GetColor: 无法加载ItemTemplate，请确保Resources/Items文件夹中有ItemTemplate资源");
                    return null;
                }
            }

            // 处理单色模式
            if (_oneColorMode)
            {
                // 确保_oneColor索引在有效范围内
                if (_oneColor < 0 || _oneColor >= items.Length)
                {
                    Debug.LogWarning($"[ItemFactory] GetColor: 单色模式索引 {_oneColor} 超出范围，使用索引0");
                    _oneColor = 0;
                }
                return items[_oneColor];
            }

            // 非单色模式：随机选择颜色（跳过索引0）
            // 确保至少有2个元素（索引0和索引1）
            if (items.Length <= 1)
            {
                Debug.LogWarning("[ItemFactory] GetColor: items数组长度不足，返回第一个元素");
                return items[0];
            }

            return items[Random.Range(1, items.Length)];
        }

        /// <summary>
        /// 获取单色模式的颜色
        /// </summary>
        /// <returns>单色模式使用的颜色模板</returns>
        public ItemTemplate GetOneColor()
        {
            // 检查items数组是否已初始化且有内容
            if (items == null || items.Length == 0)
            {
                Debug.LogWarning("[ItemFactory] GetOneColor: items数组为空，尝试重新加载");
                items = Resources.LoadAll<ItemTemplate>("Items");

                if (items == null || items.Length == 0)
                {
                    Debug.LogError("[ItemFactory] GetOneColor: 无法加载ItemTemplate");
                    return null;
                }
            }

            // 确保_oneColor索引在有效范围内
            if (_oneColor < 0 || _oneColor >= items.Length)
            {
                Debug.LogWarning($"[ItemFactory] GetOneColor: 索引 {_oneColor} 超出范围，使用索引0");
                _oneColor = 0;
            }

            return items[_oneColor];
        }

        /// <summary>
        /// 生成奖励道具 - 智能分配系统
        /// 只要还没收集完就继续生成，不做任何限制
        /// </summary>
        /// <param name="shapeObject">要添加奖励的形状</param>
        /// <param name="targets">当前的目标列表</param>
        private void GenerateBonus(Shape shapeObject, List<Target> targets)
        {
            // 构建还需要收集的bonus类型列表
            var availableBonusTypes = new List<BonusItemTemplate>();
            foreach (var target in targets)
            {
                // 只要还没收集完（amount > 0）就可以生成
                if (target.targetScriptable.bonusItem != null && target.amount > 0)
                {
                    availableBonusTypes.Add(target.targetScriptable.bonusItem);
                }
            }

            // 如果没有需要收集的bonus目标，直接返回
            if (availableBonusTypes.Count == 0)
            {
                return;
            }

            // 随机选择一个bonus类型，33%概率生成
            if (Random.Range(0, 3) == 0)
            {
                // 随机选择一个bonus类型
                var selectedBonus = availableBonusTypes[Random.Range(0, availableBonusTypes.Count)];
                // 获取该bonus的目标数量（用于显示）
                var targetAmount = targets.FirstOrDefault(t => t.targetScriptable.bonusItem == selectedBonus)?.amount ?? 1;
                shapeObject.SetBonus(selectedBonus, targetAmount);
            }
        }

        /// <summary>
        /// 生成保证可放置的形状
        /// 专门为复活功能设计，确保至少有一个方块可以放置
        /// </summary>
        /// <param name="shapeObject">形状GameObject</param>
        /// <param name="field">棋盘管理器</param>
        /// <param name="usedShapes">已使用的形状集合</param>
        /// <returns>可放置的形状，如果失败返回null</returns>
        public Shape GeneratePlaceableShape(GameObject shapeObject, FieldManager field, HashSet<ShapeTemplate> usedShapes = null)
        {
            if (field == null)
            {
                Debug.LogWarning("[ItemFactory] GeneratePlaceableShape: field is null");
                return CreateRandomShapeFits(shapeObject, usedShapes);
            }

            var shape = shapeObject.GetComponent<Shape>();

            // 获取符合条件的形状模板
            var eligibleShapes = levelManager.GetGameMode() == EGameMode.Adventure
                ? shapes.Where(s => s.spawnFromLevel <= levelManager.currentLevel).ToArray()
                : shapes.Where(s => s.scoreForSpawn <= GetClassicScore()).ToArray();

            // 优先选择小型形状（更容易放置）
            var smallShapes = eligibleShapes.Where(s => s.cells != null && s.cells.Count(c => c) <= 3).ToArray();
            if (smallShapes.Length > 0)
            {
                // 随机尝试小型形状
                var shuffledSmall = smallShapes.OrderBy(x => Random.value).ToArray();
                foreach (var template in shuffledSmall)
                {
                    if (usedShapes != null && usedShapes.Contains(template))
                        continue;

                    shape.UpdateShape(template);
                    shape.UpdateColor(GetColor());

                    if (field.CanPlaceShape(shape))
                    {
                        // 生成奖励道具（如果需要）
                        var currentTargets = targetManager.GetTargets();
                        if (currentTargets != null && currentTargets.Any(i => i.targetScriptable.bonusItem != null))
                        {
                            GenerateBonus(shape, currentTargets);
                        }
                        return shape;
                    }
                }
            }

            // 如果小型形状都不能放置，尝试所有形状
            var shuffledAll = eligibleShapes.OrderBy(x => Random.value).ToArray();
            foreach (var template in shuffledAll)
            {
                if (usedShapes != null && usedShapes.Contains(template))
                    continue;

                shape.UpdateShape(template);
                shape.UpdateColor(GetColor());

                if (field.CanPlaceShape(shape))
                {
                    // 生成奖励道具（如果需要）
                    var currentTargets = targetManager.GetTargets();
                    if (currentTargets != null && currentTargets.Any(i => i.targetScriptable.bonusItem != null))
                    {
                        GenerateBonus(shape, currentTargets);
                    }
                    return shape;
                }
            }

            // 如果还是没找到，使用默认方法
            return CreateRandomShapeFits(shapeObject, usedShapes);
        }

        /// <summary>
        /// 创建小型形状（1-3个格子）
        /// 用于复活时优先生成容易放置的形状
        /// </summary>
        /// <param name="shapeObject">形状GameObject</param>
        /// <param name="usedShapes">已使用的形状集合</param>
        /// <returns>小型形状，如果失败返回null</returns>
        public Shape CreateSmallShape(GameObject shapeObject, HashSet<ShapeTemplate> usedShapes = null)
        {
            var shape = shapeObject.GetComponent<Shape>();

            // 获取符合条件的形状模板
            var eligibleShapes = levelManager.GetGameMode() == EGameMode.Adventure
                ? shapes.Where(s => s.spawnFromLevel <= levelManager.currentLevel).ToArray()
                : shapes.Where(s => s.scoreForSpawn <= GetClassicScore()).ToArray();

            // 筛选小型形状（1-3个格子）
            var smallShapes = eligibleShapes.Where(s =>
                s.cells != null &&
                s.cells.Count(c => c) <= 3 &&
                (usedShapes == null || !usedShapes.Contains(s))
            ).ToArray();

            if (smallShapes.Length == 0)
            {
                // 如果没有未使用的小型形状，返回null
                return null;
            }

            // 随机选择小型形状
            var randomTemplate = smallShapes[Random.Range(0, smallShapes.Length)];
            shape.UpdateShape(randomTemplate);

            // 设置颜色
            shape.UpdateColor(GetColor());

            // 生成奖励道具（如果需要）
            var currentTargets = targetManager.GetTargets();
            if (currentTargets != null && currentTargets.Any(i => i.targetScriptable.bonusItem != null))
            {
                GenerateBonus(shape, currentTargets);
            }

            return shape;
        }
    }
}