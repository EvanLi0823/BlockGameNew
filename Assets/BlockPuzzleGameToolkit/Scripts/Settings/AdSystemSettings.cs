using System.Collections.Generic;
using UnityEngine;
using BlockPuzzle.AdSystem.Models;

namespace BlockPuzzleGameToolkit.Scripts.Settings
{
    /// <summary>
    /// 广告系统配置
    /// ScriptableObject用于存储广告入口配置
    /// 通过 Tools > BlockPuzzleGameToolkit > Settings > Ad System settings 菜单创建
    /// </summary>
    public class AdSystemSettings : ScriptableObject
    {
        [Header("广告入口配置")]
        [Tooltip("所有广告入口配置列表")]
        [SerializeField] private List<AdEntry> _adEntries = new List<AdEntry>();

        [Header("全局设置")]
        [Tooltip("是否启用广告系统")]
        [SerializeField] private bool _enableAdSystem = true;

        [Tooltip("广告播放失败是否当作成功处理")]
        [SerializeField] private bool _treatFailureAsSuccess = true;

        [Tooltip("调试模式")]
        [SerializeField] private bool _debugMode = false;

#if UNITY_EDITOR
        [Header("编辑器专用设置")]
        [Tooltip("编辑器中模拟广告播放失败（用于测试失败逻辑）")]
        [SerializeField] private bool _simulateAdFailureInEditor = false;
#endif

        // 公共属性
        public List<AdEntry> AdEntries => _adEntries;
        public bool EnableAdSystem => _enableAdSystem;
        public bool TreatFailureAsSuccess => _treatFailureAsSuccess;
        public bool DebugMode => _debugMode;

#if UNITY_EDITOR
        /// <summary>
        /// 编辑器中是否模拟广告失败（仅编辑器有效）
        /// </summary>
        public bool SimulateAdFailureInEditor => _simulateAdFailureInEditor;
#endif

        /// <summary>
        /// 根据名称获取广告入口配置
        /// </summary>
        public AdEntry GetEntry(string entryName)
        {
            return _adEntries.Find(e => e.Name == entryName && e.Active);
        }

        /// <summary>
        /// 获取所有激活的广告入口
        /// </summary>
        public List<AdEntry> GetActiveEntries()
        {
            return _adEntries.FindAll(e => e.Active);
        }

        /// <summary>
        /// 初始化默认配置
        /// </summary>
        public void InitializeDefaults()
        {
            _adEntries.Clear();

            // 添加默认的广告入口配置
            _adEntries.Add(AdEntry.CreatePreset(AdEntryNames.LEVEL_COMPLETE, 0));
            _adEntries.Add(AdEntry.CreatePreset(AdEntryNames.REWARD_POPUP, 0));
            _adEntries.Add(AdEntry.CreatePreset(AdEntryNames.LEVEL_FAILED_REFRESH, 0));
            _adEntries.Add(AdEntry.CreatePreset(AdEntryNames.DAILY_TASK_REWARD, 0));
            _adEntries.Add(AdEntry.CreatePreset(AdEntryNames.JACKPOT_REWARD, 0));
            _adEntries.Add(AdEntry.CreatePreset(AdEntryNames.EXTRA_MOVES, 0));
            _adEntries.Add(AdEntry.CreatePreset(AdEntryNames.DOUBLE_COINS, 0));
            _adEntries.Add(AdEntry.CreatePreset(AdEntryNames.UNLOCK_FEATURE, 1));
            _adEntries.Add(AdEntry.CreatePreset(AdEntryNames.CONTINUE_GAME, 0));
            _adEntries.Add(AdEntry.CreatePreset(AdEntryNames.SKIP_LEVEL, 0));
        }

        /// <summary>
        /// 验证配置
        /// </summary>
        public bool ValidateSettings()
        {
            // 检查重复的入口名称
            HashSet<string> names = new HashSet<string>();
            foreach (var entry in _adEntries)
            {
                if (!entry.IsValid())
                {
                    Debug.LogError($"[AdSystem] Invalid entry configuration: {entry.Name}");
                    return false;
                }

                if (names.Contains(entry.Name))
                {
                    Debug.LogError($"[AdSystem] Duplicate entry name: {entry.Name}");
                    return false;
                }
                names.Add(entry.Name);
            }

            return true;
        }

#if UNITY_EDITOR
        /// <summary>
        /// 编辑器下重置配置
        /// </summary>
        [ContextMenu("Reset to Defaults")]
        private void ResetToDefaults()
        {
            InitializeDefaults();
            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log("[AdSystem] Settings reset to defaults");
        }

        /// <summary>
        /// 验证设置
        /// </summary>
        [ContextMenu("Validate Settings")]
        private void ValidateInEditor()
        {
            if (ValidateSettings())
            {
                Debug.Log("[AdSystem] All settings are valid");
            }
        }
#endif
    }
}