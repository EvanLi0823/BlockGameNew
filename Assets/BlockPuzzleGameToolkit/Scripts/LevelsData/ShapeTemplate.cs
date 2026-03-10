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
using GameCore.DifficultySystem;

namespace BlockPuzzleGameToolkit.Scripts.LevelsData
{
    /// <summary>
    /// ShapeRow - 表示形状模板中的一行
    /// 每行包含5个布尔值，定义该位置是否激活
    /// </summary>
    [Serializable]
    public class ShapeRow
    {
        /// <summary>
        /// 5个格子的布尔数组
        /// true表示该位置有方块，false表示空
        /// </summary>
        public bool[] cells = new bool[5];
    }

    /// <summary>
    /// ShapeTemplate - 形状模板数据类
    /// 定义可拖拽方块组的形状结构
    /// 使用5x5的布尔矩阵表示，可以创建各种俄罗斯方块形状
    /// </summary>
    [CreateAssetMenu(fileName = "Shape", menuName = "BlockPuzzleGameToolkit/Items/Shape", order = 1)]
    public class ShapeTemplate : ScriptableObject
    {
        /// <summary>
        /// 5x5的布尔矩阵
        /// 定义形状的结构，true表示有方块，false表示空
        /// 可以在Unity编辑器中可视化编辑
        /// </summary>
        public ShapeRow[] rows = new ShapeRow[5];

        /// <summary>
        /// 经典模式下解锁所需的分数
        /// 达到此分数后，该形状才会出现在游戏中
        /// </summary>
        public int scoreForSpawn;

        /// <summary>
        /// 生成权重（默认为1）
        /// 数值越大，被随机选中的概率越高
        /// 用于控制不同形状的出现频率
        /// </summary>
        public float chanceForSpawn = 1;

        /// <summary>
        /// 冒险模式下解锁的关卡
        /// 从第几关开始出现此形状
        /// </summary>
        public int spawnFromLevel = 1;

        /// <summary>
        /// 获取所有激活的格子（用于兼容性）
        /// 返回一个布尔数组，表示整个5x5矩阵中哪些位置有方块
        /// </summary>
        public bool[] cells
        {
            get
            {
                var result = new bool[25];
                if (rows != null)
                {
                    for (int i = 0; i < rows.Length && i < 5; i++)
                    {
                        if (rows[i] != null && rows[i].cells != null)
                        {
                            for (int j = 0; j < rows[i].cells.Length && j < 5; j++)
                            {
                                result[i * 5 + j] = rows[i].cells[j];
                            }
                        }
                    }
                }
                return result;
            }
        }

        // ===== 新增字段 (难度控制属性) =====
        [Header("Difficulty Classification")]
        [Tooltip("Use manual category override instead of auto-calculation")]
        public bool useManualCategory = false;

        [Tooltip("Shape category: Basic, Shaped, or Large")]
        public ShapeCategory category;

        [Tooltip("Actual width of the shape")]
        public int width;

        [Tooltip("Actual height of the shape")]
        public int height;

        [Tooltip("Number of filled cells")]
        public int cellCount;

        [Tooltip("Whether the shape is a rectangle")]
        public bool isRectangle;

        [Tooltip("Whether the shape is symmetrical")]
        public bool isSymmetrical;

        /// <summary>
        /// Unity生命周期 - 初始化
        /// 确保rows数组正确初始化
        /// </summary>
        private void OnEnable()
        {
            // 确保有5行数据
            if (rows == null || rows.Length != 5)
            {
                rows = new ShapeRow[5];
                for (var i = 0; i < 5; i++)
                {
                    rows[i] = new ShapeRow();
                }
            }
        }
    }
}