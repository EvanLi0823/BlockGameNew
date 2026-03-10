// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Settings
{
    /// <summary>
    /// 滑动倍率模块配置设置
    /// </summary>
    [CreateAssetMenu(fileName = "SliderMultiplierSettings",
                     menuName = "BlockPuzzleGameToolkit/Settings/SliderMultiplierSettings")]
    public class SliderMultiplierSettings : ScriptableObject
    {
        [Header("滑动设置")]
        [Tooltip("滑块每秒移动的像素距离")]
        [Range(10f, 1000f)]
        public float sliderSpeed = 50f;

        [Header("提现前配置")]
        [Tooltip("用户提现前使用的配置组")]
        public List<MultiplierConfigSet> preWithdrawConfigs = new List<MultiplierConfigSet>();

        [Header("提现后配置")]
        [Tooltip("用户提现后使用的配置组")]
        public List<MultiplierConfigSet> postWithdrawConfigs = new List<MultiplierConfigSet>();

        [Header("重置设置")]
        [Tooltip("是否启用每日重置")]
        public bool enableDailyReset = true;

        [Tooltip("每日重置时间点（格式：HH:mm:ss）")]
        public string resetTime = "00:00:00";

        /// <summary>
        /// 倍率配置集
        /// </summary>
        [Serializable]
        public class MultiplierConfigSet
        {
            [Header("倍率值（5个区域）")]
            [Tooltip("从左到右5个区域的倍率值，区域边界在UI组件上配置")]
            public float[] multipliers = new float[5] { 0.5f, 0.8f, 1f, 3f, 5f };

            [Header("权重配置")]
            [Tooltip("该组配置在随机选择时的权重")]
            [Range(1, 100)]
            public int weight = 10;

            [Header("停止系数")]
            [Tooltip("滑块停止的难易程度，1为正常，值越大停止越困难")]
            [Range(0.1f, 3f)]
            public float stopCoefficient = 1f;

            /// <summary>
            /// 验证配置有效性
            /// </summary>
            public bool Validate()
            {
                if (multipliers == null || multipliers.Length != 5)
                {
                    Debug.LogError($"[SliderMultiplierSettings] 倍率数组必须包含5个元素");
                    return false;
                }

                if (weight <= 0)
                {
                    Debug.LogError($"[SliderMultiplierSettings] 权重必须大于0");
                    return false;
                }

                if (stopCoefficient <= 0)
                {
                    Debug.LogError($"[SliderMultiplierSettings] 停止系数必须大于0");
                    return false;
                }

                return true;
            }

            /// <summary>
            /// 获取区域内的倍率值
            /// </summary>
            /// <param name="regionIndex">区域索引（0-4）</param>
            /// <returns>该区域的倍率值</returns>
            public float GetMultiplier(int regionIndex)
            {
                if (regionIndex < 0 || regionIndex >= 5)
                {
                    Debug.LogError($"[SliderMultiplierSettings] 区域索引越界: {regionIndex}");
                    return 1f; // 返回默认倍率
                }
                return multipliers[regionIndex];
            }
        }

        /// <summary>
        /// 根据权重随机选择一个提现前配置
        /// </summary>
        public MultiplierConfigSet GetRandomPreWithdrawConfig()
        {
            return GetRandomConfig(preWithdrawConfigs);
        }

        /// <summary>
        /// 根据权重随机选择一个提现后配置
        /// </summary>
        public MultiplierConfigSet GetRandomPostWithdrawConfig()
        {
            return GetRandomConfig(postWithdrawConfigs);
        }

        /// <summary>
        /// 根据权重从配置列表中随机选择一个配置
        /// </summary>
        private MultiplierConfigSet GetRandomConfig(List<MultiplierConfigSet> configs)
        {
            if (configs == null || configs.Count == 0)
            {
                Debug.LogWarning($"[SliderMultiplierSettings] 配置列表为空，返回默认配置");
                return new MultiplierConfigSet();
            }

            // 计算总权重
            int totalWeight = 0;
            foreach (var config in configs)
            {
                if (config != null && config.weight > 0)
                {
                    totalWeight += config.weight;
                }
            }

            if (totalWeight <= 0)
            {
                Debug.LogWarning($"[SliderMultiplierSettings] 总权重为0，返回第一个配置");
                return configs[0];
            }

            // 根据权重随机选择
            int randomValue = UnityEngine.Random.Range(0, totalWeight);
            int currentWeight = 0;

            foreach (var config in configs)
            {
                if (config != null && config.weight > 0)
                {
                    currentWeight += config.weight;
                    if (randomValue < currentWeight)
                    {
                        return config;
                    }
                }
            }

            // 如果没有选中（不应该发生），返回最后一个有效配置
            return configs[configs.Count - 1];
        }

        /// <summary>
        /// 验证所有配置
        /// </summary>
        public bool ValidateAll()
        {
            bool isValid = true;

            // 验证滑动设置
            if (sliderSpeed <= 0)
            {
                Debug.LogError($"[SliderMultiplierSettings] 滑块速度必须大于0");
                isValid = false;
            }

            // 验证提现前配置
            if (preWithdrawConfigs == null || preWithdrawConfigs.Count == 0)
            {
                Debug.LogWarning($"[SliderMultiplierSettings] 提现前配置为空");
            }
            else
            {
                foreach (var config in preWithdrawConfigs)
                {
                    if (config != null && !config.Validate())
                    {
                        isValid = false;
                    }
                }
            }

            // 验证提现后配置
            if (postWithdrawConfigs == null || postWithdrawConfigs.Count == 0)
            {
                Debug.LogWarning($"[SliderMultiplierSettings] 提现后配置为空");
            }
            else
            {
                foreach (var config in postWithdrawConfigs)
                {
                    if (config != null && !config.Validate())
                    {
                        isValid = false;
                    }
                }
            }

            return isValid;
        }

        /// <summary>
        /// 根据索引获取配置
        /// </summary>
        /// <param name="isWithdrawn">是否已提现</param>
        /// <param name="index">配置索引</param>
        /// <returns>对应的配置集</returns>
        public MultiplierConfigSet GetConfig(bool isWithdrawn, int index)
        {
            var configs = isWithdrawn ? postWithdrawConfigs : preWithdrawConfigs;

            if (configs == null || configs.Count == 0)
            {
                Debug.LogWarning($"[SliderMultiplierSettings] 配置列表为空");
                return new MultiplierConfigSet();
            }

            if (index < 0 || index >= configs.Count)
            {
                Debug.LogWarning($"[SliderMultiplierSettings] 配置索引越界: {index}，返回第一个配置");
                return configs[0];
            }

            return configs[index];
        }

        /// <summary>
        /// 获取配置数量
        /// </summary>
        /// <param name="isWithdrawn">是否已提现</param>
        /// <returns>配置数量</returns>
        public int GetConfigCount(bool isWithdrawn)
        {
            var configs = isWithdrawn ? postWithdrawConfigs : preWithdrawConfigs;
            return configs?.Count ?? 0;
        }

        /// <summary>
        /// 获取重置时间
        /// </summary>
        /// <returns>重置时间的TimeSpan</returns>
        public TimeSpan GetResetTimeSpan()
        {
            if (string.IsNullOrEmpty(resetTime))
            {
                Debug.LogWarning($"[SliderMultiplierSettings] 重置时间未设置，使用默认值00:00:00");
                return TimeSpan.Zero;
            }

            if (TimeSpan.TryParse(resetTime, out TimeSpan result))
            {
                return result;
            }
            else
            {
                Debug.LogError($"[SliderMultiplierSettings] 重置时间格式错误: {resetTime}，使用默认值00:00:00");
                return TimeSpan.Zero;
            }
        }

        #region 编辑器辅助方法

#if UNITY_EDITOR
        /// <summary>
        /// 添加默认配置（编辑器用）
        /// </summary>
        [ContextMenu("添加默认配置")]
        public void AddDefaultConfigs()
        {
            // 清空现有配置
            preWithdrawConfigs.Clear();
            postWithdrawConfigs.Clear();

            // 添加提现前默认配置（较低倍率）
            var preConfig1 = new MultiplierConfigSet
            {
                multipliers = new float[] { 0.3f, 0.5f, 0.8f, 2f, 3f },
                weight = 40,
                stopCoefficient = 1.0f
            };
            preWithdrawConfigs.Add(preConfig1);

            var preConfig2 = new MultiplierConfigSet
            {
                multipliers = new float[] { 0.5f, 0.8f, 1f, 3f, 5f },
                weight = 30,
                stopCoefficient = 1.2f
            };
            preWithdrawConfigs.Add(preConfig2);

            var preConfig3 = new MultiplierConfigSet
            {
                multipliers = new float[] { 0.8f, 1f, 2f, 5f, 10f },
                weight = 20,
                stopCoefficient = 1.5f
            };
            preWithdrawConfigs.Add(preConfig3);

            var preConfig4 = new MultiplierConfigSet
            {
                multipliers = new float[] { 1f, 2f, 5f, 10f, 20f },
                weight = 10,
                stopCoefficient = 2.0f
            };
            preWithdrawConfigs.Add(preConfig4);

            // 添加提现后默认配置（更高倍率）
            var postConfig1 = new MultiplierConfigSet
            {
                multipliers = new float[] { 1f, 2f, 3f, 5f, 8f },
                weight = 40,
                stopCoefficient = 1.0f
            };
            postWithdrawConfigs.Add(postConfig1);

            var postConfig2 = new MultiplierConfigSet
            {
                multipliers = new float[] { 2f, 3f, 5f, 10f, 15f },
                weight = 30,
                stopCoefficient = 1.3f
            };
            postWithdrawConfigs.Add(postConfig2);

            var postConfig3 = new MultiplierConfigSet
            {
                multipliers = new float[] { 3f, 5f, 10f, 20f, 30f },
                weight = 20,
                stopCoefficient = 1.6f
            };
            postWithdrawConfigs.Add(postConfig3);

            var postConfig4 = new MultiplierConfigSet
            {
                multipliers = new float[] { 5f, 10f, 20f, 50f, 100f },
                weight = 10,
                stopCoefficient = 2.5f
            };
            postWithdrawConfigs.Add(postConfig4);

            Debug.Log($"[SliderMultiplierSettings] 默认配置已添加");

            UnityEditor.EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// 验证并修复配置（编辑器用）
        /// </summary>
        [ContextMenu("验证配置")]
        private void ValidateAndFix()
        {
            if (ValidateAll())
            {
                Debug.Log($"[SliderMultiplierSettings] 所有配置验证通过");
            }
            else
            {
                Debug.LogError($"[SliderMultiplierSettings] 配置存在问题，请检查并修复");
            }
        }
#endif

        #endregion
    }
}