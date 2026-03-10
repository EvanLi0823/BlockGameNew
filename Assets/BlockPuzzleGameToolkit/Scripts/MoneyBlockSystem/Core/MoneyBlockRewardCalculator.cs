// 金钱方块系统 - 奖励计算器
// 创建日期: 2026-03-05

using UnityEngine;
using BlockPuzzleGameToolkit.Scripts.RewardSystem;
using BlockPuzzleGameToolkit.Scripts.Multiplier.Core;

namespace BlockPuzzleGameToolkit.Scripts.MoneyBlockSystem
{
    /// <summary>
    /// 金钱方块奖励计算器
    /// 职责:
    /// - 小额奖励计算(基础数值×小倍率×全局倍率)
    /// - 大额奖励计算(基础数值×大倍率×全局倍率)
    /// - 广告多倍计算
    /// - 与RewardCalculator和MultiplierManager集成
    /// </summary>
    public class MoneyBlockRewardCalculator
    {
        private const string SMALL_REWARD_SOURCE_KEY = "MoneyBlockSmall";
        private const string LARGE_REWARD_SOURCE_KEY = "MoneyBlockLarge";

        private readonly MoneyBlockSettings settings;
        private bool enableDebugLog;

        public MoneyBlockRewardCalculator(MoneyBlockSettings settings)
        {
            this.settings = settings;
            this.enableDebugLog = settings != null && settings.enableDebugLog;
        }

        /// <summary>
        /// 计算小额奖励(即时消除)
        /// </summary>
        /// <returns>放大10000倍的整数值</returns>
        public int CalculateSmallReward()
        {
            var rewardCalculator = RewardCalculator.Instance;
            if (rewardCalculator == null)
            {
                Debug.LogError("[MoneyBlockRewardCalculator] RewardCalculator未找到");
                return 0;
            }

            // 获取基础数值(已经是放大10000倍的整数)
            int baseReward = rewardCalculator.CalculateReward(SMALL_REWARD_SOURCE_KEY);

            // 应用金钱方块小倍率
            int rewardAfterSmallMultiplier = Mathf.RoundToInt(baseReward * settings.smallRewardMultiplier);

            // 应用全局倍率
            int finalReward = ApplyGlobalMultiplier(rewardAfterSmallMultiplier);

            if (enableDebugLog)
            {
                Debug.Log($"[MoneyBlockRewardCalculator] 小额奖励计算: " +
                          $"基础={baseReward}, " +
                          $"小倍率={settings.smallRewardMultiplier}, " +
                          $"全局倍率后={finalReward}");
            }

            return finalReward;
        }

        /// <summary>
        /// 计算大额奖励(累计触发)
        /// </summary>
        /// <returns>放大10000倍的整数值</returns>
        public int CalculateLargeReward()
        {
            var rewardCalculator = RewardCalculator.Instance;
            if (rewardCalculator == null)
            {
                Debug.LogError("[MoneyBlockRewardCalculator] RewardCalculator未找到");
                return 0;
            }

            // 获取基础数值(已经是放大10000倍的整数)
            int baseReward = rewardCalculator.CalculateReward(LARGE_REWARD_SOURCE_KEY);

            // 应用金钱方块大倍率
            int rewardAfterLargeMultiplier = Mathf.RoundToInt(baseReward * settings.largeRewardMultiplier);

            // 应用全局倍率
            int finalReward = ApplyGlobalMultiplier(rewardAfterLargeMultiplier);

            if (enableDebugLog)
            {
                Debug.Log($"[MoneyBlockRewardCalculator] 大额奖励计算: " +
                          $"基础={baseReward}, " +
                          $"大倍率={settings.largeRewardMultiplier}, " +
                          $"全局倍率后={finalReward}");
            }

            return finalReward;
        }

        /// <summary>
        /// 应用广告多倍倍率
        /// </summary>
        public int ApplyAdMultiplier(int baseReward, float multiplier)
        {
            int finalReward = Mathf.RoundToInt(baseReward * multiplier);

            if (enableDebugLog)
            {
                Debug.Log($"[MoneyBlockRewardCalculator] 广告多倍计算: " +
                          $"基础={baseReward}, " +
                          $"倍率={multiplier}, " +
                          $"最终={finalReward}");
            }

            return finalReward;
        }

        /// <summary>
        /// 应用全局倍率
        /// </summary>
        private int ApplyGlobalMultiplier(int reward)
        {
            var multiplierManager = MultiplierManager.Instance;
            if (multiplierManager == null)
            {
                if (enableDebugLog)
                {
                    Debug.LogWarning("[MoneyBlockRewardCalculator] MultiplierManager未找到，使用默认倍率1");
                }
                return reward;
            }

            // 获取全局倍率（MultiplierManager提供公开方法）
            int globalMultiplier = multiplierManager.GetCurrentMultiplier();

            if (enableDebugLog)
            {
                Debug.Log($"[MoneyBlockRewardCalculator] 应用全局倍率: x{globalMultiplier}");
            }

            return reward * globalMultiplier;
        }
    }
}
