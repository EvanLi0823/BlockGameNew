// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Use of this software for commercial purposes is strictly not allowed.
// Use of this software for illegal purposes is strictly not allowed.
// All sales are final.

namespace GameCore.DifficultySystem
{
    /// <summary>
    /// 难度状态（用于状态机管理）
    /// </summary>
    public enum DifficultyState
    {
        /// <summary>默认基线难度</summary>
        Normal = 0,

        /// <summary>提升难度模式（连续通关N次后）</summary>
        Increased = 1,

        /// <summary>降低难度模式（连续失败M次后）</summary>
        Decreased = 2
    }
}
