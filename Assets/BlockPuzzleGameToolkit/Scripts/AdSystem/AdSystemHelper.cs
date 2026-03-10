using UnityEngine;
using BlockPuzzle.AdSystem.Managers;
using BlockPuzzle.AdSystem.Models;

namespace BlockPuzzle.AdSystem
{
    /// <summary>
    /// 广告系统快速接入工具类
    /// 提供简化的API供游戏逻辑调用
    /// </summary>
    public static class AdSystemHelper
    {
        /// <summary>
        /// 播放关卡完成广告
        /// </summary>
        public static void PlayLevelCompleteAd(System.Action<bool> onComplete)
        {
            PlayAd(AdEntryNames.LEVEL_COMPLETE, onComplete);
        }

        /// <summary>
        /// 播放奖励弹窗广告
        /// </summary>
        public static void PlayRewardPopupAd(System.Action<bool> onComplete)
        {
            PlayAd(AdEntryNames.REWARD_POPUP, onComplete);
        }

        /// <summary>
        /// 播放失败刷新广告
        /// </summary>
        public static void PlayLevelFailedRefreshAd(System.Action<bool> onComplete)
        {
            PlayAd(AdEntryNames.LEVEL_FAILED_REFRESH, onComplete);
        }

        /// <summary>
        /// 播放每日任务奖励广告
        /// </summary>
        public static void PlayDailyTaskRewardAd(System.Action<bool> onComplete)
        {
            PlayAd(AdEntryNames.DAILY_TASK_REWARD, onComplete);
        }

        /// <summary>
        /// 播放Jackpot奖励广告
        /// </summary>
        public static void PlayJackpotRewardAd(System.Action<bool> onComplete)
        {
            PlayAd(AdEntryNames.JACKPOT_REWARD, onComplete);
        }

        /// <summary>
        /// 播放额外步数广告
        /// </summary>
        public static void PlayExtraMovesAd(System.Action<bool> onComplete)
        {
            PlayAd(AdEntryNames.EXTRA_MOVES, onComplete);
        }

        /// <summary>
        /// 播放双倍金币广告
        /// </summary>
        public static void PlayDoubleCoinsAd(System.Action<bool> onComplete)
        {
            PlayAd(AdEntryNames.DOUBLE_COINS, onComplete);
        }

        /// <summary>
        /// 播放解锁功能广告
        /// </summary>
        public static void PlayUnlockFeatureAd(System.Action<bool> onComplete)
        {
            PlayAd(AdEntryNames.UNLOCK_FEATURE, onComplete);
        }

        /// <summary>
        /// 播放继续游戏广告
        /// </summary>
        public static void PlayContinueGameAd(System.Action<bool> onComplete)
        {
            PlayAd(AdEntryNames.CONTINUE_GAME, onComplete);
        }

        /// <summary>
        /// 播放跳过关卡广告
        /// </summary>
        public static void PlaySkipLevelAd(System.Action<bool> onComplete)
        {
            PlayAd(AdEntryNames.SKIP_LEVEL, onComplete);
        }

        /// <summary>
        /// 播放广告（通用方法）
        /// </summary>
        /// <param name="entryName">广告入口名称</param>
        /// <param name="onComplete">完成回调 (成功)</param>
        public static void PlayAd(string entryName, System.Action<bool> onComplete)
        {
            if (AdSystemManager.Instance == null)
            {
                Debug.LogError("[AdSystemHelper] AdSystemManager not found!");
                onComplete?.Invoke(false);
                return;
            }

            AdSystemManager.Instance.PlayAd(entryName, onComplete);
        }

        /// <summary>
        /// 检查广告是否准备好
        /// </summary>
        public static bool IsAdReady(string entryName)
        {
            if (AdSystemManager.Instance == null)
            {
                return false;
            }

            return AdSystemManager.Instance.IsAdReady(entryName);
        }

        /// <summary>
        /// 播放广告并自动应用奖励
        /// </summary>
        /// <param name="entryName">广告入口名称</param>
        /// <param name="baseReward">基础奖励值</param>
        /// <param name="onReward">奖励回调（奖励值）</param>
        public static void PlayAdWithReward(string entryName, int baseReward, System.Action<int> onReward)
        {
            PlayAd(entryName, (success) =>
            {
                if (success)
                {
                    onReward?.Invoke(baseReward);
                }
                else
                {
                    onReward?.Invoke(0);
                }
            });
        }
    }
}