// 金钱方块系统 - 奖励弹窗
// 创建日期: 2026-03-05

using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BlockPuzzleGameToolkit.Scripts.Popups;
using BlockPuzzleGameToolkit.Scripts.GUI;
using BlockPuzzleGameToolkit.Scripts.CurrencySystem;
using BlockPuzzle.AdSystem.Managers;

namespace BlockPuzzleGameToolkit.Scripts.MoneyBlockSystem
{
    /// <summary>
    /// 金钱方块累计奖励弹窗
    /// 职责:
    /// - 显示基础奖励金额和广告倍率
    /// - 提供单倍领取和广告多倍领取按钮
    /// - 集成广告系统
    /// - 处理广告成功/失败
    /// - 回调通知MoneyBlockManager
    /// </summary>
    [RequireComponent(typeof(Animator), typeof(CanvasGroup))]
    public class MoneyBlockRewardPopup : Popup
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
        [SerializeField] private TextMeshProUGUI adClaimButtonText;      // 广告按钮文本

        [Header("Ad Configuration")]
        [SerializeField] private string adEntryName = "MoneyBlock_Cumulative";  // 广告入口名称

        #endregion

        #region Private Fields

        private int baseReward;                 // 基础奖励（放大10000倍）
        private float adMultiplier;             // 广告倍率
        private Action<EClaimType, int, bool> onRewardClaimed;  // 奖励领取回调(类型, 金额, 是否成功)

        private bool isProcessingClick = false; // 防止重复点击

        #endregion

        #region Initialization

        /// <summary>
        /// 初始化弹窗
        /// </summary>
        /// <param name="reward">基础奖励（放大10000倍）</param>
        /// <param name="multiplier">广告倍率</param>
        /// <param name="callback">领取回调(EClaimType, finalReward, success)</param>
        public void Initialize(int reward, float multiplier, Action<EClaimType, int, bool> callback)
        {
            baseReward = reward;
            adMultiplier = multiplier;
            onRewardClaimed = callback;

            // 更新UI显示
            UpdateDisplay();

            // 设置按钮事件
            SetupButtons();

            Debug.Log($"[MoneyBlockRewardPopup] 初始化 - 基础奖励: {reward}, 广告倍率: {multiplier}");
        }

        protected override void Awake()
        {
            base.Awake();

            // 自动查找UI组件
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
                adClaimButtonText = adClaimButton.GetComponentInChildren<TextMeshProUGUI>();
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
                    string formattedReward = CurrencyManager.Instance.FormatCurrency(baseReward, true);
                    baseRewardText.text = formattedReward;
                }
                else
                {
                    float dollarValue = baseReward / 10000f;
                    baseRewardText.text = $"${dollarValue:F3}";
                }
            }

            // 更新广告倍率文本
            if (adMultiplierText != null)
            {
                adMultiplierText.text = $"x{adMultiplier:F0}";
            }

            // 更新按钮文本
            UpdateButtonTexts();
        }

        /// <summary>
        /// 更新按钮文本
        /// </summary>
        private void UpdateButtonTexts()
        {
            // 单倍按钮：显示基础奖励
            if (singleClaimButtonText != null)
            {
                if (CurrencyManager.Instance != null)
                {
                    string formattedReward = CurrencyManager.Instance.FormatCurrency(baseReward, true);
                    singleClaimButtonText.text = formattedReward;
                }
                else
                {
                    float dollarValue = baseReward / 10000f;
                    singleClaimButtonText.text = $"${dollarValue:F3}";
                }
            }

            // 广告按钮：显示多倍奖励
            if (adClaimButtonText != null)
            {
                int adReward = Mathf.RoundToInt(baseReward * adMultiplier);
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
                Debug.Log("[MoneyBlockRewardPopup] 正在处理中，忽略重复点击");
                return;
            }

            isProcessingClick = true;

            // 禁用按钮
            DisableButtons();

            Debug.Log("[MoneyBlockRewardPopup] 单倍领取");

            // 回调通知Manager（单倍领取总是成功）
            onRewardClaimed?.Invoke(EClaimType.Single, baseReward, true);

            // 关闭弹窗
            Close();
        }

        /// <summary>
        /// 广告多倍领取按钮点击
        /// </summary>
        private void OnAdClaimClicked()
        {
            // 防重复点击
            if (isProcessingClick)
            {
                Debug.Log("[MoneyBlockRewardPopup] 正在处理中，忽略重复点击");
                return;
            }

            isProcessingClick = true;

            // 禁用按钮
            DisableButtons();

            Debug.Log("[MoneyBlockRewardPopup] 广告多倍领取");

            // 播放激励广告
            ShowRewardedAd((success) =>
            {
                // 检查对象是否存在（防止广告期间弹窗被销毁）
                if (this == null)
                {
                    Debug.LogWarning("[MoneyBlockRewardPopup] 广告回调时弹窗对象已被销毁");
                    return;
                }

                if (success)
                {
                    // 广告成功，发放多倍奖励
                    int finalReward = Mathf.RoundToInt(baseReward * adMultiplier);
                    Debug.Log($"[MoneyBlockRewardPopup] 广告成功，发放多倍奖励: {finalReward}");

                    // 回调通知Manager（广告成功）
                    onRewardClaimed?.Invoke(EClaimType.AdMultiple, finalReward, true);

                    // 关闭弹窗
                    Close();
                }
                else
                {
                    // 广告失败，根据用户反馈：直接重置计数并关闭弹窗
                    Debug.LogWarning("[MoneyBlockRewardPopup] 广告失败，重置计数并关闭弹窗");

                    // 通知Manager广告失败（不发放奖励，不增加统计）
                    onRewardClaimed?.Invoke(EClaimType.AdMultiple, 0, false);

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

        #region Ad Integration

        /// <summary>
        /// 显示激励广告
        /// </summary>
        private void ShowRewardedAd(Action<bool> callback)
        {
            // 检查广告入口名称
            if (string.IsNullOrEmpty(adEntryName))
            {
                Debug.LogWarning("[MoneyBlockRewardPopup] 广告入口名称未配置");
                callback?.Invoke(false);
                return;
            }

            // 调用AdSystemManager播放广告
            var adManager = AdSystemManager.Instance;
            if (adManager == null)
            {
                Debug.LogError("[MoneyBlockRewardPopup] AdSystemManager未找到");
                callback?.Invoke(false);
                return;
            }

            Debug.Log($"[MoneyBlockRewardPopup] 播放激励广告: {adEntryName}");

            adManager.PlayAd(adEntryName, (success) =>
            {
                // 检查对象是否存在
                if (this == null)
                {
                    Debug.LogWarning("[MoneyBlockRewardPopup] 广告回调时对象已销毁");
                    return;
                }

                Debug.Log($"[MoneyBlockRewardPopup] 广告播放结果: {success}");
                callback?.Invoke(success);
            });
        }

        #endregion

        #region Override Methods

        public override void Close()
        {
            // 重置标志
            isProcessingClick = false;

            base.Close();
        }

        #endregion
    }
}
