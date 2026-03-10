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
using System.Collections.Generic;
using UnityEngine;
using BlockPuzzleGameToolkit.Scripts.Gameplay;
using BlockPuzzleGameToolkit.Scripts.LevelsData;

namespace GameCore.DifficultySystem
{
    /// <summary>
    /// 第四层：反死局生成器
    /// 当检测到所有方块都无法放置时，生成一个保证可放置的方块
    /// </summary>
    [Serializable]
    public class AntiDeadlockGenerator
    {
        [Header("Trigger Parameters")]
        [Tooltip("触发第四层的失败阈值")]
        public int triggerThreshold = 3;

        [Header("Statistics")]
        [Tooltip("第四层触发总次数")]
        public int layer4TriggerCount = 0;

        [Tooltip("方块生成总次数")]
        public int totalGenerations = 0;

        /// <summary>
        /// 检查是否应该触发第四层
        /// </summary>
        /// <param name="remainingShapes">剩余待放置的方块列表</param>
        /// <param name="field">棋盘管理器</param>
        /// <returns>true表示需要触发反死局</returns>
        public bool ShouldTrigger(List<ShapeTemplate> remainingShapes, FieldManager field)
        {
            if (remainingShapes == null || remainingShapes.Count == 0)
                return false;

            if (field == null || field.cells == null)
                return false;

            // 检查所有剩余方块是否都无法放置
            foreach (var shape in remainingShapes)
            {
                if (shape != null && CanPlaceShape(shape, field))
                {
                    return false; // 有可放置的，不触发
                }
            }

            return true; // 所有方块都无法放置，触发！
        }

        /// <summary>
        /// 生成一个保证可放置的方块
        /// </summary>
        /// <param name="field">棋盘管理器</param>
        /// <param name="availableShapes">可用方块模板列表</param>
        /// <returns>保证可放置的方块，如果无法找到则返回null</returns>
        public ShapeTemplate GenerateGuaranteedPlaceable(FieldManager field, List<ShapeTemplate> availableShapes)
        {
            layer4TriggerCount++;
            totalGenerations++;

            if (availableShapes == null || availableShapes.Count == 0)
            {
                Debug.LogWarning("AntiDeadlockGenerator: 可用方块列表为空");
                return null;
            }

            // 从小到大尝试：1格 → 2格 → 3格...
            // 创建按格子数排序的列表
            var sortedShapes = new List<ShapeTemplate>(availableShapes);
            sortedShapes.Sort((a, b) => a.cellCount.CompareTo(b.cellCount));

            // 找到第一个可放置的形状
            foreach (var shape in sortedShapes)
            {
                if (shape != null && CanPlaceShape(shape, field))
                {
                    return shape;
                }
            }

            // 如果所有方块都无法放置（极端情况），返回最小的方块
            Debug.LogWarning("AntiDeadlockGenerator: 无法找到可放置的方块，返回最小方块");
            return sortedShapes.Count > 0 ? sortedShapes[0] : null;
        }

        /// <summary>
        /// 获取第四层触发率
        /// </summary>
        /// <returns>触发率（0-1）</returns>
        public float GetTriggerRate()
        {
            return totalGenerations > 0
                ? (float)layer4TriggerCount / totalGenerations
                : 0f;
        }

        /// <summary>
        /// 检查形状是否可以放置在棋盘上
        /// </summary>
        /// <param name="shape">方块模板</param>
        /// <param name="field">棋盘管理器</param>
        /// <returns>true表示可以放置</returns>
        private bool CanPlaceShape(ShapeTemplate shape, FieldManager field)
        {
            if (shape == null || shape.rows == null)
                return false;

            if (field == null || field.cells == null)
                return false;

            int fieldRows = field.cells.GetLength(0);
            int fieldCols = field.cells.GetLength(1);

            // 遍历棋盘上的每个位置，尝试放置方块
            for (int fieldRow = 0; fieldRow < fieldRows; fieldRow++)
            {
                for (int fieldCol = 0; fieldCol < fieldCols; fieldCol++)
                {
                    if (CanPlaceShapeAt(shape, field, fieldRow, fieldCol))
                    {
                        return true; // 找到一个可放置的位置
                    }
                }
            }

            return false; // 没有任何位置可以放置
        }

        /// <summary>
        /// 检查形状是否可以放置在指定位置
        /// </summary>
        /// <param name="shape">方块模板</param>
        /// <param name="field">棋盘管理器</param>
        /// <param name="startRow">起始行</param>
        /// <param name="startCol">起始列</param>
        /// <returns>true表示可以放置</returns>
        private bool CanPlaceShapeAt(ShapeTemplate shape, FieldManager field, int startRow, int startCol)
        {
            if (shape.rows == null)
                return false;

            int fieldRows = field.cells.GetLength(0);
            int fieldCols = field.cells.GetLength(1);

            // 遍历形状模板的5x5矩阵
            for (int shapeRow = 0; shapeRow < shape.rows.Length && shapeRow < 5; shapeRow++)
            {
                var row = shape.rows[shapeRow];
                if (row == null || row.cells == null)
                    continue;

                for (int shapeCol = 0; shapeCol < row.cells.Length && shapeCol < 5; shapeCol++)
                {
                    // 如果形状模板的这个位置有方块
                    if (row.cells[shapeCol])
                    {
                        // 计算在棋盘上的对应位置
                        int targetRow = startRow + shapeRow;
                        int targetCol = startCol + shapeCol;

                        // 检查是否超出棋盘边界
                        if (targetRow < 0 || targetRow >= fieldRows ||
                            targetCol < 0 || targetCol >= fieldCols)
                        {
                            return false;
                        }

                        // 检查目标位置是否已被占用
                        var cell = field.cells[targetRow, targetCol];
                        if (cell == null || !cell.IsEmpty())
                        {
                            return false;
                        }
                    }
                }
            }

            return true; // 所有方块位置都可以放置
        }

        /// <summary>
        /// 重置统计数据
        /// </summary>
        public void ResetStatistics()
        {
            layer4TriggerCount = 0;
            totalGenerations = 0;
        }

        /// <summary>
        /// 获取统计信息（用于调试）
        /// </summary>
        public string GetStatistics()
        {
            float rate = GetTriggerRate();
            return $"第四层触发率: {rate:P2} ({layer4TriggerCount}/{totalGenerations})";
        }
    }
}
