// 活动系统 - 角标管理器
// 创建日期: 2026-03-09

using System.Collections.Generic;
using UnityEngine;
using BlockPuzzleGameToolkit.Scripts.Activity.Data;
using BlockPuzzleGameToolkit.Scripts.Activity.UI;

namespace BlockPuzzleGameToolkit.Scripts.Activity.Core
{
    /// <summary>
    /// 活动角标管理器
    /// 职责: 管理角标的生命周期（显示/隐藏/动画/布局）
    /// </summary>
    public class ActivityIconManager
    {
        #region Fields

        private ActivityCanvasProvider canvasProvider;
        private ActivityIconPool iconPool;
        private ActivityCanvasBounds canvasBounds;

        // 当前显示的角标 (activityId -> ActivityIcon)
        private Dictionary<string, ActivityIcon> activeIcons = new Dictionary<string, ActivityIcon>();

        // 预加载的Prefab缓存 (activityId -> GameObject)
        private Dictionary<string, GameObject> prefabCache = new Dictionary<string, GameObject>();

        private bool isInitialized = false;

        #endregion

        #region Properties

        public bool IsInitialized => isInitialized;
        public int ActiveIconCount => activeIcons.Count;

        #endregion

        #region Initialization

        /// <summary>
        /// 初始化
        /// </summary>
        public bool Initialize(ActivityCanvasProvider provider)
        {
            if (isInitialized)
            {
                ActivityLogger.LogWarning("ActivityIconManager", "已经初始化，跳过");
                return true;
            }

            if (provider == null || !provider.IsInitialized)
            {
                ActivityLogger.LogError("ActivityIconManager", "CanvasProvider无效");
                return false;
            }

            canvasProvider = provider;
            iconPool = new ActivityIconPool(maxSize: 10);

            // 创建边界检测器
            Canvas canvas = provider.GetCanvas();
            if (canvas != null)
            {
                canvasBounds = new ActivityCanvasBounds(canvas);
                ActivityLogger.Log("ActivityIconManager", $"边界检测器已创建 - {canvasBounds.GetDebugInfo()}");
            }
            else
            {
                ActivityLogger.LogWarning("ActivityIconManager", "Canvas为null，无法创建边界检测器");
            }

            isInitialized = true;
            ActivityLogger.Log("ActivityIconManager", "初始化完成");
            return true;
        }

        #endregion

        #region Show/Hide Icon

        /// <summary>
        /// 显示活动角标
        /// </summary>
        public bool ShowIcon(ActivityConfig config)
        {
            if (!isInitialized)
            {
                ActivityLogger.LogError("ActivityIconManager", "ShowIcon: 未初始化");
                return false;
            }

            if (config == null)
            {
                ActivityLogger.LogError("ActivityIconManager", "ShowIcon: config为null");
                return false;
            }

            // 检查是否已显示
            if (activeIcons.ContainsKey(config.ActivityId))
            {
                ActivityLogger.LogWarning("ActivityIconManager", $"角标已显示: {config.ActivityId}");
                return true;
            }

            // 加载Prefab
            GameObject prefab = LoadIconPrefab(config);
            if (prefab == null)
            {
                ActivityLogger.LogError("ActivityIconManager", $"Prefab加载失败: {config.ActivityId}");
                return false;
            }

            // 获取目标容器
            Transform targetContainer = canvasProvider.GetTargetContainer();
            if (targetContainer == null)
            {
                ActivityLogger.LogError("ActivityIconManager", "目标容器为null");
                return false;
            }

            // 从对象池获取GameObject实例
            GameObject iconObject = iconPool.GetInstance(prefab, targetContainer);
            if (iconObject == null)
            {
                ActivityLogger.LogError("ActivityIconManager", $"从对象池获取失例化失败: {config.ActivityId}");
                return false;
            }

            // 动态添加对应的Icon组件
            ActivityIcon icon = AddIconComponent(iconObject, config);
            if (icon == null)
            {
                ActivityLogger.LogError("ActivityIconManager", $"添加Icon组件失败: {config.ActivityId}");
                Object.Destroy(iconObject);
                return false;
            }

            // 注册到对象池的活跃列表
            iconPool.RegisterActive(icon);

            // 初始化ActivityIcon
            icon.Initialize(config.ActivityId);

            // 如果是FloatBubbleIcon，设置边界检测器并启动飘动
            if (icon is BlockPuzzleGameToolkit.Scripts.Activity.Examples.FloatBubbleIcon floatBubbleIcon)
            {
                if (canvasBounds != null)
                {
                    floatBubbleIcon.SetCanvasBounds(canvasBounds);
                    ActivityLogger.Log("ActivityIconManager", $"FloatBubbleIcon边界检测器已设置: {config.ActivityId}");
                }
                else
                {
                    ActivityLogger.LogWarning("ActivityIconManager", $"边界检测器为null，FloatBubbleIcon可能无法自动隐藏: {config.ActivityId}");
                }
            }

            // 播放显示动画
            icon.Show(animated: true);

            // 如果是FloatBubbleIcon，显示后启动飘动
            if (icon is BlockPuzzleGameToolkit.Scripts.Activity.Examples.FloatBubbleIcon floatIcon)
            {
                floatIcon.StartFloating();
                ActivityLogger.Log("ActivityIconManager", $"FloatBubbleIcon已启动飘动: {config.ActivityId}");
            }

            // 加入活跃列表
            activeIcons[config.ActivityId] = icon;

            ActivityLogger.Log("ActivityIconManager", $"显示角标: {config.ActivityId}");
            return true;
        }

        /// <summary>
        /// 隐藏活动角标
        /// </summary>
        public bool HideIcon(string activityId, bool animated = true)
        {
            if (!isInitialized)
            {
                ActivityLogger.LogError("ActivityIconManager", "HideIcon: 未初始化");
                return false;
            }

            if (string.IsNullOrEmpty(activityId))
            {
                ActivityLogger.LogError("ActivityIconManager", "HideIcon: activityId为空");
                return false;
            }

            // 检查是否存在
            if (!activeIcons.TryGetValue(activityId, out var icon))
            {
                ActivityLogger.LogWarning("ActivityIconManager", $"角标不存在: {activityId}");
                return false;
            }

            // 播放隐藏动画
            icon.Hide(animated, onComplete: () =>
            {
                // 动画完成后回收到对象池
                iconPool.Recycle(icon);
            });

            // 从活跃列表移除
            activeIcons.Remove(activityId);

            ActivityLogger.Log("ActivityIconManager", $"隐藏角标: {activityId}");
            return true;
        }

        /// <summary>
        /// 隐藏所有活动角标
        /// </summary>
        public void HideAllIcons(bool animated = true)
        {
            if (!isInitialized)
            {
                return;
            }

            ActivityLogger.Log("ActivityIconManager", $"隐藏所有角标: {activeIcons.Count}");

            // 复制列表，避免遍历时修改
            var iconsToHide = new List<string>(activeIcons.Keys);

            foreach (var activityId in iconsToHide)
            {
                HideIcon(activityId, animated);
            }
        }

        #endregion

        #region Resource Loading

        /// <summary>
        /// 加载角标Prefab
        /// </summary>
        private GameObject LoadIconPrefab(ActivityConfig config)
        {
            // 检查缓存
            if (prefabCache.TryGetValue(config.ActivityId, out var cachedPrefab))
            {
                if (cachedPrefab != null)
                {
                    return cachedPrefab;
                }
                else
                {
                    // 缓存失效，移除
                    prefabCache.Remove(config.ActivityId);
                }
            }

            // 构建资源路径
            string path = $"Activity/{config.ActivityName}/{config.IconPrefabPath}";

            // 加载Prefab
            GameObject prefab = Resources.Load<GameObject>(path);

            if (prefab == null)
            {
                ActivityLogger.LogError("ActivityIconManager", $"Prefab加载失败: {path}");
                return null;
            }

            // 加入缓存（不再验证ActivityIcon组件，因为是动态添加）
            prefabCache[config.ActivityId] = prefab;

            ActivityLogger.Log("ActivityIconManager", $"加载Prefab: {path}");
            return prefab;
        }

        /// <summary>
        /// 根据ActivityId动态添加对应的Icon组件
        /// </summary>
        private ActivityIcon AddIconComponent(GameObject iconObject, ActivityConfig config)
        {
            if (iconObject == null || config == null)
            {
                return null;
            }

            ActivityIcon icon = null;

            // 根据ActivityName映射到对应的Icon组件类型
            switch (config.ActivityName)
            {
                case "FloatBubble":
                    // 添加FloatBubbleIcon组件
                    icon = iconObject.AddComponent<BlockPuzzleGameToolkit.Scripts.Activity.Examples.FloatBubbleIcon>();
                    ActivityLogger.Log("ActivityIconManager", $"动态添加FloatBubbleIcon组件: {config.ActivityId}");
                    break;

                // 其他活动可以在这里添加
                // case "AnotherActivity":
                //     icon = iconObject.AddComponent<AnotherActivityIcon>();
                //     break;

                default:
                    // 默认使用基础ActivityIcon组件
                    icon = iconObject.AddComponent<ActivityIcon>();
                    ActivityLogger.LogWarning("ActivityIconManager", $"未找到匹配的Icon类型，使用默认ActivityIcon: {config.ActivityName}");
                    break;
            }

            return icon;
        }

        #endregion

        #region Query

        /// <summary>
        /// 检查角标是否已显示
        /// </summary>
        public bool IsIconShowing(string activityId)
        {
            return activeIcons.ContainsKey(activityId);
        }

        /// <summary>
        /// 获取活跃的角标
        /// </summary>
        public ActivityIcon GetActiveIcon(string activityId)
        {
            activeIcons.TryGetValue(activityId, out var icon);
            return icon;
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// 清理所有资源
        /// </summary>
        public void Cleanup()
        {
            if (!isInitialized)
            {
                return;
            }

            ActivityLogger.Log("ActivityIconManager", "Cleanup开始");

            // 隐藏所有角标
            HideAllIcons(animated: false);

            // 清空对象池
            iconPool?.Clear();

            // 清空缓存
            prefabCache.Clear();
            activeIcons.Clear();

            // 清空边界检测器
            canvasBounds = null;

            isInitialized = false;
            ActivityLogger.Log("ActivityIconManager", "Cleanup完成");
        }

        #endregion

        #region Debug

        public string GetDebugInfo()
        {
            return $"ActiveIcons: {activeIcons.Count}, PrefabCache: {prefabCache.Count}, Pool: {iconPool?.GetDebugInfo()}";
        }

        #endregion
    }
}
