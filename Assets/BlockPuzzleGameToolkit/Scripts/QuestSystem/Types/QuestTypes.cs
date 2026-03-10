// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

using UnityEngine;
using QuestSystem.Core;

namespace QuestSystem.Types
{
    /// <summary>
    /// 收集类任务
    /// 收集指定数量的物品
    /// </summary>
    public class CollectQuest : Quest
    {
        // 收集任务使用基类的默认实现（累加进度）
    }

    /// <summary>
    /// 分数类任务
    /// 达到指定分数
    /// </summary>
    public class ScoreQuest : Quest
    {
        /// <summary>
        /// 分数任务特殊处理：取最高分而非累加
        /// </summary>
        public override bool UpdateProgress(int value)
        {
            if (IsCompleted || Data == null) return false;

            int oldProgress = CurrentProgress;

            // 分数任务取最高值
            SetProgress(Mathf.Max(CurrentProgress, value));

            // 检查是否完成
            if (CurrentProgress >= Data.TargetValue)
            {
                SetCompleted(true);
            }

            // 返回是否有进度变化
            return oldProgress != CurrentProgress;
        }
    }

    /// <summary>
    /// 消行类任务
    /// 消除指定数量的行
    /// </summary>
    public class LineQuest : Quest
    {
        // 消行任务使用基类的默认实现（累加进度）
    }

    /// <summary>
    /// 游戏次数任务
    /// 完成指定次数的游戏
    /// </summary>
    public class PlayCountQuest : Quest
    {
        // 游戏次数任务使用基类的默认实现（累加进度）
    }

    /// <summary>
    /// 连击类任务
    /// 达到指定连击数
    /// </summary>
    public class ComboQuest : Quest
    {
        /// <summary>
        /// 连击任务特殊处理：取最高连击数
        /// </summary>
        public override bool UpdateProgress(int value)
        {
            if (IsCompleted || Data == null) return false;

            int oldProgress = CurrentProgress;

            // 连击任务取最高值
            SetProgress(Mathf.Max(CurrentProgress, value));

            // 检查是否完成
            if (CurrentProgress >= Data.TargetValue)
            {
                SetCompleted(true);
            }

            // 返回是否有进度变化
            return oldProgress != CurrentProgress;
        }
    }

    /// <summary>
    /// 完美通关任务
    /// 完美通关指定次数
    /// </summary>
    public class PerfectQuest : Quest
    {
        // 完美通关任务使用基类的默认实现（累加进度）
    }
}