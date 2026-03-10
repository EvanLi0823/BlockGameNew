// 通用奖励弹窗 - 结果类
// 创建日期: 2026-03-06

namespace BlockPuzzleGameToolkit.Scripts.Popups
{
    /// <summary>
    /// 奖励领取结果
    /// 封装奖励领取的所有结果信息
    /// </summary>
    public class RewardClaimResult
    {
        /// <summary>
        /// 领取类型（单倍/多倍）
        /// </summary>
        public EClaimType ClaimType { get; set; }

        /// <summary>
        /// 最终奖励金额（放大10000倍）
        /// </summary>
        public int FinalReward { get; set; }

        /// <summary>
        /// 是否成功
        /// - 单倍领取：总是true
        /// - 多倍领取：true=广告成功，false=广告失败
        /// </summary>
        public bool Success { get; set; }

        #region 工厂方法

        /// <summary>
        /// 创建单倍领取成功结果
        /// </summary>
        public static RewardClaimResult CreateSingleSuccess(int reward)
        {
            return new RewardClaimResult
            {
                ClaimType = EClaimType.Single,
                FinalReward = reward,
                Success = true
            };
        }

        /// <summary>
        /// 创建多倍领取成功结果
        /// </summary>
        public static RewardClaimResult CreateAdSuccess(int reward)
        {
            return new RewardClaimResult
            {
                ClaimType = EClaimType.AdMultiple,
                FinalReward = reward,
                Success = true
            };
        }

        /// <summary>
        /// 创建多倍领取失败结果（广告失败）
        /// </summary>
        public static RewardClaimResult CreateAdFailed()
        {
            return new RewardClaimResult
            {
                ClaimType = EClaimType.AdMultiple,
                FinalReward = 0,
                Success = false
            };
        }

        #endregion
    }

    /// <summary>
    /// 领取类型枚举
    /// </summary>
    public enum EClaimType
    {
        /// <summary>
        /// 单倍领取（不看广告）
        /// </summary>
        Single,

        /// <summary>
        /// 多倍领取（看广告）
        /// </summary>
        AdMultiple
    }
}
