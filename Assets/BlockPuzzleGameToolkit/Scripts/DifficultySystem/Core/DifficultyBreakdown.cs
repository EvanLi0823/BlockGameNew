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
    /// 难度六维度分解
    /// </summary>
    [Serializable]
    public class DifficultyBreakdown
    {
        /// <summary>空间压力 (25%权重)</summary>
        [Tooltip("Space pressure from board size and initial empty cells")]
        [Range(0, 100)]
        public float spaceStress;

        /// <summary>方块复杂度 (20%权重)</summary>
        [Tooltip("Complexity of available shapes")]
        [Range(0, 100)]
        public float shapeComplexity;

        /// <summary>目标压力 (25%权重)</summary>
        [Tooltip("Pressure from target requirements")]
        [Range(0, 100)]
        public float targetPressure;

        /// <summary>时间压力 (15%权重)</summary>
        [Tooltip("Pressure from time limit")]
        [Range(0, 100)]
        public float timePressure;

        /// <summary>资源限制 (10%权重)</summary>
        [Tooltip("Constraint from limited resources")]
        [Range(0, 100)]
        public float resourceConstraint;

        /// <summary>策略深度 (5%权重)</summary>
        [Tooltip("Required strategic thinking depth")]
        [Range(0, 100)]
        public float strategyDepth;
    }
}
