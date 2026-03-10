// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using BlockPuzzleGameToolkit.Scripts.LevelsData;
using GameCore.DifficultySystem;

namespace GameCore.DifficultySystem.Editor
{
    /// <summary>
    /// 批量计算关卡难度的编辑器窗口
    /// 提供批量扫描和计算功能，显示统计报告
    /// </summary>
    public class DifficultyCalculatorWindow : EditorWindow
    {
        private List<Level> allLevels = new List<Level>();
        private List<ShapeTemplate> allShapes = new List<ShapeTemplate>();
        private Vector2 scrollPosition;

        // 统计数据
        private Dictionary<DifficultyLevel, int> levelDistribution = new Dictionary<DifficultyLevel, int>();
        private Dictionary<ShapeCategory, int> shapeDistribution = new Dictionary<ShapeCategory, int>();

        private bool isCalculating = false;
        private float progress = 0f;

        [MenuItem("Tools/Difficulty System/Batch Calculator")]
        public static void ShowWindow()
        {
            var window = GetWindow<DifficultyCalculatorWindow>("批量计算器");
            window.minSize = new Vector2(500, 600);
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // 标题
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
            titleStyle.fontSize = 16;
            titleStyle.alignment = TextAnchor.MiddleCenter;
            EditorGUILayout.LabelField("关卡难度批量计算器", titleStyle);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            // 关卡部分
            DrawLevelSection();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            // 方块部分
            DrawShapeSection();

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// 绘制关卡部分
        /// </summary>
        private void DrawLevelSection()
        {
            EditorGUILayout.LabelField("关卡难度计算 (Level Difficulty)", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // 扫描按钮
            if (GUILayout.Button("扫描所有关卡 (Scan All Levels)", GUILayout.Height(30)))
            {
                ScanAllLevels();
            }

            // 显示找到的关卡数量
            if (allLevels.Count > 0)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField($"找到关卡: {allLevels.Count} 个", EditorStyles.boldLabel);

                EditorGUILayout.Space(5);

                // 批量计算按钮
                GUI.enabled = !isCalculating;
                if (GUILayout.Button("批量计算难度 (Batch Calculate)", GUILayout.Height(30)))
                {
                    BatchCalculateLevels();
                }
                GUI.enabled = true;

                // 进度条
                if (isCalculating)
                {
                    EditorGUILayout.Space(5);
                    EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(false, 20), progress, $"计算中... {(int)(progress * allLevels.Count)}/{allLevels.Count}");
                }

                // 统计报告
                if (levelDistribution.Count > 0)
                {
                    EditorGUILayout.Space(10);
                    DrawLevelStatistics();
                }
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制方块部分
        /// </summary>
        private void DrawShapeSection()
        {
            EditorGUILayout.LabelField("方块模板分析 (Shape Template Analysis)", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // 扫描按钮
            if (GUILayout.Button("扫描所有方块 (Scan All Shapes)", GUILayout.Height(30)))
            {
                ScanAllShapes();
            }

            // 显示找到的方块数量
            if (allShapes.Count > 0)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField($"找到方块: {allShapes.Count} 个", EditorStyles.boldLabel);

                EditorGUILayout.Space(5);

                // 批量分析按钮
                GUI.enabled = !isCalculating;
                if (GUILayout.Button("批量分析方块 (Batch Analyze)", GUILayout.Height(30)))
                {
                    BatchAnalyzeShapes();
                }
                GUI.enabled = true;

                // 统计报告
                if (shapeDistribution.Count > 0)
                {
                    EditorGUILayout.Space(10);
                    DrawShapeStatistics();
                }
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 扫描所有关卡（仅扫描Resources/Levels文件夹）
        /// </summary>
        private void ScanAllLevels()
        {
            allLevels.Clear();

            // 使用AssetDatabase查找所有Level资源
            string[] guids = AssetDatabase.FindAssets("t:Level");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                // 只扫描 Resources/Levels 文件夹下的关卡
                if (!path.Contains("Resources/Levels"))
                {
                    continue;
                }

                Level level = AssetDatabase.LoadAssetAtPath<Level>(path);

                if (level != null)
                {
                    allLevels.Add(level);
                }
            }

            // 按名称排序
            allLevels.Sort((a, b) => a.name.CompareTo(b.name));

            Debug.Log($"[DifficultyCalculatorWindow] 在 Resources/Levels 文件夹中找到 {allLevels.Count} 个关卡");
        }

        /// <summary>
        /// 批量计算关卡难度
        /// </summary>
        private void BatchCalculateLevels()
        {
            isCalculating = true;
            levelDistribution.Clear();

            // 初始化统计
            foreach (DifficultyLevel level in System.Enum.GetValues(typeof(DifficultyLevel)))
            {
                levelDistribution[level] = 0;
            }

            // 批量计算
            for (int i = 0; i < allLevels.Count; i++)
            {
                progress = (float)i / allLevels.Count;

                Level level = allLevels[i];

                // 根据是否运行模式选择不同的计算方式
                if (Application.isPlaying)
                {
                    var calculator = LevelDifficultyCalculator.Instance;
                    if (calculator != null)
                    {
                        calculator.CalculateAndSave(level);
                    }
                    else
                    {
                        Debug.LogWarning($"[DifficultyCalculatorWindow] Calculator not available in play mode, using editor mode for {level.name}");
                        LevelDifficultyCalculator.CalculateInEditor(level);
                    }
                }
                else
                {
                    // 编辑器模式：使用静态方法
                    LevelDifficultyCalculator.CalculateInEditor(level);
                }

                // 统计
                levelDistribution[level.difficultyLevel]++;

                // 更新进度条
                if (i % 10 == 0)
                {
                    Repaint();
                }
            }

            progress = 1f;
            isCalculating = false;

            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("完成", $"成功计算 {allLevels.Count} 个关卡的难度", "确定");

            Debug.Log($"[DifficultyCalculatorWindow] 批量计算完成: {allLevels.Count} 个关卡");
        }

        /// <summary>
        /// 扫描所有方块
        /// </summary>
        private void ScanAllShapes()
        {
            allShapes.Clear();

            // 使用AssetDatabase查找所有ShapeTemplate资源
            string[] guids = AssetDatabase.FindAssets("t:ShapeTemplate");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ShapeTemplate shape = AssetDatabase.LoadAssetAtPath<ShapeTemplate>(path);

                if (shape != null)
                {
                    allShapes.Add(shape);
                }
            }

            // 按名称排序
            allShapes.Sort((a, b) => a.name.CompareTo(b.name));

            Debug.Log($"[DifficultyCalculatorWindow] 找到 {allShapes.Count} 个方块模板");
        }

        /// <summary>
        /// 批量分析方块
        /// </summary>
        private void BatchAnalyzeShapes()
        {
            isCalculating = true;
            shapeDistribution.Clear();

            // 初始化统计
            foreach (ShapeCategory category in System.Enum.GetValues(typeof(ShapeCategory)))
            {
                shapeDistribution[category] = 0;
            }

            // 批量分析
            for (int i = 0; i < allShapes.Count; i++)
            {
                progress = (float)i / allShapes.Count;

                ShapeTemplate shape = allShapes[i];

                // 根据是否运行模式选择不同的分析方式
                if (Application.isPlaying)
                {
                    var analyzer = ShapeTemplateAnalyzer.Instance;
                    if (analyzer != null)
                    {
                        analyzer.AnalyzeAndSave(shape);
                    }
                    else
                    {
                        Debug.LogWarning($"[DifficultyCalculatorWindow] Analyzer not available in play mode, using editor mode for {shape.name}");
                        ShapeTemplateAnalyzer.AnalyzeInEditor(shape);
                    }
                }
                else
                {
                    // 编辑器模式：使用静态方法
                    ShapeTemplateAnalyzer.AnalyzeInEditor(shape);
                }

                // 统计
                shapeDistribution[shape.category]++;

                // 更新进度条
                if (i % 10 == 0)
                {
                    Repaint();
                }
            }

            progress = 1f;
            isCalculating = false;

            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("完成", $"成功分析 {allShapes.Count} 个方块模板", "确定");

            Debug.Log($"[DifficultyCalculatorWindow] 批量分析完成: {allShapes.Count} 个方块模板");
        }

        /// <summary>
        /// 绘制关卡统计信息
        /// </summary>
        private void DrawLevelStatistics()
        {
            EditorGUILayout.LabelField("统计报告 (Statistics)", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            int total = allLevels.Count;

            foreach (DifficultyLevel level in System.Enum.GetValues(typeof(DifficultyLevel)))
            {
                if (levelDistribution.ContainsKey(level))
                {
                    int count = levelDistribution[level];
                    float percentage = total > 0 ? (float)count / total * 100f : 0f;

                    EditorGUILayout.BeginHorizontal();

                    // 难度等级名称
                    string levelName = GetDifficultyLevelShortName(level);
                    EditorGUILayout.LabelField(levelName, GUILayout.Width(100));

                    // 进度条
                    Rect rect = EditorGUILayout.GetControlRect(false, 18);
                    EditorGUI.ProgressBar(rect, percentage / 100f, "");

                    // 数量和百分比
                    EditorGUILayout.LabelField($"{count} ({percentage:F0}%)", GUILayout.Width(80));

                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制方块统计信息
        /// </summary>
        private void DrawShapeStatistics()
        {
            EditorGUILayout.LabelField("统计报告 (Statistics)", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            int total = allShapes.Count;

            foreach (ShapeCategory category in System.Enum.GetValues(typeof(ShapeCategory)))
            {
                if (shapeDistribution.ContainsKey(category))
                {
                    int count = shapeDistribution[category];
                    float percentage = total > 0 ? (float)count / total * 100f : 0f;

                    EditorGUILayout.BeginHorizontal();

                    // 分类名称
                    string categoryName = GetCategoryShortName(category);
                    EditorGUILayout.LabelField(categoryName, GUILayout.Width(100));

                    // 进度条
                    Rect rect = EditorGUILayout.GetControlRect(false, 18);
                    Color barColor = GetCategoryColor(category);
                    EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width * (percentage / 100f), rect.height), barColor);
                    GUI.Box(rect, "", EditorStyles.helpBox);

                    // 数量和百分比
                    EditorGUILayout.LabelField($"{count} ({percentage:F0}%)", GUILayout.Width(80));

                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 获取难度等级的简短名称
        /// </summary>
        private string GetDifficultyLevelShortName(DifficultyLevel level)
        {
            switch (level)
            {
                case DifficultyLevel.Tutorial: return "Tutorial";
                case DifficultyLevel.Easy: return "Easy";
                case DifficultyLevel.Normal: return "Normal";
                case DifficultyLevel.Hard: return "Hard";
                case DifficultyLevel.Expert: return "Expert";
                case DifficultyLevel.Master: return "Master";
                default: return "Unknown";
            }
        }

        /// <summary>
        /// 获取分类的简短名称
        /// </summary>
        private string GetCategoryShortName(ShapeCategory category)
        {
            switch (category)
            {
                case ShapeCategory.Basic: return "Basic (基础块)";
                case ShapeCategory.Shaped: return "Shaped (异形块)";
                case ShapeCategory.Large: return "Large (大块)";
                default: return "Unknown";
            }
        }

        /// <summary>
        /// 获取分类的颜色
        /// </summary>
        private Color GetCategoryColor(ShapeCategory category)
        {
            switch (category)
            {
                case ShapeCategory.Basic: return new Color(0.3f, 0.7f, 1f);
                case ShapeCategory.Shaped: return new Color(1f, 0.9f, 0.3f);
                case ShapeCategory.Large: return new Color(1f, 0.4f, 0.4f);
                default: return Color.white;
            }
        }
    }
}
#endif
