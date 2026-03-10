// 通用奖励弹窗
// 创建日期: 2026-03-06
// 用途: 通用的奖励领取弹窗，支持单倍/多倍领取、飞币动画、广告集成

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BlockPuzzleGameToolkit.Scripts.CurrencySystem;
using BlockPuzzleGameToolkit.Scripts.FlyRewardSystem.Core;
using BlockPuzzle.AdSystem.Managers;
using BlockPuzzleGameToolkit.Scripts.Localization;

namespace BlockPuzzleGameToolkit.Scripts.Popups
{
    /// <summary>
    /// 通用奖励弹窗（广告奖励）
    /// 职责:
    /// - 显示基础奖励金额和广告倍率
    /// - 提供单倍领取和广告多倍领取按钮
    /// - 集成广告系统
    /// - 自动播放飞币动画（可选）
    /// - 通过配置对象初始化，解耦具体业务逻辑
    /// </summary>
    [RequireComponent(typeof(Animator), typeof(CanvasGroup))]
    public class CommonRewardPopup : Popup
    {
        #region Serialized Fields

        [Header("Reward Display")]
        [SerializeField] private Text baseRewardText;          // 基础奖励显示
        [SerializeField] private Text adMultiplierText;        // 广告倍率显示（如"x3"）

        [Header("Buttons")]
        [SerializeField] private Button singleClaimButton;     // 单倍领取按钮
        [SerializeField] private Button adClaimButton;         // 广告多倍领取按钮

        [Header("Button Labels")]
        [SerializeField] private TextMeshProUGUI singleClaimButtonText;  // 单倍按钮文本
        [SerializeField] private Text adClaimButtonText;      // 广告按钮文本

        #endregion

        #region Private Fields

        private RewardPopupConfig config;           // 配置对象
        private bool isProcessingClick = false;     // 防止重复点击

        #endregion

        #region Initialization

        /// <summary>
        /// 初始化弹窗（通过配置对象）
        /// </summary>
        public void Initialize(RewardPopupConfig popupConfig)
        {
            if (popupConfig == null)
            {
                Debug.LogError("[CommonRewardPopup] 配置对象为null");
                Close();
                return;
            }

            // 验证配置
            if (!popupConfig.Validate())
            {
                Debug.LogError("[CommonRewardPopup] 配置验证失败");
                Close();
                return;
            }

            config = popupConfig;

            // 更新UI显示
            UpdateDisplay();

            // 设置按钮事件
            SetupButtons();

            Debug.Log($"[CommonRewardPopup] 初始化 - 基础奖励: {config.BaseReward}, 广告倍率: {config.AdMultiplier}");
        }

        protected override void Awake()
        {
            base.Awake();

            // 自动查找UI组件（如果Inspector未配置）
            if (baseRewardText == null)
            {
                baseRewardText = transform.Find("BaseRewardText")?.GetComponent<Text>();
            }

            if (adMultiplierText == null)
            {
                adMultiplierText = transform.Find("AdMultiplierText")?.GetComponent<Text>();
            }

            if (singleClaimButton == null)
            {
                singleClaimButton = transform.Find("SingleClaimButton")?.GetComponent<Button>();
            }

            if (adClaimButton == null)
            {
                adClaimButton = transform.Find("AdClaimButton")?.GetComponent<Button>();
            }

            if (singleClaimButtonText == null && singleClaimButton != null)
            {
                singleClaimButtonText = singleClaimButton.GetComponentInChildren<TextMeshProUGUI>();
            }

            if (adClaimButtonText == null && adClaimButton != null)
            {
                adClaimButtonText = adClaimButton.GetComponentInChildren<Text>();
            }
        }

        #endregion

        #region UI Update

        /// <summary>
        /// 更新显示
        /// </summary>
        private void UpdateDisplay()
        {
            // 更新基础奖励文本
            if (baseRewardText != null)
            {
                if (CurrencyManager.Instance != null)
                {
                    string formattedReward = CurrencyManager.Instance.FormatCurrency(config.BaseReward, true);
                    baseRewardText.text = formattedReward;
                }
                else
                {
                    float dollarValue = config.BaseReward / 10000f;
                    baseRewardText.text = $"${dollarValue:F3}";
                }
            }

            // 更新广告倍率文本
            if (adMultiplierText != null)
            {
                adMultiplierText.text = $"{config.AdMultiplier:F0}";
            }

            // 更新按钮文本
            UpdateButtonTexts();

            // 控制单倍领奖按钮的显示/隐藏
            // 当 NoAdMultiplier <= 0 时，隐藏单倍领奖按钮
            if (singleClaimButton != null)
            {
                bool shouldShowSingleButton = config.NoAdMultiplier > 0f;
                singleClaimButton.gameObject.SetActive(shouldShowSingleButton);
                Debug.Log($"[CommonRewardPopup] 单倍领奖按钮显示状态: {shouldShowSingleButton} (NoAdMultiplier={config.NoAdMultiplier})");
            }
        }

        /// <summary>
        /// 更新按钮文本
        /// </summary>
        private void UpdateButtonTexts()
        {
            // 单倍按钮：显示"Claim {0}%"格式的多语言文本
            if (singleClaimButtonText != null)
            {
                string localizedFormat = LocalizationManager.GetText("ClaimPer", "Claim ({0}%)");
                float noAdMultiplierPercentage = config.NoAdMultiplier * 100f;
                singleClaimButtonText.text = string.Format(localizedFormat, noAdMultiplierPercentage.ToString("F0"));
            }

            // 广告按钮：显示多倍奖励
            if (adClaimButtonText != null)
            {
                int adReward = Mathf.RoundToInt(config.BaseReward * config.AdMultiplier);
                if (CurrencyManager.Instance != null)
                {
                    string formattedReward = CurrencyManager.Instance.FormatCurrency(adReward, true);
                    adClaimButtonText.text = formattedReward;
                }
                else
                {
                    float dollarValue = adReward / 10000f;
                    adClaimButtonText.text = $"${dollarValue:F3}";
                }
            }
        }

        #endregion

        #region Button Setup

        /// <summary>
        /// 设置按钮事件
        /// </summary>
        private void SetupButtons()
        {
            if (singleClaimButton != null)
            {
                singleClaimButton.onClick.RemoveAllListeners();
                singleClaimButton.onClick.AddListener(OnSingleClaimClicked);
            }

            if (adClaimButton != null)
            {
                adClaimButton.onClick.RemoveAllListeners();
                adClaimButton.onClick.AddListener(OnAdClaimClicked);
            }
        }

        #endregion

        #region Button Handlers

        /// <summary>
        /// 单倍领取按钮点击
        /// </summary>
        private void OnSingleClaimClicked()
        {
            // 防重复点击
            if (isProcessingClick)
            {
                Debug.Log("[CommonRewardPopup] 正在处理中，忽略重复点击");
                return;
            }

            isProcessingClick = true;

            // 禁用按钮
            DisableButtons();

            Debug.Log("[CommonRewardPopup] 单倍领取");

            // 计算不看广告的实际奖励（应用倍率）
            int finalReward = Mathf.RoundToInt(config.BaseReward * config.NoAdMultiplier);

            // 回调通知（单倍领取总是成功）
            var result = RewardClaimResult.CreateSingleSuccess(finalReward);
            config.OnRewardClaimed?.Invoke(result);

            // 播放飞币动画（如果配置启用）
            if (config.AutoPlayFlyAnimation)
            {
                // 延迟关闭弹窗，等待飞币动画完成
                PlayFlyAnimation(() =>
                {
                    Close();
                });
            }
            else
            {
                // 没有动画，立即关闭
                Close();
            }
        }

        /// <summary>
        /// 广告多倍领取按钮点击
        /// </summary>
        private void OnAdClaimClicked()
        {
            // 防重复点击
            if (isProcessingClick)
            {
                Debug.Log("[CommonRewardPopup] 正在处理中，忽略重复点击");
                return;
            }

            isProcessingClick = true;

            // 禁用按钮
            DisableButtons();

            Debug.Log("[CommonRewardPopup] 广告多倍领取");

            // 播放激励广告
            ShowRewardedAd((success) =>
            {
                // 检查对象是否存在（防止广告期间弹窗被销毁）
                if (this == null)
                {
                    Debug.LogWarning("[CommonRewardPopup] 广告回调时弹窗对象已被销毁");
                    return;
                }

                if (success)
                {
                    // 广告成功，计算多倍奖励
                    int finalReward = Mathf.RoundToInt(config.BaseReward * config.AdMultiplier);
                    Debug.Log($"[CommonRewardPopup] 广告成功，播放数值滚动动画: {config.BaseReward} -> {finalReward}");

                    // 播放奖励文本滚动动画
                    StartRewardTextAnimation(config.BaseReward, finalReward, () =>
                    {
                        // 检查对象是否存在
                        if (this == null)
                        {
                            Debug.LogWarning("[CommonRewardPopup] 动画回调时弹窗对象已被销毁");
                            return;
                        }

                        // 数值滚动完成后，回调通知发放奖励
                        var result = RewardClaimResult.CreateAdSuccess(finalReward);
                        config.OnRewardClaimed?.Invoke(result);

                        // 播放飞币动画（如果配置启用）
                        if (config.AutoPlayFlyAnimation)
                        {
                            // 延迟关闭弹窗，等待飞币动画完成
                            PlayFlyAnimation(() =>
                            {
                                Close();
                            });
                        }
                        else
                        {
                            // 没有动画，立即关闭
                            Close();
                        }
                    });
                }
                else
                {
                    // 广告失败，不发放奖励
                    Debug.LogWarning("[CommonRewardPopup] 广告失败");

                    // 通知调用者广告失败
                    var result = RewardClaimResult.CreateAdFailed();
                    config.OnRewardClaimed?.Invoke(result);

                    // 关闭弹窗
                    Close();
                }
            });
        }

        /// <summary>
        /// 禁用所有按钮
        /// </summary>
        private void DisableButtons()
        {
            if (singleClaimButton != null)
            {
                singleClaimButton.interactable = false;
            }

            if (adClaimButton != null)
            {
                adClaimButton.interactable = false;
            }
        }

        #endregion

        #region Reward Text Animation

        /// <summary>
        /// 播放奖励文本滚动动画
        /// </summary>
        /// <param name="from">起始值</param>
        /// <param name="to">目标值</param>
        /// <param name="onComplete">动画完成回调</param>
        private void StartRewardTextAnimation(float from, float to, Action onComplete = null)
        {
            if (baseRewardText == null)
            {
                // 如果文本不存在，直接执行回调
                onComplete?.Invoke();
                return;
            }

            // 开始新的动画
            StartCoroutine(AnimateRewardText(from, to, onComplete));
        }

        /// <summary>
        /// 奖励文本动画协程
        /// </summary>
        private IEnumerator AnimateRewardText(float from, float to, Action onComplete)
        {
            float duration = 1.0f; // 动画时长
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // 使用缓动函数
                float easedT = Mathf.SmoothStep(0, 1, t);
                float currentValue = Mathf.Lerp(from, to, easedT);

                // 更新文本 - 使用CurrencyManager格式化
                if (baseRewardText != null)
                {
                    if (CurrencyManager.Instance != null)
                    {
                        // 将浮点值转为整数（因为内部使用放大10000倍的整数）
                        int intValue = Mathf.RoundToInt(currentValue);
                        baseRewardText.text = CurrencyManager.Instance.FormatCurrency(intValue, true);
                    }
                    else
                    {
                        // 备用格式化
                        float dollarValue = currentValue / 10000f;
                        baseRewardText.text = $"${dollarValue:F3}";
                    }
                }

                yield return null;
            }

            // 确保最终值精确
            if (baseRewardText != null)
            {
                if (CurrencyManager.Instance != null)
                {
                    int finalIntValue = Mathf.RoundToInt(to);
                    baseRewardText.text = CurrencyManager.Instance.FormatCurrency(finalIntValue, true);
                }
                else
                {
                    float finalDollarValue = to / 10000f;
                    baseRewardText.text = $"${finalDollarValue:F3}";
                }
            }

            // 动画完成，执行回调
            onComplete?.Invoke();
        }

        #endregion

        #region Fly Animation

        /// <summary>
        /// 播放飞币动画
        /// </summary>
        /// <param name="onComplete">动画完成回调</param>
        private void PlayFlyAnimation(Action onComplete = null)
        {
            Debug.Log($"[CommonRewardPopup] PlayFlyAnimation 被调用");

            if (!config.AutoPlayFlyAnimation)
            {
                Debug.Log($"[CommonRewardPopup] AutoPlayFlyAnimation=false，跳过动画");
                onComplete?.Invoke();
                return;
            }

            var flyRewardManager = FlyRewardManager.Instance;
            if (flyRewardManager == null)
            {
                Debug.LogWarning("[CommonRewardPopup] FlyRewardManager未找到，跳过飞币动画");
                onComplete?.Invoke();
                return;
            }

            Debug.Log($"[CommonRewardPopup] FlyRewardManager已找到，IsInitialized={flyRewardManager.IsInitialized}");

            // 确定起点位置（配置指定 或 使用弹窗的世界坐标位置）
            Vector3 startPos = config.FlyStartPosition ?? transform.position;

            Debug.Log($"[CommonRewardPopup] 准备播放飞币动画: count={config.FlyingCoinCount}, startPos={startPos}");

            // 创建飞行请求
            var request = FlyRewardSystem.Data.FlyRewardRequest.CreateCoinRequest(startPos, config.FlyingCoinCount);
            request.rewardAmount = 0; // 金额在回调中处理，避免重复发放
            request.onComplete = null; // 不使用飞行完成回调

            // 播放飞币动画
            flyRewardManager.PlayFlyAnimation(request);

            Debug.Log($"[CommonRewardPopup] PlayFlyAnimation 已调用");

            // 在第一阶段动画结束后关闭弹窗（烟花散开阶段完成）
            // 第一阶段占总时长的30%（默认2秒 * 0.3 = 0.6秒）
            float firstStageDelay = request.duration * 0.3f;
            StartCoroutine(DelayedClosePopup(firstStageDelay, onComplete));
        }

        /// <summary>
        /// 延迟关闭弹窗协程
        /// </summary>
        private IEnumerator DelayedClosePopup(float delay, Action onComplete)
        {
            Debug.Log($"[CommonRewardPopup] 将在{delay}秒后关闭弹窗");
            yield return new WaitForSeconds(delay);
            Debug.Log($"[CommonRewardPopup] 延迟时间到，关闭弹窗");
            onComplete?.Invoke();
        }

        #endregion

        #region Ad Integration

        /// <summary>
        /// 显示激励广告
        /// </summary>
        private void ShowRewardedAd(Action<bool> callback)
        {
            // 检查广告入口名称
            if (string.IsNullOrEmpty(config.AdEntryName))
            {
                Debug.LogWarning("[CommonRewardPopup] 广告入口名称未配置");
                callback?.Invoke(false);
                return;
            }

            // 调用AdSystemManager播放广告
            var adManager = AdSystemManager.Instance;
            if (adManager == null)
            {
                Debug.LogError("[CommonRewardPopup] AdSystemManager未找到");
                callback?.Invoke(false);
                return;
            }

            Debug.Log($"[CommonRewardPopup] 播放激励广告: {config.AdEntryName}");

            adManager.PlayAd(config.AdEntryName, (success) =>
            {
                // 检查对象是否存在
                if (this == null)
                {
                    Debug.LogWarning("[CommonRewardPopup] 广告回调时对象已销毁");
                    return;
                }

                Debug.Log($"[CommonRewardPopup] 广告播放结果: {success}");
                callback?.Invoke(success);
            });
        }

        #endregion

        #region Override Methods

        public override void Close()
        {
            Debug.Log("[CommonRewardPopup] Close 被调用");

            // 重置标志
            isProcessingClick = false;

            // 调用弹窗关闭回调
            config?.OnPopupClosed?.Invoke();

            base.Close();
        }

        #endregion

        #region Debug

#if UNITY_EDITOR
        [ContextMenu("测试飞币动画")]
        private void Debug_TestFlyAnimation()
        {
            if (config == null)
            {
                Debug.LogError("[CommonRewardPopup] config未初始化");
                return;
            }

            Debug.Log($"[CommonRewardPopup] 开始测试飞币动画");
            Debug.Log($"[CommonRewardPopup] AutoPlayFlyAnimation={config.AutoPlayFlyAnimation}");
            Debug.Log($"[CommonRewardPopup] FlyingCoinCount={config.FlyingCoinCount}");

            PlayFlyAnimation(() =>
            {
                Debug.Log("[CommonRewardPopup] 测试飞币动画完成");
            });
        }
#endif

        #endregion
    }
}
