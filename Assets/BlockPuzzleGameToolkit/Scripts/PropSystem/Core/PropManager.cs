// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using UnityEngine;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using BlockPuzzleGameToolkit.Scripts.Gameplay;
using BlockPuzzleGameToolkit.Scripts.PropSystem.Behaviors;
using BlockPuzzleGameToolkit.Scripts.Settings;
using BlockPuzzleGameToolkit.Scripts.Enums;
using StorageSystem.Core;

namespace BlockPuzzleGameToolkit.Scripts.PropSystem.Core
{
    /// <summary>
    /// 道具管理器 - 管理所有道具的使用、购买和存储
    /// </summary>
    public class PropManager : SingletonBehaviour<PropManager>
    {
        #region Singleton设置

        /// <summary>
        /// 道具管理器需要在场景切换时保持，设置为true
        /// </summary>
        protected override bool DontDestroyOnSceneChange => true;

        #endregion

        #region 字段和属性

        [Header("管理器引用")]
        [Tooltip("格子管理器引用")]
        [SerializeField] private FieldManager fieldManager;

        [Tooltip("方块组管理器引用")]
        [SerializeField] private CellDeckManager cellDeckManager;

        [Tooltip("高亮管理器引用")]
        [SerializeField] private HighlightManager highlightManager;

        /// <summary>
        /// 道具存储数据
        /// </summary>
        private PropSaveData saveData;

        /// <summary>
        /// 道具设置
        /// </summary>
        private PropSettings propSettings;

        /// <summary>
        /// 道具购买设置
        /// </summary>
        private PropPurchaseSettings purchaseSettings;

        /// <summary>
        /// 道具行为映射
        /// </summary>
        private Dictionary<PropType, IPropBehavior> propBehaviors;

        /// <summary>
        /// 是否正在选择道具目标
        /// </summary>
        private bool isSelectingProp = false;

        /// <summary>
        /// 当前选择的道具类型
        /// </summary>
        private PropType currentSelectingProp = PropType.None;

        /// <summary>
        /// 当前使用的道具行为
        /// </summary>
        private IPropBehavior currentBehavior;

        /// <summary>
        /// 存储键值
        /// </summary>
        private const string SAVE_KEY = "PropData";

        /// <summary>
        /// 是否正在选择道具（供外部查询）
        /// </summary>
        public bool IsSelectingProp => isSelectingProp;

        /// <summary>
        /// 当前选择的道具类型（供外部查询）
        /// </summary>
        public PropType CurrentSelectingProp => currentSelectingProp;

        /// <summary>
        /// 格子管理器（供道具行为使用）
        /// </summary>
        public FieldManager FieldManager => fieldManager;

        /// <summary>
        /// 方块组管理器（供道具行为使用）
        /// </summary>
        public CellDeckManager CellDeckManager => cellDeckManager;

        /// <summary>
        /// 高亮管理器（供道具行为使用）
        /// </summary>
        public HighlightManager HighlightManager => highlightManager;

        #endregion

        #region 事件

        /// <summary>
        /// 道具数量变化事件
        /// </summary>
        public static event Action<PropType, int> OnPropCountChanged;

        /// <summary>
        /// 道具使用事件
        /// </summary>
        public static event Action<PropType> OnPropUsed;

        /// <summary>
        /// 道具购买事件
        /// </summary>
        public static event Action<PropType, int> OnPropPurchased;

        /// <summary>
        /// 道具选择开始事件
        /// </summary>
        public static event Action<PropType> OnPropSelectionStart;

        /// <summary>
        /// 道具选择结束事件
        /// </summary>
        public static event Action<PropType> OnPropSelectionEnd;

        #endregion

        #region Unity生命周期

        // 初始化优先级
        public override int InitPriority => 30;

        /// <summary>
        /// 初始化
        /// </summary>
        public override void Awake()
        {
            // 如果 Instance 已存在且不是自己，说明有重复，直接返回不执行初始化
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            base.Awake();

            // 加载配置
            LoadSettings();

            // 检查道具系统是否启用
            if (propSettings != null && !propSettings.enablePropSystem)
            {
                Debug.Log("PropManager: 道具系统已被禁用");
                enabled = false;  // 禁用组件
                return;
            }

            // 初始化行为系统
            InitializeBehaviors();

            // 订阅游戏事件
            SubscribeToEvents();
        }

        /// <summary>
        /// 单例初始化入口，由SingletonInitializer调用
        /// </summary>
        public override void OnInit()
        {
            if (propSettings != null && propSettings.enablePropSystem)
            {
                LoadSaveData();
            }
            base.OnInit(); // 设置SingletonBehaviour的IsInitialized = true
        }

        private void Start()
        {
            // 备用初始化，如果SingletonInitializer没有调用
            if (propSettings != null && propSettings.enablePropSystem && !IsInitialized)
            {
                LoadSaveData();
                IsInitialized = true;
            }
        }

        /// <summary>
        /// 销毁时清理
        /// </summary>
        private void OnDestroy()
        {
            // 在应用退出时不要保存，避免访问已销毁的单例
            if (!isApplicationQuitting)
            {
                SaveData(); // 保存数据
            }

            // 清理行为
            CleanupBehaviors();

            // 取消事件订阅
            UnsubscribeFromEvents();
        }

        private bool isApplicationQuitting = false;

        private void OnApplicationQuit()
        {
            isApplicationQuitting = true;
            // 应用退出时最后保存一次
            SaveDataSafely();
        }

        /// <summary>
        /// 安全保存数据（不创建新的单例实例）
        /// </summary>
        private void SaveDataSafely()
        {
            if (saveData == null) return;

            // 先检查StorageManager实例是否存在
            var storageManager = FindObjectOfType<StorageManager>();
            if (storageManager != null)
            {
                try
                {
                    // 明确指定使用Binary存储类型
                    storageManager.Save(SAVE_KEY, saveData, StorageType.Binary);
                    Debug.Log("[PropManager] 应用退出时数据保存成功");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[PropManager] 应用退出时保存数据异常: {e.Message}");
                }
            }
        }

        /// <summary>
        /// 应用暂停时保存
        /// </summary>
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveData();
            }
        }

        /// <summary>
        /// 失去焦点时保存
        /// </summary>
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                SaveData();
            }
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 加载设置
        /// </summary>
        private void LoadSettings()
        {
            propSettings = PropSettings.Instance;
            purchaseSettings = PropPurchaseSettings.Instance;

            if (propSettings == null)
            {
                Debug.LogError("PropManager: PropSettings未找到");
            }

            if (purchaseSettings == null)
            {
                Debug.LogError("PropManager: PropPurchaseSettings未找到");
            }
        }

        /// <summary>
        /// 初始化行为系统
        /// </summary>
        private void InitializeBehaviors()
        {
            propBehaviors = new Dictionary<PropType, IPropBehavior>
            {
                { PropType.Rotate, new RotatePropBehavior() },
                { PropType.Refresh, new RefreshPropBehavior() },
                { PropType.Bomb, new BombPropBehavior() }
            };

            // 初始化每个行为，传入PropManager引用
            foreach (var behavior in propBehaviors.Values)
            {
                behavior.Initialize(this);
            }
        }

        /// <summary>
        /// 清理行为系统
        /// </summary>
        private void CleanupBehaviors()
        {
            if (propBehaviors != null)
            {
                foreach (var behavior in propBehaviors.Values)
                {
                    behavior.Cleanup();
                }
                propBehaviors.Clear();
            }
        }

        #endregion

        #region 数据存储

        /// <summary>
        /// 加载存储数据
        /// </summary>
        private void LoadSaveData()
        {
            // 从StorageManager加载数据
            var storage = StorageManager.Instance;
            if (storage != null)
            {
                // 明确指定使用Binary存储类型
                saveData = storage.Load<PropSaveData>(SAVE_KEY, StorageType.Binary);
            }

            if (saveData == null)
            {
                // 创建新数据，使用初始配置
                saveData = new PropSaveData();

                if (propSettings != null && propSettings.initialProps != null)
                {
                    saveData.Initialize(propSettings.initialProps);
                }

                SaveData();
            }

            // 触发初始数量更新事件
            foreach (PropType type in Enum.GetValues(typeof(PropType)))
            {
                if (type != PropType.None)
                {
                    int count = saveData.GetPropCount(type);
                    // 总是触发事件，即使数量为0，以确保UI正确显示
                    OnPropCountChanged?.Invoke(type, count);
                }
            }
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        private void SaveData()
        {
            if (saveData == null) return;

            var storage = StorageManager.Instance;
            if (storage != null)
            {
                // 明确指定使用Binary存储类型
                storage.Save(SAVE_KEY, saveData, StorageType.Binary);
            }
        }

        #endregion

        #region 道具使用

        /// <summary>
        /// 使用道具
        /// </summary>
        /// <param name="type">道具类型</param>
        /// <returns>是否成功开始使用</returns>
        public bool UseProp(PropType type)
        {
            // 检查道具系统是否启用
            if (propSettings != null && !propSettings.enablePropSystem)
            {
                Debug.Log("PropManager: 道具系统已被禁用");
                return false;
            }

            // 检查游戏状态
            if (EventManager.GameStatus != EGameState.Playing)
            {
                Debug.Log($"PropManager: 游戏状态不允许使用道具 - {EventManager.GameStatus}");
                return false;
            }

            // 检查道具数量
            if (GetPropCount(type) <= 0)
            {
                Debug.Log($"PropManager: 道具 {type} 数量不足");
                return false;
            }

            // 检查是否正在选择其他道具
            if (isSelectingProp)
            {
                Debug.Log($"PropManager: 正在使用道具 {currentSelectingProp}");
                return false;
            }

            // 获取道具行为
            if (!propBehaviors.TryGetValue(type, out IPropBehavior behavior))
            {
                Debug.LogError($"PropManager: 道具 {type} 没有对应的行为");
                return false;
            }

            // 设置当前状态
            isSelectingProp = true;
            currentSelectingProp = type;
            currentBehavior = behavior;

            // 触发选择开始事件
            OnPropSelectionStart?.Invoke(type);

            // 如果道具不需要选择目标，直接执行
            if (!behavior.RequiresTarget)
            {
                ConfirmPropUse(null);
            }
            else
            {
                // 开始选择模式
                behavior.StartSelection();
            }

            return true;
        }

        /// <summary>
        /// 确认使用道具
        /// </summary>
        /// <param name="target">目标对象</param>
        public void ConfirmPropUse(object target)
        {
            if (!isSelectingProp || currentBehavior == null) return;

            // 如果需要目标，检查目标是否有效
            if (currentBehavior.RequiresTarget && !currentBehavior.CanExecute(target))
            {
                Debug.Log($"PropManager: 目标无效，无法使用道具 {currentSelectingProp}");
                return;
            }

            // 扣除道具
            AddProp(currentSelectingProp, -1);

            // 执行道具效果
            currentBehavior.Execute(target);

            // 触发使用事件
            OnPropUsed?.Invoke(currentSelectingProp);

            // 使用EventManager触发事件
            EventManager.GetEvent(EGameEvent.OnPropUsed).Invoke();

            // 保存数据
            SaveData();

            // 重置状态
            EndPropSelection();
        }

        /// <summary>
        /// 取消道具选择
        /// </summary>
        public void CancelPropSelection()
        {
            if (!isSelectingProp) return;

            if (currentBehavior != null)
            {
                currentBehavior.CancelSelection();
            }

            EndPropSelection();
        }

        /// <summary>
        /// 结束道具选择
        /// </summary>
        private void EndPropSelection()
        {
            var previousProp = currentSelectingProp;

            isSelectingProp = false;
            currentSelectingProp = PropType.None;
            currentBehavior = null;

            // 触发选择结束事件
            OnPropSelectionEnd?.Invoke(previousProp);
        }

        /// <summary>
        /// 预览道具效果
        /// </summary>
        /// <param name="target">预览目标</param>
        public void PreviewPropEffect(object target)
        {
            if (!isSelectingProp || currentBehavior == null) return;

            currentBehavior.ShowPreview(target);
        }

        /// <summary>
        /// 隐藏道具预览
        /// </summary>
        public void HidePropPreview()
        {
            if (!isSelectingProp || currentBehavior == null) return;

            currentBehavior.HidePreview();
        }

        #endregion

        #region 道具购买

        /// <summary>
        /// 购买道具（通过广告）
        /// </summary>
        /// <param name="type">道具类型</param>
        public void PurchasePropWithAds(PropType type)
        {
            var config = purchaseSettings?.GetPurchaseConfig(type);
            if (config == null)
            {
                Debug.LogError($"PropManager: 道具 {type} 没有购买配置");
                return;
            }

            if (!config.canPurchaseWithAds)
            {
                Debug.Log($"PropManager: 道具 {type} 不能通过广告购买");
                return;
            }

            // 检查每日限制
            if (config.dailyAdLimit > 0)
            {
                int todayCount = saveData.GetTodayAdPurchaseCount(type);
                if (todayCount >= config.dailyAdLimit)
                {
                    Debug.Log($"PropManager: 道具 {type} 今日广告购买已达上限");
                    return;
                }
            }

            // 添加道具
            AddProp(type, config.adsRewardAmount);

            // 记录购买
            saveData.RecordAdPurchase(type);

            // 触发购买事件
            OnPropPurchased?.Invoke(type, config.adsRewardAmount);

            // 保存数据
            SaveData();
        }

        /// <summary>
        /// 购买道具（通过金币）
        /// </summary>
        /// <param name="type">道具类型</param>
        /// <returns>是否购买成功</returns>
        public bool PurchasePropWithCoins(PropType type)
        {
            var config = purchaseSettings?.GetPurchaseConfig(type);
            if (config == null)
            {
                Debug.LogError($"PropManager: 道具 {type} 没有购买配置");
                return false;
            }

            if (!config.canPurchaseWithCoins)
            {
                Debug.Log($"PropManager: 道具 {type} 不能通过金币购买");
                return false;
            }

            // 检查每日限制
            if (config.dailyCoinLimit > 0)
            {
                int todayCount = saveData.GetTodayCoinPurchaseCount(type);
                if (todayCount >= config.dailyCoinLimit)
                {
                    Debug.Log($"PropManager: 道具 {type} 今日金币购买已达上限");
                    return false;
                }
            }

            // 这里应该检查金币是否足够（需要CurrencyManager）
            // 暂时跳过金币检查

            // 添加道具
            AddProp(type, config.coinPurchaseAmount);

            // 记录购买
            saveData.RecordCoinPurchase(type);

            // 触发购买事件
            OnPropPurchased?.Invoke(type, config.coinPurchaseAmount);

            // 保存数据
            SaveData();

            return true;
        }

        #endregion

        #region 道具数据管理

        /// <summary>
        /// 获取道具数量
        /// </summary>
        /// <param name="type">道具类型</param>
        /// <returns>道具数量</returns>
        public int GetPropCount(PropType type)
        {
            // 如果道具系统被禁用，返回0
            if (propSettings != null && !propSettings.enablePropSystem)
            {
                return 0;
            }

            return saveData?.GetPropCount(type) ?? 0;
        }

        /// <summary>
        /// 增加道具数量
        /// </summary>
        /// <param name="type">道具类型</param>
        /// <param name="amount">增加的数量（可以为负数）</param>
        /// <returns>操作后的数量</returns>
        public int AddProp(PropType type, int amount)
        {
            if (saveData == null) return 0;

            int newCount = saveData.AddProp(type, amount);

            // 触发数量变化事件
            OnPropCountChanged?.Invoke(type, newCount);

            return newCount;
        }

        /// <summary>
        /// 设置道具数量
        /// </summary>
        /// <param name="type">道具类型</param>
        /// <param name="count">道具数量</param>
        public void SetPropCount(PropType type, int count)
        {
            if (saveData == null) return;

            saveData.SetPropCount(type, count);

            // 触发数量变化事件
            OnPropCountChanged?.Invoke(type, count);
        }

        /// <summary>
        /// 重置所有道具到初始状态
        /// </summary>
        public void ResetAllProps()
        {
            if (propSettings != null && propSettings.initialProps != null)
            {
                saveData = new PropSaveData();
                saveData.Initialize(propSettings.initialProps);
                SaveData();

                // 触发所有道具的数量变化事件
                foreach (var prop in propSettings.initialProps)
                {
                    OnPropCountChanged?.Invoke(prop.propType, prop.propNum);
                }
            }
        }

        /// <summary>
        /// 获取购买配置
        /// </summary>
        /// <param name="type">道具类型</param>
        /// <returns>购买配置</returns>
        public PropPurchaseSettings.PropPurchaseConfig GetPurchaseConfig(PropType type)
        {
            return purchaseSettings?.GetPurchaseConfig(type);
        }

        /// <summary>
        /// 获取道具配置
        /// </summary>
        /// <param name="type">道具类型</param>
        /// <returns>道具配置</returns>
        public PropItemConfig GetPropConfig(PropType type)
        {
            return propSettings?.GetConfig(type);
        }

        #endregion

        #region 事件订阅

        /// <summary>
        /// 订阅游戏事件
        /// </summary>
        private void SubscribeToEvents()
        {
            // 订阅游戏状态变化
            EventManager.GetEvent<EGameState>(EGameEvent.GameStateChanged).Subscribe(OnGameStateChanged);
        }

        /// <summary>
        /// 取消事件订阅
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            EventManager.GetEvent<EGameState>(EGameEvent.GameStateChanged).Unsubscribe(OnGameStateChanged);
        }

        /// <summary>
        /// 游戏状态变化处理
        /// </summary>
        private void OnGameStateChanged(EGameState newState)
        {
            // 如果游戏不在进行中，取消道具选择
            if (newState != EGameState.Playing && isSelectingProp)
            {
                CancelPropSelection();
            }
        }

        #endregion
    }
}