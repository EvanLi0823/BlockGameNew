// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.

namespace BlockPuzzleGameToolkit.Scripts.FlyRewardSystem.Data
{
    /// <summary>
    /// 飞行动画模式
    /// </summary>
    public enum FlyAnimationPattern
    {
        /// <summary>
        /// 直接飞行到目标
        /// </summary>
        DirectFly = 0,

        /// <summary>
        /// 烟花爆炸效果（先爆炸散开，再聚合）
        /// </summary>
        FireworkBurst = 1,

        /// <summary>
        /// 抛物线飞行
        /// </summary>
        Parabolic = 2,

        /// <summary>
        /// 螺旋飞行
        /// </summary>
        Spiral = 3,

        /// <summary>
        /// 随机散开再聚合
        /// </summary>
        RandomScatter = 4
    }
}