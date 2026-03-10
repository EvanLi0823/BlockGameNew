// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

using System;
using UnityEngine;
using StorageSystem.Data;

namespace BlockPuzzleGameToolkit.Scripts.RewardSystem
{
    /// <summary>
    /// 奖励系统状态存储数据
    /// 仅存储当前区间ID，其他配置从ScriptableObject中读取
    /// </summary>
    [Serializable]
    public class RewardStateSaveData : SaveDataContainer
    {
        [SerializeField] private string currentRangeId = "no_withdraw"; // 当前区间ID

        /// <summary>
        /// 获取/设置当前区间ID
        /// </summary>
        public string CurrentRangeId
        {
            get => currentRangeId;
            set => currentRangeId = value;
        }

        /// <summary>
        /// 创建默认数据
        /// </summary>
        public static RewardStateSaveData CreateDefault()
        {
            var data = new RewardStateSaveData
            {
                currentRangeId = "no_withdraw" // 新用户默认为未提现区间
            };
            data.UpdateMetadata(1);
            return data;
        }

        /// <summary>
        /// 验证数据有效性
        /// </summary>
        public override bool IsValid()
        {
            // 区间ID不能为空且需要调用基类的验证
            return base.IsValid() && !string.IsNullOrEmpty(currentRangeId);
        }

        /// <summary>
        /// 重置数据
        /// </summary>
        public void Reset()
        {
            currentRangeId = "no_withdraw";
            UpdateMetadata(Version);
        }

        public override string ToString()
        {
            return $"RewardState: RangeId={currentRangeId}";
        }
    }
}