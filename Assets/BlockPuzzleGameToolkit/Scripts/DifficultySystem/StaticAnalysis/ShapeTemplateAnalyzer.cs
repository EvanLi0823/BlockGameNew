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
using BlockPuzzleGameToolkit.Scripts.GameCore;
using BlockPuzzleGameToolkit.Scripts.LevelsData;

namespace GameCore.DifficultySystem
{
    /// <summary>
    /// 方块模板分析器
    /// 负责分析ShapeTemplate的几何属性并进行分类
    /// </summary>
    public class ShapeTemplateAnalyzer : SingletonBehaviour<ShapeTemplateAnalyzer>
    {
        /// <summary>
        /// 初始化优先级
        /// </summary>
        public override int InitPriority => 50;

        /// <summary>
        /// 分析方块模板并返回分类结果
        /// </summary>
        /// <param name="shape">方块模板</param>
        /// <returns>方块分类</returns>
        public ShapeCategory DetermineCategory(ShapeTemplate shape)
        {
            if (shape == null)
            {
                Debug.LogError("[ShapeTemplateAnalyzer] ShapeTemplate is null");
                return ShapeCategory.Basic;
            }

            int cellCount = CountCells(shape);
            bool isRect = IsRectangle(shape);

            // 分类规则（方案2：按游戏难度划分）:
            // 1. Basic：1-3格矩形（直线、小矩形）
            // 2. Large：4格矩形（2x2方块）+ 5格及以上
            // 3. Shaped：其他（3-4格非矩形：L形、T形、Z形等）

            if (cellCount <= 3 && isRect)
            {
                return ShapeCategory.Basic;  // 1-3格矩形 → 容易放置
            }
            else if (cellCount >= 5 || (cellCount == 4 && isRect))
            {
                return ShapeCategory.Large;  // 5格+ 或 4格矩形 → 困难
            }
            else
            {
                return ShapeCategory.Shaped;  // 3-4格非矩形 → 中等
            }
        }

        /// <summary>
        /// 分析并保存到ShapeTemplate（编辑器用）
        /// </summary>
        /// <param name="shape">方块模板</param>
        public void AnalyzeAndSave(ShapeTemplate shape)
        {
            if (shape == null)
            {
                Debug.LogError("[ShapeTemplateAnalyzer] ShapeTemplate is null");
                return;
            }

            // 计算几何属性
            shape.cellCount = CountCells(shape);
            CalculateDimensions(shape, out int width, out int height);
            shape.width = width;
            shape.height = height;
            shape.isRectangle = IsRectangle(shape);
            shape.isSymmetrical = IsSymmetrical(shape);

            // 确定分类（如果不是手动模式）
            if (!shape.useManualCategory)
            {
                shape.category = DetermineCategory(shape);
            }

            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(shape);
            string categoryInfo = shape.useManualCategory ? $"{shape.category} (Manual)" : $"{shape.category} (Auto)";
            Debug.Log($"[ShapeTemplateAnalyzer] Analyzed {shape.name}: {categoryInfo}, {shape.cellCount} cells, {shape.width}x{shape.height}, Rect={shape.isRectangle}, Sym={shape.isSymmetrical}");
            #endif
        }

        #if UNITY_EDITOR
        /// <summary>
        /// 编辑器模式下的静态分析方法（不需要运行时）
        /// </summary>
        /// <param name="shape">方块模板</param>
        public static void AnalyzeInEditor(ShapeTemplate shape)
        {
            if (shape == null)
            {
                Debug.LogError("[ShapeTemplateAnalyzer] ShapeTemplate is null");
                return;
            }

            // 使用静态方法执行分析
            AnalyzeStatic(shape);

            UnityEditor.EditorUtility.SetDirty(shape);
            string categoryInfo = shape.useManualCategory ? $"{shape.category} (Manual)" : $"{shape.category} (Auto)";
            Debug.Log($"[ShapeTemplateAnalyzer] (Editor Mode) Analyzed {shape.name}: {categoryInfo}, {shape.cellCount} cells, {shape.width}x{shape.height}, Rect={shape.isRectangle}, Sym={shape.isSymmetrical}");
        }

        /// <summary>
        /// 静态分析方法（供编辑器和实例共用）
        /// </summary>
        private static void AnalyzeStatic(ShapeTemplate shape)
        {
            // 计算几何属性
            shape.cellCount = CountCellsStatic(shape);
            CalculateDimensionsStatic(shape, out int width, out int height);
            shape.width = width;
            shape.height = height;
            shape.isRectangle = IsRectangleStatic(shape);
            shape.isSymmetrical = IsSymmetricalStatic(shape);

            // 确定分类（如果不是手动模式）
            if (!shape.useManualCategory)
            {
                shape.category = DetermineCategoryStatic(shape);
            }
        }
        #endif

        /// <summary>
        /// 批量分析所有ShapeTemplate资源
        /// </summary>
        public void AnalyzeAllShapes()
        {
            #if UNITY_EDITOR
            // 加载所有ShapeTemplate资源
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:ShapeTemplate");
            int count = 0;

            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                ShapeTemplate shape = UnityEditor.AssetDatabase.LoadAssetAtPath<ShapeTemplate>(path);

                if (shape != null)
                {
                    AnalyzeAndSave(shape);
                    count++;
                }
            }

            UnityEditor.AssetDatabase.SaveAssets();
            Debug.Log($"[ShapeTemplateAnalyzer] Analyzed {count} ShapeTemplates");
            #else
            Debug.LogWarning("[ShapeTemplateAnalyzer] AnalyzeAllShapes only works in Unity Editor");
            #endif
        }

        #region 几何分析方法

        /// <summary>
        /// 计算方块中填充格子的数量
        /// </summary>
        private int CountCells(ShapeTemplate shape)
        {
            int count = 0;

            if (shape.rows != null)
            {
                foreach (var row in shape.rows)
                {
                    if (row != null && row.cells != null)
                    {
                        foreach (bool cell in row.cells)
                        {
                            if (cell)
                            {
                                count++;
                            }
                        }
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// 计算方块的实际宽度和高度（忽略空白行列）
        /// </summary>
        private void CalculateDimensions(ShapeTemplate shape, out int width, out int height)
        {
            width = 0;
            height = 0;

            if (shape.rows == null || shape.rows.Length == 0)
            {
                return;
            }

            int minRow = 5, maxRow = -1;
            int minCol = 5, maxCol = -1;

            // 找到实际的边界
            for (int row = 0; row < shape.rows.Length && row < 5; row++)
            {
                if (shape.rows[row] == null || shape.rows[row].cells == null)
                    continue;

                for (int col = 0; col < shape.rows[row].cells.Length && col < 5; col++)
                {
                    if (shape.rows[row].cells[col])
                    {
                        minRow = Mathf.Min(minRow, row);
                        maxRow = Mathf.Max(maxRow, row);
                        minCol = Mathf.Min(minCol, col);
                        maxCol = Mathf.Max(maxCol, col);
                    }
                }
            }

            // 计算尺寸
            if (maxRow >= 0 && maxCol >= 0)
            {
                height = maxRow - minRow + 1;
                width = maxCol - minCol + 1;
            }
        }

        /// <summary>
        /// 判断方块是否为矩形（所有填充格子形成完整矩形）
        /// </summary>
        private bool IsRectangle(ShapeTemplate shape)
        {
            if (shape.rows == null || shape.rows.Length == 0)
            {
                return false;
            }

            // 计算边界
            int minRow = 5, maxRow = -1;
            int minCol = 5, maxCol = -1;

            for (int row = 0; row < shape.rows.Length && row < 5; row++)
            {
                if (shape.rows[row] == null || shape.rows[row].cells == null)
                    continue;

                for (int col = 0; col < shape.rows[row].cells.Length && col < 5; col++)
                {
                    if (shape.rows[row].cells[col])
                    {
                        minRow = Mathf.Min(minRow, row);
                        maxRow = Mathf.Max(maxRow, row);
                        minCol = Mathf.Min(minCol, col);
                        maxCol = Mathf.Max(maxCol, col);
                    }
                }
            }

            if (maxRow < 0 || maxCol < 0)
            {
                return false;
            }

            // 检查边界内所有格子是否都填充
            for (int row = minRow; row <= maxRow; row++)
            {
                for (int col = minCol; col <= maxCol; col++)
                {
                    if (!shape.rows[row].cells[col])
                    {
                        return false; // 有空洞,不是矩形
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 判断方块是否对称（水平或垂直对称）
        /// </summary>
        private bool IsSymmetrical(ShapeTemplate shape)
        {
            if (shape.rows == null || shape.rows.Length == 0)
            {
                return false;
            }

            // 检查水平对称
            bool horizontalSymmetry = true;
            for (int row = 0; row < 5; row++)
            {
                if (shape.rows[row] == null || shape.rows[row].cells == null)
                    continue;

                for (int col = 0; col < 5; col++)
                {
                    int mirrorCol = 4 - col;
                    bool left = shape.rows[row].cells[col];
                    bool right = shape.rows[row].cells[mirrorCol];

                    if (left != right)
                    {
                        horizontalSymmetry = false;
                        break;
                    }
                }

                if (!horizontalSymmetry)
                    break;
            }

            if (horizontalSymmetry)
            {
                return true;
            }

            // 检查垂直对称
            bool verticalSymmetry = true;
            for (int row = 0; row < 5; row++)
            {
                int mirrorRow = 4 - row;

                if (shape.rows[row] == null || shape.rows[row].cells == null)
                    continue;
                if (shape.rows[mirrorRow] == null || shape.rows[mirrorRow].cells == null)
                    continue;

                for (int col = 0; col < 5; col++)
                {
                    bool top = shape.rows[row].cells[col];
                    bool bottom = shape.rows[mirrorRow].cells[col];

                    if (top != bottom)
                    {
                        verticalSymmetry = false;
                        break;
                    }
                }

                if (!verticalSymmetry)
                    break;
            }

            return verticalSymmetry;
        }

        #endregion

        #region 静态版本的几何分析方法（供编辑器使用）

        /// <summary>
        /// 确定方块分类（静态版本）
        /// </summary>
        private static ShapeCategory DetermineCategoryStatic(ShapeTemplate shape)
        {
            int cellCount = CountCellsStatic(shape);
            bool isRect = IsRectangleStatic(shape);

            if (cellCount <= 4 && isRect)
            {
                return ShapeCategory.Basic;
            }
            else if (cellCount >= 5)
            {
                return ShapeCategory.Large;
            }
            else
            {
                return ShapeCategory.Shaped;
            }
        }

        /// <summary>
        /// 计算格子数量（静态版本）
        /// </summary>
        private static int CountCellsStatic(ShapeTemplate shape)
        {
            int count = 0;

            if (shape.rows != null)
            {
                foreach (var row in shape.rows)
                {
                    if (row != null && row.cells != null)
                    {
                        foreach (bool cell in row.cells)
                        {
                            if (cell)
                            {
                                count++;
                            }
                        }
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// 计算尺寸（静态版本）
        /// </summary>
        private static void CalculateDimensionsStatic(ShapeTemplate shape, out int width, out int height)
        {
            width = 0;
            height = 0;

            if (shape.rows == null || shape.rows.Length == 0)
            {
                return;
            }

            int minRow = 5, maxRow = -1;
            int minCol = 5, maxCol = -1;

            for (int row = 0; row < shape.rows.Length && row < 5; row++)
            {
                if (shape.rows[row] == null || shape.rows[row].cells == null)
                    continue;

                for (int col = 0; col < shape.rows[row].cells.Length && col < 5; col++)
                {
                    if (shape.rows[row].cells[col])
                    {
                        minRow = Mathf.Min(minRow, row);
                        maxRow = Mathf.Max(maxRow, row);
                        minCol = Mathf.Min(minCol, col);
                        maxCol = Mathf.Max(maxCol, col);
                    }
                }
            }

            if (maxRow >= 0 && maxCol >= 0)
            {
                height = maxRow - minRow + 1;
                width = maxCol - minCol + 1;
            }
        }

        /// <summary>
        /// 判断是否矩形（静态版本）
        /// </summary>
        private static bool IsRectangleStatic(ShapeTemplate shape)
        {
            if (shape.rows == null || shape.rows.Length == 0)
            {
                return false;
            }

            int minRow = 5, maxRow = -1;
            int minCol = 5, maxCol = -1;

            for (int row = 0; row < shape.rows.Length && row < 5; row++)
            {
                if (shape.rows[row] == null || shape.rows[row].cells == null)
                    continue;

                for (int col = 0; col < shape.rows[row].cells.Length && col < 5; col++)
                {
                    if (shape.rows[row].cells[col])
                    {
                        minRow = Mathf.Min(minRow, row);
                        maxRow = Mathf.Max(maxRow, row);
                        minCol = Mathf.Min(minCol, col);
                        maxCol = Mathf.Max(maxCol, col);
                    }
                }
            }

            if (maxRow < 0 || maxCol < 0)
            {
                return false;
            }

            for (int row = minRow; row <= maxRow; row++)
            {
                for (int col = minCol; col <= maxCol; col++)
                {
                    if (!shape.rows[row].cells[col])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 判断是否对称（静态版本）
        /// </summary>
        private static bool IsSymmetricalStatic(ShapeTemplate shape)
        {
            if (shape.rows == null || shape.rows.Length == 0)
            {
                return false;
            }

            // 检查水平对称
            bool horizontalSymmetry = true;
            for (int row = 0; row < 5; row++)
            {
                if (shape.rows[row] == null || shape.rows[row].cells == null)
                    continue;

                for (int col = 0; col < 5; col++)
                {
                    int mirrorCol = 4 - col;
                    bool left = shape.rows[row].cells[col];
                    bool right = shape.rows[row].cells[mirrorCol];

                    if (left != right)
                    {
                        horizontalSymmetry = false;
                        break;
                    }
                }

                if (!horizontalSymmetry)
                    break;
            }

            if (horizontalSymmetry)
            {
                return true;
            }

            // 检查垂直对称
            bool verticalSymmetry = true;
            for (int row = 0; row < 5; row++)
            {
                int mirrorRow = 4 - row;

                if (shape.rows[row] == null || shape.rows[row].cells == null)
                    continue;
                if (shape.rows[mirrorRow] == null || shape.rows[mirrorRow].cells == null)
                    continue;

                for (int col = 0; col < 5; col++)
                {
                    bool top = shape.rows[row].cells[col];
                    bool bottom = shape.rows[mirrorRow].cells[col];

                    if (top != bottom)
                    {
                        verticalSymmetry = false;
                        break;
                    }
                }

                if (!verticalSymmetry)
                    break;
            }

            return verticalSymmetry;
        }

        #endregion
    }
}
