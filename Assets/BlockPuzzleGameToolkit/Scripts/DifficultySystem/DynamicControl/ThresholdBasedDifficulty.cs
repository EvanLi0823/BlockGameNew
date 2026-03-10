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
    /// 第一层：阈值基线算法
    /// 根据难度等级提供基础的方块分类概率
    /// </summary>
    [CreateAssetMenu(fileName = "ThresholdBasedDifficulty", menuName = "Game/Difficulty System/Threshold Difficulty")]
    public class ThresholdBasedDifficulty : ScriptableObject
    {
        [Header("Baseline Probabilities")]
        [Tooltip("教学关卡概率：70%基础块，30%异形块，0%大块")]
        public CategoryProbabilities tutorialBaseline = new CategoryProbabilities { basic = 0.7f, shaped = 0.3f, large = 0f };

        [Tooltip("简单关卡概率：60%基础块，30%异形块，10%大块")]
        public CategoryProbabilities easyBaseline = new CategoryProbabilities { basic = 0.6f, shaped = 0.3f, large = 0.1f };

        [Tooltip("普通关卡概率：45%基础块，40%异形块，15%大块")]
        public CategoryProbabilities normalBaseline = new CategoryProbabilities { basic = 0.45f, shaped = 0.4f, large = 0.15f };

        [Tooltip("困难关卡概率：35%基础块，40%异形块，25%大块")]
        public CategoryProbabilities hardBaseline = new CategoryProbabilities { basic = 0.35f, shaped = 0.4f, large = 0.25f };

        [Tooltip("专家关卡概率：25%基础块，45%异形块，30%大块")]
        public CategoryProbabilities expertBaseline = new CategoryProbabilities { basic = 0.25f, shaped = 0.45f, large = 0.3f };

        [Tooltip("大师关卡概率：20%基础块，45%异形块，35%大块")]
        public CategoryProbabilities masterBaseline = new CategoryProbabilities { basic = 0.2f, shaped = 0.45f, large = 0.35f };

        /// <summary>
        /// 根据难度等级获取基线概率
        /// </summary>
        /// <param name="level">难度等级</param>
        /// <returns>对应的基线概率</returns>
        public CategoryProbabilities GetBaseline(DifficultyLevel level)
        {
            switch (level)
            {
                case DifficultyLevel.Tutorial:
                    return tutorialBaseline;
                case DifficultyLevel.Easy:
                    return easyBaseline;
                case DifficultyLevel.Normal:
                    return normalBaseline;
                case DifficultyLevel.Hard:
                    return hardBaseline;
                case DifficultyLevel.Expert:
                    return expertBaseline;
                case DifficultyLevel.Master:
                    return masterBaseline;
                default:
                    return normalBaseline;
            }
        }

        // OnValidate已禁用 - 允许手动控制概率值，不自动归一化
        // /// <summary>
        // /// 验证所有基线概率是否有效
        // /// </summary>
        // private void OnValidate()
        // {
        //     tutorialBaseline.Normalize();
        //     easyBaseline.Normalize();
        //     normalBaseline.Normalize();
        //     hardBaseline.Normalize();
        //     expertBaseline.Normalize();
        //     masterBaseline.Normalize();
        // }
    }
}
