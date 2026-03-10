using System;
using System.Collections.Generic;
using UnityEngine;
using BlockPuzzle.AdSystem.Models;
using BlockPuzzleGameToolkit.Scripts.Settings;
using BlockPuzzle.NativeBridge;
using BlockPuzzle.NativeBridge.Enums;
using BlockPuzzleGameToolkit.Scripts.GameCore;

namespace BlockPuzzle.AdSystem.Managers
{
    /// <summary>
    /// 广告系统管理器
    /// 管理广告播放流程，调用NativeBridge接口
    /// </summary>
    public class AdSystemManager : SingletonBehaviour<AdSystemManager>
    {
        #region 私有字段

        private AdSystemSettings _settings;
        private Dictionary<string, AdEntry> _entryCache;
        private string _currentPlayingEntry;
        private bool _isInitialized = false;

        #endregion

        #region 事件

        /// <summary>
        /// 广告播放成功事件
        /// </summary>
        public static event Action<string> OnAdPlaySuccess;

        /// <summary>
        /// 广告播放失败事件
        /// </summary>
        public static event Action<string, string> OnAdPlayFailed;

        /// <summary>
        /// 广告开始播放事件
        /// </summary>
        public static event Action<string> OnAdStartPlaying;

        /// <summary>
        /// 任意广告播放结束事件（无论成功失败）
        /// </summary>
        public static event Action<AdPlayResult> OnAdPlayComplete;

        #endregion

        #region 生命周期

        public override void Awake()
        {
            base.Awake();
            // DontDestroyOnLoad(gameObject); // 移除重复调用，基类已处理
            // 移除这里的Initialize()调用，让GameManager统一管理初始化顺序
            // Initialize();
        }

        /// <summary>
        /// 重写OnInit方法，由GameManager统一调用
        /// </summary>
        public override void OnInit()
        {
            if (!IsInitialized)
            {
                Initialize();
            }
            base.OnInit(); // 设置IsInitialized = true
        }

        private void OnEnable()
        {
            // 订阅NativeBridge的广告回调事件
            if (NativeBridgeManager.Instance != null)
            {
                NativeBridgeManager.OnVideoPlayEnd += HandleVideoPlayEnd;
            }
        }

        private void OnDisable()
        {
            // 取消订阅
            if (NativeBridgeManager.Instance != null)
            {
                NativeBridgeManager.OnVideoPlayEnd -= HandleVideoPlayEnd;
            }
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化广告系统
        /// </summary>
        private void Initialize()
        {
            LoadSettings();

            if (_settings == null)
            {
                Debug.LogError("[AdSystem] Settings not found! Please create it via menu: Tools > BlockPuzzleGameToolkit > Settings > Ad System settings");
                return;
            }

            if (!_settings.EnableAdSystem)
            {
                Debug.Log("[AdSystem] Ad system is disabled");
                return;
            }

            // 验证设置
            if (!_settings.ValidateSettings())
            {
                Debug.LogError("[AdSystem] Settings validation failed!");
                return;
            }

            // 构建缓存
            BuildEntryCache();

            _isInitialized = true;

            LogDebug("Ad system initialized successfully");
            LogDebug($"Total entries: {_entryCache.Count}");
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        private void LoadSettings()
        {
            _settings = Resources.Load<AdSystemSettings>("Settings/AdSystemSettings");

            if (_settings == null)
            {
                Debug.LogError("[AdSystem] AdSystemSettings not found! Please create it via menu: Tools > BlockPuzzleGameToolkit > Settings > Ad System settings");
            }
        }

        /// <summary>
        /// 构建入口缓存
        /// </summary>
        private void BuildEntryCache()
        {
            _entryCache = new Dictionary<string, AdEntry>();

            foreach (var entry in _settings.AdEntries)
            {
                if (entry.Active && entry.IsValid())
                {
                    _entryCache[entry.Name] = entry;
                }
            }
        }

        #endregion

        #region 公共API

        /// <summary>
        /// 播放广告
        /// </summary>
        /// <param name="entryName">广告入口名称</param>
        /// <param name="onComplete">完成回调</param>
        public void PlayAd(string entryName, Action<bool> onComplete = null)
        {
#if UNITY_EDITOR
            // 编辑器环境下根据配置决定模拟成功还是失败
            bool simulateFailure = _settings != null && _settings.SimulateAdFailureInEditor;

            if (simulateFailure)
            {
                LogDebug($"[EDITOR MODE] Simulating ad FAILURE for: {entryName}");
            }
            else
            {
                LogDebug($"[EDITOR MODE] Simulating ad SUCCESS for: {entryName}");
            }

            // 触发开始播放事件（保持与正式环境一致的事件流）
            OnAdStartPlaying?.Invoke(entryName);

            // 使用协程模拟延迟和触发事件
            StartCoroutine(EditorSimulateAdCoroutine(entryName, onComplete, simulateFailure));
            return;
#endif

            if (!_isInitialized)
            {
                LogError("Ad system not initialized");
                onComplete?.Invoke(false);
                return;
            }

            if (!_entryCache.TryGetValue(entryName, out AdEntry entry))
            {
                LogError($"Ad entry not found: {entryName}");
                onComplete?.Invoke(false);
                return;
            }

            if (!entry.Active)
            {
                LogWarning($"Ad entry is inactive: {entryName}");
                onComplete?.Invoke(false);
                return;
            }

            LogDebug($"Playing ad: {entryName} (Type: {entry.Type})");

            // 保存当前播放的入口
            _currentPlayingEntry = entryName;

            // 触发开始播放事件
            OnAdStartPlaying?.Invoke(entryName);

            // 检查NativeBridge是否可用
            // 在Android环境下，即使异步初始化未完成，只要Instance存在就尝试使用
#if UNITY_ANDROID && !UNITY_EDITOR
            if (NativeBridgeManager.Instance == null)
            {
                LogWarning("NativeBridge instance not available, simulating success");
                SimulateAdSuccess(entryName, onComplete);
                return;
            }

            // Android环境下，即使IsInitSuccess()返回false（异步响应未到），也尝试继续
            // 因为原生平台可能已经准备好了，只是Unity端还没收到响应
            if (!NativeBridgeManager.Instance.IsInitSuccess())
            {
                LogDebug("NativeBridge initialization still in progress, attempting to play ad anyway");
            }
#else
            // 其他平台保持原有逻辑
            if (NativeBridgeManager.Instance == null || !NativeBridgeManager.Instance.IsInitSuccess())
            {
                LogWarning("NativeBridge not available, simulating success");
                SimulateAdSuccess(entryName, onComplete);
                return;
            }
#endif

            // 查询广告是否准备好
            AdType adType = (AdType)entry.Type;
            bool isReady = NativeBridgeManager.Instance.IsADReady(adType);

            if (!isReady)
            {
                LogDebug($"Ad not ready for {entryName}, returning failure");
                // 触发失败事件
                OnAdPlayFailed?.Invoke(entryName, "Ad not ready");
                var failResult = new AdPlayResult(entryName, false, entry.Type);
                OnAdPlayComplete?.Invoke(failResult);
                // 执行失败回调
                onComplete?.Invoke(false);
                return;
            }

            // 设置回调
            if (onComplete != null)
            {
                // 创建一个可以自我取消订阅的事件处理器
                Action<AdPlayResult> handler = null;
                handler = (result) =>
                {
                    if (result.entryName == entryName)
                    {
                        try
                        {
                            // 执行用户回调
                            onComplete(result.success);
                        }
                        finally
                        {
                            // ✅ 执行完成后立即取消订阅（无论成功或异常）
                            // 防止内存泄漏和重复执行
                            OnAdPlayComplete -= handler;
                        }
                    }
                };
                OnAdPlayComplete += handler;
            }

            // 调用NativeBridge播放广告
            NativeBridgeManager.Instance.SendMessageToPlatform(BridgeMessageType.ShowVideo, entryName, entry.Type);
        }

        /// <summary>
        /// 获取广告入口配置
        /// </summary>
        public AdEntry GetAdEntry(string entryName)
        {
            if (!_isInitialized || !_entryCache.TryGetValue(entryName, out AdEntry entry))
            {
                return null;
            }
            return entry;
        }

        /// <summary>
        /// 检查广告是否准备好
        /// </summary>
        public bool IsAdReady(string entryName)
        {
#if UNITY_EDITOR
            // 编辑器环境下总是返回准备好
            LogDebug($"[EDITOR MODE] Ad ready check for {entryName}: always true in editor");
            return true;
#endif

            var entry = GetAdEntry(entryName);
            if (entry == null || !entry.Active)
            {
                return false;
            }

            if (NativeBridgeManager.Instance == null || !NativeBridgeManager.Instance.IsInitSuccess())
            {
                return false;
            }

            return NativeBridgeManager.Instance.IsADReady((AdType)entry.Type);
        }

        /// <summary>
        /// 获取所有活跃的广告入口
        /// </summary>
        public List<AdEntry> GetActiveEntries()
        {
            return _settings?.GetActiveEntries() ?? new List<AdEntry>();
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 处理视频播放结束
        /// </summary>
        private void HandleVideoPlayEnd()
        {
            if (string.IsNullOrEmpty(_currentPlayingEntry))
            {
                LogWarning("Received video end callback but no entry is playing");
                return;
            }

            var entry = GetAdEntry(_currentPlayingEntry);
            if (entry == null)
            {
                LogError($"Entry not found for callback: {_currentPlayingEntry}");
                return;
            }

            // 创建播放结果
            var result = new AdPlayResult(_currentPlayingEntry, true, entry.Type);

            // 触发事件
            OnAdPlaySuccess?.Invoke(_currentPlayingEntry);
            OnAdPlayComplete?.Invoke(result);

            LogDebug($"Ad play success: {_currentPlayingEntry}");

            // 清除当前播放状态
            _currentPlayingEntry = null;
        }

        /// <summary>
        /// 处理广告完成
        /// </summary>
        private void HandleAdComplete(AdPlayResult result)
        {
            // 预留的回调处理
        }

        /// <summary>
        /// 模拟广告成功
        /// </summary>
        private void SimulateAdSuccess(string entryName, Action<bool> onComplete)
        {
            // 延迟模拟广告播放
            StartCoroutine(SimulateAdPlayCoroutine(entryName, onComplete));
        }

        private System.Collections.IEnumerator SimulateAdPlayCoroutine(string entryName, Action<bool> onComplete)
        {
            yield return new WaitForSeconds(0.5f);

            var result = new AdPlayResult(entryName, true, 0);

            OnAdPlaySuccess?.Invoke(entryName);
            OnAdPlayComplete?.Invoke(result);
            onComplete?.Invoke(true);

            _currentPlayingEntry = null;
        }

        #endregion

        #region 调试

        private void LogDebug(string message)
        {
            if (_settings != null && _settings.DebugMode)
            {
                Debug.Log($"[AdSystem] {message}");
            }
        }

        private void LogWarning(string message)
        {
            Debug.LogWarning($"[AdSystem] {message}");
        }

        private void LogError(string message)
        {
            Debug.LogError($"[AdSystem] {message}");
        }

        #endregion

#if UNITY_EDITOR
        /// <summary>
        /// 编辑器模式下模拟广告播放
        /// </summary>
        /// <param name="entryName">广告入口名称</param>
        /// <param name="onComplete">完成回调</param>
        /// <param name="simulateFailure">是否模拟失败</param>
        private System.Collections.IEnumerator EditorSimulateAdCoroutine(string entryName, Action<bool> onComplete, bool simulateFailure = false)
        {
            // 模拟广告播放延迟
            yield return new WaitForSeconds(0.5f);

            if (simulateFailure)
            {
                // 模拟失败
                var failResult = new AdPlayResult(entryName, false, 0);

                // 触发失败事件
                OnAdPlayFailed?.Invoke(entryName, "Editor simulated failure");
                OnAdPlayComplete?.Invoke(failResult);

                // 执行失败回调
                onComplete?.Invoke(false);

                Debug.LogWarning($"[AdSystem] [EDITOR] Ad simulation FAILED for: {entryName}");
            }
            else
            {
                // 模拟成功
                var result = new AdPlayResult(entryName, true, 0);

                // 触发成功事件
                OnAdPlaySuccess?.Invoke(entryName);
                OnAdPlayComplete?.Invoke(result);

                // 执行回调
                onComplete?.Invoke(true);

                Debug.Log($"[AdSystem] [EDITOR] Ad simulation completed successfully for: {entryName}");
            }

            _currentPlayingEntry = null;
        }

        /// <summary>
        /// 编辑器测试方法
        /// </summary>
        [ContextMenu("Test Level Complete Ad")]
        private void TestLevelCompleteAd()
        {
            PlayAd(AdEntryNames.LEVEL_COMPLETE, (success) =>
            {
                Debug.Log($"Test ad result - Success: {success}");
            });
        }

        [ContextMenu("Test Daily Task Ad")]
        private void TestDailyTaskAd()
        {
            PlayAd(AdEntryNames.DAILY_TASK_REWARD, (success) =>
            {
                Debug.Log($"Test ad result - Success: {success}");
            });
        }
#endif
    }
}