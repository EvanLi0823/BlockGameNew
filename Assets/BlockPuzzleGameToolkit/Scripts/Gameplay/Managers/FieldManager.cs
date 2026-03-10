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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BlockPuzzleGameToolkit.Scripts.LevelsData;
using UnityEngine;
using UnityEngine.UI;

namespace BlockPuzzleGameToolkit.Scripts.Gameplay
{
    /// <summary>
    /// FieldManager - 棋盘管理器
    /// 负责管理游戏棋盘，包括生成棋盘、检测消除、判断放置等核心功能
    /// 维护所有Cell的二维数组结构
    /// </summary>
    public class FieldManager : MonoBehaviour
    {
        /// <summary>
        /// 棋盘的RectTransform组件
        /// </summary>
        public RectTransform field;

        /// <summary>
        /// Cell格子的预制体
        /// </summary>
        public Cell prefab;

        /// <summary>
        /// 棋盘格子的二维数组
        /// 存储所有Cell的引用，[行,列]索引
        /// </summary>
        public Cell[,] cells;

        /// <summary>
        /// 连消时显示的边框特效
        /// </summary>
        public RectTransform outline;

        /// <summary>
        /// 物品工厂的引用
        /// </summary>
        [SerializeField]
        private ItemFactory itemFactory;

        /// <summary>
        /// 单个格子的尺寸（像素）
        /// </summary>
        private float _cellSize;

        /// <summary>
        /// 生成棋盘 - 根据关卡数据创建棋盘
        /// 这是棋盘初始化的主入口
        /// </summary>
        /// <param name="level">关卡数据</param>
        public void Generate(Level level)
        {
            var oneColorMode = level.levelType.singleColorMode;

            if (level == null)
            {
                Debug.LogError("Attempted to generate field with null level");
                return;
            }

            GenerateField(level.rows, level.columns);

            for (var i = 0; i < level.rows; i++)
            {
                for (var j = 0; j < level.columns; j++)
                {
                    var item = level.GetItem(i, j);
                    if (item != null)
                    {
                        cells[i, j].FillCell(item);
                    }

                    var bonus = false;
                    if (level.levelRows[i].bonusItems[j])
                    {
                        var bonusItemTemplates = level.targetInstance.Where(t => t.amount > 0 && t.targetScriptable.bonusItem != null).Select(t => t.targetScriptable.bonusItem).ToArray();
                        if (bonusItemTemplates.Length > 0)
                        {
                            cells[i, j].SetBonus(bonusItemTemplates[Random.Range(0, bonusItemTemplates.Length)]);
                            bonus = true;
                        }
                    }

                    if (item != null && oneColorMode && !bonus)
                    {
                        cells[i, j].FillCell(itemFactory.GetColor());
                    }

                    // Disable cell if it is marked as disabled in the level data
                    if (level.IsDisabled(i, j))
                    {
                        cells[i, j].DisableCell();
                    }

                    // Highlight cell if it is marked as highlighted in the level data
                    if (level.IsCellHighlighted(i, j))
                    {
                        cells[i, j].HighlightCellTutorial();
                    }
                }
            }
        }

        /// <summary>
        /// 生成棋盘格子矩阵
        /// 创建指定行列数的Cell格子
        /// </summary>
        /// <param name="rows">行数</param>
        /// <param name="columns">列数</param>
        private void GenerateField(int rows, int columns)
        {
            // 清除旧的格子
            foreach (Transform child in field)
            {
                Destroy(child.gameObject);
            }

            // 创建新的二维数组
            cells = new Cell[rows, columns];

            // 配置GridLayoutGroup组件
            var gridLayout = field.GetComponent<GridLayoutGroup>();
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = columns;

            // 计算格子尺寸以适配棋盘大小
            var totalMargin = gridLayout.padding.left + gridLayout.padding.right;
            var spacing = gridLayout.spacing;
            var availableWidth = field.rect.width - totalMargin;
            var availableHeight = field.rect.height - (gridLayout.padding.top + gridLayout.padding.bottom);

            // 根据宽高计算最合适的格子尺寸
            var cellSizeFromWidth = (availableWidth - (spacing.x * (columns - 1))) / columns;
            var cellSizeFromHeight = (availableHeight - (spacing.y * (rows - 1))) / rows;
            var cellSize = Mathf.Min(cellSizeFromWidth, cellSizeFromHeight);

            gridLayout.cellSize = new Vector2(cellSize, cellSize);

            // 创建所有格子
            for (var i = 0; i < rows; i++)
            {
                for (var j = 0; j < columns; j++)
                {
                    var cell = Instantiate(prefab, field);
                    cells[i, j] = cell;
                    cell.name = $"Cell {i}, {j}";
                    cell.InitItem();
                }
            }

            _cellSize = cellSize;

            // 延迟调整棋盘大小
            StartCoroutine(ResizeFieldWithDelay(rows, columns, cellSize, gridLayout));
        }

        /// <summary>
        /// 延迟调整棋盘大小
        /// 等待布局系统计算完成后调整
        /// </summary>
        private IEnumerator ResizeFieldWithDelay(int rows, int columns, float cellSize, GridLayoutGroup gridLayout)
        {
            // 等待一帧确保GridLayout计算完成
            yield return new WaitForFixedUpdate();

            // 根据实际布局计算精确尺寸
            var spacing = gridLayout.spacing;
            var padding = gridLayout.padding;
            float width = (cellSize * columns) + (spacing.x * (columns - 1)) + padding.left + padding.right;
            float height = (cellSize * rows) + (spacing.y * (rows - 1)) + padding.top + padding.bottom;

            // 调整棋盘大小以完全匹配格子布局
            GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
        }

        /// <summary>
        /// 从保存的状态恢复棋盘
        /// 用于继续之前的游戏
        /// </summary>
        /// <param name="levelRows">保存的关卡行数据</param>
        public void RestoreFromState(LevelRow[] levelRows)
        {
            //restore score
            if (levelRows == null) return;
            GenerateField(levelRows.Length, levelRows[0].cells.Length);
            for (var i = 0; i < levelRows.Length; i++)
            {
                for (var j = 0; j < levelRows[i].cells.Length; j++)
                {
                    var item = levelRows[i].cells[j];
                    if (item != null)
                    {
                        cells[i,j].FillCell(item);
                    }

                    if (levelRows[i].disabled[j])
                    {
                        cells[i,j].DisableCell();
                    }
                }
            }
        }

        /// <summary>
        /// 获取所有填满的行和列
        /// 这是消除检测的核心方法
        /// </summary>
        /// <param name="preview">是否是预览模式（用于拖拽时的预判）</param>
        /// <param name="merge">是否合并行列（未使用）</param>
        /// <returns>所有填满的行列的Cell列表</returns>
        public List<List<Cell>> GetFilledLines(bool preview = false, bool merge = true)
        {
            // 分别获取水平和垂直的填满行列
            var horizontalLines = GetFilledLinesHorizontal(preview);
            var verticalLines = GetFilledLinesVertical(preview);

            // 合并结果
            var lines = new List<List<Cell>>();
            lines.AddRange(horizontalLines);
            lines.AddRange(verticalLines);
            return lines;
        }

        /// <summary>
        /// 获取水平方向填满的行
        /// </summary>
        /// <param name="preview">是否是预览模式</param>
        /// <returns>填满的行的Cell列表</returns>
        public List<List<Cell>> GetFilledLinesHorizontal(bool preview)
        {
            var lines = new List<List<Cell>>();
            // 遍历每一行
            for (var i = 0; i < cells.GetLength(0); i++)
            {
                var isLineFilled = true;
                var line = new List<Cell>();
                // 检查该行的每个格子
                for (var j = 0; j < cells.GetLength(1); j++)
                {
                    if (cells[i, j].IsEmpty(preview))
                    {
                        isLineFilled = false;  // 有空格子，该行未填满
                        break;
                    }

                    line.Add(cells[i, j]);
                }

                // 如果整行都填满，添加到结果
                if (isLineFilled)
                {
                    lines.Add(line);
                }
            }

            return lines;
        }

        /// <summary>
        /// 获取垂直方向填满的列
        /// </summary>
        /// <param name="preview">是否是预览模式</param>
        /// <returns>填满的列的Cell列表</returns>
        public List<List<Cell>> GetFilledLinesVertical(bool preview)
        {
            var lines = new List<List<Cell>>();
            // 遍历每一列
            for (var i = 0; i < cells.GetLength(1); i++)
            {
                var isLineFilled = true;
                var line = new List<Cell>();
                // 检查该列的每个格子
                for (var j = 0; j < cells.GetLength(0); j++)
                {
                    if (cells[j, i].IsEmpty(preview))
                    {
                        isLineFilled = false;  // 有空格子，该列未填满
                        break;
                    }

                    line.Add(cells[j, i]);
                }

                // 如果整列都填满，添加到结果
                if (isLineFilled)
                {
                    lines.Add(line);
                }
            }

            return lines;
        }

        /// <summary>
        /// 检查形状是否能放置到棋盘上
        /// 遍历所有可能的位置检查是否有合法位置
        /// </summary>
        /// <param name="shape">要检查的形状</param>
        /// <returns>是否能放置</returns>
        public bool CanPlaceShape(Shape shape)
        {
            if (cells == null)
            {
                return false;
            }

            var activeItems = shape.GetActiveItems();
            int minX = int.MaxValue, minY = int.MaxValue;
            int maxX = int.MinValue, maxY = int.MinValue;

            // 计算形状的边界框
            foreach (var item in activeItems)
            {
                var pos = item.GetPosition();
                minX = Mathf.Min(minX, pos.x);
                minY = Mathf.Min(minY, pos.y);
                maxX = Mathf.Max(maxX, pos.x);
                maxY = Mathf.Max(maxY, pos.y);
            }

            var shapeWidth = maxX - minX + 1;
            var shapeHeight = maxY - minY + 1;

            // 尝试在棋盘的每个可能位置放置形状
            for (var fieldY = 0; fieldY <= cells.GetLength(0) - shapeHeight; fieldY++)
            {
                for (var fieldX = 0; fieldX <= cells.GetLength(1) - shapeWidth; fieldX++)
                {
                    // 检查在该位置是否可以放置
                    if (CanPlaceShapeAt(activeItems, fieldX - minX, fieldY - minY))
                    {
                        return true;  // 找到一个可放置的位置
                    }
                }
            }

            return false;  // 没有找到可放置的位置
        }

        /// <summary>
        /// 获取棋盘中心格子
        /// </summary>
        /// <returns>中心位置的Cell</returns>
        public Cell GetCenterCell()
        {
            var x = cells.GetLength(1) / 2;
            var y = cells.GetLength(0) / 2;
            return cells[y, x];
        }

        /// <summary>
        /// 检查形状是否能放置在指定位置
        /// </summary>
        /// <param name="items">形状的所有Item</param>
        /// <param name="offsetX">X偏移量</param>
        /// <param name="offsetY">Y偏移量</param>
        /// <returns>是否能放置</returns>
        private bool CanPlaceShapeAt(List<Item> items, int offsetX, int offsetY)
        {
            foreach (var item in items)
            {
                var pos = item.GetPosition();
                var x = offsetX + pos.x;
                var y = offsetY + pos.y;

                // 检查是否越界
                if (x < 0 || x >= cells.GetLength(1) || y < 0 || y >= cells.GetLength(0))
                {
                    return false;  // 越界
                }

                // 检查格子是否已被占用
                if (!cells[y, x].IsEmpty() && cells[y, x].busy)
                {
                    return false;  // 格子已被占用
                }
            }

            return true;  // 所有格子都可以放置
        }

        /// <summary>
        /// 显示或隐藏连消边框特效
        /// </summary>
        /// <param name="show">是否显示</param>
        public void ShowOutline(bool show)
        {
            // 设置边框的内边距
            var paddingX = 0.033f;
            var paddingY = 0.033f;

            // 同步边框位置和大小
            outline.anchoredPosition = field.anchoredPosition;
            outline.sizeDelta = field.sizeDelta;
            outline.anchorMin = new Vector2(field.anchorMin.x - paddingX, field.anchorMin.y - paddingY);
            outline.anchorMax = new Vector2(field.anchorMax.x + paddingX, field.anchorMax.y + paddingY);
            outline.pivot = field.pivot;
            outline.gameObject.SetActive(show);
        }

        /// <summary>
        /// 获取所有格子
        /// </summary>
        /// <returns>格子二维数组</returns>
        public Cell[,] GetAllCells()
        {
            return cells;
        }

        /// <summary>
        /// 获取指定行的所有格子
        /// </summary>
        /// <param name="i">行索引</param>
        /// <returns>该行的Cell列表</returns>
        public List<List<Cell>> GetRow(int i)
        {
            var row = new List<List<Cell>>();
            for (var j = 0; j < cells.GetLength(1); j++)
            {
                row.Add(new List<Cell> { cells[i, j] });
            }

            return row;
        }

        /// <summary>
        /// 获取所有空格子
        /// </summary>
        /// <returns>空格子数组</returns>
        public Cell[] GetEmptyCells()
        {
            return cells.Cast<Cell>().Where(cell => !cell.busy).ToArray();
        }

        /// <summary>
        /// 获取格子尺寸
        /// </summary>
        /// <returns>格子的像素尺寸</returns>
        public float GetCellSize()
        {
            return _cellSize;
        }

        /// <summary>
        /// 获取教学模式高亮的格子
        /// </summary>
        /// <returns>高亮的格子列表</returns>
        public List<Cell> GetTutorialLine()
        {
            var line = new List<Cell>();
            for (var i = 0; i < cells.GetLength(0); i++)
            {
                for (var j = 0; j < cells.GetLength(1); j++)
                {
                    if (cells[i, j].IsHighlighted())
                    {
                        line.Add(cells[i, j]);
                    }
                }
            }

            return line;
        }
    }
}