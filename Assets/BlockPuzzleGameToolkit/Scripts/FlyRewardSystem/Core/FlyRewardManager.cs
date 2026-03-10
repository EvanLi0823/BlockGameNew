// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using BlockPuzzleGameToolkit.Scripts.Settings;
using BlockPuzzleGameToolkit.Scripts.FlyRewardSystem.Data;
using BlockPuzzleGameToolkit.Scripts.CurrencySystem;
using BlockPuzzleGameToolkit.Scripts.TopPanel;
using BlockPuzzleGameToolkit.Scripts.Audio;
using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.GameCore;

namespace BlockPuzzleGameToolkit.Scripts.FlyRewardSystem.Core
{
    /// <summary>
    /// 飞行奖励系统管理器
    /// 负责管理所有飞行奖励动画的播放
    /// </summary>
    public class FlyRewardManager : SingletonBehaviour<FlyRewardManager>
    {
        #region Fields

        [Header("配置")]
        [SerializeField] private FlyRewardConfig config;

        [Header("容器")]
        [Tooltip("飞行物体的父容器")]
        [SerializeField] private Transform flyItemContainer;

        [Header("运行时状态")]
        [SerializeField] private bool isPlaying = false;
        [SerializeField] private int activeAnimationCount = 0;

        // 对象池
        private Dictionary<FlyRewardType, Queue<GameObject>> objectPools;

        // 当前活跃的动画序列
        private List<Sequence> activeSequences;

        // 缓存的等待时间
        private readonly WaitForSeconds waitHalfSecond = new WaitForSeconds(0.5f);
        private readonly WaitForSeconds waitOneSecond = new WaitForSeconds(1f);

        // 烟花爆炸位置预设
        private readonly List<Vector3[]> fireworkPaths = new()
        {
            // 烟花效果路径：先向上飞到爆炸点，然后向四周散开并下落
            new Vector3[] { new(0, 150, 0), new(-180, 100, 0) },   // 左上
            new Vector3[] { new(0, 150, 0), new(180, 100, 0) },    // 右上
            new Vector3[] { new(0, 150, 0), new(-200, 0, 0) },     // 左侧
            new Vector3[] { new(0, 150, 0), new(200, 0, 0) },      // 右侧
            new Vector3[] { new(0, 150, 0), new(-150, -100, 0) },  // 左下
            new Vector3[] { new(0, 150, 0), new(150, -100, 0) },   // 右下
            new Vector3[] { new(0, 150, 0), new(-50, 200, 0) },    // 上偏左
            new Vector3[] { new(0, 150, 0), new(50, 200, 0) },     // 上偏右
            new Vector3[] { new(0, 150, 0), new(-120, 50, 0) },    // 左中
            new Vector3[] { new(0, 150, 0), new(120, 50, 0) },     // 右中
        };

        #endregion

        #region Initialization

        public override int InitPriority => 40; // 在货币系统之后初始化

        protected override bool DontDestroyOnSceneChange => true;

        public override void OnInit()
        {
            if (IsInitialized) return;

            Debug.Log("[FlyRewardManager] 初始化飞行奖励系统");

            // 加载配置
            LoadConfig();

            // 先初始化容器（必须在对象池之前）
            InitializeContainer();

            // 初始化对象池（依赖容器）
            InitializeObjectPools();

            // 初始化列表
            activeSequences = new List<Sequence>();

            // 订阅事件
            SubscribeEvents();

            base.OnInit();
        }

        private void LoadConfig()
        {
            if (config == null)
            {
                config = Resources.Load<FlyRewardConfig>("Settings/FlyRewardConfig");
                if (config == null)
                {
                    Debug.LogError("[FlyRewardManager] 无法加载FlyRewardConfig配置文件，尝试创建默认配置");
                    // 创建默认配置以避免空引用
                    config = ScriptableObject.CreateInstance<FlyRewardConfig>();
                }
            }
        }

        private void InitializeContainer()
        {
            if (flyItemContainer == null)
            {
                // 查找或创建容器
                GameObject containerObj = new GameObject("FlyRewardContainer");
                containerObj.transform.SetParent(transform, false);

                RectTransform rect = containerObj.AddComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.sizeDelta = Vector2.zero;
                rect.anchoredPosition = Vector2.zero;

                flyItemContainer = containerObj.transform;
            }
        }

        private void InitializeObjectPools()
        {
            objectPools = new Dictionary<FlyRewardType, Queue<GameObject>>();

            // 即使config为null也要初始化对象池
            foreach (FlyRewardType type in Enum.GetValues(typeof(FlyRewardType)))
            {
                if (type == FlyRewardType.Custom) continue;
                objectPools[type] = new Queue<GameObject>();
            }

            // 如果有配置，预创建对象
            if (config != null)
            {
                foreach (FlyRewardType type in Enum.GetValues(typeof(FlyRewardType)))
                {
                    if (type == FlyRewardType.Custom) continue;

                    GameObject prefab = config.GetPrefab(type);
                    if (prefab != null)
                    {
                        for (int i = 0; i < config.PoolInitialSize; i++)
                        {
                            GameObject obj = CreatePoolObject(prefab, type);
                            if (obj != null)  // 添加空检查
                            {
                                obj.SetActive(false);
                                objectPools[type].Enqueue(obj);
                            }
                        }
                    }
                }
            }
        }

        private void SubscribeEvents()
        {
            // 订阅自定义事件（如果需要）
            EventManager.GetEvent(EGameEvent.PlayFlyReward).Subscribe(OnPlayFlyRewardEvent);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 播放飞行奖励动画
        /// </summary>
        public void PlayFlyAnimation(FlyRewardRequest request)
        {
            if (request == null)
            {
                Debug.LogError("[FlyRewardManager] 请求参数为空");
                return;
            }

            // 检查是否已初始化
            if (!IsInitialized)
            {
                Debug.LogError("[FlyRewardManager] FlyRewardManager未初始化！请确保GameManager已启动");
                return;
            }

            if (config == null)
            {
                Debug.LogError("[FlyRewardManager] 配置未加载");
                return;
            }

            StartCoroutine(PlayFlyAnimationCoroutine(request));
        }

        /// <summary>
        /// 播放金币飞行动画（便捷方法）
        /// </summary>
        public void PlayCoinFly(Vector3 startWorldPos, int amount = 0, int count = 10, Action onComplete = null)
        {
            var request = FlyRewardRequest.CreateCoinRequest(startWorldPos, count);
            request.rewardAmount = amount;
            request.onComplete = onComplete;
            PlayFlyAnimation(request);
        }

        /// <summary>
        /// 播放白包飞行动画（便捷方法）
        /// </summary>
        public void PlayWhitePackageFly(Vector3 startWorldPos, int count = 10, Action onComplete = null)
        {
            var request = FlyRewardRequest.CreateWhitePackageRequest(startWorldPos, count);
            request.onComplete = onComplete;
            PlayFlyAnimation(request);
        }

        /// <summary>
        /// 停止所有动画
        /// </summary>
        public void StopAllAnimations()
        {
            foreach (var seq in activeSequences)
            {
                seq?.Kill(false);
            }
            activeSequences.Clear();
            activeAnimationCount = 0;
            isPlaying = false;
        }

        /// <summary>
        /// 是否正在播放动画
        /// </summary>
        public bool IsPlaying => isPlaying;

        #endregion

        #region Private Methods

        private IEnumerator PlayFlyAnimationCoroutine(FlyRewardRequest request)
        {
            isPlaying = true;
            activeAnimationCount++;

            // 根据动画模式播放不同的动画
            switch (request.animationPattern)
            {
                case FlyAnimationPattern.DirectFly:
                    yield return StartCoroutine(PlayDirectFly(request));
                    break;

                case FlyAnimationPattern.FireworkBurst:
                    yield return StartCoroutine(PlayFireworkBurst(request));
                    break;

                case FlyAnimationPattern.Parabolic:
                    yield return StartCoroutine(PlayParabolicFly(request));
                    break;

                case FlyAnimationPattern.RandomScatter:
                    yield return StartCoroutine(PlayRandomScatter(request));
                    break;

                default:
                    yield return StartCoroutine(PlayFireworkBurst(request));
                    break;
            }

            activeAnimationCount--;
            if (activeAnimationCount <= 0)
            {
                isPlaying = false;
            }

            // 获取延迟时间（优先使用request中的值，如果为负数则使用config中的默认值）
            float delay = request.completionDelay >= 0 ? request.completionDelay : config.CompletionDelay;

            // 动画播放完成后等待配置的延迟时间
            if (delay > 0)
            {
                Debug.Log($"[FlyRewardManager] 动画播放完成，等待{delay}秒后执行回调");
                yield return new WaitForSeconds(delay);
            }

            // 执行完成回调
            request.onComplete?.Invoke();
        }

        /// <summary>
        /// 烟花爆炸效果
        /// </summary>
        private IEnumerator PlayFireworkBurst(FlyRewardRequest request)
        {
            // 获取或创建飞行物体
            List<GameObject> items = GetOrCreateItems(request);

            if (items.Count == 0)
            {
                Debug.LogWarning("[FlyRewardManager] 无法创建飞行物体");
                yield break;
            }

            // 播放开始音效
            if (request.playSound && config.FlyStartSound != null)
            {
                var soundBase = BlockPuzzleGameToolkit.Scripts.Audio.SoundBase.Instance;
                soundBase?.PlaySound(config.FlyStartSound);
            }

            // 将物体转换为局部坐标系
            Vector3 localStartPos = ConvertToLocalPosition(request.startWorldPosition);

            // 获取目标位置
            Vector3 targetWorldPos = GetTargetWorldPosition(request);
            Vector3 localTargetPos = ConvertToLocalPosition(targetWorldPos);

            // 创建动画序列
            Sequence mainSequence = DOTween.Sequence();
            activeSequences.Add(mainSequence);

            // ========== 第一阶段：烟花爆炸效果 ==========
            float firstStageDuration = request.duration * 0.3f; // 30% 时间用于爆炸

            Vector3 explosionPoint = localStartPos + new Vector3(0, config.FireworkBurstRadius, 0);

            for (int i = 0; i < items.Count; i++)
            {
                GameObject item = items[i];
                RectTransform rect = item.GetComponent<RectTransform>();

                if (rect == null) continue;

                // 设置初始位置和缩放
                rect.anchoredPosition = localStartPos;
                rect.localScale = Vector3.zero;
                item.SetActive(true);

                int pathIndex = i % fireworkPaths.Count;
                Vector3 burstEndPos = localStartPos + fireworkPaths[pathIndex][1];

                float delay = i * 0.02f; // 轻微延迟创建爆炸效果

                // 快速放大（爆炸瞬间）
                mainSequence.Insert(delay,
                    rect.DOScale(config.MaxScale, 0.1f)
                        .SetEase(Ease.OutExpo)
                );

                // 移动到爆炸位置
                mainSequence.Insert(delay + 0.05f,
                    rect.DOAnchorPos(burstEndPos, firstStageDuration - 0.1f)
                        .SetEase(Ease.OutCubic)
                );

                // 缩小到正常大小
                mainSequence.Insert(delay + 0.1f,
                    rect.DOScale(1f, firstStageDuration - 0.1f)
                        .SetEase(Ease.InQuad)
                );

                // 添加闪烁效果
                if (item.TryGetComponent<CanvasGroup>(out var canvasGroup) == false)
                {
                    canvasGroup = item.AddComponent<CanvasGroup>();
                }

                mainSequence.Insert(delay,
                    DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 0.7f, 0.05f)
                );
                mainSequence.Insert(delay + 0.05f,
                    DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 1f, 0.05f)
                );
            }

            // 等待第一阶段完成
            yield return new WaitForSeconds(firstStageDuration);

            // ========== 第二阶段：飞向目标 ==========
            float secondStageDuration = request.duration * 0.7f; // 70% 时间用于飞行

            Sequence flySequence = DOTween.Sequence();
            activeSequences.Add(flySequence);

            float flyInterval = 0.03f;
            bool firstItemReached = false;

            for (int i = 0; i < items.Count; i++)
            {
                GameObject item = items[i];
                RectTransform rect = item.GetComponent<RectTransform>();

                if (rect == null) continue;

                int index = i;

                flySequence.InsertCallback(index * flyInterval, () =>
                {
                    Sequence individualSequence = DOTween.Sequence();

                    // 飞向目标
                    individualSequence.Append(
                        rect.DOAnchorPos(localTargetPos, secondStageDuration * 0.6f)
                            .SetEase(Ease.InExpo)
                    );

                    // 缩小
                    individualSequence.Join(
                        rect.DOScale(config.FinalScale, secondStageDuration * 0.6f)
                            .SetEase(Ease.InQuad)
                    );

                    individualSequence.OnComplete(() =>
                    {
                        // 第一个物体到达时触发回调
                        if (!firstItemReached)
                        {
                            firstItemReached = true;

                            // 触发货币更新
                            if (request.autoUpdateCurrency && request.rewardAmount > 0)
                            {
                                EventManager.GetEvent(EGameEvent.CurrencyChanged).Invoke();
                            }

                            // 播放到达音效
                            if (request.playSound && config.ItemReachSound != null)
                            {
                                var soundBase = BlockPuzzleGameToolkit.Scripts.Audio.SoundBase.Instance;
                                soundBase?.PlaySound(config.ItemReachSound);
                            }

                            request.onFirstItemReached?.Invoke();
                        }

                        // 回收物体
                        RecycleItem(item, request.rewardType);
                    });
                });
            }

            // 等待所有动画完成
            yield return new WaitForSeconds(secondStageDuration);

            // 播放完成音效
            if (request.playSound && config.AllCompleteSound != null)
            {
                var soundBase = BlockPuzzleGameToolkit.Scripts.Audio.SoundBase.Instance;
                soundBase?.PlaySound(config.AllCompleteSound);
            }

            // 清理序列
            activeSequences.Remove(mainSequence);
            activeSequences.Remove(flySequence);
        }

        /// <summary>
        /// 直接飞行
        /// </summary>
        private IEnumerator PlayDirectFly(FlyRewardRequest request)
        {
            // 简化版本，直接飞向目标
            List<GameObject> items = GetOrCreateItems(request);

            Vector3 localStartPos = ConvertToLocalPosition(request.startWorldPosition);
            Vector3 targetWorldPos = GetTargetWorldPosition(request);
            Vector3 localTargetPos = ConvertToLocalPosition(targetWorldPos);

            Sequence sequence = DOTween.Sequence();
            activeSequences.Add(sequence);

            foreach (var item in items)
            {
                RectTransform rect = item.GetComponent<RectTransform>();
                if (rect == null) continue;

                rect.anchoredPosition = localStartPos;
                rect.localScale = Vector3.one;
                item.SetActive(true);

                sequence.Append(
                    rect.DOAnchorPos(localTargetPos, request.duration)
                        .SetEase(Ease.InOutQuad)
                );

                sequence.Join(
                    rect.DOScale(config.FinalScale, request.duration)
                        .SetEase(Ease.InQuad)
                );
            }

            sequence.OnComplete(() =>
            {
                foreach (var item in items)
                {
                    RecycleItem(item, request.rewardType);
                }
                activeSequences.Remove(sequence);
            });

            yield return new WaitForSeconds(request.duration);
        }

        /// <summary>
        /// 抛物线飞行
        /// </summary>
        private IEnumerator PlayParabolicFly(FlyRewardRequest request)
        {
            // TODO: 实现抛物线飞行效果
            yield return StartCoroutine(PlayDirectFly(request));
        }

        /// <summary>
        /// 随机散开
        /// </summary>
        private IEnumerator PlayRandomScatter(FlyRewardRequest request)
        {
            // TODO: 实现随机散开效果
            yield return StartCoroutine(PlayFireworkBurst(request));
        }

        /// <summary>
        /// 获取或创建飞行物体
        /// </summary>
        private List<GameObject> GetOrCreateItems(FlyRewardRequest request)
        {
            List<GameObject> items = new List<GameObject>();

            GameObject prefab = null;

            // 获取预制体
            if (request.rewardType == FlyRewardType.Custom)
            {
                prefab = request.customPrefab;
            }
            else
            {
                prefab = config?.GetPrefab(request.rewardType);
            }

            if (prefab == null)
            {
                Debug.LogWarning($"[FlyRewardManager] 无法找到类型 {request.rewardType} 的预制体");
                return items;
            }

            // 从对象池获取或创建新对象
            for (int i = 0; i < request.itemCount; i++)
            {
                GameObject item = GetFromPool(request.rewardType, prefab);
                if (item != null)
                {
                    items.Add(item);
                }
            }

            return items;
        }

        /// <summary>
        /// 从对象池获取对象
        /// </summary>
        private GameObject GetFromPool(FlyRewardType type, GameObject prefab)
        {
            GameObject obj = null;

            // 检查系统是否已经初始化
            if (!IsInitialized)
            {
                Debug.LogError("[FlyRewardManager] FlyRewardManager未初始化！请确保GameManager已启动");
                return null;
            }

            // 检查对象池是否已初始化
            if (objectPools == null)
            {
                Debug.LogError("[FlyRewardManager] 对象池为空！初始化可能失败");
                return null;
            }

            // 尝试从对象池获取
            if (type != FlyRewardType.Custom && objectPools.ContainsKey(type) && objectPools[type].Count > 0)
            {
                obj = objectPools[type].Dequeue();

                // 检查对象是否有效
                if (obj != null)
                {
                    obj.SetActive(true);
                }
                else
                {
                    // 对象已被销毁，需要创建新的
                    obj = CreatePoolObject(prefab, type);
                }
            }

            // 如果没有可用对象，创建新的
            if (obj == null)
            {
                obj = CreatePoolObject(prefab, type);
            }

            return obj;
        }

        /// <summary>
        /// 创建池对象
        /// </summary>
        private GameObject CreatePoolObject(GameObject prefab, FlyRewardType type)
        {
            // 检查预制体是否存在
            if (prefab == null)
            {
                Debug.LogError($"[FlyRewardManager] 无法创建飞行物体，预制体为空！类型：{type}");
                return null;
            }

            // 检查容器是否存在
            if (flyItemContainer == null)
            {
                Debug.LogError("[FlyRewardManager] 飞行物体容器未初始化！");
                InitializeContainer(); // 尝试重新初始化

                if (flyItemContainer == null)
                {
                    Debug.LogError("[FlyRewardManager] 无法初始化飞行物体容器！");
                    return null;
                }
            }

            GameObject obj = Instantiate(prefab, flyItemContainer);

            // 确保有RectTransform组件
            if (!obj.GetComponent<RectTransform>())
            {
                obj.AddComponent<RectTransform>();
            }

            // 添加标识组件（用于回收时识别）
            obj.name = $"FlyItem_{type}";

            return obj;
        }

        /// <summary>
        /// 回收物体到对象池
        /// </summary>
        private void RecycleItem(GameObject item, FlyRewardType type)
        {
            if (item == null) return;

            item.SetActive(false);

            // 重置状态
            RectTransform rect = item.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.localScale = Vector3.one;
                rect.anchoredPosition = Vector2.zero;
            }

            // 放回对象池
            if (type != FlyRewardType.Custom && objectPools.ContainsKey(type))
            {
                if (objectPools[type].Count < config.PoolMaxSize)
                {
                    objectPools[type].Enqueue(item);
                }
                else
                {
                    Destroy(item);
                }
            }
            else
            {
                Destroy(item);
            }
        }

        /// <summary>
        /// 获取目标世界位置
        /// </summary>
        private Vector3 GetTargetWorldPosition(FlyRewardRequest request)
        {
            // 优先使用Transform
            if (request.targetTransform != null)
            {
                return request.targetTransform.position;
            }

            // 其次使用指定位置
            if (request.targetWorldPosition.HasValue)
            {
                return request.targetWorldPosition.Value;
            }

            // 默认飞向货币文本位置
            var topPanel = BlockPuzzleGameToolkit.Scripts.TopPanel.TopPanel.Instance;
            if (topPanel != null)
            {
                Transform currencyTransform = topPanel.GetCurrencyTextTransForm();
                if (currencyTransform != null)
                {
                    return currencyTransform.position;
                }
            }

            // 如果都没有，返回屏幕顶部中心
            return new Vector3(Screen.width / 2f, Screen.height - 100f, 0);
        }

        /// <summary>
        /// 转换世界坐标到局部坐标
        /// </summary>
        private Vector3 ConvertToLocalPosition(Vector3 worldPos)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            Camera uiCamera = canvas.worldCamera;

            RectTransform containerRect = flyItemContainer as RectTransform;

            // 将世界坐标转换为屏幕坐标
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, worldPos);

            // 将屏幕坐标转换为容器的局部坐标
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                containerRect,
                screenPoint,
                uiCamera,
                out localPoint
            );

            return localPoint;
        }

        /// <summary>
        /// 事件响应：播放飞行奖励
        /// </summary>
        private void OnPlayFlyRewardEvent()
        {
            // 事件触发的默认处理
            // 可以从事件参数中获取FlyRewardRequest
        }

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            StopAllAnimations();

            // 清理对象池
            if (objectPools != null)
            {
                foreach (var pool in objectPools.Values)
                {
                    while (pool.Count > 0)
                    {
                        GameObject obj = pool.Dequeue();
                        if (obj != null)
                            Destroy(obj);
                    }
                }
                objectPools.Clear();
            }

            // 取消订阅事件
            EventManager.GetEvent(EGameEvent.PlayFlyReward)?.Unsubscribe(OnPlayFlyRewardEvent);
        }

        #endregion

        #region Debug

#if UNITY_EDITOR
        [ContextMenu("Test Coin Fly")]
        private void TestCoinFly()
        {
            Vector3 centerPos = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
            PlayCoinFly(centerPos, 1000, 10, () => Debug.Log("Coin fly completed!"));
        }

        [ContextMenu("Test White Package Fly")]
        private void TestWhitePackageFly()
        {
            Vector3 centerPos = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
            PlayWhitePackageFly(centerPos, 10, () => Debug.Log("White package fly completed!"));
        }
#endif

        #endregion
    }
}