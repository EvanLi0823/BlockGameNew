// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using StorageSystem.Utils;

namespace StorageSystem.Core
{
    /// <summary>
    /// 存储管理器（单例）
    /// 提供统一的数据存储接口
    /// </summary>
    public class StorageManager : SingletonBehaviour<StorageManager>
    {
        // 默认存储策略
        private IStorageStrategy defaultStrategy;

        // 策略缓存 - 初始化为空字典，避免null引用
        private Dictionary<StorageType, IStorageStrategy> strategies = new Dictionary<StorageType, IStorageStrategy>();

        // 默认存储类型
        [SerializeField] private StorageType defaultStorageType = StorageType.Binary;

        // 是否启用调试日志
        [SerializeField] private bool enableDebugLog = false;

        // 初始化锁对象
        private readonly object initLock = new object();

        // 初始化优先级（最高优先级，最先初始化）
        public override int InitPriority => 0;

        // 构造函数，确保字典初始化
        public StorageManager()
        {
            if (strategies == null)
            {
                strategies = new Dictionary<StorageType, IStorageStrategy>();
            }
        }

        // 静态构造，确保策略字典存在
        static StorageManager()
        {
            // 空的静态构造函数，确保类型初始化
        }

        // 重写Instance属性，确保初始化
        public new static StorageManager Instance
        {
            get
            {
                var instance = SingletonBehaviour<StorageManager>.Instance;
                if (instance != null && !instance.IsInitialized)
                {
                    instance.EnsureInitialized();
                }
                return instance;
            }
        }

        public override void Awake()
        {
            base.Awake();
            // 立即初始化，确保其他Manager能使用
            EnsureInitialized();
        }

        /// <summary>
        /// 单例初始化入口，由SingletonInitializer调用
        /// </summary>
        public override void OnInit()
        {
            if (!IsInitialized)
            {
                EnsureInitialized();
            }
            base.OnInit(); // 设置IsInitialized = true
        }

        private void OnEnable()
        {
            // 额外的初始化检查（如果通过其他方式访问）
            EnsureInitialized();
        }

        private void Start()
        {
            // 额外的初始化检查，以防SingletonInitializer未调用
            EnsureInitialized();

            if (enableDebugLog)
            {
                Debug.Log($"[StorageManager] Start - 已初始化: {IsInitialized}, 策略数量: {strategies?.Count ?? 0}");
            }
        }

        /// <summary>
        /// 确保策略已初始化
        /// </summary>
        private void EnsureInitialized()
        {
            if (IsInitialized) return;

            lock (initLock)
            {
                if (IsInitialized) return;

                if (enableDebugLog)
                    Debug.Log("[StorageManager] 开始初始化...");

                InitializeStrategies();
            }
        }

        /// <summary>
        /// 初始化存储策略
        /// </summary>
        private void InitializeStrategies()
        {
            try
            {
                // 确保策略字典已创建
                if (strategies == null)
                {
                    strategies = new Dictionary<StorageType, IStorageStrategy>();
                }

                // 清空现有策略
                strategies.Clear();

                // 注册内置策略 - 直接添加到字典
                strategies[StorageType.Binary] = new Strategies.BinaryStorageStrategy();
                strategies[StorageType.Json] = new Strategies.JsonStorageStrategy();
                strategies[StorageType.SecureBinary] = new Strategies.SecureStorageStrategy();

                // 设置默认策略
                if (strategies.TryGetValue(defaultStorageType, out var strategy))
                {
                    defaultStrategy = strategy;
                }
                else if (strategies.TryGetValue(StorageType.Binary, out strategy))
                {
                    // 如果默认类型不存在，使用Binary作为默认
                    defaultStrategy = strategy;
                    defaultStorageType = StorageType.Binary;
                }

                // 标记为已初始化
                IsInitialized = true;

                if (enableDebugLog)
                {
                    Debug.Log($"[StorageManager] 初始化完成，注册了 {strategies.Count} 个策略");
                    foreach (var kvp in strategies)
                    {
                        Debug.Log($"[StorageManager] - {kvp.Key}: {kvp.Value.GetType().Name}");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[StorageManager] 初始化失败: {e.Message}\n{e.StackTrace}");
                IsInitialized = false; // 确保失败时可以重试

                // 尝试至少设置Binary策略
                try
                {
                    if (strategies == null)
                        strategies = new Dictionary<StorageType, IStorageStrategy>();

                    strategies[StorageType.Binary] = new Strategies.BinaryStorageStrategy();
                    defaultStrategy = strategies[StorageType.Binary];
                    Debug.LogWarning("[StorageManager] 降级初始化，仅启用Binary策略");
                }
                catch
                {
                    // 忽略二次失败
                }
            }
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">存储键</param>
        /// <param name="data">数据对象</param>
        /// <param name="type">存储类型</param>
        /// <param name="options">存储选项</param>
        /// <returns>是否保存成功</returns>
        public bool Save<T>(string key, T data, StorageType type = StorageType.Binary, StorageOptions options = null) where T : class
        {
            try
            {
                var strategy = GetStrategy(type);
                var path = StoragePathHelper.GetSavePath(key, strategy.GetFileExtension());

                if (enableDebugLog)
                    Debug.Log($"[StorageManager] Saving {key} to {path} using {type}");

                return strategy.Save(path, data, options ?? new StorageOptions());
            }
            catch (Exception e)
            {
                Debug.LogError($"[StorageManager] Save failed for {key}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 加载数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">存储键</param>
        /// <param name="type">存储类型</param>
        /// <returns>加载的数据</returns>
        public T Load<T>(string key, StorageType type = StorageType.Binary) where T : class
        {
            try
            {
                var strategy = GetStrategy(type);
                var path = StoragePathHelper.GetSavePath(key, strategy.GetFileExtension());

                if (enableDebugLog)
                    Debug.Log($"[StorageManager] Loading {key} from {path} using {type}");

                return strategy.Load<T>(path);
            }
            catch (Exception e)
            {
                Debug.LogError($"[StorageManager] Load failed for {key}: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 异步保存数据
        /// </summary>
        public async Task<bool> SaveAsync<T>(string key, T data, StorageType type = StorageType.Binary, StorageOptions options = null) where T : class
        {
            return await Task.Run(() => Save(key, data, type, options));
        }

        /// <summary>
        /// 异步加载数据
        /// </summary>
        public async Task<T> LoadAsync<T>(string key, StorageType type = StorageType.Binary) where T : class
        {
            return await Task.Run(() => Load<T>(key, type));
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="key">存储键</param>
        /// <param name="type">存储类型</param>
        /// <returns>是否删除成功</returns>
        public bool Delete(string key, StorageType type = StorageType.Binary)
        {
            try
            {
                var strategy = GetStrategy(type);
                var path = StoragePathHelper.GetSavePath(key, strategy.GetFileExtension());

                if (enableDebugLog)
                    Debug.Log($"[StorageManager] Deleting {key} at {path}");

                return strategy.Delete(path);
            }
            catch (Exception e)
            {
                Debug.LogError($"[StorageManager] Delete failed for {key}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 检查数据是否存在
        /// </summary>
        /// <param name="key">存储键</param>
        /// <param name="type">存储类型</param>
        /// <returns>是否存在</returns>
        public bool Exists(string key, StorageType type = StorageType.Binary)
        {
            try
            {
                var strategy = GetStrategy(type);
                var path = StoragePathHelper.GetSavePath(key, strategy.GetFileExtension());
                return strategy.Exists(path);
            }
            catch (Exception e)
            {
                Debug.LogError($"[StorageManager] Exists check failed for {key}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取所有保存的键
        /// </summary>
        /// <returns>键列表</returns>
        public List<string> GetAllKeys()
        {
            return StoragePathHelper.GetAllSavedKeys();
        }

        /// <summary>
        /// 清空所有数据
        /// </summary>
        public void ClearAll()
        {
            var keys = GetAllKeys();
            foreach (var key in keys)
            {
                foreach (StorageType type in Enum.GetValues(typeof(StorageType)))
                {
                    if (type != StorageType.Custom)
                        Delete(key, type);
                }
            }
            Debug.Log("[StorageManager] All data cleared");
        }

        /// <summary>
        /// 注册自定义存储策略
        /// </summary>
        /// <param name="type">存储类型</param>
        /// <param name="strategy">策略实例</param>
        public void RegisterStrategy(StorageType type, IStorageStrategy strategy)
        {
            // 确保已初始化
            EnsureInitialized();

            // 确保策略字典已创建
            if (strategies == null)
            {
                strategies = new Dictionary<StorageType, IStorageStrategy>();
            }

            strategies[type] = strategy;

            if (enableDebugLog)
                Debug.Log($"[StorageManager] Registered strategy for {type}");
        }

        /// <summary>
        /// 获取存储策略
        /// </summary>
        /// <param name="type">存储类型</param>
        /// <returns>策略实例</returns>
        private IStorageStrategy GetStrategy(StorageType type)
        {
            // 确保策略已初始化
            EnsureInitialized();

            // 从字典中获取策略
            if (strategies != null && strategies.TryGetValue(type, out var strategy))
            {
                return strategy;
            }

            // 使用默认策略
            if (defaultStrategy != null)
            {
                if (enableDebugLog)
                    Debug.LogWarning($"[StorageManager] Strategy for {type} not found, using default {defaultStorageType}");
                return defaultStrategy;
            }

            // 这不应该发生，但作为最后的保障
            Debug.LogError($"[StorageManager] 无法获取策略 {type}，策略字典状态: " +
                         $"strategies={strategies?.Count ?? -1}, " +
                         $"IsInitialized={IsInitialized}, " +
                         $"defaultStrategy={defaultStrategy != null}");

            // 尝试强制初始化
            InitializeStrategies();

            if (strategies != null && strategies.TryGetValue(type, out strategy))
            {
                Debug.LogWarning($"[StorageManager] 强制初始化后获取到策略 {type}");
                return strategy;
            }

            throw new InvalidOperationException($"No strategy found for {type} after initialization");
        }

        /// <summary>
        /// 设置默认存储类型
        /// </summary>
        /// <param name="type">存储类型</param>
        public void SetDefaultStorageType(StorageType type)
        {
            defaultStorageType = type;

            // 确保已初始化
            EnsureInitialized();

            // 直接从字典获取策略，避免递归
            if (strategies != null && strategies.TryGetValue(type, out var strategy))
            {
                defaultStrategy = strategy;
            }
            else
            {
                Debug.LogWarning($"[StorageManager] 无法设置默认类型 {type}，策略不存在");
            }
        }
    }
}