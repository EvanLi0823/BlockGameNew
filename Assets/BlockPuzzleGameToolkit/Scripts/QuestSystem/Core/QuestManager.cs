// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using StorageSystem.Core;
using QuestSystem.Config;
using QuestSystem.Data;

namespace QuestSystem.Core
{
    /// <summary>
    /// 任务管理器（单例）
    /// 负责任务的生命周期管理和外部接口
    /// </summary>
    public class QuestManager : SingletonBehaviour<QuestManager>
    {
        #region 常量

        private const string QUEST_SAVE_KEY = "quest_save_data";
        private const float AUTO_SAVE_INTERVAL = 30f; // 自动保存间隔（秒）

        #endregion

        #region 字段

        [Header("配置")]
        [SerializeField] private QuestConfigDatabase questConfigDatabase;
        [SerializeField] private bool enableAutoSave = false;
        [SerializeField] private bool enableDebugLog = false;

        // 任务实例管理
        private Dictionary<string, Quest> activeQuests = new Dictionary<string, Quest>();

        // 进度回调管理
        private Dictionary<string, Action<Quest, float>> progressCallbacks = new Dictionary<string, Action<Quest, float>>();

        // 完成回调管理
        private Dictionary<string, Action<Quest>> completionCallbacks = new Dictionary<string, Action<Quest>>();

        // 存储数据
        private QuestSaveData saveData;

        // 脏标记
        private bool isDirty = false;

        // 协程引用
        private Coroutine autoSaveCoroutine;

        #endregion

        #region 事件

        /// <summary>
        /// 任务创建事件
        /// </summary>
        public static event Action<Quest> OnQuestCreated;

        /// <summary>
        /// 任务进度更新事件
        /// </summary>
        public static event Action<Quest, float> OnQuestProgressUpdated;

        /// <summary>
        /// 任务完成事件
        /// </summary>
        public static event Action<Quest> OnQuestCompleted;

        /// <summary>
        /// 奖励领取事件
        /// </summary>
        public static event Action<Quest, int> OnRewardClaimed;

        #endregion

        #region 生命周期

        // 初始化优先级
        public override int InitPriority => 50;

        public override void Awake()
        {
            base.Awake();
            Initialize();
        }

        /// <summary>
        /// 单例初始化入口，由SingletonInitializer调用
        /// </summary>
        public override void OnInit()
        {
            // 加载任务进度
            LoadAllQuests();
            base.OnInit(); // 设置SingletonBehaviour的IsInitialized = true
        }

        private void Start()
        {
            // 如果SingletonInitializer没有调用OnInit，备用加载
            if (!IsInitialized)
            {
                LoadAllQuests();
                IsInitialized = true;
            }

            // 启动自动保存
            if (enableAutoSave)
            {
                autoSaveCoroutine = StartCoroutine(AutoSaveCoroutine());
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus) SaveAllQuests();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus) SaveAllQuests();
        }

        private void OnDestroy()
        {
            // 在应用退出时不要保存，避免访问已销毁的单例
            if (!isApplicationQuitting)
            {
                SaveAllQuests();
            }

            if (autoSaveCoroutine != null)
            {
                StopCoroutine(autoSaveCoroutine);
            }
        }

        private bool isApplicationQuitting = false;

        private void OnApplicationQuit()
        {
            isApplicationQuitting = true;
            // 应用退出时最后保存一次
            SaveAllQuestsSafely();
        }

        /// <summary>
        /// 安全保存所有任务（不创建新的单例实例）
        /// </summary>
        private void SaveAllQuestsSafely()
        {
            if (!isDirty) return;

            // 先检查StorageManager实例是否存在
            var storageManager = FindObjectOfType<StorageManager>();
            if (storageManager != null && saveData != null)
            {
                try
                {
                    saveData.UpdateMetadata(1);
                    saveData.AddQuests(activeQuests.Values);

                    storageManager.Save(
                        QUEST_SAVE_KEY,
                        saveData,
                        StorageType.Binary,
                        new StorageOptions
                        {
                            useCompression = true,
                            useEncryption = false,
                            addChecksum = true,
                            version = 1
                        }
                    );

                    Debug.Log("[QuestManager] 应用退出时任务保存成功");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[QuestManager] 应用退出时保存失败: {e.Message}");
                }
            }
        }

        #endregion

        #region 初始化

        private void Initialize()
        {
            // 初始化配置数据库
            if (questConfigDatabase == null)
            {
                // 尝试从Resources加载
                questConfigDatabase = Resources.Load<QuestConfigDatabase>("QuestConfigDatabase");

                if (questConfigDatabase == null)
                {
                    Debug.LogError("[QuestManager] QuestConfigDatabase not found!");
                    return;
                }
            }

            questConfigDatabase.Initialize();

            // 初始化存储数据
            saveData = new QuestSaveData();

            Debug.Log("[QuestManager] Initialized");
        }

        #endregion

        #region 任务创建

        /// <summary>
        /// 通过配置ID创建任务实例
        /// </summary>
        /// <param name="configId">配置ID</param>
        /// <param name="instanceTag">实例标签（用于区分同一任务的不同实例）</param>
        /// <param name="onProgress">进度更新回调</param>
        /// <param name="onComplete">完成回调</param>
        /// <returns>任务实例</returns>
        public Quest CreateQuestFromId(int configId, string instanceTag = "",
            Action<Quest, float> onProgress = null,
            Action<Quest> onComplete = null)
        {
            // 获取任务配置
            var questData = questConfigDatabase.GetQuestData(configId);
            if (questData == null)
            {
                Debug.LogError($"[QuestManager] Quest config not found: {configId}");
                return null;
            }

            // 生成实例键
            string instanceKey = $"{configId}_{instanceTag}";

            // 检查是否已存在
            if (activeQuests.ContainsKey(instanceKey))
            {
                if (enableDebugLog)
                    Debug.LogWarning($"[QuestManager] Quest already exists: {instanceKey}");
                return activeQuests[instanceKey];
            }

            // 检查前置任务
            if (questData.HasPrerequisites())
            {
                foreach (var prereqId in questData.PrerequisiteQuests)
                {
                    if (!IsQuestCompleted(prereqId))
                    {
                        Debug.LogWarning($"[QuestManager] Prerequisite quest {prereqId} not completed for quest {configId}");
                        return null;
                    }
                }
            }

            // 创建任务实例
            Quest quest = CreateQuestInstance(questData);
            quest.Initialize(configId, instanceTag, questData);

            // 注册任务
            activeQuests[instanceKey] = quest;

            // 注册回调
            if (onProgress != null)
            {
                progressCallbacks[instanceKey] = onProgress;
            }

            if (onComplete != null)
            {
                completionCallbacks[instanceKey] = onComplete;
            }

            // 尝试恢复进度
            LoadQuestProgress(quest);

            // 触发事件
            OnQuestCreated?.Invoke(quest);

            if (enableDebugLog)
                Debug.Log($"[QuestManager] Created quest: {quest}");

            return quest;
        }

        /// <summary>
        /// 创建任务实例（可以根据类型创建不同的子类）
        /// </summary>
        private Quest CreateQuestInstance(QuestData data)
        {
            // 这里可以根据任务类型创建不同的子类
            // 目前使用基类
            return new Quest();
        }

        #endregion

        #region 进度更新

        /// <summary>
        /// 更新指定任务实例的进度
        /// </summary>
        public void UpdateProgress(int configId, string instanceTag, int value)
        {
            string instanceKey = $"{configId}_{instanceTag}";

            if (!activeQuests.TryGetValue(instanceKey, out Quest quest))
            {
                if (enableDebugLog)
                    Debug.LogWarning($"[QuestManager] Quest not found: {instanceKey}");
                return;
            }

            if (quest.IsCompleted) return;

            // 更新进度
            bool changed = quest.UpdateProgress(value);
            if (!changed) return;

            isDirty = true;

            // 触发进度回调
            if (progressCallbacks.TryGetValue(instanceKey, out var progressCallback))
            {
                progressCallback?.Invoke(quest, quest.ProgressPercentage);
            }

            // 触发全局事件
            OnQuestProgressUpdated?.Invoke(quest, quest.ProgressPercentage);

            // 检查完成
            if (quest.IsCompleted)
            {
                HandleQuestCompleted(quest);
            }

            if (enableDebugLog)
                Debug.Log($"[QuestManager] Progress updated: {quest}");
        }

        /// <summary>
        /// 根据类型批量更新进度
        /// </summary>
        public void UpdateProgressByType(QuestType type, int value, string tag = null)
        {
            var questsToUpdate = new List<Quest>();

            foreach (var kvp in activeQuests)
            {
                var quest = kvp.Value;

                // 匹配任务类型
                if (quest.Data.QuestType == type && !quest.IsCompleted)
                {
                    // 如果指定了tag，检查是否匹配
                    if (!string.IsNullOrEmpty(tag))
                    {
                        if (quest.Data.Tag != tag && quest.Data.TargetItemId != tag)
                            continue;
                    }

                    questsToUpdate.Add(quest);
                }
            }

            // 批量更新
            foreach (var quest in questsToUpdate)
            {
                UpdateProgress(quest.ConfigId, quest.InstanceTag, value);
            }
        }

        #endregion

        #region 任务完成与奖励

        private void HandleQuestCompleted(Quest quest)
        {
            if (quest == null) return;

            string instanceKey = quest.InstanceKey;

            // 触发完成回调
            if (completionCallbacks.TryGetValue(instanceKey, out var completeCallback))
            {
                completeCallback?.Invoke(quest);
            }

            // 触发全局事件
            OnQuestCompleted?.Invoke(quest);

            // 自动发放奖励
            ClaimReward(quest.ConfigId, quest.InstanceTag);

            if (enableDebugLog)
                Debug.Log($"[QuestManager] Quest completed: {quest.Data.QuestName}");
        }

        /// <summary>
        /// 领取奖励
        /// </summary>
        public bool ClaimReward(int configId, string instanceTag)
        {
            string instanceKey = $"{configId}_{instanceTag}";

            if (!activeQuests.TryGetValue(instanceKey, out Quest quest))
            {
                return false;
            }

            if (!quest.ClaimReward())
            {
                return false;
            }

            // 发放奖励
            if (quest.Data.RewardCoins > 0)
            {
                // 这里应该调用游戏的金币系统
                // GameDataManager.Instance.AddCoins(quest.Data.RewardCoins);

                // 触发奖励事件
                OnRewardClaimed?.Invoke(quest, quest.Data.RewardCoins);
            }

            // 更新存储数据
            if (saveData != null)
            {
                saveData.UpdateStats(quest.Data.RewardCoins);
            }

            isDirty = true;

            if (enableDebugLog)
                Debug.Log($"[QuestManager] Reward claimed: {quest.Data.QuestName}, Coins: {quest.Data.RewardCoins}");

            return true;
        }

        #endregion

        #region 查询接口

        /// <summary>
        /// 获取任务进度（0-1）
        /// </summary>
        public float GetQuestProgress(int configId, string instanceTag = "")
        {
            var quest = GetQuest(configId, instanceTag);
            return quest?.ProgressPercentage ?? 0f;
        }

        /// <summary>
        /// 获取任务实例
        /// </summary>
        public Quest GetQuest(int configId, string instanceTag = "")
        {
            string instanceKey = $"{configId}_{instanceTag}";
            return activeQuests.TryGetValue(instanceKey, out Quest quest) ? quest : null;
        }

        /// <summary>
        /// 获取所有活跃任务
        /// </summary>
        public List<Quest> GetActiveQuests()
        {
            return activeQuests.Values.Where(q => !q.IsCompleted).ToList();
        }

        /// <summary>
        /// 获取所有完成的任务
        /// </summary>
        public List<Quest> GetCompletedQuests()
        {
            return activeQuests.Values.Where(q => q.IsCompleted).ToList();
        }

        /// <summary>
        /// 检查任务是否完成
        /// </summary>
        public bool IsQuestCompleted(int configId)
        {
            return saveData?.IsQuestCompleted(configId) ?? false;
        }

        #endregion

        #region 任务管理

        /// <summary>
        /// 移除任务实例
        /// </summary>
        public void RemoveQuest(int configId, string instanceTag = "")
        {
            string instanceKey = $"{configId}_{instanceTag}";

            if (activeQuests.ContainsKey(instanceKey))
            {
                activeQuests.Remove(instanceKey);
                progressCallbacks.Remove(instanceKey);
                completionCallbacks.Remove(instanceKey);
                isDirty = true;

                if (enableDebugLog)
                    Debug.Log($"[QuestManager] Removed quest: {instanceKey}");
            }
        }

        /// <summary>
        /// 重置任务（用于可重复任务）
        /// </summary>
        public void ResetQuest(int configId, string instanceTag = "")
        {
            var quest = GetQuest(configId, instanceTag);
            if (quest != null)
            {
                quest.Reset();
                isDirty = true;
            }
        }

        #endregion

        #region 数据持久化

        /// <summary>
        /// 保存所有任务
        /// </summary>
        public void SaveAllQuests()
        {
            if (!isDirty) return;

            try
            {
                if (saveData == null)
                {
                    saveData = new QuestSaveData();
                }

                saveData.UpdateMetadata(1);

                // 添加所有任务到保存数据
                saveData.AddQuests(activeQuests.Values);

                // 使用通用存储模块保存
                bool success = StorageManager.Instance.Save(
                    QUEST_SAVE_KEY,
                    saveData,
                    StorageType.Binary,
                    new StorageOptions
                    {
                        useCompression = true,
                        useEncryption = false,
                        addChecksum = true,
                        version = 1
                    }
                );

                if (success)
                {
                    isDirty = false;
                    if (enableDebugLog)
                        Debug.Log($"[QuestManager] Saved {activeQuests.Count} quests");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[QuestManager] Save failed: {e.Message}");
            }
        }

        /// <summary>
        /// 加载所有任务
        /// </summary>
        public void LoadAllQuests()
        {
            try
            {
                saveData = StorageManager.Instance.Load<QuestSaveData>(
                    QUEST_SAVE_KEY,
                    StorageType.Binary
                );

                if (saveData != null && saveData.ValidateChecksum())
                {
                    // 不自动恢复任务实例，等待外部系统创建时恢复
                    if (enableDebugLog)
                        Debug.Log($"[QuestManager] Loaded save data with {saveData.QuestList.Count} quests");
                }
                else
                {
                    saveData = new QuestSaveData();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[QuestManager] Load failed: {e.Message}");
                saveData = new QuestSaveData();
            }
        }

        /// <summary>
        /// 恢复单个任务进度
        /// </summary>
        private void LoadQuestProgress(Quest quest)
        {
            if (saveData == null) return;

            var progress = saveData.GetQuestProgress(quest.ConfigId, quest.InstanceTag);
            if (progress != null)
            {
                quest.SetProgress(progress.progress);
                quest.SetCompleted(progress.completed);
                quest.SetRewardClaimed(progress.rewardClaimed);

                if (enableDebugLog)
                    Debug.Log($"[QuestManager] Restored progress for quest: {quest}");
            }
        }

        /// <summary>
        /// 自动保存协程
        /// </summary>
        private IEnumerator AutoSaveCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(AUTO_SAVE_INTERVAL);

                if (isDirty)
                {
                    SaveAllQuests();
                }
            }
        }

        #endregion

        #region 调试

        [ContextMenu("Debug: List All Quests")]
        private void DebugListAllQuests()
        {
            Debug.Log($"[QuestManager] Active Quests: {activeQuests.Count}");
            foreach (var quest in activeQuests.Values)
            {
                Debug.Log($"  - {quest}");
            }
        }

        [ContextMenu("Debug: Save Now")]
        private void DebugSaveNow()
        {
            isDirty = true;
            SaveAllQuests();
        }

        [ContextMenu("Debug: Clear All Data")]
        private void DebugClearAllData()
        {
            activeQuests.Clear();
            progressCallbacks.Clear();
            completionCallbacks.Clear();
            saveData = new QuestSaveData();
            StorageManager.Instance.Delete(QUEST_SAVE_KEY, StorageType.Binary);
            Debug.Log("[QuestManager] All data cleared");
        }

        #endregion
    }
}