// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.

namespace BlockPuzzleGameToolkit.Scripts.FlyRewardSystem.Data
{
    /// <summary>
    /// 飞行奖励物体类型
    /// </summary>
    public enum FlyRewardType
    {
        /// <summary>
        /// 金币
        /// </summary>
        Cash = 0,

        /// <summary>
        /// 钻石
        /// </summary>
        Diamond = 1,

        /// <summary>
        /// 礼包
        /// </summary>
        GiftBox = 2,

        /// <summary>
        /// 星星
        /// </summary>
        Star = 3,

        /// <summary>
        /// 白包（提现）
        /// </summary>
        WhitePackage = 4,

        /// <summary>
        /// 自定义
        /// </summary>
        Custom = 99
    }
}