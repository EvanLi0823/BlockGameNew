// 通用奖励弹窗 - 配置类
// 创建日期: 2026-03-06

using System;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Popups
{
    /// <summary>
    /// 通用奖励弹窗配置
    /// 用于配置RewardPopup的所有参数
    /// 支持单倍/多倍领取、飞币动画、广告集成
    /// </summary>
    public class RewardPopupConfig
    {
        #region 基础奖励配置

        /// <summary>
        /// 基础奖励金额（放大10000倍）
        /// </summary>
        public int BaseReward { get; set; }

        /// <summary>
        /// 广告倍率（默认3倍）
        /// </summary>
        public float AdMultiplier { get; set; } = 3f;

        /// <summary>
        /// 不看广告倍率（默认1倍）
        /// </summary>
        public float NoAdMultiplier { get; set; } = 1f;

        #endregion

        #region 飞币动画配置

        /// <summary>
        /// 是否自动播放飞币动画
        /// </summary>
        public bool AutoPlayFlyAnimation { get; set; } = true;

        /// <summary>
        /// 飞行道具数量（仅当AutoPlayFlyAnimation=true时生效）
        /// </summary>
        public int FlyingCoinCount { get; set; } = 8;

        /// <summary>
        /// 飞币起点位置（世界坐标，null = 使用弹窗的transform.position）
        /// </summary>
        public Vector3? FlyStartPosition { get; set; } = null;

        #endregion

        #region 广告配置

        /// <summary>
        /// 广告入口名称
        /// </summary>
        public string AdEntryName { get; set; } = "Common_Reward";

        #endregion

        #region 回调

        /// <summary>
        /// 奖励领取回调（在飞币动画开始前调用）
        /// </summary>
        public Action<RewardClaimResult> OnRewardClaimed { get; set; }

        /// <summary>
        /// 弹窗关闭回调（在弹窗完全关闭后调用，包括所有动画完成）
        /// </summary>
        public Action OnPopupClosed { get; set; }

        #endregion

        #region 验证

        /// <summary>
        /// 验证配置有效性
        /// </summary>
        public bool Validate()
        {
            if (BaseReward <= 0)
            {
                Debug.LogError("[RewardPopupConfig] BaseReward必须大于0");
                return false;
            }

            if (AdMultiplier <= 1f)
            {
                Debug.LogWarning("[RewardPopupConfig] AdMultiplier应该大于1，否则广告无意义");
            }

            if (string.IsNullOrEmpty(AdEntryName))
            {
                Debug.LogWarning("[RewardPopupConfig] AdEntryName为空，广告功能可能无法使用");
            }

            return true;
        }

        #endregion

        #region 工厂方法

        /// <summary>
        /// 创建默认配置（用于快速测试）
        /// </summary>
        public static RewardPopupConfig CreateDefault(int baseReward, Action<RewardClaimResult> callback)
        {
            return new RewardPopupConfig
            {
                BaseReward = baseReward,
                AdMultiplier = 3f,
                AutoPlayFlyAnimation = true,
                FlyingCoinCount = 8,
                FlyStartPosition = null,  // null = 使用弹窗的transform.position
                AdEntryName = "Common_Reward",
                OnRewardClaimed = callback
            };
        }

        #endregion
    }
}
