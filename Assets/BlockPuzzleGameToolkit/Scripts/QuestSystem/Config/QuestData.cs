// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

using System;
using UnityEngine;

namespace QuestSystem.Config
{
    /// <summary>
    /// 任务类型枚举
    /// </summary>
    public enum QuestType
    {
        Collect,      // 收集类
        Score,        // 分数类
        Lines,        // 消行类
        PlayCount,    // 游戏次数
        Combo,        // 连击类
        Perfect,      // 完美通关
        Custom        // 自定义类型
    }

    /// <summary>
    /// 任务配置数据
    /// </summary>
    [Serializable]
    public class QuestData
    {
        [Header("基础信息")]
        [SerializeField] private int questId;              // 任务ID（唯一标识）
        [SerializeField] private string questName;         // 任务名称
        [SerializeField] private string description;       // 任务描述
        [SerializeField] private QuestType questType;      // 任务类型

        [Header("目标与奖励")]
        [SerializeField] private int targetValue;          // 目标值
        [SerializeField] private int rewardCoins;          // 奖励金币

        [Header("多语言")]
        [SerializeField] private string progressTextKey;   // 进度文本多语言Key
        // 示例: "quest_progress_collect" -> "{0}/{1} 个红包已收集"

        [Header("扩展数据")]
        [SerializeField] private string tag;               // 标签（用于分组）
        [SerializeField] private string targetItemId;      // 收集任务的目标物品ID
        [SerializeField] private bool isRepeatable;        // 是否可重复
        [SerializeField] private int[] prerequisiteQuests; // 前置任务ID列表

        #region 属性访问器

        /// <summary>
        /// 任务ID
        /// </summary>
        public int QuestId => questId;

        /// <summary>
        /// 任务名称
        /// </summary>
        public string QuestName => questName;

        /// <summary>
        /// 任务描述
        /// </summary>
        public string Description => description;

        /// <summary>
        /// 任务类型
        /// </summary>
        public QuestType QuestType => questType;

        /// <summary>
        /// 目标值
        /// </summary>
        public int TargetValue => targetValue;

        /// <summary>
        /// 奖励金币
        /// </summary>
        public int RewardCoins => rewardCoins;

        /// <summary>
        /// 进度文本多语言Key
        /// </summary>
        public string ProgressTextKey => progressTextKey;

        /// <summary>
        /// 标签
        /// </summary>
        public string Tag => tag;

        /// <summary>
        /// 目标物品ID
        /// </summary>
        public string TargetItemId => targetItemId;

        /// <summary>
        /// 是否可重复
        /// </summary>
        public bool IsRepeatable => isRepeatable;

        /// <summary>
        /// 前置任务列表
        /// </summary>
        public int[] PrerequisiteQuests => prerequisiteQuests;

        #endregion

        #region 构造函数

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public QuestData()
        {
            prerequisiteQuests = new int[0];
        }

        /// <summary>
        /// 带参数的构造函数（用于代码创建）
        /// </summary>
        public QuestData(int id, string name, string desc, QuestType type, int target, int reward)
        {
            questId = id;
            questName = name;
            description = desc;
            questType = type;
            targetValue = target;
            rewardCoins = reward;
            prerequisiteQuests = new int[0];
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 验证配置是否有效
        /// </summary>
        public bool IsValid()
        {
            if (questId <= 0)
            {
                Debug.LogError($"Quest ID must be greater than 0");
                return false;
            }

            if (string.IsNullOrEmpty(questName))
            {
                Debug.LogError($"Quest {questId} has no name");
                return false;
            }

            if (targetValue <= 0)
            {
                Debug.LogError($"Quest {questId} target value must be greater than 0");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 检查前置任务
        /// </summary>
        public bool HasPrerequisites()
        {
            return prerequisiteQuests != null && prerequisiteQuests.Length > 0;
        }

        /// <summary>
        /// 获取格式化的进度文本
        /// </summary>
        public string GetProgressText(int current, int max)
        {
            if (!string.IsNullOrEmpty(progressTextKey))
            {
                // 这里应该调用多语言系统
                // return Localization.GetText(progressTextKey, current, max);
                return $"{current}/{max}"; // 临时实现
            }

            // 默认格式
            switch (questType)
            {
                case QuestType.Collect:
                    return $"收集: {current}/{max}";
                case QuestType.Score:
                    return $"分数: {current}/{max}";
                case QuestType.Lines:
                    return $"消行: {current}/{max}";
                case QuestType.PlayCount:
                    return $"游戏次数: {current}/{max}";
                case QuestType.Combo:
                    return $"连击: {current}/{max}";
                case QuestType.Perfect:
                    return $"完美通关: {current}/{max}";
                default:
                    return $"{current}/{max}";
            }
        }

        #endregion

        #region 编辑器辅助

#if UNITY_EDITOR
        /// <summary>
        /// 在编辑器中设置值（仅编辑器使用）
        /// </summary>
        public void SetValuesInEditor(int id, string name, string desc, QuestType type, int target, int reward)
        {
            questId = id;
            questName = name;
            description = desc;
            questType = type;
            targetValue = target;
            rewardCoins = reward;
        }
#endif

        #endregion
    }
}