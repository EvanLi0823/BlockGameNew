// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using BlockPuzzleGameToolkit.Scripts.Settings;
using BlockPuzzleGameToolkit.Scripts.LevelRewardMultiplier.Data;
using StorageSystem.Core;

namespace BlockPuzzleGameToolkit.Scripts.LevelRewardMultiplier.Managers
{
    /// <summary>
    /// 关卡奖励倍率管理器（单例）
    /// 负责管理关卡奖励倍率的获取、消费、重置等运行时逻辑
    /// </summary>
    public class LevelRewardMultiplierManager : SingletonBehaviour<LevelRewardMultiplierManager>
    {
        [Header("配置")]
        [SerializeField, Tooltip("关卡奖励倍率配置")]
        private LevelRewardMultiplierSettings settings;

        [SerializeField, Tooltip("是否启用调试日志")]
        private bool enableDebugLog = false;

        // 运行时数据
        private Dictionary<string, MultiplierRuntimeData> runtimeData;

        // 存储键
        private const string SAVE_KEY = "level_reward_multiplier_data";

        // 是否已初始化
        private bool isInitialized = false;

        /// <summary>
        /// 运行时数据
        /// </summary>
        [Serializable]
        private class MultiplierRuntimeData
        {
            public string configId;
            public int currentIndex;           // 当前使用的倍率索引
            public DateTime lastResetTime;     // 上次重置时间
            public int totalUseCount;          // 总使用次数（统计用）

            public MultiplierRuntimeData()
            {
                lastResetTime = DateTime.Now;
            }

            public MultiplierRuntimeData(string id)
            {
                configId = id;
                currentIndex = 0;
                lastResetTime = DateTime.Now;
                totalUseCount = 0;
            }
        }

        // 初始化优先级
        public override int InitPriority => 40;

        public override void Awake()
        {
            base.Awake();
            // 不再在Awake中初始化，等待SingletonInitializer调用OnInit
        }

        /// <summary>
        /// 单例初始化入口，由SingletonInitializer调用
        /// </summary>
        public override void OnInit()
        {
            if (!isInitialized)
            {
                Initialize();
            }
            base.OnInit(); // 设置SingletonBehaviour的IsInitialized = true
        }

        private void Start()
        {
            // 备用初始化，如果SingletonInitializer没有调用
            if (!isInitialized)
            {
                Initialize();
            }

            // 每分钟检查一次是否需要每日重置
            InvokeRepeating(nameof(CheckDailyReset), 60f, 60f);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void Initialize()
        {
            if (isInitialized) return;

            LoadSettings();
            LoadRuntimeData();
            CheckDailyReset();
            
            //订阅提现事件
            EventManager.GetEvent(Enums.EGameEvent.HasWithDraw).Subscribe(OnWithdraw);
            
            isInitialized = true;

            if (enableDebugLog)
                Debug.Log("[LevelRewardMultiplierManager] 初始化完成");
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        private void LoadSettings()
        {
            if (settings == null)
            {
                // 尝试从Resources加载
                settings = Resources.Load<LevelRewardMultiplierSettings>("Settings/LevelRewardMultiplierSettings");

                if (settings == null)
                {
                    Debug.LogError("[LevelRewardMultiplierManager] 未找到LevelRewardMultiplierSettings配置文件");
                }
            }
        }

        /// <summary>
        /// 获取当前倍率（不消费）
        /// </summary>
        /// <param name="configId">配置ID</param>
        /// <returns>当前倍率值</returns>
        public float GetCurrentMultiplier(string configId)
        {
            if (!isInitialized)
                Initialize();

            if (settings == null)
            {
                Debug.LogError("[LevelRewardMultiplierManager] 配置未加载");
                return 1.0f;
            }

            var config = settings.GetConfig(configId);
            if (config == null)
            {
                Debug.LogError($"[LevelRewardMultiplierManager] 配置不存在: {configId}");
                return 1.0f;
            }

            var data = GetOrCreateRuntimeData(configId);
            float multiplier = config.GetMultiplier(data.currentIndex);

            if (enableDebugLog)
                Debug.Log($"[LevelRewardMultiplierManager] 获取倍率 {configId}: 索引={data.currentIndex}, 倍率={multiplier}");

            return multiplier;
        }

        /// <summary>
        /// 消费倍率（索引+1）
        /// </summary>
        /// <param name="configId">配置ID</param>
        public void ConsumeMultiplier(string configId)
        {
            if (!isInitialized)
                Initialize();

            if (settings == null) return;

            var config = settings.GetConfig(configId);
            if (config == null)
            {
                Debug.LogError($"[LevelRewardMultiplierManager] 配置不存在: {configId}");
                return;
            }

            var data = GetOrCreateRuntimeData(configId);

            // 索引递增，但不超过数组长度
            if (!config.IsLastMultiplier(data.currentIndex))
            {
                data.currentIndex++;
            }

            data.totalUseCount++;
            SaveRuntimeData();

            if (enableDebugLog)
                Debug.Log($"[LevelRewardMultiplierManager] 消费倍率 {configId}: 新索引={data.currentIndex}, 总使用次数={data.totalUseCount}");
        }

        /// <summary>
        /// 重置指定配置的索引
        /// </summary>
        /// <param name="configId">配置ID</param>
        public void ResetIndex(string configId)
        {
            var data = GetOrCreateRuntimeData(configId);
            data.currentIndex = 0;
            data.lastResetTime = DateTime.Now;
            SaveRuntimeData();

            if (enableDebugLog)
                Debug.Log($"[LevelRewardMultiplierManager] 重置配置 {configId}");
        }

        /// <summary>
        /// 重置所有配置
        /// </summary>
        public void ResetAll()
        {
            if (settings == null) return;

            foreach (var config in settings.Configs)
            {
                ResetIndex(config.ConfigId);
            }

            Debug.Log("[LevelRewardMultiplierManager] 重置所有配置");
        }

        /// <summary>
        /// 提现时重置
        /// </summary>
        public void OnWithdraw()
        {
            if (settings == null) return;

            int resetCount = 0;
            foreach (var config in settings.Configs)
            {
                if (config.ResetOnWithdraw)
                {
                    ResetIndex(config.ConfigId);
                    resetCount++;
                }
            }

            if (resetCount > 0)
            {
                Debug.Log($"[LevelRewardMultiplierManager] 提现重置了 {resetCount} 个配置");
            }
        }

        /// <summary>
        /// 每日重置检查
        /// </summary>
        private void CheckDailyReset()
        {
            if (settings == null) return;

            var now = DateTime.Now;
            int resetCount = 0;

            foreach (var config in settings.Configs)
            {
                if (!config.ResetDaily) continue;

                var data = GetOrCreateRuntimeData(config.ConfigId);

                // 检查是否跨天（比较日期部分）
                if (data.lastResetTime.Date < now.Date)
                {
                    ResetIndex(config.ConfigId);
                    resetCount++;
                }
            }

            if (resetCount > 0 && enableDebugLog)
            {
                Debug.Log($"[LevelRewardMultiplierManager] 每日重置了 {resetCount} 个配置");
            }
        }

        /// <summary>
        /// 获取或创建运行时数据
        /// </summary>
        private MultiplierRuntimeData GetOrCreateRuntimeData(string configId)
        {
            if (runtimeData == null)
                runtimeData = new Dictionary<string, MultiplierRuntimeData>();

            if (!runtimeData.ContainsKey(configId))
            {
                runtimeData[configId] = new MultiplierRuntimeData(configId);
            }

            return runtimeData[configId];
        }

        /// <summary>
        /// 获取配置信息（供UI显示）
        /// </summary>
        public MultiplierInfo GetMultiplierInfo(string configId)
        {
            if (settings == null) return null;

            var config = settings.GetConfig(configId);
            if (config == null) return null;

            var data = GetOrCreateRuntimeData(configId);

            return new MultiplierInfo
            {
                ConfigId = configId,
                ConfigName = config.ConfigName,
                CurrentMultiplier = config.GetMultiplier(data.currentIndex),
                CurrentIndex = data.currentIndex,
                TotalMultipliers = config.Multipliers.Length,
                IsLastMultiplier = config.IsLastMultiplier(data.currentIndex),
                NextMultiplier = config.IsLastMultiplier(data.currentIndex)
                    ? config.GetMultiplier(data.currentIndex)
                    : config.GetMultiplier(data.currentIndex + 1),
                TotalUseCount = data.totalUseCount
            };
        }

        /// <summary>
        /// 倍率信息
        /// </summary>
        public class MultiplierInfo
        {
            public string ConfigId { get; set; }
            public string ConfigName { get; set; }
            public float CurrentMultiplier { get; set; }
            public int CurrentIndex { get; set; }
            public int TotalMultipliers { get; set; }
            public bool IsLastMultiplier { get; set; }
            public float NextMultiplier { get; set; }
            public int TotalUseCount { get; set; }
        }

        #region 数据持久化

        /// <summary>
        /// 保存运行时数据
        /// </summary>
        private void SaveRuntimeData()
        {
            if (runtimeData == null || runtimeData.Count == 0)
                return;

            try
            {
                var saveData = new LevelRewardMultiplierSaveData
                {
                    runtimeDataMap = new Dictionary<string, LevelRewardMultiplierSaveData.RuntimeDataEntry>()
                };

                // 转换运行时数据为可序列化格式
                foreach (var kvp in runtimeData)
                {
                    saveData.runtimeDataMap[kvp.Key] = new LevelRewardMultiplierSaveData.RuntimeDataEntry
                    {
                        configId = kvp.Value.configId,
                        currentIndex = kvp.Value.currentIndex,
                        lastResetTime = kvp.Value.lastResetTime.Ticks,
                        totalUseCount = kvp.Value.totalUseCount
                    };
                }

                saveData.UpdateMetadata(1);

                // 使用StorageManager保存
                if (StorageManager.Instance != null)
                {
                    StorageManager.Instance.Save(SAVE_KEY, saveData, StorageType.Binary);

                    if (enableDebugLog)
                        Debug.Log("[LevelRewardMultiplierManager] 数据已保存");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[LevelRewardMultiplierManager] 保存数据失败: {e.Message}");
            }
        }

        /// <summary>
        /// 加载运行时数据
        /// </summary>
        private void LoadRuntimeData()
        {
            try
            {
                if (StorageManager.Instance != null)
                {
                    var saveData = StorageManager.Instance.Load<LevelRewardMultiplierSaveData>(
                        SAVE_KEY, StorageType.Binary);

                    if (saveData != null && saveData.ValidateChecksum())
                    {
                        runtimeData = new Dictionary<string, MultiplierRuntimeData>();

                        // 转换保存的数据为运行时格式
                        foreach (var kvp in saveData.runtimeDataMap)
                        {
                            runtimeData[kvp.Key] = new MultiplierRuntimeData
                            {
                                configId = kvp.Value.configId,
                                currentIndex = kvp.Value.currentIndex,
                                lastResetTime = new DateTime(kvp.Value.lastResetTime),
                                totalUseCount = kvp.Value.totalUseCount
                            };
                        }

                        if (enableDebugLog)
                            Debug.Log($"[LevelRewardMultiplierManager] 加载了 {runtimeData.Count} 个配置的数据");
                    }
                    else
                    {
                        runtimeData = new Dictionary<string, MultiplierRuntimeData>();

                        if (enableDebugLog)
                            Debug.Log("[LevelRewardMultiplierManager] 没有找到保存的数据，使用新数据");
                    }
                }
                else
                {
                    runtimeData = new Dictionary<string, MultiplierRuntimeData>();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[LevelRewardMultiplierManager] 加载数据失败: {e.Message}");
                runtimeData = new Dictionary<string, MultiplierRuntimeData>();
            }
        }

        #endregion

        #region 调试方法

#if UNITY_EDITOR
        /// <summary>
        /// 调试：打印当前状态
        /// </summary>
        [ContextMenu("打印当前状态")]
        private void DebugPrintStatus()
        {
            if (settings == null)
            {
                Debug.Log("[LevelRewardMultiplierManager] 配置未加载");
                return;
            }

            Debug.Log("========== LevelRewardMultiplierManager 状态 ==========");

            foreach (var config in settings.Configs)
            {
                var info = GetMultiplierInfo(config.ConfigId);
                if (info != null)
                {
                    Debug.Log($"配置: {info.ConfigName} ({info.ConfigId})");
                    Debug.Log($"  当前倍率: {info.CurrentMultiplier}x");
                    Debug.Log($"  索引: {info.CurrentIndex}/{info.TotalMultipliers - 1}");
                    Debug.Log($"  是否最后: {info.IsLastMultiplier}");
                    Debug.Log($"  使用次数: {info.TotalUseCount}");
                }
            }

            Debug.Log("==================================================");
        }

        /// <summary>
        /// 调试：强制重置所有
        /// </summary>
        [ContextMenu("强制重置所有")]
        private void DebugResetAll()
        {
            ResetAll();
            Debug.Log("[LevelRewardMultiplierManager] 已强制重置所有配置");
        }

        /// <summary>
        /// 调试：模拟提现
        /// </summary>
        [ContextMenu("模拟提现")]
        private void DebugSimulateWithdraw()
        {
            OnWithdraw();
            Debug.Log("[LevelRewardMultiplierManager] 已模拟提现重置");
        }
#endif

        #endregion
    }
}