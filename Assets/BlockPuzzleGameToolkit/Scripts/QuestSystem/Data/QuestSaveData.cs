// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

using System;
using System.Collections.Generic;
using UnityEngine;
using StorageSystem.Data;
using QuestSystem.Core;

namespace QuestSystem.Data
{
    /// <summary>
    /// 任务存储数据
    /// 继承自存储模块的SaveDataContainer基类
    /// </summary>
    [Serializable]
    public class QuestSaveData : SaveDataContainer
    {
        /// <summary>
        /// 单个任务的进度数据
        /// </summary>
        [Serializable]
        public class QuestProgress
        {
            public int configId;           // 任务配置ID
            public string instanceTag;     // 实例标签
            public int progress;           // 当前进度
            public bool completed;         // 是否完成
            public bool rewardClaimed;     // 是否已领取奖励
            public long startTime;         // 开始时间
            public long completeTime;      // 完成时间
            public long lastUpdateTime;    // 最后更新时间

            /// <summary>
            /// 默认构造函数
            /// </summary>
            public QuestProgress()
            {
                instanceTag = "";
                lastUpdateTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            }

            /// <summary>
            /// 从Quest对象创建
            /// </summary>
            public QuestProgress(Quest quest)
            {
                configId = quest.ConfigId;
                instanceTag = quest.InstanceTag;
                progress = quest.CurrentProgress;
                completed = quest.IsCompleted;
                rewardClaimed = quest.IsRewardClaimed;
                startTime = quest.StartTime.Ticks;
                completeTime = quest.CompleteTime.Ticks;
                lastUpdateTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            }

            /// <summary>
            /// 获取唯一键
            /// </summary>
            public string GetKey()
            {
                return $"{configId}_{instanceTag}";
            }
        }

        /// <summary>
        /// 全局统计数据
        /// </summary>
        [Serializable]
        public class GlobalStats
        {
            public int totalQuestsCompleted;    // 总完成任务数
            public int totalRewardsEarned;      // 总获得奖励
            public long firstQuestTime;         // 第一个任务时间
            public long lastQuestTime;          // 最后任务时间
            public int consecutiveDays;         // 连续天数

            public GlobalStats()
            {
                firstQuestTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                lastQuestTime = firstQuestTime;
            }
        }

        [SerializeField] private List<QuestProgress> questList = new List<QuestProgress>();
        [SerializeField] private GlobalStats globalStats = new GlobalStats();
        [SerializeField] private List<int> completedQuestIds = new List<int>(); // 已完成的任务ID列表（用于检查前置任务）

        #region 属性

        /// <summary>
        /// 任务进度列表
        /// </summary>
        public List<QuestProgress> QuestList => questList;

        /// <summary>
        /// 全局统计
        /// </summary>
        public GlobalStats Stats => globalStats;

        /// <summary>
        /// 已完成的任务ID列表
        /// </summary>
        public List<int> CompletedQuestIds => completedQuestIds;

        #endregion

        #region 构造函数

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public QuestSaveData()
        {
            questList = new List<QuestProgress>();
            globalStats = new GlobalStats();
            completedQuestIds = new List<int>();
        }

        #endregion

        #region 添加/更新任务

        /// <summary>
        /// 添加或更新任务进度
        /// </summary>
        public void AddOrUpdateQuest(Quest quest)
        {
            if (quest == null) return;

            var key = quest.InstanceKey;
            var existingIndex = questList.FindIndex(q => q.GetKey() == key);

            if (existingIndex >= 0)
            {
                // 更新现有任务
                questList[existingIndex] = new QuestProgress(quest);
            }
            else
            {
                // 添加新任务
                questList.Add(new QuestProgress(quest));
            }

            // 更新完成列表
            if (quest.IsCompleted && !completedQuestIds.Contains(quest.ConfigId))
            {
                completedQuestIds.Add(quest.ConfigId);
                globalStats.totalQuestsCompleted++;
            }

            // 更新全局统计
            globalStats.lastQuestTime = DateTimeOffset.Now.ToUnixTimeSeconds();
        }

        /// <summary>
        /// 批量添加任务
        /// </summary>
        public void AddQuests(IEnumerable<Quest> quests)
        {
            foreach (var quest in quests)
            {
                AddOrUpdateQuest(quest);
            }
        }

        #endregion

        #region 查询

        /// <summary>
        /// 获取任务进度
        /// </summary>
        public QuestProgress GetQuestProgress(int configId, string instanceTag)
        {
            var key = $"{configId}_{instanceTag}";
            return questList.Find(q => q.GetKey() == key);
        }

        /// <summary>
        /// 检查任务是否完成
        /// </summary>
        public bool IsQuestCompleted(int configId)
        {
            return completedQuestIds.Contains(configId);
        }

        /// <summary>
        /// 获取所有活跃任务（未完成）
        /// </summary>
        public List<QuestProgress> GetActiveQuests()
        {
            return questList.FindAll(q => !q.completed);
        }

        /// <summary>
        /// 获取所有完成的任务
        /// </summary>
        public List<QuestProgress> GetCompletedQuests()
        {
            return questList.FindAll(q => q.completed);
        }

        #endregion

        #region 移除

        /// <summary>
        /// 移除任务进度
        /// </summary>
        public bool RemoveQuest(int configId, string instanceTag)
        {
            var key = $"{configId}_{instanceTag}";
            return questList.RemoveAll(q => q.GetKey() == key) > 0;
        }

        /// <summary>
        /// 清理过期任务
        /// </summary>
        public void CleanupOldQuests(int daysToKeep = 30)
        {
            var cutoffTime = DateTimeOffset.Now.AddDays(-daysToKeep).ToUnixTimeSeconds();
            questList.RemoveAll(q => q.completed && q.completeTime > 0 && q.completeTime < cutoffTime);
        }

        #endregion

        #region 统计

        /// <summary>
        /// 更新统计数据
        /// </summary>
        public void UpdateStats(int rewardAmount)
        {
            globalStats.totalRewardsEarned += rewardAmount;
            globalStats.lastQuestTime = DateTimeOffset.Now.ToUnixTimeSeconds();

            // 检查连续天数
            var lastDate = DateTimeOffset.FromUnixTimeSeconds(globalStats.lastQuestTime).Date;
            var today = DateTime.Today;

            if ((today - lastDate).Days == 1)
            {
                globalStats.consecutiveDays++;
            }
            else if ((today - lastDate).Days > 1)
            {
                globalStats.consecutiveDays = 1;
            }
        }

        #endregion

        #region 数据迁移

        /// <summary>
        /// 从旧版本数据迁移
        /// </summary>
        public static QuestSaveData MigrateFromOldVersion(object oldData)
        {
            // 这里处理从旧版本数据的迁移
            // 例如，如果数据结构发生变化
            var newData = new QuestSaveData();

            // 迁移逻辑...

            return newData;
        }

        #endregion

        #region 验证

        /// <summary>
        /// 验证数据完整性
        /// </summary>
        public override bool IsValid()
        {
            if (!base.IsValid()) return false;

            // 检查数据一致性
            foreach (var quest in questList)
            {
                if (quest.configId <= 0)
                {
                    Debug.LogWarning($"[QuestSaveData] Invalid quest config ID: {quest.configId}");
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region 调试

        /// <summary>
        /// 获取调试信息
        /// </summary>
        public string GetDebugInfo()
        {
            return $"QuestSaveData: {questList.Count} quests, " +
                   $"{globalStats.totalQuestsCompleted} completed, " +
                   $"{globalStats.totalRewardsEarned} coins earned";
        }

        #endregion
    }
}