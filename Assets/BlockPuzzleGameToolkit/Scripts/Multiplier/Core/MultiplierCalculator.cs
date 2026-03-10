// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

using System;
using UnityEngine;
using BlockPuzzleGameToolkit.Scripts.Settings;

namespace BlockPuzzleGameToolkit.Scripts.Multiplier.Core
{
    /// <summary>
    /// 滑动倍率计算器
    /// 提供所有倍率相关的计算逻辑
    /// </summary>
    public static class MultiplierCalculator
    {
        #region Core Calculations

        /// <summary>
        /// 根据区域索引获取倍率
        /// </summary>
        /// <param name="zoneIndex">区域索引（0-4）</param>
        /// <param name="config">配置集</param>
        /// <returns>对应的倍率值</returns>
        public static int GetMultiplierByZone(int zoneIndex, SliderMultiplierSettings.MultiplierConfigSet config)
        {
            if (config == null || config.multipliers == null)
            {
                Debug.LogError("[MultiplierCalculator] 配置为空");
                return 1;
            }

            // 边界检查
            if (zoneIndex < 0)
            {
                zoneIndex = 0;
            }
            else if (zoneIndex >= config.multipliers.Length)
            {
                zoneIndex = config.multipliers.Length - 1;
            }

            return Mathf.RoundToInt(config.multipliers[zoneIndex]);
        }

        /// <summary>
        /// 计算最终奖励
        /// </summary>
        /// <param name="baseReward">基础奖励</param>
        /// <param name="multiplier">倍率</param>
        /// <returns>最终奖励</returns>
        public static int CalculateFinalReward(int baseReward, int multiplier)
        {
            if (multiplier <= 0)
            {
                Debug.LogWarning($"[MultiplierCalculator] 倍率无效: {multiplier}，使用默认值1");
                multiplier = 1;
            }

            long result = (long)baseReward * multiplier;

            // 检查溢出
            if (result > int.MaxValue)
            {
                Debug.LogWarning($"[MultiplierCalculator] 奖励计算溢出: {baseReward} * {multiplier}");
                return int.MaxValue;
            }

            return (int)result;
        }

        /// <summary>
        /// 计算最终奖励（浮点数版本，用于货币等）
        /// </summary>
        public static float CalculateFinalReward(float baseReward, int multiplier)
        {
            if (multiplier <= 0)
            {
                multiplier = 1;
            }

            return baseReward * multiplier;
        }

        #endregion

        #region Zone Analysis

        /// <summary>
        /// 根据X坐标和区域边界获取区域索引
        /// </summary>
        /// <param name="xPosition">X坐标</param>
        /// <param name="zoneBounds">区域边界数组</param>
        /// <returns>区域索引</returns>
        public static int GetZoneIndexByPosition(float xPosition, UI.MultiplierSliderUI.ZoneBounds[] zoneBounds)
        {
            if (zoneBounds == null || zoneBounds.Length == 0)
            {
                Debug.LogError("[MultiplierCalculator] 区域边界未配置");
                return 2; // 返回中间区域
            }

            // 查找包含该位置的区域
            for (int i = 0; i < zoneBounds.Length; i++)
            {
                if (zoneBounds[i].ContainsX(xPosition))
                {
                    return i;
                }
            }

            // 边界处理
            if (xPosition < zoneBounds[0].minX)
                return 0;

            if (xPosition > zoneBounds[zoneBounds.Length - 1].maxX)
                return zoneBounds.Length - 1;

            // 如果位置在区域之间的间隙，找最近的区域
            return FindNearestZone(xPosition, zoneBounds);
        }

        /// <summary>
        /// 找到离指定位置最近的区域
        /// </summary>
        private static int FindNearestZone(float xPosition, UI.MultiplierSliderUI.ZoneBounds[] zoneBounds)
        {
            int nearestIndex = 0;
            float minDistance = float.MaxValue;

            for (int i = 0; i < zoneBounds.Length; i++)
            {
                float centerX = zoneBounds[i].GetCenterX();
                float distance = Mathf.Abs(xPosition - centerX);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestIndex = i;
                }
            }

            return nearestIndex;
        }

        #endregion

        #region Statistics

        /// <summary>
        /// 获取配置的平均倍率
        /// </summary>
        public static float GetAverageMultiplier(SliderMultiplierSettings.MultiplierConfigSet config)
        {
            if (config == null || config.multipliers == null || config.multipliers.Length == 0)
            {
                return 1f;
            }

            float sum = 0;
            foreach (float multiplier in config.multipliers)
            {
                sum += multiplier;
            }

            return sum / config.multipliers.Length;
        }

        /// <summary>
        /// 获取配置的最大倍率
        /// </summary>
        public static int GetMaxMultiplier(SliderMultiplierSettings.MultiplierConfigSet config)
        {
            if (config == null || config.multipliers == null || config.multipliers.Length == 0)
            {
                return 1;
            }

            int max = Mathf.RoundToInt(config.multipliers[0]);
            for (int i = 1; i < config.multipliers.Length; i++)
            {
                if (config.multipliers[i] > max)
                {
                    max = Mathf.RoundToInt(config.multipliers[i]);
                }
            }

            return max;
        }

        /// <summary>
        /// 获取配置的最小倍率
        /// </summary>
        public static int GetMinMultiplier(SliderMultiplierSettings.MultiplierConfigSet config)
        {
            if (config == null || config.multipliers == null || config.multipliers.Length == 0)
            {
                return 1;
            }

            int min = Mathf.RoundToInt(config.multipliers[0]);
            for (int i = 1; i < config.multipliers.Length; i++)
            {
                if (config.multipliers[i] < min)
                {
                    min = Mathf.RoundToInt(config.multipliers[i]);
                }
            }

            return min;
        }

        /// <summary>
        /// 计算获得特定倍率的概率（基于区域大小）
        /// </summary>
        public static float CalculateMultiplierProbability(int targetMultiplier,
            SliderMultiplierSettings.MultiplierConfigSet config,
            UI.MultiplierSliderUI.ZoneBounds[] zoneBounds)
        {
            if (config == null || zoneBounds == null)
            {
                return 0f;
            }

            float totalWidth = 0f;
            float targetWidth = 0f;

            for (int i = 0; i < config.multipliers.Length && i < zoneBounds.Length; i++)
            {
                float width = zoneBounds[i].GetWidth();
                totalWidth += width;

                if (Mathf.RoundToInt(config.multipliers[i]) == targetMultiplier)
                {
                    targetWidth += width;
                }
            }

            if (totalWidth <= 0)
            {
                return 0f;
            }

            return targetWidth / totalWidth;
        }

        #endregion

        #region Utility

        /// <summary>
        /// 验证倍率值是否有效
        /// </summary>
        public static bool IsValidMultiplier(int multiplier)
        {
            return multiplier > 0 && multiplier <= 100; // 假设最大倍率不超过100
        }

        /// <summary>
        /// 格式化倍率显示
        /// </summary>
        public static string FormatMultiplier(int multiplier)
        {
            return $"x{multiplier}";
        }

        /// <summary>
        /// 格式化奖励显示
        /// </summary>
        public static string FormatReward(int reward)
        {
            if (reward >= 1000000)
            {
                return $"{reward / 1000000f:F1}M";
            }
            else if (reward >= 1000)
            {
                return $"{reward / 1000f:F1}K";
            }
            else
            {
                return reward.ToString();
            }
        }

        /// <summary>
        /// 插值计算倍率（用于动画）
        /// </summary>
        public static float InterpolateMultiplier(float t, int fromMultiplier, int toMultiplier)
        {
            return Mathf.Lerp(fromMultiplier, toMultiplier, t);
        }

        /// <summary>
        /// 根据难度调整倍率
        /// </summary>
        public static int AdjustMultiplierByDifficulty(int baseMultiplier, float difficultyFactor)
        {
            if (difficultyFactor <= 0)
            {
                difficultyFactor = 1f;
            }

            int adjusted = Mathf.RoundToInt(baseMultiplier / difficultyFactor);
            return Mathf.Max(1, adjusted); // 确保至少为1
        }

        #endregion

        #region Debug Helpers

        /// <summary>
        /// 调试：输出配置统计信息
        /// </summary>
        public static void DebugPrintConfigStats(SliderMultiplierSettings.MultiplierConfigSet config)
        {
            if (config == null)
            {
                Debug.Log("[MultiplierCalculator] 配置为空");
                return;
            }

            Debug.Log("========== Config Statistics ==========");
            Debug.Log($"Multipliers: {string.Join(", ", config.multipliers)}");
            Debug.Log($"Average: {GetAverageMultiplier(config):F2}");
            Debug.Log($"Max: {GetMaxMultiplier(config)}");
            Debug.Log($"Min: {GetMinMultiplier(config)}");
            Debug.Log("=======================================");
        }

        /// <summary>
        /// 调试：模拟多次滑动并统计结果
        /// </summary>
        public static void SimulateSlides(int count, UI.MultiplierSliderUI.ZoneBounds[] zoneBounds,
            SliderMultiplierSettings.MultiplierConfigSet config)
        {
            if (config == null || zoneBounds == null)
            {
                Debug.LogError("[MultiplierCalculator] 无法进行模拟，配置或边界为空");
                return;
            }

            int[] zoneHits = new int[5];
            int[] multiplierCounts = new int[11]; // 假设倍率不超过10

            var random = new global::System.Random();

            // 获取总范围
            float minX = zoneBounds[0].minX;
            float maxX = zoneBounds[^1].maxX;
            float range = maxX - minX;

            // 模拟滑动
            for (int i = 0; i < count; i++)
            {
                float randomX = minX + (float)random.NextDouble() * range;
                int zone = GetZoneIndexByPosition(randomX, zoneBounds);
                zoneHits[zone]++;

                int multiplier = GetMultiplierByZone(zone, config);
                if (multiplier >= 0 && multiplier < multiplierCounts.Length)
                {
                    multiplierCounts[multiplier]++;
                }
            }

            // 输出统计结果
            Debug.Log($"========== Simulation Results ({count} slides) ==========");
            for (int i = 0; i < zoneHits.Length; i++)
            {
                float percentage = (float)zoneHits[i] / count * 100f;
                Debug.Log($"Zone {i}: {zoneHits[i]} hits ({percentage:F1}%)");
            }

            Debug.Log("--- Multiplier Distribution ---");
            for (int i = 1; i < multiplierCounts.Length; i++)
            {
                if (multiplierCounts[i] > 0)
                {
                    float percentage = (float)multiplierCounts[i] / count * 100f;
                    Debug.Log($"x{i}: {multiplierCounts[i]} times ({percentage:F1}%)");
                }
            }
            Debug.Log("================================================");
        }

        #endregion
    }
}