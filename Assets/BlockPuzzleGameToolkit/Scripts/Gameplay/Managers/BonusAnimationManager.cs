// Bonus飞行动画管理器
// 创建日期: 2026-03-06
// 用途: 统一管理所有bonus的即时收集飞行动画（不包括累计奖励弹窗）

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using BlockPuzzleGameToolkit.Scripts.Gameplay.FX;
using BlockPuzzleGameToolkit.Scripts.LevelsData;
using BlockPuzzleGameToolkit.Scripts.GameCore;

namespace BlockPuzzleGameToolkit.Scripts.Gameplay
{
    /// <summary>
    /// Bonus飞行动画管理器
    /// 负责管理所有bonus的即时收集飞行动画
    /// 提供统一的对象池和批量动画接口
    /// </summary>
    public class BonusAnimationManager : SingletonBehaviour<BonusAnimationManager>
    {
        #region Inspector配置

        [Header("对象池配置")]
        [SerializeField, Tooltip("BonusAnimation预制体")]
        private BonusAnimation bonusAnimationPrefab;

        [SerializeField, Tooltip("特效对象池父节点")]
        private Transform fxPool;

        [Header("动画配置")]
        [SerializeField, Tooltip("批量动画启动延迟（秒）")]
        private float batchStartDelay = 0.1f;

        [SerializeField, Tooltip("动画间隔时间（秒）")]
        private float animationInterval = 0.04f;

        [Header("调试选项")]
        [SerializeField, Tooltip("启用调试日志")]
        private bool enableDebugLog = false;

        #endregion

        #region 私有字段

        private ObjectPool<BonusAnimation> bonusAnimationPool;
        private readonly List<BonusAnimation> activeAnimations = new List<BonusAnimation>();

        #endregion

        #region 生命周期

        /// <summary>
        /// 初始化优先级（在大多数Manager之前初始化）
        /// </summary>
        public override int InitPriority => -10;

        public override void Awake()
        {
            base.Awake();

            // 场景中的BonusAnimationManager需要在Awake时立即初始化
            if (!IsInitialized)
            {
                OnInit();
            }
        }

        public override void OnInit()
        {
            base.OnInit();
            InitializePool();

            if (enableDebugLog)
            {
                Debug.Log("[BonusAnimationManager] 初始化完成");
            }
        }

        private void InitializePool()
        {
            if (bonusAnimationPrefab == null)
            {
                Debug.LogError("[BonusAnimationManager] bonusAnimationPrefab未配置!");
                return;
            }

            if (fxPool == null)
            {
                Debug.LogWarning("[BonusAnimationManager] fxPool未设置，使用自身作为父节点");
                fxPool = transform;
            }

            bonusAnimationPool = new ObjectPool<BonusAnimation>(
                createFunc: CreateBonusAnimation,
                actionOnGet: OnGetBonusAnimation,
                actionOnRelease: OnReleaseBonusAnimation,
                actionOnDestroy: OnDestroyBonusAnimation,
                collectionCheck: true,
                defaultCapacity: 10,
                maxSize: 50
            );

            if (enableDebugLog)
            {
                Debug.Log("[BonusAnimationManager] 对象池初始化完成");
            }
        }

        private void OnDestroy()
        {
            // 清理所有活动动画
            ClearAllAnimations();

            // 清理对象池
            bonusAnimationPool?.Clear();

            if (enableDebugLog)
            {
                Debug.Log("[BonusAnimationManager] 已销毁，资源已清理");
            }
        }

        #endregion

        #region 对象池回调

        private BonusAnimation CreateBonusAnimation()
        {
            var instance = Instantiate(bonusAnimationPrefab, fxPool);
            instance.gameObject.SetActive(false);
            return instance;
        }

        private void OnGetBonusAnimation(BonusAnimation animation)
        {
            if (animation != null)
            {
                animation.gameObject.SetActive(true);
            }
        }

        private void OnReleaseBonusAnimation(BonusAnimation animation)
        {
            if (animation != null)
            {
                animation.gameObject.SetActive(false);
                animation.transform.SetParent(fxPool);
            }
        }

        private void OnDestroyBonusAnimation(BonusAnimation animation)
        {
            if (animation != null)
            {
                Destroy(animation.gameObject);
            }
        }

        #endregion

        #region 公共接口

        /// <summary>
        /// 播放单个bonus飞行动画
        /// </summary>
        /// <param name="startPos">起点位置（世界坐标）</param>
        /// <param name="targetPos">终点位置（UI坐标）</param>
        /// <param name="bonusTemplate">bonus模板</param>
        /// <param name="onComplete">完成回调</param>
        public void PlayBonusAnimation(
            Vector3 startPos,
            Vector2 targetPos,
            BonusItemTemplate bonusTemplate,
            Action onComplete = null)
        {
            if (bonusAnimationPool == null)
            {
                Debug.LogWarning("[BonusAnimationManager] 对象池未初始化");
                onComplete?.Invoke();
                return;
            }

            if (bonusTemplate == null)
            {
                Debug.LogWarning("[BonusAnimationManager] bonusTemplate为null");
                onComplete?.Invoke();
                return;
            }

            var bonus = bonusAnimationPool.Get();
            if (bonus == null)
            {
                Debug.LogWarning("[BonusAnimationManager] 无法获取BonusAnimation实例");
                onComplete?.Invoke();
                return;
            }

            // 配置动画
            bonus.Fill(bonusTemplate);
            bonus.transform.position = startPos;
            bonus.targetPos = targetPos;
            bonus.OnFinish = _ =>
            {
                onComplete?.Invoke();
                activeAnimations.Remove(bonus);
                bonusAnimationPool.Release(bonus);
            };

            // 立即启动
            bonus.MoveTo();

            if (enableDebugLog)
            {
                Debug.Log($"[BonusAnimationManager] 播放单个动画: start={startPos}, target={targetPos}");
            }
        }

        /// <summary>
        /// 播放批量bonus飞行动画（带延迟和错开）
        /// </summary>
        public IEnumerator PlayBatchAnimations(List<BonusAnimationData> animations)
        {
            if (animations == null || animations.Count == 0)
            {
                yield break;
            }

            if (bonusAnimationPool == null)
            {
                Debug.LogWarning("[BonusAnimationManager] 对象池未初始化，跳过批量动画");
                yield break;
            }

            activeAnimations.Clear();

            // 准备所有动画
            foreach (var data in animations)
            {
                if (data.bonusTemplate == null)
                {
                    if (enableDebugLog)
                    {
                        Debug.LogWarning("[BonusAnimationManager] bonusTemplate为null，跳过");
                    }
                    continue;
                }

                var bonus = bonusAnimationPool.Get();
                if (bonus == null) continue;

                bonus.Fill(data.bonusTemplate);
                bonus.transform.position = data.startPos;
                bonus.targetPos = data.targetPos;
                bonus.OnFinish = _ =>
                {
                    data.onComplete?.Invoke();
                    activeAnimations.Remove(bonus);
                    bonusAnimationPool.Release(bonus);
                };

                activeAnimations.Add(bonus);
            }

            if (enableDebugLog)
            {
                Debug.Log($"[BonusAnimationManager] 准备播放批量动画: {activeAnimations.Count}个");
            }

            // 延迟启动
            yield return new WaitForSeconds(batchStartDelay);

            // 错开播放
            foreach (var bonus in new List<BonusAnimation>(activeAnimations))
            {
                if (bonus != null && bonus.gameObject.activeSelf)
                {
                    bonus.MoveTo();
                    yield return new WaitForSeconds(animationInterval);
                }
            }
        }

        /// <summary>
        /// 清理所有活动的动画
        /// </summary>
        public void ClearAllAnimations()
        {
            foreach (var animation in activeAnimations)
            {
                if (animation != null)
                {
                    bonusAnimationPool.Release(animation);
                }
            }
            activeAnimations.Clear();

            if (enableDebugLog)
            {
                Debug.Log("[BonusAnimationManager] 清理所有活动动画");
            }
        }

        #endregion

        #region 日志方法

        private void Log(string message)
        {
            if (enableDebugLog)
            {
                Debug.Log(message);
            }
        }

        #endregion
    }

    /// <summary>
    /// Bonus动画数据
    /// </summary>
    public class BonusAnimationData
    {
        public Vector3 startPos;
        public Vector2 targetPos;
        public BonusItemTemplate bonusTemplate;
        public Action onComplete;
    }
}
