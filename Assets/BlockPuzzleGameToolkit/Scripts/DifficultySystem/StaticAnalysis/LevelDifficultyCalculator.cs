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

using UnityEngine;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using BlockPuzzleGameToolkit.Scripts.LevelsData;

namespace GameCore.DifficultySystem
{
    /// <summary>
    /// 关卡静态难度计算器
    /// 负责计算关卡的六维度难度评分
    /// </summary>
    public class LevelDifficultyCalculator : SingletonBehaviour<LevelDifficultyCalculator>
    {
        [Header("Difficulty Weights Configuration")]
        [Tooltip("Difficulty weights configuration asset")]
        [SerializeField]
        private DifficultyWeights weights;

        /// <summary>
        /// 初始化优先级
        /// </summary>
        public override int InitPriority => 50;

        /// <summary>
        /// 初始化方法
        /// </summary>
        public override void OnInit()
        {
            base.OnInit();

            // 加载默认配置
            if (weights == null)
            {
                weights = Resources.Load<DifficultyWeights>("Settings/DifficultyWeights");
                if (weights == null)
                {
                    Debug.LogWarning("[LevelDifficultyCalculator] DifficultyWeights asset not found in Resources/Settings/");
                }
            }
        }

        /// <summary>
        /// 计算关卡难度
        /// </summary>
        /// <param name="level">关卡数据</param>
        /// <returns>难度评估结果</returns>
        public DifficultyResult Calculate(Level level)
        {
            if (level == null)
            {
                Debug.LogError("[LevelDifficultyCalculator] Level is null");
                return new DifficultyResult();
            }

            // 确保weights已加载
            if (weights == null)
            {
                Debug.LogWarning("[LevelDifficultyCalculator] DifficultyWeights not loaded, using default weights");
                weights = ScriptableObject.CreateInstance<DifficultyWeights>();
            }

            // 计算六维度分数
            DifficultyBreakdown breakdown = new DifficultyBreakdown
            {
                spaceStress = CalcSpaceStress(level),
                shapeComplexity = CalcShapeComplexity(level),
                targetPressure = CalcTargetPressure(level),
                timePressure = CalcTimePressure(level),
                resourceConstraint = CalcResourceConstraint(level),
                strategyDepth = CalcStrategyDepth(level)
            };

            // 加权求和计算总分
            float overallScore = CalculateOverallScore(breakdown);

            // 确定难度等级
            DifficultyLevel difficultyLevel = DetermineDifficultyLevel(overallScore);

            // 生成动态难度控制预览
            DynamicDifficultyPreview dynamicPreview = GenerateDynamicPreview(difficultyLevel);

            return new DifficultyResult
            {
                overallScore = overallScore,
                level = difficultyLevel,
                breakdown = breakdown,
                dynamicPreview = dynamicPreview
            };
        }

        /// <summary>
        /// 计算并保存到关卡（编辑器用）
        /// </summary>
        /// <param name="level">关卡数据</param>
        public void CalculateAndSave(Level level)
        {
            DifficultyResult result = Calculate(level);

            level.difficultyScore = result.overallScore;
            level.difficultyLevel = result.level;
            level.breakdown = result.breakdown;
            level.dynamicPreview = result.dynamicPreview;

            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(level);
            Debug.Log($"[LevelDifficultyCalculator] Calculated difficulty for {level.name}: {result.overallScore:F1} ({result.level})");
            #endif
        }

        #if UNITY_EDITOR
        /// <summary>
        /// 编辑器模式下的静态计算方法（不需要运行时）
        /// </summary>
        /// <param name="level">关卡数据</param>
        public static void CalculateInEditor(Level level)
        {
            if (level == null)
            {
                Debug.LogError("[LevelDifficultyCalculator] Level is null");
                return;
            }

            // 加载配置
            var weights = Resources.Load<DifficultyWeights>("Settings/DifficultyWeights");
            if (weights == null)
            {
                Debug.LogWarning("[LevelDifficultyCalculator] DifficultyWeights not found, creating default");
                weights = ScriptableObject.CreateInstance<DifficultyWeights>();
            }

            // 执行计算（使用静态辅助方法）
            DifficultyResult result = CalculateStatic(level, weights);

            // 保存结果
            level.difficultyScore = result.overallScore;
            level.difficultyLevel = result.level;
            level.breakdown = result.breakdown;
            level.dynamicPreview = result.dynamicPreview;

            UnityEditor.EditorUtility.SetDirty(level);
            Debug.Log($"[LevelDifficultyCalculator] (Editor Mode) Calculated difficulty for {level.name}: {result.overallScore:F1} ({result.level})");
        }

        /// <summary>
        /// 静态计算方法（供编辑器和实例共用）
        /// </summary>
        private static DifficultyResult CalculateStatic(Level level, DifficultyWeights weights)
        {
            // 计算六维度分数
            DifficultyBreakdown breakdown = new DifficultyBreakdown
            {
                spaceStress = CalcSpaceStressStatic(level),
                shapeComplexity = CalcShapeComplexityStatic(level),
                targetPressure = CalcTargetPressureStatic(level),
                timePressure = CalcTimePressureStatic(level),
                resourceConstraint = CalcResourceConstraintStatic(level),
                strategyDepth = CalcStrategyDepthStatic(level)
            };

            // 加权求和计算总分
            float overallScore = CalculateOverallScoreStatic(breakdown, weights);

            // 确定难度等级
            DifficultyLevel difficultyLevel = DetermineDifficultyLevelStatic(overallScore, weights);

            // 生成动态难度控制预览
            DynamicDifficultyPreview dynamicPreview = GenerateDynamicPreviewStatic(difficultyLevel);

            return new DifficultyResult
            {
                overallScore = overallScore,
                level = difficultyLevel,
                breakdown = breakdown,
                dynamicPreview = dynamicPreview
            };
        }
        #endif

        #region 六维度计算方法

        /// <summary>
        /// 计算空间压力 (25%权重)
        /// 空间压力 = f(棋盘大小, 初始空格数)
        /// </summary>
        private float CalcSpaceStress(Level level)
        {
            // 总格子数
            int totalCells = level.rows * level.columns;

            // 计算初始空格数
            int emptyCells = Mathf.RoundToInt(totalCells * level.emptyCellPercentage / 100f);

            // 可用空间比例
            float availableSpaceRatio = (float)emptyCells / totalCells;

            // 空间压力评分 (空格越多压力越小)
            // 0%空格 = 100分, 50%空格 = 0分
            float spaceScore = Mathf.Clamp01(1f - availableSpaceRatio * 2f) * 100f;

            // 棋盘尺寸修正 (小棋盘压力更大)
            // 6x6 = +20分, 8x8 = 0分, 10x10 = -10分
            float sizeModifier = (64f - totalCells) / 4f;
            spaceScore = Mathf.Clamp(spaceScore + sizeModifier, 0f, 100f);

            return spaceScore;
        }

        /// <summary>
        /// 计算方块复杂度 (20%权重)
        /// 方块复杂度 = f(方块池种类数, 方块池中大块/异形块比例)
        ///
        /// 设计逻辑：
        /// - 根据实际配置的方块生成权重计算复杂度
        /// - 权重来源：自定义权重 > Shape Difficulty Level预设 > 默认Normal
        /// </summary>
        private float CalcShapeComplexity(Level level)
        {
            // 获取实际使用的方块生成权重
            CategoryProbabilities probs = GetShapeProbabilities(level);

            // 根据方块类型分布计算复杂度
            // 基础块（简单）：权重越高，复杂度越低
            // 异形块（中等）：权重适中
            // 大块（困难）：权重越高，复杂度越高

            // 复杂度计算公式：
            // 基础块 * 0（不增加复杂度）+ 异形块 * 50（中等） + 大块 * 100（高复杂度）
            float complexityScore =
                probs.basic * 0f +        // 基础块：0分
                probs.shaped * 50f +      // 异形块：50分
                probs.large * 100f;       // 大块：100分

            return Mathf.Clamp(complexityScore, 0f, 100f);
        }

        /// <summary>
        /// 获取关卡的方块生成权重
        /// 优先级：自定义权重 > shapeWeightLevel预设 > 默认Normal
        /// </summary>
        private CategoryProbabilities GetShapeProbabilities(Level level)
        {
            // 优先1：使用关卡的自定义权重
            if (level != null && level.useCustomProbabilities)
            {
                return level.customBaseline;
            }

            // 优先2：使用 shapeWeightLevel（方块配置）而不是 difficultyLevel（计算结果）
            // shapeWeightLevel 是策划设置的方块权重预设，不会被计算结果覆盖
            switch (level?.shapeWeightLevel)
            {
                case DifficultyLevel.Tutorial:
                    return new CategoryProbabilities { basic = 0.7f, shaped = 0.3f, large = 0f };
                case DifficultyLevel.Easy:
                    return new CategoryProbabilities { basic = 0.6f, shaped = 0.3f, large = 0.1f };
                case DifficultyLevel.Normal:
                    return new CategoryProbabilities { basic = 0.5f, shaped = 0.35f, large = 0.15f };
                case DifficultyLevel.Hard:
                    return new CategoryProbabilities { basic = 0.4f, shaped = 0.4f, large = 0.2f };
                case DifficultyLevel.Expert:
                    return new CategoryProbabilities { basic = 0.3f, shaped = 0.45f, large = 0.25f };
                case DifficultyLevel.Master:
                    return new CategoryProbabilities { basic = 0.2f, shaped = 0.5f, large = 0.3f };
                default:
                    return new CategoryProbabilities { basic = 0.5f, shaped = 0.35f, large = 0.15f };
            }
        }

        /// <summary>
        /// 计算目标压力 (25%权重)
        /// 目标压力 = f(目标数量, 目标总量, 目标类型)
        /// </summary>
        private float CalcTargetPressure(Level level)
        {
            if (level.targetInstance == null || level.targetInstance.Count == 0)
            {
                return 0f;
            }

            float pressureScore = 0f;

            // 目标数量压力 (1个=20分, 3个=60分, 5个+=100分)
            int targetCount = level.targetInstance.Count;
            float countPressure = Mathf.Min(targetCount * 20f, 100f);

            // 目标总量压力 (根据总量计算)
            int totalAmount = 0;
            foreach (var target in level.targetInstance)
            {
                totalAmount += target.totalAmount;
            }

            // 总量压力 (100=20分, 300=60分, 500+=100分)
            float amountPressure = Mathf.Clamp((totalAmount - 50f) / 4.5f, 0f, 100f);

            // 目标类型压力 (不同类型有不同难度系数)
            // 注: 当前简化处理,后续可根据实际目标类型细化
            float typePressure = 50f;

            // 加权平均
            pressureScore = (countPressure * 0.3f + amountPressure * 0.5f + typePressure * 0.2f);

            return Mathf.Clamp(pressureScore, 0f, 100f);
        }

        /// <summary>
        /// 计算时间压力 (15%权重)
        /// 时间压力 = f(是否限时, 时长)
        /// </summary>
        private float CalcTimePressure(Level level)
        {
            if (!level.enableTimer)
            {
                return 0f; // 无时间限制
            }

            // 时间压力 (180s=0分, 120s=50分, 60s=100分)
            float duration = level.timerDuration;
            float timePressure = Mathf.Clamp01((180f - duration) / 120f) * 100f;

            return timePressure;
        }

        /// <summary>
        /// 计算资源限制 (10%权重)
        /// 资源限制 = f(初始方块配置)
        /// </summary>
        private float CalcResourceConstraint(Level level)
        {
            if (level.initialShapeRefreshes == null || level.initialShapeRefreshes.Count == 0)
            {
                return 0f; // 无初始配置,使用随机生成
            }

            // 有配置的初始方块说明有特定设计意图
            // 配置数量越多,资源限制越明显
            int configuredRefreshes = level.initialShapeRefreshes.Count;

            // 1-2次配置=30分, 3-4次=60分, 5次+=90分
            float constraintScore = Mathf.Min(configuredRefreshes * 18f, 90f);

            return constraintScore;
        }

        /// <summary>
        /// 计算策略深度 (5%权重)
        /// 策略深度 = f(奖励道具种类, 特殊机制)
        /// </summary>
        private float CalcStrategyDepth(Level level)
        {
            float strategyScore = 0f;

            // 奖励道具种类 (每种+25分)
            if (level.bonusItemColors != null)
            {
                int bonusTypes = level.bonusItemColors.Count;
                strategyScore += Mathf.Min(bonusTypes * 25f, 75f);
            }

            // 目标多样性 (多种目标类型需要更多策略思考)
            if (level.targetInstance != null && level.targetInstance.Count >= 2)
            {
                strategyScore += 25f;
            }

            return Mathf.Clamp(strategyScore, 0f, 100f);
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 加权求和计算总分
        /// </summary>
        private float CalculateOverallScore(DifficultyBreakdown breakdown)
        {
            float score = 0f;

            score += breakdown.spaceStress * weights.spaceStressWeight;
            score += breakdown.shapeComplexity * weights.shapeComplexityWeight;
            score += breakdown.targetPressure * weights.targetPressureWeight;
            score += breakdown.timePressure * weights.timePressureWeight;
            score += breakdown.resourceConstraint * weights.resourceConstraintWeight;
            score += breakdown.strategyDepth * weights.strategyDepthWeight;

            return Mathf.Clamp(score, 0f, 100f);
        }

        /// <summary>
        /// 根据分数确定难度等级
        /// </summary>
        private DifficultyLevel DetermineDifficultyLevel(float score)
        {
            if (score <= weights.tutorialMax)
                return DifficultyLevel.Tutorial;
            else if (score <= weights.easyMax)
                return DifficultyLevel.Easy;
            else if (score <= weights.normalMax)
                return DifficultyLevel.Normal;
            else if (score <= weights.hardMax)
                return DifficultyLevel.Hard;
            else if (score <= weights.expertMax)
                return DifficultyLevel.Expert;
            else
                return DifficultyLevel.Master;
        }

        /// <summary>
        /// 生成动态难度控制预览
        /// </summary>
        private DynamicDifficultyPreview GenerateDynamicPreview(DifficultyLevel level)
        {
            // 根据难度等级预设概率分布
            // 这些值将被DynamicDifficultyController使用
            DynamicDifficultyPreview preview = new DynamicDifficultyPreview();

            switch (level)
            {
                case DifficultyLevel.Tutorial:
                    preview.basicProb = 70f;  // 70%
                    preview.shapedProb = 20f;
                    preview.largeProb = 10f;
                    preview.failureThreshold = 999; // 无限制
                    break;
                case DifficultyLevel.Easy:
                    preview.basicProb = 60f;  // 60%
                    preview.shapedProb = 30f;
                    preview.largeProb = 10f;
                    preview.failureThreshold = 10;
                    break;
                case DifficultyLevel.Normal:
                    preview.basicProb = 50f;  // 50%
                    preview.shapedProb = 35f;
                    preview.largeProb = 15f;
                    preview.failureThreshold = 8;
                    break;
                case DifficultyLevel.Hard:
                    preview.basicProb = 40f;  // 40%
                    preview.shapedProb = 40f;
                    preview.largeProb = 20f;
                    preview.failureThreshold = 6;
                    break;
                case DifficultyLevel.Expert:
                    preview.basicProb = 30f;  // 30%
                    preview.shapedProb = 45f;
                    preview.largeProb = 25f;
                    preview.failureThreshold = 5;
                    break;
                case DifficultyLevel.Master:
                    preview.basicProb = 20f;  // 20%
                    preview.shapedProb = 50f;
                    preview.largeProb = 30f;
                    preview.failureThreshold = 3;
                    break;
            }

            return preview;
        }

        #endregion

        #region 静态版本的计算方法（供编辑器使用）

        /// <summary>
        /// 计算空间压力（静态版本）
        /// </summary>
        private static float CalcSpaceStressStatic(Level level)
        {
            int totalCells = level.rows * level.columns;
            int emptyCells = Mathf.RoundToInt(totalCells * level.emptyCellPercentage / 100f);
            float availableSpaceRatio = (float)emptyCells / totalCells;
            float spaceScore = Mathf.Clamp01(1f - availableSpaceRatio * 2f) * 100f;
            float sizeModifier = (64f - totalCells) / 4f;
            spaceScore = Mathf.Clamp(spaceScore + sizeModifier, 0f, 100f);
            return spaceScore;
        }

        /// <summary>
        /// 计算方块复杂度（静态版本）
        ///
        /// 设计逻辑：
        /// - 根据实际配置的方块生成权重计算复杂度
        /// - 权重来源：自定义权重 > Shape Difficulty Level预设 > 默认Normal
        /// </summary>
        private static float CalcShapeComplexityStatic(Level level)
        {
            // 获取实际使用的方块生成权重
            CategoryProbabilities probs = GetShapeProbabilitiesStatic(level);

            // 根据方块类型分布计算复杂度
            float complexityScore =
                probs.basic * 0f +        // 基础块：0分
                probs.shaped * 50f +      // 异形块：50分
                probs.large * 100f;       // 大块：100分

            return Mathf.Clamp(complexityScore, 0f, 100f);
        }

        /// <summary>
        /// 获取关卡的方块生成权重（静态版本）
        /// 优先级：自定义权重 > shapeWeightLevel预设 > 默认Normal
        /// </summary>
        private static CategoryProbabilities GetShapeProbabilitiesStatic(Level level)
        {
            // 优先1：使用关卡的自定义权重
            if (level != null && level.useCustomProbabilities)
            {
                return level.customBaseline;
            }

            // 优先2：使用 shapeWeightLevel（方块配置）而不是 difficultyLevel（计算结果）
            switch (level?.shapeWeightLevel)
            {
                case DifficultyLevel.Tutorial:
                    return new CategoryProbabilities { basic = 0.7f, shaped = 0.3f, large = 0f };
                case DifficultyLevel.Easy:
                    return new CategoryProbabilities { basic = 0.6f, shaped = 0.3f, large = 0.1f };
                case DifficultyLevel.Normal:
                    return new CategoryProbabilities { basic = 0.5f, shaped = 0.35f, large = 0.15f };
                case DifficultyLevel.Hard:
                    return new CategoryProbabilities { basic = 0.4f, shaped = 0.4f, large = 0.2f };
                case DifficultyLevel.Expert:
                    return new CategoryProbabilities { basic = 0.3f, shaped = 0.45f, large = 0.25f };
                case DifficultyLevel.Master:
                    return new CategoryProbabilities { basic = 0.2f, shaped = 0.5f, large = 0.3f };
                default:
                    return new CategoryProbabilities { basic = 0.5f, shaped = 0.35f, large = 0.15f };
            }
        }

        /// <summary>
        /// 计算目标压力（静态版本）
        /// </summary>
        private static float CalcTargetPressureStatic(Level level)
        {
            if (level.targetInstance == null || level.targetInstance.Count == 0)
            {
                return 0f;
            }

            float pressureScore = 0f;
            int targetCount = level.targetInstance.Count;
            float countPressure = Mathf.Min(targetCount * 20f, 100f);

            int totalAmount = 0;
            foreach (var target in level.targetInstance)
            {
                totalAmount += target.totalAmount;
            }

            float amountPressure = Mathf.Clamp((totalAmount - 50f) / 4.5f, 0f, 100f);
            float typePressure = 50f;
            pressureScore = (countPressure * 0.3f + amountPressure * 0.5f + typePressure * 0.2f);

            return Mathf.Clamp(pressureScore, 0f, 100f);
        }

        /// <summary>
        /// 计算时间压力（静态版本）
        /// </summary>
        private static float CalcTimePressureStatic(Level level)
        {
            if (!level.enableTimer)
            {
                return 0f;
            }

            float duration = level.timerDuration;
            float timePressure = Mathf.Clamp01((180f - duration) / 120f) * 100f;

            return timePressure;
        }

        /// <summary>
        /// 计算资源限制（静态版本）
        /// </summary>
        private static float CalcResourceConstraintStatic(Level level)
        {
            if (level.initialShapeRefreshes == null || level.initialShapeRefreshes.Count == 0)
            {
                return 0f;
            }

            int configuredRefreshes = level.initialShapeRefreshes.Count;
            float constraintScore = Mathf.Min(configuredRefreshes * 18f, 90f);

            return constraintScore;
        }

        /// <summary>
        /// 计算策略深度（静态版本）
        /// </summary>
        private static float CalcStrategyDepthStatic(Level level)
        {
            float strategyScore = 0f;

            if (level.bonusItemColors != null)
            {
                int bonusTypes = level.bonusItemColors.Count;
                strategyScore += Mathf.Min(bonusTypes * 25f, 75f);
            }

            if (level.targetInstance != null && level.targetInstance.Count >= 2)
            {
                strategyScore += 25f;
            }

            return Mathf.Clamp(strategyScore, 0f, 100f);
        }

        /// <summary>
        /// 加权求和计算总分（静态版本）
        /// </summary>
        private static float CalculateOverallScoreStatic(DifficultyBreakdown breakdown, DifficultyWeights weights)
        {
            float score = 0f;

            score += breakdown.spaceStress * weights.spaceStressWeight;
            score += breakdown.shapeComplexity * weights.shapeComplexityWeight;
            score += breakdown.targetPressure * weights.targetPressureWeight;
            score += breakdown.timePressure * weights.timePressureWeight;
            score += breakdown.resourceConstraint * weights.resourceConstraintWeight;
            score += breakdown.strategyDepth * weights.strategyDepthWeight;

            return Mathf.Clamp(score, 0f, 100f);
        }

        /// <summary>
        /// 根据分数确定难度等级（静态版本）
        /// </summary>
        private static DifficultyLevel DetermineDifficultyLevelStatic(float score, DifficultyWeights weights)
        {
            if (score <= weights.tutorialMax)
                return DifficultyLevel.Tutorial;
            else if (score <= weights.easyMax)
                return DifficultyLevel.Easy;
            else if (score <= weights.normalMax)
                return DifficultyLevel.Normal;
            else if (score <= weights.hardMax)
                return DifficultyLevel.Hard;
            else if (score <= weights.expertMax)
                return DifficultyLevel.Expert;
            else
                return DifficultyLevel.Master;
        }

        /// <summary>
        /// 生成动态难度控制预览（静态版本）
        /// </summary>
        private static DynamicDifficultyPreview GenerateDynamicPreviewStatic(DifficultyLevel level)
        {
            DynamicDifficultyPreview preview = new DynamicDifficultyPreview();

            switch (level)
            {
                case DifficultyLevel.Tutorial:
                    preview.basicProb = 70f;
                    preview.shapedProb = 20f;
                    preview.largeProb = 10f;
                    preview.failureThreshold = 999;
                    break;
                case DifficultyLevel.Easy:
                    preview.basicProb = 60f;
                    preview.shapedProb = 30f;
                    preview.largeProb = 10f;
                    preview.failureThreshold = 10;
                    break;
                case DifficultyLevel.Normal:
                    preview.basicProb = 50f;
                    preview.shapedProb = 35f;
                    preview.largeProb = 15f;
                    preview.failureThreshold = 8;
                    break;
                case DifficultyLevel.Hard:
                    preview.basicProb = 40f;
                    preview.shapedProb = 40f;
                    preview.largeProb = 20f;
                    preview.failureThreshold = 6;
                    break;
                case DifficultyLevel.Expert:
                    preview.basicProb = 30f;
                    preview.shapedProb = 45f;
                    preview.largeProb = 25f;
                    preview.failureThreshold = 5;
                    break;
                case DifficultyLevel.Master:
                    preview.basicProb = 20f;
                    preview.shapedProb = 50f;
                    preview.largeProb = 30f;
                    preview.failureThreshold = 3;
                    break;
            }

            return preview;
        }

        #endregion
    }
}
