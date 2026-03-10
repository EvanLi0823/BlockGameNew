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
using System.Linq;
using BlockPuzzleGameToolkit.Scripts.LevelsData;

namespace GameCore.DifficultySystem.Editor
{
    /// <summary>
    /// 难度曲线可视化工具
    /// 提供关卡难度曲线图表、异常检测、平滑处理等功能
    /// </summary>
    public class DifficultyChartWindow : EditorWindow
    {
        private List<Level> allLevels = new List<Level>();
        private Vector2 scrollPosition;

        // 图表设置
        private float chartWidth = 800f;
        private float chartHeight = 400f;
        private bool showDifficultyLines = true;
        private bool showAnomalies = true;
        private bool showStatistics = true;

        // 异常检测设置
        private float jumpThreshold = 15f; // 难度跳跃阈值
        private List<int> anomalyIndices = new List<int>();

        // 筛选设置
        private int startLevel = 1;
        private int endLevel = 200;

        [MenuItem("Tools/Difficulty System/Difficulty Chart")]
        public static void ShowWindow()
        {
            var window = GetWindow<DifficultyChartWindow>("难度曲线图");
            window.minSize = new Vector2(900, 700);
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // 标题
            DrawTitle();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            // 控制面板
            DrawControlPanel();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            // 图表
            if (allLevels.Count > 0)
            {
                DrawChart();

                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                // 统计信息
                if (showStatistics)
                {
                    DrawStatistics();
                }

                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                // 异常列表
                if (showAnomalies && anomalyIndices.Count > 0)
                {
                    DrawAnomaliesList();
                }
            }

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// 绘制标题
        /// </summary>
        private void DrawTitle()
        {
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
            titleStyle.fontSize = 16;
            titleStyle.alignment = TextAnchor.MiddleCenter;
            EditorGUILayout.LabelField("关卡难度曲线可视化", titleStyle);
        }

        /// <summary>
        /// 绘制控制面板
        /// </summary>
        private void DrawControlPanel()
        {
            EditorGUILayout.LabelField("控制面板 (Control Panel)", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // 扫描按钮
            if (GUILayout.Button("扫描所有关卡 (Scan All Levels)", GUILayout.Height(30)))
            {
                ScanAllLevels();
            }

            if (allLevels.Count > 0)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField($"找到关卡: {allLevels.Count} 个", EditorStyles.boldLabel);

                EditorGUILayout.Space(10);

                // 筛选范围
                EditorGUILayout.LabelField("显示范围:", EditorStyles.miniBoldLabel);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("起始关卡:", GUILayout.Width(80));
                startLevel = EditorGUILayout.IntSlider(startLevel, 1, allLevels.Count);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("结束关卡:", GUILayout.Width(80));
                endLevel = EditorGUILayout.IntSlider(endLevel, startLevel, allLevels.Count);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(10);

                // 显示选项
                EditorGUILayout.LabelField("显示选项:", EditorStyles.miniBoldLabel);
                showDifficultyLines = EditorGUILayout.Toggle("显示难度等级线", showDifficultyLines);
                showAnomalies = EditorGUILayout.Toggle("高亮异常跳跃", showAnomalies);
                showStatistics = EditorGUILayout.Toggle("显示统计信息", showStatistics);

                EditorGUILayout.Space(10);

                // 异常检测设置
                EditorGUILayout.LabelField("异常检测:", EditorStyles.miniBoldLabel);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("跳跃阈值:", GUILayout.Width(80));
                jumpThreshold = EditorGUILayout.Slider(jumpThreshold, 5f, 30f);
                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button("检测异常 (Detect Anomalies)", GUILayout.Height(25)))
                {
                    DetectAnomalies();
                }
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制图表
        /// </summary>
        private void DrawChart()
        {
            EditorGUILayout.LabelField("难度曲线图 (Difficulty Chart)", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // 获取筛选后的关卡
            var filteredLevels = allLevels
                .Skip(startLevel - 1)
                .Take(endLevel - startLevel + 1)
                .ToList();

            if (filteredLevels.Count == 0)
            {
                EditorGUILayout.LabelField("没有可显示的关卡", EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.EndVertical();
                return;
            }

            // 图表区域
            Rect chartRect = GUILayoutUtility.GetRect(chartWidth, chartHeight);

            // 绘制背景
            EditorGUI.DrawRect(chartRect, new Color(0.2f, 0.2f, 0.2f));

            // 计算绘图参数
            float padding = 40f;
            float plotWidth = chartRect.width - padding * 2;
            float plotHeight = chartRect.height - padding * 2;
            Rect plotRect = new Rect(chartRect.x + padding, chartRect.y + padding, plotWidth, plotHeight);

            // 绘制网格和坐标轴
            DrawGrid(plotRect);

            // 绘制难度等级参考线
            if (showDifficultyLines)
            {
                DrawDifficultyLevelLines(plotRect);
            }

            // 绘制难度曲线
            DrawDifficultyCurve(plotRect, filteredLevels);

            // 绘制异常高亮
            if (showAnomalies)
            {
                DrawAnomalyHighlights(plotRect, filteredLevels);
            }

            // 绘制坐标轴标签
            DrawAxisLabels(chartRect, plotRect, filteredLevels);

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制网格和坐标轴
        /// </summary>
        private void DrawGrid(Rect plotRect)
        {
            Handles.BeginGUI();

            // 横向网格线 (难度分数)
            Handles.color = new Color(0.3f, 0.3f, 0.3f);
            for (int i = 0; i <= 10; i++)
            {
                float y = plotRect.y + plotRect.height * (1f - i / 10f);
                Handles.DrawLine(
                    new Vector2(plotRect.x, y),
                    new Vector2(plotRect.x + plotRect.width, y)
                );
            }

            // 纵向网格线 (关卡编号)
            int gridCount = Mathf.Min(10, endLevel - startLevel + 1);
            for (int i = 0; i <= gridCount; i++)
            {
                float x = plotRect.x + plotRect.width * (i / (float)gridCount);
                Handles.DrawLine(
                    new Vector2(x, plotRect.y),
                    new Vector2(x, plotRect.y + plotRect.height)
                );
            }

            // 坐标轴
            Handles.color = Color.white;
            Handles.DrawLine(
                new Vector2(plotRect.x, plotRect.y + plotRect.height),
                new Vector2(plotRect.x + plotRect.width, plotRect.y + plotRect.height)
            ); // X轴
            Handles.DrawLine(
                new Vector2(plotRect.x, plotRect.y),
                new Vector2(plotRect.x, plotRect.y + plotRect.height)
            ); // Y轴

            Handles.EndGUI();
        }

        /// <summary>
        /// 绘制难度等级参考线
        /// </summary>
        private void DrawDifficultyLevelLines(Rect plotRect)
        {
            Handles.BeginGUI();

            // 难度等级阈值
            float[] thresholds = { 15f, 30f, 50f, 70f, 85f }; // Tutorial, Easy, Normal, Hard, Expert
            Color[] colors = {
                new Color(0.5f, 1f, 0.5f, 0.3f), // Tutorial - Green
                new Color(0.7f, 1f, 0.3f, 0.3f), // Easy - Light Green
                new Color(1f, 1f, 0.3f, 0.3f),   // Normal - Yellow
                new Color(1f, 0.7f, 0.3f, 0.3f), // Hard - Orange
                new Color(1f, 0.5f, 0.3f, 0.3f)  // Expert - Deep Orange
            };

            for (int i = 0; i < thresholds.Length; i++)
            {
                float normalizedY = thresholds[i] / 100f;
                float y = plotRect.y + plotRect.height * (1f - normalizedY);

                Handles.color = colors[i];
                Handles.DrawDottedLine(
                    new Vector2(plotRect.x, y),
                    new Vector2(plotRect.x + plotRect.width, y),
                    3f
                );
            }

            Handles.EndGUI();
        }

        /// <summary>
        /// 绘制难度曲线（光滑曲线）
        /// </summary>
        private void DrawDifficultyCurve(Rect plotRect, List<Level> levels)
        {
            if (levels.Count < 2)
                return;

            Handles.BeginGUI();

            // 构建数据点列表
            List<Vector2> dataPoints = new List<Vector2>();
            for (int i = 0; i < levels.Count; i++)
            {
                float x = plotRect.x + plotRect.width * (i / (float)(levels.Count - 1));
                float y = plotRect.y + plotRect.height * (1f - levels[i].difficultyScore / 100f);
                dataPoints.Add(new Vector2(x, y));
            }

            // 使用 Catmull-Rom 样条曲线绘制光滑曲线
            Handles.color = new Color(0.3f, 0.7f, 1f, 0.8f); // 蓝色曲线
            int segments = 20; // 每段之间的插值点数量

            for (int i = 0; i < dataPoints.Count - 1; i++)
            {
                // 获取控制点
                Vector2 p0 = i > 0 ? dataPoints[i - 1] : dataPoints[i];
                Vector2 p1 = dataPoints[i];
                Vector2 p2 = dataPoints[i + 1];
                Vector2 p3 = i < dataPoints.Count - 2 ? dataPoints[i + 2] : dataPoints[i + 1];

                // 绘制光滑曲线段
                for (int j = 0; j < segments; j++)
                {
                    float t1 = j / (float)segments;
                    float t2 = (j + 1) / (float)segments;

                    Vector2 point1 = CatmullRom(p0, p1, p2, p3, t1);
                    Vector2 point2 = CatmullRom(p0, p1, p2, p3, t2);

                    Handles.DrawLine(point1, point2);
                }
            }

            // 绘制数据点
            Handles.color = new Color(0.3f, 0.7f, 1f);
            foreach (Vector2 point in dataPoints)
            {
                Handles.DrawSolidDisc(point, Vector3.forward, 3f);
            }

            Handles.EndGUI();
        }

        /// <summary>
        /// Catmull-Rom 样条曲线插值
        /// </summary>
        private Vector2 CatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;

            float x = 0.5f * (
                (2f * p1.x) +
                (-p0.x + p2.x) * t +
                (2f * p0.x - 5f * p1.x + 4f * p2.x - p3.x) * t2 +
                (-p0.x + 3f * p1.x - 3f * p2.x + p3.x) * t3
            );

            float y = 0.5f * (
                (2f * p1.y) +
                (-p0.y + p2.y) * t +
                (2f * p0.y - 5f * p1.y + 4f * p2.y - p3.y) * t2 +
                (-p0.y + 3f * p1.y - 3f * p2.y + p3.y) * t3
            );

            return new Vector2(x, y);
        }

        /// <summary>
        /// 绘制异常高亮
        /// </summary>
        private void DrawAnomalyHighlights(Rect plotRect, List<Level> levels)
        {
            Handles.BeginGUI();

            foreach (int index in anomalyIndices)
            {
                // 调整索引到筛选后的范围
                int adjustedIndex = index - (startLevel - 1);

                if (adjustedIndex >= 0 && adjustedIndex < levels.Count)
                {
                    float x = plotRect.x + plotRect.width * (adjustedIndex / (float)(levels.Count - 1));
                    float y = plotRect.y + plotRect.height * (1f - levels[adjustedIndex].difficultyScore / 100f);

                    // 绘制红色圆圈高亮
                    Handles.color = new Color(1f, 0.2f, 0.2f, 0.5f);
                    Handles.DrawSolidDisc(new Vector2(x, y), Vector3.forward, 8f);

                    Handles.color = Color.red;
                    Handles.DrawWireDisc(new Vector2(x, y), Vector3.forward, 10f);
                }
            }

            Handles.EndGUI();
        }

        /// <summary>
        /// 绘制坐标轴标签
        /// </summary>
        private void DrawAxisLabels(Rect chartRect, Rect plotRect, List<Level> levels)
        {
            GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel);
            labelStyle.alignment = TextAnchor.MiddleRight;

            // Y轴标签 (难度分数)
            for (int i = 0; i <= 10; i++)
            {
                float y = plotRect.y + plotRect.height * (1f - i / 10f);
                float score = i * 10f;

                Rect labelRect = new Rect(chartRect.x, y - 8f, 30f, 16f);
                GUI.Label(labelRect, score.ToString("F0"), labelStyle);
            }

            // X轴标签 (关卡编号)
            labelStyle.alignment = TextAnchor.UpperCenter;
            int labelCount = Mathf.Min(10, levels.Count);
            for (int i = 0; i <= labelCount; i++)
            {
                float x = plotRect.x + plotRect.width * (i / (float)labelCount);
                int levelIndex = Mathf.RoundToInt((levels.Count - 1) * (i / (float)labelCount));
                int levelNum = startLevel + levelIndex;

                Rect labelRect = new Rect(x - 20f, plotRect.y + plotRect.height + 5f, 40f, 16f);
                GUI.Label(labelRect, levelNum.ToString(), labelStyle);
            }

            // 坐标轴标题
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
            titleStyle.alignment = TextAnchor.MiddleCenter;

            // X轴标题
            Rect xTitleRect = new Rect(plotRect.x, plotRect.y + plotRect.height + 25f, plotRect.width, 20f);
            GUI.Label(xTitleRect, "关卡编号 (Level Number)", titleStyle);

            // Y轴标题 (旋转90度显示)
            GUIUtility.RotateAroundPivot(-90, new Vector2(chartRect.x + 5f, plotRect.y + plotRect.height / 2f));
            Rect yTitleRect = new Rect(chartRect.x - plotRect.height / 2f - 50f, plotRect.y + plotRect.height / 2f - 10f, 100f, 20f);
            GUI.Label(yTitleRect, "难度分数 (Difficulty Score)", titleStyle);
            GUI.matrix = Matrix4x4.identity; // 重置旋转
        }

        /// <summary>
        /// 绘制统计信息
        /// </summary>
        private void DrawStatistics()
        {
            EditorGUILayout.LabelField("统计信息 (Statistics)", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            var filteredLevels = allLevels
                .Skip(startLevel - 1)
                .Take(endLevel - startLevel + 1)
                .ToList();

            if (filteredLevels.Count == 0)
            {
                EditorGUILayout.EndVertical();
                return;
            }

            // 基础统计
            float minScore = filteredLevels.Min(l => l.difficultyScore);
            float maxScore = filteredLevels.Max(l => l.difficultyScore);
            float avgScore = filteredLevels.Average(l => l.difficultyScore);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"最低难度: {minScore:F1}", GUILayout.Width(150));
            EditorGUILayout.LabelField($"最高难度: {maxScore:F1}", GUILayout.Width(150));
            EditorGUILayout.LabelField($"平均难度: {avgScore:F1}", GUILayout.Width(150));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // 难度等级分布
            Dictionary<DifficultyLevel, int> distribution = new Dictionary<DifficultyLevel, int>();
            foreach (DifficultyLevel level in System.Enum.GetValues(typeof(DifficultyLevel)))
            {
                distribution[level] = filteredLevels.Count(l => l.difficultyLevel == level);
            }

            EditorGUILayout.LabelField("难度等级分布:", EditorStyles.miniBoldLabel);

            foreach (var kvp in distribution)
            {
                if (kvp.Value > 0)
                {
                    float percentage = (float)kvp.Value / filteredLevels.Count * 100f;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"{kvp.Key}:", GUILayout.Width(80));

                    Rect barRect = EditorGUILayout.GetControlRect(false, 18);
                    EditorGUI.ProgressBar(barRect, percentage / 100f, "");

                    EditorGUILayout.LabelField($"{kvp.Value} ({percentage:F0}%)", GUILayout.Width(80));
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制异常列表
        /// </summary>
        private void DrawAnomaliesList()
        {
            EditorGUILayout.LabelField("异常跳跃列表 (Anomalies)", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField($"检测到 {anomalyIndices.Count} 处异常跳跃", EditorStyles.miniBoldLabel);

            EditorGUILayout.Space(5);

            foreach (int index in anomalyIndices)
            {
                if (index >= 0 && index < allLevels.Count - 1)
                {
                    Level current = allLevels[index];
                    Level next = allLevels[index + 1];
                    float jump = Mathf.Abs(next.difficultyScore - current.difficultyScore);

                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                    EditorGUILayout.LabelField($"关卡 {index + 1} → {index + 2}", GUILayout.Width(120));
                    EditorGUILayout.LabelField($"{current.difficultyScore:F1} → {next.difficultyScore:F1}", GUILayout.Width(120));

                    GUIStyle jumpStyle = new GUIStyle(EditorStyles.boldLabel);
                    jumpStyle.normal.textColor = Color.red;
                    EditorGUILayout.LabelField($"跳跃: {jump:F1}", jumpStyle, GUILayout.Width(100));

                    if (GUILayout.Button("查看", GUILayout.Width(50)))
                    {
                        Selection.activeObject = current;
                        EditorGUIUtility.PingObject(current);
                    }

                    EditorGUILayout.EndHorizontal();
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

            // 尝试按关卡编号排序
            allLevels.Sort((a, b) => GetLevelNumber(a).CompareTo(GetLevelNumber(b)));

            // 更新结束关卡范围
            if (allLevels.Count > 0)
            {
                endLevel = allLevels.Count;
            }

            Debug.Log($"[DifficultyChartWindow] 在 Resources/Levels 文件夹中找到 {allLevels.Count} 个关卡");

            // 自动检测异常
            DetectAnomalies();
        }

        /// <summary>
        /// 检测异常跳跃
        /// </summary>
        private void DetectAnomalies()
        {
            anomalyIndices.Clear();

            for (int i = 0; i < allLevels.Count - 1; i++)
            {
                float currentScore = allLevels[i].difficultyScore;
                float nextScore = allLevels[i + 1].difficultyScore;
                float jump = Mathf.Abs(nextScore - currentScore);

                if (jump >= jumpThreshold)
                {
                    anomalyIndices.Add(i);
                }
            }

            Debug.Log($"[DifficultyChartWindow] 检测到 {anomalyIndices.Count} 处异常跳跃");
        }

        /// <summary>
        /// 获取关卡编号
        /// </summary>
        private int GetLevelNumber(Level level)
        {
            // 尝试从名称中提取数字
            string name = level.name;
            string numberStr = "";

            foreach (char c in name)
            {
                if (char.IsDigit(c))
                {
                    numberStr += c;
                }
            }

            if (int.TryParse(numberStr, out int number))
            {
                return number;
            }

            return 0;
        }
    }
}
#endif
