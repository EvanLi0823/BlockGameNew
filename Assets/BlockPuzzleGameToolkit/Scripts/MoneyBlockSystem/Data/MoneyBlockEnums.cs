// 金钱方块系统 - 枚举定义
// 创建日期: 2026-03-05

namespace BlockPuzzleGameToolkit.Scripts.MoneyBlockSystem
{
    /// <summary>
    /// 奖励类型枚举
    /// </summary>
    public enum EMoneyBlockRewardType
    {
        /// <summary>
        /// 小额奖励(即时消除)
        /// </summary>
        Small,

        /// <summary>
        /// 大额奖励(累计触发)
        /// </summary>
        Large
    }

    // 注意：EClaimType已移至BlockPuzzleGameToolkit.Scripts.Popups命名空间
    // 使其成为通用类型，可供多个模块使用

    /// <summary>
    /// 金钱方块状态枚举
    /// </summary>
    public enum EMoneyBlockState
    {
        /// <summary>
        /// 空闲状态
        /// </summary>
        Idle,

        /// <summary>
        /// 刷新中
        /// </summary>
        Spawning,

        /// <summary>
        /// 激活可消除
        /// </summary>
        Active,

        /// <summary>
        /// 消除中
        /// </summary>
        Eliminating,

        /// <summary>
        /// 已消耗(批量消除时被消耗掉的部分)
        /// </summary>
        Consumed
    }
}
