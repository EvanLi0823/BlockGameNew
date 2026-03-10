// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

using System;
using UnityEngine;
using System.Linq;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using StorageSystem.Core;
using StorageSystem.Data;
using BlockPuzzleGameToolkit.Scripts.Settings;

namespace BlockPuzzleGameToolkit.Scripts.RewardSystem
{
    /// <summary>
    /// 奖励计算器（单例）
    /// 负责根据配置计算奖励基础数值
    /// </summary>
    public class RewardCalculator : SingletonBehaviour<RewardCalculator>
    {
        private const string SAVE_KEY = "reward_state_data";
        private const int PRECISION_SCALE = 10000; // 货币系统的放大倍数

        [SerializeField] private RewardSystemSettings config;
        private RewardStateSaveData stateData;
        private StorageOptions storageOptions;

        // 事件
        public event Action<string> OnRangeChanged;
        //此接口跟货币系统关联
        public event Action<int> OnRewardCalculated;  // 改为int，返回放大后的整数值

        // 属性
        public string CurrentRangeId => stateData?.CurrentRangeId ?? "no_withdraw";
        // IsInitialized 属性已经在基类 SingletonBehaviour 中定义，无需重复定义

        // 初始化优先级（在CurrencyManager之后）
        public override int InitPriority => 20;

        public override void Awake()
        {
            base.Awake();
            // 不再在Awake中初始化，等待SingletonInitializer调用OnInit
        }

        /// <summary>
        /// 单例初始化入口，由SingletonInitializer调用
        /// </summary>
        public override void OnInit()
        {
            if (!IsInitialized)
            {
                Initialize();
            }
            base.OnInit(); // 设置SingletonBehaviour的IsInitialized = true
        }

        private void Start()
        {
            // 备用初始化，如果SingletonInitializer没有调用
            if (!IsInitialized)
            {
                Initialize();
            }
        }

        private void Initialize()
        {
            // 加载配置
            if (config == null)
            {
                config = Resources.Load<RewardSystemSettings>("Settings/RewardSystemSettings");
                if (config == null)
                {
                    Debug.LogError("[RewardCalculator] 未找到RewardSystemSettings配置文件");
                    return;
                }
            }

            // 配置存储选项
            storageOptions = new StorageOptions
            {
                useEncryption = false,      // 不使用加密
                useCompression = false,     // 数据量小，不需要压缩
                addChecksum = true,         // 数据完整性验证
                version = 1,
                encryptionKey = "Reward_" + Application.identifier
            };

            LoadState();
            // IsInitialized 会在基类的 OnInit 中设置，无需在此重复设置
            Debug.Log($"[RewardCalculator] 初始化完成，当前区间: {CurrentRangeId}");
        }

        #region 公开接口

        /// <summary>
        /// 计算奖励基础数值
        /// </summary>
        /// <param name="sourceKey">来源键（用于获取倍率）</param>
        /// <returns>奖励基础数值（放大10000倍的整数值）</returns>
        public int CalculateReward(string sourceKey)
        {
            if (!IsInitialized || config == null)
            {
                Debug.LogError("[RewardCalculator] 系统未初始化或配置丢失");
                return 0;
            }

            // 从货币系统获取当前金币（已放大10000倍的整数值）
            var currencyManager = Scripts.CurrencySystem.CurrencyManager.Instance;
            if (currencyManager == null)
            {
                Debug.LogError("[RewardCalculator] CurrencyManager未初始化");
                return 0;
            }

            int currentCoinsInt = currencyManager.GetCoins();
            Debug.Log($"[RewardCalculator] 当前金币（整数值）: {currentCoinsInt}");
            // 将放大的整数值转换为实际美元值用于区间比较
            float actualCoins = currentCoinsInt / (float)PRECISION_SCALE;

            // 获取当前区间配置
            var rangeGroup = GetCurrentRangeGroup();
            if (rangeGroup == null)
            {
                Debug.LogError($"[RewardCalculator] 未找到区间配置: {CurrentRangeId}");
                return 0;
            }

            // 获取基础数值（使用实际美元值进行比较）
            float baseValue = GetBaseValue(rangeGroup, actualCoins);
            if (baseValue <= 0)
            {
                Debug.LogWarning($"[RewardCalculator] 基础数值为0，当前金币: ${actualCoins:F2}");
                return 0;
            }

            // 获取倍率
            float multiplier = GetMultiplier(sourceKey);
            Debug.Log($"[RewardCalculator] 来源: {sourceKey}, 倍率: {multiplier:F2}");

            // 计算最终奖励（美元值）
            float finalRewardDollar = baseValue * multiplier;

            // 转换为放大后的整数值
            int finalRewardInt = Mathf.RoundToInt(finalRewardDollar * PRECISION_SCALE);

            // 检查是否超过最大值限制
            // 将最大值转换为放大后的整数进行比较
            int maxRewardValueInt = Mathf.RoundToInt(config.MaxRewardValue * PRECISION_SCALE);

            // 检查：当前金币 + 计算的奖励 > 最大值
            if (currentCoinsInt + finalRewardInt > maxRewardValueInt)
            {
                Debug.LogWarning($"[RewardCalculator] 奖励超过最大值限制！当前金币={currentCoinsInt}, 计算奖励={finalRewardInt}, 最大值={maxRewardValueInt}，返回0");
                return 0;
            }

            Debug.Log($"[RewardCalculator] 计算奖励: 当前金币=${actualCoins:F2}, 基础=${baseValue:F3}, 倍率={multiplier:F2}, 最终=${finalRewardDollar:F3} (整数值:{finalRewardInt})");
            OnRewardCalculated?.Invoke(finalRewardInt);

            return finalRewardInt;
        }

        /// <summary>
        /// 切换到指定区间
        /// </summary>
        /// <param name="rangeId">区间ID</param>
        public bool SwitchToRange(string rangeId)
        {
            if (string.IsNullOrEmpty(rangeId))
            {
                Debug.LogError("[RewardCalculator] 区间ID不能为空");
                return false;
            }

            // 验证区间是否存在
            if (!config.HasRangeGroup(rangeId))
            {
                Debug.LogError($"[RewardCalculator] 未找到区间: {rangeId}");
                return false;
            }

            string oldRangeId = stateData.CurrentRangeId;
            stateData.CurrentRangeId = rangeId;

            if (SaveState())
            {
                Debug.Log($"[RewardCalculator] 区间切换: {oldRangeId} -> {rangeId}");
                OnRangeChanged?.Invoke(rangeId);
                return true;
            }
            else
            {
                // 回滚
                stateData.CurrentRangeId = oldRangeId;
                Debug.LogError("[RewardCalculator] 保存失败，区间切换已回滚");
                return false;
            }
        }

        /// <summary>
        /// 处理提现后的区间重置
        /// </summary>
        /// <param name="withdrawTier">提现档位（1, 2, 3...）</param>
        public void HandleWithdraw(int withdrawTier)
        {
            string newRangeId = $"tier_{withdrawTier}";

            // 切换到对应档位区间
            if (!SwitchToRange(newRangeId))
            {
                // 如果对应档位不存在，使用默认档位
                Debug.LogWarning($"[RewardCalculator] 档位{withdrawTier}不存在，使用默认区间");
                SwitchToRange("tier_1");
            }

            Debug.Log($"[RewardCalculator] 提现处理完成，新区间: {CurrentRangeId}");
        }

        /// <summary>
        /// 获取当前区间信息
        /// </summary>
        public Settings.RangeGroup GetCurrentRangeGroup()
        {
            if (config == null) return null;
            return config.GetRangeGroup(CurrentRangeId);
        }

        /// <summary>
        /// 获取指定来源的倍率
        /// </summary>
        public float GetMultiplier(string sourceKey)
        {
            if (config == null || string.IsNullOrEmpty(sourceKey))
                return 1.0f;

            return config.GetMultiplier(sourceKey);
        }

        /// <summary>
        /// 重置到默认区间
        /// </summary>
        public void Reset()
        {
            if (stateData == null)
            {
                Debug.LogError("[RewardCalculator] 状态数据未初始化");
                return;
            }

            stateData.Reset();

            if (SaveState())
            {
                Debug.Log("[RewardCalculator] 已重置到默认区间");
                OnRangeChanged?.Invoke(stateData.CurrentRangeId);
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 根据当前金币获取基础数值
        /// </summary>
        /// <param name="rangeGroup">区间配置组</param>
        /// <param name="currentCoins">当前金币数量（实际美元值，未放大）</param>
        /// <returns>奖励基础数值</returns>
        private float GetBaseValue(Settings.RangeGroup rangeGroup, float currentCoins)
        {
            // 找到当前金币所在的数值范围
            var valueRange = rangeGroup.ValueRanges
                .FirstOrDefault(r => currentCoins >= r.MinCoins && currentCoins < r.MaxCoins);

            if (valueRange == null)
            {
                // 如果超出所有范围，使用最后一个范围
                valueRange = rangeGroup.ValueRanges.LastOrDefault();
                if (valueRange == null) return 0f;
            }

            Debug.Log($"[RewardCalculator] 选定数值范围: {valueRange.MinCoins} - {valueRange.MaxCoins} (当前金币: ${currentCoins:F2})");
            // 在指定范围内生成随机值
            float randomValue = UnityEngine.Random.Range(valueRange.MinReward, valueRange.MaxReward);

            // 保留3位小数（最小单位0.001）
            return Mathf.Round(randomValue * 1000f) / 1000f;
        }

        #endregion

        #region 存储操作

        private void LoadState()
        {
            try
            {
                stateData = StorageManager.Instance.Load<RewardStateSaveData>(
                    SAVE_KEY,
                    StorageType.Binary
                );

                if (stateData != null)
                {
                    // 验证数据完整性
                    if (!stateData.ValidateChecksum())
                    {
                        Debug.LogWarning("[RewardCalculator] 数据校验失败，使用默认数据");
                        stateData = RewardStateSaveData.CreateDefault();
                        SaveState();
                    }
                    else if (!stateData.IsValid())
                    {
                        Debug.LogWarning("[RewardCalculator] 数据无效，重置为默认");
                        stateData = RewardStateSaveData.CreateDefault();
                        SaveState();
                    }
                    // 验证区间是否存在于配置中
                    else if (!config.HasRangeGroup(stateData.CurrentRangeId))
                    {
                        Debug.LogWarning($"[RewardCalculator] 区间{stateData.CurrentRangeId}不存在，重置为默认");
                        stateData = RewardStateSaveData.CreateDefault();
                        SaveState();
                    }
                }
                else
                {
                    // 首次运行，创建默认数据
                    stateData = RewardStateSaveData.CreateDefault();
                    SaveState();
                }

                Debug.Log($"[RewardCalculator] 状态加载成功: {stateData}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[RewardCalculator] 加载状态失败: {e.Message}");
                stateData = RewardStateSaveData.CreateDefault();
            }
        }

        private bool SaveState()
        {
            if (stateData == null)
            {
                Debug.LogError("[RewardCalculator] 无状态可保存");
                return false;
            }

            try
            {
                stateData.UpdateMetadata(storageOptions.version);

                bool success = StorageManager.Instance.Save(
                    SAVE_KEY,
                    stateData,
                    StorageType.Binary,
                    storageOptions
                );

                if (success)
                {
                    Debug.Log($"[RewardCalculator] 状态保存成功: {stateData}");
                }
                else
                {
                    Debug.LogError("[RewardCalculator] 状态保存失败");
                }

                return success;
            }
            catch (Exception e)
            {
                Debug.LogError($"[RewardCalculator] 保存状态异常: {e.Message}");
                return false;
            }
        }

        #endregion

        #region Unity生命周期

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && IsInitialized)
            {
                SaveState(); // 暂停时保存
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && IsInitialized)
            {
                SaveState(); // 失去焦点时保存
            }
        }

        private void OnDestroy()
        {
            // 在应用退出时不要保存，避免访问已销毁的单例
            if (IsInitialized && !isApplicationQuitting)
            {
                SaveState(); // 销毁时保存
            }
        }

        private bool isApplicationQuitting = false;

        private void OnApplicationQuit()
        {
            isApplicationQuitting = true;
            // 应用退出时最后保存一次
            if (IsInitialized)
            {
                SaveStateSafely();
            }
        }

        /// <summary>
        /// 安全保存状态（不创建新的单例实例）
        /// </summary>
        private void SaveStateSafely()
        {
            if (stateData == null) return;

            // 先检查StorageManager实例是否存在
            var storageManager = FindObjectOfType<StorageManager>();
            if (storageManager != null)
            {
                try
                {
                    stateData.UpdateMetadata(storageOptions.version);
                    storageManager.Save(
                        SAVE_KEY,
                        stateData,
                        StorageType.Binary,
                        storageOptions
                    );
                    Debug.Log("[RewardCalculator] 应用退出时状态保存成功");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[RewardCalculator] 应用退出时保存状态异常: {e.Message}");
                }
            }
        }

        #endregion

        #region 调试功能

#if UNITY_EDITOR
        [ContextMenu("切换到档位1")]
        private void Debug_SwitchToTier1()
        {
            SwitchToRange("tier_1");
        }

        [ContextMenu("切换到档位2")]
        private void Debug_SwitchToTier2()
        {
            SwitchToRange("tier_2");
        }

        [ContextMenu("计算测试奖励")]
        private void Debug_CalculateTestReward()
        {
            int rewardInt = CalculateReward("FloatingReward");
            float rewardDollar = rewardInt / (float)PRECISION_SCALE;
            Debug.Log($"[RewardCalculator] 测试奖励: ${rewardDollar:F3} (整数值:{rewardInt})");
        }

        [ContextMenu("打印当前状态")]
        private void Debug_PrintStatus()
        {
            Debug.Log($"[RewardCalculator] 当前状态:\n" +
                     $"- 区间: {CurrentRangeId}\n" +
                     $"- 初始化: {IsInitialized}\n" +
                     $"- 配置: {(config != null ? "已加载" : "未加载")}");
        }
#endif

        #endregion
    }
}