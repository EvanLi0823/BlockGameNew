// 活动系统 - Canvas边界检测
// 创建日期: 2026-03-09

using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Activity.Core
{
    /// <summary>
    /// ActivityCanvas边界检测工具
    /// 用于检测Icon是否移出Canvas可视区域
    /// </summary>
    public class ActivityCanvasBounds
    {
        #region Fields

        private RectTransform canvasRectTransform;
        private Canvas canvas;
        private Vector2 referenceResolution;

        #endregion

        #region Initialization

        /// <summary>
        /// 初始化边界检测器
        /// </summary>
        public ActivityCanvasBounds(Canvas canvas)
        {
            this.canvas = canvas;
            this.canvasRectTransform = canvas.GetComponent<RectTransform>();

            // 获取参考分辨率（从CanvasScaler）
            var canvasScaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
            if (canvasScaler != null)
            {
                referenceResolution = canvasScaler.referenceResolution;
            }
            else
            {
                // 默认使用Canvas的实际大小
                referenceResolution = canvasRectTransform.sizeDelta;
            }

            ActivityLogger.Log("ActivityCanvasBounds", $"初始化 - 参考分辨率: {referenceResolution}");
        }

        #endregion

        #region Boundary Detection

        /// <summary>
        /// 检测RectTransform是否完全移出Canvas的上方边界
        /// </summary>
        /// <param name="rectTransform">要检测的RectTransform</param>
        /// <returns>如果完全移出上方边界返回true，否则返回false</returns>
        public bool IsCompletelyAboveTop(RectTransform rectTransform)
        {
            if (rectTransform == null || canvasRectTransform == null)
            {
                return false;
            }

            // 获取Canvas的上边界（世界坐标）
            Vector3[] canvasCorners = new Vector3[4];
            canvasRectTransform.GetWorldCorners(canvasCorners);
            // canvasCorners[1] 是右上角
            float canvasTopY = canvasCorners[1].y;

            // 获取Icon的边界（世界坐标）
            Vector3[] iconCorners = new Vector3[4];
            rectTransform.GetWorldCorners(iconCorners);
            // iconCorners[0] 是左下角
            // iconCorners[1] 是右下角（Y值最小）
            float iconBottomY = iconCorners[0].y;

            // 判断Icon的底部是否高于Canvas的顶部
            bool isAbove = iconBottomY > canvasTopY;

            if (isAbove)
            {
                ActivityLogger.Log("ActivityCanvasBounds",
                    $"Icon完全移出上方边界 - Icon底部Y:{iconBottomY:F2}, Canvas顶部Y:{canvasTopY:F2}");
            }

            return isAbove;
        }

        /// <summary>
        /// 检测RectTransform是否部分移出Canvas的上方边界
        /// </summary>
        public bool IsPartiallyAboveTop(RectTransform rectTransform)
        {
            if (rectTransform == null || canvasRectTransform == null)
            {
                return false;
            }

            // 获取Canvas的上边界（世界坐标）
            Vector3[] canvasCorners = new Vector3[4];
            canvasRectTransform.GetWorldCorners(canvasCorners);
            float canvasTopY = canvasCorners[1].y;

            // 获取Icon的中心点
            Vector3[] iconCorners = new Vector3[4];
            rectTransform.GetWorldCorners(iconCorners);
            // iconCorners[1] 是右上角（Y值最大）
            float iconTopY = iconCorners[1].y;

            // 判断Icon的顶部是否高于Canvas的顶部
            return iconTopY > canvasTopY;
        }

        /// <summary>
        /// 获取Canvas的参考分辨率
        /// </summary>
        public Vector2 GetReferenceResolution()
        {
            return referenceResolution;
        }

        /// <summary>
        /// 获取Canvas的上边界Y坐标（本地坐标）
        /// </summary>
        public float GetTopBoundaryLocalY()
        {
            // Canvas参考分辨率的一半就是上边界
            return referenceResolution.y / 2f;
        }

        /// <summary>
        /// 获取Canvas的下边界Y坐标（本地坐标）
        /// </summary>
        public float GetBottomBoundaryLocalY()
        {
            return -referenceResolution.y / 2f;
        }

        /// <summary>
        /// 获取Canvas的左边界X坐标（本地坐标）
        /// </summary>
        public float GetLeftBoundaryLocalX()
        {
            return -referenceResolution.x / 2f;
        }

        /// <summary>
        /// 获取Canvas的右边界X坐标（本地坐标）
        /// </summary>
        public float GetRightBoundaryLocalX()
        {
            return referenceResolution.x / 2f;
        }

        /// <summary>
        /// 检测RectTransform是否触碰左边界（使用世界坐标，适配所有分辨率）
        /// </summary>
        /// <param name="rectTransform">要检测的RectTransform</param>
        /// <returns>如果触碰左边界返回true</returns>
        public bool IsTouchingLeftBoundary(RectTransform rectTransform)
        {
            if (rectTransform == null || canvasRectTransform == null)
            {
                return false;
            }

            // 获取Canvas的左边界（世界坐标）
            Vector3[] canvasCorners = new Vector3[4];
            canvasRectTransform.GetWorldCorners(canvasCorners);
            // canvasCorners[0] 是左下角
            float canvasLeftX = canvasCorners[0].x;

            // 获取Icon的左边界（世界坐标）
            Vector3[] iconCorners = new Vector3[4];
            rectTransform.GetWorldCorners(iconCorners);
            // iconCorners[0] 是左下角
            float iconLeftX = iconCorners[0].x;

            // 判断Icon的左边是否碰到或超出Canvas的左边
            return iconLeftX <= canvasLeftX;
        }

        /// <summary>
        /// 检测RectTransform是否触碰右边界（使用世界坐标，适配所有分辨率）
        /// </summary>
        /// <param name="rectTransform">要检测的RectTransform</param>
        /// <returns>如果触碰右边界返回true</returns>
        public bool IsTouchingRightBoundary(RectTransform rectTransform)
        {
            if (rectTransform == null || canvasRectTransform == null)
            {
                return false;
            }

            // 获取Canvas的右边界（世界坐标）
            Vector3[] canvasCorners = new Vector3[4];
            canvasRectTransform.GetWorldCorners(canvasCorners);
            // canvasCorners[2] 是右上角
            float canvasRightX = canvasCorners[2].x;

            // 获取Icon的右边界（世界坐标）
            Vector3[] iconCorners = new Vector3[4];
            rectTransform.GetWorldCorners(iconCorners);
            // iconCorners[2] 是右上角
            float iconRightX = iconCorners[2].x;

            // 判断Icon的右边是否碰到或超出Canvas的右边
            return iconRightX >= canvasRightX;
        }

        #endregion

        #region Debug

        public string GetDebugInfo()
        {
            return $"ReferenceResolution: {referenceResolution}, " +
                   $"TopY: {GetTopBoundaryLocalY():F2}, BottomY: {GetBottomBoundaryLocalY():F2}, " +
                   $"LeftX: {GetLeftBoundaryLocalX():F2}, RightX: {GetRightBoundaryLocalX():F2}";
        }

        #endregion
    }
}
