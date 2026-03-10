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
using System.Globalization;
using System.Linq;
using System.Resources;
using BlockPuzzle.AdSystem.Managers;
using BlockPuzzle.NativeBridge;
using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.Gameplay;
using BlockPuzzleGameToolkit.Scripts.Gameplay.Managers;
using BlockPuzzleGameToolkit.Scripts.GUI;
using BlockPuzzleGameToolkit.Scripts.Loading;
using BlockPuzzleGameToolkit.Scripts.Popups;
using BlockPuzzleGameToolkit.Scripts.Popups.Daily;
using BlockPuzzleGameToolkit.Scripts.Services;
// using BlockPuzzleGameToolkit.Scripts.Services.IAP; // IAP功能已移除
using BlockPuzzleGameToolkit.Scripts.Settings;
using DG.Tweening;
using UnityEngine;
using ResourceManager = BlockPuzzleGameToolkit.Scripts.Data.ResourceManager;

namespace BlockPuzzleGameToolkit.Scripts.GameCore
{
    // 确保GameManager最先执行，避免其子节点过早访问Instance导致创建重复实例
    [DefaultExecutionOrder(-1000)]
    public class GameManager : SingletonBehaviour<GameManager>
    {
        public Action<string> purchaseSucceded;
        public DebugSettings debugSettings;
        public DailyBonusSettings dailyBonusSettings;
        public GameSettings GameSettings;
        public SpinSettings luckySpinSettings;
        // public CoinsShopSettings coinsShopSettings; // CoinsShop功能已移除
        // IAP相关字段已移除
        private int lastBackgroundIndex = -1;
        private bool isTutorialMode;
        private MainMenu mainMenu;
        public Action<bool, List<string>> OnPurchasesRestored;
        // noAdsProduct字段已移除
        private bool blockButtons;

        [Header("启动Loading设置")]
        [SerializeField] private bool showStartupLoading = true;  // 是否显示启动Loading
        [SerializeField] private float startupLoadingDuration = 3f;  // Loading持续时间
        [SerializeField] private float startupLoadingDelay = 0.5f;  // Loading延迟时间

        public int Score { get=> ResourceManager.Instance.GetResource("Score").GetValue(); set => ResourceManager.Instance.GetResource("Score").Set(value); }

        public override void Awake()
        {
            // 由于设置了最高执行优先级(-1000)，GameManager应该总是第一个执行
            // 如果仍然发现重复，说明配置有问题
            if (Instance != null && Instance != this)
            {
                Debug.LogError($"[GameManager] 意外的重复实例！请检查场景配置。");
                Debug.LogError($"[GameManager] 保留: {Instance.name}(ID:{Instance.GetInstanceID()}), 销毁: {name}(ID:{GetInstanceID()})");

                // 兜底方案：迁移子节点避免数据丢失
                MigrateChildrenToExistingInstance();
                Destroy(gameObject);
                return;
            }

            base.Awake();
            DontDestroyOnLoad(gameObject);
            Application.targetFrameRate = 60;
            DOTween.SetTweensCapacity(1250, 512);

            // 清理旧的加密存档数据（从加密存储切换到非加密存储）
            #if !UNITY_EDITOR
            if (Utils.SaveDataCleaner.HasOldSaveData())
            {
                Debug.Log("[GameManager] 检测到旧的加密存档，正在清理...");
                Utils.SaveDataCleaner.CleanOldSaveData();
            }
            #endif

            

            mainMenu = FindObjectOfType<MainMenu>();
            if (mainMenu != null)
            {
                mainMenu.OnAnimationEnded += OnMainMenuAnimationEnded;
            }
        }

        private void OnEnable()
        {
            // IAP功能已移除
            if (StateManager.Instance.CurrentState == EScreenStates.MainMenu)
            {
                if (!GameDataManager.isTestPlay && CheckDailyBonusConditions())
                {
                    blockButtons = true;
                }
            }

            // 引导判断已移至LoadingManager中处理
        }

        private void OnDisable()
        {
            // IAP功能已移除
            if (mainMenu != null)
            {
                mainMenu.OnAnimationEnded -= OnMainMenuAnimationEnded;
            }
            GameDataManager.isTestPlay = false; // Reset isTestPlay
        }

        private bool IsTutorialShown()
        {
            return PlayerPrefs.GetInt("tutorial", 0) == 1;
        }

        public void SetTutorialCompleted()
        {
            PlayerPrefs.SetInt("tutorial", 1);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 迁移当前 GameManager 的子节点到已存在的持久化 GameManager
        /// </summary>
        private void MigrateChildrenToExistingInstance()
        {
            if (Instance == null) return;

            Transform existingParent = Instance.transform;
            Transform[] children = new Transform[transform.childCount];

            // 先收集所有子节点
            for (int i = 0; i < transform.childCount; i++)
            {
                children[i] = transform.GetChild(i);
            }

            // 迁移每个子节点
            foreach (Transform child in children)
            {
                if (child == null) continue;

                string childName = child.name;

                // 检查目标是否已有同名子节点
                Transform existingChild = existingParent.Find(childName);
                if (existingChild != null)
                {
                    continue;
                }

                child.SetParent(existingParent, false);
            }
        }

        private async void Start()
        {
            // 统一初始化所有Manager，确保正确的初始化顺序
            InitializeAllManagers();

            // 埋点上报：game_home
            if (NativeBridgeManager.Instance != null)
            {
                NativeBridgeManager.Instance.SendMessageToPlatform(
                    BlockPuzzle.NativeBridge.Enums.BridgeMessageType.BuryPoint,
                    "game_home",
                    "1"
                );
                Debug.Log("[GameManager] 埋点上报：game_home");
            }

            // 埋点上报：EnterGame
            if (NativeBridgeManager.Instance != null)
            {
                NativeBridgeManager.Instance.SendMessageToPlatform(
                    BlockPuzzle.NativeBridge.Enums.BridgeMessageType.BuryPoint,
                    "EnterGame",
                    "1"
                );
                NativeBridgeManager.Instance.SendMessageToPlatform(
                    BlockPuzzle.NativeBridge.Enums.BridgeMessageType.EnterGame
                );
                Debug.Log("[GameManager] 埋点上报：EnterGame");
            }
            // 显示启动Loading
            ShowStartupLoading();

            // IAP和广告功能已移除

            if (GameDataManager.isTestPlay)
            {
                GameDataManager.SetLevel(GameDataManager.GetLevel());
            }
        }

        /// <summary>
        /// 统一初始化所有Manager，按照依赖顺序
        /// </summary>
        private void InitializeAllManagers()
        {
            Debug.Log("[GameManager] 开始初始化所有Manager...");

            try
            {
                
                // 1. StorageManager - 最基础的存储服务，其他Manager都依赖它
                if (StorageSystem.Core.StorageManager.Instance != null)
                {
                    Debug.Log("[GameManager] 初始化 StorageManager");
                    // 通过访问Instance触发初始化
                    var storageManager = StorageSystem.Core.StorageManager.Instance;
                    if (!storageManager.IsInitialized)
                    {
                        storageManager.OnInit();
                    }
                }
                // 2. NativeBridgeManager - 原生桥接服务，依赖StorageManager
                if (NativeBridgeManager.Instance != null && !NativeBridgeManager.Instance.IsInitialized)
                {
                    Debug.Log("[GameManager] 初始化 NativeBridgeManager");
                    NativeBridgeManager.Instance.OnInit();
                }

                // 3. LocalizationManager - 本地化系统，应该很早初始化
                if (Localization.LocalizationManager.Instance != null && !Localization.LocalizationManager.Instance.IsInitialized)
                {
                    Debug.Log("[GameManager] 初始化 LocalizationManager");
                    Localization.LocalizationManager.Instance.OnInit();
                }

                // 4. CurrencyManager - 货币系统，很多系统依赖它
                if (CurrencySystem.CurrencyManager.Instance != null && !CurrencySystem.CurrencyManager.Instance.IsInitialized)
                {
                    Debug.Log("[GameManager] 初始化 CurrencyManager");
                    var currencyManager = CurrencySystem.CurrencyManager.Instance;
                    // 触发初始化（通过访问Instance）
                    if (!currencyManager.IsInitialized)
                    {
                        currencyManager.OnInit();
                    }
                }

                // 5. RewardCalculator - 奖励计算，依赖CurrencyManager
                if (RewardSystem.RewardCalculator.Instance != null && !RewardSystem.RewardCalculator.Instance.IsInitialized)
                {
                    Debug.Log("[GameManager] 初始化 RewardCalculator");
                    RewardSystem.RewardCalculator.Instance.OnInit();
                }

                // 6. MultiplierManager - 倍率系统，依赖CurrencyManager
                if (Multiplier.Core.MultiplierManager.Instance != null && !Multiplier.Core.MultiplierManager.Instance.IsInitialized)
                {
                    Debug.Log("[GameManager] 初始化 MultiplierManager");
                    Multiplier.Core.MultiplierManager.Instance.OnInit();
                }

                // 7. PropManager - 道具系统
                if (PropSystem.Core.PropManager.Instance != null && !PropSystem.Core.PropManager.Instance.IsInitialized)
                {
                    Debug.Log("[GameManager] 初始化 PropManager");
                    PropSystem.Core.PropManager.Instance.OnInit();
                }

                // 8. LevelRewardMultiplierManager - 等级奖励倍数
                if (LevelRewardMultiplier.Managers.LevelRewardMultiplierManager.Instance != null && !LevelRewardMultiplier.Managers.LevelRewardMultiplierManager.Instance.IsInitialized)
                {
                    Debug.Log("[GameManager] 初始化 LevelRewardMultiplierManager");
                    LevelRewardMultiplier.Managers.LevelRewardMultiplierManager.Instance.OnInit();
                }

                // // 9. QuestManager - 任务系统
                // if (QuestSystem.Core.QuestManager.Instance != null && !QuestSystem.Core.QuestManager.Instance.IsInitialized)
                // {
                //     Debug.Log("[GameManager] 初始化 QuestManager");
                //     QuestSystem.Core.QuestManager.Instance.OnInit();
                // }

                // 10. AdSystemManager - 广告系统
                if (AdSystemManager.Instance != null && !AdSystemManager.Instance.IsInitialized)
                {
                    Debug.Log("[GameManager] 初始化 AdSystemManager");
                    AdSystemManager.Instance.OnInit();
                }

                // 11. FlyRewardManager - 飞行奖励系统
                if (FlyRewardSystem.Core.FlyRewardManager.Instance != null && !FlyRewardSystem.Core.FlyRewardManager.Instance.IsInitialized)
                {
                    Debug.Log("[GameManager] 初始化 FlyRewardManager");
                    FlyRewardSystem.Core.FlyRewardManager.Instance.OnInit();
                }

                // 12. DynamicDifficultyController - 动态难度系统（独立，不依赖其他Manager）
                if (global::GameCore.DifficultySystem.DynamicDifficultyController.Instance != null && !global::GameCore.DifficultySystem.DynamicDifficultyController.Instance.IsInitialized)
                {
                    Debug.Log("[GameManager] 初始化 DynamicDifficultyController");
                    global::GameCore.DifficultySystem.DynamicDifficultyController.Instance.OnInit();
                }

                // 13. MoneyBlockManager - 金钱方块系统初始化
                if (MoneyBlockSystem.MoneyBlockManager.Instance != null && !MoneyBlockSystem.MoneyBlockManager.Instance.IsInitialized)
                {
                    Debug.Log("[GameManager] 初始化 MoneyBlockManager");
                    MoneyBlockSystem.MoneyBlockManager.Instance.OnInit();
                }

                // 14. ActivityManager - 活动系统（依赖MenuManager、货币系统、广告系统）
                if (BlockPuzzleGameToolkit.Scripts.Activity.Core.ActivityManager.Instance != null && !BlockPuzzleGameToolkit.Scripts.Activity.Core.ActivityManager.Instance.IsInitialized)
                {
                    Debug.Log("[GameManager] 初始化 ActivityManager");
                    BlockPuzzleGameToolkit.Scripts.Activity.Core.ActivityManager.Instance.OnInit();
                }

                Debug.Log("[GameManager] 所有Manager初始化完成");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[GameManager] 初始化Manager时发生错误: {e.Message}\n{e.StackTrace}");
            }
        }

        /// <summary>
        /// 显示启动Loading并跳转到Map场景
        /// </summary>
        private void ShowStartupLoading()
        {
            // 检查是否需要显示Loading
            if (!showStartupLoading)
            {
                Debug.Log("[GameManager] Startup loading disabled");
                return;
            }

            // 如果StateManager存在，设置初始状态为Loading
            if (StateManager.Instance != null)
            {
                // 设置初始状态为Loading
                if (StateManager.Instance.CurrentState != EScreenStates.Loading)
                {
                    StateManager.Instance.CurrentState = EScreenStates.Loading;
                }

                // 检查LoadingManager是否存在
                if (LoadingManager.Instance != null)
                {
                    Debug.Log($"[GameManager] Showing startup loading for {startupLoadingDuration} seconds");

                    // 延迟显示Loading（给Unity初始化留时间）
                    if (startupLoadingDelay > 0)
                    {
                        Invoke(nameof(ShowLoadingAndGoToMap), startupLoadingDelay);
                    }
                    else
                    {
                        ShowLoadingAndGoToMap();
                    }
                }
                else
                {
                    Debug.LogWarning("[GameManager] LoadingManager not found, skipping startup loading");
                    // 如果没有LoadingManager，直接跳转到Map场景
                    GoToMapDirectly();
                }
            }
            else
            {
                Debug.LogWarning("[GameManager] StateManager not found, cannot show loading");
            }
        }

        /// <summary>
        /// 显示Loading并跳转到游戏场景
        /// </summary>
        private void ShowLoadingAndGoToMap()
        {
            if (LoadingManager.Instance != null)
            {
                // 使用LoadingManager显示Loading并跳转到游戏场景（而不是Map）
                LoadingManager.Instance.ShowLoadingAndGoToGame(startupLoadingDuration);
            }
        }

        /// <summary>
        /// 直接跳转到游戏场景（跳过Loading）
        /// </summary>
        private void GoToMapDirectly()
        {
            // 直接跳转到游戏场景
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.StartGameScene();
            }
            else if (StateManager.Instance != null)
            {
                StateManager.Instance.CurrentState = EScreenStates.Game;
            }
        }

        private void OnInitializeSuccess()
        {
            Debug.Log("Gaming services initialized successfully");
        }

        private void OnInitializeError(string errorMessage)
        {
            Debug.LogError($"Failed to initialize gaming services: {errorMessage}");
        }

        private void HandleDailyBonus()
        {
            if (StateManager.Instance.CurrentState != EScreenStates.MainMenu || !dailyBonusSettings.dailyBonusEnabled || !GameSettings.enableInApps)
            {
                return;
            }

            var shouldShowDailyBonus = CheckDailyBonusConditions();

            if (shouldShowDailyBonus)
            {
                var daily = MenuManager.Instance.ShowPopup<DailyBonus>(()=>
                {
                    blockButtons = false;
                });
            }
        }

        private bool CheckDailyBonusConditions()
        {
            var today = DateTime.Today;
            var lastRewardDate = DateTime.Parse(PlayerPrefs.GetString("DailyBonusDay", today.Subtract(TimeSpan.FromDays(1)).ToString(CultureInfo.CurrentCulture)));
            return today.Date > lastRewardDate.Date && dailyBonusSettings.dailyBonusEnabled;
        }

        public void RestartLevel()
        {
            DOTween.KillAll();
            MenuManager.Instance.CloseAllPopups();
            EventManager.GetEvent(EGameEvent.RestartLevel).Invoke();
        }

        public void RemoveAds()
        {
            // if (GameSettings.enableAds) {
            //     MenuManager.Instance.ShowPopup<NoAds>();
            // }
        }

        public void MainMenu()
        {
            DOTween.KillAll();
            if (StateManager.Instance.CurrentState == EScreenStates.Game && GameDataManager.GetGameMode() == EGameMode.Classic)
            {
                SceneLoader.Instance.GoMain();
            }
            else if (StateManager.Instance.CurrentState == EScreenStates.Game && GameDataManager.GetGameMode() == EGameMode.Adventure)
            {
                SceneLoader.Instance.StartMapScene();
            }
            else if (StateManager.Instance.CurrentState == EScreenStates.Map)
            {
                SceneLoader.Instance.GoMain();
            }
            else if (StateManager.Instance.CurrentState == EScreenStates.MainMenu)
            {
                MenuManager.Instance.ShowPopup<Quit>();
            }
            else
            {
                SceneLoader.Instance.GoMain();
            }
        }

        public void OpenMap()
        {
            if (blockButtons && StateManager.Instance.CurrentState == EScreenStates.MainMenu)
                return;
            if (GetGameMode() == EGameMode.Classic)
            {
                SceneLoader.Instance.StartGameSceneClassic();
            }
            else if (GetGameMode() == EGameMode.Timed)
            {
                SceneLoader.Instance.StartGameSceneTimed();
            }
            else
            {
                SceneLoader.Instance.StartMapScene();
            }
        }

        public void OpenGame()
        {
            SceneLoader.Instance.StartGameScene();
        }

        public void PurchaseSucceeded(string id)
        {
            purchaseSucceded?.Invoke(id);
        }

        public bool IsNoAdsPurchased()
        {
            // 广告功能已移除，始终返回true
            return true;
        }

        public void SetGameMode(EGameMode gameMode)
        {
            GameDataManager.SetGameMode(gameMode);
        }

        private EGameMode GetGameMode()
        {
            return GameDataManager.GetGameMode();
        }

        public int GetLastBackgroundIndex()
        {
            return lastBackgroundIndex;
        }

        public void SetLastBackgroundIndex(int index)
        {
            lastBackgroundIndex = index;
        }

        public void NextLevel()
        {
            GameDataManager.LevelNum++;

            // 立即更新TopPanel的关卡显示
            if (TopPanel.TopPanel.Instance != null)
            {
                TopPanel.TopPanel.Instance.UpdateLevelDisplay();
                Debug.Log($"[GameManager] NextLevel - 更新TopPanel关卡显示: {GameDataManager.LevelNum}");
            }

            OpenGame();
            RestartLevel();
        }

        public void SetTutorialMode(bool tutorial)
        {
            Debug.Log("Tutorial mode set to " + tutorial);
            isTutorialMode = tutorial;
        }

        public bool IsTutorialMode()
        {
            return isTutorialMode;
        }

        private void OnMainMenuAnimationEnded()
        {
            if (StateManager.Instance.CurrentState == EScreenStates.MainMenu)
            {

                HandleDailyBonus();
            }
        }

        internal void RestorePurchases(Action<bool, List<string>> OnPurchasesRestored)
        {
            // IAP功能已移除
            OnPurchasesRestored?.Invoke(false, new List<string>());
        }

        public bool IsPurchased(string id)
        {
            // IAP功能已移除
            return false;
        }
    }
}