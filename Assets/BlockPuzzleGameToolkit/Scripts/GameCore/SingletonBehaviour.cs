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

using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.GameCore
{
    public class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
    {
        private static T _instance;

        /// <summary>
        /// 是否在场景切换时保持单例（默认为false）
        /// 子类可以重写此属性来控制持久化行为
        /// </summary>
        protected virtual bool DontDestroyOnSceneChange => false;

        /// <summary>
        /// 初始化优先级（数值越小越先初始化）
        /// StorageManager = 0, CurrencyManager = 10, RewardCalculator = 20等
        /// </summary>
        public virtual int InitPriority => 100;

        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized { get; protected set; } = false;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();

                    // GameManager 特殊处理：必须使用场景中的实例，不自动创建
                    if (_instance == null && typeof(T) == typeof(GameManager))
                    {
                        Debug.LogWarning($"[SingletonBehaviour] GameManager not found in scene! GameManager must exist in the scene.");
                        return null;
                    }

                    // 如果场景中没有找到实例，自动创建
                    if (_instance == null)
                    {
                        // 查找或创建GameManagers节点
                        GameObject gameManagers = GameObject.Find("GameManagers");
                        if (gameManagers == null)
                        {
                            gameManagers = new GameObject("GameManagers");
                        }

                        // 创建新的GameObject并挂载组件
                        string gameObjectName = typeof(T).Name;
                        GameObject singletonObject = new GameObject(gameObjectName);

                        // 设置为GameManagers的子节点
                        singletonObject.transform.SetParent(gameManagers.transform);

                        // 挂载组件
                        _instance = singletonObject.AddComponent<T>();

                        // 如果需要持久化，设置DontDestroyOnLoad
                        if (_instance.DontDestroyOnSceneChange)
                        {
                            DontDestroyOnLoad(gameManagers);
                        }
                    }
                }

                return _instance;
            }
            private set => _instance = value;
        }

        public virtual void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = (T)this;

                // 如果需要持久化，设置DontDestroyOnLoad
                if (DontDestroyOnSceneChange)
                {
                    // 找到根节点（GameManagers或当前对象的根）
                    Transform root = transform.root;
                    DontDestroyOnLoad(root.gameObject);
                }
            }
        }

        /// <summary>
        /// 单例初始化方法，由SingletonInitializer统一调用
        /// 子类应重写此方法进行初始化逻辑
        /// </summary>
        public virtual void OnInit()
        {
            IsInitialized = true;
        }
    }
}