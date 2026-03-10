// 活动系统 - 核心管理器
// 创建日期: 2026-03-09

using System;
using System.Collections.Generic;
using UnityEngine;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using BlockPuzzleGameToolkit.Scripts.Activity.Data;
using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.Popups;
using BlockPuzzleGameToolkit.Scripts.Activity.Examples; // 示例活动模块

namespace BlockPuzzleGameToolkit.Scripts.Activity.Core
{
    /// <summary>
    /// 活动系统核心管理器
    /// 职责: 协调活动系统，处理事件，管理活动模块生命周期
    /// </summary>
    public class ActivityManager : SingletonBehaviour<ActivityManager>
    {
        #region Fields

        private ActivitySettings settings;
        private ActivityCanvasProvider canvasProvider;
        private ActivityIconManager iconManager;

        // 已注册的活动模块 (activityId -> ActivityModule)
        private Dictionary<string, ActivityModule> registeredActivities = new Dictionary<string, ActivityModule>();

        // 活动模块工厂注册表 (activityName -> Factory)
        private static Dictionary<string, Func<ActivityModule>> moduleFactories = new Dictionary<string, Func<ActivityModule>>();

        private bool isProcessingRefresh = false;

        // 保存委托引用，避免Lambda表达式导致的内存泄漏
        private Action onLevelCompletedHandler;
        private Action onCurrencyChangedHandler;
        private Action onGameSceneReadyHandler;

        #endregion

        #region Module Factory

        /// <summary>
        /// 手动注册所有活动模块
        /// 在OnInit()中调用，在加载配置之前
        /// </summary>
        private void RegisterAllModules()
        {
            ActivityLogger.Log("ActivityManager", "开始注册活动模块");

            // 在这里手动调用各个活动模块的Register()方法
            // 示例：
            FloatBubbleActivity.Register();

            // 添加更多活动模块的注册...
            // MyActivity.Register();
            // AnotherActivity.Register();

            ActivityLogger.Log("ActivityManager", $"活动模块注册完成，总数: {moduleFactories.Count}");
        }

        /// <summary>
        /// 注册活动模块（静态方法，供各个活动模块调用）
        /// </summary>
        /// <param name="activityName">活动名称（需与配置中的ActivityName一致）</param>
        /// <param name="createFunc">创建模块实例的委托</param>
        public static void RegisterModule(string activityName, Func<ActivityModule> createFunc)
        {
            if (string.IsNullOrEmpty(activityName))
            {
                ActivityLogger.LogError("ActivityManager", "RegisterModule: activityName为空");
                return;
            }

            if (createFunc == null)
            {
                ActivityLogger.LogError("ActivityManager", $"RegisterModule: createFunc为null, activityName={activityName}");
                return;
            }

            moduleFactories[activityName] = createFunc;
            ActivityLogger.Log("ActivityManager", $"注册模块工厂: {activityName}");
        }

        #endregion

        #region Properties

        public override int InitPriority => 150; // 在MenuManager(100)之后初始化

        #endregion

        #region Initialization

        public override void OnInit()
        {
            if (IsInitialized)
            {
                ActivityLogger.LogWarning("ActivityManager", "已经初始化，跳过");
                return;
            }

            ActivityLogger.Log("ActivityManager", "开始初始化");

            // ===== 手动注册所有活动模块 =====
            // 在这里调用各个活动模块的Register()方法
            RegisterAllModules();

            // 加载配置
            settings = ActivitySettings.Instance;
            if (settings == null)
            {
                ActivityLogger.LogError("ActivityManager", "ActivitySettings未找到");
                return;
            }

            // 检查系统开关
            if (!settings.EnableActivitySystem)
            {
                ActivityLogger.Log("ActivityManager", "活动系统已禁用");
                return;
            }

            // 验证配置
            if (!settings.ValidateAllConfigs())
            {
                ActivityLogger.LogError("ActivityManager", "配置验证失败");
                return;
            }

            // 初始化CanvasProvider
            canvasProvider = new ActivityCanvasProvider();
            if (!canvasProvider.Initialize(settings.ActivityCanvasName))
            {
                ActivityLogger.LogError("ActivityManager", "CanvasProvider初始化失败");
                return;
            }

            // 初始化IconManager
            iconManager = new ActivityIconManager();
            if (!iconManager.Initialize(canvasProvider))
            {
                ActivityLogger.LogError("ActivityManager", "IconManager初始化失败");
                return;
            }

            // 注册所有活动模块
            RegisterAllActivities();

            // 订阅事件
            SubscribeEvents();

            // 设置DontDestroyOnLoad
            DontDestroyOnLoad(gameObject);

            base.OnInit(); // 设置IsInitialized = true

            ActivityLogger.Log("ActivityManager", "初始化完成");

            // ===== 注意：不在初始化时刷新活动 =====
            // Icon显示应该在Loading结束、关卡加载完成后
            // 由外部（LoadingManager）调用 ShowAllActivityIcons() 触发
        }

        #endregion

        #region Activity Registration

        /// <summary>
        /// 注册所有活动模块
        /// </summary>
        private void RegisterAllActivities()
        {
            var configs = settings.GetEnabledConfigs();

            ActivityLogger.Log("ActivityManager", $"开始注册活动，数量: {configs.Count}");

            foreach (var config in configs)
            {
                RegisterActivity(config);
            }

            ActivityLogger.Log("ActivityManager", $"活动注册完成，成功: {registeredActivities.Count}/{configs.Count}");
        }

        /// <summary>
        /// 注册单个活动模块
        /// </summary>
        private bool RegisterActivity(ActivityConfig config)
        {
            if (config == null)
            {
                ActivityLogger.LogError("ActivityManager", "RegisterActivity: config为null");
                return false;
            }

            // 检查是否已注册
            if (registeredActivities.ContainsKey(config.ActivityId))
            {
                ActivityLogger.LogWarning("ActivityManager", $"活动已注册: {config.ActivityId}");
                return false;
            }

            // 创建ActivityModule实例（使用工厂）
            ActivityModule module = CreateActivityModule(config);
            if (module == null)
            {
                ActivityLogger.LogError("ActivityManager", $"创建ActivityModule失败: {config.ActivityId}");
                return false;
            }

            // 初始化模块
            module.Initialize(config);

            // 加入注册表
            registeredActivities[config.ActivityId] = module;

            ActivityLogger.Log("ActivityManager", $"注册活动: {config.ActivityId}");
            return true;
        }

        /// <summary>
        /// 创建ActivityModule实例
        /// 使用工厂模式，不再使用反射
        /// </summary>
        private ActivityModule CreateActivityModule(ActivityConfig config)
        {
            string activityName = config.ActivityName;

            if (string.IsNullOrEmpty(activityName))
            {
                ActivityLogger.LogError("ActivityManager", $"ActivityName为空: {config.ActivityId}");
                return null;
            }

            // 从工厂注册表中查找
            if (!moduleFactories.TryGetValue(activityName, out var createFunc))
            {
                ActivityLogger.LogError("ActivityManager", $"未注册的活动模块: {activityName}");
                ActivityLogger.LogError("ActivityManager", $"请在ActivityManager.OnInit()之前调用 {activityName}Activity.Register()");
                return null;
            }

            try
            {
                // 使用工厂创建实例
                ActivityModule module = createFunc();

                if (module == null)
                {
                    ActivityLogger.LogError("ActivityManager", $"工厂返回null: {activityName}");
                    return null;
                }

                ActivityLogger.Log("ActivityManager", $"创建模块成功: {activityName}");
                return module;
            }
            catch (Exception e)
            {
                ActivityLogger.LogError("ActivityManager", $"创建实例失败: {activityName}, {e.Message}");
                return null;
            }
        }

        #endregion

        #region Event Subscription

        /// <summary>
        /// 订阅事件
        /// </summary>
        private void SubscribeEvents()
        {
            // 活动模块事件
            ActivityEvents.OnActivityShouldShow += HandleActivityShouldShow;
            ActivityEvents.OnActivityShouldHide += HandleActivityShouldHide;
            ActivityEvents.OnActivityIconClicked += HandleActivityIconClicked;
            ActivityEvents.OnRequestOpenPopup += HandleRequestOpenPopup;
            ActivityEvents.OnActivityPopupClosed += HandleActivityPopupClosed;
            ActivityEvents.OnRequestRefreshActivity += HandleRequestRefreshActivity;

            // 游戏事件（触发刷新）- 保存委托引用以便正确取消订阅
            onLevelCompletedHandler = () => RefreshAllActivities(EActivityRefreshEvent.LevelCompleted);
            onCurrencyChangedHandler = () => RefreshAllActivities(EActivityRefreshEvent.CurrencyChanged);
            onGameSceneReadyHandler = () => ShowAllActivityIcons();

            EventManager.GetEvent(EGameEvent.LevelCompleted).Subscribe(onLevelCompletedHandler);
            EventManager.GetEvent(EGameEvent.CurrencyChanged).Subscribe(onCurrencyChangedHandler);
            EventManager.GetEvent(EGameEvent.GameSceneReady).Subscribe(onGameSceneReadyHandler);

            ActivityLogger.Log("ActivityManager", "事件订阅完成");
        }

        /// <summary>
        /// 取消订阅事件
        /// </summary>
        private void UnsubscribeEvents()
        {
            // 活动模块事件
            ActivityEvents.OnActivityShouldShow -= HandleActivityShouldShow;
            ActivityEvents.OnActivityShouldHide -= HandleActivityShouldHide;
            ActivityEvents.OnActivityIconClicked -= HandleActivityIconClicked;
            ActivityEvents.OnRequestOpenPopup -= HandleRequestOpenPopup;
            ActivityEvents.OnActivityPopupClosed -= HandleActivityPopupClosed;
            ActivityEvents.OnRequestRefreshActivity -= HandleRequestRefreshActivity;

            // 游戏事件 - 使用保存的委托引用进行取消订阅
            if (onLevelCompletedHandler != null)
            {
                EventManager.GetEvent(EGameEvent.LevelCompleted)?.Unsubscribe(onLevelCompletedHandler);
                onLevelCompletedHandler = null;
            }

            if (onCurrencyChangedHandler != null)
            {
                EventManager.GetEvent(EGameEvent.CurrencyChanged)?.Unsubscribe(onCurrencyChangedHandler);
                onCurrencyChangedHandler = null;
            }

            if (onGameSceneReadyHandler != null)
            {
                EventManager.GetEvent(EGameEvent.GameSceneReady)?.Unsubscribe(onGameSceneReadyHandler);
                onGameSceneReadyHandler = null;
            }

            ActivityLogger.Log("ActivityManager", "事件取消订阅完成");
        }

        #endregion

        #region Event Handlers

        private void HandleActivityShouldShow(string activityId)
        {
            ActivityLogger.LogEvent("ActivityShouldShow", activityId);

            var config = settings.GetConfig(activityId);
            if (config == null)
            {
                ActivityLogger.LogError("ActivityManager", $"配置未找到: {activityId}");
                return;
            }

            iconManager.ShowIcon(config);
        }

        private void HandleActivityShouldHide(string activityId)
        {
            ActivityLogger.LogEvent("ActivityShouldHide", activityId);
            iconManager.HideIcon(activityId, animated: true);
        }

        private void HandleActivityIconClicked(string activityId)
        {
            ActivityLogger.LogEvent("ActivityIconClicked", activityId);

            if (!registeredActivities.TryGetValue(activityId, out var module))
            {
                ActivityLogger.LogWarning("ActivityManager", $"活动未注册: {activityId}");
                return;
            }

            module.OnIconClicked();
        }

        private void HandleRequestOpenPopup(string activityId, string popupPath)
        {
            ActivityLogger.LogEvent("RequestOpenPopup", $"{activityId} | {popupPath}");

            // 调用MenuManager加载弹窗
            var menuManager = MenuManager.Instance;
            if (menuManager == null)
            {
                ActivityLogger.LogError("ActivityManager", "MenuManager未找到");
                return;
            }

            // 显示弹窗，监听关闭事件
            menuManager.ShowPopup(popupPath, onShow: null, onClose: (result) =>
            {
                // 弹窗关闭时触发ActivityPopupClosed事件
                ActivityEvents.TriggerActivityPopupClosed(activityId);
            });
        }

        private void HandleActivityPopupClosed(string activityId)
        {
            ActivityLogger.LogEvent("ActivityPopupClosed", activityId);

            if (!registeredActivities.TryGetValue(activityId, out var module))
            {
                return;
            }

            module.OnPopupClosed();
        }

        private void HandleRequestRefreshActivity(string activityId, EActivityRefreshEvent refreshEvent)
        {
            ActivityLogger.LogEvent("RequestRefreshActivity", $"{activityId} | {refreshEvent}");

            if (!registeredActivities.TryGetValue(activityId, out var module))
            {
                return;
            }

            module.OnRefresh(refreshEvent);
        }

        #endregion

        #region Refresh

        /// <summary>
        /// 刷新所有活动
        /// </summary>
        public void RefreshAllActivities(EActivityRefreshEvent refreshEvent)
        {
            if (!IsInitialized)
            {
                return;
            }

            // 防止重复刷新
            if (isProcessingRefresh)
            {
                ActivityLogger.LogWarning("ActivityManager", "正在刷新中，跳过");
                return;
            }

            isProcessingRefresh = true;

            ActivityLogger.Log("ActivityManager", $"刷新所有活动: {refreshEvent}");

            foreach (var kvp in registeredActivities)
            {
                kvp.Value.OnRefresh(refreshEvent);
            }

            isProcessingRefresh = false;
        }

        #endregion

        #region Update Loop

        /// <summary>
        /// 每帧更新，调用所有活动模块的OnUpdate
        /// </summary>
        private void Update()
        {
            if (!IsInitialized)
            {
                return;
            }

            float deltaTime = Time.deltaTime;

            foreach (var kvp in registeredActivities)
            {
                kvp.Value?.OnUpdate(deltaTime);
            }
        }

        #endregion

        #region Coroutine Proxy

        /// <summary>
        /// 启动协程（供ActivityModule调用）
        /// ActivityModule不是MonoBehaviour，无法直接启动协程
        /// </summary>
        public Coroutine StartActivityCoroutine(System.Collections.IEnumerator routine)
        {
            if (routine == null)
            {
                ActivityLogger.LogError("ActivityManager", "StartActivityCoroutine: routine为null");
                return null;
            }

            return StartCoroutine(routine);
        }

        #endregion

        #region Scene Lifecycle

        private void OnEnable()
        {
            if (!IsInitialized)
            {
                return;
            }

            ActivityLogger.Log("ActivityManager", "OnEnable - 场景进入");

            // 触发场景切换事件
            ActivityEvents.TriggerSceneChange(isEntering: true);

            // 重新刷新所有活动
            RefreshAllActivities(EActivityRefreshEvent.SceneChanged);
        }

        private void OnDisable()
        {
            if (!IsInitialized)
            {
                return;
            }

            ActivityLogger.Log("ActivityManager", "OnDisable - 场景离开");

            // 触发场景切换事件
            ActivityEvents.TriggerSceneChange(isEntering: false);

            // 隐藏所有角标
            iconManager?.HideAllIcons(animated: false);
        }

        private void OnDestroy()
        {
            ActivityLogger.Log("ActivityManager", "OnDestroy");

            // 取消事件订阅
            UnsubscribeEvents();

            // 销毁所有活动模块
            foreach (var kvp in registeredActivities)
            {
                kvp.Value?.OnDestroy();
            }
            registeredActivities.Clear();

            // 清理IconManager
            iconManager?.Cleanup();

            // 清理CanvasProvider
            canvasProvider?.Cleanup();

            // 清理事件
            ActivityEvents.ClearAllEvents();
        }

        #endregion

        #region Public API

        /// <summary>
        /// 显示所有活动Icon
        /// 应该在Loading结束、关卡加载完成后调用
        /// </summary>
        public void ShowAllActivityIcons()
        {
            if (!IsInitialized)
            {
                ActivityLogger.LogWarning("ActivityManager", "ShowAllActivityIcons: 未初始化");
                return;
            }

            ActivityLogger.Log("ActivityManager", "开始显示所有活动Icon");

            // 刷新所有活动（会触发Icon显示）
            RefreshAllActivities(EActivityRefreshEvent.UserAction);
        }

        /// <summary>
        /// 手动刷新活动（供外部调用）
        /// </summary>
        public void RequestRefresh()
        {
            RefreshAllActivities(EActivityRefreshEvent.UserAction);
        }

        /// <summary>
        /// 获取活动模块
        /// </summary>
        public ActivityModule GetActivityModule(string activityId)
        {
            registeredActivities.TryGetValue(activityId, out var module);
            return module;
        }

        /// <summary>
        /// 检查活动是否已注册
        /// </summary>
        public bool IsActivityRegistered(string activityId)
        {
            return registeredActivities.ContainsKey(activityId);
        }

        /// <summary>
        /// 获取IconManager（供ActivityModule内部使用）
        /// </summary>
        internal ActivityIconManager GetIconManager()
        {
            return iconManager;
        }

        #endregion

        #region Debug

#if UNITY_EDITOR
        [Header("调试信息")]
        [SerializeField] private bool showDebugInfo = true;

        private void OnGUI()
        {
            if (!showDebugInfo || !IsInitialized)
            {
                return;
            }

            GUILayout.BeginArea(new Rect(10, 300, 400, 300));
            GUILayout.Label("=== ActivitySystem Debug ===");
            GUILayout.Label($"Registered: {registeredActivities.Count}");
            GUILayout.Label($"IconManager: {iconManager?.GetDebugInfo()}");

            if (GUILayout.Button("刷新所有活动"))
            {
                RequestRefresh();
            }

            GUILayout.EndArea();
        }
#endif

        #endregion
    }
}
