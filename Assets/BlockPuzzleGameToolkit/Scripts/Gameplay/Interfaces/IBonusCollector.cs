// Bonus收集者接口
// 创建日期: 2026-03-06
// 用途: 定义所有bonus收集系统的通用接口，支持扩展新的bonus类型

using UnityEngine;
using BlockPuzzleGameToolkit.Scripts.LevelsData;

namespace BlockPuzzleGameToolkit.Scripts.Gameplay
{
    /// <summary>
    /// Bonus收集者接口
    /// 任何需要在消除时收集bonus并播放飞行动画的系统都应实现此接口
    /// 例如：金钱方块、能量方块、经验方块等
    /// </summary>
    public interface IBonusCollector
    {
        /// <summary>
        /// 获取飞行动画的终点位置（UI坐标）
        /// </summary>
        /// <returns>目标UI的世界坐标</returns>
        Vector2 GetFlyTargetPosition();

        /// <summary>
        /// 获取bonus图标模板（用于飞行动画显示）
        /// </summary>
        /// <returns>BonusItemTemplate资源</returns>
        BonusItemTemplate GetBonusTemplate();

        /// <summary>
        /// 当bonus被收集时的回调（飞行动画完成后调用）
        /// </summary>
        /// <param name="cell">包含bonus的格子</param>
        void OnBonusCollected(Cell cell);

        /// <summary>
        /// 检查指定格子是否包含此类型的bonus
        /// </summary>
        /// <param name="cell">待检查的格子</param>
        /// <returns>是否包含该bonus</returns>
        bool HasBonus(Cell cell);

        /// <summary>
        /// 检查此收集者是否启用
        /// 例如：金钱方块仅在冒险模式生效
        /// </summary>
        /// <returns>是否启用</returns>
        bool IsEnabled();
    }
}
