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
    /// 方块分类概率（用于动态难度控制）
    /// </summary>
    [Serializable]
    public struct CategoryProbabilities
    {
        /// <summary>基础块概率 (0-1)</summary>
        [Range(0, 1)]
        public float basic;

        /// <summary>异形块概率 (0-1)</summary>
        [Range(0, 1)]
        public float shaped;

        /// <summary>大块概率 (0-1)</summary>
        [Range(0, 1)]
        public float large;

        /// <summary>
        /// 归一化概率，确保总和为1
        /// </summary>
        public void Normalize()
        {
            float total = basic + shaped + large;
            if (total > 0)
            {
                basic /= total;
                shaped /= total;
                large /= total;
            }
            else
            {
                // 默认均分
                basic = 0.33f;
                shaped = 0.33f;
                large = 0.34f;
            }
        }

        /// <summary>
        /// 检查概率是否有效
        /// </summary>
        public bool IsValid()
        {
            return basic >= 0 && shaped >= 0 && large >= 0 &&
                   Mathf.Abs(basic + shaped + large - 1f) < 0.01f;
        }
    }
}
