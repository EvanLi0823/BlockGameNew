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
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using GameCore.DifficultySystem;

namespace BlockPuzzleGameToolkit.Scripts.LevelsData.Editor
{
    [CustomEditor(typeof(ShapeTemplate))]
    public class ShapeEditor : UnityEditor.Editor
    {
        private const int GridSize = 5;
        private VisualElement gridContainer;
        private ShapeTemplate[] shapes;
        private int currentIndex;
        private ShapeTemplate _target;

        public override VisualElement CreateInspectorGUI()
        {
            _target = (ShapeTemplate)target;

            var root = new VisualElement();

            // Load and apply USS
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/BlockPuzzleGameToolkit/UIBuilder/ShapeEditorStyles.uss");
            root.styleSheets.Add(styleSheet);

            root.Add(new Label(_target.name) { name = "title" });

            var navContainer = new VisualElement { name = "nav-container" };
            navContainer.Add(CreateButton("<<", () => NavigateShapes(-1)));
            navContainer.Add(CreateButton(">>", () => NavigateShapes(1)));
            navContainer.Add(CreateButton("+", AddShape));
            navContainer.Add(CreateButton("-", RemoveShape));
            root.Add(navContainer);

            var actionContainer = new VisualElement { name = "action-container" };
            actionContainer.Add(CreateButton("Clear All", ClearAll));
            actionContainer.Add(CreateButton("Save", Save));
            root.Add(actionContainer);

            root.Add(new Label("Click on a square to add/remove a block") { name = "instructions" });

            var classicModeContainer = new VisualElement { name = "classic-mode-container" };
            classicModeContainer.AddToClassList("classic-mode-box");
            classicModeContainer.Add(new Label("Classic mode parameters") { name = "classic-mode-title" });

            var scoreField = new IntegerField("Score for Spawn") { value = _target.scoreForSpawn };
            scoreField.AddToClassList("classic-mode-field");
            scoreField.RegisterValueChangedCallback(evt =>
            {
                _target.scoreForSpawn = evt.newValue;
                EditorUtility.SetDirty(_target);
            });
            classicModeContainer.Add(scoreField);


            gridContainer = new VisualElement { name = "grid-container" };
            root.Add(gridContainer);

            var sliderContainer = new VisualElement { name = "slider-container" };
            sliderContainer.style.flexDirection = FlexDirection.Row;
            var chanceField = new Slider("Chance for Spawn", 0, 1) { value = _target.chanceForSpawn };
            //value of slider
            var chanceValue = new FloatField { value = _target.chanceForSpawn };
            chanceValue.style.marginLeft = 10;
            chanceValue.RegisterValueChangedCallback(evt => { chanceField.value = evt.newValue; });
            chanceField.RegisterValueChangedCallback(evt =>
            {
                chanceValue.value = evt.newValue;
                _target.chanceForSpawn = evt.newValue;
            });
            chanceField.AddToClassList("classic-mode-field");
            chanceField.style.width = 200;
            chanceField.RegisterValueChangedCallback(evt =>
            {
                _target.chanceForSpawn = evt.newValue;
                EditorUtility.SetDirty(_target);
            });
            sliderContainer.Add(chanceField);
            sliderContainer.Add(chanceValue);
            root.Add(sliderContainer);

            root.Add(classicModeContainer);

            var adventureModeContainer = new VisualElement { name = "classic-mode-container" };
            adventureModeContainer.AddToClassList("classic-mode-box");
            adventureModeContainer.Add(new Label("Adventure mode parameters") { name = "adventure-mode-title" });

            var spawnFromLevel = new IntegerField("spawn from level") { value = _target.spawnFromLevel };
            spawnFromLevel.AddToClassList("adventure-mode-field");
            spawnFromLevel.RegisterValueChangedCallback(evt =>
            {
                _target.spawnFromLevel = evt.newValue;
                EditorUtility.SetDirty(_target);
            });
            adventureModeContainer.Add(spawnFromLevel);
            root.Add(adventureModeContainer);

            // 添加难度分析面板
            CreateDifficultyAnalysisPanel(root);

            LoadShapes();
            CreateGrid();

            return root;
        }

        private void LoadShapes()
        {
            shapes = Resources.LoadAll<ShapeTemplate>("Shapes");
            currentIndex = shapes.ToList().IndexOf(_target);
            if (currentIndex == -1 && shapes.Length > 0)
            {
                currentIndex = 0;
                Selection.activeObject = shapes[currentIndex];
            }
        }

        private void NavigateShapes(int direction)
        {
            Save();
            currentIndex = (currentIndex + direction + shapes.Length) % shapes.Length;
            Selection.activeObject = shapes[currentIndex];
            _target = shapes[currentIndex];
            CreateGrid();
        }

        private void AddShape()
        {
            Save();
            var path = AssetDatabase.GetAssetPath(_target);
            if (string.IsNullOrEmpty(path))
            {
                path = "Assets/BlockPuzzleGameToolkit/ScriptableObjects/Shapes";
            }
            else
            {
                path = Path.GetDirectoryName(path);
            }

            var newShapeTemplate = CreateInstance<ShapeTemplate>();
            for (var i = 0; i < GridSize; i++)
            {
                newShapeTemplate.rows[i] = new ShapeRow();
            }

            var assetPath = AssetDatabase.GenerateUniqueAssetPath($"{path}/NewShape.asset");
            AssetDatabase.CreateAsset(newShapeTemplate, assetPath);
            AssetDatabase.SaveAssets();

            LoadShapes();
            currentIndex = shapes.Length - 1;
            Selection.activeObject = newShapeTemplate;
            _target = newShapeTemplate;
            CreateGrid();
        }

        private void RemoveShape()
        {
            if (shapes.Length <= 1)
            {
                return;
            }

            var path = AssetDatabase.GetAssetPath(_target);
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.SaveAssets();

            LoadShapes();
            currentIndex = Mathf.Clamp(currentIndex, 0, shapes.Length - 1);
            Selection.activeObject = shapes[currentIndex];
            _target = shapes[currentIndex];
            CreateGrid();
        }

        private void CreateGrid()
        {
            gridContainer.Clear();
            for (var i = 0; i < GridSize; i++)
            {
                if (_target.rows[i] == null)
                {
                    _target.rows[i] = new ShapeRow();
                }

                var row = new VisualElement();
                row.AddToClassList("grid-row");
                for (var j = 0; j < GridSize; j++)
                {
                    var cell = new Button();
                    cell.AddToClassList("grid-cell");
                    cell.AddToClassList(_target.rows[i].cells[j] ? "active" : "inactive");
                    int x = i, y = j;
                    cell.clicked += () =>
                    {
                        _target.rows[x].cells[y] = !_target.rows[x].cells[y];
                        cell.ToggleInClassList("active");
                        cell.ToggleInClassList("inactive");
                        EditorUtility.SetDirty(_target);
                    };

                    row.Add(cell);
                }

                gridContainer.Add(row);
            }
        }

        private Button CreateButton(string text, Action clickEvent)
        {
            var button = new Button(clickEvent) { text = text };
            button.style.flexGrow = 1;
            return button;
        }

        private void ClearAll()
        {
            for (var i = 0; i < GridSize; i++)
            {
                for (var j = 0; j < GridSize; j++)
                {
                    _target.rows[i].cells[j] = false;
                }
            }

            EditorUtility.SetDirty(_target);
            CreateGrid();
        }

        private void Save()
        {
            EditorUtility.SetDirty(_target);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 创建难度分析面板
        /// </summary>
        private void CreateDifficultyAnalysisPanel(VisualElement root)
        {
            var analysisContainer = new VisualElement { name = "analysis-container" };
            analysisContainer.AddToClassList("classic-mode-box");
            analysisContainer.style.marginTop = 10;

            var title = new Label("Shape Analysis (难度分析)") { name = "analysis-title" };
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.fontSize = 14;
            analysisContainer.Add(title);

            // 手动分类选项
            var manualCategoryToggle = new Toggle("Use Manual Category") { value = _target.useManualCategory };
            manualCategoryToggle.RegisterValueChangedCallback(evt =>
            {
                _target.useManualCategory = evt.newValue;
                EditorUtility.SetDirty(_target);
            });
            analysisContainer.Add(manualCategoryToggle);

            // 分类选择
            var categoryField = new EnumField("Category", _target.category);
            categoryField.RegisterValueChangedCallback(evt =>
            {
                _target.category = (ShapeCategory)evt.newValue;
                EditorUtility.SetDirty(_target);
            });
            analysisContainer.Add(categoryField);

            // 分析按钮
            var analyzeButton = new Button(() => AnalyzeShape())
            {
                text = "Analyze Shape (分析方块)"
            };
            analyzeButton.style.height = 30;
            analyzeButton.style.marginTop = 5;
            analyzeButton.style.marginBottom = 5;
            analysisContainer.Add(analyzeButton);

            // 统计信息
            var statsContainer = new VisualElement();
            statsContainer.style.marginTop = 5;

            var cellCountLabel = new Label($"Cell Count: {_target.cellCount}");
            var sizeLabel = new Label($"Size: {_target.width} x {_target.height}");
            var rectangleLabel = new Label($"Is Rectangle: {(_target.isRectangle ? "Yes" : "No")}");
            var symmetryLabel = new Label($"Is Symmetrical: {(_target.isSymmetrical ? "Yes" : "No")}");

            statsContainer.Add(cellCountLabel);
            statsContainer.Add(sizeLabel);
            statsContainer.Add(rectangleLabel);
            statsContainer.Add(symmetryLabel);

            analysisContainer.Add(statsContainer);

            root.Add(analysisContainer);
        }

        /// <summary>
        /// 分析方块
        /// </summary>
        private void AnalyzeShape()
        {
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("提示",
                    "方块分析需要在运行模式下使用\n请点击Play按钮进入运行模式后再分析",
                    "确定");
                return;
            }

            var analyzer = ShapeTemplateAnalyzer.Instance;
            if (analyzer == null)
            {
                EditorUtility.DisplayDialog("错误",
                    "ShapeTemplateAnalyzer未初始化\n请确保场景中存在GameManager",
                    "确定");
                return;
            }

            analyzer.AnalyzeAndSave(_target);
            EditorUtility.SetDirty(_target);

            // 刷新Inspector以显示更新后的数据
            Selection.activeObject = null;
            Selection.activeObject = _target;
        }
    }
}