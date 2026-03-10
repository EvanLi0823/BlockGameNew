// 活动系统 - Canvas提供者
// 创建日期: 2026-03-09

using UnityEngine;
using BlockPuzzleGameToolkit.Scripts.Activity.Data;

namespace BlockPuzzleGameToolkit.Scripts.Activity.Core
{
    /// <summary>
    /// ActivityCanvas引用和容器管理
    /// 职责: 查找和维护ActivityCanvas引用，直接使用ActivityCanvas作为Icon容器
    /// </summary>
    public class ActivityCanvasProvider
    {
        #region Fields

        private Transform activityCanvas;
        private Canvas canvas;
        private bool isInitialized = false;

        #endregion

        #region Properties

        public bool IsInitialized => isInitialized;
        public Transform ActivityCanvas => activityCanvas;

        #endregion

        #region Initialization

        /// <summary>
        /// 初始化Canvas引用
        /// </summary>
        public bool Initialize(string canvasName)
        {
            if (isInitialized)
            {
                ActivityLogger.LogWarning("ActivityCanvasProvider", "已经初始化，跳过");
                return true;
            }

            // 查找ActivityCanvas
            GameObject canvasObject = GameObject.Find(canvasName);
            if (canvasObject == null)
            {
                ActivityLogger.LogError("ActivityCanvasProvider", $"未找到ActivityCanvas: {canvasName}");
                return false;
            }

            activityCanvas = canvasObject.transform;
            canvas = canvasObject.GetComponent<Canvas>();

            if (canvas == null)
            {
                ActivityLogger.LogError("ActivityCanvasProvider", $"ActivityCanvas缺少Canvas组件: {canvasName}");
                return false;
            }

            ActivityLogger.Log("ActivityCanvasProvider", $"找到ActivityCanvas: {canvasName}");

            isInitialized = true;
            ActivityLogger.Log("ActivityCanvasProvider", "初始化完成");
            return true;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 获取目标容器
        /// 所有Icon都添加到ActivityCanvas下
        /// </summary>
        public Transform GetTargetContainer()
        {
            if (!isInitialized)
            {
                ActivityLogger.LogError("ActivityCanvasProvider", "GetTargetContainer: 未初始化");
                return null;
            }

            // 直接返回ActivityCanvas作为容器
            return activityCanvas;
        }

        /// <summary>
        /// 获取Canvas组件
        /// </summary>
        public Canvas GetCanvas()
        {
            if (!isInitialized)
            {
                ActivityLogger.LogError("ActivityCanvasProvider", "GetCanvas: 未初始化");
                return null;
            }

            return canvas;
        }

        /// <summary>
        /// 验证容器是否有效
        /// </summary>
        public bool ValidateContainers()
        {
            if (!isInitialized)
            {
                return false;
            }

            bool isValid = activityCanvas != null;

            if (!isValid)
            {
                ActivityLogger.LogWarning("ActivityCanvasProvider", "ActivityCanvas引用已失效");
            }

            return isValid;
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// 清理引用
        /// </summary>
        public void Cleanup()
        {
            activityCanvas = null;
            canvas = null;
            isInitialized = false;

            ActivityLogger.Log("ActivityCanvasProvider", "Cleanup完成");
        }

        #endregion
    }
}
