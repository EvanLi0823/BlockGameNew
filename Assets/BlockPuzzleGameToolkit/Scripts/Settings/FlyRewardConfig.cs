// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.

using UnityEngine;
using System;
using System.Collections.Generic;
using BlockPuzzleGameToolkit.Scripts.FlyRewardSystem.Data;

namespace BlockPuzzleGameToolkit.Scripts.Settings
{
    /// <summary>
    /// 飞行奖励系统配置
    /// </summary>
    [CreateAssetMenu(fileName = "FlyRewardConfig", menuName = "Block Puzzle/Settings/Fly Reward Config")]
    public class FlyRewardConfig : ScriptableObject
    {
        [Header("预制体配置")]
        [SerializeField] private List<RewardPrefabMapping> prefabMappings = new List<RewardPrefabMapping>();

        [Header("动画默认参数")]
        [Tooltip("默认动画持续时间（秒）")]
        [Range(1f, 5f)]
        [SerializeField] private float defaultDuration = 2f;

        [Tooltip("烟花爆炸半径")]
        [Range(50f, 300f)]
        [SerializeField] private float fireworkBurstRadius = 150f;

        [Tooltip("物体初始缩放")]
        [Range(0f, 2f)]
        [SerializeField] private float initialScale = 0f;

        [Tooltip("物体最大缩放")]
        [Range(1f, 3f)]
        [SerializeField] private float maxScale = 1.5f;

        [Tooltip("物体最终缩放")]
        [Range(0.1f, 1f)]
        [SerializeField] private float finalScale = 0.2f;

        [Tooltip("动画完成后的延迟时间（秒）\n在进入下一关前的等待时间")]
        [Range(0f, 5f)]
        [SerializeField] private float completionDelay = 1.5f;

        [Header("音效配置")]
        [SerializeField] private AudioClip flyStartSound;
        [SerializeField] private AudioClip itemReachSound;
        [SerializeField] private AudioClip allCompleteSound;

        [Header("性能优化")]
        [Tooltip("对象池初始大小")]
        [SerializeField] private int poolInitialSize = 20;

        [Tooltip("对象池最大大小")]
        [SerializeField] private int poolMaxSize = 50;

        /// <summary>
        /// 奖励预制体映射
        /// </summary>
        [Serializable]
        public class RewardPrefabMapping
        {
            public FlyRewardType rewardType;
            public GameObject prefab;
            public Sprite sprite;  // 如果使用Image组件
        }

        #region Properties

        public float DefaultDuration => defaultDuration;
        public float FireworkBurstRadius => fireworkBurstRadius;
        public float InitialScale => initialScale;
        public float MaxScale => maxScale;
        public float FinalScale => finalScale;
        public float CompletionDelay => completionDelay;
        public AudioClip FlyStartSound => flyStartSound;
        public AudioClip ItemReachSound => itemReachSound;
        public AudioClip AllCompleteSound => allCompleteSound;
        public int PoolInitialSize => poolInitialSize;
        public int PoolMaxSize => poolMaxSize;

        #endregion

        #region Public Methods

        /// <summary>
        /// 获取奖励类型对应的预制体
        /// </summary>
        public GameObject GetPrefab(FlyRewardType rewardType)
        {
            // 确保prefabMappings不为null
            if (prefabMappings == null || prefabMappings.Count == 0)
            {
                Debug.LogWarning($"[FlyRewardConfig] 预制体映射列表为空，无法获取类型 {rewardType} 的预制体");
                return null;
            }

            var mapping = prefabMappings.Find(m => m.rewardType == rewardType);
            if (mapping != null && mapping.prefab != null)
            {
                return mapping.prefab;
            }

            Debug.LogWarning($"[FlyRewardConfig] 未找到类型 {rewardType} 的预制体配置");
            return null;
        }

        /// <summary>
        /// 获取奖励类型对应的精灵图片
        /// </summary>
        public Sprite GetSprite(FlyRewardType rewardType)
        {
            var mapping = prefabMappings.Find(m => m.rewardType == rewardType);
            if (mapping != null && mapping.sprite != null)
            {
                return mapping.sprite;
            }

            return null;
        }

        /// <summary>
        /// 检查配置是否有效
        /// </summary>
        public bool ValidateConfig()
        {
            bool isValid = true;

            if (prefabMappings == null || prefabMappings.Count == 0)
            {
                Debug.LogError("[FlyRewardConfig] 预制体映射列表为空");
                isValid = false;
            }

            foreach (var mapping in prefabMappings)
            {
                if (mapping.prefab == null && mapping.sprite == null)
                {
                    Debug.LogWarning($"[FlyRewardConfig] 类型 {mapping.rewardType} 没有配置预制体或精灵");
                }
            }

            return isValid;
        }

        #endregion

        #region Editor

#if UNITY_EDITOR
        /// <summary>
        /// 添加默认配置
        /// </summary>
        [ContextMenu("Add Default Mappings")]
        private void AddDefaultMappings()
        {
            prefabMappings.Clear();

            // 添加所有奖励类型的默认映射
            foreach (FlyRewardType type in Enum.GetValues(typeof(FlyRewardType)))
            {
                if (type == FlyRewardType.Custom) continue;

                prefabMappings.Add(new RewardPrefabMapping
                {
                    rewardType = type,
                    prefab = null,
                    sprite = null
                });
            }

            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif

        #endregion
    }
}