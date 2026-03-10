// 漂浮泡泡活动 - 持久化数据
// 创建日期: 2026-03-09

using System;
using StorageSystem.Core;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Activity.Examples
{
    /// <summary>
    /// 漂浮泡泡活动持久化数据
    /// 使用StorageManager进行存储
    /// </summary>
    [Serializable]
    public class FloatingBubbleData
    {
        #region 存储Key

        private const string STORAGE_KEY = "FloatingBubbleData";

        #endregion

        #region 数据字段

        /// <summary>
        /// 是否已解锁
        /// </summary>
        public bool isUnlocked;

        /// <summary>
        /// 冷却结束时间戳（UTC Ticks）
        /// </summary>
        public long cooldownEndTime;

        /// <summary>
        /// 累计领取次数
        /// </summary>
        public int claimCount;

        #endregion

        #region 存储管理

        /// <summary>
        /// 加载数据
        /// </summary>
        public static FloatingBubbleData Load()
        {
            var storage = StorageManager.Instance;
            if (storage == null)
            {
                Debug.LogWarning("[FloatingBubbleData] StorageManager未找到，使用默认数据");
                return CreateDefault();
            }

            // 从PlayerPrefs加载JSON
            string json = PlayerPrefs.GetString(STORAGE_KEY, string.Empty);

            if (string.IsNullOrEmpty(json))
            {
                // 首次运行，创建默认数据
                return CreateDefault();
            }

            try
            {
                // 反序列化JSON
                var data = JsonUtility.FromJson<FloatingBubbleData>(json);
                if (data == null)
                {
                    Debug.LogWarning("[FloatingBubbleData] 反序列化失败，使用默认数据");
                    return CreateDefault();
                }

                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"[FloatingBubbleData] 加载数据失败: {e.Message}");
                return CreateDefault();
            }
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        public void Save()
        {
            var storage = StorageManager.Instance;
            if (storage == null)
            {
                Debug.LogWarning("[FloatingBubbleData] StorageManager未找到，无法保存");
                return;
            }

            try
            {
                // 序列化为JSON
                string json = JsonUtility.ToJson(this, prettyPrint: false);

                // 保存到PlayerPrefs
                PlayerPrefs.SetString(STORAGE_KEY, json);
                PlayerPrefs.Save();
            }
            catch (Exception e)
            {
                Debug.LogError($"[FloatingBubbleData] 保存数据失败: {e.Message}");
            }
        }

        /// <summary>
        /// 创建默认数据
        /// </summary>
        private static FloatingBubbleData CreateDefault()
        {
            return new FloatingBubbleData
            {
                isUnlocked = false,
                cooldownEndTime = 0,
                claimCount = 0
            };
        }

        #endregion

        #region 业务方法

        /// <summary>
        /// 检查是否在冷却中
        /// </summary>
        public bool IsInCooldown()
        {
            if (cooldownEndTime <= 0)
            {
                return false;
            }

            long nowTicks = DateTime.UtcNow.Ticks;
            return nowTicks < cooldownEndTime;
        }

        /// <summary>
        /// 获取剩余冷却时间（秒）
        /// </summary>
        public float GetRemainingCooldown()
        {
            if (!IsInCooldown())
            {
                return 0f;
            }

            long nowTicks = DateTime.UtcNow.Ticks;
            long remainingTicks = cooldownEndTime - nowTicks;
            return (float)TimeSpan.FromTicks(remainingTicks).TotalSeconds;
        }

        /// <summary>
        /// 设置冷却时间
        /// </summary>
        /// <param name="durationSeconds">冷却持续时间（秒）</param>
        public void SetCooldown(float durationSeconds)
        {
            long nowTicks = DateTime.UtcNow.Ticks;
            long durationTicks = (long)(durationSeconds * TimeSpan.TicksPerSecond);
            cooldownEndTime = nowTicks + durationTicks;
        }

        /// <summary>
        /// 清除冷却
        /// </summary>
        public void ClearCooldown()
        {
            cooldownEndTime = 0;
        }

        /// <summary>
        /// 解锁活动
        /// </summary>
        public void Unlock()
        {
            isUnlocked = true;
        }

        /// <summary>
        /// 增加领取次数
        /// </summary>
        public void IncrementClaimCount()
        {
            claimCount++;
        }

        #endregion

        #region Debug

        /// <summary>
        /// 获取调试信息
        /// </summary>
        public string GetDebugInfo()
        {
            return $"FloatingBubbleData[Unlocked={isUnlocked}, InCooldown={IsInCooldown()}, " +
                   $"RemainingCooldown={GetRemainingCooldown():F1}s, ClaimCount={claimCount}]";
        }

        #endregion
    }
}
