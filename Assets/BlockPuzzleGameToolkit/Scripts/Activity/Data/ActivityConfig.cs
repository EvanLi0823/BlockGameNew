// 活动系统 - 配置数据
// 创建日期: 2026-03-09

using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Activity.Data
{
    /// <summary>
    /// 单个活动的配置数据结构
    /// </summary>
    [System.Serializable]
    public class ActivityConfig
    {
        [Header("基础信息")]
        [Tooltip("活动唯一标识符（小写+下划线，如float_bubble）")]
        [SerializeField] private string activityId;

        [Tooltip("活动显示名称（用于资源路径，如FloatBubble）")]
        [SerializeField] private string activityName;

        [Tooltip("活动类型")]
        [SerializeField] private EActivityType activityType = EActivityType.Limited;

        [Header("启用控制")]
        [Tooltip("活动是否启用")]
        [SerializeField] private bool isEnabled = true;

        [Header("UI配置")]
        [Tooltip("显示排序优先级（数值越小越靠前）")]
        [SerializeField] private int sortOrder = 0;

        [Header("资源路径")]
        [Tooltip("角标Prefab路径（相对于Resources/Activity/{ActivityName}/）")]
        [SerializeField] private string iconPrefabPath = "Icon";

        [Header("模块配置")]
        [Tooltip("活动模块命名空间（默认：BlockPuzzleGameToolkit.Scripts.Activity.Examples）")]
        [SerializeField] private string moduleNamespace = "BlockPuzzleGameToolkit.Scripts.Activity.Examples";

        #region Properties

        public string ActivityId => activityId;
        public string ActivityName => activityName;
        public EActivityType ActivityType => activityType;
        public bool IsEnabled
        {
            get => isEnabled;
            set => isEnabled = value;
        }
        public int SortOrder => sortOrder;
        public string IconPrefabPath => iconPrefabPath;
        public string ModuleNamespace => moduleNamespace;

        #endregion

        #region Convention-based Path Generation

        /// <summary>
        /// 根据约定获取模块类名
        /// 约定：{ActivityName}Activity
        /// 例如：FloatBubble -> FloatBubbleActivity
        /// </summary>
        public string GetModuleClassName()
        {
            return $"{activityName}Activity";
        }

        /// <summary>
        /// 根据约定获取模块完整类型名（包含命名空间）
        /// 例如：BlockPuzzleGameToolkit.Scripts.Activity.Examples.FloatBubbleActivity
        /// </summary>
        public string GetModuleFullTypeName()
        {
            string className = GetModuleClassName();
            return string.IsNullOrEmpty(moduleNamespace)
                ? className
                : $"{moduleNamespace}.{className}";
        }

        /// <summary>
        /// 根据约定获取弹窗Prefab路径
        /// 约定：Activity/{ActivityName}/{ActivityName}Popup
        /// 例如：FloatBubble -> Activity/FloatBubble/FloatBubblePopup
        /// </summary>
        public string GetPopupPath()
        {
            return $"Activity/{activityName}/{activityName}Popup";
        }

        #endregion

        #region Validation

        /// <summary>
        /// 验证配置有效性
        /// </summary>
        public bool Validate()
        {
            if (string.IsNullOrEmpty(activityId))
            {
                Debug.LogError("[ActivityConfig] activityId不能为空");
                return false;
            }

            if (string.IsNullOrEmpty(activityName))
            {
                Debug.LogError($"[ActivityConfig] activityName不能为空: {activityId}");
                return false;
            }

            if (string.IsNullOrEmpty(iconPrefabPath))
            {
                Debug.LogWarning($"[ActivityConfig] iconPrefabPath为空: {activityId}");
            }

            return true;
        }

        #endregion

        #region Debug

        public override string ToString()
        {
            return $"ActivityConfig[{activityId}] Name={activityName}, Type={activityType}, " +
                   $"Enabled={isEnabled}";
        }

        #endregion
    }
}
