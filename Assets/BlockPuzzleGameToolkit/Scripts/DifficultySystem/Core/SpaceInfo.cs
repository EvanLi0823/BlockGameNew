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
    /// 棋盘空间分析信息
    /// </summary>
    [Serializable]
    public class SpaceInfo
    {
        /// <summary>总格子数</summary>
        public int totalCells;

        /// <summary>空格子数</summary>
        public int emptyCells;

        /// <summary>空格百分比 (0-1)</summary>
        [Range(0, 1)]
        public float emptyPercentage;

        /// <summary>最大连续空区大小</summary>
        public int largestEmptyArea;

        /// <summary>碎片化程度 (0-100，越高越碎片化)</summary>
        [Range(0, 100)]
        public int fragmentationLevel;

        /// <summary>空间等级</summary>
        public SpaceLevel level;

        /// <summary>是否处于危险状态</summary>
        public bool IsCritical => level == SpaceLevel.Critical;
    }
}
