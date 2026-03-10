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
using BlockPuzzleGameToolkit.Scripts.LevelsData;

namespace GameCore.DifficultySystem.Editor
{
    /// <summary>
    /// 单个关卡的深度分析窗口
    /// 提供详细的六维度分析和优化建议
    /// </summary>
    public class DifficultyAnalysisWindow : EditorWindow
    {
        private Level selectedLevel;
        private Vector2 scrollPosition;
        private bool hasAnalyzed = false;

        [MenuItem("Tools/Difficulty System/Level Analysis")]
        public static void ShowWindow()
        {
            var window = GetWindow<DifficultyAnalysisWindow>("关卡分析");
            window.minSize = new Vector2(600, 700);
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // 标题
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
            titleStyle.fontSize = 16;
            titleStyle.alignment = TextAnchor.MiddleCenter;
            EditorGUILayout.LabelField("关卡难度深度分析", titleStyle);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            // 选择关卡
            DrawLevelSelection();

            if (selectedLevel != null)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                // 分析按钮
                DrawAnalyzeButton();

                if (hasAnalyzed)
                {
                    EditorGUILayout.Space(10);
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    // 总体概览
                    DrawOverview();

                    EditorGUILayout.Space(10);
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    // 六维度详细分析
                    DrawDetailedAnalysis();

                    EditorGUILayout.Space(10);
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    // 雷达图
                    DrawRadarChart();

                    EditorGUILayout.Space(10);
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    // 优化建议
                    DrawOptimizationSuggestions();
                }
            }

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// 绘制关卡选择
        /// </summary>
        private void DrawLevelSelection()
        {
            EditorGUILayout.LabelField("选择关卡 (Select Level)", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            Level newLevel = (Level)EditorGUILayout.ObjectField("关卡:", selectedLevel, typeof(Level), false);

            if (newLevel != selectedLevel)
            {
                selectedLevel = newLevel;
                hasAnalyzed = false;
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制分析按钮
        /// </summary>
        private void DrawAnalyzeButton()
        {
            if (GUILayout.Button("分析关卡 (Analyze Level)", GUILayout.Height(40)))
            {
                AnalyzeLevel();
            }
        }

        /// <summary>
        /// 分析关卡
        /// </summary>
        private void AnalyzeLevel()
        {
            if (selectedLevel == null)
            {
                EditorUtility.DisplayDialog("错误", "请先选择一个关卡", "确定");
                return;
            }

            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("提示",
                    "关卡分析需要在运行模式下使用\n请点击Play按钮进入运行模式后再分析",
                    "确定");
                return;
            }

            var calculator = LevelDifficultyCalculator.Instance;
            if (calculator == null)
            {
                EditorUtility.DisplayDialog("错误",
                    "LevelDifficultyCalculator未初始化\n请确保场景中存在GameManager",
                    "确定");
                return;
            }

            calculator.CalculateAndSave(selectedLevel);
            EditorUtility.SetDirty(selectedLevel);

            hasAnalyzed = true;
            Repaint();
        }

        /// <summary>
        /// 绘制总体概览
        /// </summary>
        private void DrawOverview()
        {
            EditorGUILayout.LabelField("总体概览 (Overview)", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // 关卡名称
            EditorGUILayout.LabelField($"关卡: {selectedLevel.name}", EditorStyles.boldLabel);

            EditorGUILayout.Space(5);

            // 总分
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("总分:", GUILayout.Width(100));
            EditorGUILayout.LabelField($"{selectedLevel.difficultyScore:F1} / 100", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            // 进度条
            Rect rect = EditorGUILayout.GetControlRect(false, 25);
            EditorGUI.ProgressBar(rect, selectedLevel.difficultyScore / 100f, "");

            // 难度等级
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("等级:", GUILayout.Width(100));
            string levelText = GetDifficultyLevelText(selectedLevel.difficultyLevel);
            Color levelColor = GetDifficultyLevelColor(selectedLevel.difficultyLevel);

            GUIStyle coloredStyle = new GUIStyle(EditorStyles.boldLabel);
            coloredStyle.normal.textColor = levelColor;
            EditorGUILayout.LabelField(levelText, coloredStyle);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制详细分析
        /// </summary>
        private void DrawDetailedAnalysis()
        {
            EditorGUILayout.LabelField("六维度详细分析 (Detailed Analysis)", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            DrawDimensionDetail("空间压力 (Space Stress)", selectedLevel.breakdown.spaceStress,
                "棋盘空间的使用压力，受棋盘大小和空格比例影响");

            DrawDimensionDetail("方块复杂度 (Shape Complexity)", selectedLevel.breakdown.shapeComplexity,
                "方块池的复杂程度，大块和异形块占比越高越复杂");

            DrawDimensionDetail("目标压力 (Target Pressure)", selectedLevel.breakdown.targetPressure,
                "目标完成的难度，受目标数量、总量和类型影响");

            DrawDimensionDetail("时间压力 (Time Pressure)", selectedLevel.breakdown.timePressure,
                "时间限制带来的压力，限时越短压力越大");

            DrawDimensionDetail("资源限制 (Resource Constraint)", selectedLevel.breakdown.resourceConstraint,
                "初始方块配置的限制程度");

            DrawDimensionDetail("策略深度 (Strategy Depth)", selectedLevel.breakdown.strategyDepth,
                "需要的策略思考深度，受奖励道具和目标多样性影响");

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制单个维度的详细信息
        /// </summary>
        private void DrawDimensionDetail(string label, float value, string description)
        {
            EditorGUILayout.Space(5);

            // 维度名称和分数
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel, GUILayout.Width(250));
            EditorGUILayout.LabelField($"{value:F1}", EditorStyles.boldLabel, GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();

            // 描述
            EditorGUILayout.LabelField(description, EditorStyles.miniLabel);

            // 进度条
            Rect rect = EditorGUILayout.GetControlRect(false, 20);
            Color barColor = GetDimensionColor(value);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width * (value / 100f), rect.height), barColor);
            GUI.Box(rect, "", EditorStyles.helpBox);

            EditorGUILayout.Space(3);
        }

        /// <summary>
        /// 绘制雷达图
        /// </summary>
        private void DrawRadarChart()
        {
            EditorGUILayout.LabelField("雷达图 (Radar Chart)", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            float size = 200f;
            Rect chartRect = GUILayoutUtility.GetRect(size, size);
            chartRect.x += (EditorGUIUtility.currentViewWidth - size) / 2f - 20f; // 居中

            DrawRadarChartInternal(chartRect, size);

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制雷达图内部实现
        /// </summary>
        private void DrawRadarChartInternal(Rect rect, float size)
        {
            Vector2 center = new Vector2(rect.x + size / 2f, rect.y + size / 2f);
            float radius = size / 2f - 20f;

            // 绘制背景圆圈
            Handles.color = new Color(0.3f, 0.3f, 0.3f);
            for (int i = 1; i <= 5; i++)
            {
                float r = radius * i / 5f;
                Handles.DrawWireDisc(center, Vector3.forward, r);
            }

            // 六个维度
            float[] values = new float[]
            {
                selectedLevel.breakdown.spaceStress,
                selectedLevel.breakdown.shapeComplexity,
                selectedLevel.breakdown.targetPressure,
                selectedLevel.breakdown.timePressure,
                selectedLevel.breakdown.resourceConstraint,
                selectedLevel.breakdown.strategyDepth
            };

            string[] labels = new string[]
            {
                "空间压力",
                "方块复杂度",
                "目标压力",
                "时间压力",
                "资源限制",
                "策略深度"
            };

            int count = values.Length;
            Vector2[] points = new Vector2[count];

            // 计算六边形顶点
            for (int i = 0; i < count; i++)
            {
                float angle = (360f / count) * i - 90f; // 从顶部开始
                float angleRad = angle * Mathf.Deg2Rad;

                // 绘制轴线
                Vector2 axisEnd = center + new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * radius;
                Handles.color = new Color(0.5f, 0.5f, 0.5f);
                Handles.DrawLine(center, axisEnd);

                // 计算数值点
                float normalizedValue = values[i] / 100f;
                points[i] = center + new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * radius * normalizedValue;

                // 绘制标签
                Vector2 labelPos = center + new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * (radius + 15f);
                GUI.Label(new Rect(labelPos.x - 30f, labelPos.y - 8f, 60f, 16f), labels[i], EditorStyles.miniLabel);
            }

            // 绘制数值多边形
            Handles.color = new Color(0.3f, 0.7f, 1f, 0.5f);
            for (int i = 0; i < count; i++)
            {
                int next = (i + 1) % count;
                Handles.DrawLine(points[i], points[next]);
            }

            // 绘制数值点
            Handles.color = new Color(0.3f, 0.7f, 1f);
            for (int i = 0; i < count; i++)
            {
                Handles.DrawSolidDisc(points[i], Vector3.forward, 3f);
            }
        }

        /// <summary>
        /// 绘制优化建议
        /// </summary>
        private void DrawOptimizationSuggestions()
        {
            EditorGUILayout.LabelField("优化建议 (Optimization Suggestions)", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // 根据各维度分数给出建议
            if (selectedLevel.breakdown.spaceStress > 70f)
            {
                DrawSuggestion("⚠️ 空间压力过高", "建议增加棋盘空格比例或扩大棋盘尺寸");
            }
            else if (selectedLevel.breakdown.spaceStress < 30f)
            {
                DrawSuggestion("💡 空间压力较低", "可以减少空格比例以提升难度");
            }

            if (selectedLevel.breakdown.shapeComplexity > 70f)
            {
                DrawSuggestion("⚠️ 方块复杂度过高", "建议增加基础块的比例");
            }
            else if (selectedLevel.breakdown.shapeComplexity < 30f)
            {
                DrawSuggestion("💡 方块复杂度较低", "可以增加异形块和大块的比例");
            }

            if (selectedLevel.breakdown.targetPressure > 70f)
            {
                DrawSuggestion("⚠️ 目标压力过高", "建议减少目标数量或降低目标总量");
            }
            else if (selectedLevel.breakdown.targetPressure < 30f)
            {
                DrawSuggestion("💡 目标压力较低", "可以增加目标数量或提高目标总量");
            }

            if (selectedLevel.breakdown.timePressure > 70f)
            {
                DrawSuggestion("⚠️ 时间压力过高", "建议延长时间限制");
            }

            // 整体平衡性检查
            float maxDiff = GetMaxDimensionDifference();
            if (maxDiff > 50f)
            {
                DrawSuggestion("⚠️ 维度不平衡", "各维度分数差异较大，建议调整使其更平衡");
            }

            // 难度曲线建议
            if (selectedLevel.difficultyScore > 85f)
            {
                DrawSuggestion("🎯 大师级关卡", "适合作为挑战关卡或后期关卡");
            }
            else if (selectedLevel.difficultyScore < 20f)
            {
                DrawSuggestion("🎯 教学级关卡", "适合作为新手引导关卡");
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制单条建议
        /// </summary>
        private void DrawSuggestion(string title, string description)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(description, EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(3);
        }

        /// <summary>
        /// 获取最大维度差异
        /// </summary>
        private float GetMaxDimensionDifference()
        {
            float[] values = new float[]
            {
                selectedLevel.breakdown.spaceStress,
                selectedLevel.breakdown.shapeComplexity,
                selectedLevel.breakdown.targetPressure,
                selectedLevel.breakdown.timePressure,
                selectedLevel.breakdown.resourceConstraint,
                selectedLevel.breakdown.strategyDepth
            };

            float min = float.MaxValue;
            float max = float.MinValue;

            foreach (float value in values)
            {
                if (value < min) min = value;
                if (value > max) max = value;
            }

            return max - min;
        }

        /// <summary>
        /// 获取难度等级的文本表示
        /// </summary>
        private string GetDifficultyLevelText(DifficultyLevel level)
        {
            switch (level)
            {
                case DifficultyLevel.Tutorial: return "Tutorial (教学) ⭐";
                case DifficultyLevel.Easy: return "Easy (简单) ⭐⭐";
                case DifficultyLevel.Normal: return "Normal (普通) ⭐⭐⭐";
                case DifficultyLevel.Hard: return "Hard (困难) ⭐⭐⭐⭐";
                case DifficultyLevel.Expert: return "Expert (专家) ⭐⭐⭐⭐⭐";
                case DifficultyLevel.Master: return "Master (大师) ⭐⭐⭐⭐⭐⭐";
                default: return "Unknown";
            }
        }

        /// <summary>
        /// 获取难度等级的颜色
        /// </summary>
        private Color GetDifficultyLevelColor(DifficultyLevel level)
        {
            switch (level)
            {
                case DifficultyLevel.Tutorial: return new Color(0.5f, 1f, 0.5f);
                case DifficultyLevel.Easy: return new Color(0.7f, 1f, 0.3f);
                case DifficultyLevel.Normal: return new Color(1f, 1f, 0.3f);
                case DifficultyLevel.Hard: return new Color(1f, 0.7f, 0.3f);
                case DifficultyLevel.Expert: return new Color(1f, 0.5f, 0.3f);
                case DifficultyLevel.Master: return new Color(1f, 0.3f, 0.3f);
                default: return Color.white;
            }
        }

        /// <summary>
        /// 获取维度颜色（根据分数）
        /// </summary>
        private Color GetDimensionColor(float value)
        {
            if (value < 30f)
                return new Color(0.5f, 1f, 0.5f); // 绿色
            else if (value < 50f)
                return new Color(1f, 1f, 0.3f); // 黄色
            else if (value < 70f)
                return new Color(1f, 0.7f, 0.3f); // 橙色
            else
                return new Color(1f, 0.3f, 0.3f); // 红色
        }
    }
}
#endif
