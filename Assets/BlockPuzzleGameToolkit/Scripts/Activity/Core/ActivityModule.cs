// 活动系统 - 模块基类
// 创建日期: 2026-03-09

using System.Collections;
using BlockPuzzleGameToolkit.Scripts.Activity.Data;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Activity.Core
{
    /// <summary>
    /// 活动模块抽象基类
    /// 职责: 纯逻辑层，不直接操作UI，通过事件通知Manager
    /// 所有具体活动模块必须继承此类
    /// </summary>
    public abstract class ActivityModule
    {
        #region Fields

        protected ActivityConfig config;
        protected bool isInitialized = false;
        private bool lastCanShowState = false;

        #endregion

        #region Properties

        public string ActivityId => config?.ActivityId;
        public bool IsInitialized => isInitialized;

        #endregion

        #region Lifecycle

        /// <summary>
        /// 初始化活动模块
        /// </summary>
        public virtual void Initialize(ActivityConfig activityConfig)
        {
            if (activityConfig == null)
            {
                ActivityLogger.LogError("ActivityModule", "Initialize: config为null");
                return;
            }

            config = activityConfig;
            isInitialized = true;

            ActivityLogger.Log("ActivityModule", $"Initialize: {config.ActivityId}");

            // 初始化后立即检查显示条件
            CheckAndNotifyVisibilityChange();
        }

        /// <summary>
        /// 刷新活动数据
        /// 子类重写此方法以更新活动特定数据
        /// </summary>
        public virtual void OnRefresh(EActivityRefreshEvent refreshEvent)
        {
            if (!isInitialized)
            {
                return;
            }

            ActivityLogger.Log(ActivityId, $"OnRefresh: {refreshEvent}");

            // 刷新后重新检查显示条件
            CheckAndNotifyVisibilityChange();
        }

        /// <summary>
        /// 每帧更新（子类可选重写）
        /// 用于需要实时更新的活动（如漂浮泡泡的运动）
        /// </summary>
        public virtual void OnUpdate(float deltaTime)
        {
            // 默认空实现，子类按需重写
        }

        /// <summary>
        /// 销毁活动模块
        /// </summary>
        public virtual void OnDestroy()
        {
            ActivityLogger.Log(ActivityId, "OnDestroy");
            isInitialized = false;
            config = null;
        }

        #endregion

        #region Visibility Logic

        /// <summary>
        /// 检查活动是否应该显示
        /// 子类必须实现此方法
        /// </summary>
        public abstract bool CanShow();

        /// <summary>
        /// 检查并通知显示条件变化
        /// </summary>
        protected void CheckAndNotifyVisibilityChange()
        {
            if (!isInitialized)
            {
                return;
            }

            bool currentCanShow = CanShow();

            // 状态发生变化时触发事件
            if (currentCanShow != lastCanShowState)
            {
                lastCanShowState = currentCanShow;

                if (currentCanShow)
                {
                    ActivityLogger.Log(ActivityId, "CanShow: true → 触发显示事件");
                    ActivityEvents.TriggerActivityShouldShow(config.ActivityId);
                }
                else
                {
                    ActivityLogger.Log(ActivityId, "CanShow: false → 触发隐藏事件");
                    ActivityEvents.TriggerActivityShouldHide(config.ActivityId);
                }
            }
        }

        #endregion

        #region User Interaction

        /// <summary>
        /// 用户点击活动角标
        /// 默认实现：使用约定规则打开弹窗（Activity/{ActivityName}/{ActivityName}Popup）
        /// 子类可重写此方法以自定义弹窗打开逻辑（例如：根据条件打开不同弹窗）
        /// </summary>
        public virtual void OnIconClicked()
        {
            if (!isInitialized)
            {
                ActivityLogger.LogWarning(ActivityId, "OnIconClicked: 模块未初始化");
                return;
            }

            ActivityLogger.Log(ActivityId, "OnIconClicked");

            // 默认使用约定规则：Activity/{ActivityName}/{ActivityName}Popup
            string popupPath = $"Activity/{config.ActivityName}/{config.ActivityName}Popup";

            // 请求打开弹窗
            ActivityEvents.TriggerRequestOpenPopup(config.ActivityId, popupPath);
        }

        /// <summary>
        /// 活动弹窗关闭回调
        /// 子类可重写此方法以添加特定逻辑
        /// </summary>
        public virtual void OnPopupClosed()
        {
            if (!isInitialized)
            {
                return;
            }

            ActivityLogger.Log(ActivityId, "OnPopupClosed");

            // 弹窗关闭后触发刷新
            ActivityEvents.TriggerRequestRefreshActivity(config.ActivityId, EActivityRefreshEvent.PopupClosed);
        }

        #endregion

        #region Coroutine Support

        /// <summary>
        /// 启动协程（通过ActivityManager代理）
        /// ActivityModule不是MonoBehaviour，需要通过Manager启动协程
        /// </summary>
        protected Coroutine StartCoroutine(IEnumerator routine)
        {
            var manager = ActivityManager.Instance;
            if (manager == null)
            {
                LogError("StartCoroutine: ActivityManager未找到");
                return null;
            }

            return manager.StartActivityCoroutine(routine);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// 日志输出（使用统一Logger）
        /// </summary>
        protected void Log(string message)
        {
            ActivityLogger.Log(ActivityId, message);
        }

        protected void LogWarning(string message)
        {
            ActivityLogger.LogWarning(ActivityId, message);
        }

        protected void LogError(string message)
        {
            ActivityLogger.LogError(ActivityId, message);
        }

        #endregion
    }
}
