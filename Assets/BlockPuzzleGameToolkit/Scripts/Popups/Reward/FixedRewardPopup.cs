// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BlockPuzzleGameToolkit.Scripts.Settings;
using BlockPuzzleGameToolkit.Scripts.GUI;
using BlockPuzzleGameToolkit.Scripts.LevelsData;
using BlockPuzzleGameToolkit.Scripts.CurrencySystem;
using BlockPuzzleGameToolkit.Scripts.Localization;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using System.Collections;

namespace BlockPuzzleGameToolkit.Scripts.Popups.Reward
{
    /// <summary>
    /// 固定倍率奖励弹窗
    /// 弹窗打开时获取固定倍率值，获取奖励时的倍数不会变动
    /// </summary>
    public class FixedRewardPopup : RewardPopupBase
    {
        #region Private Fields

        private LevelRewardMultiplierSettings.FixedMultiplierConfig currentConfig;
        private float fixedMultiplierValue = 1f;  // 固定倍率值（避免与基类冲突）
        private int currentMultiplierIndex = 0;  // 当前使用的倍率索引
        private bool isProcessingFixedClick = false;  // 防重复点击标志（子类独立字段）

        #endregion

        #region Initialization

        protected override void InitializeMultiplierUI()
        {
            // 获取白包模式状态
            bool isWhitePackageMode = GetWhitePackageMode();

            if (isWhitePackageMode)
            {
                // 白包模式：隐藏倍率显示，使用基础奖励
                if (multiplierDisplayText != null)
                {
                    multiplierDisplayText.gameObject.SetActive(false);
                    Debug.Log("[FixedRewardPopup] 白包模式：隐藏倍率显示");
                }

                fixedMultiplierValue = 1f; // 白包模式固定倍率为1
                Debug.Log("[FixedRewardPopup] 白包模式：使用基础奖励（倍率x1）");
            }
            else
            {
                // 标准模式：原有逻辑
                // 加载倍率配置
                if (!LoadMultiplierConfig())
                {
                    Debug.LogError("[FixedRewardPopup] 加载倍率配置失败");
                    return;
                }

                // 获取当前倍率
                LoadCurrentMultiplier();

                // 更新倍率显示
                if (multiplierDisplayText != null)
                {
                    multiplierDisplayText.text = $"{(int)fixedMultiplierValue}";
                }
            }

            // 设置按钮显示
            SetupButtons();

            // 更新UI显示（除了倍率）
            if (baseRewardText != null && !isWhitePackageMode)
            {
                // 标准模式：显示基础奖励
                if (CurrencyManager.Instance != null)
                {
                    string formattedReward = CurrencyManager.Instance.FormatCurrency(baseReward, true);
                    baseRewardText.text = formattedReward;
                }
                else
                {
                    float baseRewardDollar = baseReward / 10000f;
                    baseRewardText.text = $"${baseRewardDollar:F3}";
                }
            }
        }

        /// <summary>
        /// 加载倍率配置
        /// </summary>
        private bool LoadMultiplierConfig()
        {
            if (rewardConfig == null)
            {
                Debug.LogError("[FixedRewardPopup] rewardConfig为空");
                return false;
            }

            // 加载配置文件
            var settings = Resources.Load<LevelRewardMultiplierSettings>("Settings/LevelRewardMultiplierSettings");
            if (settings == null)
            {
                Debug.LogError("[FixedRewardPopup] 未找到LevelRewardMultiplierSettings配置文件");
                return false;
            }

            // 获取指定配置
            currentConfig = settings.GetConfig(rewardConfig.FixedMultiplierConfigId);
            if (currentConfig == null)
            {
                // 使用默认配置
                var allConfigs = settings.Configs;
                if (allConfigs != null && allConfigs.Count > 0)
                {
                    currentConfig = allConfigs[0];
                    Debug.LogWarning($"[FixedRewardPopup] 使用默认配置: {currentConfig.ConfigName}");
                }
                else
                {
                    Debug.LogError("[FixedRewardPopup] 没有可用的倍率配置");
                    return false;
                }
            }

            return currentConfig != null && currentConfig.Multipliers != null && currentConfig.Multipliers.Length > 0;
        }

        /// <summary>
        /// 加载当前倍率
        /// </summary>
        private void LoadCurrentMultiplier()
        {
            if (currentConfig == null || currentConfig.Multipliers == null)
            {
                Debug.LogError("[FixedRewardPopup] 配置无效");
                fixedMultiplierValue = 1f;
                return;
            }

            // 从PlayerPrefs加载索引
            string indexKey = $"FixedMultiplier_{currentConfig.ConfigId}_Index";
            currentMultiplierIndex = PlayerPrefs.GetInt(indexKey, 0);

            // 检查是否需要重置
            CheckAndResetIndex();

            // 确保索引在有效范围内
            currentMultiplierIndex = Mathf.Clamp(currentMultiplierIndex, 0, currentConfig.Multipliers.Length - 1);

            // 获取当前倍率
            fixedMultiplierValue = currentConfig.Multipliers[currentMultiplierIndex];

            Debug.Log($"[FixedRewardPopup] 使用倍率: x{fixedMultiplierValue} (索引: {currentMultiplierIndex})");
        }

        /// <summary>
        /// 检查并重置索引
        /// </summary>
        private void CheckAndResetIndex()
        {
            string lastResetKey = $"FixedMultiplier_{currentConfig.ConfigId}_LastReset";
            string lastResetDate = PlayerPrefs.GetString(lastResetKey, "");

            // 检查每日重置
            if (currentConfig.ResetDaily)
            {
                string today = System.DateTime.Today.ToString("yyyy-MM-dd");
                if (lastResetDate != today)
                {
                    currentMultiplierIndex = 0;
                    PlayerPrefs.SetString(lastResetKey, today);
                    PlayerPrefs.SetInt($"FixedMultiplier_{currentConfig.ConfigId}_Index", 0);
                    Debug.Log("[FixedRewardPopup] 每日重置倍率索引");
                }
            }
        }

        /// <summary>
        /// 设置按钮显示
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
                // 根据配置显示免费或广告按钮文本
                bool showRewardAd = rewardConfig != null && rewardConfig.ShowRewardAd;
                int currentReward = (int)(baseReward * fixedMultiplierValue);
                if (claimFreeButton != null)
                {
                    claimFreeButton.onClick.RemoveAllListeners();
                    claimFreeButton.onClick.AddListener(OnClaimFreeClicked);

                    var freeText = claimFreeButton.GetComponentInChildren<Text>();
                    if (freeText != null)
                    {
                        freeText.text = CurrencyManager.Instance.FormatCurrency(currentReward, true);
                    }

                    // 根据配置显示或隐藏
                    claimFreeButton.gameObject.SetActive(!showRewardAd);
                }

                if (claimAdButton != null)
                {
                    claimAdButton.onClick.RemoveAllListeners();
                    claimAdButton.onClick.AddListener(OnClaimAdClicked);

                    var adText = claimAdButton.GetComponentInChildren<Text>();
                    if (adText != null)
                    {
                        adText.text = CurrencyManager.Instance.FormatCurrency(currentReward, true);
                    }

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
                        string localizedFormat = LocalizationManager.GetText("ClaimPer", "Claim ({0}%)");
                        skipText.text = string.Format(localizedFormat, (skipMultiplier * 100).ToString("F0"));
                    }
                }
            }
        }

        /// <summary>
        /// 更新UI显示
        /// </summary>
        private void UpdateUIDisplay()
        {
            // 更新倍率显示
            if (multiplierDisplayText != null)
            {
                //显示整数倍率
                multiplierDisplayText.text = $"{(int)fixedMultiplierValue}";
            }

            // 更新奖励显示（使用CurrencyManager格式化）
            if (baseRewardText != null)
            {
                if (CurrencyManager.Instance != null)
                {
                    // 使用CurrencyManager的FormatCurrency方法
                    string formattedReward = CurrencyManager.Instance.FormatCurrency(baseReward, true);
                    baseRewardText.text = formattedReward;
                }
                else
                {
                    // 如果CurrencyManager未初始化，使用默认格式
                    float baseRewardDollar = baseReward / 10000f;
                    baseRewardText.text = $"{baseRewardDollar:F3}";
                }
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
            if (isProcessingFixedClick)
            {
                Debug.Log("[FixedRewardPopup] 正在处理中，忽略重复点击");
                return;
            }

            isProcessingFixedClick = true; // 设置处理标志

            // 禁用所有按钮交互
            if (claimFreeButton != null) claimFreeButton.interactable = false;
            if (claimAdButton != null) claimAdButton.interactable = false;
            if (skipButton != null) skipButton.interactable = false;

            Debug.Log($"[FixedRewardPopup] 免费领取奖励: x{fixedMultiplierValue}");

            // 获取白包模式状态
            bool isWhitePackageMode = GetWhitePackageMode();

            if (isWhitePackageMode)
            {
                // 白包模式：直接使用基础奖励，不播放滚动动画
                finalReward = baseReward;

                // 发放奖励（只播放飞行动画）
                GrantRewardWhitePackageMode(finalReward);
                TriggerRewardClaimed(finalReward, 1);
                result = EPopupResult.Claimed;

                Debug.Log($"[FixedRewardPopup] 白包模式：直接领取奖励 ${finalReward/10000f:F3}");
            }
            else
            {
                // 标准模式：原有逻辑
                // 更新索引（用于下次）
                SaveNextMultiplierIndex();

                // 计算最终奖励
                finalReward = (int)(baseReward * fixedMultiplierValue);

                // 播放奖励文本滚动动画，动画完成后发放奖励
                StartRewardTextAnimation(baseReward, finalReward, () =>
                {
                    // 动画完成后发放奖励
                    GrantReward(finalReward, (int)fixedMultiplierValue);
                    TriggerRewardClaimed(finalReward, (int)fixedMultiplierValue);
                    result = EPopupResult.Claimed;

                    ShowNextLevelButton();
                });
            }
        }

        /// <summary>
        /// 广告领取按钮点击
        /// </summary>
        private void OnClaimAdClicked()
        {
            // 防重复点击检查
            if (isProcessingFixedClick)
            {
                Debug.Log("[FixedRewardPopup] 正在处理中，忽略重复点击");
                return;
            }

            isProcessingFixedClick = true; // 设置处理标志

            // 禁用所有按钮交互
            if (claimFreeButton != null) claimFreeButton.interactable = false;
            if (claimAdButton != null) claimAdButton.interactable = false;
            if (skipButton != null) skipButton.interactable = false;

            Debug.Log($"[FixedRewardPopup] 观看广告领取奖励: x{fixedMultiplierValue}");

            // 获取白包模式状态
            bool isWhitePackageMode = GetWhitePackageMode();

            if (isWhitePackageMode)
            {
                // 白包模式：此按钮应该被隐藏，但如果触发了，按免费领取处理
                OnClaimFreeClicked();
                return;
            }

            // 标准模式：原有逻辑
            // 显示激励广告
            ShowRewardedAd(success =>
            {
                // 检查对象是否仍然存在（防止广告播放期间弹窗被销毁）
                if (this == null)
                {
                    Debug.LogWarning("[FixedRewardPopup] 广告回调时弹窗对象已被销毁");
                    return;
                }

                if (success)
                {
                    // 广告播放成功，发放奖励
                    Debug.Log("[FixedRewardPopup] 广告播放成功，发放奖励");
                    SaveNextMultiplierIndex();

                    // 计算最终奖励
                    finalReward = (int)(baseReward * fixedMultiplierValue);

                    // 播放奖励文本滚动动画，动画完成后发放奖励
                    StartRewardTextAnimation(baseReward, finalReward, () =>
                    {
                        // 再次检查对象是否存在
                        if (this == null)
                        {
                            Debug.LogWarning("[FixedRewardPopup] 动画回调时弹窗对象已被销毁");
                            return;
                        }

                        // 动画完成后发放奖励
                        GrantReward(finalReward, (int)fixedMultiplierValue);
                        TriggerRewardClaimed(finalReward, (int)fixedMultiplierValue);
                        result = EPopupResult.Claimed;

                        ShowNextLevelButton();
                    });
                }
                else
                {
                    // 广告播放失败，直接关闭弹窗，不发放奖励，累加关卡进度
                    Debug.LogWarning("[FixedRewardPopup] 广告播放失败，关闭弹窗并进入下一关");
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
        /// 不领奖按钮点击（获得部分奖励）
        /// </summary>
        private void OnSkipPartialClicked()
        {
            // 防重复点击检查
            if (isProcessingFixedClick)
            {
                Debug.Log("[FixedRewardPopup] 正在处理中，忽略重复点击");
                return;
            }

            isProcessingFixedClick = true; // 设置处理标志

            // 禁用所有按钮交互
            if (claimFreeButton != null) claimFreeButton.interactable = false;
            if (claimAdButton != null) claimAdButton.interactable = false;
            if (skipButton != null) skipButton.interactable = false;

            float skipMultiplier = rewardConfig != null ? rewardConfig.SkipMultiplier : 0.2f;
            int skipReward = (int)(baseReward * skipMultiplier);

            Debug.Log($"[FixedRewardPopup] 不领奖，获得 {skipMultiplier * 100}% 奖励: ${skipReward:F3}");

            if (rewardConfig != null && rewardConfig.ShowInterstitialAd)
            {
                // 显示插屏广告
                ShowInterstitialAd((success) =>
                {
                    // 检查对象是否仍然存在
                    if (this == null)
                    {
                        Debug.LogWarning("[FixedRewardPopup] 插屏广告回调时弹窗对象已被销毁");
                        return;
                    }

                    if (success)
                    {
                        // 广告播放成功，发放奖励
                        GrantReward(skipReward, 0);
                        TriggerRewardSkipped(skipReward);
                        result = EPopupResult.Skip;
                        ShowNextLevelButton();
                    }
                    else
                    {
                        // 广告播放失败，关闭弹窗并累加关卡进度
                        Debug.LogWarning("[FixedRewardPopup] 插屏广告播放失败，关闭弹窗并进入下一关");
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
                GrantReward(skipReward, 0);
                TriggerRewardSkipped(skipReward);
                result = EPopupResult.Skip;

                // TODO: 播放道具飞行动画

                ShowNextLevelButton();
            }
        }

        #endregion

        #region Helper Methods

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
        /// 播放奖励文本滚动动画
        /// </summary>
        /// <param name="from">起始值</param>
        /// <param name="to">目标值</param>
        /// <param name="onComplete">动画完成回调</param>
        private void StartRewardTextAnimation(float from, float to, System.Action onComplete = null)
        {
            if (baseRewardText == null)
            {
                // 如果文本不存在，直接执行回调
                onComplete?.Invoke();
                return;
            }

            // 停止之前的动画（如果有）
            StopCoroutine("AnimateRewardText");

            // 开始新的动画
            StartCoroutine(AnimateRewardText(from, to, onComplete));
        }

        /// <summary>
        /// 奖励文本动画协程
        /// </summary>
        private System.Collections.IEnumerator AnimateRewardText(float from, float to, System.Action onComplete)
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

        /// <summary>
        /// 保存下一次的倍率索引
        /// </summary>
        private void SaveNextMultiplierIndex()
        {
            if (currentConfig == null) return;

            // 索引+1
            currentMultiplierIndex++;

            // 如果超过数组长度，保持在最后一个
            if (currentMultiplierIndex >= currentConfig.Multipliers.Length)
            {
                currentMultiplierIndex = currentConfig.Multipliers.Length - 1;
            }

            // 保存到PlayerPrefs
            string indexKey = $"FixedMultiplier_{currentConfig.ConfigId}_Index";
            PlayerPrefs.SetInt(indexKey, currentMultiplierIndex);
            PlayerPrefs.Save();

            Debug.Log($"[FixedRewardPopup] 下次倍率索引: {currentMultiplierIndex}");
        }

        /// <summary>
        /// 显示下一关按钮（替代Win弹窗功能）
        /// </summary>
        private void ShowNextLevelButton()
        {
            // 弹窗关闭时会自动有缩小渐隐动画，无需手动隐藏UI元素
            // FlyRewardManager会在动画完成后通知并进入下一关
        }

        #endregion

        #region Override Methods

        protected override int GetSelectedMultiplier()
        {
            return Mathf.RoundToInt(fixedMultiplierValue);
        }

        protected override void UpdateMultiplierDisplay()
        {
            UpdateUIDisplay();
        }

        public override void Close()
        {
            // 重置防重复点击标志
            isProcessingFixedClick = false;

            base.Close();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 提现时重置索引（预留接口）
        /// </summary>
        public static void ResetOnWithdraw(string configId = null)
        {
            // TODO: 实现提现重置逻辑
            Debug.Log($"[FixedRewardPopup] 提现重置倍率索引 - ConfigId: {configId ?? "All"}");

            if (!string.IsNullOrEmpty(configId))
            {
                PlayerPrefs.SetInt($"FixedMultiplier_{configId}_Index", 0);
            }
            // 可以扩展为重置所有配置
        }

        #endregion

        #region Debug

#if UNITY_EDITOR
        [ContextMenu("测试加载配置")]
        private void Debug_LoadConfig()
        {
            if (rewardConfig == null)
            {
                rewardConfig = LevelRewardConfig.CreateDefault();
            }
            LoadMultiplierConfig();
            LoadCurrentMultiplier();
            Debug.Log($"[FixedRewardPopup] 当前倍率: x{fixedMultiplierValue}");
        }

        [ContextMenu("测试重置索引")]
        private void Debug_ResetIndex()
        {
            if (currentConfig != null)
            {
                PlayerPrefs.SetInt($"FixedMultiplier_{currentConfig.ConfigId}_Index", 0);
                PlayerPrefs.Save();
                Debug.Log("[FixedRewardPopup] 索引已重置");
            }
        }
#endif

        #endregion
    }
}