// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

using System;
using UnityEngine;
using QuestSystem.Config;

namespace QuestSystem.Core
{
    /// <summary>
    /// 任务运行时类
    /// </summary>
    [Serializable]
    public class Quest
    {
        #region 私有字段

        [SerializeField] private int configId;           // 配置ID
        [SerializeField] private string instanceTag;     // 实例标签
        [SerializeField] private int currentProgress;    // 当前进度
        [SerializeField] private bool isCompleted;       // 是否已完成
        [SerializeField] private bool isRewardClaimed;   // 是否已领取奖励
        [SerializeField] private long startTime;         // 开始时间
        [SerializeField] private long completeTime;      // 完成时间

        private QuestData data;                          // 配置数据引用

        #endregion

        #region 属性

        /// <summary>
        /// 配置ID
        /// </summary>
        public int ConfigId => configId;

        /// <summary>
        /// 实例标签
        /// </summary>
        public string InstanceTag => instanceTag;

        /// <summary>
        /// 配置数据
        /// </summary>
        public QuestData Data => data;

        /// <summary>
        /// 当前进度
        /// </summary>
        public int CurrentProgress => currentProgress;

        /// <summary>
        /// 是否已完成
        /// </summary>
        public bool IsCompleted => isCompleted;

        /// <summary>
        /// 是否已领取奖励
        /// </summary>
        public bool IsRewardClaimed => isRewardClaimed;

        /// <summary>
        /// 进度百分比（0-1）
        /// </summary>
        public float ProgressPercentage => data != null ? Mathf.Clamp01((float)currentProgress / data.TargetValue) : 0f;

        /// <summary>
        /// 生成唯一实例键（用于存储和查找）
        /// </summary>
        public string InstanceKey => $"{configId}_{instanceTag}";

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime => DateTimeOffset.FromUnixTimeSeconds(startTime).DateTime;

        /// <summary>
        /// 完成时间
        /// </summary>
        public DateTime CompleteTime => completeTime > 0 ? DateTimeOffset.FromUnixTimeSeconds(completeTime).DateTime : DateTime.MinValue;

        #endregion

        #region 构造函数

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public Quest()
        {
            instanceTag = "";
            currentProgress = 0;
            isCompleted = false;
            isRewardClaimed = false;
        }

        /// <summary>
        /// 带参数的构造函数
        /// </summary>
        public Quest(int configId, string tag, QuestData questData)
        {
            Initialize(configId, tag, questData);
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化任务
        /// </summary>
        public void Initialize(int id, string tag, QuestData questData)
        {
            configId = id;
            instanceTag = tag ?? "";
            data = questData;
            currentProgress = 0;
            isCompleted = false;
            isRewardClaimed = false;
            startTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            completeTime = 0;

            if (data == null)
            {
                Debug.LogError($"[Quest] Failed to initialize quest {id}: QuestData is null");
            }
        }

        #endregion

        #region 进度管理

        /// <summary>
        /// 更新进度
        /// </summary>
        public virtual bool UpdateProgress(int value)
        {
            if (isCompleted || data == null) return false;

            int oldProgress = currentProgress;

            // 根据任务类型处理进度
            switch (data.QuestType)
            {
                case QuestType.Score:
                case QuestType.Combo:
                    // 分数和连击任务取最高值
                    currentProgress = Mathf.Max(currentProgress, value);
                    break;

                default:
                    // 其他任务累加进度
                    currentProgress = Mathf.Clamp(currentProgress + value, 0, data.TargetValue);
                    break;
            }

            // 检查是否完成
            if (currentProgress >= data.TargetValue)
            {
                Complete();
            }

            // 返回是否有进度变化
            return oldProgress != currentProgress;
        }

        /// <summary>
        /// 设置进度（用于加载存档）
        /// </summary>
        public void SetProgress(int progress)
        {
            currentProgress = Mathf.Clamp(progress, 0, data?.TargetValue ?? progress);
        }

        /// <summary>
        /// 设置完成状态（用于加载存档）
        /// </summary>
        public void SetCompleted(bool completed)
        {
            isCompleted = completed;
            if (completed && completeTime == 0)
            {
                completeTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            }
        }

        /// <summary>
        /// 设置奖励领取状态
        /// </summary>
        public void SetRewardClaimed(bool claimed)
        {
            isRewardClaimed = claimed;
        }

        #endregion

        #region 任务完成

        /// <summary>
        /// 完成任务
        /// </summary>
        protected virtual void Complete()
        {
            if (isCompleted) return;

            isCompleted = true;
            completeTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            currentProgress = data.TargetValue;

            Debug.Log($"[Quest] Quest completed: {data.QuestName} ({InstanceKey})");
        }

        /// <summary>
        /// 领取奖励
        /// </summary>
        public virtual bool ClaimReward()
        {
            if (!isCompleted || isRewardClaimed || data == null) return false;

            isRewardClaimed = true;
            Debug.Log($"[Quest] Reward claimed for quest: {data.QuestName}, Coins: {data.RewardCoins}");
            return true;
        }

        #endregion

        #region 查询方法

        /// <summary>
        /// 获取进度文本
        /// </summary>
        public string GetProgressText()
        {
            if (data == null) return "";
            return data.GetProgressText(currentProgress, data.TargetValue);
        }

        /// <summary>
        /// 是否可以开始（检查前置任务等条件）
        /// </summary>
        public bool CanStart(Func<int, bool> checkPrerequisite)
        {
            if (data == null) return false;

            // 检查前置任务
            if (data.HasPrerequisites())
            {
                foreach (var prereqId in data.PrerequisiteQuests)
                {
                    if (!checkPrerequisite(prereqId))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 获取剩余进度
        /// </summary>
        public int GetRemainingProgress()
        {
            if (data == null) return 0;
            return Mathf.Max(0, data.TargetValue - currentProgress);
        }

        /// <summary>
        /// 获取任务持续时间
        /// </summary>
        public TimeSpan GetDuration()
        {
            if (startTime == 0) return TimeSpan.Zero;

            if (completeTime > 0)
            {
                return TimeSpan.FromSeconds(completeTime - startTime);
            }
            else
            {
                return DateTime.Now - StartTime;
            }
        }

        #endregion

        #region 重置

        /// <summary>
        /// 重置任务（用于可重复任务）
        /// </summary>
        public virtual void Reset()
        {
            if (data != null && !data.IsRepeatable)
            {
                Debug.LogWarning($"[Quest] Quest {data.QuestName} is not repeatable");
                return;
            }

            currentProgress = 0;
            isCompleted = false;
            isRewardClaimed = false;
            startTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            completeTime = 0;

            Debug.Log($"[Quest] Quest reset: {data?.QuestName} ({InstanceKey})");
        }

        #endregion

        #region 调试

        public override string ToString()
        {
            return $"Quest[{InstanceKey}]: {data?.QuestName ?? "Unknown"} - Progress: {currentProgress}/{data?.TargetValue ?? 0} ({ProgressPercentage:P0})";
        }

        #endregion
    }
}