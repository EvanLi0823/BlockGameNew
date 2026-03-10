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
    /// 动态难度控制预览
    /// </summary>
    [Serializable]
    public class DynamicDifficultyPreview
    {
        /// <summary>基础块概率</summary>
        [Tooltip("Probability of basic shapes")]
        [Range(0, 100)]
        public float basicProb;

        /// <summary>异形块概率</summary>
        [Tooltip("Probability of shaped blocks")]
        [Range(0, 100)]
        public float shapedProb;

        /// <summary>大块概率</summary>
        [Tooltip("Probability of large blocks")]
        [Range(0, 100)]
        public float largeProb;

        /// <summary>失败上限阈值</summary>
        [Tooltip("Failure count threshold before difficulty adjustment")]
        public int failureThreshold;
    }
}
