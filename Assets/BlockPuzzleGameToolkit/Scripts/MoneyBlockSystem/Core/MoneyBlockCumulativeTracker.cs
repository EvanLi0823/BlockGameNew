// 金钱方块系统 - 累计追踪器
// 创建日期: 2026-03-05

using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.MoneyBlockSystem
{
    /// <summary>
    /// 累计消除追踪器
    /// 职责:
    /// - 累计计数管理
    /// - 阈值检测和触发判定
    /// - 批量消除逻辑处理(含消耗规则)
    /// - 重置管理
    /// </summary>
    public class MoneyBlockCumulativeTracker
    {
        private readonly MoneyBlockSettings settings;
        private bool enableDebugLog;

        public MoneyBlockCumulativeTracker(MoneyBlockSettings settings)
        {
            this.settings = settings;
            this.enableDebugLog = settings != null && settings.enableDebugLog;
        }

        /// <summary>
        /// 处理批量消除
        /// 根据累计计数和阈值决定如何处理
        /// </summary>
        /// <param name="eliminateCount">本次消除的金钱方块数量</param>
        /// <param name="currentCumulative">当前累计计数</param>
        /// <param name="threshold">累计阈值</param>
        /// <returns>批量消除结果</returns>
        public BatchEliminateResult ProcessBatchEliminate(int eliminateCount, int currentCumulative, int threshold)
        {
            // 参数验证
            if (eliminateCount <= 0)
            {
                Debug.LogWarning($"[CumulativeTracker] 无效的消除数量: {eliminateCount}");
                return new BatchEliminateResult { processedCount = 0 };
            }

            if (threshold <= 0)
            {
                Debug.LogError($"[CumulativeTracker] 无效的阈值: {threshold}");
                return new BatchEliminateResult 
                { 
                    processedCount = eliminateCount, 
                    rewardedCount = eliminateCount,
                    consumedCount = 0,
                    triggeredCumulative = false
                };
            }

            BatchEliminateResult result = new BatchEliminateResult
            {
                processedCount = eliminateCount,
                triggeredCumulative = false
            };

            // 检查是否会触发阈值
            if (currentCumulative + eliminateCount >= threshold)
            {
                // 场景B: 达到或超出阈值
                // ⚠️ 新方案：不发放任何即时奖励，全部消耗，立即触发累计奖励
                result.rewardedCount = 0;
                result.consumedCount = eliminateCount;
                result.triggeredCumulative = true;
                result.immediateReward = 0;

                if (enableDebugLog)
                {
                    Debug.Log($"[CumulativeTracker] 达到阈值: 当前={currentCumulative}, " +
                              $"本次={eliminateCount}, 阈值={threshold}, " +
                              $"全部消耗({eliminateCount}个), 触发累计奖励");
                }
            }
            else
            {
                // 场景A: 未达阈值
                // 全部发放即时奖励
                result.rewardedCount = eliminateCount;
                result.consumedCount = 0;
                result.triggeredCumulative = false;
                // immediateReward将由RewardCalculator计算并设置

                if (enableDebugLog)
                {
                    Debug.Log($"[CumulativeTracker] 未达阈值: 当前={currentCumulative}, " +
                              $"本次={eliminateCount}, 阈值={threshold}, " +
                              $"全部发放奖励({eliminateCount}个)");
                }
            }

            return result;
        }

        /// <summary>
        /// 检查是否应该触发累计奖励
        /// </summary>
        public bool ShouldTriggerCumulative(int currentCount, int threshold)
        {
            return currentCount >= threshold;
        }

        /// <summary>
        /// 重置累计计数
        /// </summary>
        public void ResetCumulativeCount()
        {
            if (enableDebugLog)
            {
                Debug.Log("[CumulativeTracker] 重置累计计数");
            }
        }
    }
}
