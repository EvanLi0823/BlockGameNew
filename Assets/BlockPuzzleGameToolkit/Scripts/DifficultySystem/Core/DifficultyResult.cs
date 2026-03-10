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
    /// 关卡难度评估结果
    /// </summary>
    [Serializable]
    public class DifficultyResult
    {
        /// <summary>综合难度评分 (0-100)</summary>
        [Tooltip("Overall difficulty score")]
        [Range(0, 100)]
        public float overallScore;

        /// <summary>难度等级</summary>
        [Tooltip("Difficulty classification")]
        public DifficultyLevel level;

        /// <summary>六维度分解</summary>
        [Tooltip("Breakdown of difficulty dimensions")]
        public DifficultyBreakdown breakdown;

        /// <summary>动态难度控制预览</summary>
        [Tooltip("Preview of dynamic difficulty control")]
        public DynamicDifficultyPreview dynamicPreview;
    }
}
