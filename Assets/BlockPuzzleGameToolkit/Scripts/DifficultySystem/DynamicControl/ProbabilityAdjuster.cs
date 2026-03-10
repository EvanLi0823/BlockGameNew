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
using UnityEngine;

namespace GameCore.DifficultySystem
{
    /// <summary>
    /// 第二层：概率动态调整算法
    /// 根据玩家表现实时调整方块生成概率
    /// </summary>
    [Serializable]
    public class ProbabilityAdjuster
    {
        /// <summary>配置文件引用（运行时初始化）</summary>
        private DifficultyWeights config;

        /// <summary>是否已初始化配置</summary>
        private bool isConfigInitialized = false;

        [Header("Adjustment Parameters")]
        [Tooltip("每次放置成功的调整步长")]
        [Range(0.01f, 0.1f)]
        public float placementAdjustStep = 0.02f;

        [Tooltip("每次失败的调整步长")]
        [Range(0.01f, 0.2f)]
        public float failureAdjustStep = 0.05f;

        [Header("Adjustment Limits")]
        [Tooltip("Basic概率的最低限制")]
        [Range(0f, 0.5f)]
        public float minBasicProbability = 0.1f;

        [Tooltip("Large概率的最高限制")]
        [Range(0f, 0.5f)]
        public float maxLargeProbability = 0.4f;

        /// <summary>
        /// 初始化配置（在DynamicDifficultyController.OnInit中调用）
        /// </summary>
        /// <param name="difficultyWeights">难度权重配置</param>
        public void Initialize(DifficultyWeights difficultyWeights)
        {
            if (difficultyWeights == null)
            {
                Debug.LogError("[ProbabilityAdjuster] Initialize: DifficultyWeights为null，使用Inspector默认值");
                isConfigInitialized = false;
                return;
            }

            config = difficultyWeights;
            isConfigInitialized = true;

            Debug.Log($"[ProbabilityAdjuster] 已初始化配置 | N={config.firstIncreaseThreshold}, Y={config.subsequentIncreaseInterval}, M={config.decreaseFailureThreshold}");
        }

        /// <summary>
        /// 根据连续通关次数调整概率（阶段式提升难度）
        /// </summary>
        /// <param name="probs">当前概率（引用传递）</param>
        /// <param name="consecutiveWins">连续通关次数</param>
        public void AdjustByPlacement(ref CategoryProbabilities probs, int consecutiveWins)
        {
            int N = isConfigInitialized ? config.firstIncreaseThreshold : 10;
            int Y = isConfigInitialized ? config.subsequentIncreaseInterval : 5;

            // 阶段1：冷却期（连胜次数 < N）
            if (consecutiveWins < N)
            {
                return;
            }

            // 阶段2：首次提升（连胜次数 == N）
            if (consecutiveWins == N)
            {
                probs.basic -= placementAdjustStep;
                probs.large += placementAdjustStep;

                probs.basic = Mathf.Max(probs.basic, minBasicProbability);
                probs.large = Mathf.Min(probs.large, maxLargeProbability);

                probs.Normalize();

                Debug.Log($"[ProbabilityAdjuster] 首次提升难度 | consecutiveWins={consecutiveWins}, Basic={probs.basic:F2}, Large={probs.large:F2}");
                return;
            }

            // 阶段3：持续提升（连胜次数 > N，每Y次提升一次）
            int winsAfterFirst = consecutiveWins - N;
            if (winsAfterFirst > 0 && winsAfterFirst % Y == 0)
            {
                probs.basic -= placementAdjustStep;
                probs.large += placementAdjustStep;

                probs.basic = Mathf.Max(probs.basic, minBasicProbability);
                probs.large = Mathf.Min(probs.large, maxLargeProbability);

                probs.Normalize();

                int adjustmentLevel = winsAfterFirst / Y;
                Debug.Log($"[ProbabilityAdjuster] 再次提升难度(档位{adjustmentLevel + 1}) | consecutiveWins={consecutiveWins}, Basic={probs.basic:F2}, Large={probs.large:F2}");
            }
        }

        /// <summary>
        /// 重置到基线难度（失败时调用）
        /// </summary>
        /// <param name="probs">当前概率（引用传递）</param>
        /// <param name="baseline">基线概率</param>
        public void ResetToBaseline(ref CategoryProbabilities probs, CategoryProbabilities baseline)
        {
            probs.basic = baseline.basic;
            probs.shaped = baseline.shaped;
            probs.large = baseline.large;
            probs.Normalize();

            Debug.Log($"[ProbabilityAdjuster] 恢复基线难度 | Basic={probs.basic:F2}, Shaped={probs.shaped:F2}, Large={probs.large:F2}");
        }

        /// <summary>
        /// 应用降低难度调整（连续失败M次后调用）
        /// </summary>
        /// <param name="probs">当前概率（引用传递）</param>
        /// <param name="baseline">基线概率</param>
        public void ApplyDecreasedDifficulty(ref CategoryProbabilities probs, CategoryProbabilities baseline)
        {
            float adjustAmount = isConfigInitialized ? config.decreaseAdjustAmount : 0.1f;

            probs.basic = baseline.basic + adjustAmount;
            probs.large = baseline.large - adjustAmount;
            probs.shaped = baseline.shaped;

            probs.basic = Mathf.Clamp01(probs.basic);
            probs.large = Mathf.Max(0f, probs.large);

            probs.Normalize();

            Debug.Log($"[ProbabilityAdjuster] 应用降低难度 | Basic={probs.basic:F2}(+{adjustAmount:F2}), Large={probs.large:F2}(-{adjustAmount:F2})");
        }

        /// <summary>
        /// 根据失败次数调整概率（连续失败 → 降低难度）
        /// </summary>
        /// <param name="probs">当前概率（引用传递）</param>
        /// <param name="failureCount">连续失败次数</param>
        [Obsolete("新规则中失败使用ResetToBaseline()和ApplyDecreasedDifficulty()")]
        public void AdjustByFailure(ref CategoryProbabilities probs, int failureCount)
        {
            if (failureCount > 0)
            {
                float basicAdjust = failureCount * failureAdjustStep;
                float largeAdjust = failureCount * failureAdjustStep;

                probs.basic += basicAdjust;
                probs.large -= largeAdjust;

                // 防止负值
                probs.basic = Mathf.Clamp01(probs.basic);
                probs.large = Mathf.Max(0f, probs.large);

                probs.Normalize();
            }
        }

        /// <summary>
        /// 重置调整参数为默认值
        /// </summary>
        public void ResetToDefaults()
        {
            placementAdjustStep = 0.02f;
            failureAdjustStep = 0.05f;
            minBasicProbability = 0.1f;
            maxLargeProbability = 0.4f;
        }
    }
}
