// 活动系统 - 浮动气泡角标组件
// 创建日期: 2026-03-09

using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using BlockPuzzleGameToolkit.Scripts.Activity.UI;
using BlockPuzzleGameToolkit.Scripts.Activity.Core;
using BlockPuzzleGameToolkit.Scripts.CurrencySystem;

namespace BlockPuzzleGameToolkit.Scripts.Activity.Examples
{
    /// <summary>
    /// 浮动气泡活动角标组件
    /// 继承自ActivityIcon，添加FloatBubble特定的UI组件引用
    /// 在角标加载完成后动态添加到GameObject上
    /// </summary>
    public class FloatBubbleIcon : ActivityIcon
    {
        #region UI Component References

        private SkeletonGraphic spineBg;          // Spine背景动画
        private Text txtReward;                    // 奖励文本
        private Text txtMultiple;                  // 倍率文本（"x2"）
        private Text txtX;                         // "x"文本

        #endregion

        #region Reward Data

        // 缓存计算出的奖励数值
        private int cachedBaseReward = 0;
        private float cachedAdMultiplier = 0f;

        #endregion

        #region Floating Configuration (从FloatingBubbleSettings加载)

        // 运动参数（从FloatingBubbleSettings加载）
        private float verticalSpeed;
        private float horizontalSpeed;
        private Vector2 horizontalBounds;
        private float disappearYRatio;

        #endregion

        #region Floating State

        private RectTransform rectTransform;
        private bool isFloating = false;
        private ActivityCanvasBounds canvasBounds;

        // 运动状态
        private float horizontalDirection = 1f; // 1=右，-1=左
        private bool isInitialPositionSet = false;

        // 边界缓存
        private float leftBoundary;
        private float rightBoundary;
        private float topBoundary;

        #endregion

        #region Initialization

        /// <summary>
        /// 初始化：自动查找所有UI组件
        /// </summary>
        public override void Initialize(string id)
        {
            ActivityLogger.Log("FloatBubbleIcon", $"Initialize被调用 - id:{id}");
            base.Initialize(id);

            // 获取RectTransform引用
            rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                ActivityLogger.LogError("FloatBubbleIcon", "未找到RectTransform组件");
            }

            // 查找FloatBubble特定的UI组件
            ActivityLogger.Log("FloatBubbleIcon", "准备调用FindUIComponents");
            FindUIComponents();

            ActivityLogger.Log("FloatBubbleIcon", "组件初始化完成");
        }

        /// <summary>
        /// 设置边界检测器
        /// </summary>
        public void SetCanvasBounds(ActivityCanvasBounds bounds)
        {
            canvasBounds = bounds;

            // 计算边界
            if (canvasBounds != null)
            {
                CalculateBoundaries();
                ActivityLogger.Log("FloatBubbleIcon", "边界检测器已设置并计算边界");
            }
        }

        /// <summary>
        /// 从FloatingBubbleSettings加载配置参数
        /// </summary>
        public void LoadSettings(FloatingBubbleSettings settings)
        {
            if (settings == null)
            {
                ActivityLogger.LogWarning("FloatBubbleIcon", "LoadSettings: settings为null");
                return;
            }

            verticalSpeed = settings.VerticalSpeed;
            horizontalSpeed = settings.HorizontalSpeed;
            horizontalBounds = settings.HorizontalBounds;
            disappearYRatio = settings.DisappearYRatio;

            ActivityLogger.Log("FloatBubbleIcon", $"加载配置完成 - 垂直速度:{verticalSpeed}, 水平速度:{horizontalSpeed}");

            // 重新计算边界（因为 horizontalBounds 和 disappearYRatio 已更新）
            CalculateBoundaries();
        }

        /// <summary>
        /// 计算边界
        /// </summary>
        private void CalculateBoundaries()
        {
            if (canvasBounds == null || rectTransform == null)
            {
                return;
            }

            Vector2 referenceResolution = canvasBounds.GetReferenceResolution();
            float canvasWidth = referenceResolution.x;
            float canvasHeight = referenceResolution.y;

            // 计算水平边界（相对Canvas中心）
            leftBoundary = (-canvasWidth / 2f) + (canvasWidth * horizontalBounds.x);
            rightBoundary = (-canvasWidth / 2f) + (canvasWidth * horizontalBounds.y);

            // 计算顶部边界
            topBoundary = (canvasHeight / 2f) * disappearYRatio;

            ActivityLogger.Log("FloatBubbleIcon", $"边界计算完成 - Left:{leftBoundary:F1}, Right:{rightBoundary:F1}, Top:{topBoundary:F1}");
        }

        /// <summary>
        /// 查找UI组件
        /// </summary>
        private void FindUIComponents()
        {
            ActivityLogger.Log("FloatBubbleIcon", "FindUIComponents开始执行");
            Transform root = transform;
            ActivityLogger.Log("FloatBubbleIcon", $"root transform: {root.name}, 子物体数量: {root.childCount}");

            // 查找spine_bg（Spine背景动画）
            Transform spineBgTransform = root.Find("spine_bg");
            if (spineBgTransform != null)
            {
                spineBg = spineBgTransform.GetComponent<SkeletonGraphic>();
                if (spineBg != null)
                {
                    ActivityLogger.Log("FloatBubbleIcon", "找到Spine背景组件");
                }
                else
                {
                    ActivityLogger.LogWarning("FloatBubbleIcon", "spine_bg节点缺少SkeletonGraphic组件");
                }
            }
            else
            {
                ActivityLogger.LogWarning("FloatBubbleIcon", "未找到spine_bg节点");
            }

            // 查找txt_reward（奖励文本）
            Transform txtRewardTransform = root.Find("txt_reward");
            ActivityLogger.Log("FloatBubbleIcon", $"查找txt_reward结果: {(txtRewardTransform != null ? "找到" : "null")}");
            if (txtRewardTransform != null)
            {
                txtReward = txtRewardTransform.GetComponent<Text>();
                if (txtReward != null)
                {
                    ActivityLogger.Log("FloatBubbleIcon", "找到奖励文本组件");
                }
                else
                {
                    ActivityLogger.LogWarning("FloatBubbleIcon", "txt_reward节点缺少Text组件");
                }
            }
            else
            {
                ActivityLogger.LogWarning("FloatBubbleIcon", "未找到txt_reward节点");
            }

            // 查找txt_multiple（倍率文本）
            Transform txtMultipleTransform = root.Find("txt_multiple");
            ActivityLogger.Log("FloatBubbleIcon", $"查找txt_multiple结果: {(txtMultipleTransform != null ? "找到" : "null")}");
            if (txtMultipleTransform != null)
            {
                txtMultiple = txtMultipleTransform.GetComponent<Text>();
                if (txtMultiple != null)
                {
                    ActivityLogger.Log("FloatBubbleIcon", "找到倍率文本组件");
                }

                // 查找txt_multiple的子节点txt_x
                Transform txtXTransform = txtMultipleTransform.Find("txt_x");
                ActivityLogger.Log("FloatBubbleIcon", $"查找txt_x结果: {(txtXTransform != null ? "找到" : "null")}");
                if (txtXTransform != null)
                {
                    txtX = txtXTransform.GetComponent<Text>();
                    if (txtX != null)
                    {
                        ActivityLogger.Log("FloatBubbleIcon", "找到x文本组件");
                    }
                }
            }
            else
            {
                ActivityLogger.LogWarning("FloatBubbleIcon", "未找到txt_multiple节点");
            }

            ActivityLogger.Log("FloatBubbleIcon", "FindUIComponents执行完成");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 更新奖励显示
        /// </summary>
        public void UpdateRewardDisplay(int baseReward, float adMultiplier)
        {
            ActivityLogger.Log("FloatBubbleIcon", $"[开始] UpdateRewardDisplay - baseReward:{baseReward}, adMultiplier:{adMultiplier}");
            ActivityLogger.Log("FloatBubbleIcon", $"[状态] txtReward:{(txtReward != null ? "存在" : "null")}, txtMultiple:{(txtMultiple != null ? "存在" : "null")}, txtX:{(txtX != null ? "存在" : "null")}");

            // 缓存奖励数值
            cachedBaseReward = baseReward;
            cachedAdMultiplier = adMultiplier;

            // 更新奖励文本（使用CurrencySystem的统一格式化接口）
            if (txtReward != null)
            {
                var currencyManager = CurrencyManager.Instance;
                if (currencyManager != null)
                {
                    // 使用CurrencyManager的格式化方法，自动处理汇率和货币符号
                    string formattedText = currencyManager.FormatCurrency(baseReward, useExchangeRate: true);
                    txtReward.text = formattedText;
                    ActivityLogger.Log("FloatBubbleIcon", $"[txtReward] 设置文本: {formattedText}");
                }
                else
                {
                    // 备用：直接显示数值（除以10000）
                    string formattedText = (baseReward / 10000f).ToString("F2");
                    txtReward.text = formattedText;
                    ActivityLogger.LogWarning("FloatBubbleIcon", $"[txtReward] CurrencyManager未找到，使用备用格式: {formattedText}");
                }
                txtReward.gameObject.SetActive(true);
                ActivityLogger.Log("FloatBubbleIcon", $"[txtReward] 激活状态: active={txtReward.gameObject.activeInHierarchy}, enabled={txtReward.enabled}");
            }
            else
            {
                ActivityLogger.LogError("FloatBubbleIcon", "[txtReward] txtReward为null，无法更新");
            }

            // 更新倍率文本
            if (txtMultiple != null)
            {
                txtMultiple.text = adMultiplier.ToString("F0");
                txtMultiple.gameObject.SetActive(true);
                ActivityLogger.Log("FloatBubbleIcon", $"[txtMultiple] 设置文本: {adMultiplier.ToString("F0")}");
            }
            else
            {
                ActivityLogger.LogError("FloatBubbleIcon", "[txtMultiple] txtMultiple为null，无法更新");
            }

            // 确保"x"文本显示
            if (txtX != null)
            {
                txtX.text = "x";
                txtX.gameObject.SetActive(true);
                ActivityLogger.Log("FloatBubbleIcon", $"[txtX] 设置文本: x");
            }
            else
            {
                ActivityLogger.LogError("FloatBubbleIcon", "[txtX] txtX为null，无法更新");
            }

            ActivityLogger.Log("FloatBubbleIcon", $"[完成] UpdateRewardDisplay - 缓存值: baseReward={cachedBaseReward}, adMultiplier={cachedAdMultiplier}");
        }

        /// <summary>
        /// 获取缓存的基础奖励
        /// </summary>
        public int GetCachedBaseReward()
        {
            return cachedBaseReward;
        }

        /// <summary>
        /// 获取缓存的广告倍率
        /// </summary>
        public float GetCachedAdMultiplier()
        {
            return cachedAdMultiplier;
        }

        /// <summary>
        /// 播放Spine动画
        /// </summary>
        public void PlaySpineAnimation(string animationName, bool loop = true)
        {
            if (spineBg == null)
            {
                ActivityLogger.LogWarning("FloatBubbleIcon", "Spine组件未找到，无法播放动画");
                return;
            }

            try
            {
                var animationState = spineBg.AnimationState;
                if (animationState != null)
                {
                    animationState.SetAnimation(0, animationName, loop);
                    ActivityLogger.Log("FloatBubbleIcon", $"播放Spine动画: {animationName}, 循环:{loop}");
                }
            }
            catch (System.Exception e)
            {
                ActivityLogger.LogError("FloatBubbleIcon", $"播放Spine动画失败: {e.Message}");
            }
        }

        /// <summary>
        /// 设置Spine皮肤
        /// </summary>
        public void SetSpineSkin(string skinName)
        {
            if (spineBg == null)
            {
                ActivityLogger.LogWarning("FloatBubbleIcon", "Spine组件未找到，无法设置皮肤");
                return;
            }

            try
            {
                var skeleton = spineBg.Skeleton;
                if (skeleton != null)
                {
                    skeleton.SetSkin(skinName);
                    skeleton.SetSlotsToSetupPose();
                    ActivityLogger.Log("FloatBubbleIcon", $"设置Spine皮肤: {skinName}");
                }
            }
            catch (System.Exception e)
            {
                ActivityLogger.LogError("FloatBubbleIcon", $"设置Spine皮肤失败: {e.Message}");
            }
        }

        #endregion

        #region Floating Control

        /// <summary>
        /// Unity Update回调 - 处理飘动逻辑（左右反弹+垂直上升）
        /// </summary>
        private void Update()
        {
            if (!isFloating)
            {
                return;
            }

            // 检查必要组件
            if (rectTransform == null || canvasBounds == null)
            {
                return;
            }

            // 设置初始位置（第一次更新时）
            if (!isInitialPositionSet)
            {
                SetInitialPosition();
                isInitialPositionSet = true;
            }

            // 计算位移
            float deltaTime = Time.deltaTime;
            float verticalDelta = verticalSpeed * deltaTime;
            float horizontalDelta = horizontalSpeed * horizontalDirection * deltaTime;

            Vector2 currentPos = rectTransform.anchoredPosition;
            Vector2 newPosition = currentPos;
            newPosition.y += verticalDelta;
            newPosition.x += horizontalDelta;

            // 左右边界反弹
            if (newPosition.x <= leftBoundary)
            {
                newPosition.x = leftBoundary;
                horizontalDirection = 1f; // 向右
            }
            else if (newPosition.x >= rightBoundary)
            {
                newPosition.x = rightBoundary;
                horizontalDirection = -1f; // 向左
            }

            // 顶部消失（检测Icon底部是否超过边界，确保整个Icon完全移出）
            float iconHeight = rectTransform.rect.height;
            float iconBottomY = newPosition.y - (iconHeight / 2f);
            if (iconBottomY >= topBoundary)
            {
                ActivityLogger.Log("FloatBubbleIcon", "Icon完全移出上方边界，开始隐藏");
                StopFloating();

                // 通知ActivityManager隐藏此Icon（触发Activity的冷却流程）
                Hide(animated: false, onComplete: () =>
                {
                    ActivityLogger.Log("FloatBubbleIcon", "Icon已隐藏并回收");
                });
                return;
            }

            // 更新位置
            rectTransform.anchoredPosition = newPosition;
        }

        /// <summary>
        /// 设置初始位置（底部中心）
        /// </summary>
        private void SetInitialPosition()
        {
            if (rectTransform == null || canvasBounds == null)
            {
                return;
            }

            Vector2 referenceResolution = canvasBounds.GetReferenceResolution();
            float canvasHeight = referenceResolution.y;

            // 设置初始位置（底部中心）
            Vector2 startPosition = new Vector2(0f, -canvasHeight / 2f + 100f);
            rectTransform.anchoredPosition = startPosition;

            // 随机初始方向
            horizontalDirection = Random.value > 0.5f ? 1f : -1f;

            ActivityLogger.Log("FloatBubbleIcon", $"初始位置设置完成 - 起点:{startPosition}, 方向:{horizontalDirection}");
        }

        /// <summary>
        /// 开始向上飘动
        /// </summary>
        public void StartFloating()
        {
            if (isFloating)
            {
                ActivityLogger.LogWarning("FloatBubbleIcon", "已在飘动中，忽略StartFloating");
                return;
            }

            if (rectTransform == null)
            {
                ActivityLogger.LogError("FloatBubbleIcon", "RectTransform为null，无法开始飘动");
                return;
            }

            if (canvasBounds == null)
            {
                ActivityLogger.LogWarning("FloatBubbleIcon", "边界检测器未设置，飘动可能无法自动隐藏");
            }

            isFloating = true;
            isInitialPositionSet = false; // 重置初始位置标志

            ActivityLogger.Log("FloatBubbleIcon", $"开始飘动 - 垂直速度:{verticalSpeed}, 水平速度:{horizontalSpeed}");
        }

        /// <summary>
        /// 停止飘动
        /// </summary>
        public void StopFloating()
        {
            if (!isFloating)
            {
                return;
            }

            isFloating = false;
            ActivityLogger.Log("FloatBubbleIcon", "停止飘动");
        }

        /// <summary>
        /// 检查是否正在飘动
        /// </summary>
        public bool IsFloating()
        {
            return isFloating;
        }

        #endregion

        #region Debug

#if UNITY_EDITOR
        [ContextMenu("测试 - 更新奖励显示")]
        private void TestUpdateRewardDisplay()
        {
            UpdateRewardDisplay(1000000, 3);
        }

        [ContextMenu("测试 - 播放Spine动画")]
        private void TestPlaySpineAnimation()
        {
            PlaySpineAnimation("animation", true);
        }
#endif

        #endregion
    }
}
