// 金钱方块系统 - 存档数据
// 创建日期: 2026-03-05

using System;
using StorageSystem.Data;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.MoneyBlockSystem
{
    /// <summary>
    /// 金钱方块运行时数据
    /// 用于数据持久化
    /// </summary>
    [Serializable]
    public class MoneyBlockSaveData : SaveDataContainer
    {
        // 关卡内临时数据(关卡结束重置)
        public int shapePlacementCount;        // 当前放置形状计数
        public int spawnCountInLevel;          // 关卡内刷新计数
        public int eliminateCountInLevel;      // 关卡内消除计数
        public int cumulativeEliminateCount;   // 累计消除计数(用于触发大额奖励)

        // 全局统计数据(持久化)
        public int totalSpawnCount;            // 总刷新次数
        public int totalEliminateCount;        // 总消除次数
        public int cumulativeTriggerCount;     // 累计奖励触发次数(全局)
        public int adClaimCount;               // 广告领取次数(全局)
        public int singleClaimCount;           // 单倍领取次数(全局)
        public long totalRewardAmount;         // 累计获得金钱数(整数,放大10000倍)

        /// <summary>
        /// 创建默认数据
        /// </summary>
        public static MoneyBlockSaveData CreateDefault()
        {
            return new MoneyBlockSaveData
            {
                shapePlacementCount = 0,
                spawnCountInLevel = 0,
                eliminateCountInLevel = 0,
                cumulativeEliminateCount = 0,
                totalSpawnCount = 0,
                totalEliminateCount = 0,
                cumulativeTriggerCount = 0,
                adClaimCount = 0,
                singleClaimCount = 0,
                totalRewardAmount = 0
            };
        }

        /// <summary>
        /// 重置所有数据
        /// </summary>
        public void Reset()
        {
            shapePlacementCount = 0;
            spawnCountInLevel = 0;
            eliminateCountInLevel = 0;
            cumulativeEliminateCount = 0;
            totalSpawnCount = 0;
            totalEliminateCount = 0;
            cumulativeTriggerCount = 0;
            adClaimCount = 0;
            singleClaimCount = 0;
            totalRewardAmount = 0;

            UpdateMetadata(1);
        }

        /// <summary>
        /// 重置关卡内数据
        /// </summary>
        public void ResetLevelData()
        {
            shapePlacementCount = 0;
            spawnCountInLevel = 0;
            eliminateCountInLevel = 0;
            cumulativeEliminateCount = 0;
        }

        /// <summary>
        /// 验证数据有效性
        /// </summary>
        public override bool IsValid()
        {
            return shapePlacementCount >= 0
                && spawnCountInLevel >= 0
                && eliminateCountInLevel >= 0
                && cumulativeEliminateCount >= 0
                && totalSpawnCount >= 0
                && totalEliminateCount >= 0;
        }

        /// <summary>
        /// 转字符串(用于调试)
        /// </summary>
        public override string ToString()
        {
            return $"MoneyBlockSaveData: " +
                   $"Placed={shapePlacementCount}, " +
                   $"Spawned={spawnCountInLevel}/{totalSpawnCount}, " +
                   $"Eliminated={eliminateCountInLevel}/{totalEliminateCount}, " +
                   $"Cumulative={cumulativeEliminateCount}, " +
                   $"CumulativeTriggers={cumulativeTriggerCount}, " +
                   $"AdClaims={adClaimCount}";
        }
    }
}
