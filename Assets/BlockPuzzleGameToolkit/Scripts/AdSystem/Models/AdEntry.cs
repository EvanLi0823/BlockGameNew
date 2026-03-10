using System;
using UnityEngine;

namespace BlockPuzzle.AdSystem.Models
{
    /// <summary>
    /// 广告入口配置
    /// 定义每个广告入口的参数和属性
    /// </summary>
    [Serializable]
    public class AdEntry
    {
        [Header("基础配置")]
        [Tooltip("广告入口唯一标识名称")]
        [SerializeField] private string _name = "";

        [Tooltip("广告类型：0-激励视频，1-插屏，2-AdMob")]
        [SerializeField] private int _type = 0;

        [Tooltip("是否启用此广告入口")]
        [SerializeField] private bool _active = true;

        [Header("显示配置")]
        [Tooltip("广告入口描述")]
        [SerializeField] private string _description = "";

        // 公共属性访问器
        public string Name => _name;
        public int Type => _type;
        public bool Active => _active;
        public string Description => _description;

        /// <summary>
        /// 验证配置是否有效
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(_name) && _type >= 0 && _type <= 2;
        }

        /// <summary>
        /// 创建预设的广告入口
        /// </summary>
        public static AdEntry CreatePreset(string name, int type)
        {
            return new AdEntry
            {
                _name = name,
                _type = type,
                _active = true
            };
        }
    }

    /// <summary>
    /// 广告播放结果
    /// </summary>
    [Serializable]
    public class AdPlayResult
    {
        public string entryName;
        public bool success;
        public int adType;
        public string error;
        public DateTime timestamp;

        public AdPlayResult(string name, bool isSuccess, int type)
        {
            entryName = name;
            success = isSuccess;
            adType = type;
            timestamp = DateTime.Now;
        }
    }

    /// <summary>
    /// 预定义的广告入口名称
    /// </summary>
    public static class AdEntryNames
    {
        // 关卡相关
        public const string LEVEL_COMPLETE = "level_complete";
        public const string LEVEL_FAILED_REFRESH = "level_failed_refresh";
        public const string CONTINUE_GAME = "continue_game";
        public const string SKIP_LEVEL = "skip_level";

        // 奖励相关
        public const string REWARD_POPUP = "reward_popup";
        public const string DAILY_TASK_REWARD = "daily_task_reward";
        public const string JACKPOT_REWARD = "jackpot_reward";
        public const string DOUBLE_COINS = "double_coins";

        // 功能相关
        public const string EXTRA_MOVES = "extra_moves";
        public const string UNLOCK_FEATURE = "unlock_feature";
    }
}