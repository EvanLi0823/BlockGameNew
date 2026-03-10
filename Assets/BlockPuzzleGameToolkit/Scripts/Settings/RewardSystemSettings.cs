// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Settings
{
    /// <summary>
    /// 奖励系统配置（ScriptableObject）
    /// 集中管理所有区间、数值范围和倍率配置
    /// </summary>
    public class RewardSystemSettings : SettingsBase
    {
        [Header("精度配置")]
        [SerializeField, Tooltip("存储精度倍数，用于将浮点数转换为整数存储")]
        private int precisionScale = 10000;

        [Header("区间配置")]
        [SerializeField, Tooltip("所有区间分组")]
        private List<RangeGroup> rangeGroups = new List<RangeGroup>();

        [Header("倍率配置")]
        [SerializeField, Tooltip("不同来源的基础倍率")]
        private List<MultiplierEntry> multipliers = new List<MultiplierEntry>();

        [Header("限制配置")]
        [SerializeField, Tooltip("奖励最大值限制（美元）")]
        private float maxRewardValue = 10000f; // 默认最大值为10000美元

        // 属性
        public int PrecisionScale => precisionScale;
        public float MaxRewardValue => maxRewardValue;
        public IReadOnlyList<RangeGroup> RangeGroups => rangeGroups;
        public IReadOnlyList<MultiplierEntry> Multipliers => multipliers;

        /// <summary>
        /// 获取指定ID的区间分组
        /// </summary>
        public RangeGroup GetRangeGroup(string rangeId)
        {
            return rangeGroups.FirstOrDefault(g => g.RangeId == rangeId);
        }

        /// <summary>
        /// 检查区间是否存在
        /// </summary>
        public bool HasRangeGroup(string rangeId)
        {
            return rangeGroups.Any(g => g.RangeId == rangeId);
        }

        /// <summary>
        /// 获取指定来源的倍率
        /// </summary>
        public float GetMultiplier(string sourceKey)
        {
            var entry = multipliers.FirstOrDefault(m => m.SourceKey == sourceKey);
            return entry != null ? entry.Multiplier : 1.0f;
        }

        /// <summary>
        /// 验证配置有效性
        /// </summary>
        public bool ValidateConfig()
        {
            // 检查是否有默认区间
            if (!HasRangeGroup("no_withdraw"))
            {
                Debug.LogError("[RewardSystemSettings] 缺少默认区间: no_withdraw");
                return false;
            }

            // 检查每个区间的配置
            foreach (var group in rangeGroups)
            {
                if (!group.Validate())
                {
                    Debug.LogError($"[RewardSystemSettings] 区间配置无效: {group.RangeId}");
                    return false;
                }
            }

            // 检查倍率配置
            foreach (var multiplier in multipliers)
            {
                if (!multiplier.Validate())
                {
                    Debug.LogError($"[RewardSystemSettings] 倍率配置无效: {multiplier.SourceKey}");
                    return false;
                }
            }

            return true;
        }

        #region 编辑器辅助方法

#if UNITY_EDITOR
        /// <summary>
        /// 添加默认配置（编辑器用）
        /// </summary>
        [ContextMenu("添加默认配置")]
        public void AddDefaultConfig()
        {
            // 清空现有配置
            rangeGroups.Clear();
            multipliers.Clear();

            // 添加默认区间：未提现（0-300）
            var noWithdrawRange = new RangeGroup
            {
                RangeId = "no_withdraw",
                DisplayName = "未提现",
                MinTotal = 0,
                MaxTotal = 300,
                ValueRanges = new List<ValueRange>
                {
                    new ValueRange { MinCoins = 0, MaxCoins = 100, MinReward = 0.001f, MaxReward = 0.01f },
                    new ValueRange { MinCoins = 100, MaxCoins = 200, MinReward = 0.005f, MaxReward = 0.008f },
                    new ValueRange { MinCoins = 200, MaxCoins = 300, MinReward = 0.003f, MaxReward = 0.005f }
                }
            };
            rangeGroups.Add(noWithdrawRange);

            // 添加档位1（0-900）
            var tier1Range = new RangeGroup
            {
                RangeId = "tier_1",
                DisplayName = "档位1",
                MinTotal = 0,
                MaxTotal = 900,
                ValueRanges = new List<ValueRange>
                {
                    new ValueRange { MinCoins = 0, MaxCoins = 300, MinReward = 0.01f, MaxReward = 0.05f },
                    new ValueRange { MinCoins = 300, MaxCoins = 600, MinReward = 0.008f, MaxReward = 0.03f },
                    new ValueRange { MinCoins = 600, MaxCoins = 900, MinReward = 0.005f, MaxReward = 0.01f }
                }
            };
            rangeGroups.Add(tier1Range);

            // 添加档位2（0-3000）
            var tier2Range = new RangeGroup
            {
                RangeId = "tier_2",
                DisplayName = "档位2",
                MinTotal = 0,
                MaxTotal = 3000,
                ValueRanges = new List<ValueRange>
                {
                    new ValueRange { MinCoins = 0, MaxCoins = 1000, MinReward = 0.02f, MaxReward = 0.1f },
                    new ValueRange { MinCoins = 1000, MaxCoins = 2000, MinReward = 0.015f, MaxReward = 0.05f },
                    new ValueRange { MinCoins = 2000, MaxCoins = 3000, MinReward = 0.01f, MaxReward = 0.02f }
                }
            };
            rangeGroups.Add(tier2Range);

            // 添加默认倍率
            multipliers.Add(new MultiplierEntry { SourceKey = "FloatingReward", Multiplier = 0.8f });
            multipliers.Add(new MultiplierEntry { SourceKey = "LevelComplete", Multiplier = 1.0f });
            multipliers.Add(new MultiplierEntry { SourceKey = "DailyBonus", Multiplier = 1.2f });
            multipliers.Add(new MultiplierEntry { SourceKey = "LuckySpin", Multiplier = 1.5f });

            Debug.Log("[RewardSystemSettings] 默认配置已添加");
        }

        /// <summary>
        /// 验证并修复配置
        /// </summary>
        [ContextMenu("验证配置")]
        private void ValidateAndFix()
        {
            bool hasIssues = false;

            // 修复区间ID为空的问题
            foreach (var group in rangeGroups)
            {
                if (string.IsNullOrEmpty(group.RangeId))
                {
                    Debug.LogWarning("[RewardSystemSettings] 发现空区间ID，请手动设置");
                    hasIssues = true;
                }

                // 确保数值范围不重叠
                group.SortValueRanges();

                // 检查数值范围连续性
                for (int i = 1; i < group.ValueRanges.Count; i++)
                {
                    if (Math.Abs(group.ValueRanges[i].MinCoins - group.ValueRanges[i - 1].MaxCoins) > 0.001f)
                    {
                        Debug.LogWarning($"[RewardSystemSettings] 区间 {group.RangeId} 的数值范围不连续");
                        hasIssues = true;
                    }
                }
            }

            // 检查倍率键重复
            var duplicateKeys = multipliers
                .GroupBy(m => m.SourceKey)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            foreach (var key in duplicateKeys)
            {
                Debug.LogWarning($"[RewardSystemSettings] 发现重复的倍率键: {key}");
                hasIssues = true;
            }

            if (!hasIssues)
            {
                Debug.Log("[RewardSystemSettings] 配置验证通过");
            }
            else
            {
                Debug.LogError("[RewardSystemSettings] 配置存在问题，请检查并修复");
            }
        }
#endif

        #endregion
    }

    /// <summary>
    /// 区间分组
    /// </summary>
    [Serializable]
    public class RangeGroup
    {
        [SerializeField, Tooltip("区间唯一标识")]
        private string rangeId;

        [SerializeField, Tooltip("显示名称")]
        private string displayName;

        [SerializeField, Tooltip("区间最小值")]
        private float minTotal;

        [SerializeField, Tooltip("区间最大值")]
        private float maxTotal;

        [SerializeField, Tooltip("数值范围列表")]
        private List<ValueRange> valueRanges = new List<ValueRange>();

        // 属性
        public string RangeId
        {
            get => rangeId;
            set => rangeId = value;
        }

        public string DisplayName
        {
            get => displayName;
            set => displayName = value;
        }

        public float MinTotal
        {
            get => minTotal;
            set => minTotal = value;
        }

        public float MaxTotal
        {
            get => maxTotal;
            set => maxTotal = value;
        }

        public List<ValueRange> ValueRanges
        {
            get => valueRanges;
            set => valueRanges = value;
        }

        /// <summary>
        /// 验证配置有效性
        /// </summary>
        public bool Validate()
        {
            if (string.IsNullOrEmpty(rangeId))
                return false;

            if (minTotal < 0 || maxTotal <= minTotal)
                return false;

            if (valueRanges == null || valueRanges.Count == 0)
                return false;

            foreach (var range in valueRanges)
            {
                if (!range.Validate())
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 排序数值范围
        /// </summary>
        public void SortValueRanges()
        {
            valueRanges.Sort((a, b) => a.MinCoins.CompareTo(b.MinCoins));
        }
    }

    /// <summary>
    /// 数值范围
    /// </summary>
    [Serializable]
    public class ValueRange
    {
        [SerializeField, Tooltip("金币范围最小值")]
        private float minCoins;

        [SerializeField, Tooltip("金币范围最大值")]
        private float maxCoins;

        [SerializeField, Tooltip("奖励最小值")]
        private float minReward;

        [SerializeField, Tooltip("奖励最大值")]
        private float maxReward;

        // 属性
        public float MinCoins
        {
            get => minCoins;
            set => minCoins = value;
        }

        public float MaxCoins
        {
            get => maxCoins;
            set => maxCoins = value;
        }

        public float MinReward
        {
            get => minReward;
            set => minReward = value;
        }

        public float MaxReward
        {
            get => maxReward;
            set => maxReward = value;
        }

        /// <summary>
        /// 验证配置有效性
        /// </summary>
        public bool Validate()
        {
            if (minCoins < 0 || maxCoins <= minCoins)
                return false;

            if (minReward < 0.001f || maxReward < minReward)
                return false;

            return true;
        }
    }

    /// <summary>
    /// 倍率配置项
    /// </summary>
    [Serializable]
    public class MultiplierEntry
    {
        [SerializeField, Tooltip("来源键")]
        private string sourceKey;

        [SerializeField, Tooltip("倍率值")]
        private float multiplier = 1.0f;

        // 属性
        public string SourceKey
        {
            get => sourceKey;
            set => sourceKey = value;
        }

        public float Multiplier
        {
            get => multiplier;
            set => multiplier = Mathf.Max(0.01f, value);
        }

        /// <summary>
        /// 验证配置有效性
        /// </summary>
        public bool Validate()
        {
            if (string.IsNullOrEmpty(sourceKey))
                return false;

            if (multiplier <= 0)
                return false;

            return true;
        }
    }
}