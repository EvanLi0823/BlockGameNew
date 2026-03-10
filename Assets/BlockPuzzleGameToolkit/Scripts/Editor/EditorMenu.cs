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

using BlockPuzzleGameToolkit.Scripts.Gameplay;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using BlockPuzzleGameToolkit.Scripts.PropSystem.Core;
using BlockPuzzleGameToolkit.Scripts.Settings;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Editor
{
    public static class EditorMenu
    {
        public static string BlockPuzzleGameToolkit = "BlockPuzzleGameToolkit";

        // CoinsShopSettings 已删除
        /*
        [MenuItem("Tools/" + nameof(BlockPuzzleGameToolkit) + "/Settings/Shop settings")]
        public static void IAPProducts()
        {
            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath("Assets/" + BlockPuzzleGameToolkit + "/Resources/Settings/CoinsShopSettings.asset");
        }
        */

        // AdsSettings 已删除
        /*
        [MenuItem("Tools/" + nameof(BlockPuzzleGameToolkit) + "/Settings/Ads settings")]
        public static void AdsSettings()
        {
            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath("Assets/" + BlockPuzzleGameToolkit + "/Resources/Settings/AdsSettings.asset");
        }
        */

        //DailyBonusSettings
        [MenuItem("Tools/" + nameof(BlockPuzzleGameToolkit) + "/Settings/Daily bonus settings")]
        public static void DailyBonusSettings()
        {
            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath("Assets/" + BlockPuzzleGameToolkit + "/Resources/Settings/DailyBonusSettings.asset");
        }

        //GameSettings
        [MenuItem("Tools/" + nameof(BlockPuzzleGameToolkit) + "/Settings/Game settings")]
        public static void GameSettings()
        {
            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath("Assets/" + BlockPuzzleGameToolkit + "/Resources/Settings/GameSettings.asset");
        }

        //SpinSettings
        [MenuItem("Tools/" + nameof(BlockPuzzleGameToolkit) + "/Settings/Spin settings")]
        public static void SpinSettings()
        {
            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath("Assets/" + BlockPuzzleGameToolkit + "/Resources/Settings/SpinSettings.asset");
        }

        //DebugSettings
        [MenuItem("Tools/" + nameof(BlockPuzzleGameToolkit) + "/Settings/Debug settings")]
        public static void DebugSettings()
        {
            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath("Assets/" + BlockPuzzleGameToolkit + "/Resources/Settings/DebugSettings.asset");
        }

        //TutorialSettings
        [MenuItem("Tools/" + nameof(BlockPuzzleGameToolkit) + "/Settings/Tutorial settings")]
        public static void TutorialSettings()
        {
            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath("Assets/" + BlockPuzzleGameToolkit + "/Resources/Settings/TutorialSettings.asset");
        }

        //NativeBridgeSettings
        [MenuItem("Tools/" + nameof(BlockPuzzleGameToolkit) + "/Settings/Native Bridge settings")]
        public static void NativeBridgeSettings()
        {
            var settings = AssetDatabase.LoadMainAssetAtPath("Assets/" + BlockPuzzleGameToolkit + "/Resources/Settings/NativeBridgeSettings.asset");
            if (settings == null)
            {
                // 如果不存在，创建新的设置文件
                var newSettings = ScriptableObject.CreateInstance<BlockPuzzleGameToolkit.Scripts.Settings.NativeBridgeSettings>();

                // 设置默认值
                newSettings.enableNativeBridge = true;
                newSettings.androidPackageName = "com.blockpuzzle.game.NativeBridge";
                newSettings.androidMethodName = "callUnity";
                newSettings.iOSMethodName = "callNative";
                newSettings.enableDebugLogs = true;
                newSettings.mockResponseInEditor = true;
                newSettings.autoInitialize = true;
                newSettings.autoRequestCommonParams = true;
                newSettings.autoRequestWhiteBao = true;
                newSettings.autoRequestCurrency = true;
                newSettings.nativeCallTimeout = 5f;

                // 确保目录存在
                if (!AssetDatabase.IsValidFolder("Assets/" + BlockPuzzleGameToolkit + "/Resources"))
                {
                    AssetDatabase.CreateFolder("Assets/" + BlockPuzzleGameToolkit, "Resources");
                }
                if (!AssetDatabase.IsValidFolder("Assets/" + BlockPuzzleGameToolkit + "/Resources/Settings"))
                {
                    AssetDatabase.CreateFolder("Assets/" + BlockPuzzleGameToolkit + "/Resources", "Settings");
                }

                // 创建资源文件
                AssetDatabase.CreateAsset(newSettings, "Assets/" + BlockPuzzleGameToolkit + "/Resources/Settings/NativeBridgeSettings.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log("[NativeBridge] Settings created at: Assets/" + BlockPuzzleGameToolkit + "/Resources/Settings/NativeBridgeSettings.asset");
                Selection.activeObject = newSettings;
            }
            else
            {
                Selection.activeObject = settings;
            }
        }

        //AdSystemSettings
        [MenuItem("Tools/" + nameof(BlockPuzzleGameToolkit) + "/Settings/Ad System settings")]
        public static void AdSystemSettings()
        {
            var settings = AssetDatabase.LoadMainAssetAtPath("Assets/" + BlockPuzzleGameToolkit + "/Resources/Settings/AdSystemSettings.asset");
            if (settings == null)
            {
                // 如果不存在，创建新的设置文件
                var newSettings = ScriptableObject.CreateInstance<BlockPuzzleGameToolkit.Scripts.Settings.AdSystemSettings>();

                // 初始化默认配置
                newSettings.InitializeDefaults();

                // 确保目录存在
                if (!AssetDatabase.IsValidFolder("Assets/" + BlockPuzzleGameToolkit + "/Resources"))
                {
                    AssetDatabase.CreateFolder("Assets/" + BlockPuzzleGameToolkit, "Resources");
                }
                if (!AssetDatabase.IsValidFolder("Assets/" + BlockPuzzleGameToolkit + "/Resources/Settings"))
                {
                    AssetDatabase.CreateFolder("Assets/" + BlockPuzzleGameToolkit + "/Resources", "Settings");
                }

                // 创建资源文件
                AssetDatabase.CreateAsset(newSettings, "Assets/" + BlockPuzzleGameToolkit + "/Resources/Settings/AdSystemSettings.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log("[AdSystem] Settings created at: Assets/" + BlockPuzzleGameToolkit + "/Resources/Settings/AdSystemSettings.asset");
                Selection.activeObject = newSettings;
            }
            else
            {
                Selection.activeObject = settings;
            }
        }

        // RewardSystemSettings
        [MenuItem("Tools/" + nameof(BlockPuzzleGameToolkit) + "/Settings/Reward System settings")]
        public static void RewardSystemSettings()
        {
            var settings = AssetDatabase.LoadMainAssetAtPath("Assets/" + BlockPuzzleGameToolkit + "/Resources/Settings/RewardSystemSettings.asset");
            if (settings == null)
            {
                // 如果不存在，创建新的设置文件
                var newSettings = ScriptableObject.CreateInstance<RewardSystemSettings>();

                // 确保目录存在
                if (!AssetDatabase.IsValidFolder("Assets/" + BlockPuzzleGameToolkit + "/Resources"))
                {
                    AssetDatabase.CreateFolder("Assets/" + BlockPuzzleGameToolkit, "Resources");
                }
                if (!AssetDatabase.IsValidFolder("Assets/" + BlockPuzzleGameToolkit + "/Resources/Settings"))
                {
                    AssetDatabase.CreateFolder("Assets/" + BlockPuzzleGameToolkit + "/Resources", "Settings");
                }

                // 创建资源文件
                AssetDatabase.CreateAsset(newSettings, "Assets/" + BlockPuzzleGameToolkit + "/Resources/Settings/RewardSystemSettings.asset");
                AssetDatabase.SaveAssets();

                // 添加默认配置
                newSettings.AddDefaultConfig();

                // 标记为已修改并保存
                EditorUtility.SetDirty(newSettings);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log("[RewardSystem] Settings created with default configuration at: Assets/" + BlockPuzzleGameToolkit + "/Resources/Settings/RewardSystemSettings.asset");
                Selection.activeObject = newSettings;
            }
            else
            {
                Selection.activeObject = settings;
            }
        }

        // SliderMultiplierSettings - 滑动倍率配置
        [MenuItem("Tools/" + nameof(BlockPuzzleGameToolkit) + "/Settings/Slider Multiplier Settings")]
        public static void SliderMultiplierSettings()
        {
            var settings = AssetDatabase.LoadMainAssetAtPath("Assets/" + BlockPuzzleGameToolkit + "/Resources/Settings/SliderMultiplierSettings.asset");
            if (settings == null)
            {
                // 如果不存在，创建新的设置文件
                var newSettings = ScriptableObject.CreateInstance<Settings.SliderMultiplierSettings>();

                // 确保目录存在
                if (!AssetDatabase.IsValidFolder("Assets/" + BlockPuzzleGameToolkit + "/Resources"))
                {
                    AssetDatabase.CreateFolder("Assets/" + BlockPuzzleGameToolkit, "Resources");
                }
                if (!AssetDatabase.IsValidFolder("Assets/" + BlockPuzzleGameToolkit + "/Resources/Settings"))
                {
                    AssetDatabase.CreateFolder("Assets/" + BlockPuzzleGameToolkit + "/Resources", "Settings");
                }

                // 创建资源文件
                AssetDatabase.CreateAsset(newSettings, "Assets/" + BlockPuzzleGameToolkit + "/Resources/Settings/SliderMultiplierSettings.asset");
                AssetDatabase.SaveAssets();

                // 添加默认配置
                newSettings.AddDefaultConfigs();

                // 标记为已修改并保存
                EditorUtility.SetDirty(newSettings);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log("[SliderMultiplierSettings] Settings created with default configuration at: Assets/" + BlockPuzzleGameToolkit + "/Resources/Settings/SliderMultiplierSettings.asset");
                Selection.activeObject = newSettings;
            }
            else
            {
                Selection.activeObject = settings;
            }
        }

        // LevelRewardMultiplierSettings - 关卡奖励倍率配置
        [MenuItem("Tools/" + nameof(BlockPuzzleGameToolkit) + "/Settings/Level Reward Multiplier Settings")]
        public static void LevelRewardMultiplierSettings()
        {
            var settings = AssetDatabase.LoadMainAssetAtPath("Assets/" + BlockPuzzleGameToolkit + "/Resources/Settings/LevelRewardMultiplierSettings.asset");
            if (settings == null)
            {
                // 如果不存在，创建新的设置文件
                var newSettings = ScriptableObject.CreateInstance<Settings.LevelRewardMultiplierSettings>();

                // 确保目录存在
                if (!AssetDatabase.IsValidFolder("Assets/" + BlockPuzzleGameToolkit + "/Resources"))
                {
                    AssetDatabase.CreateFolder("Assets/" + BlockPuzzleGameToolkit, "Resources");
                }
                if (!AssetDatabase.IsValidFolder("Assets/" + BlockPuzzleGameToolkit + "/Resources/Settings"))
                {
                    AssetDatabase.CreateFolder("Assets/" + BlockPuzzleGameToolkit + "/Resources", "Settings");
                }

                // 创建资源文件
                AssetDatabase.CreateAsset(newSettings, "Assets/" + BlockPuzzleGameToolkit + "/Resources/Settings/LevelRewardMultiplierSettings.asset");
                AssetDatabase.SaveAssets();

                // 添加默认配置
                newSettings.AddDefaultConfigs();

                // 标记为已修改并保存
                EditorUtility.SetDirty(newSettings);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log("[LevelRewardMultiplierSettings] Settings created with default configuration at: Assets/" + BlockPuzzleGameToolkit + "/Resources/Settings/LevelRewardMultiplierSettings.asset");
                Selection.activeObject = newSettings;
            }
            else
            {
                Selection.activeObject = settings;
            }
        }

        // FailedPopupSettings - 关卡失败弹窗配置
        [MenuItem("Tools/" + nameof(BlockPuzzleGameToolkit) + "/Settings/Failed Popup Settings")]
        public static void FailedPopupSettings()
        {
            var settings = AssetDatabase.LoadMainAssetAtPath("Assets/" + BlockPuzzleGameToolkit + "/Resources/Settings/FailedPopupSettings.asset");
            if (settings == null)
            {
                // 如果不存在，创建新的设置文件
                var newSettings = ScriptableObject.CreateInstance<Settings.FailedPopupSettings>();

                // 设置默认值
                newSettings.allowFreeRevive = true;
                newSettings.refreshShapeCount = 3;
                newSettings.maxRevivesPerLevel = 1;
                newSettings.adType = Services.Ads.AdUnits.EAdType.Rewarded;
                newSettings.allowReviveOnAdFail = true;
                newSettings.guaranteePlaceableShape = true;
                newSettings.smallShapePriority = 0.7f;
                newSettings.showProgressBar = true;
                newSettings.progressAnimationDuration = 0.5f;
                newSettings.debugFreeRevive = false;

                // 确保目录存在
                if (!AssetDatabase.IsValidFolder("Assets/" + BlockPuzzleGameToolkit + "/Resources"))
                {
                    AssetDatabase.CreateFolder("Assets/" + BlockPuzzleGameToolkit, "Resources");
                }
                if (!AssetDatabase.IsValidFolder("Assets/" + BlockPuzzleGameToolkit + "/Resources/Settings"))
                {
                    AssetDatabase.CreateFolder("Assets/" + BlockPuzzleGameToolkit + "/Resources", "Settings");
                }

                // 创建资源文件
                AssetDatabase.CreateAsset(newSettings, "Assets/" + BlockPuzzleGameToolkit + "/Resources/Settings/FailedPopupSettings.asset");

                // 标记为已修改并保存
                EditorUtility.SetDirty(newSettings);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log("[FailedPopupSettings] Settings created at: Assets/" + BlockPuzzleGameToolkit + "/Resources/Settings/FailedPopupSettings.asset");
                Selection.activeObject = newSettings;
            }
            else
            {
                Selection.activeObject = settings;
            }
        }

        // FlyRewardConfig - 飞行奖励系统配置
        [MenuItem("Tools/" + nameof(BlockPuzzleGameToolkit) + "/Settings/Fly Reward Config")]
        public static void FlyRewardConfig()
        {
            var settings = AssetDatabase.LoadMainAssetAtPath("Assets/" + BlockPuzzleGameToolkit + "/Resources/Settings/FlyRewardConfig.asset");
            if (settings == null)
            {
                // 如果不存在，创建新的设置文件
                var newSettings = ScriptableObject.CreateInstance<Settings.FlyRewardConfig>();

                // 确保目录存在
                if (!AssetDatabase.IsValidFolder("Assets/" + BlockPuzzleGameToolkit + "/Resources"))
                {
                    AssetDatabase.CreateFolder("Assets/" + BlockPuzzleGameToolkit, "Resources");
                }
                if (!AssetDatabase.IsValidFolder("Assets/" + BlockPuzzleGameToolkit + "/Resources/Settings"))
                {
                    AssetDatabase.CreateFolder("Assets/" + BlockPuzzleGameToolkit + "/Resources", "Settings");
                }

                // 创建资源文件
                AssetDatabase.CreateAsset(newSettings, "Assets/" + BlockPuzzleGameToolkit + "/Resources/Settings/FlyRewardConfig.asset");
                AssetDatabase.SaveAssets();

                // 标记为已修改并保存
                EditorUtility.SetDirty(newSettings);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log("[FlyRewardConfig] Settings created at: Assets/" + BlockPuzzleGameToolkit + "/Resources/Settings/FlyRewardConfig.asset");
                Selection.activeObject = newSettings;
            }
            else
            {
                Selection.activeObject = settings;
            }
        }

        #region 道具系统菜单 Props System

        // PropSettings - 道具初始化配置
        [MenuItem("Tools/" + nameof(BlockPuzzleGameToolkit) + "/Props/Create Prop Settings", priority = 100)]
        public static void CreatePropSettings()
        {
            string path = "Assets/" + BlockPuzzleGameToolkit + "/Resources/Settings/PropSettings.asset";

            if (System.IO.File.Exists(path))
            {
                Debug.LogWarning("PropSettings already exists at: " + path);
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<PropSettings>(path);
                return;
            }

            PropSettings settings = ScriptableObject.CreateInstance<PropSettings>();

            // 设置默认初始道具
            settings.initialProps = new List<PropData>
            {
                new PropData(PropType.Rotate, 3),
                new PropData(PropType.Refresh, 3),
                new PropData(PropType.Bomb, 1)
            };

            // 确保目录存在
            EnsureDirectoryExists();

            AssetDatabase.CreateAsset(settings, path);
            AssetDatabase.SaveAssets();

            Debug.Log("Created PropSettings at: " + path);
            Selection.activeObject = settings;
        }

        // PropPurchaseSettings - 道具购买配置
        [MenuItem("Tools/" + nameof(BlockPuzzleGameToolkit) + "/Props/Create Purchase Settings", priority = 101)]
        public static void CreatePropPurchaseSettings()
        {
            string path = "Assets/" + BlockPuzzleGameToolkit + "/Resources/Settings/PropPurchaseSettings.asset";

            if (System.IO.File.Exists(path))
            {
                Debug.LogWarning("PropPurchaseSettings already exists at: " + path);
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<PropPurchaseSettings>(path);
                return;
            }

            PropPurchaseSettings settings = ScriptableObject.CreateInstance<PropPurchaseSettings>();

            // 设置默认购买配置
            settings.purchaseConfigs = new List<PropPurchaseSettings.PropPurchaseConfig>
            {
                new PropPurchaseSettings.PropPurchaseConfig
                {
                    propType = PropType.Rotate,
                    canPurchaseWithAds = true,
                    adsRewardAmount = 3,
                    canPurchaseWithCoins = true,
                    coinPrice = 100,
                    coinPurchaseAmount = 5
                },
                new PropPurchaseSettings.PropPurchaseConfig
                {
                    propType = PropType.Refresh,
                    canPurchaseWithAds = true,
                    adsRewardAmount = 3,
                    canPurchaseWithCoins = true,
                    coinPrice = 100,
                    coinPurchaseAmount = 5
                },
                new PropPurchaseSettings.PropPurchaseConfig
                {
                    propType = PropType.Bomb,
                    canPurchaseWithAds = true,
                    adsRewardAmount = 1,
                    canPurchaseWithCoins = true,
                    coinPrice = 200,
                    coinPurchaseAmount = 3
                }
            };

            // 确保目录存在
            EnsureDirectoryExists();

            AssetDatabase.CreateAsset(settings, path);
            AssetDatabase.SaveAssets();

            Debug.Log("Created PropPurchaseSettings at: " + path);
            Selection.activeObject = settings;
        }

        // 创建所有道具配置
        [MenuItem("Tools/" + nameof(BlockPuzzleGameToolkit) + "/Props/Create All Prop Configs", priority = 102)]
        public static void CreateAllPropConfigs()
        {
            CreatePropSettings();
            CreatePropPurchaseSettings();
            CreatePropItemConfigs();

            Debug.Log("All prop configuration files created successfully!");
        }

        // 创建道具项配置
        private static void CreatePropItemConfigs()
        {
            string directory = "Assets/" + BlockPuzzleGameToolkit + "/Resources/Settings/Props";

            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
                AssetDatabase.Refresh();
            }

            // 创建旋转道具配置
            CreatePropItemConfig(directory, "RotatePropConfig", PropType.Rotate, "旋转道具", "顺时针旋转选中的方块90度");

            // 创建刷新道具配置
            CreatePropItemConfig(directory, "RefreshPropConfig", PropType.Refresh, "刷新道具", "重新生成所有待放置的方块");

            // 创建炸弹道具配置
            CreatePropItemConfig(directory, "BombPropConfig", PropType.Bomb, "炸弹道具", "清除选中位置3x3范围的格子");

            AssetDatabase.Refresh();
        }

        // 创建单个道具项配置
        private static void CreatePropItemConfig(string directory, string fileName, PropType propType, string propName, string description)
        {
            string path = $"{directory}/{fileName}.asset";

            if (System.IO.File.Exists(path))
            {
                Debug.LogWarning($"{fileName} already exists at: {path}");
                return;
            }

            PropItemConfig config = ScriptableObject.CreateInstance<PropItemConfig>();
            config.propType = propType;
            config.propName = propName;
            config.description = description;
            config.maxStack = 99;
            config.cooldownTime = 0f;
            config.highlightColor = new Color(1f, 1f, 0f, 0.5f);
            config.outlineColor = Color.yellow;

            AssetDatabase.CreateAsset(config, path);
            AssetDatabase.SaveAssets();

            Debug.Log($"Created {fileName} at: {path}");
        }

        // 确保道具系统目录存在
        private static void EnsureDirectoryExists()
        {
            if (!AssetDatabase.IsValidFolder("Assets/" + BlockPuzzleGameToolkit + "/Resources"))
            {
                AssetDatabase.CreateFolder("Assets/" + BlockPuzzleGameToolkit, "Resources");
            }
            if (!AssetDatabase.IsValidFolder("Assets/" + BlockPuzzleGameToolkit + "/Resources/Settings"))
            {
                AssetDatabase.CreateFolder("Assets/" + BlockPuzzleGameToolkit + "/Resources", "Settings");
            }
        }

        // 打开道具设置
        [MenuItem("Tools/" + nameof(BlockPuzzleGameToolkit) + "/Settings/Prop Settings")]
        public static void OpenPropSettings()
        {
            var settings = AssetDatabase.LoadMainAssetAtPath("Assets/" + BlockPuzzleGameToolkit + "/Resources/Settings/PropSettings.asset");
            if (settings != null)
            {
                Selection.activeObject = settings;
            }
            else
            {
                CreatePropSettings();
            }
        }

        // 打开道具购买设置
        [MenuItem("Tools/" + nameof(BlockPuzzleGameToolkit) + "/Settings/Prop Purchase Settings")]
        public static void OpenPropPurchaseSettings()
        {
            var settings = AssetDatabase.LoadMainAssetAtPath("Assets/" + BlockPuzzleGameToolkit + "/Resources/Settings/PropPurchaseSettings.asset");
            if (settings != null)
            {
                Selection.activeObject = settings;
            }
            else
            {
                CreatePropPurchaseSettings();
            }
        }

        #endregion

        [MenuItem("Tools/" + nameof(BlockPuzzleGameToolkit) + "/Scenes/Main scene &1", priority = 0)]
        public static void MainScene()
        {
            EditorSceneManager.OpenScene("Assets/" + BlockPuzzleGameToolkit + "/Scenes/main.unity");
            StateManager.Instance.CurrentState = EScreenStates.MainMenu;
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        [MenuItem("Tools/" + nameof(BlockPuzzleGameToolkit) + "/Scenes/Game scene &2")]
        public static void GameScene()
        {
            StateManager.Instance.CurrentState = EScreenStates.Game;
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        [MenuItem("Tools/" + nameof(BlockPuzzleGameToolkit) + "/Scenes/Map scene &3")]
        public static void MapScene()
        {
            StateManager.Instance.CurrentState = EScreenStates.Map;
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }


        [MenuItem("Tools/" + nameof(BlockPuzzleGameToolkit) + "/Editor/Level Editor _C", priority = 1)]
        public static void LevelEditor()
        {
            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath("Assets/" + BlockPuzzleGameToolkit + "/Resources/Levels/Level_1.asset");
        }

        [MenuItem("Tools/" + nameof(BlockPuzzleGameToolkit) + "/Editor/Color editor", priority = 1)]
        public static void ColorEditor()
        {
            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath("Assets/" + BlockPuzzleGameToolkit + "/Resources/Items/ItemTemplate 0.asset");
        }

        [MenuItem("Tools/" + nameof(BlockPuzzleGameToolkit) + "/Editor/Shape editor", priority = 1)]
        public static void ShapeEditor()
        {
            var shapeAssets = Resources.LoadAll("Shapes");
            if (shapeAssets.Length > 0)
            {
                Selection.activeObject = shapeAssets[0];
            }
            else
            {
                Debug.LogWarning("No shape assets found in the specified folder.");
            }
        }

        [MenuItem("Tools/" + nameof(BlockPuzzleGameToolkit) + "/Documentation/Main", priority = 2)]
        public static void MainDoc()
        {
            Application.OpenURL("https://candy-smith.gitbook.io/main");
        }

        [MenuItem("Tools/" + nameof(BlockPuzzleGameToolkit) + "/Documentation/ADS/Setup ads")]
        public static void UnityadsDoc()
        {
            Application.OpenURL("https://candy-smith.gitbook.io/bubble-shooter-toolkit/tutorials/ads-setup/");
        }

        [MenuItem("Tools/" + nameof(BlockPuzzleGameToolkit) + "/Documentation/Unity IAP (in-apps)")]
        public static void Inapp()
        {
            Application.OpenURL("https://candy-smith.gitbook.io/main/block-puzzle-game-toolkit/setting-up-in-app-purchase-products");
        }


        [MenuItem("Tools/" + nameof(BlockPuzzleGameToolkit) + "/Reset PlayerPrefs")]
        private static void ResetPlayerPrefs()
        {
            GameDataManager.ClearALlData();
            PlayerPrefs.DeleteKey("GameState");
            Debug.Log("PlayerPrefs are reset");
        }

        #region 开发者工具快捷菜单 Developer Tools

        [MenuItem("Tools/" + nameof(BlockPuzzleGameToolkit) + "/Developer/Open Dev Tools Window &d", priority = 0)]
        public static void OpenDevToolsWindow()
        {
            DevTools.DevToolsWindow.ShowWindow();
        }

        [MenuItem("Tools/" + nameof(BlockPuzzleGameToolkit) + "/Developer/Quick Actions/Add 100 Coins", priority = 10)]
        public static void QuickAdd100Coins()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[DevTools] 游戏未运行，无法添加金币");
                return;
            }

            var currencyManager = UnityEngine.Object.FindObjectOfType<CurrencySystem.CurrencyManager>();
            if (currencyManager != null)
            {
                // 添加100金币（内部值 = 100 * 10000）
                currencyManager.AddCoins(1000000);
                Debug.Log("[DevTools] 已添加100金币");
            }
            else
            {
                Debug.LogError("[DevTools] CurrencyManager未找到");
            }
        }

        [MenuItem("Tools/" + nameof(BlockPuzzleGameToolkit) + "/Developer/Quick Actions/Add 1000 Coins", priority = 11)]
        public static void QuickAdd1000Coins()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[DevTools] 游戏未运行，无法添加金币");
                return;
            }

            var currencyManager = UnityEngine.Object.FindObjectOfType<CurrencySystem.CurrencyManager>();
            if (currencyManager != null)
            {
                // 添加1000金币（内部值 = 1000 * 10000）
                currencyManager.AddCoins(10000000);
                Debug.Log("[DevTools] 已添加1000金币");
            }
            else
            {
                Debug.LogError("[DevTools] CurrencyManager未找到");
            }
        }

        [MenuItem("Tools/" + nameof(BlockPuzzleGameToolkit) + "/Developer/Quick Actions/Unlock Next Level", priority = 20)]
        public static void QuickUnlockNextLevel()
        {
            int currentLevel = GameDataManager.GetLevelNum();
            int nextLevel = currentLevel + 1;
            PlayerPrefs.SetInt("Level", nextLevel);
            PlayerPrefs.Save();
            Debug.Log($"[DevTools] 已解锁关卡 {nextLevel}");
        }

        [MenuItem("Tools/" + nameof(BlockPuzzleGameToolkit) + "/Developer/Quick Actions/Unlock 10 Levels", priority = 21)]
        public static void QuickUnlock10Levels()
        {
            int currentLevel = GameDataManager.GetLevelNum();
            int targetLevel = currentLevel + 10;
            PlayerPrefs.SetInt("Level", targetLevel);
            PlayerPrefs.Save();
            Debug.Log($"[DevTools] 已解锁到关卡 {targetLevel}");
        }

        [MenuItem("Tools/" + nameof(BlockPuzzleGameToolkit) + "/Developer/Quick Actions/All Props +10", priority = 30)]
        public static void QuickAddAllProps()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[DevTools] 游戏未运行，无法添加道具");
                return;
            }

            var propManager = UnityEngine.Object.FindObjectOfType<PropSystem.Core.PropManager>();
            if (propManager != null)
            {
                propManager.AddProp(PropSystem.Core.PropType.Rotate, 10);
                propManager.AddProp(PropSystem.Core.PropType.Refresh, 10);
                propManager.AddProp(PropSystem.Core.PropType.Bomb, 10);
                Debug.Log("[DevTools] 所有道具已+10");
            }
            else
            {
                Debug.LogError("[DevTools] PropManager未找到或道具系统已禁用");
            }
        }

        [MenuItem("Tools/" + nameof(BlockPuzzleGameToolkit) + "/Developer/Quick Actions/Max All Props (99)", priority = 31)]
        public static void QuickMaxAllProps()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[DevTools] 游戏未运行，无法设置道具");
                return;
            }

            var propManager = UnityEngine.Object.FindObjectOfType<PropSystem.Core.PropManager>();
            if (propManager != null)
            {
                propManager.SetPropCount(PropSystem.Core.PropType.Rotate, 99);
                propManager.SetPropCount(PropSystem.Core.PropType.Refresh, 99);
                propManager.SetPropCount(PropSystem.Core.PropType.Bomb, 99);
                Debug.Log("[DevTools] 所有道具已设为99");
            }
            else
            {
                Debug.LogError("[DevTools] PropManager未找到或道具系统已禁用");
            }
        }

        [MenuItem("Tools/" + nameof(BlockPuzzleGameToolkit) + "/Developer/Quick Actions/Switch to Test Reward Group", priority = 40)]
        public static void QuickSwitchToTestRewardGroup()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[DevTools] 游戏未运行，无法切换奖励分组");
                return;
            }

            var rewardCalculator = UnityEngine.Object.FindObjectOfType<RewardSystem.RewardCalculator>();
            if (rewardCalculator != null)
            {
                bool success = rewardCalculator.SwitchToRange("tier_1");
                if (success)
                {
                    Debug.Log("[DevTools] 已切换到测试奖励分组: tier_1");
                }
                else
                {
                    Debug.LogError("[DevTools] 切换奖励分组失败");
                }
            }
            else
            {
                Debug.LogError("[DevTools] RewardCalculator未找到");
            }
        }

        [MenuItem("Tools/" + nameof(BlockPuzzleGameToolkit) + "/Developer/Test Mode/Enable All Cheats", priority = 50)]
        public static void EnableAllCheats()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[DevTools] 游戏未运行");
                return;
            }

            // 金币最大
            var currencyManager = UnityEngine.Object.FindObjectOfType<CurrencySystem.CurrencyManager>();
            if (currencyManager != null)
            {
                currencyManager.SetCoins(999990000); // 99999 USD * 10000
            }

            // 道具最大
            var propManager = UnityEngine.Object.FindObjectOfType<PropSystem.Core.PropManager>();
            if (propManager != null)
            {
                propManager.SetPropCount(PropSystem.Core.PropType.Rotate, 99);
                propManager.SetPropCount(PropSystem.Core.PropType.Refresh, 99);
                propManager.SetPropCount(PropSystem.Core.PropType.Bomb, 99);
            }

            // 解锁100关
            PlayerPrefs.SetInt("Level", 100);
            PlayerPrefs.Save();

            Debug.Log("[DevTools] 所有作弊已启用：金币99999，道具全满，解锁100关");
        }

        #endregion
    }
}