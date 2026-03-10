// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

using System.Collections;
using UnityEngine;
using TMPro;
using BlockPuzzleGameToolkit.Scripts.GUI;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using BlockPuzzleGameToolkit.Scripts.LevelsData;
using BlockPuzzleGameToolkit.Scripts.CurrencySystem;
using BlockPuzzleGameToolkit.Scripts.Multiplier.Core;
using BlockPuzzleGameToolkit.Scripts.Multiplier.UI;
using DG.Tweening;
using BlockPuzzleGameToolkit.Scripts.Localization;

namespace BlockPuzzleGameToolkit.Scripts.Popups.Reward
{
    /// <summary>
    /// 滑动倍率奖励弹窗
    /// 集成MultiplierSliderUI模块，点击领奖时从模块获取倍率
    /// </summary>
    public class SlidingRewardPopup : RewardPopupBase
    {
        #region Fields

        [Header("Slider Module")]
        [SerializeField] private MultiplierSliderUI sliderModule;     // 滑动倍率模块引用

        private float currentMultiplierValue = 1f;                    // 当前倍率值
        private bool hasStartedSliding = false;                       // 是否已开始滑动
        private bool isProcessingSliderClick = false;                 // 防重复点击标志（子类独立字段）

        // 缓存的WaitForSeconds对象，提高性能
        private readonly WaitForSeconds waitForUpdateInterval = new WaitForSeconds(0.05f);

        #endregion

        #region Initialization

        protected override void InitializeMultiplierUI()
        {
            // 获取白包模式状态
            bool isWhitePackageMode = GetWhitePackageMode();

            if (isWhitePackageMode)
            {
                // 白包模式：隐藏倍率显示，不启动滑动
                if (multiplierDisplayText != null)
                {
                    multiplierDisplayText.gameObject.SetActive(false);
                    Debug.Log("[SlidingRewardPopup] 白包模式：隐藏倍率显示");
                }

                // 禁用滑动模块，防止它自动移动
                if (sliderModule != null)
                {
                    sliderModule.gameObject.SetActive(false);
                    Debug.Log("[SlidingRewardPopup] 白包模式：禁用滑动模块");
                }

                // 不初始化MultiplierManager
                Debug.Log("[SlidingRewardPopup] 白包模式：跳过倍率滑动初始化");
            }
            else
            {
                // 标准模式：确保滑动模块是激活的
                if (sliderModule != null)
                {
                    sliderModule.gameObject.SetActive(true);
                }

                // 初始化MultiplierManager
                if (MultiplierManager.Instance == null)
                {
                    Debug.LogError("[SlidingRewardPopup] MultiplierManager未初始化");
                    return;
                }

                // 设置滑动模块到管理器
                MultiplierManager.Instance.SetSliderUI(sliderModule);

                // 自动开始滑动
                StartSliding();

                // 隐藏倍率文本，等待动画播放时显示
                multiplierDisplayText.gameObject.SetActive(false);
            }

            // 设置按钮
            SetupButtons();

            // 更新显示
            UpdateRewardDisplay();
        }

        /// <summary>
        /// 设置按钮事件
        /// </summary>
        protected override void SetupButtons()
        {
            // 调用基类方法（会处理白包模式的按钮显示）
            base.SetupButtons();

            // 获取白包模式状态
            bool isWhitePackageMode = GetWhitePackageMode();

            if (isWhitePackageMode)
            {
                // 白包模式：只设置免费领取按钮
                if (claimFreeButton != null)
                {
                    claimFreeButton.onClick.RemoveAllListeners();
                    claimFreeButton.onClick.AddListener(OnClaimFreeClicked);
                    // 文字已经在基类中设置为 "Claim"
                }
                // 其他按钮在基类中已隐藏
            }
            else
            {
                // 标准模式：原有逻辑
                // 根据配置决定显示哪个按钮
                bool showRewardAd = rewardConfig != null && rewardConfig.ShowRewardAd;

                if (claimFreeButton != null)
                {
                    claimFreeButton.onClick.RemoveAllListeners();
                    claimFreeButton.onClick.AddListener(OnClaimFreeClicked);

                    // 根据配置显示或隐藏
                    claimFreeButton.gameObject.SetActive(!showRewardAd);
                }

                if (claimAdButton != null)
                {
                    claimAdButton.onClick.RemoveAllListeners();
                    claimAdButton.onClick.AddListener(OnClaimAdClicked);

                    // 根据配置显示或隐藏
                    claimAdButton.gameObject.SetActive(showRewardAd);
                }

                if (skipButton != null)
                {
                    skipButton.onClick.RemoveAllListeners();
                    skipButton.onClick.AddListener(OnSkipPartialClicked);

                    // 设置跳过按钮文本
                    var skipText = skipButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (skipText != null)
                    {
                        float skipMultiplier = rewardConfig != null ? rewardConfig.SkipMultiplier : 0.2f;
                         // 使用方案1：占位符格式化
                        string localizedFormat = LocalizationManager.GetText("ClaimPer", "Claim {0}%");
                        skipText.text = string.Format(localizedFormat, (skipMultiplier * 100).ToString("F0"));
                    }
                }
            }
        }

        #endregion

        #region Sliding Logic

        /// <summary>
        /// 开始滑动
        /// </summary>
        private void StartSliding()
        {
            if (hasStartedSliding)
            {
                Debug.LogWarning("[SlidingRewardPopup] 已经开始滑动");
                return;
            }

            if (MultiplierManager.Instance != null)
            {
                // 确保MultiplierManager有正确的UI引用
                if (sliderModule != null)
                {
                    MultiplierManager.Instance.SetSliderUI(sliderModule);
                }

                MultiplierManager.Instance.StartSliding();
                hasStartedSliding = true;

                // 启动协程持续更新显示
                // StartCoroutine(UpdateDisplayWhileSliding());

                Debug.Log("[SlidingRewardPopup] 滑动模块已启动");
            }
            else
            {
                Debug.LogError("[SlidingRewardPopup] MultiplierManager未初始化");
            }
        }

        /// <summary>
        /// 停止滑动并获取倍率
        /// </summary>
        private int StopSlidingAndGetMultiplier()
        {
            if (!hasStartedSliding)
            {
                Debug.LogWarning("[SlidingRewardPopup] 尚未开始滑动");
                return 1;
            }

            if (MultiplierManager.Instance != null)
            {
                int multiplier = MultiplierManager.Instance.StopSliding();
                hasStartedSliding = false;
                Debug.Log($"[SlidingRewardPopup] 停止滑动，获得倍率: x{multiplier}");
                return multiplier;
            }
            else
            {
                Debug.LogError("[SlidingRewardPopup] MultiplierManager未初始化");
                return 1;
            }
        }

        #endregion

        #region Button Handlers

        /// <summary>
        /// 免费领取按钮点击
        /// </summary>
        private void OnClaimFreeClicked()
        {
            // 防重复点击检查
            if (isProcessingSliderClick)
            {
                Debug.Log("[SlidingRewardPopup] 正在处理中，忽略重复点击");
                return;
            }

            isProcessingSliderClick = true; // 设置处理标志

            // 禁用所有按钮交互
            if (claimFreeButton != null) claimFreeButton.interactable = false;
            if (claimAdButton != null) claimAdButton.interactable = false;
            if (skipButton != null) skipButton.interactable = false;

            Debug.Log("[SlidingRewardPopup] 免费领取奖励");

            // 获取白包模式状态
            bool isWhitePackageMode = GetWhitePackageMode();

            if (isWhitePackageMode)
            {
                // 白包模式：直接使用基础奖励，不使用倍率
                currentMultiplierValue = 1;
                finalReward = baseReward;

                // 直接发放奖励（不播放倍率动画）
                GrantRewardWhitePackageMode(finalReward);
                TriggerRewardClaimed(finalReward, 1);
                result = EPopupResult.Claimed;

                Debug.Log($"[SlidingRewardPopup] 白包模式：直接领取奖励 ${finalReward/10000f:F3}");
            }
            else
            {
                // 标准模式：原有逻辑
                // 停止滑动并获取倍率
                currentMultiplierValue = StopSlidingAndGetMultiplier();

                // 计算最终奖励（转换为整数）
                finalReward = Mathf.RoundToInt(baseReward * currentMultiplierValue);

                // 启动动画序列：第一（盖章）-> 第二（滚动）-> 第三（飞行）
                StartCoroutine(PlayFullRewardAnimation());
            }
        }

        /// <summary>
        /// 播放完整的领奖动画序列
        /// </summary>
        private IEnumerator PlayFullRewardAnimation()
        {
            // 阶段1：盖章动画（0.3秒）
            if (multiplierDisplayText != null)
            {
                // 保存初始状态
                Vector3 originalScale = multiplierDisplayText.transform.localScale;

                // 设置倍率文本内容
                multiplierDisplayText.text = $"{(int)currentMultiplierValue}";

                // 初始化：从大到小的简单盖章效果
                multiplierDisplayText.transform.localScale = originalScale * 2f;
                multiplierDisplayText.gameObject.SetActive(true);

                // 创建简单的从大到小动画
                Sequence stampSequence = DOTween.Sequence();

                // 从2倍大小缩小到正常大小
                stampSequence.Append(
                    multiplierDisplayText.transform.DOScale(originalScale, 0.3f)
                        .SetEase(Ease.OutQuad)
                );

                // 等待盖章动画完成
                yield return stampSequence.WaitForCompletion();
            }

            // 阶段2：文本滚动动画（1.5秒）
            if (baseRewardText != null)
            {
                int startValue = baseReward;
                int endValue = finalReward;

                // 创建滚动动画
                var rollTween = DOTween.To(
                    () => startValue,
                    x => {
                        // 更新文本显示
                        if (CurrencyManager.Instance != null)
                        {
                            baseRewardText.text = CurrencyManager.Instance.FormatCurrency(x, true);
                        }
                        else
                        {
                            float dollarValue = x / 10000f;
                            baseRewardText.text = $"${dollarValue:F3}";
                        }
                    },
                    endValue,
                    1.5f
                ).SetEase(Ease.OutQuad);

                // 等待滚动动画完成
                yield return rollTween.WaitForCompletion();

                // 确保最终值准确
                if (CurrencyManager.Instance != null)
                {
                    baseRewardText.text = CurrencyManager.Instance.FormatCurrency(endValue, true);
                }
                else
                {
                    float dollarValue = endValue / 10000f;
                    baseRewardText.text = $"${dollarValue:F3}";
                }
            }

            // 阶段3：发放奖励并播放飞行动画
            // 先添加货币
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.AddCoins(finalReward);
            }

            // 播放飞行动画（调用基类方法）
            base.PlayRewardAnimation();

            // 触发奖励领取事件
            TriggerRewardClaimed(finalReward, (int)currentMultiplierValue);
            result = EPopupResult.Claimed;
        }


        /// <summary>
        /// 广告领取按钮点击
        /// </summary>
        private void OnClaimAdClicked()
        {
            // 防重复点击检查
            if (isProcessingSliderClick)
            {
                Debug.Log("[SlidingRewardPopup] 正在处理中，忽略重复点击");
                return;
            }

            isProcessingSliderClick = true; // 设置处理标志

            // 禁用所有按钮交互
            if (claimFreeButton != null) claimFreeButton.interactable = false;
            if (claimAdButton != null) claimAdButton.interactable = false;
            if (skipButton != null) skipButton.interactable = false;

            Debug.Log("[SlidingRewardPopup] 观看广告领取奖励");

            // 获取白包模式状态
            bool isWhitePackageMode = GetWhitePackageMode();

            if (isWhitePackageMode)
            {
                // 白包模式：此按钮应该被隐藏，但如果触发了，按免费领取处理
                OnClaimFreeClicked();
                return;
            }

            // 标准模式：原有逻辑
            // 停止滑动并获取倍率
            currentMultiplierValue = StopSlidingAndGetMultiplier();

            // 计算最终奖励
            finalReward = Mathf.RoundToInt(baseReward * currentMultiplierValue);

            // 显示激励广告
            ShowRewardedAd(success =>
            {
                // 检查对象是否仍然存在（防止广告播放期间弹窗被销毁）
                if (this == null)
                {
                    Debug.LogWarning("[SlidingRewardPopup] 广告回调时弹窗对象已被销毁");
                    return;
                }

                if (success)
                {
                    // 广告播放成功，发放奖励
                    Debug.Log("[SlidingRewardPopup] 广告播放成功，发放奖励");
                    // 启动动画序列：第一（盖章）-> 第二（滚动）-> 第三（飞行）
                    StartCoroutine(PlayFullRewardAnimation());
                }
                else
                {
                    // 广告播放失败，直接关闭弹窗，不发放奖励，累加关卡进度
                    Debug.LogWarning("[SlidingRewardPopup] 广告播放失败，关闭弹窗并进入下一关");
                    result = EPopupResult.Skip;
                    Close();

                    // 进入下一关
                    if (GameDataManager.HasMoreLevels())
                    {
                        GameManager.Instance.NextLevel();
                    }
                    else
                    {
                        GameManager.Instance.RestartLevel();
                    }
                }
            });
        }

        /// <summary>
        /// 不领奖按钮点击
        /// </summary>
        private void OnSkipPartialClicked()
        {
            // 防重复点击检查
            if (isProcessingSliderClick)
            {
                Debug.Log("[SlidingRewardPopup] 正在处理中，忽略重复点击");
                return;
            }

            isProcessingSliderClick = true; // 设置处理标志

            // 禁用所有按钮交互
            if (claimFreeButton != null) claimFreeButton.interactable = false;
            if (claimAdButton != null) claimAdButton.interactable = false;
            if (skipButton != null) skipButton.interactable = false;

            float skipMultiplier = rewardConfig != null ? rewardConfig.SkipMultiplier : 0.2f;
            int skipReward = Mathf.RoundToInt(baseReward * skipMultiplier);

            float skipRewardDollar = skipReward / 10000f;
            Debug.Log($"[SlidingRewardPopup] 不领奖，获得 {skipMultiplier * 100}% 奖励: ${skipRewardDollar:F3}");

            // 清理滑块倍率模块
            CleanupSliderModule();

            if (rewardConfig != null && rewardConfig.ShowInterstitialAd)
            {
                // 显示插屏广告
                ShowInterstitialAd((success) =>
                {
                    // 检查对象是否仍然存在（防止广告播放期间弹窗被销毁）
                    if (this == null)
                    {
                        Debug.LogWarning("[SlidingRewardPopup] 插屏广告回调时弹窗对象已被销毁");
                        return;
                    }

                    if (success)
                    {
                        // 广告播放成功，发放奖励
                        // 广告结束后发放奖励（不播放滚动动画）
                        if (baseRewardText != null)
                        {
                            // 使用CurrencyManager格式化显示
                            if (CurrencyManager.Instance != null)
                            {
                                baseRewardText.text = CurrencyManager.Instance.FormatCurrency(skipReward, true);
                            }
                            else
                            {
                                float dollarValue = skipReward / 10000f;
                                baseRewardText.text = $"${dollarValue:F3}";
                            }
                        }

                        GrantReward(skipReward, 0);
                        TriggerRewardSkipped(skipReward);
                        result = EPopupResult.Skip;

                        ShowCloseAndNextLevel();
                    }
                    else
                    {
                        // 广告播放失败，关闭弹窗并累加关卡进度
                        Debug.LogWarning("[SlidingRewardPopup] 插屏广告播放失败，关闭弹窗并进入下一关");
                        result = EPopupResult.Skip;
                        Close();

                        // 进入下一关
                        if (GameDataManager.HasMoreLevels())
                        {
                            GameManager.Instance.NextLevel();
                        }
                        else
                        {
                            GameManager.Instance.RestartLevel();
                        }
                    }
                });
            }
            else
            {
                // 直接发放奖励（不播放滚动动画）
                if (baseRewardText != null)
                {
                    // 使用CurrencyManager格式化显示
                    if (CurrencyManager.Instance != null)
                    {
                        baseRewardText.text = CurrencyManager.Instance.FormatCurrency(skipReward, true);
                    }
                    else
                    {
                        float dollarValue = skipReward / 10000f;
                        baseRewardText.text = $"${dollarValue:F3}";
                    }
                }

                GrantReward(skipReward, 0);
                TriggerRewardSkipped(skipReward);
                result = EPopupResult.Skip;

                // TODO: 播放道具飞行动画

                ShowCloseAndNextLevel();
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// 显示关闭并进入下一关
        /// </summary>
        private void ShowCloseAndNextLevel()
        {
            // 隐藏所有按钮
            if (claimFreeButton != null)
                claimFreeButton.gameObject.SetActive(false);
            if (claimAdButton != null)
                claimAdButton.gameObject.SetActive(false);
            if (skipButton != null)
                skipButton.gameObject.SetActive(false);

            // 立即关闭
            // 注意：无论是领取还是跳过，都会播放飞行奖励动画
            // FlyRewardManager会在动画完成后负责调用NextLevel进入下一关
            Close();
        }

        /// <summary>
        /// 白包模式的奖励发放（只播放飞行动画）
        /// </summary>
        private void GrantRewardWhitePackageMode(int amount)
        {
            // 先添加货币
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.AddCoins(amount);
            }

            // 只播放飞行动画（调用基类方法）
            base.PlayRewardAnimation();
        }

        /// <summary>
        /// 清理滑块倍率模块
        /// </summary>
        private void CleanupSliderModule()
        {
            if (hasStartedSliding && MultiplierManager.Instance != null)
            {
                MultiplierManager.Instance.StopSliding();
                hasStartedSliding = false;
            }
        }

        #endregion

        #region Override Methods

        /// <summary>
        /// 重写发放奖励方法
        /// </summary>
        protected override void GrantReward(int amount, int multiplier)
        {
            // 先添加货币
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.AddCoins(amount);
            }

            // 播放动画
            PlayRewardAnimation();
        }


        protected override int GetSelectedMultiplier()
        {
            // 如果滑动还在进行，获取当前倍率
            if (hasStartedSliding && MultiplierManager.Instance != null)
            {
                return MultiplierManager.Instance.GetCurrentMultiplier();
            }
            // 否则返回最后选中的倍率
            return Mathf.RoundToInt(currentMultiplierValue);
        }

        protected override void UpdateMultiplierDisplay()
        {
            // int multiplier = GetSelectedMultiplier();

            // 更新倍率显示文本
            // if (multiplierDisplayText != null)
            // {
            //     // 确保文本对象是激活的
            //     if (!multiplierDisplayText.gameObject.activeInHierarchy)
            //     {
            //         multiplierDisplayText.gameObject.SetActive(true);
            //     }

            //     // 显示整数倍率
            //     multiplierDisplayText.text = $"x{multiplier}";
            // }

            // 更新奖励文本显示当前可能获得的奖励
            // int potentialReward = baseReward * multiplier;
            // if (baseRewardText != null)
            // {
            //     // 如果还在滑动，显示实时变化的潜在奖励
            //     if (hasStartedSliding)
            //     {
            //         // 使用CurrencyManager格式化显示
            //         if (CurrencyManager.Instance != null)
            //         {
            //             baseRewardText.text = CurrencyManager.Instance.FormatCurrency(potentialReward, true);
            //         }
            //         else
            //         {
            //             float dollarValue = potentialReward / 10000f;
            //             baseRewardText.text = $"${dollarValue:F3}";
            //         }
            //     }
            // }
        }

        public override void Close()
        {
            // 重置防重复点击标志
            isProcessingSliderClick = false;

            // 清理滑块模块
            CleanupSliderModule();

            base.Close();
        }

        #endregion

        #region Debug

#if UNITY_EDITOR
        [ContextMenu("测试开始滑动")]
        private void Debug_StartSliding()
        {
            StartSliding();
        }

        [ContextMenu("测试停止滑动")]
        private void Debug_StopSliding()
        {
            int multiplier = StopSlidingAndGetMultiplier();
            Debug.Log($"[Debug] 获得倍率: x{multiplier}");
        }

        [ContextMenu("测试获取当前倍率")]
        private void Debug_GetCurrentMultiplier()
        {
            int multiplier = GetSelectedMultiplier();
            Debug.Log($"[Debug] 当前倍率: x{multiplier}");
        }
#endif

        #endregion
    }
}