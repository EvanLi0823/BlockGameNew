// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

using System;
using System.Collections.Generic;
using UnityEngine;
using StorageSystem.Data;

namespace BlockPuzzleGameToolkit.Scripts.LevelRewardMultiplier.Data
{
    /// <summary>
    /// 关卡奖励倍率系统的保存数据
    /// </summary>
    [Serializable]
    public class LevelRewardMultiplierSaveData : SaveDataContainer
    {
        /// <summary>
        /// 运行时数据条目
        /// </summary>
        [Serializable]
        public class RuntimeDataEntry
        {
            [SerializeField] public string configId;
            [SerializeField] public int currentIndex;
            [SerializeField] public long lastResetTime;  // Ticks
            [SerializeField] public int totalUseCount;
        }

        [SerializeField]
        public Dictionary<string, RuntimeDataEntry> runtimeDataMap;

        public LevelRewardMultiplierSaveData()
        {
            runtimeDataMap = new Dictionary<string, RuntimeDataEntry>();
        }
    }
}