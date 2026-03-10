// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

using System;
using UnityEngine;
using TMPro;
using BlockPuzzleGameToolkit.Scripts.GUI;
using BlockPuzzleGameToolkit.Scripts.LevelsData;
using BlockPuzzleGameToolkit.Scripts.CurrencySystem;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using System.Collections;
using BlockPuzzleGameToolkit.Scripts.Utils;
using UnityEngine.UI;
using BlockPuzzleGameToolkit.Scripts.Audio;
using BlockPuzzleGameToolkit.Scripts.FlyRewardSystem.Data;
using BlockPuzzle.NativeBridge;
using BlockPuzzle.AdSystem.Managers;
using BlockPuzzleGameToolkit.Scripts.Localization;

namespace BlockPuzzleGameToolkit.Scripts.Popups.Reward
{
    /// <summary>
    /// 奖励弹窗基类
    /// 提供固定倍率和滑动倍率弹窗的通用功能
    /// </summary>
    public abstract class RewardPopupBase : Popup
    {
        #region Fields

        [Header("Reward Data")]
        [SerializeField] protected int baseReward;             // 基础奖励数值（放大10000倍的整数值）
        [SerializeField] protected int currentMultiplier = 1;  // 当前选中的倍率
        [SerializeField] protected int finalReward;            // 最终奖励（基础*倍率，放大10000倍的整数值）

        // 配置引用
        protected LevelRewardConfig rewardConfig;
        protected int levelNumber;
        protected string rewardSource;

        [Header("Common UI Elements")]
        public Text baseRewardText;         // 基础奖励显示
        public Text multiplierDisplayText;             // 倍率显示文本
        public Button skipButton;                      // 跳过按钮（不领奖）
        public Button claimFreeButton;                 // 免费领取按钮
        public Button claimAdButton;                   // 看广告领取按钮

        [Header("Reward Images")]
        public Image img_reward;                       // 奖励图片（正常显示）
        public Image img_white;                        // 白包图片（白包模式显示）

        [Header("Animation")]
        public float rewardAnimDuration = 0.5f;        // 奖励数字动画时长

        [Header("Ad Entry Names")]
        [SerializeField] protected string rewardedAdEntryName;         // 看广告领奖按钮的广告入口名称
        [SerializeField] protected string interstitialAdEntryName;     // 跳过按钮的广告入口名称

        // 白包模式标志 - 现在从NativeBridgeManager动态获取
        // private static bool isWhitePackageMode = false;  // [已废弃] 改为从NativeBridgeManager获取

        // 防重复点击标志
        protected bool isProcessingClick = false;        // 防止按钮重复点击（protected让子类可以访问）

        // 性能优化：缓存WaitForSeconds
        private readonly WaitForSeconds waitForDelayedClose = new WaitForSeconds(0.5f);

        #endregion

        #region Events

        /// <summary>
        /// 奖励领取事件（奖励金额（放大10000倍的整数值），使用的倍率）
        /// </summary>
        public static event Action<int, int> OnRewardClaimed;

        /// <summary>
        /// 奖励跳过事件（跳过时获得的奖励（放大10000倍的整数值））
        /// </summary>
        public static event Action<int> OnRewardSkipped;

        /// <summary>
        /// 触发奖励领取事件（供子类调用）
        /// </summary>
        protected void TriggerRewardClaimed(int reward, int multiplier)
        {
            OnRewardClaimed?.Invoke(reward, multiplier);
        }

        /// <summary>
        /// 触发奖励跳过事件（供子类调用）
        /// </summary>
        protected void TriggerRewardSkipped(int reward)
        {
            OnRewardSkipped?.Invoke(reward);
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// 初始化倍率UI（由子类实现）
        /// </summary>
        protected abstract void InitializeMultiplierUI();

        /// <summary>
        /// 获取当前选中的倍率（由子类实现）
        /// </summary>
        protected abstract int GetSelectedMultiplier();

        /// <summary>
        /// 更新倍率显示（由子类实现）
        /// </summary>
        protected abstract void UpdateMultiplierDisplay();

        #endregion

        #region Public Methods

        /// <summary>
        /// 设置白包模式（此方法已废弃，白包模式由NativeBridgeManager统一管理）
        /// </summary>
        /// <param name="isWhitePackage">是否为白包模式</param>
        [System.Obsolete("白包模式由NativeBridgeManager统一管理，此方法仅供兼容性保留")]
        public static void SetWhitePackageMode(bool isWhitePackage)
        {
            Debug.LogWarning($"[RewardPopupBase] SetWhitePackageMode已废弃，白包模式由NativeBridgeManager统一管理。当前白包模式: {GetWhitePackageMode()}");
        }

        /// <summary>
        /// 获取当前是否为白包模式（从NativeBridgeManager获取）
        /// </summary>
        /// <returns>白包模式状态</returns>
        public static bool GetWhitePackageMode()
        {
            // 从NativeBridgeManager获取白包模式
            if (NativeBridgeManager.Instance != null)
            {
                return NativeBridgeManager.Instance.IsWhitePackage();
            }

            // 如果NativeBridgeManager不可用，返回false
            return false;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// 初始化奖励弹窗
        /// </summary>
        /// <param name="reward">基础奖励金额（放大10000倍的整数值）</param>
        /// <param name="config">奖励配置</param>
        /// <param name="level">关卡编号</param>
        /// <param name="source">奖励来源</param>
        public virtual void Initialize(int reward, LevelRewardConfig config, int level = 0, string source = "LevelComplete")
        {
            baseReward = reward;
            rewardConfig = config;
            levelNumber = level;
            rewardSource = source;

            //查找UI
            FindChildUI();

            // 设置UI文本
            SetupUITexts();

            // 初始化图片显示（白包判断）
            InitializeRewardImages();

            // 初始化倍率UI（由子类实现）
            InitializeMultiplierUI();

            // 设置按钮事件
            SetupButtons();

            // 更新显示
            UpdateRewardDisplay();

            Debug.Log($"[RewardPopupBase] 初始化完成 - 基础奖励: ${baseReward/10000f:F3}, 类型: {config.PopupType}, 白包模式: {GetWhitePackageMode()}");
        }


        /// <summary>
        /// 查找UI组件
        /// </summary>
        protected virtual void FindChildUI()
        {

        }

        /// <summary>
        /// 初始化奖励图片显示
        /// </summary>
        protected virtual void InitializeRewardImages()
        {
            // 根据白包模式显示不同的图片
            if (img_reward != null && img_white != null)
            {
                if (GetWhitePackageMode())
                {
                    // 白包模式：显示白包图片
                    img_reward.gameObject.SetActive(false);
                    img_white.gameObject.SetActive(true);
                    Debug.Log("[RewardPopupBase] 白包模式：显示白包图片");
                }
                else
                {
                    // 正常模式：显示奖励图片
                    img_reward.gameObject.SetActive(true);
                    img_white.gameObject.SetActive(false);
                    Debug.Log("[RewardPopupBase] 正常模式：显示奖励图片");
                }
            }
            else
            {
                if (img_reward == null)
                    Debug.LogWarning("[RewardPopupBase] img_reward 未设置");
                if (img_white == null)
                    Debug.LogWarning("[RewardPopupBase] img_white 未设置");
            }
        }

        /// <summary>
        /// 设置UI文本
        /// </summary>
        protected virtual void SetupUITexts()
        {
            // 获取白包模式状态
            bool isWhitePackageMode = GetWhitePackageMode();

            // 基础奖励文本
            if (baseRewardText != null)
            {
                // 白包模式下隐藏基础奖励文本
                if (isWhitePackageMode)
                {
                    baseRewardText.gameObject.SetActive(false);
                    Debug.Log("[RewardPopupBase] 白包模式：隐藏基础奖励文本");
                }
                else
                {
                    baseRewardText.gameObject.SetActive(true);
                    // 从CurrencyManager获取格式化字符串
                    if (CurrencyManager.Instance != null)
                    {
                        // 使用CurrencyManager的FormatCurrency方法，它会使用当前货币类型
                        string formattedReward = CurrencyManager.Instance.FormatCurrency(baseReward, true); // true表示应用汇率
                        baseRewardText.text = formattedReward;
                        Debug.Log($"[RewardPopupBase] 奖励显示: {formattedReward}");
                    }
                    else
                    {
                        // 如果CurrencyManager未初始化，使用默认格式
                        float baseRewardDollar = baseReward / 10000f;
                        baseRewardText.text = $"${baseRewardDollar:F3}";
                        Debug.LogWarning("[RewardPopupBase] CurrencyManager未初始化，使用默认货币格式");
                    }
                }
            }
        }

        /// <summary>
        /// 设置按钮事件
        /// </summary>
        protected virtual void SetupButtons()
        {
            // 获取白包模式状态
            bool isWhitePackageMode = GetWhitePackageMode();

            // 白包模式下的按钮配置
            if (isWhitePackageMode)
            {
                // 隐藏跳过按钮
                if (skipButton != null)
                {
                    skipButton.gameObject.SetActive(false);
                    Debug.Log("[RewardPopupBase] 白包模式：隐藏跳过按钮");
                }

                // 隐藏看广告领奖按钮
                if (claimAdButton != null)
                {
                    claimAdButton.gameObject.SetActive(false);
                    Debug.Log("[RewardPopupBase] 白包模式：隐藏看广告领奖按钮");
                }

                // 显示免费领取按钮，设置文字为 "Claim"
                if (claimFreeButton != null)
                {
                    claimFreeButton.gameObject.SetActive(true);
                    var buttonText = claimFreeButton.GetComponentInChildren<Text>();
                    if (buttonText != null)
                    {
                        //获取Claim的多语言文本
                        string claimText = LocalizationManager.GetText("Claim","Claim");
                        buttonText.text = claimText;
                    }
                    Debug.Log("[RewardPopupBase] 白包模式：显示免费领取按钮 (Claim)");
                }
            }
            else
            {
                // 非白包模式下的按钮配置

                // 跳过按钮显示逻辑：只有当 ShowRewardAd = true 时才显示跳过按钮
                // 即使 ShowInterstitialAd = true，但 ShowRewardAd = false 时也不显示跳过按钮
                if (skipButton != null)
                {
                    bool showSkipButton = rewardConfig != null && rewardConfig.ShowRewardAd;
                    skipButton.gameObject.SetActive(showSkipButton);

                    if (!showSkipButton)
                    {
                        Debug.Log("[RewardPopupBase] ShowRewardAd = false，隐藏跳过按钮");
                    }
                    else
                    {
                        Debug.Log("[RewardPopupBase] ShowRewardAd = true，显示跳过按钮");
                    }
                }

                // 看广告领奖按钮显示逻辑
                if (claimAdButton != null)
                {
                    bool showAdButton = rewardConfig != null && rewardConfig.ShowRewardAd;
                    claimAdButton.gameObject.SetActive(showAdButton);

                    if (showAdButton)
                    {
                        Debug.Log("[RewardPopupBase] 显示看广告领奖按钮");
                    }
                }

                // 免费领取按钮显示逻辑
                if (claimFreeButton != null)
                {
                    bool showFreeButton = rewardConfig == null || !rewardConfig.ShowRewardAd;
                    claimFreeButton.gameObject.SetActive(showFreeButton);

                    if (showFreeButton)
                    {
                        Debug.Log("[RewardPopupBase] 显示免费领取按钮");
                    }
                }
            }
        }

        #endregion

        #region Button Handlers

        /// <summary>
        /// 跳过按钮点击处理
        /// </summary>
        protected virtual void OnSkipButtonClicked()
        {
            // 防重复点击检查
            if (isProcessingClick)
            {
                Debug.Log("[RewardPopupBase] 正在处理中，忽略重复点击");
                return;
            }

            isProcessingClick = true; // 设置处理标志

            // 禁用所有按钮交互
            if (claimFreeButton != null) claimFreeButton.interactable = false;
            if (claimAdButton != null) claimAdButton.interactable = false;
            if (skipButton != null) skipButton.interactable = false;

            Debug.Log("[RewardPopupBase] 跳过奖励");

            // 计算跳过奖励（使用配置的跳过倍率）
            int skipReward = Mathf.RoundToInt(baseReward * rewardConfig.SkipMultiplier);

            // 如果配置了插屏广告，显示广告
            if (rewardConfig.ShowInterstitialAd)
            {
                ShowInterstitialAd((success) =>
                {
                    // 检查对象是否仍然存在
                    if (this == null)
                    {
                        Debug.LogWarning("[RewardPopupBase] 插屏广告回调时弹窗对象已被销毁");
                        return;
                    }

                    if (success)
                    {
                        // 广告播放成功，发放跳过奖励
                        GrantReward(skipReward, 0);
                        TriggerRewardSkipped(skipReward);
                        result = EPopupResult.Skip;
                        Close();
                    }
                    else
                    {
                        // 广告播放失败，关闭弹窗并累加关卡进度
                        Debug.LogWarning("[RewardPopupBase] 插屏广告播放失败，关闭弹窗并进入下一关");
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
                // 直接发放跳过奖励
                GrantReward(skipReward, 0);
                TriggerRewardSkipped(skipReward);
                result = EPopupResult.Skip;
                // 移除此处的重置，由Close()方法统一处理
                Close();
            }
        }

        /// <summary>
        /// 领取按钮点击处理
        /// </summary>
        protected virtual void OnClaimButtonClicked()
        {
            // 防重复点击检查
            if (isProcessingClick)
            {
                Debug.Log("[RewardPopupBase] 正在处理中，忽略重复点击");
                return;
            }

            isProcessingClick = true; // 设置处理标志

            // 禁用所有按钮交互
            if (claimFreeButton != null) claimFreeButton.interactable = false;
            if (claimAdButton != null) claimAdButton.interactable = false;
            if (skipButton != null) skipButton.interactable = false;

            Debug.Log("[RewardPopupBase] 领取奖励");

            // 获取白包模式状态
            bool isWhitePackageMode = GetWhitePackageMode();

            if (isWhitePackageMode)
            {
                // 白包模式：使用基础奖励（倍率为1），直接领取
                currentMultiplier = 1;
                finalReward = baseReward;

                // 直接发放奖励（不播放倍率动画）
                GrantReward(finalReward, currentMultiplier);
                TriggerRewardClaimed(finalReward, currentMultiplier);
                result = EPopupResult.Claimed;
                // 移除此处的重置，由Close()方法统一处理
                Close();

                Debug.Log($"[RewardPopupBase] 白包模式：直接领取奖励 ${finalReward/10000f:F3}");
            }
            else
            {
                // 标准模式：原有逻辑
                // 获取当前选中的倍率
                currentMultiplier = GetSelectedMultiplier();

                // 计算最终奖励
                finalReward = baseReward * currentMultiplier;

                // 如果配置了激励广告，显示广告
                if (rewardConfig.ShowRewardAd)
                {
                    ShowRewardedAd(success =>
                    {
                        // 检查对象是否仍然存在（防止广告播放期间弹窗被销毁）
                        if (this == null)
                        {
                            Debug.LogWarning("[RewardPopupBase] 广告回调时弹窗对象已被销毁");
                            return;
                        }

                        if (success)
                        {
                            // 广告观看成功，发放奖励
                            GrantReward(finalReward, currentMultiplier);
                            TriggerRewardClaimed(finalReward, currentMultiplier);
                            result = EPopupResult.Claimed;
                            // 移除此处的重置，由Close()方法统一处理
                            Close();
                        }
                        else
                        {
                            // 广告失败，提示玩家
                            Debug.LogWarning("[RewardPopupBase] 广告观看失败");
                            isProcessingClick = false; // 广告失败时需要重置，允许重试
                        }
                    });
                }
                else
                {
                    // 免费领取
                    GrantReward(finalReward, currentMultiplier);
                    TriggerRewardClaimed(finalReward, currentMultiplier);
                    result = EPopupResult.Claimed;
                    // 移除此处的重置，由Close()方法统一处理
                    Close();
                }
            }
        }

        #endregion

        #region Override Methods

        /// <summary>
        /// 重写弹窗显示音效，播放胜利音效
        /// </summary>
        public override void ShowAnimationSound()
        {
            // 播放胜利音效
            if (SoundBase.Instance != null)
            {
                // 优先播放win音效
                if (SoundBase.Instance.win != null)
                {
                    SoundBase.Instance.PlaySound(SoundBase.Instance.win);
                }
                // 如果没有win音效，回退到基类的默认音效
                else
                {
                    base.ShowAnimationSound();
                }
            }
        }

        #endregion

        #region Reward Methods

        /// <summary>
        /// 发放奖励
        /// </summary>
        /// <param name="amount">奖励金额（放大10000倍的整数值）</param>
        /// <param name="multiplier">使用的倍率</param>
        protected virtual void GrantReward(int amount, int multiplier)
        {
            // 添加货币（直接使用整数值）
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.AddCoins(amount);
            }

            //播放奖励动画
            PlayRewardAnimation();
        }

        protected virtual void PlayRewardAnimation()
        {
            // 检查对象是否存在（防止在广告回调中调用时对象已被销毁）
            if (this == null)
            {
                Debug.LogWarning("[RewardPopupBase] PlayRewardAnimation: 对象已被销毁");
                return;
            }

            // 使用新的飞行奖励系统
            var flyRewardManager = BlockPuzzleGameToolkit.Scripts.FlyRewardSystem.Core.FlyRewardManager.Instance;
            if (flyRewardManager != null)
            {
                // 获取起始位置（使用弹窗中心位置）
                Vector3 startWorldPos = transform.position;

                // 创建飞行请求
                FlyRewardRequest request = new FlyRewardRequest
                {
                    rewardType = GetWhitePackageMode() ? FlyRewardType.WhitePackage : FlyRewardType.Cash,
                    animationPattern = FlyAnimationPattern.FireworkBurst,
                    startWorldPosition = startWorldPos,
                    itemCount = 10,
                    rewardAmount = finalReward,
                    autoUpdateCurrency = false, // 货币已经在GrantReward中更新了
                    playSound = true,
                    duration = 1f,
                    onComplete = () =>
                    {
                        Debug.Log("[RewardPopupBase] 飞行奖励动画完成");

                        // 检查是否需要进入下一关
                        if (GameDataManager.HasMoreLevels())
                        {
                            GameManager.Instance.NextLevel();
                            Debug.Log("[RewardPopupBase] 已触发进入下一关");
                        }
                        else
                        {
                            GameManager.Instance.RestartLevel();
                            Debug.Log("[RewardPopupBase] 没有更多关卡，返回主菜单");
                        }
                    },
                    onFirstItemReached = () =>
                    {
                        // 第一个物体到达时触发货币滚动动画
                        EventManager.GetEvent(Enums.EGameEvent.CurrencyChanged).Invoke();
                    }
                };

                // 播放飞行动画
                flyRewardManager.PlayFlyAnimation(request);

                // 在第一阶段动画结束后关闭弹窗（烟花散开阶段完成）
                // 第一阶段占总时长的30%（默认1秒 * 0.3 = 0.3秒）
                float firstStageDelay = request.duration * 0.3f;

                // 再次检查对象是否存在
                if (this != null)
                {
                    StartCoroutine(DelayedClosePopup(firstStageDelay));
                }
            }
            else
            {
                Debug.LogError("[RewardPopupBase] FlyRewardManager 未初始化");
                // 直接进入下一关
                if (GameDataManager.HasMoreLevels())
                {
                    GameManager.Instance.NextLevel();
                }
                else
                {
                    GameManager.Instance.RestartLevel();
                }
                Close();
            }
        }

        /// <summary>
        /// 延迟关闭弹窗
        /// </summary>
        private IEnumerator DelayedClosePopup(float delay)
        {
            Debug.Log($"[RewardPopupBase] 将在{delay}秒后关闭弹窗（第一阶段动画结束）");

            // 等待第一阶段动画完成（烟花散开）
            yield return new WaitForSeconds(delay);

            Debug.Log($"[RewardPopupBase] 第一阶段动画完成，关闭弹窗");

            // 关闭弹窗（飞行动画会继续在 FlyRewardSystem 中播放）
            Close();
        }

        /// <summary>
        /// 更新奖励显示
        /// </summary>
        protected virtual void UpdateRewardDisplay()
        {
            currentMultiplier = GetSelectedMultiplier();
            finalReward = baseReward * currentMultiplier;

            // 更新倍率相关显示（由子类实现）
            UpdateMultiplierDisplay();
        }

        /// <summary>
        /// 更新领取按钮状态 按钮上的文本依据弹窗类型变化，若是FixedRewardPopup，那么按钮上显示的数值为固定倍率乘以基础奖励，若是SlidingRewardPopup，那么按钮上显示的数值为滑动倍率乘以基础奖励
        /// </summary>
        protected virtual void UpdateClaimButton(float multiplier)
        {
            bool showRewardAd = rewardConfig != null && rewardConfig.ShowRewardAd;
            int currentReward = (int)(baseReward * multiplier);
            // 更新免费领取按钮
            if (claimFreeButton != null)
            {
                var freeText = claimFreeButton.GetComponentInChildren<TextMeshProUGUI>();
                if (freeText != null)
                {
                    //获取当前奖励数值
                    freeText.text = CurrencyManager.Instance.FormatCurrency(currentReward, true);
                }
                claimFreeButton.gameObject.SetActive(!showRewardAd);
            }

            // 更新广告领取按钮
            if (claimAdButton != null)
            {
                var adText = claimAdButton.GetComponentInChildren<TextMeshProUGUI>();
                if (adText != null)
                {
                    adText.text = CurrencyManager.Instance.FormatCurrency(currentReward, true);
                }
                claimAdButton.gameObject.SetActive(showRewardAd);
            }
        }

        #endregion

        #region Ad Integration

        /// <summary>
        /// 显示激励广告
        /// </summary>
        /// <param name="callback">广告结束回调</param>
        protected virtual void ShowRewardedAd(Action<bool> callback)
        {
            // 检查是否配置了广告入口名称
            if (string.IsNullOrEmpty(rewardedAdEntryName))
            {
                Debug.LogWarning("[RewardPopupBase] 激励广告入口名称未配置，使用默认行为");
                callback?.Invoke(true);
                return;
            }

            // 调用 AdSystemManager 播放广告
            if (AdSystemManager.Instance != null)
            {
                Debug.Log($"[RewardPopupBase] 播放激励广告: {rewardedAdEntryName}");
                AdSystemManager.Instance.PlayAd(rewardedAdEntryName, (success) =>
                {
                    // 检查对象是否仍然存在（防止广告播放期间弹窗被销毁）
                    if (this == null)
                    {
                        Debug.LogWarning("[RewardPopupBase] 激励广告回调时弹窗对象已被销毁");
                        return;
                    }

                    Debug.Log($"[RewardPopupBase] 激励广告播放结果: {success}");
                    callback?.Invoke(success);
                });
            }
            else
            {
                Debug.LogError("[RewardPopupBase] AdSystemManager 未初始化");
                callback?.Invoke(false);
            }
        }

        /// <summary>
        /// 显示插屏广告
        /// </summary>
        /// <param name="callback">广告结束回调，参数为广告播放是否成功</param>
        protected virtual void ShowInterstitialAd(Action<bool> callback)
        {
            // 检查是否配置了广告入口名称
            if (string.IsNullOrEmpty(interstitialAdEntryName))
            {
                Debug.LogWarning("[RewardPopupBase] 插屏广告入口名称未配置，使用默认行为");
                callback?.Invoke(true);
                return;
            }

            // 调用 AdSystemManager 播放广告
            if (AdSystemManager.Instance != null)
            {
                Debug.Log($"[RewardPopupBase] 播放插屏广告: {interstitialAdEntryName}");
                AdSystemManager.Instance.PlayAd(interstitialAdEntryName, (success) =>
                {
                    // 检查对象是否仍然存在（防止广告播放期间弹窗被销毁）
                    if (this == null)
                    {
                        Debug.LogWarning("[RewardPopupBase] 插屏广告回调时弹窗对象已被销毁");
                        return;
                    }

                    Debug.Log($"[RewardPopupBase] 插屏广告播放结果: {success}");
                    // 传递广告播放结果
                    callback?.Invoke(success);
                });
            }
            else
            {
                Debug.LogError("[RewardPopupBase] AdSystemManager 未初始化");
                callback?.Invoke(false);
            }
        }

        #endregion

        #region Popup Overrides

        protected override void Awake()
        {
            base.Awake();
        }

        public override void AfterShowAnimation()
        {
            base.AfterShowAnimation();
            // 弹窗显示动画结束后的处理
        }

        public override void Close()
        {
            // 重置防重复点击标志
            isProcessingClick = false;

            // 禁用交互
            StopInteration();

            // 记录分析数据
            LogAnalytics();

            // 注意：进入下一关的逻辑现在由RewardPanel处理
            // RewardPanel会在所有动画播放完成后自动调用NextLevel
            if (result == EPopupResult.Claimed || result == EPopupResult.Skip)
            {
                Debug.Log("[RewardPopupBase] 奖励处理完成，RewardPanel将负责进入下一关");
            }

            // 重置 RewardPopupManager 的状态标志（防止广告失败时状态未重置）
            if (RewardPopupManager.Instance != null)
            {
                RewardPopupManager.Instance.ResetShowingState();
            }

            base.Close();
        }

        #endregion

        #region Analytics

        /// <summary>
        /// 记录分析数据
        /// </summary>
        protected virtual void LogAnalytics()
        {
            // TODO: 集成分析系统
            // 记录：关卡、奖励类型、倍率选择、是否观看广告等
            Debug.Log($"[RewardPopupBase] 分析数据 - 关卡: {levelNumber}, 结果: {result}, 倍率: {currentMultiplier}");
        }

        #endregion
    }
}