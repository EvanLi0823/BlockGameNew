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
using GameCore.DifficultySystem;

namespace BlockPuzzleGameToolkit.Scripts.LevelsData
{
    [Serializable]
    public class LevelRow
    {
        public ItemTemplate[] cells;
        public bool[] bonusItems; // bonus item with diamonds
        public bool[] disabled; // disabled collider
        public bool[] highlighted; // highlighted cell for tutorial

        public LevelRow(int size)
        {
            cells = new ItemTemplate[size];
            bonusItems = new bool[size];
            disabled = new bool[size];
            highlighted = new bool[size];
        }
    }

    /// <summary>
    /// 奖励道具配置 - 指定类型和数量
    /// </summary>
    [Serializable]
    public class BonusItemConfig
    {
        [Tooltip("奖励道具模板")]
        public BonusItemTemplate bonusItem;

        [Tooltip("该类型奖励道具的数量")]
        [Range(1, 10)]
        public int count = 1;
    }

    /// <summary>
    /// 单个槽位的完整配置 - 形状、颜色、BonusItem
    /// </summary>
    [Serializable]
    public class ShapeSlotConfig
    {
        [Tooltip("方块模板，null表示随机")]
        public ShapeTemplate shape;

        [Tooltip("颜色模板，null表示随机")]
        public ItemTemplate color;

        [Tooltip("该方块的奖励道具配置列表")]
        public List<BonusItemConfig> bonusItems = new List<BonusItemConfig>();
    }

    /// <summary>
    /// 初始刷新配置 - 包含3个槽位
    /// </summary>
    [Serializable]
    public class InitialShapeRefresh
    {
        [Tooltip("3个槽位的配置")]
        public ShapeSlotConfig[] slots = new ShapeSlotConfig[3];

        /// <summary>
        /// 构造函数 - 初始化3个空槽位
        /// </summary>
        public InitialShapeRefresh()
        {
            slots = new ShapeSlotConfig[3];
            for (int i = 0; i < 3; i++)
            {
                slots[i] = new ShapeSlotConfig();
            }
        }
    }

    [CreateAssetMenu(fileName = "Level", menuName = "BlockPuzzleGameToolkit/Levels/Level", order = 1)]
    public class Level : ScriptableObject
    {
        public int rows = 8;
        public int columns = 8;
        public LevelRow[] levelRows;
        public LevelTypeScriptable levelType;
        public bool enableTimer = false;
        public int timerDuration = 120;

        [Header("Reward Configuration")]
        [SerializeField]
        public LevelRewardConfig rewardConfig;

        [SerializeField]
        public Dictionary<Color, int> bonusItemColors;

        [SerializeField]
        public List<Target> targetInstance = new();

        public float emptyCellPercentage = 10f;

        [Header("Initial Shape Configuration")]
        [Tooltip("配置初始几次刷新的方块类型，每项包含3个方块。不配置则使用随机生成")]
        public List<InitialShapeRefresh> initialShapeRefreshes = new();

        // ===== 新增字段 (难度评估结果) =====
        [Header("Difficulty Analysis (Auto-Calculated)")]
        [Tooltip("是否手动设置难度（否则使用自动计算）")]
        public bool manualDifficultyOverride = false;

        [Tooltip("Overall difficulty score (0-100)")]
        public float difficultyScore;

        [Tooltip("Difficulty level classification (Auto-calculated, read-only)")]
        public DifficultyLevel difficultyLevel;

        [Tooltip("Breakdown of difficulty dimensions")]
        public DifficultyBreakdown breakdown = new DifficultyBreakdown();

        [Tooltip("Preview of dynamic difficulty control")]
        public DynamicDifficultyPreview dynamicPreview = new DynamicDifficultyPreview();

        [Header("Shape Generation Config (方块生成配置)")]
        [Tooltip("方块权重等级（控制运行时方块生成）")]
        public DifficultyLevel shapeWeightLevel = DifficultyLevel.Normal;

        [Header("Custom Shape Probabilities (Optional)")]
        [Tooltip("启用自定义方块生成权重（覆盖全局配置）")]
        public bool useCustomProbabilities = false;

        [Tooltip("自定义方块概率配置")]
        public CategoryProbabilities customBaseline = new CategoryProbabilities { basic = 0.5f, shaped = 0.35f, large = 0.15f };

        public int Number => GetLevelNum();


        private void OnEnable()
        {
            InitializeIfNeeded();
        }

        public void InitializeIfNeeded()
        {
            if (levelRows == null || levelRows.Length != rows)
            {
                levelRows = new LevelRow[rows];
                for (var i = 0; i < rows; i++)
                {
                    levelRows[i] = new LevelRow(columns);
                }
            }

            // 初始化奖励配置
            if (rewardConfig == null)
            {
                rewardConfig = LevelRewardConfig.CreateDefault();
            }
        }

        public ItemTemplate GetItem(int row, int column)
        {
            if (row >= 0 && row < rows && column >= 0 && column < columns)
            {
                return levelRows[row].cells[column];
            }

            return null;
        }

        public void SetBonus(int row, int column, bool bonus)
        {
            if (row >= 0 && row < rows && column >= 0 && column < columns)
            {
                levelRows[row].bonusItems[column] = bonus;
            }
        }

        public void Resize(int newRows, int newColumns)
        {
            var newLevelRows = new LevelRow[newRows];
            for (var i = 0; i < newRows; i++)
            {
                newLevelRows[i] = new LevelRow(newColumns);
            }

            rows = newRows;
            columns = newColumns;
            levelRows = newLevelRows;
        }

        private int GetLevelNum()
        {
            var levelName = name;

            // 使用传统循环提取数字字符
            var numericChars = new System.Text.StringBuilder();
            foreach (char c in levelName)
            {
                if (char.IsDigit(c))
                {
                    numericChars.Append(c);
                }
            }

            if (int.TryParse(numericChars.ToString(), out var levelNum))
            {
                return levelNum;
            }

            Debug.LogWarning("Unable to parse the numeric part from the level name.");
            return -1;
        }

        public bool GetBonus(int row, int col)
        {
            if (row >= 0 && row < rows && col >= 0 && col < columns)
            {
                return levelRows[row].bonusItems[col];
            }

            return false;
        }

        public void UpdateTargets()
        {
            targetInstance.Clear();
            foreach (var targetScriptable in levelType.targets)
            {
                targetInstance.Add(new Target(targetScriptable));
            }
        }

        public void SetItem(int row, int column, ItemTemplate item)
        {
            if (row >= 0 && row < rows && column >= 0 && column < columns)
            {
                levelRows[row].cells[column] = item;
            }

            if (item == null)
            {
                SetBonus(row, column, false);
            }
        }

        public bool IsDisabled(int row, int column)
        {
            if (row >= 0 && row < rows && column >= 0 && column < columns)
            {
                if (levelRows[row].disabled == null || column >= levelRows[row].disabled.Length)
                {
                    return false;
                }

                return levelRows[row].disabled[column];
            }

            return false;
        }

        public void DisableCellToggle(int row, int column)
        {
            if (row >= 0 && row < rows && column >= 0 && column < columns)
            {
                if (levelRows[row].disabled == null || column >= levelRows[row].disabled.Length)
                {
                    levelRows[row].disabled = new bool[columns];
                }

                levelRows[row].disabled[column] = !levelRows[row].disabled[column];
            }
        }

        //for tutorial
        public void HighlightCellToggle(int row, int column)
        {
            if (row >= 0 && row < rows && column >= 0 && column < columns)
            {
                if (levelRows[row].highlighted == null || column >= levelRows[row].highlighted.Length)
                {
                    levelRows[row].highlighted = new bool[columns];
                }

                levelRows[row].highlighted[column] = !levelRows[row].highlighted[column];
            }
        }

        public bool IsCellHighlighted(int row, int column)
        {
            if (row >= 0 && row < rows && column >= 0 && column < columns)
            {
                if (levelRows[row].highlighted == null || column >= levelRows[row].highlighted.Length)
                {
                    return false;
                }

                return levelRows[row].highlighted[column];
            }

            return false;
        }
    }
}