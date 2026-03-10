// 漂浮泡泡活动 - 核心逻辑
// 创建日期: 2026-03-09

using System.Collections;
using UnityEngine;
using BlockPuzzleGameToolkit.Scripts.Activity.Core;
using BlockPuzzleGameToolkit.Scripts.Activity.Data;
using BlockPuzzleGameToolkit.Scripts.Activity.UI;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using BlockPuzzleGameToolkit.Scripts.Popups;
using BlockPuzzleGameToolkit.Scripts.RewardSystem;
using BlockPuzzleGameToolkit.Scripts.Enums;

namespace BlockPuzzleGameToolkit.Scripts.Activity.Examples
{
    /// <summary>
    /// 漂浮泡泡活动模块
    /// 功能: 在Canvas上显示可运动的泡泡图标，点击领取广告奖励
    /// </summary>
    public class FloatBubbleActivity : ActivityModule
    {
        #region Static Registration

        /// <summary>
        /// 注册活动模块（在ActivityManager.OnInit()之前调用）
        /// </summary>
        public static void Register()
        {
            ActivityManager.RegisterModule(
                activityName: "FloatBubble",
                createFunc: () => new FloatBubbleActivity()
            );
        }

        #endregion

        #region Fields

        private FloatingBubbleSettings settings;
        private FloatingBubbleData activityData;

        // Icon相关
        private ActivityIcon bubbleIcon;

        // 冷却协程
        private Coroutine cooldownCoroutine;

        // 协程等待缓存（避免GC分配）
        private static readonly WaitForSeconds waitOneSecond = new WaitForSeconds(1f);

        // 调试开关
        private bool enableDebugLog = true;

        #endregion

        #region Lifecycle Override

        /// <summary>
        /// 初始化
        /// </summary>
        public override void Initialize(ActivityConfig activityConfig)
        {
            base.Initialize(activityConfig);

            // 加载配置
            settings = FloatingBubbleSettings.Instance;
            if (settings == null)
            {
                LogError("FloatingBubbleSettings未找到");
                return;
            }

            // 验证配置
            if (!settings.Validate())
            {
                LogError("FloatingBubbleSettings验证失败");
                return;
            }

            // 加载持久化数据
            activityData = FloatingBubbleData.Load();
            if (activityData == null)
            {
                LogError("加载FloatingBubbleData失败");
                return;
            }

            // 初始化时清除冷却时间（每次启动游戏都能看到活动）
            if (activityData.IsInCooldown())
            {
                Log($"初始化时清除冷却时间（剩余:{activityData.GetRemainingCooldown():F1}秒）");
                activityData.ClearCooldown();
                activityData.Save();
            }

            Log($"FloatBubbleActivity初始化完成 - {activityData.GetDebugInfo()}");
        }

        /// <summary>
        /// 每帧更新 - 尝试初始化Icon
        /// </summary>
        public override void OnUpdate(float deltaTime)
        {
            if (!isInitialized)
            {
                return;
            }

            // 尝试初始化Icon（如果还未初始化）
            TryInitializeIcon();
        }

        /// <summary>
        /// 刷新
        /// </summary>
        public override void OnRefresh(EActivityRefreshEvent refreshEvent)
        {
            if (!isInitialized)
            {
                return;
            }

            Log($"OnRefresh: {refreshEvent}");

            switch (refreshEvent)
            {
                case EActivityRefreshEvent.LevelCompleted:
                    // 关卡完成，检查是否解锁
                    CheckUnlockCondition();
                    break;

                case EActivityRefreshEvent.UserAction:
                case EActivityRefreshEvent.SceneChanged:
                    // 场景切换或手动刷新，重新加载数据
                    activityData = FloatingBubbleData.Load();
                    CheckUnlockCondition();
                    break;
            }

            // 基类方法会自动检查并通知显示条件变化
            base.OnRefresh(refreshEvent);

            // 如果Icon已显示，尝试初始化运动
            TryInitializeIcon();
        }

        /// <summary>
        /// 销毁
        /// </summary>
        public override void OnDestroy()
        {
            // 停止冷却协程
            if (cooldownCoroutine != null)
            {
                var manager = ActivityManager.Instance;
                if (manager != null)
                {
                    manager.StopCoroutine(cooldownCoroutine);
                }
                cooldownCoroutine = null;
            }

            // 清理引用
            bubbleIcon = null;

            Log("FloatBubbleActivity销毁");
            base.OnDestroy();
        }

        #endregion

        #region Visibility Logic

        /// <summary>
        /// 判断活动是否应该显示
        /// 条件: 已解锁 且 不在冷却中
        /// </summary>
        public override bool CanShow()
        {
            if (!isInitialized || activityData == null)
            {
                return false;
            }

            // 条件1: 已解锁
            if (!activityData.isUnlocked)
            {
                if (enableDebugLog)
                {
                    Log("未解锁");
                }
                return false;
            }

            // 条件2: 不在冷却中
            if (activityData.IsInCooldown())
            {
                if (enableDebugLog)
                {
                    Log($"冷却中，剩余: {activityData.GetRemainingCooldown():F1}秒");
                }
                return false;
            }

            return true;
        }

        #endregion

        #region User Interaction Override

        /// <summary>
        /// 点击泡泡
        /// </summary>
        public override void OnIconClicked()
        {
            if (!isInitialized)
            {
                LogWarning("OnIconClicked: 模块未初始化");
                return;
            }

            Log("泡泡被点击");

            // 停止FloatBubbleIcon的飘动
            var floatBubbleIcon = bubbleIcon as FloatBubbleIcon;
            if (floatBubbleIcon != null)
            {
                floatBubbleIcon.StopFloating();
                Log("FloatBubbleIcon飘动已停止");
            }

            // 打开CommonRewardPopup
            OpenRewardPopup();
        }

        /// <summary>
        /// 弹窗关闭（不使用，改用RewardPopupConfig的回调）
        /// </summary>
        public override void OnPopupClosed()
        {
            // 不使用此回调，使用RewardPopupConfig.OnRewardClaimed
        }

        #endregion

        #region Icon Management

        /// <summary>
        /// 尝试初始化Icon（如果Icon已显示但未初始化）
        /// </summary>
        private void TryInitializeIcon()
        {
            // 已初始化，跳过
            if (bubbleIcon != null)
            {
                return;
            }

            // 获取Icon引用
            var iconManager = GetIconManager();
            if (iconManager == null)
            {
                return;
            }

            // 检查Icon是否已显示
            if (!iconManager.IsIconShowing(config.ActivityId))
            {
                return;
            }

            // 获取Icon
            bubbleIcon = iconManager.GetActiveIcon(config.ActivityId);
            if (bubbleIcon == null)
            {
                return;
            }

            // 类型转换为FloatBubbleIcon
            var floatBubbleIcon = bubbleIcon as FloatBubbleIcon;
            if (floatBubbleIcon == null)
            {
                LogWarning($"Icon不是FloatBubbleIcon类型: {bubbleIcon.GetType().Name}");
                return;
            }

            // 加载settings到Icon
            floatBubbleIcon.LoadSettings(settings);

            // 计算并更新Icon的奖励显示
            UpdateIconRewardDisplay();

            Log("泡泡Icon准备完成");
        }

        /// <summary>
        /// 获取IconManager
        /// </summary>
        private ActivityIconManager GetIconManager()
        {
            var manager = ActivityManager.Instance;
            if (manager == null)
            {
                return null;
            }

            return manager.GetIconManager();
        }

        #endregion

        #region Reward Popup

        /// <summary>
        /// 打开奖励弹窗
        /// </summary>
        private void OpenRewardPopup()
        {
            if (settings == null)
            {
                LogError("Settings未加载");
                return;
            }

            // 从FloatBubbleIcon读取已缓存的奖励数值（在Icon显示时已计算）
            var floatBubbleIcon = bubbleIcon as FloatBubbleIcon;
            if (floatBubbleIcon == null)
            {
                LogError("bubbleIcon不是FloatBubbleIcon类型");
                return;
            }

            int baseReward = floatBubbleIcon.GetCachedBaseReward();
            float adMultiplier = floatBubbleIcon.GetCachedAdMultiplier();

            if (baseReward <= 0)
            {
                LogError($"Icon缓存的奖励数值无效: {baseReward}");
                return;
            }

            Log($"使用Icon缓存的奖励数值 - 基础奖励: {baseReward}, 倍率: {adMultiplier}x");

            // 构建RewardPopupConfig
            var config = new RewardPopupConfig
            {
                BaseReward = baseReward,
                AdMultiplier = adMultiplier,
                NoAdMultiplier = 0f, // 隐藏单倍领奖按钮
                AdEntryName = settings.AdEntryName,
                AutoPlayFlyAnimation = true,
                FlyingCoinCount = 10,
                FlyStartPosition = null, // 使用默认弹窗位置
                OnRewardClaimed = OnRewardClaimed
            };

            // 打开弹窗
            var menuManager = MenuManager.Instance;
            if (menuManager == null)
            {
                LogError("MenuManager未找到");
                return;
            }

            string popupPath = "Popups/CommonRewardPopup"; // 固定路径

            // 先打开弹窗，再初始化配置
            var popup = menuManager.ShowPopup(popupPath);
            if (popup == null)
            {
                LogError("弹窗打开失败");
                return;
            }

            // 类型转换并初始化
            var rewardPopup = popup as CommonRewardPopup;
            if (rewardPopup != null)
            {
                rewardPopup.Initialize(config);
                Log("已打开奖励弹窗");
            }
            else
            {
                LogError($"弹窗类型错误，期望CommonRewardPopup，实际: {popup.GetType().Name}");
                popup.Close();
            }
        }

        /// <summary>
        /// 奖励领取回调
        /// </summary>
        private void OnRewardClaimed(RewardClaimResult result)
        {
            if (result == null)
            {
                LogError("RewardClaimResult为null");
                return;
            }

            Log($"奖励领取回调 - 类型:{result.ClaimType}, 成功:{result.Success}, 金额:{result.FinalReward}");

            if (result.Success)
            {
                // 广告成功：发放奖励，隐藏泡泡，进入冷却
                Log($"广告成功，发放奖励: {result.FinalReward}");

                // 发放奖励（增加货币）
                var currencyManager = Scripts.CurrencySystem.CurrencyManager.Instance;
                if (currencyManager != null)
                {
                    currencyManager.AddCoins(result.FinalReward);
                    Log($"已增加货币: {result.FinalReward}");

                    // 触发货币变化事件，通知TopPanel刷新
                    EventManager.GetEvent(EGameEvent.CurrencyChanged).Invoke();
                    Log("已触发CurrencyChanged事件");
                }
                else
                {
                    LogError("CurrencyManager未找到，无法发放奖励");
                }

                // 增加领取次数
                activityData.IncrementClaimCount();
                activityData.Save();

                // 隐藏泡泡
                HideBubble();

                // 进入冷却
                EnterCooldown();
            }
            else
            {
                // 广告失败：恢复运动（无提示）
                Log("广告失败，恢复泡泡运动");

                // 恢复FloatBubbleIcon的飘动
                var floatBubbleIcon = bubbleIcon as FloatBubbleIcon;
                if (floatBubbleIcon != null)
                {
                    floatBubbleIcon.StartFloating();
                    Log("FloatBubbleIcon飘动已恢复");
                }
            }
        }

        #endregion

        #region Cooldown Management

        /// <summary>
        /// 进入冷却
        /// </summary>
        private void EnterCooldown()
        {
            if (settings == null || activityData == null)
            {
                return;
            }

            // 设置冷却时间
            activityData.SetCooldown(settings.CooldownDuration);
            activityData.Save();

            Log($"进入冷却，时长: {settings.CooldownDuration}秒");

            // 启动冷却协程
            cooldownCoroutine = StartCoroutine(CooldownRoutine());

            // 触发刷新（隐藏Icon）
            CheckAndNotifyVisibilityChange();
        }

        /// <summary>
        /// 冷却协程
        /// </summary>
        private IEnumerator CooldownRoutine()
        {
            if (activityData == null)
            {
                yield break;
            }

            Log("冷却协程开始");

            // 等待冷却结束
            while (activityData.IsInCooldown())
            {
                yield return waitOneSecond; // 使用缓存，避免GC分配
            }

            Log("冷却结束");

            // 清空冷却协程引用
            cooldownCoroutine = null;

            // 触发刷新（显示Icon）
            CheckAndNotifyVisibilityChange();
        }

        #endregion

        #region Icon Display Update

        /// <summary>
        /// 更新Icon的奖励显示（每次Icon出现时调用）
        /// </summary>
        private void UpdateIconRewardDisplay()
        {
            Debug.Log("[FloatBubbleActivity] [开始] UpdateIconRewardDisplay");

            if (bubbleIcon == null)
            {
                Debug.LogWarning("[FloatBubbleActivity] UpdateIconRewardDisplay: bubbleIcon为null");
                return;
            }

            if (settings == null)
            {
                Debug.LogError("[FloatBubbleActivity] UpdateIconRewardDisplay: settings为null");
                return;
            }

            // 类型转换为FloatBubbleIcon
            var floatBubbleIcon = bubbleIcon as FloatBubbleIcon;
            if (floatBubbleIcon == null)
            {
                Debug.LogWarning($"[FloatBubbleActivity] UpdateIconRewardDisplay: Icon不是FloatBubbleIcon类型，实际类型: {bubbleIcon.GetType().Name}");
                return;
            }

            Debug.Log($"[FloatBubbleActivity] FloatBubbleIcon类型转换成功");

            // 使用RewardCalculator计算基础奖励
            var rewardCalculator = RewardCalculator.Instance;
            if (rewardCalculator == null)
            {
                Debug.LogError("[FloatBubbleActivity] UpdateIconRewardDisplay: RewardCalculator未找到");
                return;
            }

            Debug.Log($"[FloatBubbleActivity] RewardCalculator已找到，开始计算奖励 - sourceKey: {settings.RewardSourceKey}");

            int baseReward = rewardCalculator.CalculateReward(settings.RewardSourceKey);

            Debug.Log($"[FloatBubbleActivity] RewardCalculator返回奖励: {baseReward}");

            if (baseReward <= 0)
            {
                Debug.LogError($"[FloatBubbleActivity] UpdateIconRewardDisplay: RewardCalculator返回无效奖励: {baseReward}");
                return;
            }

            // 更新Icon显示（会缓存数值）
            Debug.Log($"[FloatBubbleActivity] [调用] floatBubbleIcon.UpdateRewardDisplay({baseReward}, {settings.AdMultiplier})");
            floatBubbleIcon.UpdateRewardDisplay(baseReward, settings.AdMultiplier);
            Debug.Log($"[FloatBubbleActivity] [完成] UpdateIconRewardDisplay - 基础奖励:{baseReward}, 倍率:{settings.AdMultiplier}x");
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// 检查解锁条件
        /// </summary>
        private void CheckUnlockCondition()
        {
            if (activityData == null || settings == null)
            {
                return;
            }

            // 已解锁，跳过
            if (activityData.isUnlocked)
            {
                return;
            }

            // 检查关卡条件
            int currentLevel = GameDataManager.GetLevelNum();
            if (currentLevel >= settings.UnlockLevel)
            {
                // 解锁
                activityData.Unlock();
                activityData.Save();

                Log($"活动已解锁！当前关卡: {currentLevel}, 要求关卡: {settings.UnlockLevel}");

                // 首次解锁立即显示（不进入冷却）
                CheckAndNotifyVisibilityChange();
            }
        }

        /// <summary>
        /// 隐藏泡泡
        /// </summary>
        private void HideBubble()
        {
            // 通过事件隐藏
            ActivityEvents.TriggerActivityShouldHide(config.ActivityId);

            // 清空Icon引用，避免引用已销毁对象
            bubbleIcon = null;

            Log("泡泡已隐藏，Icon引用已清空");
        }

        #endregion

        #region Debug

#if UNITY_EDITOR
        /// <summary>
        /// 获取调试信息
        /// </summary>
        public string GetDebugInfo()
        {
            if (!isInitialized || activityData == null)
            {
                return "未初始化";
            }

            string iconStatus = "无Icon";
            if (bubbleIcon != null)
            {
                var floatBubbleIcon = bubbleIcon as FloatBubbleIcon;
                if (floatBubbleIcon != null)
                {
                    iconStatus = $"飘动中: {floatBubbleIcon.IsFloating()}";
                }
                else
                {
                    iconStatus = "Icon存在";
                }
            }

            return $"{activityData.GetDebugInfo()}\n{iconStatus}";
        }
#endif

        #endregion
    }

}
