// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

using System;
using UnityEngine;
using BlockPuzzleGameToolkit.Scripts.LevelsData;

namespace BlockPuzzleGameToolkit.Scripts.Popups.Reward
{
    /// <summary>
    /// 奖励弹窗数据结构
    /// 用于在不同组件间传递奖励相关数据
    /// </summary>
    [Serializable]
    public class RewardPopupData
    {
        [Header("基础数据")]
        [Tooltip("基础奖励金额（放大10000倍的整数值）")]
        public int baseReward;

        [Tooltip("关卡编号")]
        public int levelNumber;

        [Tooltip("奖励来源标识（用于获取倍率配置）")]
        public string source = "LevelComplete";

        [Header("配置")]
        [Tooltip("关卡奖励配置")]
        public LevelRewardConfig config;

        [Header("可选数据")]
        [Tooltip("关卡难度（用于调整奖励）")]
        public int difficulty = 1;

        [Tooltip("是否完美通关")]
        public bool isPerfect = false;

        [Tooltip("连续通关数")]
        public int comboCount = 0;

        /// <summary>
        /// 创建默认数据
        /// </summary>
        public static RewardPopupData CreateDefault()
        {
            return new RewardPopupData
            {
                baseReward = 1000,  // 0.1美元 = 1000（0.1 * 10000）
                levelNumber = 1,
                source = "LevelComplete",
                config = LevelRewardConfig.CreateDefault()
            };
        }

        /// <summary>
        /// 验证数据有效性
        /// </summary>
        public bool IsValid()
        {
            if (baseReward <= 0)
            {
                Debug.LogWarning("[RewardPopupData] 基础奖励无效");
                return false;
            }

            if (config == null)
            {
                Debug.LogWarning("[RewardPopupData] 配置为空");
                return false;
            }

            if (string.IsNullOrEmpty(source))
            {
                Debug.LogWarning("[RewardPopupData] 来源标识为空");
                return false;
            }

            return true;
        }

        public override string ToString()
        {
            float baseRewardDollar = baseReward / 10000f;
            return $"RewardPopupData[Level:{levelNumber}, Base:${baseRewardDollar:F3}, Source:{source}, Type:{config?.PopupType}]";
        }
    }
}