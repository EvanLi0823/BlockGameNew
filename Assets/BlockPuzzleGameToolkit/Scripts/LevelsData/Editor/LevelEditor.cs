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
using BlockPuzzleGameToolkit.Scripts.Editor;
using BlockPuzzleGameToolkit.Scripts.Editor.Drawers;
using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.Settings;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Random = System.Random;
using GameCore.DifficultySystem;

namespace BlockPuzzleGameToolkit.Scripts.LevelsData.Editor
{
    [CustomEditor(typeof(Level))]
    public class LevelEditor : UnityEditor.Editor
    {
        private List<ItemTemplate> availableTemplates;
        private Level level;
        private VisualElement root;
        private VisualElement matrixContainer;
        private IntegerField rowsField;
        private IntegerField columnsField;

        private VisualElement targetParameters;
        private VisualElement shapeConfigContainer;
        private VisualElement difficultyInfoContainer; // 添加难度信息容器引用
        private VisualElement customProbabilitiesContainer; // 添加自定义权重容器引用
        private string brush;
        private LevelTypeScriptable _previousELevelType;
        private Toggle symmetricalGenerationToggle;
        private PopupField<string> levelTypeDropdown;
        private Button cellGreyWithBonus;
        private readonly Color _highlightColor = new(0.6f, 0.6f, 0.6f);
        private readonly Color _disableColor = new(0.3f, 0.3f, 0.3f);

        private void OnEnable()
        {
            level = (Level)target;
            level.InitializeIfNeeded();
            LoadAvailableTemplates();
            if (level.bonusItemColors == null)
            {
                level.bonusItemColors = new Dictionary<Color, int>();
            }

            _previousELevelType = level.levelType;
        }

        private void LoadAvailableTemplates()
        {
            availableTemplates = new List<ItemTemplate>(Resources.LoadAll<ItemTemplate>(""));
            availableTemplates.Insert(0, null); // Add null as the first option (empty cell)
        }

        public override VisualElement CreateInspectorGUI()
        {
            root = new VisualElement();

            // Load and apply USS
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/BlockPuzzleGameToolkit/UIBuilder/LevelEditorStyles.uss");
            root.styleSheets.Add(styleSheet);

            root.Add(new Label(level.name) { name = "title" });
            root.Add(new LevelSwitcher(serializedObject, level, this));
            root.Add(new Label(""));

            var dimensionContainer = new VisualElement { name = "dimension-container" };
            rowsField = new IntegerField("Rows") { value = level.rows };
            columnsField = new IntegerField("Columns") { value = level.columns };
            var resizeButton = CreateButton("Resize", Color.white, "", false, ResizeMatrix);
            rowsField.RegisterValueChangedCallback(evt => level.rows = evt.newValue);
            columnsField.RegisterValueChangedCallback(evt => level.columns = evt.newValue);

            dimensionContainer.Add(rowsField);
            dimensionContainer.Add(columnsField);
            dimensionContainer.Add(resizeButton);
            root.Add(new Label(""));

            root.Add(dimensionContainer);
            root.Add(new Label(""));

            // Add Timer Settings Section
            var timerContainer = new VisualElement { name = "timer-container" };
            timerContainer.style.marginTop = 10;
            timerContainer.style.marginBottom = 10;
            timerContainer.style.paddingTop = 10;
            timerContainer.style.paddingBottom = 10;
            timerContainer.style.borderTopWidth = 1;
            timerContainer.style.borderBottomWidth = 1;
            timerContainer.style.borderTopColor = new Color(0.3f, 0.3f, 0.3f);
            timerContainer.style.borderBottomColor = new Color(0.3f, 0.3f, 0.3f);

            var timerLabel = new Label("Timer Settings");
            timerLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            timerLabel.style.marginBottom = 5;
            timerContainer.Add(timerLabel);


            var timerDurationField = new FloatField("Duration (seconds)") { value = level.timerDuration };
            timerDurationField.SetEnabled(level.enableTimer);
            timerDurationField.RegisterValueChangedCallback(evt =>
            {
                level.timerDuration = (int)evt.newValue;
                EditorUtility.SetDirty(target);
            });
            var enableTimerToggle = new Toggle("Enable Timer") { value = level.enableTimer };
            enableTimerToggle.RegisterValueChangedCallback(evt => 
            {
                level.enableTimer = evt.newValue;
                timerDurationField.SetEnabled(evt.newValue);
                EditorUtility.SetDirty(target);
            });
            timerContainer.Add(enableTimerToggle);
            timerContainer.Add(timerDurationField);

            root.Add(timerContainer);
            root.Add(new Label(""));

            // Add Reward Configuration Section - Manual UI Creation
            var rewardConfigContainer = new VisualElement { name = "reward-config-container" };
            rewardConfigContainer.style.marginTop = 10;
            rewardConfigContainer.style.marginBottom = 10;
            rewardConfigContainer.style.paddingTop = 10;
            rewardConfigContainer.style.paddingBottom = 10;
            rewardConfigContainer.style.paddingLeft = 10;
            rewardConfigContainer.style.paddingRight = 10;
            rewardConfigContainer.style.borderTopWidth = 1;
            rewardConfigContainer.style.borderBottomWidth = 1;
            rewardConfigContainer.style.borderTopColor = new Color(0.3f, 0.3f, 0.3f);
            rewardConfigContainer.style.borderBottomColor = new Color(0.3f, 0.3f, 0.3f);

            var rewardConfigLabel = new Label("Reward Configuration");
            rewardConfigLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            rewardConfigLabel.style.marginBottom = 10;
            rewardConfigContainer.Add(rewardConfigLabel);

            // Initialize rewardConfig if null
            if (level.rewardConfig == null)
            {
                level.rewardConfig = LevelRewardConfig.CreateDefault();
                EditorUtility.SetDirty(level);
            }

            // Popup Type Selection
            var popupTypeContainer = new VisualElement();
            popupTypeContainer.style.flexDirection = FlexDirection.Row;
            popupTypeContainer.style.marginBottom = 5;
            var popupTypeLabel = new Label("Popup Type");
            popupTypeLabel.style.width = 120;
            popupTypeContainer.Add(popupTypeLabel);

            var popupTypeField = new EnumField(level.rewardConfig.PopupType);
            popupTypeField.style.flexGrow = 1;
            popupTypeField.RegisterValueChangedCallback(evt =>
            {
                level.rewardConfig.PopupType = (LevelRewardConfig.RewardPopupType)evt.newValue;
                EditorUtility.SetDirty(level);
            });
            popupTypeContainer.Add(popupTypeField);
            rewardConfigContainer.Add(popupTypeContainer);

            // Skip Multiplier
            var skipMultiplierContainer = new VisualElement();
            skipMultiplierContainer.style.flexDirection = FlexDirection.Row;
            skipMultiplierContainer.style.marginBottom = 5;
            var skipMultiplierLabel = new Label("Skip Multiplier");
            skipMultiplierLabel.style.width = 120;
            skipMultiplierContainer.Add(skipMultiplierLabel);

            var skipMultiplierField = new Slider(0.01f, 1f);
            skipMultiplierField.style.flexGrow = 1;
            skipMultiplierField.value = level.rewardConfig.SkipMultiplier;
            skipMultiplierField.RegisterValueChangedCallback(evt =>
            {
                level.rewardConfig.SkipMultiplier = evt.newValue;
                EditorUtility.SetDirty(level);
            });
            skipMultiplierContainer.Add(skipMultiplierField);

            var skipValueLabel = new Label(level.rewardConfig.SkipMultiplier.ToString("F2"));
            skipValueLabel.style.width = 40;
            skipMultiplierField.RegisterValueChangedCallback(evt =>
            {
                skipValueLabel.text = evt.newValue.ToString("F2");
            });
            skipMultiplierContainer.Add(skipValueLabel);
            rewardConfigContainer.Add(skipMultiplierContainer);

            // Show Reward Ad
            var showRewardAdContainer = new VisualElement();
            showRewardAdContainer.style.flexDirection = FlexDirection.Row;
            showRewardAdContainer.style.marginBottom = 5;
            var showRewardAdLabel = new Label("Show Reward Ad");
            showRewardAdLabel.style.width = 120;
            showRewardAdContainer.Add(showRewardAdLabel);

            var showRewardAdField = new Toggle();
            showRewardAdField.value = level.rewardConfig.ShowRewardAd;
            showRewardAdField.RegisterValueChangedCallback(evt =>
            {
                level.rewardConfig.ShowRewardAd = evt.newValue;
                EditorUtility.SetDirty(level);
            });
            showRewardAdContainer.Add(showRewardAdField);
            rewardConfigContainer.Add(showRewardAdContainer);

            // Show Interstitial Ad
            var showInterstitialContainer = new VisualElement();
            showInterstitialContainer.style.flexDirection = FlexDirection.Row;
            showInterstitialContainer.style.marginBottom = 5;
            var showInterstitialLabel = new Label("Show Interstitial Ad");
            showInterstitialLabel.style.width = 120;
            showInterstitialContainer.Add(showInterstitialLabel);

            var showInterstitialField = new Toggle();
            showInterstitialField.value = level.rewardConfig.ShowInterstitialAd;
            showInterstitialField.RegisterValueChangedCallback(evt =>
            {
                level.rewardConfig.ShowInterstitialAd = evt.newValue;
                EditorUtility.SetDirty(level);
            });
            showInterstitialContainer.Add(showInterstitialField);
            rewardConfigContainer.Add(showInterstitialContainer);

            // Fixed Config ID (only show for Fixed type)
            var fixedConfigContainer = new VisualElement();
            fixedConfigContainer.style.flexDirection = FlexDirection.Row;
            fixedConfigContainer.style.marginBottom = 5;
            var fixedConfigLabel = new Label("Fixed Config");
            fixedConfigLabel.style.width = 120;
            fixedConfigContainer.Add(fixedConfigLabel);

            // Container for popup and refresh button
            var configSelectContainer = new VisualElement();
            configSelectContainer.style.flexDirection = FlexDirection.Row;
            configSelectContainer.style.flexGrow = 1;

            // Config preview label
            var configPreviewLabel = new Label();
            configPreviewLabel.style.marginLeft = 10;
            configPreviewLabel.style.fontSize = 11;
            configPreviewLabel.style.color = new Color(0.7f, 0.7f, 0.7f);

            PopupField<string> fixedConfigPopup = null;

            // Function to create/recreate the popup field with latest configs
            void RefreshConfigPopup()
            {
                // Clear the container
                configSelectContainer.Clear();

                // Load settings to get available configs
                var settings = Resources.Load<LevelRewardMultiplierSettings>("Settings/LevelRewardMultiplierSettings");

                if (settings != null)
                {
                    var configIds = settings.GetAllConfigIds();
                    var configNames = settings.GetAllConfigNames();

                    if (configIds != null && configIds.Length > 0)
                    {
                        // Find current selection index
                        int currentIndex = System.Array.IndexOf(configIds, level.rewardConfig.FixedMultiplierConfigId);
                        if (currentIndex < 0)
                        {
                            currentIndex = 0;
                            // Auto-select first config if current is invalid
                            level.rewardConfig.FixedMultiplierConfigId = configIds[0];
                            EditorUtility.SetDirty(level);
                        }

                        // Create PopupField with config names
                        var configNamesList = new List<string>(configNames);
                        fixedConfigPopup = new PopupField<string>(configNamesList, currentIndex);
                        fixedConfigPopup.style.flexGrow = 1;
                        fixedConfigPopup.style.maxWidth = 180;

                        // Update preview function
                        void UpdatePreview(int index)
                        {
                            if (index >= 0 && index < configIds.Length)
                            {
                                var config = settings.GetConfig(configIds[index]);
                                if (config != null && config.Multipliers != null)
                                {
                                    string multiplierStr = string.Join(", ", config.Multipliers);
                                    if (multiplierStr.Length > 30) multiplierStr = multiplierStr.Substring(0, 27) + "...";
                                    configPreviewLabel.text = $"[{multiplierStr}]";
                                    configPreviewLabel.tooltip = $"倍率: {string.Join(", ", config.Multipliers)}\n" +
                                                                 $"每日重置: {(config.ResetDaily ? "是" : "否")}\n" +
                                                                 $"提现重置: {(config.ResetOnWithdraw ? "是" : "否")}";
                                }
                            }
                        }

                        // Initial preview
                        UpdatePreview(currentIndex);

                        fixedConfigPopup.RegisterValueChangedCallback(evt =>
                        {
                            int selectedIndex = System.Array.IndexOf(configNames, evt.newValue);
                            if (selectedIndex >= 0 && selectedIndex < configIds.Length)
                            {
                                level.rewardConfig.FixedMultiplierConfigId = configIds[selectedIndex];
                                UpdatePreview(selectedIndex);
                                EditorUtility.SetDirty(level);
                            }
                        });
                        configSelectContainer.Add(fixedConfigPopup);

                        // Add refresh button
                        var refreshButton = new Button(() => RefreshConfigPopup()) { text = "↻" };
                        refreshButton.style.width = 25;
                        refreshButton.style.marginLeft = 5;
                        refreshButton.tooltip = "刷新配置列表";
                        configSelectContainer.Add(refreshButton);
                    }
                    else
                    {
                        var warningLabel = new Label("No configurations found! Click refresh →");
                        warningLabel.style.color = Color.yellow;
                        configSelectContainer.Add(warningLabel);

                        // Add refresh button even when no configs
                        var refreshButton = new Button(() => RefreshConfigPopup()) { text = "↻" };
                        refreshButton.style.width = 25;
                        refreshButton.style.marginLeft = 5;
                        configSelectContainer.Add(refreshButton);
                    }
                }
                else
                {
                    // Settings file not found - add create button
                    var errorLabel = new Label("Settings not found!");
                    errorLabel.style.color = Color.red;
                    configSelectContainer.Add(errorLabel);

                    var createButton = new Button(() =>
                    {
                        // Create settings file
                        EditorMenu.LevelRewardMultiplierSettings();
                        RefreshConfigPopup();
                    }) { text = "Create" };
                    createButton.style.marginLeft = 10;
                    configSelectContainer.Add(createButton);
                }
            }

            // Initial load
            RefreshConfigPopup();

            fixedConfigContainer.Add(configSelectContainer);
            fixedConfigContainer.Add(configPreviewLabel);

            // Show/hide based on popup type
            fixedConfigContainer.style.display = level.rewardConfig.PopupType == LevelRewardConfig.RewardPopupType.Fixed
                ? DisplayStyle.Flex : DisplayStyle.None;

            popupTypeField.RegisterValueChangedCallback(evt =>
            {
                var newType = (LevelRewardConfig.RewardPopupType)evt.newValue;
                bool isFixed = newType == LevelRewardConfig.RewardPopupType.Fixed;
                fixedConfigContainer.style.display = isFixed ? DisplayStyle.Flex : DisplayStyle.None;

                // Refresh when switching to Fixed type
                if (isFixed)
                {
                    RefreshConfigPopup();
                }
            });

            rewardConfigContainer.Add(fixedConfigContainer);

            root.Add(rewardConfigContainer);
            root.Add(new Label(""));

            // Add Initial Shape Configuration Section
            shapeConfigContainer = CreateInitialShapeConfigUI();
            root.Add(shapeConfigContainer);
            root.Add(new Label(""));

            // 使用传统循环过滤selectable的LevelType
            var allLevelTypes = Resources.LoadAll<LevelTypeScriptable>("");
            var levelTypes = new List<LevelTypeScriptable>();
            foreach (var lt in allLevelTypes)
            {
                if (lt.selectable)
                {
                    levelTypes.Add(lt);
                }
            }

            var levelTypeNames = new List<string>();
            foreach (var levelType in levelTypes)
            {
                levelTypeNames.Add(levelType.name);
            }

            if (level.levelType.elevelType != ELevelType.Classic)
            {
                // Register callback for level type change
                levelTypeDropdown = new PopupField<string>("Level Type", levelTypeNames, level.levelType.name);
                levelTypeDropdown.RegisterValueChangedCallback(evt =>
                {
                    // 使用传统循环查找匹配的LevelType
                    LevelTypeScriptable selectedLevelType = null;
                    foreach (var lt in levelTypes)
                    {
                        if (lt.name == evt.newValue)
                        {
                            selectedLevelType = lt;
                            break;
                        }
                    }

                    if (selectedLevelType != null)
                    {
                        level.levelType = selectedLevelType;
                        OnLevelTypeChanged();
                    }
                });
                root.Add(levelTypeDropdown);
            }

            root.Add(new Label(""));

            var targetField = new PropertyField(serializedObject.FindProperty("target"));
            root.Add(targetField);

            // Create bonus item color container
            targetParameters = new VisualElement { name = "bonus-item-color-container" };
            root.Add(targetParameters);
            root.Add(new Label(""));

            ToolPanel();
            root.Add(new Label("Target Parameters"));
            CreateBonusItemColorUI();
            root.Add(new Label("Click on cells to cycle through available ItemTemplates"));

            matrixContainer = new VisualElement { name = "grid-container" };
            root.Add(matrixContainer);

            // Add slider for empty cell percentage
            var emptyCellSlider = new Slider("Empty Cell %", 0, 100) { value = level.emptyCellPercentage };
            emptyCellSlider.style.width = 300;
            emptyCellSlider.RegisterValueChangedCallback(evt =>
            {
                level.emptyCellPercentage = evt.newValue;
                Randomize();
            });
            root.Add(emptyCellSlider);
            var randomButton = new Button(Randomize) { text = "Randomize" };
            randomButton.style.width = 150;
            randomButton.style.backgroundColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f));
            randomButton.RegisterCallback<ClickEvent>(evt => Randomize());
            root.Add(randomButton);

            // Add checkbox for symmetrical generation
            symmetricalGenerationToggle = new Toggle("Symmetrical Generation");
            symmetricalGenerationToggle.value = true;
            root.Add(symmetricalGenerationToggle);

            UpdateMatrixUI();

            // Add Difficulty System Section
            root.Add(new Label(""));
            var difficultyContainer = CreateDifficultySystemUI();
            root.Add(difficultyContainer);

            return root;
        }

        private void OnLevelTypeChanged()
        {
            if (level.levelType != _previousELevelType)
            {
                level.UpdateTargets();

                CreateBonusItemColorUI();
                Save();
                _previousELevelType = level.levelType;
                UpdateToolPanel();
            }
        }

        /// <summary>
        /// 创建初始方块配置UI
        /// </summary>
        private VisualElement CreateInitialShapeConfigUI()
        {
            var container = new VisualElement { name = "initial-shape-config-container" };
            container.style.marginTop = 10;
            container.style.marginBottom = 10;
            container.style.paddingTop = 10;
            container.style.paddingBottom = 10;
            container.style.paddingLeft = 10;
            container.style.paddingRight = 10;
            container.style.borderTopWidth = 1;
            container.style.borderBottomWidth = 1;
            container.style.borderTopColor = new Color(0.3f, 0.3f, 0.3f);
            container.style.borderBottomColor = new Color(0.3f, 0.3f, 0.3f);

            PopulateShapeConfigContainer(container);

            return container;
        }

        /// <summary>
        /// 填充形状配置容器内容
        /// </summary>
        private void PopulateShapeConfigContainer(VisualElement container)
        {
            // 清空容器
            container.Clear();

            // Header
            var headerContainer = new VisualElement();
            headerContainer.style.flexDirection = FlexDirection.Row;
            headerContainer.style.marginBottom = 10;

            var titleLabel = new Label("Initial Shape Configuration");
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.flexGrow = 1;
            headerContainer.Add(titleLabel);

            // Add Refresh Button (使用 + 符号)
            var addRefreshButton = new Button(() =>
            {
                if (level.initialShapeRefreshes == null)
                {
                    level.initialShapeRefreshes = new List<InitialShapeRefresh>();
                }
                level.initialShapeRefreshes.Add(new InitialShapeRefresh());
                EditorUtility.SetDirty(level);

                // 重建配置UI部分
                PopulateShapeConfigContainer(shapeConfigContainer);
            })
            {
                text = "+"
            };
            addRefreshButton.style.width = 25;
            addRefreshButton.style.backgroundColor = new Color(0.3f, 0.6f, 0.3f);
            headerContainer.Add(addRefreshButton);

            container.Add(headerContainer);

            // Help text
            var helpLabel = new Label("配置初始几次刷新的方块类型、颜色和奖励道具，不配置则使用随机生成。");
            helpLabel.style.fontSize = 11;
            helpLabel.style.color = new Color(0.7f, 0.7f, 0.7f);
            helpLabel.style.marginBottom = 10;
            container.Add(helpLabel);

            // Initialize list if null
            if (level.initialShapeRefreshes == null)
            {
                level.initialShapeRefreshes = new List<InitialShapeRefresh>();
            }

            // Display refresh configurations
            if (level.initialShapeRefreshes.Count == 0)
            {
                var emptyLabel = new Label("暂无配置，点击 '+' 添加");
                emptyLabel.style.color = new Color(0.6f, 0.6f, 0.6f);
                emptyLabel.style.marginTop = 5;
                container.Add(emptyLabel);
            }
            else
            {
                for (int i = 0; i < level.initialShapeRefreshes.Count; i++)
                {
                    int index = i; // Capture for lambda
                    var refreshContainer = CreateRefreshItemUI(index);
                    container.Add(refreshContainer);
                }
            }
        }

        /// <summary>
        /// 创建单个刷新项的UI
        /// </summary>
        private VisualElement CreateRefreshItemUI(int index)
        {
            var itemContainer = new VisualElement();
            itemContainer.style.marginBottom = 15;
            itemContainer.style.paddingBottom = 10;
            itemContainer.style.borderBottomWidth = 1;
            itemContainer.style.borderBottomColor = new Color(0.2f, 0.2f, 0.2f);

            // Header with index and delete button
            var headerContainer = new VisualElement();
            headerContainer.style.flexDirection = FlexDirection.Row;
            headerContainer.style.marginBottom = 8;

            var indexLabel = new Label($"第 {index + 1} 次刷新");
            indexLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            indexLabel.style.flexGrow = 1;
            headerContainer.Add(indexLabel);

            // Delete button (使用 - 符号)
            var deleteButton = new Button(() =>
            {
                level.initialShapeRefreshes.RemoveAt(index);
                EditorUtility.SetDirty(level);
                PopulateShapeConfigContainer(shapeConfigContainer);
            })
            {
                text = "−"
            };
            deleteButton.style.width = 25;
            deleteButton.style.backgroundColor = new Color(0.8f, 0.3f, 0.3f);
            headerContainer.Add(deleteButton);

            itemContainer.Add(headerContainer);

            // 获取或初始化刷新配置
            var refresh = level.initialShapeRefreshes[index];
            if (refresh.slots == null || refresh.slots.Length != 3)
            {
                refresh.slots = new ShapeSlotConfig[3];
                for (int i = 0; i < 3; i++)
                {
                    refresh.slots[i] = new ShapeSlotConfig();
                }
            }

            // 创建3个槽位的UI
            for (int slotIndex = 0; slotIndex < 3; slotIndex++)
            {
                int capturedSlotIndex = slotIndex;
                var slotContainer = CreateSlotUI(index, capturedSlotIndex, refresh.slots[slotIndex]);
                itemContainer.Add(slotContainer);
            }

            return itemContainer;
        }

        /// <summary>
        /// 创建单个槽位的UI（包含形状、颜色、BonusItem配置）
        /// </summary>
        private VisualElement CreateSlotUI(int refreshIndex, int slotIndex, ShapeSlotConfig slotConfig)
        {
            var slotContainer = new VisualElement();
            slotContainer.style.marginBottom = 10;
            slotContainer.style.paddingLeft = 15;
            slotContainer.style.paddingTop = 8;
            slotContainer.style.paddingBottom = 8;
            slotContainer.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f);

            // Slot title
            var slotLabel = new Label($"槽位 {slotIndex + 1}");
            slotLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            slotLabel.style.fontSize = 11;
            slotLabel.style.marginBottom = 5;
            slotContainer.Add(slotLabel);

            // Shape field
            var shapeField = new ObjectField("形状")
            {
                objectType = typeof(ShapeTemplate),
                value = slotConfig.shape
            };
            shapeField.RegisterValueChangedCallback(evt =>
            {
                slotConfig.shape = evt.newValue as ShapeTemplate;
                EditorUtility.SetDirty(level);
            });
            slotContainer.Add(shapeField);

            // Color field
            var colorField = new ObjectField("颜色")
            {
                objectType = typeof(ItemTemplate),
                value = slotConfig.color
            };
            colorField.RegisterValueChangedCallback(evt =>
            {
                slotConfig.color = evt.newValue as ItemTemplate;
                EditorUtility.SetDirty(level);
            });
            slotContainer.Add(colorField);

            // BonusItem configuration section
            var bonusSection = CreateBonusItemSection(refreshIndex, slotIndex, slotConfig);
            slotContainer.Add(bonusSection);

            return slotContainer;
        }

        /// <summary>
        /// 创建BonusItem配置区域
        /// </summary>
        private VisualElement CreateBonusItemSection(int refreshIndex, int slotIndex, ShapeSlotConfig slotConfig)
        {
            var section = new VisualElement();
            section.style.marginTop = 8;
            section.style.paddingTop = 8;
            section.style.paddingBottom = 5;
            section.style.paddingLeft = 10;
            section.style.paddingRight = 10;
            section.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);

            // Header with add button
            var headerContainer = new VisualElement();
            headerContainer.style.flexDirection = FlexDirection.Row;
            headerContainer.style.marginBottom = 5;

            var titleLabel = new Label("奖励道具配置");
            titleLabel.style.fontSize = 10;
            titleLabel.style.flexGrow = 1;
            titleLabel.style.alignSelf = Align.Center;
            headerContainer.Add(titleLabel);

            // Add bonus button (使用 + 符号)
            var addBonusButton = new Button(() =>
            {
                if (slotConfig.bonusItems == null)
                {
                    slotConfig.bonusItems = new List<BonusItemConfig>();
                }
                slotConfig.bonusItems.Add(new BonusItemConfig());
                EditorUtility.SetDirty(level);
                PopulateShapeConfigContainer(shapeConfigContainer);
            })
            {
                text = "+"
            };
            addBonusButton.style.width = 25;
            addBonusButton.style.backgroundColor = new Color(0.3f, 0.6f, 0.3f);
            headerContainer.Add(addBonusButton);

            section.Add(headerContainer);

            // Initialize bonus items list if null
            if (slotConfig.bonusItems == null)
            {
                slotConfig.bonusItems = new List<BonusItemConfig>();
            }

            // Display bonus items
            if (slotConfig.bonusItems.Count == 0)
            {
                var emptyLabel = new Label("（无奖励道具）");
                emptyLabel.style.fontSize = 9;
                emptyLabel.style.color = new Color(0.5f, 0.5f, 0.5f);
                emptyLabel.style.marginLeft = 5;
                section.Add(emptyLabel);
            }
            else
            {
                for (int i = 0; i < slotConfig.bonusItems.Count; i++)
                {
                    int bonusIndex = i;
                    var bonusItemUI = CreateBonusItemUI(refreshIndex, slotIndex, slotConfig, bonusIndex);
                    section.Add(bonusItemUI);
                }
            }

            return section;
        }

        /// <summary>
        /// 创建单个BonusItem配置项UI
        /// </summary>
        private VisualElement CreateBonusItemUI(int refreshIndex, int slotIndex, ShapeSlotConfig slotConfig, int bonusIndex)
        {
            var bonusContainer = new VisualElement();
            bonusContainer.style.flexDirection = FlexDirection.Row;
            bonusContainer.style.marginBottom = 3;
            bonusContainer.style.alignItems = Align.Center;

            var bonusConfig = slotConfig.bonusItems[bonusIndex];

            // Bullet point
            var bulletLabel = new Label("•");
            bulletLabel.style.marginRight = 5;
            bulletLabel.style.fontSize = 12;
            bonusContainer.Add(bulletLabel);

            // BonusItem selection
            var bonusField = new ObjectField
            {
                objectType = typeof(BonusItemTemplate),
                value = bonusConfig.bonusItem
            };
            bonusField.style.width = 150;
            bonusField.RegisterValueChangedCallback(evt =>
            {
                bonusConfig.bonusItem = evt.newValue as BonusItemTemplate;
                EditorUtility.SetDirty(level);
            });
            bonusContainer.Add(bonusField);

            // Count label
            var countLabel = new Label("数量:");
            countLabel.style.marginLeft = 5;
            countLabel.style.fontSize = 10;
            bonusContainer.Add(countLabel);

            // Count field (文本输入)
            var countField = new IntegerField
            {
                value = bonusConfig.count
            };
            countField.style.width = 50;
            countField.RegisterValueChangedCallback(evt =>
            {
                // 限制范围1-10
                bonusConfig.count = Mathf.Clamp(evt.newValue, 1, 10);
                countField.SetValueWithoutNotify(bonusConfig.count);
                EditorUtility.SetDirty(level);
            });
            bonusContainer.Add(countField);

            // Delete button (使用 - 符号)
            var deleteButton = new Button(() =>
            {
                slotConfig.bonusItems.RemoveAt(bonusIndex);
                EditorUtility.SetDirty(level);
                PopulateShapeConfigContainer(shapeConfigContainer);
            })
            {
                text = "−"
            };
            deleteButton.style.width = 25;
            deleteButton.style.marginLeft = 5;
            deleteButton.style.backgroundColor = new Color(0.8f, 0.3f, 0.3f);
            bonusContainer.Add(deleteButton);

            return bonusContainer;
        }

        private void UpdateToolPanel()
        {
            var isBonusItemLevelType = level.levelType.elevelType == ELevelType.CollectItems;
            cellGreyWithBonus.style.display = isBonusItemLevelType ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void CreateBonusItemColorUI()
        {
            targetParameters.Clear();
            targetParameters.style.flexDirection = FlexDirection.Row;

            for (var index = 0; index < level.targetInstance.Count; index++)
            {
                var targetInstance = level.targetInstance[index];

                if (targetInstance.targetScriptable.bonusItem == null)
                {
                    var label = new Label("Score");
                    targetParameters.Add(label);
                }

                var amountField = new IntegerField();
                amountField.style.width = 50;
                amountField.value = targetInstance.amount;
                amountField.RegisterValueChangedCallback(evt =>
                {
                    targetInstance.amount = evt.newValue;
                    EditorUtility.SetDirty(target);
                });

                var container = new VisualElement { style = { flexDirection = FlexDirection.Column } };

                if (targetInstance.targetScriptable != null && targetInstance.targetScriptable.bonusItem != null)
                {
                    var bonusItemSerializedObject = new SerializedObject(targetInstance.targetScriptable.bonusItem);
                    var prefabProperty = bonusItemSerializedObject.FindProperty("prefab");

                    var iconDrawer = new IconDrawer();
                    var iconField = iconDrawer.CreatePropertyGUI(prefabProperty);
                    iconField.style.width = 25;
                    iconField.style.height = 25;
                    iconField.style.marginLeft = 25;

                    container.Add(iconField);
                }

                amountField.style.marginLeft = 25;
                container.Add(amountField);
                targetParameters.Add(container);
            }
        }

        private void ToolPanel()
        {
            var actionContainer = new VisualElement { name = "action-container" };
            actionContainer.Add(CreateButton("Clear All", Color.white, "", false, ClearAll));

            var cellGrey = CreateButton("", Color.white, "tool-cell", true, () => SwitchBrush("Grey"));
            cellGrey.style.marginLeft = 50;
            actionContainer.Add(cellGrey);
            cellGreyWithBonus = CreateButton("O", Color.black, "tool-cell", true, () => SwitchBrush("GreyWithBonus"));
            actionContainer.Add(cellGreyWithBonus);
            actionContainer.Add(CreateButton("X", Color.black, "tool-cell", true, DeleteItem));
            root.Add(actionContainer);
            UpdateToolPanel();
        }

        private void SwitchBrush(string grey)
        {
            brush = brush != grey ? grey : "";
        }

        private void Randomize()
        {
            var random = new Random();
            ItemTemplate randomTemplate = null;

            if (level.levelType.singleColorMode)
            {
                randomTemplate = availableTemplates[random.Next(1, availableTemplates.Count)];
            }

            if (level.levelType.elevelType == ELevelType.CollectItems)
            {
                RandomizeCollectItemsLevel(random);
            }
            else
            {
                RandomizeOtherLevelTypes(random);
            }

            if (!symmetricalGenerationToggle.value)
            {
                RandomizeMatrix(random, level.levelType.singleColorMode, randomTemplate);
            }
            else
            {
                RandomizeSymmetricalMatrix(random, level.levelType.singleColorMode, randomTemplate);
            }

            EnsureNoFullRowsOrColumns(random);

            // Check if the matrix is empty and regenerate if necessary
            if (IsMatrixEmpty())
            {
                Randomize();
            }

            UpdateMatrixUI();

            Save();
        }

        private bool IsMatrixEmpty()
        {
            for (var i = 0; i < level.rows; i++)
            {
                for (var j = 0; j < level.columns; j++)
                {
                    if (level.GetItem(i, j) != null)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void RandomizeCollectItemsLevel(Random random)
        {
            var targetCount = random.Next(1, 6);
            var selectedIndexes = new HashSet<int>();

            for (var i = 0; i < level.targetInstance.Count; i++)
            {
                level.targetInstance[i].amount = 0;
            }

            while (selectedIndexes.Count < targetCount)
            {
                var index = random.Next(0, level.targetInstance.Count);
                selectedIndexes.Add(index);
            }

            var totalAmount = 0;
            foreach (var index in selectedIndexes)
            {
                int[] possibleAmounts = { 5, 10, 15 };
                var amount = possibleAmounts[random.Next(0, possibleAmounts.Length)];

                if (totalAmount + amount > 15)
                {
                    amount = 15 - totalAmount;
                }

                level.targetInstance[index].amount = amount;
                totalAmount += amount;

                if (totalAmount >= 15)
                {
                    break;
                }
            }

            CreateBonusItemColorUI();
        }

        private void RandomizeOtherLevelTypes(Random random)
        {
            foreach (var t in level.targetInstance)
            {
                var randomValue = random.Next(50, 500);
                var roundedTo50 = (int)Math.Round(randomValue / 50.0) * 50;
                t.amount = roundedTo50;
            }

            CreateBonusItemColorUI();
        }

        private void RandomizeMatrix(Random random, bool singleColorMode, ItemTemplate randomTemplate)
        {
            for (var i = 0; i < level.rows; i++)
            {
                for (var j = 0; j < level.columns; j++)
                {
                    SetRandomItem(random, singleColorMode, randomTemplate, i, j);
                }
            }
        }

        private void RandomizeSymmetricalMatrix(Random random, bool gameSettings, ItemTemplate randomTemplate)
        {
            for (var i = 0; i < level.rows / 2; i++)
            {
                for (var j = 0; j < level.columns / 2; j++)
                {
                    SetRandomItem(random, gameSettings, randomTemplate, i, j);
                }
            }

            MirrorMatrix();
        }

        private void SetRandomItem(Random random, bool singleColorMode, ItemTemplate randomTemplate, int i, int j)
        {
            if (random.Next(0, 100) > level.emptyCellPercentage)
            {
                level.SetItem(i, j, null);
                level.SetBonus(i, j, false);
            }
            else
            {
                var template = singleColorMode ? randomTemplate : availableTemplates[random.Next(1, availableTemplates.Count)];
                var bonus = random.Next(0, 2) == 0;

                if (bonus && level.levelType.targets[0].bonusItem != null)
                {
                    level.SetItem(i, j, availableTemplates[1]);
                    level.SetBonus(i, j, true);
                }
                else
                {
                    level.SetItem(i, j, template);
                    level.SetBonus(i, j, false);
                }
            }
        }

        private void MirrorMatrix()
        {
            for (var i = 0; i < level.rows / 2; i++)
            {
                for (var j = 0; j < level.columns / 2; j++)
                {
                    level.SetItem(i, level.columns - j - 1, level.GetItem(i, j));
                    level.SetBonus(i, level.columns - j - 1, level.GetBonus(i, j));

                    level.SetItem(level.rows - i - 1, j, level.GetItem(i, j));
                    level.SetBonus(level.rows - i - 1, j, level.GetBonus(i, j));

                    level.SetItem(level.rows - i - 1, level.columns - j - 1, level.GetItem(i, j));
                    level.SetBonus(level.rows - i - 1, level.columns - j - 1, level.GetBonus(i, j));
                }
            }
        }

        private void EnsureNoFullRowsOrColumns(Random random)
        {
            for (var i = 0; i < level.rows; i++)
            {
                if (IsRowFull(i))
                {
                    var randomColumn = random.Next(0, level.columns);
                    level.SetItem(i, randomColumn, null);
                    level.SetBonus(i, randomColumn, false);
                }
            }

            for (var i = 0; i < level.columns; i++)
            {
                if (IsColumnFull(i))
                {
                    var randomRow = random.Next(0, level.rows);
                    level.SetItem(randomRow, i, null);
                    level.SetBonus(randomRow, i, false);
                }
            }
        }

        private bool IsRowFull(int row)
        {
            for (var j = 0; j < level.columns; j++)
            {
                if (level.GetItem(row, j) == null)
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsColumnFull(int column)
        {
            for (var j = 0; j < level.rows; j++)
            {
                if (level.GetItem(j, column) == null)
                {
                    return false;
                }
            }

            return true;
        }

        private void DeleteItem()
        {
            brush = brush == "X" ? "" : "X";
        }

        private void ResizeMatrix()
        {
            var newRows = Mathf.Max(1, rowsField.value);
            var newColumns = Mathf.Max(1, columnsField.value);
            level.Resize(newRows, newColumns);
            UpdateMatrixUI();
            Save();
        }

        private void UpdateMatrixUI()
        {
            matrixContainer.Clear();

            for (var i = 0; i < level.rows; i++)
            {
                var row = new VisualElement();
                row.AddToClassList("grid-row");
                matrixContainer.Add(row);

                for (var j = 0; j < level.columns; j++)
                {
                    var cell = new Button();
                    cell.AddToClassList("grid-cell");
                    int x = i, y = j; // Capture loop variables
                    var item = level.GetItem(x, y);
                    var color = item != null ? (Color?)item.overlayColor : null;
                    UpdateCellColor(cell, level.GetBonus(x, y), color);

                    if (level.IsDisabled(x, y))
                    {
                        cell.style.backgroundColor = new StyleColor(_disableColor);
                    }
                    else if (level.IsCellHighlighted(x, y))
                    {
                        cell.style.backgroundColor = new StyleColor(_highlightColor);
                    }

                    cell.RegisterCallback<MouseDownEvent>(evt =>
                    {
                        if (evt.button == 1) // Right-click
                        {
                            ShowContextMenu(x, y, cell);
                            evt.StopPropagation();
                        }
                    });

                    cell.clicked += () =>

                    {
                        if (brush == "X")
                        {
                            level.SetItem(x, y, availableTemplates[0]);
                            level.SetBonus(x, y, false);
                        }
                        else if (brush == "Grey")
                        {
                            level.SetItem(x, y, availableTemplates[1]);
                            level.SetBonus(x, y, false);
                        }
                        else if (brush == "GreyWithBonus")
                        {
                            level.SetItem(x, y, availableTemplates[1]);
                            level.SetBonus(x, y, true);
                        }
                        else
                        {
                            CycleItemTemplate(x, y);
                        }

                        var newItem = level.GetItem(x, y);
                        var newColor = newItem != null ? (Color?)newItem.overlayColor : null;
                        UpdateCellColor(cell, level.GetBonus(x, y), newColor);
                        Save();
                    };

                    row.Add(cell);
                }
            }

            // Update the IntegerFields to reflect the current level dimensions
            rowsField.SetValueWithoutNotify(level.rows);
            columnsField.SetValueWithoutNotify(level.columns);
        }

        private void ShowContextMenu(int x, int y, Button cell)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Highlight"), level.IsCellHighlighted(x, y), () => HighlightCell(x, y, cell));
            menu.AddItem(new GUIContent("Disable"), level.IsDisabled(x, y), () => DisableCell(x, y, cell));
            menu.ShowAsContext();
        }

        private void HighlightCell(int x, int y, Button cell)
        {
            level.HighlightCellToggle(x, y);
            UpdateCellColor(cell, level.GetBonus(x, y), level.IsCellHighlighted(x, y) ? _highlightColor : null);
            Save();
        }

        private void DisableCell(int x, int y, Button cell)
        {
            level.DisableCellToggle(x, y);
            UpdateCellColor(cell, level.GetBonus(x, y), level.IsDisabled(x, y) ? _disableColor : null);
            Save();
        }

        private void CycleItemTemplate(int x, int y)
        {
            var currentIndex = availableTemplates.IndexOf(level.GetItem(x, y));
            var nextIndex = currentIndex == 0 ? 1 : (currentIndex + 1) % availableTemplates.Count;
            if (nextIndex == 0)
            {
                nextIndex = 1; // Ensure we skip the 0th element
            }

            level.SetItem(x, y, availableTemplates[nextIndex]);
        }

        private void UpdateCellColor(Button cell, bool bonus, Color? templateOverlayColor)
        {
            if (templateOverlayColor != null)
            {
                cell.style.backgroundColor = templateOverlayColor.Value;
                if (bonus)
                {
                    cell.Clear();
                    cell.Add(new Label("O") { style = { color = Color.black } });
                    cell.style.justifyContent = Justify.Center;
                }
                else
                {
                    cell.Clear();
                }
            }
            else
            {
                cell.style.backgroundColor = StyleKeyword.Null;
                cell.Clear();
            }
        }

        private Button CreateButton(string text, StyleColor colorLabel, string styleClass, bool pressedState, Action clickEvent)
        {
            var button = new Button(clickEvent);
            // label
            var label = new Label(text);
            label.style.color = colorLabel;
            button.style.flexGrow = 1;
            button.style.justifyContent = Justify.Center;
            button.Add(label);

            if (!string.IsNullOrEmpty(styleClass))
            {
                button.AddToClassList(styleClass);
            }

            if (pressedState)
            {
                button.RegisterCallback<ClickEvent>(_ => ToggleActiveState());
            }

            void ToggleActiveState()
            {
                if (button.ClassListContains("pressed"))
                {
                    button.RemoveFromClassList("pressed");
                }
                else
                {
                    //remove all other active buttons
                    foreach (var child in button.parent.Children())
                    {
                        child.RemoveFromClassList("pressed");
                    }

                    button.AddToClassList("pressed");
                }
            }

            return button;
        }

        private void ClearAll()
        {
            for (var i = 0; i < level.rows; i++)
            {
                for (var j = 0; j < level.columns; j++)
                {
                    level.SetItem(i, j, null);
                    level.levelRows[i].disabled[j] = false;
                    level.levelRows[i].highlighted[j] = false;
                }
            }

            UpdateMatrixUI();
            Save();
        }

        public void Save()
        {
            EditorUtility.SetDirty(target);
            // AssetDatabase.SaveAssetIfDirty(target);
        }

        /// <summary>
        /// 创建难度系统UI
        /// </summary>
        private VisualElement CreateDifficultySystemUI()
        {
            var container = new VisualElement { name = "difficulty-system-container" };
            container.style.marginTop = 10;
            container.style.marginBottom = 10;
            container.style.paddingTop = 10;
            container.style.paddingBottom = 10;
            container.style.paddingLeft = 10;
            container.style.paddingRight = 10;
            container.style.borderTopWidth = 1;
            container.style.borderBottomWidth = 1;
            container.style.borderTopColor = new Color(0.3f, 0.3f, 0.3f);
            container.style.borderBottomColor = new Color(0.3f, 0.3f, 0.3f);

            // Header
            var headerLabel = new Label("Difficulty System (难度系统)");
            headerLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            headerLabel.style.marginBottom = 10;
            container.Add(headerLabel);

            // Difficulty Analysis Section (Shape Generation Config)
            var analysisSection = CreateDifficultyAnalysisUI();
            container.Add(analysisSection);

            container.Add(new Label("") { style = { marginTop = 10 } });

            // Custom Probabilities Section (保存容器引用供后续刷新)
            customProbabilitiesContainer = new VisualElement();
            container.Add(customProbabilitiesContainer);
            RefreshCustomProbabilities();

            container.Add(new Label("") { style = { marginTop = 10 } });

            // Calculate Difficulty Button（计算难度按钮）- 移到最下方
            var calculateButton = new Button(() =>
            {
                if (Application.isPlaying)
                {
                    // 运行时模式：使用单例实例
                    var calculator = LevelDifficultyCalculator.Instance;
                    if (calculator == null)
                    {
                        EditorUtility.DisplayDialog("错误",
                            "LevelDifficultyCalculator未初始化\n请确保场景中存在GameManager",
                            "确定");
                        return;
                    }

                    calculator.CalculateAndSave(level);
                }
                else
                {
                    // 编辑器模式：使用静态方法
                    LevelDifficultyCalculator.CalculateInEditor(level);
                }

                // 刷新UI显示
                EditorUtility.SetDirty(level);

                // 刷新难度信息显示
                RefreshDifficultyInfo();

                Debug.Log($"[LevelEditor] 计算完成: {level.name} - {level.difficultyLevel} ({level.difficultyScore:F2})");
            })
            {
                text = "Calculate Difficulty (计算难度)"
            };
            calculateButton.style.height = 30;
            calculateButton.style.backgroundColor = new Color(0.3f, 0.5f, 0.7f);
            container.Add(calculateButton);

            // Calculated Result (保存容器引用供后续刷新) - 移到最下方
            difficultyInfoContainer = CreateDifficultyInfoUI();
            container.Add(difficultyInfoContainer);

            return container;
        }

        /// <summary>
        /// 创建难度分析UI（Shape Generation Config）
        /// </summary>
        private VisualElement CreateDifficultyAnalysisUI()
        {
            var section = new VisualElement();
            section.style.marginBottom = 10;
            section.style.paddingBottom = 10;
            section.style.borderBottomWidth = 1;
            section.style.borderBottomColor = new Color(0.2f, 0.2f, 0.2f);

            var sectionLabel = new Label("Shape Generation Config (方块生成配置)");
            sectionLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            sectionLabel.style.fontSize = 12;
            sectionLabel.style.marginBottom = 5;
            section.Add(sectionLabel);

            // Shape Generation Config Section（方块生成配置）
            var configContainer = new VisualElement();
            configContainer.style.marginTop = 10;
            configContainer.style.paddingTop = 10;
            configContainer.style.paddingBottom = 10;
            configContainer.style.paddingLeft = 10;
            configContainer.style.paddingRight = 10;
            configContainer.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f);

            var helpLabel = new Label("💡 选择方块权重等级（控制运行时生成的方块类型分布）");
            helpLabel.style.fontSize = 10;
            helpLabel.style.color = new Color(0.6f, 0.6f, 0.6f);
            helpLabel.style.marginBottom = 10;
            helpLabel.style.whiteSpace = WhiteSpace.Normal;
            configContainer.Add(helpLabel);

            // Shape Weight Level 下拉框（方块权重配置，独立于计算结果）
            var difficultyField = new EnumField("Shape Weight Level (方块权重等级)", level.shapeWeightLevel);
            difficultyField.RegisterValueChangedCallback(evt =>
            {
                level.shapeWeightLevel = (DifficultyLevel)evt.newValue;
                EditorUtility.SetDirty(level);
            });
            configContainer.Add(difficultyField);

            // 提示文本
            var presetInfoLabel = new Label("💡 Tutorial/Easy=更多基础块，Hard/Master=更多异形块/大块");
            presetInfoLabel.style.fontSize = 9;
            presetInfoLabel.style.color = new Color(0.5f, 0.5f, 0.5f);
            presetInfoLabel.style.marginTop = 5;
            presetInfoLabel.style.marginLeft = 10;
            presetInfoLabel.style.whiteSpace = WhiteSpace.Normal;
            configContainer.Add(presetInfoLabel);

            var customHintLabel = new Label("或启用下方的自定义权重进行精细配置");
            customHintLabel.style.fontSize = 9;
            customHintLabel.style.color = new Color(0.5f, 0.5f, 0.5f);
            customHintLabel.style.marginTop = 2;
            customHintLabel.style.marginLeft = 10;
            configContainer.Add(customHintLabel);

            section.Add(configContainer);

            return section;
        }

        /// <summary>
        /// 创建难度信息显示UI
        /// </summary>
        private VisualElement CreateDifficultyInfoUI()
        {
            var container = new VisualElement();
            container.style.marginTop = 10;
            container.style.paddingTop = 10;
            container.style.paddingBottom = 10;
            container.style.paddingLeft = 10;
            container.style.paddingRight = 10;
            container.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f);

            // 添加说明标签
            var infoLabel = new Label("📊 Calculated Result (计算结果)");
            infoLabel.style.fontSize = 11;
            infoLabel.style.color = new Color(0.3f, 0.7f, 1f);
            infoLabel.style.marginBottom = 8;
            container.Add(infoLabel);

            // Difficulty Level
            var levelContainer = new VisualElement();
            levelContainer.style.flexDirection = FlexDirection.Row;
            levelContainer.style.marginBottom = 5;

            var levelLabel = new Label("Difficulty Level:");
            levelLabel.style.width = 120;
            levelContainer.Add(levelLabel);

            var levelValueLabel = new Label(level.difficultyLevel.ToString());
            levelValueLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            levelValueLabel.style.color = GetDifficultyColor(level.difficultyLevel);
            levelContainer.Add(levelValueLabel);

            container.Add(levelContainer);

            // Difficulty Score
            var scoreContainer = new VisualElement();
            scoreContainer.style.flexDirection = FlexDirection.Row;
            scoreContainer.style.marginBottom = 5;

            var scoreLabel = new Label("Difficulty Score:");
            scoreLabel.style.width = 120;
            scoreContainer.Add(scoreLabel);

            var scoreValueLabel = new Label(level.difficultyScore.ToString("F2"));
            scoreValueLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            scoreContainer.Add(scoreValueLabel);

            container.Add(scoreContainer);

            // Six Dimensions Breakdown
            if (level.breakdown != null && level.difficultyScore > 0)
            {
                var breakdownLabel = new Label("Six Dimensions:");
                breakdownLabel.style.marginTop = 5;
                breakdownLabel.style.fontSize = 11;
                breakdownLabel.style.color = new Color(0.7f, 0.7f, 0.7f);
                container.Add(breakdownLabel);

                // Create formatted breakdown text
                var breakdownText = string.Format(
                    "Space Stress: {0:F1}  |  Shape Complexity: {1:F1}\n" +
                    "Target Pressure: {2:F1}  |  Time Pressure: {3:F1}\n" +
                    "Resource Constraint: {4:F1}  |  Strategy Depth: {5:F1}",
                    level.breakdown.spaceStress,
                    level.breakdown.shapeComplexity,
                    level.breakdown.targetPressure,
                    level.breakdown.timePressure,
                    level.breakdown.resourceConstraint,
                    level.breakdown.strategyDepth
                );

                var breakdownTextLabel = new Label(breakdownText);
                breakdownTextLabel.style.fontSize = 10;
                breakdownTextLabel.style.whiteSpace = WhiteSpace.Normal;
                breakdownTextLabel.style.marginLeft = 10;
                breakdownTextLabel.style.color = new Color(0.6f, 0.6f, 0.6f);
                container.Add(breakdownTextLabel);
            }

            return container;
        }

        /// <summary>
        /// 刷新难度信息显示
        /// </summary>
        private void RefreshDifficultyInfo()
        {
            if (difficultyInfoContainer == null)
                return;

            // 清空容器
            difficultyInfoContainer.Clear();

            // 添加说明标签
            var infoLabel = new Label("📊 Calculated Result (计算结果)");
            infoLabel.style.fontSize = 11;
            infoLabel.style.color = new Color(0.3f, 0.7f, 1f);
            infoLabel.style.marginBottom = 8;
            difficultyInfoContainer.Add(infoLabel);

            // Difficulty Level
            var levelContainer = new VisualElement();
            levelContainer.style.flexDirection = FlexDirection.Row;
            levelContainer.style.marginBottom = 5;

            var levelLabel = new Label("Difficulty Level:");
            levelLabel.style.width = 120;
            levelContainer.Add(levelLabel);

            var levelValueLabel = new Label(level.difficultyLevel.ToString());
            levelValueLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            levelValueLabel.style.color = GetDifficultyColor(level.difficultyLevel);
            levelContainer.Add(levelValueLabel);

            difficultyInfoContainer.Add(levelContainer);

            // Difficulty Score
            var scoreContainer = new VisualElement();
            scoreContainer.style.flexDirection = FlexDirection.Row;
            scoreContainer.style.marginBottom = 5;

            var scoreLabel = new Label("Difficulty Score:");
            scoreLabel.style.width = 120;
            scoreContainer.Add(scoreLabel);

            var scoreValueLabel = new Label(level.difficultyScore.ToString("F2"));
            scoreValueLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            scoreContainer.Add(scoreValueLabel);

            difficultyInfoContainer.Add(scoreContainer);

            // Six Dimensions Breakdown
            if (level.breakdown != null && level.difficultyScore > 0)
            {
                var breakdownLabel = new Label("Six Dimensions:");
                breakdownLabel.style.marginTop = 5;
                breakdownLabel.style.fontSize = 11;
                breakdownLabel.style.color = new Color(0.7f, 0.7f, 0.7f);
                difficultyInfoContainer.Add(breakdownLabel);

                // Create formatted breakdown text
                var breakdownText = string.Format(
                    "Space Stress: {0:F1}  |  Shape Complexity: {1:F1}\n" +
                    "Target Pressure: {2:F1}  |  Time Pressure: {3:F1}\n" +
                    "Resource Constraint: {4:F1}  |  Strategy Depth: {5:F1}",
                    level.breakdown.spaceStress,
                    level.breakdown.shapeComplexity,
                    level.breakdown.targetPressure,
                    level.breakdown.timePressure,
                    level.breakdown.resourceConstraint,
                    level.breakdown.strategyDepth
                );

                var breakdownTextLabel = new Label(breakdownText);
                breakdownTextLabel.style.fontSize = 10;
                breakdownTextLabel.style.whiteSpace = WhiteSpace.Normal;
                breakdownTextLabel.style.marginLeft = 10;
                breakdownTextLabel.style.color = new Color(0.6f, 0.6f, 0.6f);
                difficultyInfoContainer.Add(breakdownTextLabel);
            }
        }

        /// <summary>
        /// 创建自定义权重配置UI
        /// </summary>
        private VisualElement CreateCustomProbabilitiesUI()
        {
            var section = new VisualElement();

            var sectionLabel = new Label("Custom Shape Probabilities (自定义方块权重)");
            sectionLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            sectionLabel.style.fontSize = 12;
            sectionLabel.style.marginBottom = 5;
            section.Add(sectionLabel);

            // 添加配置方式说明
            var methodLabel = new Label(!level.useCustomProbabilities
                ? "📌 当前使用: Shape Weight Level 快捷预设"
                : "📌 当前使用: 自定义权重配置");
            methodLabel.style.fontSize = 10;
            methodLabel.style.color = new Color(0.3f, 0.8f, 0.5f);
            methodLabel.style.marginBottom = 5;
            section.Add(methodLabel);

            var helpLabel = new Label("启用后将使用自定义权重，而不是 Shape Weight Level 快捷预设。");
            helpLabel.style.fontSize = 10;
            helpLabel.style.color = new Color(0.6f, 0.6f, 0.6f);
            helpLabel.style.marginBottom = 10;
            helpLabel.style.whiteSpace = WhiteSpace.Normal;
            section.Add(helpLabel);

            // Enable toggle
            var enableToggle = new Toggle("Enable Custom Probabilities (启用自定义权重)")
            {
                value = level.useCustomProbabilities
            };
            enableToggle.RegisterValueChangedCallback(evt =>
            {
                level.useCustomProbabilities = evt.newValue;
                EditorUtility.SetDirty(level);

                // 刷新自定义权重UI
                RefreshCustomProbabilities();
            });
            section.Add(enableToggle);

            if (level.useCustomProbabilities)
            {
                var configContainer = new VisualElement();
                configContainer.style.marginTop = 10;
                configContainer.style.paddingTop = 10;
                configContainer.style.paddingBottom = 10;
                configContainer.style.paddingLeft = 10;
                configContainer.style.paddingRight = 10;
                configContainer.style.backgroundColor = new Color(0.25f, 0.25f, 0.25f);

                // Quick presets
                var presetContainer = new VisualElement();
                presetContainer.style.flexDirection = FlexDirection.Row;
                presetContainer.style.marginBottom = 10;

                var presetLabel = new Label("Quick Presets:");
                presetLabel.style.width = 120;
                presetLabel.style.alignSelf = Align.Center;
                presetContainer.Add(presetLabel);

                var simpleButton = CreatePresetButton("Simple", 0.7f, 0.2f, 0.1f);
                presetContainer.Add(simpleButton);

                var normalButton = CreatePresetButton("Normal", 0.5f, 0.35f, 0.15f);
                presetContainer.Add(normalButton);

                var hardButton = CreatePresetButton("Hard", 0.3f, 0.45f, 0.25f);
                presetContainer.Add(hardButton);

                configContainer.Add(presetContainer);

                // Basic slider
                var basicSlider = CreateProbabilitySlider("Basic (基础块)",
                    level.customBaseline.basic,
                    value =>
                    {
                        level.customBaseline.basic = value;
                        NormalizeProbabilities();
                        EditorUtility.SetDirty(level);
                    });
                configContainer.Add(basicSlider);

                // Shaped slider
                var shapedSlider = CreateProbabilitySlider("Shaped (异形块)",
                    level.customBaseline.shaped,
                    value =>
                    {
                        level.customBaseline.shaped = value;
                        NormalizeProbabilities();
                        EditorUtility.SetDirty(level);
                    });
                configContainer.Add(shapedSlider);

                // Large slider
                var largeSlider = CreateProbabilitySlider("Large (大块)",
                    level.customBaseline.large,
                    value =>
                    {
                        level.customBaseline.large = value;
                        NormalizeProbabilities();
                        EditorUtility.SetDirty(level);
                    });
                configContainer.Add(largeSlider);

                // Sum display
                var sumContainer = new VisualElement();
                sumContainer.style.flexDirection = FlexDirection.Row;
                sumContainer.style.marginTop = 10;

                var sumLabel = new Label("Total:");
                sumLabel.style.width = 120;
                sumContainer.Add(sumLabel);

                float sum = level.customBaseline.basic + level.customBaseline.shaped + level.customBaseline.large;
                var sumValueLabel = new Label(sum.ToString("F2"));
                sumValueLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                sumValueLabel.style.color = Mathf.Abs(sum - 1.0f) < 0.01f ? Color.green : Color.yellow;
                sumContainer.Add(sumValueLabel);

                configContainer.Add(sumContainer);

                section.Add(configContainer);
            }

            return section;
        }

        /// <summary>
        /// 创建预设按钮
        /// </summary>
        private Button CreatePresetButton(string name, float basic, float shaped, float large)
        {
            var button = new Button(() =>
            {
                level.customBaseline.basic = basic;
                level.customBaseline.shaped = shaped;
                level.customBaseline.large = large;
                EditorUtility.SetDirty(level);

                // 刷新自定义权重UI
                RefreshCustomProbabilities();
            })
            {
                text = name
            };
            button.style.flexGrow = 1;
            button.style.marginLeft = 5;
            button.style.backgroundColor = new Color(0.3f, 0.5f, 0.3f);
            return button;
        }

        /// <summary>
        /// 创建权重滑块
        /// </summary>
        private VisualElement CreateProbabilitySlider(string label, float value, Action<float> onChange)
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.marginBottom = 5;
            container.style.alignItems = Align.Center;

            var labelElement = new Label(label);
            labelElement.style.width = 120;
            container.Add(labelElement);

            var slider = new Slider(0f, 1f);
            slider.style.flexGrow = 1;
            slider.value = value;
            slider.RegisterValueChangedCallback(evt =>
            {
                onChange(evt.newValue);
            });
            container.Add(slider);

            var valueLabel = new Label(value.ToString("F2"));
            valueLabel.style.width = 40;
            valueLabel.style.marginLeft = 5;
            slider.RegisterValueChangedCallback(evt =>
            {
                valueLabel.text = evt.newValue.ToString("F2");
            });
            container.Add(valueLabel);

            return container;
        }

        /// <summary>
        /// 归一化权重
        /// </summary>
        private void NormalizeProbabilities()
        {
            float sum = level.customBaseline.basic + level.customBaseline.shaped + level.customBaseline.large;
            if (sum > 0f && Mathf.Abs(sum - 1.0f) > 0.01f)
            {
                level.customBaseline.basic /= sum;
                level.customBaseline.shaped /= sum;
                level.customBaseline.large /= sum;
            }
        }

        /// <summary>
        /// 获取难度等级的颜色
        /// </summary>
        private Color GetDifficultyColor(DifficultyLevel difficultyLevel)
        {
            switch (difficultyLevel)
            {
                case DifficultyLevel.Tutorial: return new Color(0.5f, 1f, 0.5f);
                case DifficultyLevel.Easy: return new Color(0.7f, 1f, 0.7f);
                case DifficultyLevel.Normal: return new Color(1f, 1f, 0.5f);
                case DifficultyLevel.Hard: return new Color(1f, 0.7f, 0.3f);
                case DifficultyLevel.Expert: return new Color(1f, 0.4f, 0.4f);
                case DifficultyLevel.Master: return new Color(1f, 0.2f, 0.2f);
                default: return Color.white;
            }
        }

        /// <summary>
        /// 刷新自定义权重配置UI
        /// </summary>
        private void RefreshCustomProbabilities()
        {
            if (customProbabilitiesContainer == null)
                return;

            // 清空容器
            customProbabilitiesContainer.Clear();

            // 重新创建内容
            var content = CreateCustomProbabilitiesUI();

            // 将内容的所有子元素添加到容器
            while (content.childCount > 0)
            {
                var child = content[0];
                content.RemoveAt(0);
                customProbabilitiesContainer.Add(child);
            }
        }
    }
}