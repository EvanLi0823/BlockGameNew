// 活动系统 - 角标UI组件
// 创建日期: 2026-03-09

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using BlockPuzzleGameToolkit.Scripts.Activity.Core;

namespace BlockPuzzleGameToolkit.Scripts.Activity.UI
{
    /// <summary>
    /// 活动角标UI组件
    /// 职责: 角标显示、动画、点击交互
    /// 大小: 150x150px
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class ActivityIcon : MonoBehaviour
    {
        #region Serialized Fields

        [Header("UI组件")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Button button;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("可选组件")]
        [SerializeField] private GameObject redDot;
        [SerializeField] private TextMeshProUGUI countdownText;

        [Header("动画配置")]
        [SerializeField] private float showDuration = 0.3f;
        [SerializeField] private float hideDuration = 0.2f;
        [SerializeField] private Ease showEase = Ease.OutBack;
        [SerializeField] private Ease hideEase = Ease.InBack;

        #endregion

        #region Private Fields

        private string activityId;
        private bool isProcessing = false;
        private float lastClickTime = 0f;
        private const float CLICK_COOLDOWN = 1.0f; // 1秒冷却

        private Sequence showSequence;
        private Sequence hideSequence;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            // 自动查找组件
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            if (button == null)
            {
                button = GetComponent<Button>();
            }

            if (iconImage == null)
            {
                iconImage = GetComponent<Image>();
            }

            // 设置按钮事件
            if (button != null)
            {
                button.onClick.AddListener(OnClick);
            }
        }

        private void OnDestroy()
        {
            // 清理按钮事件
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
            }

            // 停止所有动画并释放资源
            if (showSequence != null)
            {
                showSequence.Kill(true); // 参数true会完成动画并调用回调
                showSequence = null;
            }
            if (hideSequence != null)
            {
                hideSequence.Kill(true);
                hideSequence = null;
            }

            // 停止所有协程
            StopAllCoroutines();

            ActivityLogger.Log(activityId, "ActivityIcon已销毁");
        }

        #endregion

        #region Initialization

        /// <summary>
        /// 初始化角标
        /// </summary>
        public virtual void Initialize(string id)
        {
            activityId = id;
            ActivityLogger.Log(activityId, "ActivityIcon初始化");
        }

        #endregion

        #region Show/Hide Animation

        /// <summary>
        /// 显示角标（放大淡入动画）
        /// </summary>
        public void Show(bool animated = true)
        {
            // 停止之前的动画
            hideSequence?.Kill();

            if (!animated)
            {
                // 无动画，直接显示
                canvasGroup.alpha = 1f;
                transform.localScale = Vector3.one;
                gameObject.SetActive(true);
                ActivityLogger.Log(activityId, "Show (无动画)");
                return;
            }

            // 初始状态
            canvasGroup.alpha = 0f;
            transform.localScale = Vector3.zero;
            gameObject.SetActive(true);

            // DOTween动画序列
            showSequence = DOTween.Sequence();
            showSequence.Append(canvasGroup.DOFade(1f, showDuration));
            showSequence.Join(transform.DOScale(Vector3.one, showDuration).SetEase(showEase));
            showSequence.OnComplete(() =>
            {
                ActivityLogger.Log(activityId, "Show动画完成");
            });

            ActivityLogger.Log(activityId, "Show (放大淡入)");
        }

        /// <summary>
        /// 隐藏角标（缩小淡出动画）
        /// </summary>
        public void Hide(bool animated = true, Action onComplete = null)
        {
            // 停止之前的动画
            showSequence?.Kill();

            if (!animated)
            {
                // 无动画，直接隐藏
                gameObject.SetActive(false);
                onComplete?.Invoke();
                ActivityLogger.Log(activityId, "Hide (无动画)");
                return;
            }

            // DOTween动画序列
            hideSequence = DOTween.Sequence();
            hideSequence.Append(canvasGroup.DOFade(0f, hideDuration));
            hideSequence.Join(transform.DOScale(Vector3.zero, hideDuration).SetEase(hideEase));
            hideSequence.OnComplete(() =>
            {
                gameObject.SetActive(false);
                onComplete?.Invoke();
                ActivityLogger.Log(activityId, "Hide动画完成");
            });

            ActivityLogger.Log(activityId, "Hide (缩小淡出)");
        }

        #endregion

        #region Click Handling

        /// <summary>
        /// 按钮点击处理
        /// </summary>
        private void OnClick()
        {
            // 防重复点击
            if (isProcessing)
            {
                ActivityLogger.Log(activityId, "点击被忽略：正在处理中");
                return;
            }

            // 冷却时间检查
            float timeSinceLastClick = Time.time - lastClickTime;
            if (timeSinceLastClick < CLICK_COOLDOWN)
            {
                float remainingTime = CLICK_COOLDOWN - timeSinceLastClick;
                ActivityLogger.Log(activityId, $"点击被忽略：冷却中 ({remainingTime:F1}秒)");
                return;
            }

            // 通过检查
            isProcessing = true;
            lastClickTime = Time.time;

            ActivityLogger.Log(activityId, "按钮点击");

            // 播放点击反馈动画
            PlayClickFeedback();

            // 触发点击事件
            ActivityEvents.TriggerActivityIconClicked(activityId);

            // 1秒后重置processing标志
            StartCoroutine(ResetProcessingFlag());
        }

        /// <summary>
        /// 重置处理标志（协程）
        /// </summary>
        private IEnumerator ResetProcessingFlag()
        {
            yield return new WaitForSeconds(CLICK_COOLDOWN);
            isProcessing = false;
        }

        /// <summary>
        /// 播放点击反馈动画（缩放弹跳）
        /// </summary>
        private void PlayClickFeedback()
        {
            transform.DOScale(Vector3.one * 0.9f, 0.05f)
                     .SetEase(Ease.InOutQuad)
                     .OnComplete(() =>
                     {
                         transform.DOScale(Vector3.one, 0.05f)
                                  .SetEase(Ease.OutBack);
                     });
        }

        #endregion

        #region Red Dot

        /// <summary>
        /// 设置红点状态
        /// </summary>
        public void SetRedDot(bool active)
        {
            if (redDot != null)
            {
                redDot.SetActive(active);
                ActivityLogger.Log(activityId, $"红点状态: {active}");
            }
        }

        /// <summary>
        /// 检查红点是否显示
        /// </summary>
        public bool IsRedDotActive()
        {
            return redDot != null && redDot.activeSelf;
        }

        #endregion

        #region Countdown Text

        /// <summary>
        /// 更新倒计时文本
        /// </summary>
        public void SetCountdownText(string text)
        {
            if (countdownText != null)
            {
                countdownText.text = text;
                countdownText.gameObject.SetActive(!string.IsNullOrEmpty(text));
                ActivityLogger.Log(activityId, $"倒计时文本: {text}");
            }
        }

        /// <summary>
        /// 隐藏倒计时文本
        /// </summary>
        public void HideCountdownText()
        {
            if (countdownText != null)
            {
                countdownText.gameObject.SetActive(false);
            }
        }

        #endregion

        #region Icon Sprite

        /// <summary>
        /// 设置图标Sprite
        /// </summary>
        public void SetIconSprite(Sprite sprite)
        {
            if (iconImage != null && sprite != null)
            {
                iconImage.sprite = sprite;
                ActivityLogger.Log(activityId, "图标Sprite已更新");
            }
        }

        #endregion

        #region Debug

#if UNITY_EDITOR
        [ContextMenu("测试显示动画")]
        private void Debug_TestShow()
        {
            Show(animated: true);
        }

        [ContextMenu("测试隐藏动画")]
        private void Debug_TestHide()
        {
            Hide(animated: true);
        }

        [ContextMenu("测试点击反馈")]
        private void Debug_TestClickFeedback()
        {
            PlayClickFeedback();
        }
#endif

        #endregion
    }
}
