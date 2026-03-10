// 金钱方块系统 - 方块组件
// 创建日期: 2026-03-05

using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.MoneyBlockSystem
{
    /// <summary>
    /// 金钱方块组件
    /// 挂载在Cell上作为标识
    /// 职责:
    /// - 标识该格子是金钱方块
    /// - 消除时通知MoneyBlockManager
    /// - 状态追踪
    /// 注意: 金钱图标由Cell的Bonus系统管理，不需要额外引用
    /// </summary>
    public class MoneyBlock : MonoBehaviour
    {
        /// <summary>
        /// 当前状态
        /// </summary>
        public EMoneyBlockState State { get; private set; } = EMoneyBlockState.Idle;

        /// <summary>
        /// 初始化（图标由Cell的Bonus系统管理）
        /// </summary>
        public void Initialize()
        {
            State = EMoneyBlockState.Active;
        }

        /// <summary>
        /// 消除时调用（已弃用 - 新架构由TargetManager统一管理）
        /// 保留此方法以防向后兼容，但实际消除逻辑由IBonusCollector.OnBonusCollected()处理
        /// </summary>
        [System.Obsolete("已弃用，消除逻辑由TargetManager统一管理")]
        public void OnEliminated()
        {
            State = EMoneyBlockState.Eliminating;

            // 注意：不再主动通知MoneyBlockManager
            // 新架构：TargetManager → 检查HasBonus() → 飞行动画 → OnBonusCollected()
        }

        /// <summary>
        /// 标记为已消耗（已弃用 - 新架构不再使用批量消除）
        /// 保留此方法以防向后兼容
        /// </summary>
        [System.Obsolete("已弃用，新架构不再使用批量消除")]
        public void MarkAsConsumed()
        {
            State = EMoneyBlockState.Consumed;
            // 注意：图标清除由Cell的Bonus系统自动管理
        }
    }
}
