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

namespace GameCore.DifficultySystem
{
    /// <summary>
    /// 难度评估权重配置
    /// ScriptableObject配置文件,用于调整六维度权重和难度等级阈值
    /// </summary>
    [CreateAssetMenu(fileName = "DifficultyWeights", menuName = "Game/Difficulty System/Difficulty Weights", order = 1)]
    public class DifficultyWeights : ScriptableObject
    {
        [Header("Dimension Weights (Total = 100%)")]
        [Tooltip("Space stress weight (recommended: 0.25)")]
        [Range(0f, 1f)]
        public float spaceStressWeight = 0.25f;

        [Tooltip("Shape complexity weight (recommended: 0.20)")]
        [Range(0f, 1f)]
        public float shapeComplexityWeight = 0.20f;

        [Tooltip("Target pressure weight (recommended: 0.25)")]
        [Range(0f, 1f)]
        public float targetPressureWeight = 0.25f;

        [Tooltip("Time pressure weight (recommended: 0.15)")]
        [Range(0f, 1f)]
        public float timePressureWeight = 0.15f;

        [Tooltip("Resource constraint weight (recommended: 0.10)")]
        [Range(0f, 1f)]
        public float resourceConstraintWeight = 0.10f;

        [Tooltip("Strategy depth weight (recommended: 0.05)")]
        [Range(0f, 1f)]
        public float strategyDepthWeight = 0.05f;

        [Header("Difficulty Level Thresholds")]
        [Tooltip("Tutorial level max score (0-15)")]
        [Range(0f, 100f)]
        public float tutorialMax = 15f;

        [Tooltip("Easy level max score (16-30)")]
        [Range(0f, 100f)]
        public float easyMax = 30f;

        [Tooltip("Normal level max score (31-50)")]
        [Range(0f, 100f)]
        public float normalMax = 50f;

        [Tooltip("Hard level max score (51-70)")]
        [Range(0f, 100f)]
        public float hardMax = 70f;

        [Tooltip("Expert level max score (71-85)")]
        [Range(0f, 100f)]
        public float expertMax = 85f;

        // Master level: 86-100 (implicit)

        [Header("动态难度配置（关卡级）")]
        [Tooltip("连续N关一次性通关后提升难度\n" +
                 "一次性通关：关卡中无复活/重试\n" +
                 "推荐值：10关（保守15，激进8）")]
        public int firstIncreaseThreshold = 10;

        [Tooltip("（已废弃，提升模式不再使用此参数）\n" +
                 "保留用于降低模式的调整幅度")]
        public int subsequentIncreaseInterval = 5;

        [Tooltip("当前关卡失败M次后降低难度\n" +
                 "失败计数：复活/重试都算一次失败\n" +
                 "推荐值：3次（保守2，激进4）")]
        public int decreaseFailureThreshold = 3;

        [Tooltip("难度调整幅度\n" +
                 "提升模式：Large概率+X, Basic概率-X\n" +
                 "降低模式：Basic概率+X, Large概率-X\n" +
                 "推荐值：0.1（保守0.15，激进0.05）")]
        public float decreaseAdjustAmount = 0.1f;

        /// <summary>
        /// 验证权重总和是否为1.0
        /// </summary>
        public bool ValidateWeights()
        {
            float sum = spaceStressWeight + shapeComplexityWeight + targetPressureWeight
                        + timePressureWeight + resourceConstraintWeight + strategyDepthWeight;

            return Mathf.Approximately(sum, 1.0f);
        }

        /// <summary>
        /// 获取权重总和
        /// </summary>
        public float GetWeightSum()
        {
            return spaceStressWeight + shapeComplexityWeight + targetPressureWeight
                   + timePressureWeight + resourceConstraintWeight + strategyDepthWeight;
        }

        /// <summary>
        /// 重置为默认值
        /// </summary>
        public void ResetToDefaults()
        {
            spaceStressWeight = 0.25f;
            shapeComplexityWeight = 0.20f;
            targetPressureWeight = 0.25f;
            timePressureWeight = 0.15f;
            resourceConstraintWeight = 0.10f;
            strategyDepthWeight = 0.05f;

            tutorialMax = 15f;
            easyMax = 30f;
            normalMax = 50f;
            hardMax = 70f;
            expertMax = 85f;

            // 动态难度默认值
            firstIncreaseThreshold = 10;
            subsequentIncreaseInterval = 5;
            decreaseFailureThreshold = 3;
            decreaseAdjustAmount = 0.1f;

            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
        }

        #if UNITY_EDITOR
        /// <summary>
        /// 编辑器验证(当Inspector值改变时调用)
        /// </summary>
        private void OnValidate()
        {
            // 验证权重总和
            float sum = GetWeightSum();
            if (!Mathf.Approximately(sum, 1.0f))
            {
                Debug.LogWarning($"[DifficultyWeights] Weight sum is {sum:F3}, should be 1.0");
            }

            // 验证阈值顺序
            if (tutorialMax >= easyMax || easyMax >= normalMax || normalMax >= hardMax || hardMax >= expertMax)
            {
                Debug.LogWarning("[DifficultyWeights] Threshold values should be in ascending order");
            }

            // 验证动态难度配置
            if (firstIncreaseThreshold <= 0)
            {
                Debug.LogWarning($"[DifficultyWeights] firstIncreaseThreshold({firstIncreaseThreshold}) 应该 > 0，当前值无效");
            }

            // subsequentIncreaseInterval 已废弃，不再验证

            if (decreaseFailureThreshold <= 0)
            {
                Debug.LogWarning($"[DifficultyWeights] decreaseFailureThreshold({decreaseFailureThreshold}) 应该 > 0，当前值无效");
            }

            if (decreaseAdjustAmount <= 0 || decreaseAdjustAmount >= 1.0f)
            {
                Debug.LogWarning($"[DifficultyWeights] decreaseAdjustAmount({decreaseAdjustAmount}) 应该在 0 到 1 之间，当前值可能导致异常");
            }
        }
        #endif
    }
}
