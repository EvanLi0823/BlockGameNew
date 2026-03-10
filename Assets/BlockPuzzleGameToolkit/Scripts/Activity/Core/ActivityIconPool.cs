// 活动系统 - 角标对象池
// 创建日期: 2026-03-09

using System.Collections.Generic;
using UnityEngine;
using BlockPuzzleGameToolkit.Scripts.Activity.UI;

namespace BlockPuzzleGameToolkit.Scripts.Activity.Core
{
    /// <summary>
    /// 活动角标对象池
    /// 职责: 复用ActivityIcon GameObject，减少创建销毁开销
    /// </summary>
    public class ActivityIconPool
    {
        #region Fields

        private Queue<ActivityIcon> pool = new Queue<ActivityIcon>();
        private HashSet<ActivityIcon> activeIcons = new HashSet<ActivityIcon>();
        private int maxPoolSize = 10;

        #endregion

        #region Properties

        public int PoolSize => pool.Count;
        public int ActiveCount => activeIcons.Count;

        #endregion

        #region Initialization

        /// <summary>
        /// 初始化对象池
        /// </summary>
        public ActivityIconPool(int maxSize = 10)
        {
            maxPoolSize = maxSize;
            ActivityLogger.Log("ActivityIconPool", $"初始化，最大容量: {maxPoolSize}");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 从对象池获取GameObject实例
        /// 如果池为空，则从prefab实例化新的GameObject
        /// 注意：此方法返回GameObject，Icon组件需要外部动态添加
        /// </summary>
        public GameObject GetInstance(GameObject prefab, Transform parent)
        {
            if (prefab == null)
            {
                ActivityLogger.LogError("ActivityIconPool", "GetInstance: prefab为null");
                return null;
            }

            GameObject iconObject = null;

            // 尝试从对象池获取
            if (pool.Count > 0)
            {
                ActivityIcon icon = pool.Dequeue();
                iconObject = icon.gameObject;
                iconObject.SetActive(true);
                iconObject.transform.SetParent(parent, false);

                // 从活跃列表移除旧的引用（会重新添加）
                activeIcons.Remove(icon);

                ActivityLogger.Log("ActivityIconPool", $"从对象池获取，剩余: {pool.Count}");
            }
            else
            {
                // 对象池为空，实例化新GameObject
                iconObject = Object.Instantiate(prefab, parent);
                ActivityLogger.Log("ActivityIconPool", "创建新实例");
            }

            return iconObject;
        }

        /// <summary>
        /// 注册ActivityIcon到活跃列表
        /// 在外部动态添加Icon组件后调用
        /// </summary>
        public void RegisterActive(ActivityIcon icon)
        {
            if (icon != null)
            {
                activeIcons.Add(icon);
            }
        }

        /// <summary>
        /// 回收ActivityIcon到对象池
        /// </summary>
        public void Recycle(ActivityIcon icon)
        {
            if (icon == null)
            {
                ActivityLogger.LogWarning("ActivityIconPool", "Recycle: icon为null");
                return;
            }

            // 从活跃列表移除
            activeIcons.Remove(icon);

            // 检查对象池容量
            if (pool.Count < maxPoolSize)
            {
                // 重置状态
                icon.gameObject.SetActive(false);
                icon.transform.SetParent(null);

                // 加入对象池
                pool.Enqueue(icon);
                ActivityLogger.Log("ActivityIconPool", $"回收到对象池，当前: {pool.Count}");
            }
            else
            {
                // 对象池已满，直接销毁
                Object.Destroy(icon.gameObject);
                ActivityLogger.Log("ActivityIconPool", "对象池已满，销毁实例");
            }
        }

        /// <summary>
        /// 回收所有活跃的ActivityIcon
        /// </summary>
        public void RecycleAll()
        {
            ActivityLogger.Log("ActivityIconPool", $"回收所有活跃实例: {activeIcons.Count}");

            // 复制列表，避免遍历时修改
            var iconsToRecycle = new List<ActivityIcon>(activeIcons);

            foreach (var icon in iconsToRecycle)
            {
                Recycle(icon);
            }

            ActivityLogger.Log("ActivityIconPool", "RecycleAll完成");
        }

        /// <summary>
        /// 清空对象池（销毁所有缓存对象）
        /// </summary>
        public void Clear()
        {
            ActivityLogger.Log("ActivityIconPool", $"清空对象池，销毁: {pool.Count} 个实例");

            while (pool.Count > 0)
            {
                var icon = pool.Dequeue();
                if (icon != null)
                {
                    Object.Destroy(icon.gameObject);
                }
            }

            pool.Clear();
            ActivityLogger.Log("ActivityIconPool", "Clear完成");
        }

        #endregion

        #region Debug

        public string GetDebugInfo()
        {
            return $"Pool: {pool.Count}/{maxPoolSize}, Active: {activeIcons.Count}";
        }

        #endregion
    }
}
