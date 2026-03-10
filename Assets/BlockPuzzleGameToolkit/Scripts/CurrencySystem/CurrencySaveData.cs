// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

using System;
using UnityEngine;
using StorageSystem.Data;

namespace BlockPuzzleGameToolkit.Scripts.CurrencySystem
{
    /// <summary>
    /// 金币存储数据（整数版）
    /// 继承自存储模块的SaveDataContainer，自动获得版本管理和校验功能
    /// </summary>
    [Serializable]
    public class CurrencySaveData : SaveDataContainer
    {
        [SerializeField] private int coinsInt;     // 金币（整数形式，已放大10000倍）

        // 精度配置：使用10000倍精度
        private const int PRECISION_SCALE = 10000;

        /// <summary>
        /// 获取/设置金币整数值（内部存储格式，已放大10000倍）
        /// </summary>
        public int CoinsInt
        {
            get => coinsInt;
            set => coinsInt = Mathf.Max(0, value);
        }

        /// <summary>
        /// 创建默认数据
        /// </summary>
        public static CurrencySaveData CreateDefault()
        {
            var data = new CurrencySaveData { coinsInt = 0 };
            data.UpdateMetadata(1);
            return data;
        }

        /// <summary>
        /// 验证数据有效性
        /// </summary>
        public override bool IsValid()
        {
            // 金币不能为负数且需要调用基类的验证
            return base.IsValid() && coinsInt >= 0;
        }

        /// <summary>
        /// 重置数据
        /// </summary>
        public void Reset()
        {
            coinsInt = 0;
            UpdateMetadata(Version);
        }

        public override string ToString()
        {
            float displayValue = coinsInt / (float)PRECISION_SCALE;
            return $"Currency: {coinsInt} (Display: ${displayValue:F3})";
        }
    }
}