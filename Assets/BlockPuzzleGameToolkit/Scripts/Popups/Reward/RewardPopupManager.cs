// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

using System;
using UnityEngine;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using BlockPuzzleGameToolkit.Scripts.CurrencySystem;
using BlockPuzzleGameToolkit.Scripts.RewardSystem;
using BlockPuzzleGameToolkit.Scripts.LevelsData;
using BlockPuzzleGameToolkit.Scripts.Gameplay;
using BlockPuzzleGameToolkit.Scripts.Enums;

namespace BlockPuzzleGameToolkit.Scripts.Popups.Reward
{
    using RewardPopupType = LevelRewardConfig.RewardPopupType;
    /// <summary>
    /// 奖励弹窗管理器（单例）
    /// 负责管理和显示奖励弹窗
    /// </summary>
    public class RewardPopupManager : SingletonBehaviour<RewardPopupManager>
    {
        #region Fields

        [Header("Popup Prefabs")]
        [SerializeField] private FixedRewardPopup fixedRewardPrefab;      // 固定倍率弹窗预制体
        [SerializeField] private SlidingRewardPopup slidingRewardPrefab;  // 滑动倍率弹窗预制体

        [Header("Settings")]
        [SerializeField] private bool autoShowOnWin = false;              // 是否在胜利时自动显示（现已由LevelStateHandler直接控制）
        [SerializeField] private float showDelay = 0.5f;                  // 显示延迟
        [SerializeField] private bool testMode = false;                   // 测试模式（不需要真实广告）

        private RewardPopupBase currentRewardPopup;                       // 当前显示的奖励弹窗
        private RewardPopupData pendingRewardData;                        // 待处理的奖励数据
        private bool isShowingPopup = false;                              // 防止重复显示的标记

        // 统计数据
        private int totalRewardsShown = 0;
        private int totalRewardsClaimed = 0;
        private int totalRewardsSkipped = 0;
        private float totalRewardAmount = 0f;

        #endregion

        #region Events

        /// <summary>
        /// 弹窗显示事件
        /// </summary>
        public static event Action<RewardPopupType> OnRewardPopupShown;

        /// <summary>
        /// 奖励领取完成事件
        /// </summary>
        public static event Action<float, int> OnRewardClaimComplete;

        /// <summary>
        /// 奖励跳过事件
        /// </summary>
        public static event Action<float> OnRewardSkipComplete;

        #endregion

        #region Unity Lifecycle

        public override void Awake()
        {
            base.Awake();
            InitializeManager();
        }

        private void OnEnable()
        {
            // 订阅事件
            if (autoShowOnWin)
            {
                EventManager.OnGameStateChanged += OnGameStateChanged;
            }

            RewardPopupBase.OnRewardClaimed += HandleRewardClaimed;
            RewardPopupBase.OnRewardSkipped += HandleRewardSkipped;
        }

        private void OnDisable()
        {
            // 取消订阅
            EventManager.OnGameStateChanged -= OnGameStateChanged;
            RewardPopupBase.OnRewardClaimed -= HandleRewardClaimed;
            RewardPopupBase.OnRewardSkipped -= HandleRewardSkipped;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// 初始化管理器
        /// </summary>
        private void InitializeManager()
        {
            // 验证预制体
            if (fixedRewardPrefab == null || slidingRewardPrefab == null)
            {
                Debug.LogWarning("[RewardPopupManager] 预制体未设置，尝试从Resources加载");
                LoadPrefabsFromResources();
            }

            // 加载保存的统计数据
            LoadStatistics();

            Debug.Log("[RewardPopupManager] 初始化完成");
        }

        /// <summary>
        /// 从Resources加载预制体
        /// </summary>
        private void LoadPrefabsFromResources()
        {
            if (fixedRewardPrefab == null)
            {
                fixedRewardPrefab = Resources.Load<FixedRewardPopup>("Popups/FixedRewardPopup");
                if (fixedRewardPrefab == null)
                {
                    Debug.LogError("[RewardPopupManager] 无法加载FixedRewardPopup预制体");
                }
            }

            if (slidingRewardPrefab == null)
            {
                slidingRewardPrefab = Resources.Load<SlidingRewardPopup>("Popups/SlidingRewardPopup");
                if (slidingRewardPrefab == null)
                {
                    Debug.LogError("[RewardPopupManager] 无法加载SlidingRewardPopup预制体");
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 显示奖励弹窗
        /// </summary>
        /// <param name="data">奖励数据</param>
        public void ShowRewardPopup(RewardPopupData data)
        {
            // 防止重复显示
            if (isShowingPopup)
            {
                Debug.Log("[RewardPopupManager] 已在显示流程中，忽略重复调用");
                return;
            }

            if (data == null || !data.IsValid())
            {
                Debug.LogError("[RewardPopupManager] 无效的奖励数据");
                return;
            }

            // 如果已有弹窗在显示，先关闭
            if (currentRewardPopup != null)
            {
                Debug.LogWarning("[RewardPopupManager] 已有奖励弹窗在显示，将关闭旧弹窗");
                currentRewardPopup.Close();
                currentRewardPopup = null;
            }

            // 设置显示标记
            isShowingPopup = true;

            // 保存数据
            pendingRewardData = data;

            // 根据配置类型显示对应弹窗
            switch (data.config.PopupType)
            {
                case RewardPopupType.Fixed:
                    ShowFixedRewardPopup(data);
                    break;
                case RewardPopupType.Sliding:
                    ShowSlidingRewardPopup(data);
                    break;
                default:
                    Debug.LogError($"[RewardPopupManager] 未知的弹窗类型: {data.config.PopupType}");
                    break;
            }
        }

        /// <summary>
        /// 显示奖励弹窗（使用当前关卡数据）
        /// </summary>
        public void ShowRewardPopupForCurrentLevel()
        {
            var levelManager = FindObjectOfType<LevelManager>();
            if (levelManager == null)
            {
                Debug.LogError("[RewardPopupManager] 无法找到LevelManager");
                return;
            }

            var level = levelManager.GetCurrentLevel();
            if (level == null)
            {
                Debug.LogError("[RewardPopupManager] 无法获取当前关卡");
                return;
            }

            if (level.rewardConfig == null)
            {
                Debug.LogWarning($"[RewardPopupManager] 关卡 {level.Number} 没有奖励配置");
                return;
            }

            // 计算基础奖励（返回放大后的整数值）
            int baseReward = CalculateBaseReward();

            // 创建奖励数据
            var data = new RewardPopupData
            {
                baseReward = baseReward,
                levelNumber = level.Number,
                source = "LevelComplete",
                config = level.rewardConfig,
                difficulty = GetLevelDifficulty(level),
                isPerfect = CheckPerfectClear(),
                comboCount = GetComboCount()
            };

            // 显示弹窗
            ShowRewardPopup(data);
        }

        /// <summary>
        /// 关闭当前奖励弹窗
        /// </summary>
        public void CloseCurrentPopup()
        {
            if (currentRewardPopup != null)
            {
                currentRewardPopup.Close();
                currentRewardPopup = null;
                isShowingPopup = false;  // 重置显示标记
            }
        }

        /// <summary>
        /// 重置显示状态（由弹窗Close时调用，避免递归）
        /// </summary>
        public void ResetShowingState()
        {
            currentRewardPopup = null;
            isShowingPopup = false;
            Debug.Log("[RewardPopupManager] 显示状态已重置");
        }

        /// <summary>
        /// 获取统计信息
        /// </summary>
        public string GetStatistics()
        {
            float avgReward = totalRewardsClaimed > 0 ? totalRewardAmount / totalRewardsClaimed : 0f;
            float claimRate = totalRewardsShown > 0 ? (float)totalRewardsClaimed / totalRewardsShown * 100f : 0f;

            return $"奖励统计:\n" +
                   $"- 显示次数: {totalRewardsShown}\n" +
                   $"- 领取次数: {totalRewardsClaimed}\n" +
                   $"- 跳过次数: {totalRewardsSkipped}\n" +
                   $"- 总奖励: ${totalRewardAmount:F3}\n" +
                   $"- 平均奖励: ${avgReward:F3}\n" +
                   $"- 领取率: {claimRate:F1}%";
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 显示固定倍率弹窗
        /// </summary>
        private void ShowFixedRewardPopup(RewardPopupData data)
        {
            if (fixedRewardPrefab == null)
            {
                Debug.LogError("[RewardPopupManager] FixedRewardPopup预制体未设置");
                isShowingPopup = false;  // 重置显示标记
                return;
            }

            Debug.Log($"[RewardPopupManager] 显示固定倍率弹窗 - 基础奖励: ${data.baseReward:F3}");

            // 使用MenuManager显示弹窗
            var popupBase = MenuManager.Instance.ShowPopup(fixedRewardPrefab);
            var popup = popupBase as FixedRewardPopup;
            if (popup != null)
            {
                currentRewardPopup = popup;
                popup.Initialize(data.baseReward, data.config, data.levelNumber, data.source);

                // 记录统计
                totalRewardsShown++;
                OnRewardPopupShown?.Invoke(RewardPopupType.Fixed);
            }
            else
            {
                Debug.LogError("[RewardPopupManager] 无法创建FixedRewardPopup实例");
                isShowingPopup = false;  // 重置显示标记
            }
        }

        /// <summary>
        /// 显示滑动倍率弹窗
        /// </summary>
        private void ShowSlidingRewardPopup(RewardPopupData data)
        {
            if (slidingRewardPrefab == null)
            {
                Debug.LogError("[RewardPopupManager] SlidingRewardPopup预制体未设置");
                isShowingPopup = false;  // 重置显示标记
                return;
            }

            Debug.Log($"[RewardPopupManager] 显示滑动倍率弹窗 - 基础奖励: ${data.baseReward:F3}");

            // 使用MenuManager显示弹窗
            var popupBase = MenuManager.Instance.ShowPopup(slidingRewardPrefab);
            var popup = popupBase as SlidingRewardPopup;
            if (popup != null)
            {
                currentRewardPopup = popup;
                popup.Initialize(data.baseReward, data.config, data.levelNumber, data.source);

                // 记录统计
                totalRewardsShown++;
                OnRewardPopupShown?.Invoke(RewardPopupType.Sliding);
            }
            else
            {
                Debug.LogError("[RewardPopupManager] 无法创建SlidingRewardPopup实例");
                isShowingPopup = false;  // 重置显示标记
            }
        }

        /// <summary>
        /// 计算基础奖励
        /// </summary>
        /// <returns>奖励基础数值（放大10000倍的整数值）</returns>
        private int CalculateBaseReward()
        {
            // 使用RewardCalculator计算
            if (RewardCalculator.Instance != null)
            {
                // 直接调用RewardCalculator，它会自动从CurrencyManager获取当前金币
                // 返回的是放大后的整数值
                return RewardCalculator.Instance.CalculateReward("LevelComplete");
            }
            else
            {
                // 如果没有RewardCalculator，使用简单计算
                Debug.LogWarning("[RewardPopupManager] RewardCalculator未初始化，使用默认奖励");
                return 1000;  // 默认0.1美元 = 1000（0.1 * 10000）
            }
        }

        /// <summary>
        /// 获取关卡难度
        /// </summary>
        private int GetLevelDifficulty(Level level)
        {
            // 根据关卡编号估算难度
            if (level.Number <= 10) return 1;
            if (level.Number <= 30) return 2;
            if (level.Number <= 60) return 3;
            if (level.Number <= 100) return 4;
            return 5;
        }

        /// <summary>
        /// 检查是否完美通关
        /// </summary>
        private bool CheckPerfectClear()
        {
            // TODO: 实现完美通关判断逻辑
            // 例如：无失误、全部目标达成、时间内完成等
            return false;
        }

        /// <summary>
        /// 获取连击数
        /// </summary>
        private int GetComboCount()
        {
            // TODO: 从游戏管理器获取连击数
            return 0;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// 游戏状态变化处理
        /// </summary>
        private void OnGameStateChanged(EGameState newState)
        {
            if (newState == EGameState.Win && autoShowOnWin)
            {
                Debug.Log("[RewardPopupManager] 检测到胜利，准备显示奖励弹窗");
                // 延迟显示，让Win弹窗先显示
                Invoke(nameof(ShowRewardPopupForCurrentLevel), showDelay);
            }
        }

        /// <summary>
        /// 处理奖励领取
        /// </summary>
        private void HandleRewardClaimed(int reward, int multiplier)
        {
            float rewardDollar = reward / 10000f;
            Debug.Log($"[RewardPopupManager] 奖励已领取 - 金额: ${rewardDollar:F3}, 倍率: x{multiplier}");

            // 更新统计
            totalRewardsClaimed++;
            totalRewardAmount += rewardDollar;

            // 触发事件
            OnRewardClaimComplete?.Invoke(rewardDollar, multiplier);

            // 清理当前弹窗引用
            currentRewardPopup = null;
            isShowingPopup = false;  // 重置显示标记

            // 保存统计数据
            SaveStatistics();
        }

        /// <summary>
        /// 处理奖励跳过
        /// </summary>
        private void HandleRewardSkipped(int reward)
        {
            float rewardDollar = reward / 10000f;
            Debug.Log($"[RewardPopupManager] 奖励已跳过 - 金额: ${rewardDollar:F3}");

            // 更新统计
            totalRewardsSkipped++;
            totalRewardAmount += rewardDollar;  // 跳过也会获得部分奖励

            // 触发事件
            OnRewardSkipComplete?.Invoke(rewardDollar);

            // 清理当前弹窗引用
            currentRewardPopup = null;
            isShowingPopup = false;  // 重置显示标记

            // 清理当前弹窗引用
            currentRewardPopup = null;

            // 保存统计数据
            SaveStatistics();
        }

        #endregion

        #region Statistics

        /// <summary>
        /// 保存统计数据
        /// </summary>
        private void SaveStatistics()
        {
            PlayerPrefs.SetInt("RewardPopup_TotalShown", totalRewardsShown);
            PlayerPrefs.SetInt("RewardPopup_TotalClaimed", totalRewardsClaimed);
            PlayerPrefs.SetInt("RewardPopup_TotalSkipped", totalRewardsSkipped);
            PlayerPrefs.SetFloat("RewardPopup_TotalAmount", totalRewardAmount);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 加载统计数据
        /// </summary>
        private void LoadStatistics()
        {
            totalRewardsShown = PlayerPrefs.GetInt("RewardPopup_TotalShown", 0);
            totalRewardsClaimed = PlayerPrefs.GetInt("RewardPopup_TotalClaimed", 0);
            totalRewardsSkipped = PlayerPrefs.GetInt("RewardPopup_TotalSkipped", 0);
            totalRewardAmount = PlayerPrefs.GetFloat("RewardPopup_TotalAmount", 0f);
        }

        /// <summary>
        /// 重置统计数据
        /// </summary>
        public void ResetStatistics()
        {
            totalRewardsShown = 0;
            totalRewardsClaimed = 0;
            totalRewardsSkipped = 0;
            totalRewardAmount = 0f;
            SaveStatistics();
            Debug.Log("[RewardPopupManager] 统计数据已重置");
        }

        #endregion

        #region Debug

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private float debugBaseRewardDollar = 0.1f;  // 美元值，用于编辑器测试
        [SerializeField] private RewardPopupType debugPopupType = RewardPopupType.Fixed;

        [ContextMenu("测试显示固定倍率弹窗")]
        private void Debug_ShowFixedPopup()
        {
            var config = LevelRewardConfig.CreateDefault();
            config.PopupType = RewardPopupType.Fixed;

            var data = new RewardPopupData
            {
                baseReward = Mathf.RoundToInt(debugBaseRewardDollar * 10000),  // 转换为整数值
                levelNumber = 1,
                source = "Debug",
                config = config
            };

            ShowRewardPopup(data);
        }

        [ContextMenu("测试显示滑动倍率弹窗")]
        private void Debug_ShowSlidingPopup()
        {
            var config = LevelRewardConfig.CreateDefault();
            config.PopupType = RewardPopupType.Sliding;

            var data = new RewardPopupData
            {
                baseReward = Mathf.RoundToInt(debugBaseRewardDollar * 10000),  // 转换为整数值
                levelNumber = 1,
                source = "Debug",
                config = config
            };

            ShowRewardPopup(data);
        }

        [ContextMenu("打印统计信息")]
        private void Debug_PrintStatistics()
        {
            Debug.Log("[RewardPopupManager] " + GetStatistics());
        }

        [ContextMenu("测试计算基础奖励")]
        private void Debug_CalculateBaseReward()
        {
            int rewardInt = CalculateBaseReward();
            float rewardDollar = BlockPuzzleGameToolkit.Scripts.CurrencySystem.CurrencyFormatter.ToDisplayValue(rewardInt);
            Debug.Log($"[RewardPopupManager] 计算的基础奖励: ${rewardDollar:F3} (整数值:{rewardInt})");
        }
#endif

        #endregion
    }
}