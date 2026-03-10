// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QuestSystem.Config
{
    /// <summary>
    /// 任务配置数据库
    /// ScriptableObject，集中管理所有任务配置
    /// </summary>
    [CreateAssetMenu(fileName = "QuestConfigDatabase", menuName = "Quest/ConfigDatabase", order = 1)]
    public class QuestConfigDatabase : ScriptableObject
    {
        [Header("任务配置列表")]
        [SerializeField] private List<QuestData> questConfigs = new List<QuestData>();

        // 快速查找字典（运行时构建）
        private Dictionary<int, QuestData> questLookup;
        private bool isInitialized = false;

        #region 初始化

        /// <summary>
        /// 初始化数据库
        /// </summary>
        public void Initialize()
        {
            if (isInitialized) return;

            questLookup = new Dictionary<int, QuestData>();
            foreach (var config in questConfigs)
            {
                if (config != null && config.IsValid())
                {
                    if (questLookup.ContainsKey(config.QuestId))
                    {
                        Debug.LogWarning($"[QuestConfigDatabase] Duplicate quest ID: {config.QuestId}");
                    }
                    else
                    {
                        questLookup[config.QuestId] = config;
                    }
                }
            }

            isInitialized = true;
            Debug.Log($"[QuestConfigDatabase] Initialized with {questLookup.Count} quests");
        }

        #endregion

        #region 查询接口

        /// <summary>
        /// 根据ID获取任务数据
        /// </summary>
        public QuestData GetQuestData(int questId)
        {
            if (!isInitialized) Initialize();

            if (questLookup.TryGetValue(questId, out var data))
            {
                return data;
            }

            Debug.LogWarning($"[QuestConfigDatabase] Quest ID {questId} not found");
            return null;
        }

        /// <summary>
        /// 获取所有任务配置
        /// </summary>
        public List<QuestData> GetAllQuests()
        {
            if (!isInitialized) Initialize();
            return new List<QuestData>(questConfigs);
        }

        /// <summary>
        /// 根据类型获取任务
        /// </summary>
        public List<QuestData> GetQuestsByType(QuestType type)
        {
            if (!isInitialized) Initialize();
            return questConfigs.Where(q => q != null && q.QuestType == type).ToList();
        }

        /// <summary>
        /// 根据标签获取任务
        /// </summary>
        public List<QuestData> GetQuestsByTag(string tag)
        {
            if (!isInitialized) Initialize();
            return questConfigs.Where(q => q != null && q.Tag == tag).ToList();
        }

        /// <summary>
        /// 获取可重复的任务
        /// </summary>
        public List<QuestData> GetRepeatableQuests()
        {
            if (!isInitialized) Initialize();
            return questConfigs.Where(q => q != null && q.IsRepeatable).ToList();
        }

        /// <summary>
        /// 检查任务是否存在
        /// </summary>
        public bool HasQuest(int questId)
        {
            if (!isInitialized) Initialize();
            return questLookup.ContainsKey(questId);
        }

        #endregion

        #region 验证

        /// <summary>
        /// 验证所有任务配置
        /// </summary>
        public void ValidateAllQuests()
        {
            int validCount = 0;
            int invalidCount = 0;
            HashSet<int> usedIds = new HashSet<int>();

            foreach (var quest in questConfigs)
            {
                if (quest == null)
                {
                    Debug.LogError("[QuestConfigDatabase] Null quest found in config");
                    invalidCount++;
                    continue;
                }

                if (!quest.IsValid())
                {
                    Debug.LogError($"[QuestConfigDatabase] Invalid quest: {quest.QuestName}");
                    invalidCount++;
                    continue;
                }

                if (usedIds.Contains(quest.QuestId))
                {
                    Debug.LogError($"[QuestConfigDatabase] Duplicate quest ID: {quest.QuestId}");
                    invalidCount++;
                }
                else
                {
                    usedIds.Add(quest.QuestId);
                    validCount++;
                }

                // 验证前置任务
                if (quest.HasPrerequisites())
                {
                    foreach (var prereqId in quest.PrerequisiteQuests)
                    {
                        if (!questConfigs.Any(q => q != null && q.QuestId == prereqId))
                        {
                            Debug.LogWarning($"[QuestConfigDatabase] Quest {quest.QuestId} has invalid prerequisite: {prereqId}");
                        }
                    }
                }
            }

            Debug.Log($"[QuestConfigDatabase] Validation complete: {validCount} valid, {invalidCount} invalid");
        }

        #endregion

        #region 编辑器辅助

#if UNITY_EDITOR
        /// <summary>
        /// 添加任务配置（仅编辑器）
        /// </summary>
        public void AddQuest(QuestData questData)
        {
            if (questData != null && !questConfigs.Contains(questData))
            {
                questConfigs.Add(questData);
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }

        /// <summary>
        /// 移除任务配置（仅编辑器）
        /// </summary>
        public void RemoveQuest(int questId)
        {
            var quest = questConfigs.FirstOrDefault(q => q != null && q.QuestId == questId);
            if (quest != null)
            {
                questConfigs.Remove(quest);
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }

        /// <summary>
        /// 清空所有任务（仅编辑器）
        /// </summary>
        public void ClearAllQuests()
        {
            questConfigs.Clear();
            UnityEditor.EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// 排序任务列表（仅编辑器）
        /// </summary>
        public void SortQuests()
        {
            questConfigs = questConfigs.Where(q => q != null).OrderBy(q => q.QuestId).ToList();
            UnityEditor.EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// 生成示例数据（仅编辑器）
        /// </summary>
        [ContextMenu("Generate Sample Data")]
        private void GenerateSampleData()
        {
            questConfigs.Clear();

            // 主线任务
            var mainQuest1 = new QuestData(1001, "初次胜利", "完成一局游戏", QuestType.PlayCount, 1, 50);
            questConfigs.Add(mainQuest1);

            var mainQuest2 = new QuestData(1002, "高分挑战", "单局获得5000分", QuestType.Score, 5000, 100);
            questConfigs.Add(mainQuest2);

            // 收集任务
            var collectQuest = new QuestData(2001, "收集大师", "收集10个特殊道具", QuestType.Collect, 10, 200);
            questConfigs.Add(collectQuest);

            // 消行任务
            var lineQuest = new QuestData(3001, "消行专家", "累计消除100行", QuestType.Lines, 100, 150);
            questConfigs.Add(lineQuest);

            // 连击任务
            var comboQuest = new QuestData(4001, "连击达人", "达成5连击", QuestType.Combo, 5, 300);
            questConfigs.Add(comboQuest);

            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log($"[QuestConfigDatabase] Generated {questConfigs.Count} sample quests");
        }
#endif

        #endregion

        #region 生命周期

        private void OnEnable()
        {
            isInitialized = false;
        }

        private void OnValidate()
        {
            // 编辑器中自动验证
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                ValidateAllQuests();
            }
#endif
        }

        #endregion
    }
}