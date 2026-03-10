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
    /// 关卡奖励倍率配置（ScriptableObject）
    /// 管理关卡通关后的固定倍率奖励配置
    /// </summary>
    [CreateAssetMenu(fileName = "LevelRewardMultiplierSettings",
                     menuName = "BlockPuzzle/Settings/LevelRewardMultiplierSettings")]
    public class LevelRewardMultiplierSettings : SettingsBase
    {
        /// <summary>
        /// 固定倍率配置
        /// </summary>
        [Serializable]
        public class FixedMultiplierConfig
        {
            [Header("基础信息")]
            [SerializeField, Tooltip("配置唯一标识")]
            private string configId;

            [SerializeField, Tooltip("配置显示名称")]
            private string configName;

            [Header("倍率配置")]
            [SerializeField, Tooltip("按顺序使用的倍率值，到达最后一个后保持不变")]
            private float[] multipliers = {10f, 8f, 8f, 7f, 5f, 3f, 2f};

            [Header("重置策略")]
            [SerializeField, Tooltip("是否每日0点重置")]
            private bool resetDaily = true;

            [SerializeField, Tooltip("是否提现时重置")]
            private bool resetOnWithdraw = true;

            // 属性
            public string ConfigId
            {
                get => configId;
                set => configId = value;
            }

            public string ConfigName
            {
                get => configName;
                set => configName = value;
            }

            public float[] Multipliers
            {
                get => multipliers;
                set => multipliers = value;
            }

            public bool ResetDaily
            {
                get => resetDaily;
                set => resetDaily = value;
            }

            public bool ResetOnWithdraw
            {
                get => resetOnWithdraw;
                set => resetOnWithdraw = value;
            }

            /// <summary>
            /// 验证配置有效性
            /// </summary>
            public bool Validate()
            {
                if (string.IsNullOrEmpty(configId))
                {
                    Debug.LogError("[LevelRewardMultiplierSettings] 配置ID不能为空");
                    return false;
                }

                if (multipliers == null || multipliers.Length == 0)
                {
                    Debug.LogError($"[LevelRewardMultiplierSettings] 配置 {configId} 的倍率数组不能为空");
                    return false;
                }

                foreach (var multiplier in multipliers)
                {
                    if (multiplier <= 0)
                    {
                        Debug.LogError($"[LevelRewardMultiplierSettings] 配置 {configId} 的倍率必须大于0");
                        return false;
                    }
                }

                return true;
            }

            /// <summary>
            /// 获取指定索引的倍率
            /// </summary>
            public float GetMultiplier(int index)
            {
                if (multipliers == null || multipliers.Length == 0)
                    return 1.0f;

                // 确保索引不越界，到达最后一个后保持不变
                index = Mathf.Clamp(index, 0, multipliers.Length - 1);
                return multipliers[index];
            }

            /// <summary>
            /// 是否已达到最后一个倍率
            /// </summary>
            public bool IsLastMultiplier(int index)
            {
                return index >= multipliers.Length - 1;
            }
        }

        [Header("配置列表")]
        [SerializeField]
        private List<FixedMultiplierConfig> fixedMultiplierConfigs = new();

        /// <summary>
        /// 获取所有配置
        /// </summary>
        public IReadOnlyList<FixedMultiplierConfig> Configs => fixedMultiplierConfigs;

        /// <summary>
        /// 获取指定ID的配置
        /// </summary>
        public FixedMultiplierConfig GetConfig(string configId)
        {
            if (string.IsNullOrEmpty(configId))
                return null;

            return fixedMultiplierConfigs.FirstOrDefault(c => c.ConfigId == configId);
        }

        /// <summary>
        /// 获取所有配置ID列表（供编辑器下拉选择）
        /// </summary>
        public string[] GetAllConfigIds()
        {
            return fixedMultiplierConfigs
                .Where(c => !string.IsNullOrEmpty(c.ConfigId))
                .Select(c => c.ConfigId)
                .ToArray();
        }

        /// <summary>
        /// 获取所有配置名称列表（供编辑器显示）
        /// </summary>
        public string[] GetAllConfigNames()
        {
            return fixedMultiplierConfigs
                .Select(c => string.IsNullOrEmpty(c.ConfigName) ? c.ConfigId : c.ConfigName)
                .ToArray();
        }

        /// <summary>
        /// 检查配置是否存在
        /// </summary>
        public bool HasConfig(string configId)
        {
            return !string.IsNullOrEmpty(configId) &&
                   fixedMultiplierConfigs.Any(c => c.ConfigId == configId);
        }

        /// <summary>
        /// 验证所有配置
        /// </summary>
        public bool ValidateAll()
        {
            if (fixedMultiplierConfigs == null || fixedMultiplierConfigs.Count == 0)
            {
                Debug.LogWarning("[LevelRewardMultiplierSettings] 没有配置任何倍率");
                return false;
            }

            // 检查ID重复
            var duplicateIds = fixedMultiplierConfigs
                .Where(c => !string.IsNullOrEmpty(c.ConfigId))
                .GroupBy(c => c.ConfigId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            foreach (var id in duplicateIds)
            {
                Debug.LogError($"[LevelRewardMultiplierSettings] 发现重复的配置ID: {id}");
                return false;
            }

            // 验证每个配置
            bool allValid = true;
            foreach (var config in fixedMultiplierConfigs)
            {
                if (!config.Validate())
                {
                    allValid = false;
                }
            }

            return allValid;
        }

        #region 编辑器辅助方法

#if UNITY_EDITOR
        /// <summary>
        /// 添加默认配置（编辑器用）
        /// </summary>
        [ContextMenu("添加默认配置")]
        public void AddDefaultConfigs()
        {
            fixedMultiplierConfigs.Clear();

            // 第一个配置 - 默认配置（必须是第一个）
            var defaultConfig = new FixedMultiplierConfig
            {
                ConfigId = "default",
                ConfigName = "默认配置",
                Multipliers = new float[] {10f, 8f, 8f, 7f, 5f, 3f, 2f},
                ResetDaily = true,
                ResetOnWithdraw = true
            };
            fixedMultiplierConfigs.Add(defaultConfig);

            // 新手配置
            var beginnerConfig = new FixedMultiplierConfig
            {
                ConfigId = "beginner",
                ConfigName = "新手配置",
                Multipliers = new float[] {15f, 12f, 10f, 10f, 8f, 5f, 3f},
                ResetDaily = true,
                ResetOnWithdraw = false
            };
            fixedMultiplierConfigs.Add(beginnerConfig);

            // VIP配置
            var vipConfig = new FixedMultiplierConfig
            {
                ConfigId = "vip",
                ConfigName = "VIP配置",
                Multipliers = new float[] {20f, 15f, 12f, 10f, 8f, 5f, 5f},
                ResetDaily = false,
                ResetOnWithdraw = true
            };
            fixedMultiplierConfigs.Add(vipConfig);

            Debug.Log("[LevelRewardMultiplierSettings] 默认配置已添加");
        }

        /// <summary>
        /// 验证并修复配置
        /// </summary>
        [ContextMenu("验证配置")]
        private void ValidateAndFix()
        {
            if (ValidateAll())
            {
                Debug.Log("[LevelRewardMultiplierSettings] 所有配置验证通过");
            }
            else
            {
                Debug.LogError("[LevelRewardMultiplierSettings] 配置存在问题，请检查并修复");
            }
        }

        /// <summary>
        /// 添加新配置
        /// </summary>
        public void AddConfig(FixedMultiplierConfig config)
        {
            if (config != null && config.Validate())
            {
                fixedMultiplierConfigs.Add(config);
            }
        }

        /// <summary>
        /// 移除配置
        /// </summary>
        public void RemoveConfig(string configId)
        {
            fixedMultiplierConfigs.RemoveAll(c => c.ConfigId == configId);
        }
#endif

        #endregion
    }
}