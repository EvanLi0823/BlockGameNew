// 金钱方块系统 - 数据结构
// 创建日期: 2026-03-05

using System;

namespace BlockPuzzleGameToolkit.Scripts.MoneyBlockSystem
{
    /// <summary>
    /// 批量消除结果数据
    /// </summary>
    [Serializable]
    public struct BatchEliminateResult
    {
        /// <summary>
        /// 处理的金钱方块数量
        /// </summary>
        public int processedCount;

        /// <summary>
        /// 发放即时奖励的数量
        /// 场景A(未达阈值): processedCount
        /// 场景B(达到阈值): 0
        /// </summary>
        public int rewardedCount;

        /// <summary>
        /// 被消耗掉的数量(达到阈值后多余的)
        /// 场景A(未达阈值): 0
        /// 场景B(达到阈值): processedCount
        /// </summary>
        public int consumedCount;

        /// <summary>
        /// 是否触发了累计奖励
        /// </summary>
        public bool triggeredCumulative;

        /// <summary>
        /// 即时奖励金额(货币放大10000倍后的整数)
        /// 场景A: 有奖励值
        /// 场景B: 0
        /// </summary>
        public int immediateReward;

        /// <summary>
        /// 转字符串(用于调试)
        /// </summary>
        public override string ToString()
        {
            return $"BatchEliminate: Processed={processedCount}, " +
                   $"Rewarded={rewardedCount}, Consumed={consumedCount}, " +
                   $"Triggered={triggeredCumulative}, Reward={immediateReward}";
        }
    }
}
