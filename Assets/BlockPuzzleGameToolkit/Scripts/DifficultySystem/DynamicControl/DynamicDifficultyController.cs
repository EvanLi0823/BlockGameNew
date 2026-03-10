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
using UnityEngine;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using BlockPuzzleGameToolkit.Scripts.Gameplay;
using BlockPuzzleGameToolkit.Scripts.LevelsData;
using BlockPuzzleGameToolkit.Scripts.Enums;

namespace GameCore.DifficultySystem
{
    /// <summary>
    /// 动态难度控制器（四层算法集成）
    /// 整合四层算法，根据游戏状态动态选择合适的方块
    /// </summary>
    public class DynamicDifficultyController : SingletonBehaviour<DynamicDifficultyController>
    {
        [Header("Layer References")]
        [Tooltip("第一层：阈值基线算法")]
        public ThresholdBasedDifficulty layer1;

        [Tooltip("第二层：概率动态调整")]
        public ProbabilityAdjuster layer2 = new ProbabilityAdjuster();

        [Tooltip("第三层：棋盘空间分析")]
        public BoardSpaceAnalyzer layer3 = new BoardSpaceAnalyzer();

        [Tooltip("第四层：反死局生成")]
        public AntiDeadlockGenerator layer4 = new AntiDeadlockGenerator();

        [Header("Debug Info")]
        [Tooltip("显示调试信息")]
        public bool showDebugInfo = false;

        [Tooltip("最近选择的方块")]
        public ShapeTemplate lastSelectedShape;

        [Tooltip("最近使用的概率")]
        public CategoryProbabilities lastProbabilities;

        private GameState currentGameState = new GameState();
        private List<ShapeTemplate> availableShapes = new List<ShapeTemplate>();
        private LevelManager levelManager;
        private DifficultyWeights difficultyConfig;

        #region Log Methods

        /// <summary>
        /// 统一的日志输出方法（普通日志）
        /// </summary>
        private void Log(string message)
        {
            if (showDebugInfo)
            {
                Debug.Log(message);
            }
        }

        /// <summary>
        /// 统一的日志输出方法（警告）
        /// </summary>
        private void LogWarning(string message)
        {
            if (showDebugInfo)
            {
                Debug.LogWarning(message);
            }
        }

        /// <summary>
        /// 统一的日志输出方法（错误）
        /// </summary>
        private void LogError(string message)
        {
            if (showDebugInfo)
            {
                Debug.LogError(message);
            }
        }

        #endregion

        /// <summary>
        /// Unity生命周期 - 启用时订阅事件
        /// </summary>
        private void OnEnable()
        {
            // 订阅关卡级事件
            EventManager.GetEvent(EGameEvent.LevelStarted).Subscribe(OnLevelStarted);
            EventManager.GetEvent(EGameEvent.LevelCompleted).Subscribe(OnLevelCompleted);
            EventManager.GetEvent(EGameEvent.LevelFailed).Subscribe(OnLevelFailed);
        }

        /// <summary>
        /// Unity生命周期 - 禁用时取消订阅
        /// </summary>
        private void OnDisable()
        {
            EventManager.GetEvent(EGameEvent.LevelStarted).Unsubscribe(OnLevelStarted);
            EventManager.GetEvent(EGameEvent.LevelCompleted).Unsubscribe(OnLevelCompleted);
            EventManager.GetEvent(EGameEvent.LevelFailed).Unsubscribe(OnLevelFailed);
        }

        /// <summary>
        /// 初始化方法
        /// </summary>
        public override void OnInit()
        {
            base.OnInit();

            Log("🔧 [DynamicDifficultyController] OnInit() 开始执行");

            // 确保layer1被正确加载
            if (layer1 == null)
            {
                LogWarning("DynamicDifficultyController: layer1 (ThresholdBasedDifficulty) 未设置，尝试从Resources加载");
                layer1 = Resources.Load<ThresholdBasedDifficulty>("Settings/ThresholdBasedDifficulty");

                if (layer1 == null)
                {
                    LogError("DynamicDifficultyController: 无法加载ThresholdBasedDifficulty配置文件 (路径: Resources/Settings/ThresholdBasedDifficulty.asset)");
                }
                else
                {
                    Log($"✅ [DynamicDifficultyController] 成功加载 layer1: {layer1.name}");
                }
            }
            else
            {
                Log($"✅ [DynamicDifficultyController] layer1 已在Inspector中设置: {layer1.name}");
            }

            // 加载DifficultyWeights配置
            difficultyConfig = Resources.Load<DifficultyWeights>("Settings/DifficultyWeights");

            if (difficultyConfig == null)
            {
                LogError("[DynamicDifficultyController] 无法加载DifficultyWeights配置文件 (路径: Resources/Settings/DifficultyWeights.asset)");
            }
            else
            {
                Log($"[DynamicDifficultyController] 已加载配置 | N={difficultyConfig.firstIncreaseThreshold}, Y={difficultyConfig.subsequentIncreaseInterval}, M={difficultyConfig.decreaseFailureThreshold}");

                // 初始化Layer2
                if (layer2 != null)
                {
                    layer2.Initialize(difficultyConfig);
                }
            }

            // 自动加载所有可用形状
            if (availableShapes.Count == 0)
            {
                Log("🔍 [DynamicDifficultyController] 尝试从 Resources/Shapes 加载形状模板...");
                var shapes = Resources.LoadAll<ShapeTemplate>("Shapes");
                if (shapes != null && shapes.Length > 0)
                {
                    availableShapes = new List<ShapeTemplate>(shapes);
                    Log($"✅ [DynamicDifficultyController] 已加载 {availableShapes.Count} 个形状模板");
                }
                else
                {
                    LogWarning("⚠️ [DynamicDifficultyController] 未找到任何形状模板资源 (路径: Resources/Shapes/*.asset)");
                }
            }
            else
            {
                Log($"✅ [DynamicDifficultyController] 形状模板已加载: {availableShapes.Count} 个");
            }

            // 获取LevelManager引用
            if (levelManager == null)
            {
                levelManager = FindObjectOfType<LevelManager>();
                if (levelManager != null)
                {
                    Log("✅ [DynamicDifficultyController] 已找到LevelManager引用");
                }
                else
                {
                    LogWarning("⚠️ [DynamicDifficultyController] 未找到LevelManager，spawnFromLevel过滤将无法工作");
                }
            }

            // 重置所有层的状态
            ResetAllLayers();

            Log($"🔧 [DynamicDifficultyController] OnInit() 完成 | layer1={(layer1 != null ? "✅" : "❌")}, shapes={availableShapes.Count}");
        }

        /// <summary>
        /// 选择下一个方块（主方法）
        /// </summary>
        /// <param name="field">棋盘管理器</param>
        /// <param name="level">当前关卡</param>
        /// <param name="state">游戏状态</param>
        /// <returns>选中的方块模板</returns>
        public ShapeTemplate SelectNextShape(FieldManager field, Level level, GameState state)
        {
            Log($"[DynamicDifficultyController] 🎯 开始选择方块 | 关卡={level?.name}");

            if (field == null || level == null)
            {
                LogWarning("DynamicDifficultyController: FieldManager或Level为null");
                return null;
            }

            // ⭐ 根据游戏模式和关卡进度过滤可用形状
            List<ShapeTemplate> filteredShapes = availableShapes;

            // 动态获取LevelManager（防止初始化顺序问题）
            if (levelManager == null)
            {
                levelManager = FindObjectOfType<LevelManager>();
            }

            if (levelManager != null)
            {
                var gameMode = levelManager.GetGameMode();
                var currentLevelNumber = levelManager.currentLevel;

                if (gameMode == EGameMode.Adventure)
                {
                    // 冒险模式：根据关卡号过滤
                    Log($"[DynamicDifficultyController] 🔍 过滤前 | 游戏模式=冒险, 当前关卡={currentLevelNumber}, 总形状数={availableShapes.Count}");
                    filteredShapes = availableShapes.FindAll(s => s != null && s.spawnFromLevel <= currentLevelNumber);
                    Log($"[DynamicDifficultyController] 🔒 冒险模式过滤完成 | 可用形状数={filteredShapes.Count}/{availableShapes.Count}");

                    // 输出被过滤掉的形状
                    var filtered_out = availableShapes.Where(s => s != null && s.spawnFromLevel > currentLevelNumber).ToList();
                    if (filtered_out.Count > 0)
                    {
                        Log($"[DynamicDifficultyController] ⛔ 被过滤的形状({filtered_out.Count}个): {string.Join(", ", filtered_out.Select(s => $"{s.name}(需要关卡{s.spawnFromLevel})"))}");
                    }
                }
                else
                {
                    // 经典/计时模式：根据分数过滤
                    var currentScore = GetClassicScore();
                    filteredShapes = availableShapes.FindAll(s => s != null && s.scoreForSpawn <= currentScore);
                    Log($"[DynamicDifficultyController] 🔒 经典/计时模式过滤 | 分数={currentScore}, 可用形状数={filteredShapes.Count}/{availableShapes.Count}");
                }
            }
            else
            {
                LogWarning("[DynamicDifficultyController] ⚠️ LevelManager为null，跳过spawnFromLevel过滤");
            }

            // 如果过滤后没有可用形状，使用所有形状
            if (filteredShapes.Count == 0)
            {
                LogWarning("[DynamicDifficultyController] ⚠️ 过滤后没有可用形状，使用所有形状");
                filteredShapes = availableShapes;
            }

            // 更新游戏状态
            currentGameState = state ?? currentGameState ?? new GameState();
            Log($"[DynamicDifficultyController] 游戏状态 | 当前关卡失败={currentGameState.currentLevelFailures}, 连续通关={currentGameState.consecutiveCleanWins}, 状态={currentGameState.currentState}");

            // 第一层：获取基线概率
            CategoryProbabilities probs = GetBaselineProbabilities(level);
            Log($"[DynamicDifficultyController] Layer 1 (基线概率) | Basic={probs.basic:F2}, Shaped={probs.shaped:F2}, Large={probs.large:F2}");

            // 第二层：动态调整概率
            var probsBeforeLayer2 = new CategoryProbabilities { basic = probs.basic, shaped = probs.shaped, large = probs.large };
            ApplyDynamicAdjustment(ref probs, currentGameState);
            if (probs.basic != probsBeforeLayer2.basic || probs.shaped != probsBeforeLayer2.shaped || probs.large != probsBeforeLayer2.large)
            {
                // 日志已在 ApplyDynamicAdjustment 内部输出
            }

            // 第三层：空间感知调整
            var probsBeforeLayer3 = new CategoryProbabilities { basic = probs.basic, shaped = probs.shaped, large = probs.large };
            ApplySpaceAwareAdjustment(ref probs, field);
            if (probs.basic != probsBeforeLayer3.basic || probs.shaped != probsBeforeLayer3.shaped || probs.large != probsBeforeLayer3.large)
            {
                Log($"[DynamicDifficultyController] Layer 3 (空间感知) | Basic={probs.basic:F2}, Shaped={probs.shaped:F2}, Large={probs.large:F2}");
            }

            // 保存最近使用的概率（用于调试）
            lastProbabilities = probs;

            // 第四层：反死局检测
            var remainingShapes = GetRemainingShapes();
            if (layer4 != null && layer4.ShouldTrigger(remainingShapes, field))
            {
                Log($"[DynamicDifficultyController] Layer 4 (反死局) ⚠️ 触发！生成保证可放置的方块");

                var guaranteedShape = layer4.GenerateGuaranteedPlaceable(field, filteredShapes);
                lastSelectedShape = guaranteedShape;
                Log($"[DynamicDifficultyController] ✅ 返回反死局方块: {guaranteedShape?.name}");
                return guaranteedShape;
            }

            // 根据最终概率选择方块（使用过滤后的形状列表）
            var selectedShape = SelectShapeByProbability(probs, filteredShapes);
            lastSelectedShape = selectedShape;

            if (selectedShape != null)
            {
                Log($"[DynamicDifficultyController] ✅ 最终选择: {selectedShape.name} | Category={selectedShape.category}, CellCount={selectedShape.cellCount}");
            }
            else
            {
                LogWarning("[DynamicDifficultyController] ⚠️ 未能选择方块，返回null");
            }

            return selectedShape;
        }

        /// <summary>
        /// 关卡开始时调用
        /// </summary>
        private void OnLevelStarted()
        {
            if (currentGameState == null)
                currentGameState = new GameState();

            // 重置当前关卡的失败计数
            currentGameState.OnLevelStarted();

            Log($"[DynamicDifficultyController] 📍 关卡开始 | 当前状态={currentGameState.currentState}, 连续一次性通关={currentGameState.consecutiveCleanWins}");
        }

        /// <summary>
        /// 关卡通关时调用
        /// </summary>
        private void OnLevelCompleted()
        {
            if (currentGameState == null)
                return;

            // 记录通关并更新难度状态
            currentGameState.OnLevelCompleted();

            // 检查是否达到提升条件
            if (difficultyConfig != null)
            {
                int N = difficultyConfig.firstIncreaseThreshold;
                if (currentGameState.consecutiveCleanWins >= N && currentGameState.currentState != DifficultyState.Increased)
                {
                    currentGameState.EnterIncreasedMode();
                    Log($"[DynamicDifficultyController] ⬆️ 连续{N}关一次性通关，进入提升难度模式");
                }
            }

            Log($"[DynamicDifficultyController] ✅ 关卡通关 | 一次性通关={!currentGameState.currentLevelHasFailure}, 连续通关={currentGameState.consecutiveCleanWins}, 状态={currentGameState.currentState}");

            layer3?.MarkDirty(); // 标记空间分析缓存失效
        }

        /// <summary>
        /// 关卡失败时调用（Retry/Revive触发）
        /// </summary>
        private void OnLevelFailed()
        {
            if (currentGameState == null)
                return;

            // 记录失败
            currentGameState.OnLevelFailed();

            // 检查是否需要进入降低难度模式
            if (difficultyConfig != null)
            {
                int M = difficultyConfig.decreaseFailureThreshold;
                if (currentGameState.currentLevelFailures >= M && currentGameState.currentState != DifficultyState.Decreased)
                {
                    currentGameState.EnterDecreasedMode();
                    Log($"[DynamicDifficultyController] ⬇️ 当前关卡失败{M}次，进入降低难度模式");
                }
            }

            Log($"[DynamicDifficultyController] ❌ 关卡失败 | 当前关卡失败次数={currentGameState.currentLevelFailures}, 状态={currentGameState.currentState}");
        }

        /// <summary>
        /// 获取基线概率（第一层）
        /// 优先使用关卡的自定义权重，否则使用全局配置
        /// </summary>
        private CategoryProbabilities GetBaselineProbabilities(Level level)
        {
            // 🔍 添加入口日志
            Log($"[DynamicDifficultyController] 🔍 GetBaselineProbabilities 入口 | level={(level != null ? level.name : "null")}, layer1={(layer1 != null ? "已加载" : "null")}");

            // 优先使用关卡的自定义权重
            if (level != null && level.useCustomProbabilities)
            {
                Log($"[DynamicDifficultyController] 📋 使用自定义权重 | Basic={level.customBaseline.basic:F2}, Shaped={level.customBaseline.shaped:F2}, Large={level.customBaseline.large:F2}");
                return level.customBaseline;
            }

            // 否则使用全局配置（基于 shapeWeightLevel）
            if (layer1 == null)
            {
                LogWarning("DynamicDifficultyController: layer1未设置，使用默认Normal难度");
                return new CategoryProbabilities { basic = 0.5f, shaped = 0.35f, large = 0.15f };
            }

            // 🔍 添加详细日志
            if (level == null)
            {
                LogError("[DynamicDifficultyController] ⚠️ level为null，使用Normal难度");
                return layer1.GetBaseline(DifficultyLevel.Normal);
            }

            Log($"[DynamicDifficultyController] 🔍 准备调用 layer1.GetBaseline({level.shapeWeightLevel})");

            // 使用 shapeWeightLevel（方块配置）而不是 difficultyLevel（计算结果）
            var baseline = layer1.GetBaseline(level.shapeWeightLevel);

            Log($"[DynamicDifficultyController] 📋 使用预设权重 | ShapeWeightLevel={level.shapeWeightLevel}, Basic={baseline.basic:F2}, Shaped={baseline.shaped:F2}, Large={baseline.large:F2}");
            return baseline;
        }

        /// <summary>
        /// 应用动态调整（第二层）
        /// 关卡级调整：只根据当前状态（Normal/Increased/Decreased）调整概率
        /// </summary>
        private void ApplyDynamicAdjustment(ref CategoryProbabilities probs, GameState state)
        {
            if (layer2 == null)
                return;

            // 根据当前难度状态调整概率
            switch (state.currentState)
            {
                case DifficultyState.Normal:
                    // 基线难度，不调整
                    break;

                case DifficultyState.Increased:
                    // 提升模式：增加Large概率，减少Basic概率
                    var increaseAmount = difficultyConfig?.decreaseAdjustAmount ?? 0.1f; // 复用调整幅度参数
                    probs.basic -= increaseAmount;
                    probs.large += increaseAmount;
                    probs.Normalize();
                    Log($"[DynamicDifficultyController] Layer 2 (提升模式) | Basic={probs.basic:F2}, Shaped={probs.shaped:F2}, Large={probs.large:F2}");
                    break;

                case DifficultyState.Decreased:
                    // 降低模式：应用降低难度调整
                    var level = GetCurrentLevel();
                    var baseline = GetBaselineProbabilities(level);
                    layer2.ApplyDecreasedDifficulty(ref probs, baseline);
                    Log($"[DynamicDifficultyController] Layer 2 (降低模式) | Basic={probs.basic:F2}, Shaped={probs.shaped:F2}, Large={probs.large:F2}");
                    break;
            }
        }

        /// <summary>
        /// 应用空间感知调整（第三层）
        /// </summary>
        private void ApplySpaceAwareAdjustment(ref CategoryProbabilities probs, FieldManager field)
        {
            if (layer3 == null)
                return;

            var spaceInfo = layer3.AnalyzeSpace(field);

            // 如果空间危险或碎片化严重，增加小方块概率
            if (spaceInfo.IsCritical)
            {
                probs.basic += 0.2f;
                probs.large -= 0.2f;
                probs.Normalize();

                Log($"[DynamicDifficultyController] 空间危险，调整概率: " +
                    $"空格率={spaceInfo.emptyPercentage:P2}, " +
                    $"碎片化={spaceInfo.fragmentationLevel}");
            }
            else if (spaceInfo.level == SpaceLevel.Tight)
            {
                probs.basic += 0.1f;
                probs.large -= 0.1f;
                probs.Normalize();
            }
        }

        /// <summary>
        /// 根据概率选择方块
        /// </summary>
        /// <param name="probs">分类概率</param>
        /// <param name="shapesToSelect">可选的形状列表，如果为null则使用availableShapes</param>
        private ShapeTemplate SelectShapeByProbability(CategoryProbabilities probs, List<ShapeTemplate> shapesToSelect = null)
        {
            // 使用传入的形状列表，如果为null则使用availableShapes
            var shapes = shapesToSelect ?? availableShapes;

            if (shapes == null || shapes.Count == 0)
            {
                LogWarning("DynamicDifficultyController: 可用方块列表为空");
                return null;
            }

            // 随机选择分类
            float random = Random.value;
            ShapeCategory category;

            if (random < probs.basic)
            {
                category = ShapeCategory.Basic;
            }
            else if (random < probs.basic + probs.shaped)
            {
                category = ShapeCategory.Shaped;
            }
            else
            {
                category = ShapeCategory.Large;
            }

            // 从该分类中随机选择一个方块
            var categoryShapes = shapes.FindAll(s => s != null && s.category == category);

            if (categoryShapes.Count == 0)
            {
                // 如果该分类没有可用方块，从所有方块中随机选择
                categoryShapes = new List<ShapeTemplate>(shapes);
            }

            if (categoryShapes.Count == 0)
                return null;

            // 同分类内均匀随机选择
            var selectedShape = categoryShapes[Random.Range(0, categoryShapes.Count)];
            Log($"[DynamicDifficultyController] 🎲 随机选择: {selectedShape.name} (category={category}, 候选数={categoryShapes.Count})");
            return selectedShape;
        }

        /// <summary>
        /// 获取剩余待放置的方块列表
        /// </summary>
        private List<ShapeTemplate> GetRemainingShapes()
        {
            var cellDeckManager = FindObjectOfType<CellDeckManager>();
            if (cellDeckManager == null)
            {
                LogWarning("DynamicDifficultyController: CellDeckManager未找到");
                return new List<ShapeTemplate>();
            }

            var shapes = cellDeckManager.GetShapes();
            var templates = new List<ShapeTemplate>();

            foreach (var shape in shapes)
            {
                if (shape != null && shape.shapeTemplate != null)
                {
                    templates.Add(shape.shapeTemplate);
                }
            }

            return templates;
        }

        /// <summary>
        /// 设置可用方块列表
        /// </summary>
        public void SetAvailableShapes(List<ShapeTemplate> shapes)
        {
            availableShapes = shapes ?? new List<ShapeTemplate>();
        }

        /// <summary>
        /// 重置所有层的状态
        /// </summary>
        public void ResetAllLayers()
        {
            currentGameState = new GameState();
            layer3?.Reset();
            layer4?.ResetStatistics();
        }

        /// <summary>
        /// 获取经典/计时模式的当前分数
        /// </summary>
        private int GetClassicScore()
        {
            // 尝试获取经典模式分数
            var classicHandler = FindObjectOfType<ClassicModeHandler>();
            if (classicHandler != null)
                return classicHandler.score;

            // 尝试获取计时模式分数
            var timedHandler = FindObjectOfType<TimedModeHandler>();
            if (timedHandler != null)
                return timedHandler.score;

            return 0;
        }

        /// <summary>
        /// 获取当前关卡
        /// </summary>
        private Level GetCurrentLevel()
        {
            if (levelManager == null)
            {
                levelManager = FindObjectOfType<LevelManager>();
            }

            return levelManager?.GetCurrentLevel();
        }

        /// <summary>
        /// 获取统计信息（用于调试）
        /// </summary>
        public string GetStatistics()
        {
            if (layer4 == null)
                return "统计信息不可用";

            return $"动态难度控制统计:\n" +
                   $"连续一次性通关: {currentGameState.consecutiveCleanWins}\n" +
                   $"当前关卡失败: {currentGameState.currentLevelFailures}\n" +
                   $"当前状态: {currentGameState.currentState}\n" +
                   $"{layer4.GetStatistics()}";
        }
    }
}
