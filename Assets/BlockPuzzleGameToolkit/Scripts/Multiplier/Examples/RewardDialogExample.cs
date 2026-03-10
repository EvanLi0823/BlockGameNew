// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using BlockPuzzleGameToolkit.Scripts.Multiplier.Core;
using BlockPuzzleGameToolkit.Scripts.Multiplier.UI;

namespace BlockPuzzleGameToolkit.Scripts.Multiplier.Examples
{
    /// <summary>
    /// 奖励弹窗集成示例
    /// 展示如何在奖励弹窗中集成滑动倍率模块
    /// </summary>
    public class RewardDialogExample : MonoBehaviour
    {
        #region Inspector Fields

        [Header("弹窗UI组件")]
        [Tooltip("弹窗面板")]
        [SerializeField] private GameObject dialogPanel;

        [Tooltip("基础奖励文本")]
        [SerializeField] private Text baseRewardText;

        [Tooltip("最终奖励文本")]
        [SerializeField] private Text finalRewardText;

        [Tooltip("倍率显示文本")]
        [SerializeField] private Text multiplierDisplayText;

        [Tooltip("领奖按钮")]
        [SerializeField] private Button claimButton;

        [Tooltip("关闭按钮")]
        [SerializeField] private Button closeButton;

        [Header("倍率模块")]
        [Tooltip("倍率滑块UI组件")]
        [SerializeField] private MultiplierSliderUI multiplierSliderUI;

        [Tooltip("倍率模块容器（可选）")]
        [SerializeField] private GameObject multiplierContainer;

        [Header("奖励设置")]
        [Tooltip("基础奖励类型")]
        [SerializeField] private RewardType rewardType = RewardType.Coins;

        [Tooltip("基础奖励数量")]
        [SerializeField] private int baseRewardAmount = 100;

        [Header("动画设置")]
        [Tooltip("显示动画时长")]
        [SerializeField] private float showAnimationDuration = 0.3f;

        [Tooltip("隐藏动画时长")]
        [SerializeField] private float hideAnimationDuration = 0.2f;

        [Tooltip("奖励数字动画时长")]
        [SerializeField] private float rewardCountAnimationDuration = 1f;

        #endregion

        #region Private Fields

        private MultiplierManager multiplierManager;
        private int currentMultiplier = 1;
        private int finalRewardAmount = 0;
        private bool isClaimProcessing = false;
        private Coroutine countAnimationCoroutine;

        #endregion

        #region Enums

        public enum RewardType
        {
            Coins,
            Gems,
            Stars,
            Experience,
            Custom
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // 获取管理器引用
            multiplierManager = MultiplierManager.Instance;

            // 设置按钮监听
            if (claimButton != null)
            {
                claimButton.onClick.AddListener(OnClaimButtonClick);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseButtonClick);
            }

            // 初始隐藏弹窗
            if (dialogPanel != null)
            {
                dialogPanel.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            // 清理事件监听
            if (claimButton != null)
            {
                claimButton.onClick.RemoveListener(OnClaimButtonClick);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(OnCloseButtonClick);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 显示奖励弹窗
        /// </summary>
        /// <param name="baseAmount">基础奖励数量</param>
        /// <param name="type">奖励类型</param>
        public void ShowRewardDialog(int baseAmount, RewardType type = RewardType.Coins)
        {
            baseRewardAmount = baseAmount;
            rewardType = type;

            ShowDialog();
        }

        /// <summary>
        /// 显示弹窗（使用预设的奖励）
        /// </summary>
        public void ShowDialog()
        {
            if (dialogPanel != null)
            {
                StartCoroutine(ShowDialogCoroutine());
            }
        }

        /// <summary>
        /// 隐藏弹窗
        /// </summary>
        public void HideDialog()
        {
            if (dialogPanel != null)
            {
                StartCoroutine(HideDialogCoroutine());
            }
        }

        #endregion

        #region Button Handlers

        /// <summary>
        /// 领奖按钮点击处理
        /// </summary>
        private void OnClaimButtonClick()
        {
            if (isClaimProcessing)
            {
                Debug.LogWarning("[RewardDialogExample] 正在处理领奖，请稍候");
                return;
            }

            StartCoroutine(ProcessClaimReward());
        }

        /// <summary>
        /// 关闭按钮点击处理
        /// </summary>
        private void OnCloseButtonClick()
        {
            // 如果滑块还在滑动，先停止
            if (multiplierManager != null)
            {
                multiplierManager.StopSliding();
            }

            HideDialog();
        }

        #endregion

        #region Coroutines

        /// <summary>
        /// 显示弹窗协程
        /// </summary>
        private IEnumerator ShowDialogCoroutine()
        {
            // 显示弹窗
            dialogPanel.SetActive(true);

            // 重置状态
            isClaimProcessing = false;
            currentMultiplier = 1;
            finalRewardAmount = baseRewardAmount;

            // 更新UI显示
            UpdateRewardDisplay();

            // 启动倍率滑动
            if (multiplierManager != null)
            {
                multiplierManager.StartSliding();
            }

            // 播放显示动画
            if (showAnimationDuration > 0)
            {
                // 简单的缩放动画
                Transform dialogTransform = dialogPanel.transform;
                dialogTransform.localScale = Vector3.zero;

                float elapsed = 0f;
                while (elapsed < showAnimationDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / showAnimationDuration;
                    dialogTransform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one,
                        AnimationCurve.EaseInOut(0, 0, 1, 1).Evaluate(t));
                    yield return null;
                }

                dialogTransform.localScale = Vector3.one;
            }

            Debug.Log("[RewardDialogExample] 弹窗已显示");
        }

        /// <summary>
        /// 隐藏弹窗协程
        /// </summary>
        private IEnumerator HideDialogCoroutine()
        {
            // 播放隐藏动画
            if (hideAnimationDuration > 0 && dialogPanel != null)
            {
                Transform dialogTransform = dialogPanel.transform;

                float elapsed = 0f;
                while (elapsed < hideAnimationDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / hideAnimationDuration;
                    dialogTransform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero,
                        AnimationCurve.EaseInOut(0, 0, 1, 1).Evaluate(t));
                    yield return null;
                }
            }

            // 隐藏弹窗
            if (dialogPanel != null)
            {
                dialogPanel.SetActive(false);
            }

            Debug.Log("[RewardDialogExample] 弹窗已隐藏");
        }

        /// <summary>
        /// 处理领奖流程
        /// </summary>
        private IEnumerator ProcessClaimReward()
        {
            isClaimProcessing = true;

            // 禁用按钮
            if (claimButton != null)
            {
                claimButton.interactable = false;
            }

            // 停止滑动并获取倍率
            if (multiplierManager != null)
            {
                currentMultiplier = multiplierManager.StopSliding();
            }

            // 计算最终奖励
            finalRewardAmount = MultiplierCalculator.CalculateFinalReward(baseRewardAmount, currentMultiplier);

            // 更新显示
            UpdateMultiplierDisplay();

            // 播放奖励数字动画
            if (countAnimationCoroutine != null)
            {
                StopCoroutine(countAnimationCoroutine);
            }
            countAnimationCoroutine = StartCoroutine(AnimateRewardCount());

            // 等待动画完成
            yield return new WaitForSeconds(rewardCountAnimationDuration);

            // 发放奖励
            GrantReward();

            // 移动到下一个配置
            if (multiplierManager != null)
            {
                multiplierManager.MoveToNextConfig();
            }

            // 等待一小段时间
            yield return new WaitForSeconds(0.5f);

            // 自动关闭弹窗
            HideDialog();

            isClaimProcessing = false;

            // 重新启用按钮
            if (claimButton != null)
            {
                claimButton.interactable = true;
            }
        }

        /// <summary>
        /// 奖励数字动画
        /// </summary>
        private IEnumerator AnimateRewardCount()
        {
            if (finalRewardText == null) yield break;

            int startValue = baseRewardAmount;
            int endValue = finalRewardAmount;

            float elapsed = 0f;
            while (elapsed < rewardCountAnimationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / rewardCountAnimationDuration;

                // 使用缓动曲线
                t = Mathf.SmoothStep(0, 1, t);

                int currentValue = Mathf.RoundToInt(Mathf.Lerp(startValue, endValue, t));
                finalRewardText.text = FormatReward(currentValue);

                yield return null;
            }

            finalRewardText.text = FormatReward(endValue);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 更新奖励显示
        /// </summary>
        private void UpdateRewardDisplay()
        {
            // 更新基础奖励文本
            if (baseRewardText != null)
            {
                baseRewardText.text = $"基础奖励: {FormatReward(baseRewardAmount)}";
            }

            // 更新最终奖励文本
            if (finalRewardText != null)
            {
                finalRewardText.text = FormatReward(finalRewardAmount);
            }

            UpdateMultiplierDisplay();
        }

        /// <summary>
        /// 更新倍率显示
        /// </summary>
        private void UpdateMultiplierDisplay()
        {
            if (multiplierDisplayText != null)
            {
                multiplierDisplayText.text = MultiplierCalculator.FormatMultiplier(currentMultiplier);
            }
        }

        /// <summary>
        /// 格式化奖励显示
        /// </summary>
        private string FormatReward(int amount)
        {
            string icon = GetRewardIcon();
            string formattedAmount = MultiplierCalculator.FormatReward(amount);
            return $"{icon} {formattedAmount}";
        }

        /// <summary>
        /// 获取奖励图标
        /// </summary>
        private string GetRewardIcon()
        {
            switch (rewardType)
            {
                case RewardType.Coins:
                    return "🪙";
                case RewardType.Gems:
                    return "💎";
                case RewardType.Stars:
                    return "⭐";
                case RewardType.Experience:
                    return "✨";
                default:
                    return "🎁";
            }
        }

        /// <summary>
        /// 发放奖励
        /// </summary>
        private void GrantReward()
        {
            // TODO: 这里调用实际的奖励系统发放奖励
            // 例如：RewardManager.Instance.GrantReward(rewardType, finalRewardAmount);

            Debug.Log($"[RewardDialogExample] 发放奖励: {rewardType} x{finalRewardAmount}");

            // 触发奖励发放事件
            OnRewardGranted?.Invoke(rewardType, finalRewardAmount);
        }

        #endregion

        #region Events

        /// <summary>
        /// 奖励发放事件
        /// </summary>
        public event Action<RewardType, int> OnRewardGranted;

        #endregion

        #region Debug

        /// <summary>
        /// 调试：测试显示弹窗
        /// </summary>
        [ContextMenu("Test - Show Dialog")]
        private void TestShowDialog()
        {
            ShowRewardDialog(100, RewardType.Coins);
        }

        /// <summary>
        /// 调试：测试不同类型奖励
        /// </summary>
        [ContextMenu("Test - Show Gems Dialog")]
        private void TestShowGemsDialog()
        {
            ShowRewardDialog(50, RewardType.Gems);
        }

        /// <summary>
        /// 调试：测试大额奖励
        /// </summary>
        [ContextMenu("Test - Show Large Reward")]
        private void TestShowLargeReward()
        {
            ShowRewardDialog(10000, RewardType.Coins);
        }

        #endregion
    }

    /// <summary>
    /// 提现监听器示例
    /// 展示如何监听提现事件并更新倍率配置
    /// </summary>
    public class WithdrawListenerExample : MonoBehaviour
    {
        private MultiplierManager multiplierManager;

        private void Start()
        {
            // 获取管理器引用
            multiplierManager = MultiplierManager.Instance;

            // TODO: 监听实际的提现事件
            // WithdrawManager.OnWithdrawSuccess += HandleWithdraw;

            // 示例：监听键盘测试
            #if UNITY_EDITOR
            Debug.Log("[WithdrawListenerExample] 按W键模拟提现事件");
            #endif
        }

        private void Update()
        {
            #if UNITY_EDITOR
            // 测试用：按W键模拟提现
            if (Input.GetKeyDown(KeyCode.W))
            {
                SimulateWithdraw();
            }
            #endif
        }

        /// <summary>
        /// 处理提现事件
        /// </summary>
        private void HandleWithdraw()
        {
            if (multiplierManager != null)
            {
                // 切换到提现后配置
                multiplierManager.SwitchToWithdrawConfig();

                // 重置提现后配置索引
                multiplierManager.ResetPostWithdrawConfig();

                Debug.Log("[WithdrawListenerExample] 提现事件已处理，切换到提现后配置");
            }
        }

        /// <summary>
        /// 模拟提现（测试用）
        /// </summary>
        [ContextMenu("Simulate Withdraw")]
        private void SimulateWithdraw()
        {
            Debug.Log("[WithdrawListenerExample] 模拟提现事件");
            HandleWithdraw();
        }
    }
}