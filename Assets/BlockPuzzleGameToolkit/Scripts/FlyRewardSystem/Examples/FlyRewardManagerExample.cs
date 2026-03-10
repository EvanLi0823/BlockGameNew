// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.

using UnityEngine;
using BlockPuzzleGameToolkit.Scripts.FlyRewardSystem.Core;
using BlockPuzzleGameToolkit.Scripts.FlyRewardSystem.Data;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using BlockPuzzleGameToolkit.Scripts.Enums;

namespace BlockPuzzleGameToolkit.Scripts.FlyRewardSystem.Examples
{
    /// <summary>
    /// FlyRewardManager 使用示例
    /// 展示如何在不同场景下使用飞行奖励系统
    /// </summary>
    public class FlyRewardManagerExample : MonoBehaviour
    {
        [Header("测试配置")]
        [SerializeField] private Transform startPoint;
        [SerializeField] private Transform targetPoint;
        [SerializeField] private int rewardAmount = 1000;
        [SerializeField] private int itemCount = 10;

        #region 基本使用示例

        /// <summary>
        /// 示例1：最简单的金币飞行动画
        /// </summary>
        public void PlaySimpleCoinFly()
        {
            // 获取起始位置（世界坐标）
            Vector3 startWorldPos = startPoint ? startPoint.position : transform.position;

            // 播放金币飞行动画（会自动飞向货币文本位置）
            Core.FlyRewardManager.Instance.PlayCoinFly(
                startWorldPos,
                rewardAmount,
                itemCount,
                () => Debug.Log("金币飞行动画完成")
            );
        }

        /// <summary>
        /// 示例2：白包飞行动画
        /// </summary>
        public void PlayWhitePackageFly()
        {
            Vector3 startWorldPos = startPoint ? startPoint.position : transform.position;

            // 播放白包飞行动画
            FlyRewardManager.Instance.PlayWhitePackageFly(
                startWorldPos,
                itemCount,
                () => Debug.Log("白包飞行动画完成")
            );
        }

        #endregion

        #region 高级使用示例

        /// <summary>
        /// 示例3：自定义飞行请求
        /// </summary>
        public void PlayCustomFly()
        {
            // 创建自定义飞行请求
            var request = new FlyRewardRequest
            {
                // 奖励类型
                rewardType = FlyRewardType.Diamond,

                // 动画模式
                animationPattern = FlyAnimationPattern.FireworkBurst,

                // 起始位置
                startWorldPosition = startPoint ? startPoint.position : transform.position,

                // 目标位置（可选）
                targetTransform = targetPoint,

                // 飞行物体数量
                itemCount = 15,

                // 奖励金额
                rewardAmount = 5000,

                // 动画持续时间
                duration = 3f,

                // 是否自动更新货币
                autoUpdateCurrency = true,

                // 是否播放音效
                playSound = true,

                // 完成回调
                onComplete = () =>
                {
                    Debug.Log("自定义飞行动画完成");
                    // 可以在这里触发其他逻辑
                },

                // 第一个物体到达时的回调
                onFirstItemReached = () =>
                {
                    Debug.Log("第一个物体已到达目标");
                    // 可以在这里触发货币滚动动画
                }
            };

            // 播放动画
            FlyRewardManager.Instance.PlayFlyAnimation(request);
        }

        /// <summary>
        /// 示例4：使用不同的动画模式
        /// </summary>
        public void PlayWithDifferentPattern(FlyAnimationPattern pattern)
        {
            var request = FlyRewardRequest.CreateCoinRequest(
                startPoint ? startPoint.position : transform.position,
                itemCount
            );

            // 设置动画模式
            request.animationPattern = pattern;
            request.duration = 2.5f;

            // 播放动画
            FlyRewardManager.Instance.PlayFlyAnimation(request);
        }

        /// <summary>
        /// 示例5：自定义预制体飞行
        /// </summary>
        public void PlayCustomPrefabFly(GameObject customPrefab)
        {
            var request = new FlyRewardRequest
            {
                rewardType = FlyRewardType.Custom,
                customPrefab = customPrefab,
                startWorldPosition = startPoint ? startPoint.position : transform.position,
                itemCount = 8,
                animationPattern = FlyAnimationPattern.RandomScatter,
                duration = 2f
            };

            FlyRewardManager.Instance.PlayFlyAnimation(request);
        }

        #endregion

        #region 事件触发示例

        /// <summary>
        /// 示例6：通过事件触发飞行动画
        /// </summary>
        public void TriggerFlyRewardEvent()
        {
            // 创建请求数据
            var request = FlyRewardRequest.CreateCoinRequest(transform.position, 10);

            // 通过事件管理器触发
            // 注意：需要在FlyRewardManager中处理这个事件
            EventManager.GetEvent(Enums.EGameEvent.PlayFlyReward).Invoke();
        }

        #endregion

        #region 实际使用场景示例

        /// <summary>
        /// 场景1：关卡完成奖励
        /// </summary>
        public void OnLevelComplete(int levelReward, bool isWhitePackage)
        {
            // 获取关卡完成位置（例如从屏幕中心）
            Vector3 centerScreen = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);

            if (isWhitePackage)
            {
                // 白包奖励
                FlyRewardManager.Instance.PlayWhitePackageFly(
                    centerScreen,
                    10,
                    () =>
                    {
                        Debug.Log("关卡奖励发放完成，进入下一关");
                        // GameManager.Instance.NextLevel();
                    }
                );
            }
            else
            {
                // 普通金币奖励
                FlyRewardManager.Instance.PlayCoinFly(
                    centerScreen,
                    levelReward,
                    10,
                    () =>
                    {
                        Debug.Log("关卡奖励发放完成");
                    }
                );
            }
        }

        /// <summary>
        /// 场景2：道具购买成功
        /// </summary>
        public void OnPropPurchased(Transform propButton, int cost)
        {
            // 从道具按钮位置飞出金币（表示消费）
            var request = new FlyRewardRequest
            {
                rewardType = FlyRewardType.Cash,
                animationPattern = FlyAnimationPattern.DirectFly,
                startWorldPosition = propButton.position,
                // 反向飞行（从货币位置飞向道具按钮）
                targetTransform = propButton,
                itemCount = 5,
                rewardAmount = -cost, // 负数表示消费
                duration = 1f,
                autoUpdateCurrency = false // 手动控制货币更新
            };

            FlyRewardManager.Instance.PlayFlyAnimation(request);
        }

        /// <summary>
        /// 场景3：连消奖励
        /// </summary>
        public void OnComboReward(Vector3 comboPosition, int comboCount)
        {
            // 根据连消数量决定奖励
            int reward = comboCount * 100;
            int itemCount = Mathf.Min(comboCount * 2, 20); // 最多20个飞行物体

            var request = new FlyRewardRequest
            {
                rewardType = FlyRewardType.Star,
                animationPattern = FlyAnimationPattern.FireworkBurst,
                startWorldPosition = comboPosition,
                itemCount = itemCount,
                rewardAmount = reward,
                duration = 1.5f,
                playSound = true
            };

            FlyRewardManager.Instance.PlayFlyAnimation(request);
        }

        #endregion

        #region 测试按钮（Unity Editor）

#if UNITY_EDITOR
        [ContextMenu("Test Simple Coin Fly")]
        private void TestSimpleCoinFly()
        {
            PlaySimpleCoinFly();
        }

        [ContextMenu("Test White Package Fly")]
        private void TestWhitePackageFly()
        {
            PlayWhitePackageFly();
        }

        [ContextMenu("Test Firework Pattern")]
        private void TestFireworkPattern()
        {
            PlayWithDifferentPattern(FlyAnimationPattern.FireworkBurst);
        }

        [ContextMenu("Test Direct Fly Pattern")]
        private void TestDirectPattern()
        {
            PlayWithDifferentPattern(FlyAnimationPattern.DirectFly);
        }

        [ContextMenu("Test Stop All Animations")]
        private void TestStopAllAnimations()
        {
            FlyRewardManager.Instance.StopAllAnimations();
            Debug.Log("所有飞行动画已停止");
        }
#endif

        #endregion
    }
}