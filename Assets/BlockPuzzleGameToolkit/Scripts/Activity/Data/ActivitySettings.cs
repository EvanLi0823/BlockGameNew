// 活动系统 - 全局配置
// 创建日期: 2026-03-09

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BlockPuzzleGameToolkit.Scripts.GameCore;

namespace BlockPuzzleGameToolkit.Scripts.Activity.Data
{
    /// <summary>
    /// 活动系统全局配置
    /// 路径: /Assets/BlockPuzzleGameToolkit/Resources/Settings/ActivitySettings.asset
    /// </summary>
    [CreateAssetMenu(fileName = "ActivitySettings", menuName = "BlockPuzzleGameToolkit/Activity/Settings")]
    public class ActivitySettings : SingletonScriptableSettings<ActivitySettings>
    {
        [Header("系统开关")]
        [Tooltip("启用活动系统")]
        [SerializeField] private bool enableActivitySystem = true;

        [Header("Canvas配置")]
        [Tooltip("ActivityCanvas对象名称")]
        [SerializeField] private string activityCanvasName = "ActivityCanvas";

        [Header("活动配置列表")]
        [Tooltip("所有活动配置")]
        [SerializeField] private List<ActivityConfig> activityConfigs = new List<ActivityConfig>();

        [Header("调试选项")]
        [Tooltip("启用调试日志")]
        [SerializeField] private bool enableDebugLog = false;

        #region Properties

        public bool EnableActivitySystem => enableActivitySystem;
        public string ActivityCanvasName => activityCanvasName;
        public bool EnableDebugLog => enableDebugLog;

        #endregion

        #region Public Methods

        /// <summary>
        /// 获取所有活动配置
        /// </summary>
        public List<ActivityConfig> GetAllConfigs()
        {
            return activityConfigs;
        }

        /// <summary>
        /// 根据ID获取配置
        /// </summary>
        public ActivityConfig GetConfig(string activityId)
        {
            if (string.IsNullOrEmpty(activityId))
            {
                return null;
            }

            return activityConfigs.FirstOrDefault(c => c.ActivityId == activityId);
        }

        /// <summary>
        /// 获取已启用的活动配置
        /// </summary>
        public List<ActivityConfig> GetEnabledConfigs()
        {
            return activityConfigs.Where(c => c.IsEnabled).ToList();
        }

        #endregion

        #region Validation

        /// <summary>
        /// 验证所有配置
        /// </summary>
        public bool ValidateAllConfigs()
        {
            bool allValid = true;

            if (activityConfigs == null || activityConfigs.Count == 0)
            {
                Debug.LogWarning("[ActivitySettings] 没有配置任何活动");
                return true;
            }

            // 检查每个配置
            foreach (var config in activityConfigs)
            {
                if (!config.Validate())
                {
                    Debug.LogError($"[ActivitySettings] 活动配置无效: {config.ActivityId}");
                    allValid = false;
                }
            }

            // 检查重复ID
            var duplicates = activityConfigs.GroupBy(c => c.ActivityId)
                                            .Where(g => g.Count() > 1)
                                            .Select(g => g.Key);

            foreach (var duplicateId in duplicates)
            {
                Debug.LogError($"[ActivitySettings] 活动ID重复: {duplicateId}");
                allValid = false;
            }

            return allValid;
        }

        #endregion

        #region Unity Callbacks

#if UNITY_EDITOR
        private void OnValidate()
        {
            // 编辑器中修改配置时自动验证
            ValidateAllConfigs();
        }
#endif

        #endregion
    }
}
