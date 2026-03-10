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
    /// 批量自定义权重管理工具
    /// 用于批量设置多个关卡的自定义方块权重
    /// </summary>
    public class BatchCustomProbabilitiesEditor : EditorWindow
    {
        private List<Level> allLevels = new List<Level>();
        private List<Level> selectedLevels = new List<Level>();
        private Vector2 scrollPosition;

        // 筛选设置
        private int startLevel = 1;
        private int endLevel = 200;
        private bool onlyShowCustomEnabled = false;

        // 批量设置参数
        private bool batchEnableCustom = true;
        private CategoryProbabilities batchProbabilities = new CategoryProbabilities { basic = 0.5f, shaped = 0.35f, large = 0.15f };

        [MenuItem("Tools/Difficulty System/Batch Custom Probabilities")]
        public static void ShowWindow()
        {
            var window = GetWindow<BatchCustomProbabilitiesEditor>("批量权重管理");
            window.minSize = new Vector2(700, 600);
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // 标题
            DrawTitle();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            // 扫描和筛选
            DrawScanAndFilter();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            // 关卡列表
            if (allLevels.Count > 0)
            {
                DrawLevelList();

                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                // 批量操作面板
                if (selectedLevels.Count > 0)
                {
                    DrawBatchOperations();
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
            EditorGUILayout.LabelField("批量自定义权重管理", titleStyle);
        }

        /// <summary>
        /// 绘制扫描和筛选
        /// </summary>
        private void DrawScanAndFilter()
        {
            EditorGUILayout.LabelField("扫描和筛选 (Scan & Filter)", EditorStyles.boldLabel);

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
                EditorGUILayout.LabelField("筛选范围:", EditorStyles.miniBoldLabel);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("起始关卡:", GUILayout.Width(80));
                startLevel = EditorGUILayout.IntSlider(startLevel, 1, allLevels.Count);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("结束关卡:", GUILayout.Width(80));
                endLevel = EditorGUILayout.IntSlider(endLevel, startLevel, allLevels.Count);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(5);

                // 筛选选项
                onlyShowCustomEnabled = EditorGUILayout.Toggle("只显示已启用自定义权重的关卡", onlyShowCustomEnabled);

                EditorGUILayout.Space(5);

                // 统计信息
                int customEnabledCount = allLevels.Count(l => l.useCustomProbabilities);
                EditorGUILayout.LabelField($"已启用自定义权重: {customEnabledCount} / {allLevels.Count}", EditorStyles.miniBoldLabel);
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制关卡列表
        /// </summary>
        private void DrawLevelList()
        {
            EditorGUILayout.LabelField("关卡列表 (Level List)", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // 全选/取消全选按钮
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("全选", GUILayout.Width(80)))
            {
                SelectAll();
            }
            if (GUILayout.Button("取消全选", GUILayout.Width(80)))
            {
                DeselectAll();
            }
            if (GUILayout.Button("反选", GUILayout.Width(80)))
            {
                InvertSelection();
            }
            EditorGUILayout.LabelField($"已选择: {selectedLevels.Count} 个", EditorStyles.miniBoldLabel);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // 获取筛选后的关卡
            var filteredLevels = allLevels
                .Skip(startLevel - 1)
                .Take(endLevel - startLevel + 1)
                .Where(l => !onlyShowCustomEnabled || l.useCustomProbabilities)
                .ToList();

            if (filteredLevels.Count == 0)
            {
                EditorGUILayout.LabelField("没有符合条件的关卡", EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                // 关卡列表
                foreach (var level in filteredLevels)
                {
                    DrawLevelItem(level);
                }
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制单个关卡项
        /// </summary>
        private void DrawLevelItem(Level level)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

            // 选择框
            bool isSelected = selectedLevels.Contains(level);
            bool newSelected = EditorGUILayout.Toggle(isSelected, GUILayout.Width(20));

            if (newSelected != isSelected)
            {
                if (newSelected)
                {
                    selectedLevels.Add(level);
                }
                else
                {
                    selectedLevels.Remove(level);
                }
            }

            // 关卡名称
            EditorGUILayout.LabelField(level.name, EditorStyles.boldLabel, GUILayout.Width(150));

            // 难度等级
            EditorGUILayout.LabelField($"[{level.difficultyLevel}]", GUILayout.Width(80));

            // 自定义权重状态
            if (level.useCustomProbabilities)
            {
                GUI.color = Color.green;
                EditorGUILayout.LabelField("✓ 自定义", EditorStyles.boldLabel, GUILayout.Width(80));
                GUI.color = Color.white;

                // 显示概率
                EditorGUILayout.LabelField(
                    $"B:{level.customBaseline.basic * 100:F0}% S:{level.customBaseline.shaped * 100:F0}% L:{level.customBaseline.large * 100:F0}%",
                    EditorStyles.miniLabel,
                    GUILayout.Width(150)
                );
            }
            else
            {
                EditorGUILayout.LabelField("全局配置", EditorStyles.miniLabel, GUILayout.Width(80));
            }

            // 查看按钮
            if (GUILayout.Button("查看", GUILayout.Width(50)))
            {
                Selection.activeObject = level;
                EditorGUIUtility.PingObject(level);
            }

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 绘制批量操作面板
        /// </summary>
        private void DrawBatchOperations()
        {
            EditorGUILayout.LabelField($"批量操作 (Batch Operations) - 已选择 {selectedLevels.Count} 个关卡", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // 操作选项
            EditorGUILayout.LabelField("操作选项:", EditorStyles.miniBoldLabel);

            batchEnableCustom = EditorGUILayout.Toggle("启用自定义权重", batchEnableCustom);

            if (batchEnableCustom)
            {
                EditorGUILayout.Space(5);

                EditorGUILayout.LabelField("方块概率分布:", EditorStyles.miniBoldLabel);

                // Basic
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("基础块 (Basic):", GUILayout.Width(120));
                batchProbabilities.basic = EditorGUILayout.Slider(batchProbabilities.basic, 0f, 1f);
                EditorGUILayout.LabelField($"{batchProbabilities.basic * 100:F0}%", GUILayout.Width(50));
                EditorGUILayout.EndHorizontal();

                // Shaped
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("异形块 (Shaped):", GUILayout.Width(120));
                batchProbabilities.shaped = EditorGUILayout.Slider(batchProbabilities.shaped, 0f, 1f);
                EditorGUILayout.LabelField($"{batchProbabilities.shaped * 100:F0}%", GUILayout.Width(50));
                EditorGUILayout.EndHorizontal();

                // Large
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("大块 (Large):", GUILayout.Width(120));
                batchProbabilities.large = EditorGUILayout.Slider(batchProbabilities.large, 0f, 1f);
                EditorGUILayout.LabelField($"{batchProbabilities.large * 100:F0}%", GUILayout.Width(50));
                EditorGUILayout.EndHorizontal();

                // 总和验证
                float sum = batchProbabilities.basic + batchProbabilities.shaped + batchProbabilities.large;
                if (!Mathf.Approximately(sum, 1.0f))
                {
                    EditorGUILayout.HelpBox($"概率总和为 {sum:F3}，应该为 1.0", MessageType.Warning);

                    if (GUILayout.Button("归一化概率", GUILayout.Height(25)))
                    {
                        batchProbabilities.Normalize();
                    }
                }
                else
                {
                    EditorGUILayout.LabelField($"概率总和: {sum:F3} ✓", EditorStyles.miniLabel);
                }

                EditorGUILayout.Space(5);

                // 快速预设
                EditorGUILayout.LabelField("快速预设:", EditorStyles.miniBoldLabel);
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("简单 (70/30/0)"))
                {
                    batchProbabilities = new CategoryProbabilities { basic = 0.7f, shaped = 0.3f, large = 0f };
                }

                if (GUILayout.Button("普通 (50/35/15)"))
                {
                    batchProbabilities = new CategoryProbabilities { basic = 0.5f, shaped = 0.35f, large = 0.15f };
                }

                if (GUILayout.Button("困难 (30/45/25)"))
                {
                    batchProbabilities = new CategoryProbabilities { basic = 0.3f, shaped = 0.45f, large = 0.25f };
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(10);

            // 执行按钮
            GUI.color = new Color(0.5f, 1f, 0.5f);
            if (GUILayout.Button($"应用到 {selectedLevels.Count} 个关卡 (Apply to Selected)", GUILayout.Height(40)))
            {
                ApplyBatchOperation();
            }
            GUI.color = Color.white;

            EditorGUILayout.Space(5);

            // 其他批量操作
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("禁用所选关卡的自定义权重", GUILayout.Height(30)))
            {
                DisableCustomProbabilities();
            }

            if (GUILayout.Button("从第一个复制权重到其他", GUILayout.Height(30)))
            {
                CopyFromFirst();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 扫描所有关卡
        /// </summary>
        private void ScanAllLevels()
        {
            allLevels.Clear();
            selectedLevels.Clear();

            string[] guids = AssetDatabase.FindAssets("t:Level");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Level level = AssetDatabase.LoadAssetAtPath<Level>(path);

                if (level != null)
                {
                    allLevels.Add(level);
                }
            }

            // 按编号排序
            allLevels.Sort((a, b) => GetLevelNumber(a).CompareTo(GetLevelNumber(b)));

            if (allLevels.Count > 0)
            {
                endLevel = allLevels.Count;
            }

            Debug.Log($"[BatchCustomProbabilitiesEditor] 找到 {allLevels.Count} 个关卡");
        }

        /// <summary>
        /// 全选
        /// </summary>
        private void SelectAll()
        {
            selectedLevels.Clear();
            var filteredLevels = allLevels
                .Skip(startLevel - 1)
                .Take(endLevel - startLevel + 1)
                .Where(l => !onlyShowCustomEnabled || l.useCustomProbabilities);

            selectedLevels.AddRange(filteredLevels);
        }

        /// <summary>
        /// 取消全选
        /// </summary>
        private void DeselectAll()
        {
            selectedLevels.Clear();
        }

        /// <summary>
        /// 反选
        /// </summary>
        private void InvertSelection()
        {
            var filteredLevels = allLevels
                .Skip(startLevel - 1)
                .Take(endLevel - startLevel + 1)
                .Where(l => !onlyShowCustomEnabled || l.useCustomProbabilities)
                .ToList();

            var newSelection = new List<Level>();

            foreach (var level in filteredLevels)
            {
                if (!selectedLevels.Contains(level))
                {
                    newSelection.Add(level);
                }
            }

            selectedLevels = newSelection;
        }

        /// <summary>
        /// 应用批量操作
        /// </summary>
        private void ApplyBatchOperation()
        {
            if (selectedLevels.Count == 0)
            {
                EditorUtility.DisplayDialog("错误", "请先选择要操作的关卡", "确定");
                return;
            }

            bool confirm = EditorUtility.DisplayDialog("确认批量操作",
                $"确定要对 {selectedLevels.Count} 个关卡应用以下设置吗？\n\n" +
                $"启用自定义权重: {(batchEnableCustom ? "是" : "否")}\n" +
                (batchEnableCustom ? $"概率: Basic={batchProbabilities.basic * 100:F0}% Shaped={batchProbabilities.shaped * 100:F0}% Large={batchProbabilities.large * 100:F0}%" : ""),
                "确定", "取消");

            if (!confirm)
                return;

            foreach (var level in selectedLevels)
            {
                Undo.RecordObject(level, "Batch Apply Custom Probabilities");
                level.useCustomProbabilities = batchEnableCustom;

                if (batchEnableCustom)
                {
                    level.customBaseline = new CategoryProbabilities
                    {
                        basic = batchProbabilities.basic,
                        shaped = batchProbabilities.shaped,
                        large = batchProbabilities.large
                    };
                }

                EditorUtility.SetDirty(level);
            }

            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog("完成", $"成功修改 {selectedLevels.Count} 个关卡", "确定");

            Debug.Log($"[BatchCustomProbabilitiesEditor] 批量修改完成: {selectedLevels.Count} 个关卡");
        }

        /// <summary>
        /// 禁用自定义权重
        /// </summary>
        private void DisableCustomProbabilities()
        {
            if (selectedLevels.Count == 0)
            {
                EditorUtility.DisplayDialog("错误", "请先选择要操作的关卡", "确定");
                return;
            }

            bool confirm = EditorUtility.DisplayDialog("确认禁用",
                $"确定要禁用 {selectedLevels.Count} 个关卡的自定义权重吗？\n\n这些关卡将使用全局配置的权重。",
                "确定", "取消");

            if (!confirm)
                return;

            foreach (var level in selectedLevels)
            {
                Undo.RecordObject(level, "Disable Custom Probabilities");
                level.useCustomProbabilities = false;
                EditorUtility.SetDirty(level);
            }

            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog("完成", $"成功禁用 {selectedLevels.Count} 个关卡的自定义权重", "确定");
        }

        /// <summary>
        /// 从第一个复制权重到其他
        /// </summary>
        private void CopyFromFirst()
        {
            if (selectedLevels.Count < 2)
            {
                EditorUtility.DisplayDialog("错误", "请至少选择2个关卡", "确定");
                return;
            }

            Level firstLevel = selectedLevels[0];

            if (!firstLevel.useCustomProbabilities)
            {
                EditorUtility.DisplayDialog("错误", "第一个关卡未启用自定义权重", "确定");
                return;
            }

            bool confirm = EditorUtility.DisplayDialog("确认复制",
                $"确定要将 \"{firstLevel.name}\" 的自定义权重复制到其他 {selectedLevels.Count - 1} 个关卡吗？\n\n" +
                $"权重: Basic={firstLevel.customBaseline.basic * 100:F0}% Shaped={firstLevel.customBaseline.shaped * 100:F0}% Large={firstLevel.customBaseline.large * 100:F0}%",
                "确定", "取消");

            if (!confirm)
                return;

            for (int i = 1; i < selectedLevels.Count; i++)
            {
                Level level = selectedLevels[i];
                Undo.RecordObject(level, "Copy Custom Probabilities");
                level.useCustomProbabilities = true;
                level.customBaseline = new CategoryProbabilities
                {
                    basic = firstLevel.customBaseline.basic,
                    shaped = firstLevel.customBaseline.shaped,
                    large = firstLevel.customBaseline.large
                };
                EditorUtility.SetDirty(level);
            }

            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog("完成", $"成功复制到 {selectedLevels.Count - 1} 个关卡", "确定");
        }

        /// <summary>
        /// 获取关卡编号
        /// </summary>
        private int GetLevelNumber(Level level)
        {
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
