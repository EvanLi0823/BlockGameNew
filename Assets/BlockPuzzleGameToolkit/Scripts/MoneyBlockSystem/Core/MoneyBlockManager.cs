// 金钱方块系统 - 核心管理器
// 创建日期: 2026-03-05

using System;
using System.Collections.Generic;
using UnityEngine;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using BlockPuzzleGameToolkit.Scripts.CurrencySystem;
using BlockPuzzleGameToolkit.Scripts.Gameplay;
using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.TopPanel;
using BlockPuzzleGameToolkit.Scripts.LevelsData;
using BlockPuzzleGameToolkit.Scripts.Popups;
using StorageSystem.Core;

namespace BlockPuzzleGameToolkit.Scripts.MoneyBlockSystem
{
    /// <summary>
    /// 金钱方块系统管理器
    /// 实现IBonusCollector接口，集成到Bonus飞行动画系统
    /// 职责:
    /// - 系统初始化和配置加载
    /// - 刷新逻辑协调(监听形状放置事件)
    /// - 提供飞行动画接口（终点位置、图标模板、收集回调）
    /// - 累计追踪和触发判定
    /// - 准备奖励弹窗参数并触发弹窗
    /// - 处理奖励领取回调（发放奖励、更新统计）
    /// - 数据持久化管理
    /// 动画职责分离:
    /// - 即时消除动画: BonusAnimationManager（棋盘→货币栏）
    /// - 累计奖励飞币: CommonRewardPopup内部自动处理（弹窗→货币栏）
    /// - MoneyBlockManager只负责准备参数，不关注动画细节
    /// </summary>
    public class MoneyBlockManager : SingletonBehaviour<MoneyBlockManager>, IBonusCollector
    {
        private const string SAVE_KEY = "money_block_data";
        private const string SETTINGS_PATH = "Settings/MoneyBlockSettings";

        #region Fields

        // 配置（动态加载）
        private MoneyBlockSettings settings;

        // 核心组件
        private MoneyBlockSaveData saveData;
        private MoneyBlockSpawner spawner;
        private MoneyBlockRewardCalculator rewardCalculator;
        private MoneyBlockCumulativeTracker cumulativeTracker;

        // 状态标记
        private bool isProcessingCumulative = false;
        private bool hasPendingSpawn = false;  // 有待生成的金钱方块（上次失败，下次继续尝试）

        #endregion

        #region Public Properties

        /// <summary>
        /// 是否正在处理累计奖励弹窗
        /// 用于延迟通关判定，避免弹窗冲突
        /// </summary>
        public bool IsProcessingCumulative => isProcessingCumulative;

        #endregion

        #region Events

        /// <summary>
        /// 金钱方块刷新事件
        /// </summary>
        public event Action<int> OnMoneyBlockSpawned;

        /// <summary>
        /// 金钱方块消除事件(数量,奖励)
        /// </summary>
        public event Action<int, int> OnMoneyBlockEliminated;

        /// <summary>
        /// 累计奖励触发事件
        /// </summary>
        public event Action<int> OnCumulativeTriggered;

        #endregion

        #region Initialization

        /// <summary>
        /// 初始化优先级 (在CurrencyManager, RewardCalculator之后)
        /// </summary>
        public override int InitPriority => 50;

        /// <summary>
        /// 单例初始化入口
        /// </summary>
        public override void OnInit()
        {
            if (IsInitialized)
            {
                Debug.LogWarning("[MoneyBlockManager] 已经初始化，跳过");
                return;
            }

            // 动态加载配置
            settings = Resources.Load<MoneyBlockSettings>(SETTINGS_PATH);
            if (settings == null)
            {
                Debug.LogError($"[MoneyBlockManager] 配置文件未找到: {SETTINGS_PATH}");
                return;
            }

            // 验证配置
            if (!settings.ValidateSettings())
            {
                Debug.LogError("[MoneyBlockManager] 配置验证失败");
                return;
            }

            // 加载存档数据
            LoadData();

            // 初始化核心组件
            spawner = new MoneyBlockSpawner(settings);
            rewardCalculator = new MoneyBlockRewardCalculator(settings);
            cumulativeTracker = new MoneyBlockCumulativeTracker(settings);

            // 订阅事件
            SubscribeEvents();

            base.OnInit(); // 设置IsInitialized = true

            // 自动注册到TargetManager（如果存在）
            RegisterToTargetManager();

            Debug.Log("[MoneyBlockManager] 初始化完成");
        }

        /// <summary>
        /// 自动注册到TargetManager
        /// </summary>
        private void RegisterToTargetManager()
        {
            var targetManager = FindObjectOfType<Gameplay.TargetManager>();
            if (targetManager != null)
            {
                Debug.Log("[MoneyBlockManager] 找到TargetManager，尝试注册...");
                targetManager.RegisterCollector(this);
                Debug.Log("[MoneyBlockManager] 注册完成");
            }
            else
            {
                Debug.Log("[MoneyBlockManager] 未找到TargetManager（可能非冒险模式）");
            }
        }

        #endregion

        #region Event Subscription

        /// <summary>
        /// 订阅事件
        /// </summary>
        private void SubscribeEvents()
        {
            // 订阅关卡加载事件（app启动或切换关卡时）
            EventManager.GetEvent<LevelsData.Level>(EGameEvent.LevelLoaded).Subscribe(OnLevelLoaded);

            // 订阅形状放置事件（带Shape参数，与ShapeDraggable触发的事件匹配）
            EventManager.GetEvent<Gameplay.Shape>(EGameEvent.ShapePlaced).Subscribe(OnShapePlaced);

            // ❌ 移除：不再监听ItemDestroyed事件（改为被TargetManager主动调用）
            // EventManager.GetEvent<GameObject>(EGameEvent.ItemDestroyed).Subscribe(OnBlockDestroyed);

            // 订阅关卡完成事件
            EventManager.GetEvent(EGameEvent.LevelCompleted).Subscribe(OnLevelCompleted);

            // 订阅关卡失败事件
            EventManager.GetEvent(EGameEvent.LevelFailed).Subscribe(OnLevelFailed);

            // 订阅关卡重启事件
            EventManager.GetEvent(EGameEvent.RestartLevel).Subscribe(OnLevelRestarted);

            if (settings.enableDebugLog)
            {
                Debug.Log("[MoneyBlockManager] 事件订阅完成");
            }
        }

        /// <summary>
        /// 取消订阅事件
        /// </summary>
        private void UnsubscribeEvents()
        {
            // 取消订阅关卡加载事件
            EventManager.GetEvent<LevelsData.Level>(EGameEvent.LevelLoaded)?.Unsubscribe(OnLevelLoaded);

            // 取消订阅形状放置事件
            EventManager.GetEvent<Gameplay.Shape>(EGameEvent.ShapePlaced)?.Unsubscribe(OnShapePlaced);

            // ❌ 移除：不再监听ItemDestroyed事件
            // EventManager.GetEvent<GameObject>(EGameEvent.ItemDestroyed)?.Unsubscribe(OnBlockDestroyed);

            // 取消订阅关卡完成事件
            EventManager.GetEvent(EGameEvent.LevelCompleted)?.Unsubscribe(OnLevelCompleted);

            // 取消订阅关卡失败事件
            EventManager.GetEvent(EGameEvent.LevelFailed)?.Unsubscribe(OnLevelFailed);

            // 取消订阅关卡重启事件
            EventManager.GetEvent(EGameEvent.RestartLevel)?.Unsubscribe(OnLevelRestarted);

            if (settings != null && settings.enableDebugLog)
            {
                Debug.Log("[MoneyBlockManager] 事件取消订阅完成");
            }
        }

        private void OnDisable()
        {
            // OnDisable更可靠，场景切换时必定调用
            UnsubscribeEvents();

            // 从TargetManager取消注册
            UnregisterFromTargetManager();
        }

        private void OnDestroy()
        {
            // 保险起见，再次尝试取消订阅
            UnsubscribeEvents();

            // 再次尝试取消注册
            UnregisterFromTargetManager();

            // 保存数据
            SaveData();
        }

        /// <summary>
        /// 从TargetManager取消注册
        /// </summary>
        private void UnregisterFromTargetManager()
        {
            var targetManager = FindObjectOfType<Gameplay.TargetManager>();
            if (targetManager != null)
            {
                targetManager.UnregisterCollector(this);
                if (settings != null && settings.enableDebugLog)
                {
                    Debug.Log("[MoneyBlockManager] 已从TargetManager取消注册");
                }
            }
        }

        #endregion

        #region Data Management

        /// <summary>
        /// 加载数据
        /// </summary>
        private void LoadData()
        {
            var storageManager = StorageManager.Instance;
            if (storageManager == null)
            {
                Debug.LogError("[MoneyBlockManager] StorageManager未找到");
                saveData = MoneyBlockSaveData.CreateDefault();
                return;
            }

            saveData = storageManager.Load<MoneyBlockSaveData>(SAVE_KEY, StorageType.Binary);
            if (saveData == null || !saveData.IsValid())
            {
                Debug.LogWarning("[MoneyBlockManager] 数据无效，创建默认数据");
                saveData = MoneyBlockSaveData.CreateDefault();
            }
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        private void SaveData()
        {
            if (saveData == null)
            {
                Debug.LogError("[MoneyBlockManager] saveData为null");
                return;
            }

            var storageManager = StorageManager.Instance;
            if (storageManager == null)
            {
                Debug.LogError("[MoneyBlockManager] StorageManager未找到");
                return;
            }

            saveData.UpdateMetadata(1);
            bool success = storageManager.Save(SAVE_KEY, saveData, StorageType.Binary);

            if (!success)
            {
                Debug.LogError("[MoneyBlockManager] 数据保存失败");
            }
        }

        /// <summary>
        /// 重置关卡数据
        /// </summary>
        public void ResetLevelData()
        {
            if (saveData == null)
                return;

            if (settings.enableDebugLog)
            {
                Debug.Log($"[MoneyBlockManager] 重置前: spawnCountInLevel={saveData.spawnCountInLevel}, shapePlacementCount={saveData.shapePlacementCount}");
            }

            saveData.ResetLevelData();

            // 重置pending标志
            hasPendingSpawn = false;

            SaveData();

            if (settings.enableDebugLog)
            {
                Debug.Log($"[MoneyBlockManager] 重置后: spawnCountInLevel={saveData.spawnCountInLevel}, shapePlacementCount={saveData.shapePlacementCount}, pending={hasPendingSpawn}");
            }
        }

        #endregion

        #region Shape Placement Handler

        /// <summary>
        /// 形状放置回调
        /// </summary>
        public void OnShapePlaced(Gameplay.Shape shape)
        {
            if (!IsInitialized || saveData == null)
                return;

            saveData.shapePlacementCount++;
            SaveData();

            if (settings.enableDebugLog)
            {
                Debug.Log($"[MoneyBlockManager] 形状放置计数: {saveData.shapePlacementCount}");
            }
        }

        /// <summary>
        /// 获取刷新器（供CellDeckManager调用）
        /// 仅检查刷新条件，不更新计数（计数在成功生成后由NotifyMoneyBlockSpawned()更新）
        /// 支持pending模式：如果上次生成失败，下次继续尝试
        /// </summary>
        /// <returns>满足条件返回spawner，否则返回null</returns>
        public MoneyBlockSpawner GetSpawner()
        {
            if (!IsInitialized || saveData == null || settings == null)
                return null;

            if (settings.enableDebugLog)
            {
                Debug.Log($"[MoneyBlockManager] GetSpawner调用 - pending={hasPendingSpawn}, spawnCountInLevel={saveData.spawnCountInLevel}, shapePlacementCount={saveData.shapePlacementCount}");
            }

            // 优先检查pending标志：如果有待生成的金钱方块，直接返回spawner
            if (hasPendingSpawn)
            {
                if (settings.enableDebugLog)
                {
                    Debug.Log("[MoneyBlockManager] 检测到pending生成请求，继续尝试生成金钱方块");
                }
                return spawner;
            }

            // 常规检查：是否满足刷新条件
            if (!spawner.CanSpawn(saveData, settings))
                return null;

            return spawner;
        }

        /// <summary>
        /// 金钱方块成功生成回调（由CellDeckManager调用）
        /// 更新刷新计数和触发事件，清除pending标志，重置放置计数开始下一轮
        /// </summary>
        public void NotifyMoneyBlockSpawned()
        {
            if (!IsInitialized || saveData == null)
                return;

            // 清除pending标志（如果之前有待生成，现在成功了）
            hasPendingSpawn = false;

            // 更新刷新计数
            saveData.spawnCountInLevel++;
            saveData.totalSpawnCount++;

            // 重置放置计数，开始下一轮计数
            saveData.shapePlacementCount = 0;

            SaveData();

            if (settings.enableDebugLog)
            {
                Debug.Log($"[MoneyBlockManager] 金钱方块生成成功: 关卡内={saveData.spawnCountInLevel}/{settings.maxMoneyBlocksPerLevel}, " +
                          $"总计={saveData.totalSpawnCount}，已重置放置计数");
            }

            // 触发事件
            OnMoneyBlockSpawned?.Invoke(1);
        }

        /// <summary>
        /// 设置pending标志（由CellDeckManager调用）
        /// 当满足刷新条件但生成失败时调用，下次继续尝试生成
        /// </summary>
        public void SetPendingSpawn()
        {
            if (!IsInitialized)
                return;

            hasPendingSpawn = true;

            if (settings.enableDebugLog)
            {
                Debug.Log("[MoneyBlockManager] 本次生成失败（无可用格子），设置pending标志，下次继续尝试");
            }
        }

        #endregion

        #region Cumulative Reward

        /// <summary>
        /// 触发累计奖励
        /// </summary>
        public void TriggerCumulativeReward()
        {
            if (isProcessingCumulative)
            {
                Debug.LogWarning("[MoneyBlockManager] 已在处理累计奖励");
                return;
            }

            isProcessingCumulative = true;

            // 计算大额奖励
            int baseReward = rewardCalculator.CalculateLargeReward();

            // 配置弹窗参数
            var config = new RewardPopupConfig
            {
                BaseReward = baseReward,
                AdMultiplier = settings.GetAdMultiplier(),
                NoAdMultiplier = settings.GetNoAdMultiplier(),
                AutoPlayFlyAnimation = true,
                FlyingCoinCount = settings.flyingCoinCountLarge,
                FlyStartPosition = null, // null = 使用弹窗的世界坐标位置
                AdEntryName = "moneyBlock_cumulative",
                OnRewardClaimed = OnRewardClaimed,
                OnPopupClosed = OnPopupClosed  // 弹窗关闭后回调
            };

            // 使用MenuManager加载弹窗
            var menuManager = MenuManager.Instance;
            if (menuManager == null)
            {
                Debug.LogError("[MoneyBlockManager] MenuManager未找到");
                isProcessingCumulative = false;
                return;
            }

            var popup = menuManager.ShowPopup<CommonRewardPopup>();

            if (popup == null)
            {
                Debug.LogError("[MoneyBlockManager] 累计奖励弹窗加载失败");
                isProcessingCumulative = false;
                return;
            }

            // 初始化弹窗
            popup.Initialize(config);

            // 触发事件
            OnCumulativeTriggered?.Invoke(baseReward);

            if (settings.enableDebugLog)
            {
                Debug.Log($"[MoneyBlockManager] 触发累计奖励: 基础奖励={baseReward}");
            }
        }

        /// <summary>
        /// 弹窗关闭回调（所有动画完成后调用）
        /// </summary>
        private void OnPopupClosed()
        {
            if (settings.enableDebugLog)
            {
                Debug.Log("[MoneyBlockManager] 累计奖励弹窗已关闭");
            }

            // 恢复状态
            isProcessingCumulative = false;
            SaveData();

            // 弹窗关闭后，检查关卡是否完成并触发通关
            CheckAndTriggerWinIfNeeded();
        }

        /// <summary>
        /// 奖励领取回调
        /// </summary>
        /// <param name="result">领取结果</param>
        private void OnRewardClaimed(RewardClaimResult result)
        {
            // 广告失败处理
            if (!result.Success)
            {
                // 广告失败，重置计数
                saveData.cumulativeEliminateCount = 0;
                SaveData();

                // ❌ 不在此处恢复状态，等待OnPopupClosed回调
                // isProcessingCumulative = false;

                if (settings.enableDebugLog)
                {
                    Debug.Log("[MoneyBlockManager] 广告失败，重置累计计数");
                }
                return;
            }

            // 成功领取，发放奖励
            GiveReward(result.FinalReward);

            // 更新统计
            saveData.cumulativeTriggerCount++;
            if (result.ClaimType == EClaimType.AdMultiple)
            {
                saveData.adClaimCount++;
            }
            else
            {
                saveData.singleClaimCount++;
            }

            // 注意：飞币动画已由RewardPopup内部自动处理，无需在此手动调用

            // 重置累计计数
            saveData.cumulativeEliminateCount = 0;

            // ❌ 不在此处恢复状态，等待弹窗完全关闭后再恢复
            // isProcessingCumulative = false;

            SaveData();

            if (settings.enableDebugLog)
            {
                Debug.Log($"[MoneyBlockManager] 累计奖励已领取: 类型={result.ClaimType}, 金额={result.FinalReward}");
            }

            // ❌ 移除：不在此处检查通关，等待弹窗完全关闭后再检查
            // CheckAndTriggerWinIfNeeded();
        }

        /// <summary>
        /// 检查关卡是否完成，如果完成则触发通关
        /// 用于累计弹窗关闭后的延迟通关判定
        /// </summary>
        private void CheckAndTriggerWinIfNeeded()
        {
            // 只在冒险模式生效
            var levelManager = FindObjectOfType<LevelManager>();
            if (levelManager == null || levelManager.GetGameMode() == EGameMode.Classic)
                return;

            // 获取TargetManager
            var targetManager = FindObjectOfType<Gameplay.TargetManager>();
            if (targetManager == null)
                return;

            // ✅ 修复: 使用WillLevelBeComplete()而不是IsLevelComplete()
            // WillLevelBeComplete()会考虑正在飞行的动画，避免过早触发通关
            if (targetManager.WillLevelBeComplete())
            {
                if (settings.enableDebugLog)
                {
                    Debug.Log("[MoneyBlockManager] 累计弹窗关闭后检测到关卡即将完成，延迟触发通关");
                }

                // 延迟触发通关（给飞币动画留时间）
                StartCoroutine(DelayedTriggerWin());
            }
        }

        /// <summary>
        /// 延迟触发通关（协程）
        /// </summary>
        private System.Collections.IEnumerator DelayedTriggerWin()
        {
            // 等待飞币动画完成（大约0.5秒）
            yield return new WaitForSeconds(0.5f);

            // ✅ 修复: 再次检查关卡是否即将完成（使用WillLevelBeComplete）
            var targetManager = FindObjectOfType<Gameplay.TargetManager>();
            if (targetManager != null && targetManager.WillLevelBeComplete())
            {
                if (settings.enableDebugLog)
                {
                    Debug.Log("[MoneyBlockManager] 触发延迟通关，设置游戏状态为PreWin");
                }

                // 解锁下一关
                var levelManager = FindObjectOfType<LevelManager>();
                if (levelManager != null)
                {
                    int currentLevel = GameDataManager.GetLevelNum();
                    GameDataManager.UnlockLevel(currentLevel + 1);
                }

                // 触发关卡通关事件
                EventManager.GetEvent(EGameEvent.LevelCompleted).Invoke();

                // 设置游戏状态为预胜利（触发PreWin弹窗）
                EventManager.GameStatus = EGameState.PreWin;
            }
        }

        #endregion

        #region Reward Distribution

        /// <summary>
        /// 发放奖励
        /// </summary>
        private void GiveReward(int rewardAmount)
        {
            var currencyManager = CurrencyManager.Instance;
            if (currencyManager == null)
            {
                Debug.LogError("[MoneyBlockManager] CurrencyManager未找到");
                return;
            }

            if (currencyManager.AddCoins(rewardAmount))
            {
                saveData.totalRewardAmount += rewardAmount;

                // 触发货币UI更新事件（TopPanel滚动动画和加钱文本）
                EventManager.GetEvent(EGameEvent.CurrencyChanged).Invoke();

                if (settings.enableDebugLog)
                {
                    Debug.Log($"[MoneyBlockManager] 发放奖励成功: {rewardAmount}");
                }
            }
        }


        #endregion

        #region Level Events

        /// <summary>
        /// 关卡加载回调（app启动或切换关卡时）
        /// </summary>
        private void OnLevelLoaded(LevelsData.Level level)
        {
            ResetLevelData();

            // 关卡加载时尝试注册到TargetManager（如果之前未注册成功）
            RegisterToTargetManager();

            if (settings.enableDebugLog)
            {
                Debug.Log("[MoneyBlockManager] 关卡加载，已重置数据");
            }
        }

        /// <summary>
        /// 关卡完成回调
        /// </summary>
        private void OnLevelCompleted()
        {
            ResetLevelData();
        }

        /// <summary>
        /// 关卡失败回调
        /// </summary>
        private void OnLevelFailed()
        {
            ResetLevelData();
        }

        /// <summary>
        /// 关卡重启回调
        /// </summary>
        private void OnLevelRestarted()
        {
            ResetLevelData();

            if (settings.enableDebugLog)
            {
                Debug.Log("[MoneyBlockManager] 关卡重启，已重置数据");
            }
        }

        #endregion

        #region IBonusCollector接口实现

        /// <summary>
        /// 获取金钱UI的位置（飞行终点）
        /// </summary>
        public Vector2 GetFlyTargetPosition()
        {
            var topPanel = TopPanel.TopPanel.Instance;
            if (topPanel == null)
            {
                Debug.LogWarning("[MoneyBlockManager] TopPanel未找到");
                return Vector2.zero;
            }

            var currencyTransform = topPanel.GetCurrencyTextTransForm();
            return currencyTransform != null ? currencyTransform.position : Vector2.zero;
        }

        /// <summary>
        /// 获取金钱方块的bonus模板
        /// </summary>
        public BonusItemTemplate GetBonusTemplate()
        {
            return settings?.moneyBonusTemplate;
        }

        /// <summary>
        /// 当金钱方块被收集时的回调（飞行动画完成后调用）
        /// </summary>
        public void OnBonusCollected(Cell cell)
        {
            if (!IsInitialized || saveData == null || cell == null)
                return;

            // 获取金钱方块组件
            var moneyBlock = cell.item?.GetComponent<MoneyBlock>();
            if (moneyBlock == null || moneyBlock.State != EMoneyBlockState.Active)
                return;

            if (settings.enableDebugLog)
            {
                Debug.Log($"[MoneyBlockManager] OnBonusCollected被调用: cell={cell.name}");
            }

            // 使用CumulativeTracker处理单个消除逻辑
            var result = cumulativeTracker.ProcessBatchEliminate(
                1,  // 单个方块
                saveData.cumulativeEliminateCount,
                settings.cumulativeThreshold
            );

            // 更新统计数据
            saveData.eliminateCountInLevel += result.processedCount;
            saveData.totalEliminateCount += result.processedCount;

            if (result.triggeredCumulative)
            {
                // 达到阈值，触发累计奖励
                saveData.cumulativeEliminateCount = settings.cumulativeThreshold;

                if (settings.enableDebugLog)
                {
                    Debug.Log($"[MoneyBlockManager] 触发累计奖励");
                }

                TriggerCumulativeReward();
            }
            else
            {
                // 未达阈值，发放即时奖励
                saveData.cumulativeEliminateCount += result.rewardedCount;

                int singleReward = rewardCalculator.CalculateSmallReward();
                result.immediateReward = singleReward;

                // 发放奖励
                GiveReward(singleReward);

                // 触发货币UI更新事件（TopPanel滚动动画和加钱文本）
                EventManager.GetEvent(EGameEvent.CurrencyChanged).Invoke();

                if (settings.enableDebugLog)
                {
                    Debug.Log($"[MoneyBlockManager] 发放即时奖励: {singleReward}, 累计={saveData.cumulativeEliminateCount}/{settings.cumulativeThreshold}");
                }

                // 触发事件
                OnMoneyBlockEliminated?.Invoke(1, singleReward);

                // ❌ 移除：不在此处检查通关，由LevelManager.CheckLose()统一处理
                // 金钱方块系统只负责在累计弹窗期间延迟通关判定
            }

            SaveData();
        }

        /// <summary>
        /// 检查格子是否包含金钱方块
        /// </summary>
        public bool HasBonus(Cell cell)
        {
            if (cell == null || cell.item == null)
                return false;

            var moneyBlock = cell.item.GetComponent<MoneyBlock>();
            return moneyBlock != null && moneyBlock.State == EMoneyBlockState.Active;
        }

        /// <summary>
        /// 检查金钱方块系统是否启用（仅冒险模式）
        /// </summary>
        public bool IsEnabled()
        {
            if (!IsInitialized || settings == null)
                return false;

            // 仅在冒险模式生效
            var levelManager = FindObjectOfType<LevelManager>();
            if (levelManager == null)
                return false;

            return levelManager.GetGameMode() == EGameMode.Adventure;
        }

        #endregion

        #region Debug

#if UNITY_EDITOR
        [Header("调试工具")]
        [SerializeField] private bool showDebugInfo = true;

        private void OnGUI()
        {
            if (!showDebugInfo || saveData == null)
                return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label($"金钱方块系统调试信息");
            GUILayout.Label($"放置计数: {saveData.shapePlacementCount}");
            GUILayout.Label($"关卡刷新: {saveData.spawnCountInLevel}/{settings.maxMoneyBlocksPerLevel}");
            GUILayout.Label($"关卡消除: {saveData.eliminateCountInLevel}");
            GUILayout.Label($"累计计数: {saveData.cumulativeEliminateCount}/{settings.cumulativeThreshold}");
            GUILayout.Label($"累计触发次数: {saveData.cumulativeTriggerCount}");
            GUILayout.EndArea();
        }

        [ContextMenu("强制触发累计奖励")]
        private void DebugTriggerCumulative()
        {
            saveData.cumulativeEliminateCount = settings.cumulativeThreshold;
            TriggerCumulativeReward();
        }

        [ContextMenu("重置累计计数")]
        private void DebugResetCumulative()
        {
            saveData.cumulativeEliminateCount = 0;
            SaveData();
        }

        [ContextMenu("重置关卡数据")]
        private void DebugResetLevel()
        {
            ResetLevelData();
        }
#endif

        #endregion
    }
}
