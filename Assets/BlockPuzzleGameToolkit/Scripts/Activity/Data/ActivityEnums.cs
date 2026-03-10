// 活动系统 - 枚举定义
// 创建日期: 2026-03-09

namespace BlockPuzzleGameToolkit.Scripts.Activity.Data
{
    /// <summary>
    /// 活动类型
    /// </summary>
    public enum EActivityType
    {
        Daily,      // 每日活动
        Limited,    // 限时活动
        Permanent   // 永久活动
    }

    /// <summary>
    /// 活动刷新事件类型
    /// </summary>
    public enum EActivityRefreshEvent
    {
        LevelCompleted,      // 关卡完成
        CurrencyChanged,     // 货币变化
        ItemCollected,       // 道具收集
        TimeReached,         // 时间到达（如每日0点）
        PopupClosed,         // 活动弹窗关闭
        ServerConfigUpdated, // 服务器配置更新
        UserAction,          // 用户主动刷新
        SceneChanged         // 场景切换
    }
}
