// 活动系统 - 弹窗基类
// 创建日期: 2026-03-09

using System;
using UnityEngine;
using BlockPuzzleGameToolkit.Scripts.Popups;
using BlockPuzzleGameToolkit.Scripts.Activity.Core;
using BlockPuzzleGameToolkit.Scripts.Activity.Data;

namespace BlockPuzzleGameToolkit.Scripts.Activity.UI
{
    /// <summary>
    /// 活动弹窗基类
    /// 职责: 统一活动弹窗的生命周期、数据刷新、事件通知
    /// 继承自: Popup (项目通用弹窗基类)
    /// </summary>
    public abstract class ActivityPopup : Popup
    {
        #region Protected Fields

        protected ActivityConfig config;
        protected string activityId;
        protected bool isDataInitialized = false;

        #endregion

        #region Properties

        public ActivityConfig Config => config;
        public string ActivityId => activityId;
        public bool IsDataInitialized => isDataInitialized;

        #endregion

        #region Initialization

        /// <summary>
        /// 初始化活动弹窗
        /// 必须在Show之前调用
        /// </summary>
        public virtual void Initialize(ActivityConfig activityConfig)
        {
            if (activityConfig == null)
            {
                ActivityLogger.LogError("ActivityPopup", "Initialize: config为null");
                return;
            }

            config = activityConfig;
            activityId = activityConfig.ActivityId;

            ActivityLogger.Log(activityId, "ActivityPopup初始化");

            // 初始化数据
            OnInitializeData();
            isDataInitialized = true;
        }

        #endregion

        #region Lifecycle Override

        /// <summary>
        /// 弹窗显示动画完成后调用
        /// </summary>
        public override void AfterShowAnimation()
        {
            base.AfterShowAnimation();

            if (!isDataInitialized)
            {
                ActivityLogger.LogWarning(activityId, "数据未初始化就显示弹窗");
                return;
            }

            ActivityLogger.Log(activityId, "ActivityPopup显示完成");

            // 子类可重写此方法
            OnActivityShow();
        }

        /// <summary>
        /// 弹窗关闭动画完成后调用
        /// </summary>
        public override void AfterHideAnimation()
        {
            ActivityLogger.Log(activityId, "ActivityPopup关闭完成");

            // 子类可重写此方法
            OnActivityClose(result);

            base.AfterHideAnimation();
        }

        #endregion

        #region Virtual Methods

        /// <summary>
        /// 初始化数据（子类重写）
        /// 在Initialize()中调用，Show之前
        /// </summary>
        protected virtual void OnInitializeData()
        {
            // 子类实现：加载数据、初始化UI等
        }

        /// <summary>
        /// 活动弹窗显示完成（子类重写）
        /// 在AfterShowAnimation()中调用
        /// </summary>
        protected virtual void OnActivityShow()
        {
            // 子类实现：启动定时器、开始动画等
        }

        /// <summary>
        /// 活动弹窗关闭完成（子类重写）
        /// 在AfterHideAnimation()中调用
        /// </summary>
        protected virtual void OnActivityClose(EPopupResult popupResult)
        {
            // 子类实现：保存数据、停止定时器等
        }

        /// <summary>
        /// 刷新弹窗数据（子类重写）
        /// 外部可调用，用于更新显示内容
        /// </summary>
        public virtual void RefreshData()
        {
            if (!isDataInitialized)
            {
                ActivityLogger.LogWarning(activityId, "RefreshData: 数据未初始化");
                return;
            }

            ActivityLogger.Log(activityId, "RefreshData");

            // 子类实现：重新加载数据、更新UI等
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// 设置弹窗结果并关闭
        /// </summary>
        protected void CloseWithResult(EPopupResult popupResult)
        {
            result = popupResult;
            ActivityLogger.Log(activityId, $"CloseWithResult: {popupResult}");
            Close();
        }

        /// <summary>
        /// 检查配置是否有效
        /// </summary>
        protected bool ValidateConfig()
        {
            if (config == null)
            {
                ActivityLogger.LogError(activityId, "Config为null");
                return false;
            }

            if (!config.Validate())
            {
                ActivityLogger.LogError(activityId, "Config验证失败");
                return false;
            }

            return true;
        }

        #endregion

        #region Debug

#if UNITY_EDITOR
        [ContextMenu("测试刷新数据")]
        private void Debug_TestRefreshData()
        {
            RefreshData();
        }

        [ContextMenu("测试关闭(Claimed)")]
        private void Debug_TestCloseClaimed()
        {
            CloseWithResult(EPopupResult.Claimed);
        }

        [ContextMenu("测试关闭(Closed)")]
        private void Debug_TestCloseClosed()
        {
            CloseWithResult(EPopupResult.Closed);
        }
#endif

        #endregion
    }
}
