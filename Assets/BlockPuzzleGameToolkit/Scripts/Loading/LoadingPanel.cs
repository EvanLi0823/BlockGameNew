// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BlockPuzzleGameToolkit.Scripts.Loading
{
    /// <summary>
    /// 假Loading界面控制脚本
    /// 仅用于显示加载动画，不实际加载资源
    /// </summary>
    public class LoadingPanel : MonoBehaviour
    {
        [Header("UI组件")]
        [SerializeField] private GameObject loadingContainer;
        [SerializeField] private Image progressBar;  // 使用Image的fillAmount作为进度条
        [SerializeField] private TextMeshProUGUI progressText;  // 进度百分比文本
        [SerializeField] private Image loadingIcon;

        [Header("加载配置")]
        [SerializeField] private float loadingDuration = 3f;  // 加载持续时间
        [SerializeField] private AnimationCurve loadingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);  // 加载曲线

        [Header("动画设置")]
        [SerializeField] private bool animateLoadingIcon = true;
        [SerializeField] private float iconRotationSpeed = 180f;  // 图标旋转速度

        [Header("文本格式")]
        [SerializeField] private string progressFormat = "{0}%";  // 进度文本格式
        [SerializeField] private bool showProgressText = true;  // 是否显示进度文本

        private Coroutine loadingCoroutine;
        private Coroutine iconAnimationCoroutine;
        private Action onLoadingComplete;
        private WaitForSeconds completionDelay;  // 缓存WaitForSeconds

        #region 生命周期

        private void Awake()
        {
            // 缓存WaitForSeconds
            completionDelay = new WaitForSeconds(0.3f);

            // 不再在Awake中隐藏，让StateManager通过状态控制显示
            // loadingContainer的显示/隐藏应该由StateManager的Loading状态控制

            // 初始化进度条
            if (progressBar != null)
            {
                progressBar.type = Image.Type.Filled;
                progressBar.fillMethod = Image.FillMethod.Horizontal;
                progressBar.fillOrigin = (int)Image.OriginHorizontal.Left;
                progressBar.fillAmount = 0f;
            }

            // 初始化进度文本
            if (progressText != null && showProgressText)
            {
                progressText.text = string.Format(progressFormat, 0);
            }
        }

        private void Start()
        {
            // 在Start时初始化进度为0
            InitializeProgress();
        }

        private void OnDestroy()
        {
            StopLoading();
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 初始化进度显示
        /// </summary>
        public void InitializeProgress()
        {
            SetProgress(0);
        }

        /// <summary>
        /// 显示Loading界面
        /// </summary>
        /// <param name="duration">持续时间（秒）</param>
        /// <param name="onComplete">完成回调</param>
        public void ShowLoading(float duration = 0, Action onComplete = null)
        {
            if (loadingCoroutine != null)
            {
                Debug.LogWarning("[LoadingPanel] Loading already in progress");
                return;
            }

            // 使用默认或指定的持续时间
            float actualDuration = duration > 0 ? duration : loadingDuration;
            onLoadingComplete = onComplete;

            // 注意：不再控制loadingContainer的显示
            // 显示/隐藏应该由StateManager的状态切换控制

            // 初始化
            SetProgress(0);

            // 开始加载动画
            loadingCoroutine = StartCoroutine(FakeLoadingProcess(actualDuration));

            // 开始图标动画
            if (animateLoadingIcon && loadingIcon != null)
            {
                iconAnimationCoroutine = StartCoroutine(AnimateLoadingIcon());
            }
        }

        /// <summary>
        /// 隐藏Loading界面
        /// </summary>
        public void HideLoading()
        {
            StopLoading();

            // 注意：不再控制loadingContainer的显示
            // 显示/隐藏应该由StateManager的状态切换控制
        }

        /// <summary>
        /// 停止Loading
        /// </summary>
        public void StopLoading()
        {
            if (loadingCoroutine != null)
            {
                StopCoroutine(loadingCoroutine);
                loadingCoroutine = null;
            }

            if (iconAnimationCoroutine != null)
            {
                StopCoroutine(iconAnimationCoroutine);
                iconAnimationCoroutine = null;
            }
        }

        #endregion

        #region 私有方法

        private IEnumerator FakeLoadingProcess(float duration)
        {
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float normalizedTime = elapsedTime / duration;

                // 使用曲线调整进度
                float progress = loadingCurve.Evaluate(normalizedTime);
                SetProgress(progress);

                yield return null;
            }

            // 确保进度到100%
            SetProgress(1f);

            // 等待一小段时间
            yield return completionDelay;

            // 执行完成回调
            onLoadingComplete?.Invoke();

            // 清理
            loadingCoroutine = null;
            HideLoading();
        }

        private void SetProgress(float progress)
        {
            progress = Mathf.Clamp01(progress);

            // 更新进度条
            if (progressBar != null)
                progressBar.fillAmount = progress;

            // 更新进度文本
            if (progressText != null && showProgressText)
            {
                int percentage = Mathf.RoundToInt(progress * 100);
                progressText.text = string.Format(progressFormat, percentage);
            }
        }

        private IEnumerator AnimateLoadingIcon()
        {
            if (loadingIcon == null) yield break;

            while (loadingContainer != null && loadingContainer.activeSelf)
            {
                loadingIcon.transform.Rotate(Vector3.forward, -iconRotationSpeed * Time.deltaTime);
                yield return null;
            }
        }

        #endregion

        #region 编辑器验证

        #if UNITY_EDITOR
        private void OnValidate()
        {
            // 验证进度条Image设置
            if (progressBar != null)
            {
                // 确保Image使用Filled类型
                if (progressBar.type != Image.Type.Filled)
                {
                    Debug.LogWarning("[LoadingPanel] Progress Bar Image should be set to 'Filled' type for proper progress display");
                }
            }

            // 验证进度文本格式
            if (string.IsNullOrEmpty(progressFormat))
            {
                progressFormat = "{0}%";
            }

            // 验证加载时长
            if (loadingDuration < 0.1f)
            {
                loadingDuration = 0.1f;
            }
        }
        #endif

        #endregion
    }
}