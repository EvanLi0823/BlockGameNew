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
using BlockPuzzleGameToolkit.Scripts.Gameplay;

namespace GameCore.DifficultySystem
{
    /// <summary>
    /// 第三层：棋盘空间分析器（带缓存优化）
    /// 分析棋盘空间状态，计算碎片化程度和空间等级
    /// </summary>
    [Serializable]
    public class BoardSpaceAnalyzer
    {
        // 缓存数据
        private SpaceInfo cachedInfo;
        private int cachedBoardHash;
        private bool isDirty = true;

        [Header("Analysis Parameters")]
        [Tooltip("碎片化阈值：小于此大小的空区被视为碎片")]
        public int fragmentationThreshold = 4;

        /// <summary>
        /// 分析棋盘空间状态（带缓存）
        /// </summary>
        /// <param name="field">棋盘管理器</param>
        /// <returns>空间分析结果</returns>
        public SpaceInfo AnalyzeSpace(FieldManager field)
        {
            if (field == null || field.cells == null)
            {
                Debug.LogWarning("BoardSpaceAnalyzer: FieldManager或cells为null");
                return CreateEmptySpaceInfo();
            }

            int currentHash = CalculateBoardHash(field);

            // 检查缓存是否有效
            if (!isDirty && cachedBoardHash == currentHash && cachedInfo != null)
            {
                return cachedInfo;
            }

            // 重新计算
            cachedInfo = CalculateSpaceInfo(field);
            cachedBoardHash = currentHash;
            isDirty = false;

            return cachedInfo;
        }

        /// <summary>
        /// 标记缓存失效（方块放置后调用）
        /// </summary>
        public void MarkDirty()
        {
            isDirty = true;
        }

        /// <summary>
        /// 计算棋盘状态哈希值
        /// </summary>
        private int CalculateBoardHash(FieldManager field)
        {
            int hash = 17;
            int rows = field.cells.GetLength(0);
            int cols = field.cells.GetLength(1);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    var cell = field.cells[i, j];
                    bool isEmpty = cell != null && cell.IsEmpty();

                    // 使用简单的哈希算法
                    hash = hash * 31 + (isEmpty ? 0 : 1);
                }
            }

            return hash;
        }

        /// <summary>
        /// 计算空间分析信息
        /// </summary>
        private SpaceInfo CalculateSpaceInfo(FieldManager field)
        {
            var info = new SpaceInfo();
            int rows = field.cells.GetLength(0);
            int cols = field.cells.GetLength(1);

            info.totalCells = rows * cols;
            info.emptyCells = 0;

            // 标记已访问的格子（用于计算连续空区）
            bool[,] visited = new bool[rows, cols];

            // 第一遍遍历：统计空格数
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    var cell = field.cells[i, j];
                    if (cell != null && cell.IsEmpty())
                    {
                        info.emptyCells++;
                    }
                }
            }

            // 计算空格百分比
            info.emptyPercentage = info.totalCells > 0
                ? (float)info.emptyCells / info.totalCells
                : 0f;

            // 第二遍遍历：计算最大连续空区和碎片化
            info.largestEmptyArea = 0;
            int fragmentCount = 0;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    var cell = field.cells[i, j];
                    if (cell != null && cell.IsEmpty() && !visited[i, j])
                    {
                        // 使用深度优先搜索计算连续空区大小
                        int areaSize = FloodFillCount(field.cells, visited, i, j, rows, cols);

                        if (areaSize > info.largestEmptyArea)
                        {
                            info.largestEmptyArea = areaSize;
                        }

                        // 小区域算作碎片
                        if (areaSize < fragmentationThreshold)
                        {
                            fragmentCount++;
                        }
                    }
                }
            }

            // 计算碎片化程度（0-100）
            info.fragmentationLevel = CalculateFragmentation(
                info.emptyCells,
                info.largestEmptyArea,
                fragmentCount
            );

            // 确定空间等级
            info.level = DetermineSpaceLevel(info.emptyPercentage, info.fragmentationLevel);

            return info;
        }

        /// <summary>
        /// 洪水填充算法计算连续空区大小
        /// </summary>
        private int FloodFillCount(Cell[,] cells, bool[,] visited, int row, int col, int rows, int cols)
        {
            // 边界检查
            if (row < 0 || row >= rows || col < 0 || col >= cols)
                return 0;

            // 已访问或非空格
            if (visited[row, col])
                return 0;

            var cell = cells[row, col];
            if (cell == null || !cell.IsEmpty())
                return 0;

            // 标记为已访问
            visited[row, col] = true;
            int count = 1;

            // 递归检查四个方向（上下左右）
            count += FloodFillCount(cells, visited, row - 1, col, rows, cols); // 上
            count += FloodFillCount(cells, visited, row + 1, col, rows, cols); // 下
            count += FloodFillCount(cells, visited, row, col - 1, rows, cols); // 左
            count += FloodFillCount(cells, visited, row, col + 1, rows, cols); // 右

            return count;
        }

        /// <summary>
        /// 计算碎片化程度
        /// </summary>
        private int CalculateFragmentation(int emptyCells, int largestArea, int fragmentCount)
        {
            if (emptyCells == 0)
                return 0;

            // 碎片化 = (1 - 最大连续区/总空格) * 100 + 碎片数量惩罚
            float ratio = 1f - (float)largestArea / emptyCells;
            int baseFragmentation = Mathf.RoundToInt(ratio * 100f);
            int fragmentPenalty = fragmentCount * 5; // 每个碎片+5分

            return Mathf.Clamp(baseFragmentation + fragmentPenalty, 0, 100);
        }

        /// <summary>
        /// 确定空间等级
        /// </summary>
        private SpaceLevel DetermineSpaceLevel(float emptyPercentage, int fragmentation)
        {
            // 如果空格很少或碎片化严重，判定为危险
            if (emptyPercentage < 0.2f || fragmentation > 70)
                return SpaceLevel.Critical;

            // 如果空格较少或碎片化较高，判定为紧张
            if (emptyPercentage < 0.4f || fragmentation > 50)
                return SpaceLevel.Tight;

            // 如果空格适中，判定为正常
            if (emptyPercentage < 0.6f)
                return SpaceLevel.Normal;

            // 空格充足，判定为充裕
            return SpaceLevel.Abundant;
        }

        /// <summary>
        /// 创建空的SpaceInfo
        /// </summary>
        private SpaceInfo CreateEmptySpaceInfo()
        {
            return new SpaceInfo
            {
                totalCells = 0,
                emptyCells = 0,
                emptyPercentage = 0f,
                largestEmptyArea = 0,
                fragmentationLevel = 0,
                level = SpaceLevel.Normal
            };
        }

        /// <summary>
        /// 重置分析器状态
        /// </summary>
        public void Reset()
        {
            cachedInfo = null;
            cachedBoardHash = 0;
            isDirty = true;
        }
    }
}
