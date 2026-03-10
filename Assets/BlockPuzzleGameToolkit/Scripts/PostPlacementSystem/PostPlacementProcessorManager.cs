// // ©2015 - 2025 Candy Smith
// // All rights reserved
// // Redistribution of this software is strictly not allowed.
// // Copy of this software can be obtained from unity asset store only.
// // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// // IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// // FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// // AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// // LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// // OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// // THE SOFTWARE.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BlockPuzzleGameToolkit.Scripts.Gameplay;

namespace BlockPuzzleGameToolkit.Scripts.PostPlacementSystem
{
    /// <summary>
    /// 放置后处理器管理器
    /// 负责注册、管理和执行所有放置后处理器
    /// 场景级别单例，随游戏场景生命周期管理
    ///
    /// 设计说明：
    /// - 不继承SingletonBehaviour，因为不需要跨场景持久化
    /// - 处理器持有LevelManager等场景组件的引用，场景切换时应一并销毁
    /// - 每次进入游戏场景重新创建和注册处理器
    /// </summary>
    public class PostPlacementProcessorManager : MonoBehaviour
    {
        // ========== 场景级别单例 ==========
        private static PostPlacementProcessorManager instance;

        /// <summary>
        /// 获取当前场景的单例实例
        /// 注意：此单例不跨场景持久化，场景切换后需要重新创建
        /// </summary>
        public static PostPlacementProcessorManager Instance
        {
            get
            {
                if (instance == null)
                {
                    // 场景中查找
                    instance = FindObjectOfType<PostPlacementProcessorManager>();

                    // 如果场景中没有，创建一个新的（作为LevelManager的子对象）
                    if (instance == null)
                    {
                        var levelManager = FindObjectOfType<LevelManager>();
                        if (levelManager != null)
                        {
                            var go = new GameObject("PostPlacementProcessorManager");
                            go.transform.SetParent(levelManager.transform);
                            instance = go.AddComponent<PostPlacementProcessorManager>();
                        }
                        else
                        {
                            Debug.LogWarning("[PostPlacementProcessorManager] LevelManager未找到，无法创建PostPlacementProcessorManager");
                        }
                    }
                }
                return instance;
            }
        }

        // ========== 处理器列表 ==========
        /// <summary>
        /// 已注册的处理器列表（按优先级排序）
        /// </summary>
        private readonly List<IPostPlacementProcessor> processors = new List<IPostPlacementProcessor>();

        /// <summary>
        /// 是否正在处理中（防止并发）
        /// </summary>
        private bool isProcessing = false;

        // ========== Unity生命周期 ==========
        private void Awake()
        {
            // 确保同一场景只有一个实例
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            Debug.Log("[PostPlacementProcessorManager] 场景级别单例创建完成");
        }

        private void OnDestroy()
        {
            // 场景切换时清理单例引用
            if (instance == this)
            {
                instance = null;
                Debug.Log("[PostPlacementProcessorManager] 场景级别单例销毁");
            }
        }

        // ========== 处理器注册 ==========
        /// <summary>
        /// 注册处理器
        /// </summary>
        public void RegisterProcessor(IPostPlacementProcessor processor)
        {
            if (processor == null)
            {
                Debug.LogWarning("[PostPlacementProcessorManager] 尝试注册null处理器");
                return;
            }

            if (processors.Contains(processor))
            {
                Debug.LogWarning($"[PostPlacementProcessorManager] 处理器已存在: {processor.GetType().Name}");
                return;
            }

            processors.Add(processor);

            // 按优先级排序（数值越小越先执行）
            processors.Sort((a, b) => a.Priority.CompareTo(b.Priority));

            Debug.Log($"[PostPlacementProcessorManager] 注册处理器: {processor.GetType().Name}, 优先级: {processor.Priority}");
        }

        /// <summary>
        /// 注销处理器
        /// </summary>
        public void UnregisterProcessor(IPostPlacementProcessor processor)
        {
            if (processor == null)
            {
                return;
            }

            if (processors.Remove(processor))
            {
                Debug.Log($"[PostPlacementProcessorManager] 注销处理器: {processor.GetType().Name}");
            }
        }

        /// <summary>
        /// 清空所有处理器
        /// </summary>
        public void ClearProcessors()
        {
            processors.Clear();
            Debug.Log("[PostPlacementProcessorManager] 清空所有处理器");
        }

        // ========== 处理执行 ==========
        /// <summary>
        /// 执行所有处理器（协程）
        /// </summary>
        /// <param name="context">上下文对象</param>
        /// <returns>协程枚举器</returns>
        public IEnumerator ProcessAll(PostPlacementContext context)
        {
            // 并发保护
            if (isProcessing)
            {
                Debug.LogWarning("[PostPlacementProcessorManager] 正在处理中，跳过本次调用");
                yield break;
            }

            isProcessing = true;

            try
            {
                Debug.Log($"[PostPlacementProcessorManager] 开始处理，处理器数量: {processors.Count}");

                // 按优先级依次执行处理器
                foreach (var processor in processors)
                {
                    if (processor == null)
                    {
                        Debug.LogWarning("[PostPlacementProcessorManager] 遇到null处理器，跳过");
                        continue;
                    }

                    // 检查是否可以执行
                    if (!processor.CanProcess(context))
                    {
                        Debug.Log($"[PostPlacementProcessorManager] 处理器跳过: {processor.GetType().Name}");
                        continue;
                    }

                    // 执行处理器
                    Debug.Log($"[PostPlacementProcessorManager] 执行处理器: {processor.GetType().Name}");
                    yield return processor.Process(context);
                }

                Debug.Log($"[PostPlacementProcessorManager] 处理完成，消除行数: {context.EliminatedLines}, 总得分: {context.TotalScore}");
            }
            finally
            {
                isProcessing = false;
            }
        }

        // ========== 查询方法 ==========
        /// <summary>
        /// 获取已注册的处理器数量
        /// </summary>
        public int GetProcessorCount()
        {
            return processors.Count;
        }

        /// <summary>
        /// 获取所有处理器（只读）
        /// </summary>
        public IReadOnlyList<IPostPlacementProcessor> GetProcessors()
        {
            return processors.AsReadOnly();
        }

        /// <summary>
        /// 检查是否正在处理
        /// </summary>
        public bool IsProcessing()
        {
            return isProcessing;
        }
    }
}
