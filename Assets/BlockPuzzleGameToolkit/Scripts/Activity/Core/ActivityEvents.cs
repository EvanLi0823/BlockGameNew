// 活动系统 - 事件定义
// 创建日期: 2026-03-09

using System;
using BlockPuzzleGameToolkit.Scripts.Activity.Data;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BlockPuzzleGameToolkit.Scripts.Activity.Core
{
    /// <summary>
    /// 活动系统事件定义
    /// 用于ActivityModule与ActivityManager之间的解耦通信
    /// </summary>
    public static class ActivityEvents
    {
        /// <summary>
        /// 活动需要显示（由ActivityModule触发）
        /// </summary>
        public static event Action<string> OnActivityShouldShow;

        /// <summary>
        /// 活动需要隐藏（由ActivityModule触发）
        /// </summary>
        public static event Action<string> OnActivityShouldHide;

        /// <summary>
        /// 活动角标被点击（由ActivityIcon触发）
        /// </summary>
        public static event Action<string> OnActivityIconClicked;

        /// <summary>
        /// 请求打开活动弹窗（由ActivityModule触发）
        /// </summary>
        public static event Action<string, string> OnRequestOpenPopup; // activityId, popupPath

        /// <summary>
        /// 活动弹窗已关闭（由弹窗系统触发）
        /// </summary>
        public static event Action<string> OnActivityPopupClosed;

        /// <summary>
        /// 请求刷新活动（由ActivityModule或外部系统触发）
        /// </summary>
        public static event Action<string, EActivityRefreshEvent> OnRequestRefreshActivity; // activityId, refreshEvent

        /// <summary>
        /// 请求刷新所有活动（由ActivityManager触发）
        /// </summary>
        public static event Action<EActivityRefreshEvent> OnRequestRefreshAll;

        /// <summary>
        /// 场景切换事件（由ActivityManager触发）
        /// </summary>
        public static event Action<bool> OnSceneChange; // isEntering (true=进入场景, false=离开场景)

        #region Trigger Methods

        public static void TriggerActivityShouldShow(string activityId)
        {
            OnActivityShouldShow?.Invoke(activityId);
        }

        public static void TriggerActivityShouldHide(string activityId)
        {
            OnActivityShouldHide?.Invoke(activityId);
        }

        public static void TriggerActivityIconClicked(string activityId)
        {
            OnActivityIconClicked?.Invoke(activityId);
        }

        public static void TriggerRequestOpenPopup(string activityId, string popupPath)
        {
            OnRequestOpenPopup?.Invoke(activityId, popupPath);
        }

        public static void TriggerActivityPopupClosed(string activityId)
        {
            OnActivityPopupClosed?.Invoke(activityId);
        }

        public static void TriggerRequestRefreshActivity(string activityId, EActivityRefreshEvent refreshEvent)
        {
            OnRequestRefreshActivity?.Invoke(activityId, refreshEvent);
        }

        public static void TriggerRequestRefreshAll(EActivityRefreshEvent refreshEvent)
        {
            OnRequestRefreshAll?.Invoke(refreshEvent);
        }

        public static void TriggerSceneChange(bool isEntering)
        {
            OnSceneChange?.Invoke(isEntering);
        }

        #endregion

        #region Clear Methods

        /// <summary>
        /// 清理所有事件订阅（用于避免内存泄漏）
        /// </summary>
        public static void ClearAllEvents()
        {
            OnActivityShouldShow = null;
            OnActivityShouldHide = null;
            OnActivityIconClicked = null;
            OnRequestOpenPopup = null;
            OnActivityPopupClosed = null;
            OnRequestRefreshActivity = null;
            OnRequestRefreshAll = null;
            OnSceneChange = null;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor模式下自动清理事件（避免PlayMode切换时的内存泄漏）
        /// </summary>
        [InitializeOnLoadMethod]
        private static void RegisterEditorCleanup()
        {
            EditorApplication.playModeStateChanged += (state) =>
            {
                if (state == PlayModeStateChange.ExitingPlayMode)
                {
                    ClearAllEvents();
                    ActivityLogger.Log("ActivityEvents", "Editor模式：PlayMode退出时自动清理所有事件");
                }
            };
        }
#endif

        #endregion
    }
}
