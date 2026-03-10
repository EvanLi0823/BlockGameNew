// 漂浮泡泡活动 - 配置文件
// 创建日期: 2026-03-09

using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Activity.Examples
{
    /// <summary>
    /// 漂浮泡泡活动配置
    /// 存储所有可配置参数
    /// </summary>
    [CreateAssetMenu(fileName = "FloatingBubbleSettings", menuName = "Activity/FloatingBubbleSettings")]
    public class FloatingBubbleSettings : ScriptableObject
    {
        #region Instance (单例访问)

        private static FloatingBubbleSettings instance;

        /// <summary>
        /// 单例实例（从Resources加载）
        /// </summary>
        public static FloatingBubbleSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<FloatingBubbleSettings>("Settings/Activity/FloatingBubbleSettings");
                    if (instance == null)
                    {
                        Debug.LogError("[FloatingBubbleSettings] 配置文件未找到: Resources/Settings/Activity/FloatingBubbleSettings");
                    }
                }
                return instance;
            }
        }

        #endregion

        #region 解锁条件

        [Header("解锁条件")]
        [Tooltip("解锁所需关卡数")]
        [SerializeField] private int unlockLevel = 5;

        /// <summary>
        /// 解锁所需关卡数
        /// </summary>
        public int UnlockLevel => unlockLevel;

        #endregion

        #region 冷却配置

        [Header("冷却配置")]
        [Tooltip("冷却时间（秒）")]
        [SerializeField] private float cooldownDuration = 1800f; // 30分钟

        /// <summary>
        /// 冷却时间（秒）
        /// </summary>
        public float CooldownDuration => cooldownDuration;

        #endregion

        #region 运动参数

        [Header("运动参数")]
        [Tooltip("垂直上升速度（Canvas坐标/秒）")]
        [SerializeField] private float verticalSpeed = 100f;

        [Tooltip("水平移动速度（Canvas坐标/秒）")]
        [SerializeField] private float horizontalSpeed = 50f;

        [Tooltip("水平边界（归一化比例，0-1）")]
        [SerializeField] private Vector2 horizontalBounds = new Vector2(0.1f, 0.9f); // 左右各10%

        [Tooltip("顶部消失Y坐标比例（归一化，相对Canvas高度）")]
        [SerializeField] private float disappearYRatio = 0.9f;

        /// <summary>
        /// 垂直上升速度（Canvas坐标/秒）
        /// </summary>
        public float VerticalSpeed => verticalSpeed;

        /// <summary>
        /// 水平移动速度（Canvas坐标/秒）
        /// </summary>
        public float HorizontalSpeed => horizontalSpeed;

        /// <summary>
        /// 水平边界（归一化比例，0-1）
        /// x = 左边界比例，y = 右边界比例
        /// </summary>
        public Vector2 HorizontalBounds => horizontalBounds;

        /// <summary>
        /// 顶部消失Y坐标比例（归一化，相对Canvas高度）
        /// </summary>
        public float DisappearYRatio => disappearYRatio;

        #endregion

        #region 奖励参数

        [Header("奖励参数")]
        [Tooltip("RewardCalculator的sourceKey（用于计算基础奖励）")]
        [SerializeField] private string rewardSourceKey = "FloatingReward";

        [Tooltip("广告倍率")]
        [SerializeField] private float adMultiplier = 3f;

        /// <summary>
        /// RewardCalculator的sourceKey
        /// </summary>
        public string RewardSourceKey => rewardSourceKey;

        /// <summary>
        /// 广告倍率
        /// </summary>
        public float AdMultiplier => adMultiplier;

        #endregion

        #region 广告配置

        [Header("广告配置")]
        [Tooltip("广告入口名称")]
        [SerializeField] private string adEntryName = "FloatingBubble";

        /// <summary>
        /// 广告入口名称
        /// </summary>
        public string AdEntryName => adEntryName;

        #endregion

        #region 验证

        /// <summary>
        /// 验证配置有效性
        /// </summary>
        public bool Validate()
        {
            bool isValid = true;

            if (unlockLevel < 1)
            {
                Debug.LogError("[FloatingBubbleSettings] unlockLevel必须大于0");
                isValid = false;
            }

            if (cooldownDuration < 0f)
            {
                Debug.LogError("[FloatingBubbleSettings] cooldownDuration不能为负数");
                isValid = false;
            }

            if (verticalSpeed <= 0f)
            {
                Debug.LogError("[FloatingBubbleSettings] verticalSpeed必须大于0");
                isValid = false;
            }

            if (horizontalSpeed < 0f)
            {
                Debug.LogError("[FloatingBubbleSettings] horizontalSpeed不能为负数");
                isValid = false;
            }

            if (horizontalBounds.x < 0f || horizontalBounds.x > 1f ||
                horizontalBounds.y < 0f || horizontalBounds.y > 1f ||
                horizontalBounds.x >= horizontalBounds.y)
            {
                Debug.LogError("[FloatingBubbleSettings] horizontalBounds范围错误（应在0-1之间，且x<y）");
                isValid = false;
            }

            if (disappearYRatio < 0f || disappearYRatio > 1f)
            {
                Debug.LogError("[FloatingBubbleSettings] disappearYRatio范围错误（应在0-1之间）");
                isValid = false;
            }

            if (string.IsNullOrEmpty(rewardSourceKey))
            {
                Debug.LogError("[FloatingBubbleSettings] rewardSourceKey不能为空");
                isValid = false;
            }

            if (adMultiplier <= 1f)
            {
                Debug.LogWarning("[FloatingBubbleSettings] adMultiplier应该大于1，否则广告无意义");
            }

            if (string.IsNullOrEmpty(adEntryName))
            {
                Debug.LogWarning("[FloatingBubbleSettings] adEntryName为空");
            }

            return isValid;
        }

        #endregion

        #region Editor

#if UNITY_EDITOR
        private void OnValidate()
        {
            // 编辑器中修改参数时自动验证
            Validate();
        }
#endif

        #endregion
    }
}
