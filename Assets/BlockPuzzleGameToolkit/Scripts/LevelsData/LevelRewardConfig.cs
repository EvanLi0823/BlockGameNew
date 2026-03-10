// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

using System;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.LevelsData
{
    /// <summary>
    /// 关卡奖励配置
    /// 配置关卡通关后的奖励弹窗类型和相关参数
    /// </summary>
    [Serializable]
    public class LevelRewardConfig
    {
        /// <summary>
        /// 奖励弹窗类型
        /// </summary>
        public enum RewardPopupType
        {
            [Tooltip("固定倍率弹窗")]
            Fixed,      // 固定倍率弹窗

            [Tooltip("滑动倍率弹窗")]
            Sliding     // 滑动倍率弹窗
        }

        [Header("弹窗类型")]
        [SerializeField, Tooltip("选择奖励弹窗的类型")]
        private RewardPopupType popupType = RewardPopupType.Fixed;

        [Header("通用配置")]
        [SerializeField, Tooltip("不领奖按钮的倍率，两种弹窗都使用")]
        [Range(0.01f, 1f)]
        private float skipMultiplier = 0.2f;

        [Header("广告配置")]
        [SerializeField, Tooltip("是否显示激励广告（false时显示免费领奖按钮）")]
        private bool showRewardAd = true;

        [SerializeField, Tooltip("是否在点击不领奖按钮时显示插屏广告")]
        private bool showInterstitialAd = true;

        [Header("固定倍率配置")]
        [SerializeField, Tooltip("固定倍率配置ID（仅Fixed类型使用）")]
        private string fixedMultiplierConfigId = "default";

        // 属性访问器
        public RewardPopupType PopupType
        {
            get => popupType;
            set => popupType = value;
        }

        public float SkipMultiplier
        {
            get => skipMultiplier;
            set => skipMultiplier = Mathf.Clamp(value, 0.01f, 1f);
        }

        public bool ShowRewardAd
        {
            get => showRewardAd;
            set => showRewardAd = value;
        }

        public bool ShowInterstitialAd
        {
            get => showInterstitialAd;
            set => showInterstitialAd = value;
        }

        public string FixedMultiplierConfigId
        {
            get => fixedMultiplierConfigId;
            set => fixedMultiplierConfigId = value;
        }

        /// <summary>
        /// 验证配置有效性
        /// </summary>
        public bool Validate()
        {
            if (skipMultiplier <= 0 || skipMultiplier > 1)
            {
                Debug.LogError($"[LevelRewardConfig] 不领奖倍率必须在0-1之间，当前值：{skipMultiplier}");
                return false;
            }

            if (popupType == RewardPopupType.Fixed && string.IsNullOrEmpty(fixedMultiplierConfigId))
            {
                Debug.LogError("[LevelRewardConfig] Fixed类型必须指定倍率配置ID");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 克隆配置
        /// </summary>
        public LevelRewardConfig Clone()
        {
            return new LevelRewardConfig
            {
                popupType = this.popupType,
                skipMultiplier = this.skipMultiplier,
                showRewardAd = this.showRewardAd,
                showInterstitialAd = this.showInterstitialAd,
                fixedMultiplierConfigId = this.fixedMultiplierConfigId
            };
        }

        /// <summary>
        /// 创建默认配置
        /// </summary>
        public static LevelRewardConfig CreateDefault()
        {
            // 尝试获取第一个配置的ID
            string firstConfigId = "default";

            #if UNITY_EDITOR
            // 在编辑器中尝试从配置文件获取第一个配置
            var settings = Resources.Load<Settings.LevelRewardMultiplierSettings>("Settings/LevelRewardMultiplierSettings");
            if (settings != null)
            {
                var allIds = settings.GetAllConfigIds();
                if (allIds != null && allIds.Length > 0)
                {
                    firstConfigId = allIds[0];
                }
            }
            #endif

            return new LevelRewardConfig
            {
                popupType = RewardPopupType.Fixed,
                skipMultiplier = 0.2f,
                showRewardAd = true,
                showInterstitialAd = true,
                fixedMultiplierConfigId = firstConfigId
            };
        }

        /// <summary>
        /// 创建新手配置
        /// </summary>
        public static LevelRewardConfig CreateBeginner()
        {
            return new LevelRewardConfig
            {
                popupType = RewardPopupType.Fixed,
                skipMultiplier = 0.3f,  // 新手更高的保底奖励
                showRewardAd = false,    // 新手不显示广告
                showInterstitialAd = false,
                fixedMultiplierConfigId = "beginner"
            };
        }

        /// <summary>
        /// 创建VIP配置
        /// </summary>
        public static LevelRewardConfig CreateVIP()
        {
            return new LevelRewardConfig
            {
                popupType = RewardPopupType.Fixed,
                skipMultiplier = 0.5f,  // VIP更高的保底奖励
                showRewardAd = false,    // VIP不显示广告
                showInterstitialAd = false,
                fixedMultiplierConfigId = "vip"
            };
        }

        /// <summary>
        /// 创建滑动倍率配置
        /// </summary>
        public static LevelRewardConfig CreateSliding()
        {
            return new LevelRewardConfig
            {
                popupType = RewardPopupType.Sliding,
                skipMultiplier = 0.2f,
                showRewardAd = true,
                showInterstitialAd = true,
                fixedMultiplierConfigId = ""  // 滑动类型不需要配置ID
            };
        }

        public override string ToString()
        {
            return $"LevelRewardConfig: Type={popupType}, Skip={skipMultiplier:F2}x, " +
                   $"RewardAd={showRewardAd}, InterstitialAd={showInterstitialAd}, " +
                   $"ConfigId={fixedMultiplierConfigId}";
        }
    }

    /// <summary>
    /// 编辑器扩展：条件显示属性
    /// </summary>
    public class ShowIfAttribute : PropertyAttribute
    {
        public string ConditionField { get; }
        public object ExpectedValue { get; }

        public ShowIfAttribute(string conditionField, object expectedValue)
        {
            ConditionField = conditionField;
            ExpectedValue = expectedValue;
        }
    }
}