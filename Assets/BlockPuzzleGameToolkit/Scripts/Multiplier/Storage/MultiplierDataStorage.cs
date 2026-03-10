// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

using System;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Multiplier.Storage
{
    /// <summary>
    /// 滑动倍率模块数据存储管理
    /// 负责所有持久化数据的存取
    /// </summary>
    public static class MultiplierDataStorage
    {
        // PlayerPrefs键名常量
        private const string KEY_PREFIX = "Multiplier_";
        private const string KEY_PRE_INDEX = KEY_PREFIX + "PreWithdrawIndex";
        private const string KEY_POST_INDEX = KEY_PREFIX + "PostWithdrawIndex";
        private const string KEY_HAS_WITHDRAWN = KEY_PREFIX + "HasWithdrawn";
        private const string KEY_LAST_RESET_DATE = KEY_PREFIX + "LastResetDate";
        private const string KEY_LAST_WITHDRAW_DATE = KEY_PREFIX + "LastWithdrawDate";
        private const string KEY_CURRENT_CONFIG_INDEX = KEY_PREFIX + "CurrentConfigIndex";
        private const string KEY_IS_INITIALIZED = KEY_PREFIX + "IsInitialized";

        /// <summary>
        /// 保存配置索引
        /// </summary>
        /// <param name="isWithdraw">是否为提现后配置</param>
        /// <param name="index">配置索引</param>
        public static void SaveConfigIndex(bool isWithdraw, int index)
        {
            string key = isWithdraw ? KEY_POST_INDEX : KEY_PRE_INDEX;
            PlayerPrefs.SetInt(key, index);
            PlayerPrefs.Save();

            Debug.Log($"[MultiplierDataStorage] 保存配置索引: {key} = {index}");
        }

        /// <summary>
        /// 加载配置索引
        /// </summary>
        /// <param name="isWithdraw">是否为提现后配置</param>
        /// <returns>配置索引</returns>
        public static int LoadConfigIndex(bool isWithdraw)
        {
            string key = isWithdraw ? KEY_POST_INDEX : KEY_PRE_INDEX;
            int index = PlayerPrefs.GetInt(key, 0);

            Debug.Log($"[MultiplierDataStorage] 加载配置索引: {key} = {index}");
            return index;
        }

        /// <summary>
        /// 保存当前使用的配置索引（通用）
        /// </summary>
        public static void SaveCurrentConfigIndex(int index)
        {
            PlayerPrefs.SetInt(KEY_CURRENT_CONFIG_INDEX, index);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 加载当前使用的配置索引
        /// </summary>
        public static int LoadCurrentConfigIndex()
        {
            return PlayerPrefs.GetInt(KEY_CURRENT_CONFIG_INDEX, 0);
        }

        /// <summary>
        /// 保存提现状态
        /// </summary>
        /// <param name="hasWithdrawn">是否已提现</param>
        public static void SaveWithdrawStatus(bool hasWithdrawn)
        {
            PlayerPrefs.SetInt(KEY_HAS_WITHDRAWN, hasWithdrawn ? 1 : 0);

            if (hasWithdrawn)
            {
                // 记录提现时间
                SaveLastWithdrawDate(DateTime.Now);
            }

            PlayerPrefs.Save();
            Debug.Log($"[MultiplierDataStorage] 保存提现状态: {hasWithdrawn}");
        }

        /// <summary>
        /// 加载提现状态
        /// </summary>
        /// <returns>是否已提现</returns>
        public static bool LoadWithdrawStatus()
        {
            bool hasWithdrawn = PlayerPrefs.GetInt(KEY_HAS_WITHDRAWN, 0) == 1;
            Debug.Log($"[MultiplierDataStorage] 加载提现状态: {hasWithdrawn}");
            return hasWithdrawn;
        }

        /// <summary>
        /// 保存上次重置日期
        /// </summary>
        /// <param name="date">重置日期</param>
        public static void SaveLastResetDate(DateTime date)
        {
            string dateStr = date.ToString("yyyy-MM-dd");
            PlayerPrefs.SetString(KEY_LAST_RESET_DATE, dateStr);
            PlayerPrefs.Save();

            Debug.Log($"[MultiplierDataStorage] 保存重置日期: {dateStr}");
        }

        /// <summary>
        /// 加载上次重置日期
        /// </summary>
        /// <returns>上次重置的日期，如果没有记录返回默认值</returns>
        public static DateTime LoadLastResetDate()
        {
            string dateStr = PlayerPrefs.GetString(KEY_LAST_RESET_DATE, "");

            if (string.IsNullOrEmpty(dateStr))
            {
                // 如果没有记录，返回昨天的日期
                return DateTime.Now.AddDays(-1);
            }

            if (DateTime.TryParse(dateStr, out DateTime date))
            {
                return date;
            }

            // 解析失败返回昨天
            Debug.LogWarning($"[MultiplierDataStorage] 解析重置日期失败: {dateStr}");
            return DateTime.Now.AddDays(-1);
        }

        /// <summary>
        /// 保存上次提现日期
        /// </summary>
        private static void SaveLastWithdrawDate(DateTime date)
        {
            string dateStr = date.ToString("yyyy-MM-dd HH:mm:ss");
            PlayerPrefs.SetString(KEY_LAST_WITHDRAW_DATE, dateStr);
            PlayerPrefs.Save();

            Debug.Log($"[MultiplierDataStorage] 保存提现时间: {dateStr}");
        }

        /// <summary>
        /// 加载上次提现日期
        /// </summary>
        public static DateTime? LoadLastWithdrawDate()
        {
            string dateStr = PlayerPrefs.GetString(KEY_LAST_WITHDRAW_DATE, "");

            if (string.IsNullOrEmpty(dateStr))
            {
                return null;
            }

            if (DateTime.TryParse(dateStr, out DateTime date))
            {
                return date;
            }

            return null;
        }

        /// <summary>
        /// 检查是否需要每日重置
        /// </summary>
        /// <param name="resetTimeSpan">重置时间点</param>
        /// <returns>是否需要重置</returns>
        public static bool ShouldDailyReset(TimeSpan resetTimeSpan)
        {
            DateTime lastResetDate = LoadLastResetDate();
            DateTime now = DateTime.Now;
            DateTime todayResetTime = now.Date + resetTimeSpan;

            // 如果当前时间已过今天的重置时间点
            if (now >= todayResetTime)
            {
                // 检查上次重置是否在今天的重置时间点之前
                if (lastResetDate < todayResetTime)
                {
                    return true;
                }
            }
            else
            {
                // 如果当前时间还未到今天的重置时间点
                // 检查上次重置是否在昨天的重置时间点之前
                DateTime yesterdayResetTime = now.Date.AddDays(-1) + resetTimeSpan;
                if (lastResetDate < yesterdayResetTime)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 重置配置索引
        /// </summary>
        /// <param name="resetAll">是否重置所有配置（true：重置所有，false：仅重置提现后配置）</param>
        public static void ResetConfigIndexes(bool resetAll = true)
        {
            if (resetAll)
            {
                // 重置所有配置索引
                SaveConfigIndex(false, 0);  // 提现前
                SaveConfigIndex(true, 0);   // 提现后
                Debug.Log("[MultiplierDataStorage] 重置所有配置索引");
            }
            else
            {
                // 仅重置提现后配置
                SaveConfigIndex(true, 0);
                Debug.Log("[MultiplierDataStorage] 重置提现后配置索引");
            }

            // 更新重置时间
            SaveLastResetDate(DateTime.Now);
        }

        /// <summary>
        /// 标记为已初始化
        /// </summary>
        public static void MarkAsInitialized()
        {
            PlayerPrefs.SetInt(KEY_IS_INITIALIZED, 1);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 检查是否已初始化
        /// </summary>
        public static bool IsInitialized()
        {
            return PlayerPrefs.GetInt(KEY_IS_INITIALIZED, 0) == 1;
        }

        /// <summary>
        /// 清除所有倍率模块数据
        /// </summary>
        public static void ClearAllData()
        {
            // 删除所有相关键
            PlayerPrefs.DeleteKey(KEY_PRE_INDEX);
            PlayerPrefs.DeleteKey(KEY_POST_INDEX);
            PlayerPrefs.DeleteKey(KEY_HAS_WITHDRAWN);
            PlayerPrefs.DeleteKey(KEY_LAST_RESET_DATE);
            PlayerPrefs.DeleteKey(KEY_LAST_WITHDRAW_DATE);
            PlayerPrefs.DeleteKey(KEY_CURRENT_CONFIG_INDEX);
            PlayerPrefs.DeleteKey(KEY_IS_INITIALIZED);

            PlayerPrefs.Save();

            Debug.Log("[MultiplierDataStorage] 清除所有倍率模块数据");
        }

        /// <summary>
        /// 调试输出当前所有存储数据
        /// </summary>
        public static void DebugPrintAllData()
        {
            Debug.Log("========== Multiplier Storage Data ==========");
            Debug.Log($"PreWithdraw Index: {LoadConfigIndex(false)}");
            Debug.Log($"PostWithdraw Index: {LoadConfigIndex(true)}");
            Debug.Log($"Has Withdrawn: {LoadWithdrawStatus()}");
            Debug.Log($"Last Reset Date: {LoadLastResetDate()}");
            Debug.Log($"Last Withdraw Date: {LoadLastWithdrawDate()?.ToString() ?? "Never"}");
            Debug.Log($"Current Config Index: {LoadCurrentConfigIndex()}");
            Debug.Log($"Is Initialized: {IsInitialized()}");
            Debug.Log("=============================================");
        }
    }
}