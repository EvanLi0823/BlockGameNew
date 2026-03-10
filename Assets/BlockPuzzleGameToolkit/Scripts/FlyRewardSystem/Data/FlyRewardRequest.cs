// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.

using UnityEngine;
using System;

namespace BlockPuzzleGameToolkit.Scripts.FlyRewardSystem.Data
{
    /// <summary>
    /// 飞行奖励请求数据
    /// </summary>
    [Serializable]
    public class FlyRewardRequest
    {
        /// <summary>
        /// 奖励类型
        /// </summary>
        public FlyRewardType rewardType = FlyRewardType.Cash;

        /// <summary>
        /// 动画模式
        /// </summary>
        public FlyAnimationPattern animationPattern = FlyAnimationPattern.FireworkBurst;

        /// <summary>
        /// 起始世界坐标位置
        /// </summary>
        public Vector3 startWorldPosition;

        /// <summary>
        /// 目标世界坐标位置（如果为null，则飞向货币文本位置）
        /// </summary>
        public Vector3? targetWorldPosition;

        /// <summary>
        /// 目标Transform（优先级高于targetWorldPosition）
        /// </summary>
        public Transform targetTransform;

        /// <summary>
        /// 飞行物体数量
        /// </summary>
        public int itemCount = 10;

        /// <summary>
        /// 奖励金额（用于触发货币更新）
        /// </summary>
        public int rewardAmount = 0;

        /// <summary>
        /// 自定义预制体（当rewardType为Custom时使用）
        /// </summary>
        public GameObject customPrefab;

        /// <summary>
        /// 动画持续时间（秒）
        /// </summary>
        public float duration = 2f;

        /// <summary>
        /// 动画完成回调
        /// </summary>
        public Action onComplete;

        /// <summary>
        /// 第一个物体到达时的回调（用于触发货币滚动）
        /// </summary>
        public Action onFirstItemReached;

        /// <summary>
        /// 是否自动触发货币更新
        /// </summary>
        public bool autoUpdateCurrency = true;

        /// <summary>
        /// 是否播放音效
        /// </summary>
        public bool playSound = true;

        /// <summary>
        /// 动画完成后的延迟时间（秒）
        /// 如果为负数，则使用FlyRewardConfig中的默认值
        /// </summary>
        public float completionDelay = -1f;

        /// <summary>
        /// 创建默认的金币飞行请求
        /// </summary>
        public static FlyRewardRequest CreateCoinRequest(Vector3 startPos, int count = 10)
        {
            return new FlyRewardRequest
            {
                rewardType = FlyRewardType.Cash,
                animationPattern = FlyAnimationPattern.FireworkBurst,
                startWorldPosition = startPos,
                itemCount = count
            };
        }

        /// <summary>
        /// 创建白包飞行请求
        /// </summary>
        public static FlyRewardRequest CreateWhitePackageRequest(Vector3 startPos, int count = 10)
        {
            return new FlyRewardRequest
            {
                rewardType = FlyRewardType.WhitePackage,
                animationPattern = FlyAnimationPattern.FireworkBurst,
                startWorldPosition = startPos,
                itemCount = count
            };
        }
    }
}