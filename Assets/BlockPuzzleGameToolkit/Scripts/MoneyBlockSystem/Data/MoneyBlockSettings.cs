// 金钱方块系统 - 配置类
// 创建日期: 2026-03-05

using UnityEngine;
using BlockPuzzleGameToolkit.Scripts.Services.Ads.AdUnits;
using BlockPuzzleGameToolkit.Scripts.LevelsData;

namespace BlockPuzzleGameToolkit.Scripts.MoneyBlockSystem
{
    /// <summary>
    /// 金钱方块系统配置
    /// 路径: /Assets/BlockPuzzleGameToolkit/Resources/Settings/MoneyBlockSettings.asset
    /// </summary>
    [CreateAssetMenu(fileName = "MoneyBlockSettings", menuName = "BlockPuzzleGameToolkit/MoneyBlock/Settings")]
    public class MoneyBlockSettings : ScriptableObject
    {
        [Header("刷新条件")]
        [Tooltip("每放置N个形状触发刷新")]
        public int shapePlacementTrigger = 5;

        [Tooltip("关卡内最大出现次数")]
        public int maxMoneyBlocksPerLevel = 10;

        [Header("奖励倍率")]
        [Tooltip("小额奖励倍率(即时消除)")]
        [Range(0.1f, 10f)]
        public float smallRewardMultiplier = 0.5f;

        [Tooltip("大额奖励倍率(累计触发)")]
        [Range(1f, 50f)]
        public float largeRewardMultiplier = 5.0f;

        [Header("累计系统")]
        [Tooltip("累计触发阈值(消除N个触发大额奖励)")]
        [Range(1, 50)]
        public int cumulativeThreshold = 10;

        [Header("广告配置")]
        [Tooltip("广告多倍比例(%)")]
        [Range(100, 1000)]
        public int adMultiplierPercentage = 300;

        [Tooltip("不看广告领奖倍率(%)")]
        [Range(0, 200)]
        public int noAdRewardMultiplierPercentage = 100;

        [Tooltip("广告类型")]
        public EAdType adType = EAdType.Rewarded;

        [Header("飞币动画")]
        [Tooltip("小额飞币数量")]
        [Range(1, 10)]
        public int flyingCoinCountSmall = 2;

        [Tooltip("大额飞币数量")]
        [Range(5, 20)]
        public int flyingCoinCountLarge = 8;

        [Tooltip("飞币动画延迟(秒)")]
        [Range(0f, 2f)]
        public float flyAnimationDelay = 0.2f;

        [Header("资源引用")]
        [Tooltip("金钱方块图标模板（复用Bonus系统）")]
        public BlockPuzzleGameToolkit.Scripts.LevelsData.BonusItemTemplate moneyBonusTemplate;

        // 注意：
        // - 飞行动画由BonusAnimationManager统一管理，无需在此配置
        // - 累计奖励弹窗由MenuManager自动加载（Resources/Popups/MoneyBlockRewardPopup.prefab），无需手动配置

        [Header("调试选项")]
        [Tooltip("启用调试日志")]
        public bool enableDebugLog = false;

        /// <summary>
        /// 验证配置有效性
        /// </summary>
        public bool ValidateSettings()
        {
            if (shapePlacementTrigger <= 0)
            {
                Debug.LogError("[MoneyBlockSettings] shapePlacementTrigger必须大于0");
                return false;
            }

            if (cumulativeThreshold <= 0)
            {
                Debug.LogError("[MoneyBlockSettings] cumulativeThreshold必须大于0");
                return false;
            }

            if (moneyBonusTemplate == null)
            {
                Debug.LogWarning("[MoneyBlockSettings] moneyBonusTemplate未设置，金钱图标将无法显示");
            }

            return true;
        }

        /// <summary>
        /// 获取广告多倍倍率(转换为浮点数)
        /// </summary>
        public float GetAdMultiplier()
        {
            return adMultiplierPercentage / 100f;
        }

        /// <summary>
        /// 获取不看广告倍率(转换为浮点数)
        /// </summary>
        public float GetNoAdMultiplier()
        {
            return noAdRewardMultiplierPercentage / 100f;
        }
    }
}
