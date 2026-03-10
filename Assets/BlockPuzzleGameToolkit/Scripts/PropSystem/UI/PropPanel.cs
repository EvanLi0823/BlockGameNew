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

using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using BlockPuzzleGameToolkit.Scripts.PropSystem.Core;
using BlockPuzzleGameToolkit.Scripts.Settings;
using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using BlockPuzzleGameToolkit.Scripts.Popups;

namespace BlockPuzzleGameToolkit.Scripts.PropSystem.UI
{
    /// <summary>
    /// 道具面板管理器 - 管理所有道具的UI显示
    /// </summary>
    public class PropPanel : MonoBehaviour
    {
        #region UI引用

        [Header("UI组件")]
        [Tooltip("道具容器")]
        [SerializeField] private Transform propContainer;

        [Tooltip("道具项预制体（可选，如果不设置则从设置中获取）")]
        [SerializeField] private GameObject propItemPrefab;

        [Tooltip("面板Canvas Group（用于整体控制）")]
        [SerializeField] private CanvasGroup panelCanvasGroup;

        #endregion

        #region 配置

        [Header("显示配置")]
        [Tooltip("道具项之间的间距")]
        [SerializeField] private float itemSpacing = 10f;

        [Tooltip("是否自动创建道具项")]
        [SerializeField] private bool autoCreateItems = true;

        [Tooltip("显示的道具类型列表")]
        [SerializeField] private List<PropType> displayPropTypes = new List<PropType>
        {
            PropType.Rotate,
            PropType.Refresh,
            PropType.Bomb
        };

        [Header("动画配置")]
        [Tooltip("面板显示/隐藏动画时长")]
        [Range(0.1f, 1f)]
        [SerializeField] private float panelAnimDuration = 0.3f;

        [Tooltip("道具项出现延迟")]
        [Range(0f, 0.5f)]
        [SerializeField] private float itemAppearDelay = 0.1f;

        #endregion

        #region 私有字段

        /// <summary>
        /// 道具项字典
        /// </summary>
        private Dictionary<PropType, PropItem> propItems;

        /// <summary>
        /// 道具设置
        /// </summary>
        private PropSettings propSettings;

        /// <summary>
        /// 购买设置
        /// </summary>
        private PropPurchaseSettings purchaseSettings;

        /// <summary>
        /// 当前选中的道具
        /// </summary>
        private PropType currentSelectedProp = PropType.None;

        /// <summary>
        /// 面板是否可见
        /// </summary>
        private bool isPanelVisible = true;

        /// <summary>
        /// 购买弹窗引用
        /// </summary>
        private PropPurchase purchasePopup;

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            propItems = new Dictionary<PropType, PropItem>();

            // 获取设置
            propSettings = PropSettings.Instance;
            purchaseSettings = PropPurchaseSettings.Instance;

            // 初始化Canvas Group
            if (panelCanvasGroup == null)
            {
                panelCanvasGroup = GetComponent<CanvasGroup>();
                if (panelCanvasGroup == null)
                {
                    panelCanvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }
        }

        private void Start()
        {
            // 检查道具系统是否启用
            if (propSettings != null && !propSettings.enablePropSystem)
            {
                // 道具系统被禁用，隐藏面板
                gameObject.SetActive(false);
                return;
            }

            if (autoCreateItems)
            {
                InitializeProps();
            }

            SubscribeEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        private void OnEnable()
        {
            // 刷新所有道具显示
            RefreshAllItems();
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 初始化道具面板
        /// </summary>
        public void InitializeProps()
        {
            if (propSettings == null)
            {
                Debug.LogError("PropPanel: PropSettings未找到");
                return;
            }

            // 清除现有道具项
            ClearPropItems();

            // 创建道具项
            CreatePropItems();

            // 移除初始动画，道具直接显示
            // PlayAppearAnimation();
        }

        /// <summary>
        /// 添加道具项
        /// </summary>
        /// <param name="propType">道具类型</param>
        public void AddPropItem(PropType propType)
        {
            if (propItems.ContainsKey(propType))
            {
                Debug.LogWarning($"PropPanel: 道具 {propType} 已存在");
                return;
            }

            var config = propSettings.GetConfig(propType);
            if (config == null)
            {
                Debug.LogError($"PropPanel: 未找到道具 {propType} 的配置");
                return;
            }

            CreatePropItem(config);
        }

        /// <summary>
        /// 移除道具项
        /// </summary>
        /// <param name="propType">道具类型</param>
        public void RemovePropItem(PropType propType)
        {
            if (!propItems.TryGetValue(propType, out PropItem item))
            {
                return;
            }

            propItems.Remove(propType);

            if (item != null)
            {
                // 播放消失动画
                item.transform.DOScale(0, panelAnimDuration)
                    .OnComplete(() => Destroy(item.gameObject));
            }
        }

        /// <summary>
        /// 显示/隐藏面板
        /// </summary>
        /// <param name="visible">是否显示</param>
        /// <param name="animated">是否播放动画</param>
        public void SetPanelVisible(bool visible, bool animated = true)
        {
            isPanelVisible = visible;

            if (animated)
            {
                if (visible)
                {
                    gameObject.SetActive(true);
                    panelCanvasGroup.DOFade(1, panelAnimDuration);
                    transform.DOScale(1, panelAnimDuration).SetEase(Ease.OutBack);
                }
                else
                {
                    panelCanvasGroup.DOFade(0, panelAnimDuration);
                    transform.DOScale(0.8f, panelAnimDuration)
                        .OnComplete(() => gameObject.SetActive(false));
                }
            }
            else
            {
                gameObject.SetActive(visible);
                panelCanvasGroup.alpha = visible ? 1 : 0;
                transform.localScale = visible ? Vector3.one : Vector3.one * 0.8f;
            }
        }

        /// <summary>
        /// 设置道具可交互性
        /// </summary>
        /// <param name="interactable">是否可交互</param>
        public void SetPropsInteractable(bool interactable)
        {
            foreach (var item in propItems.Values)
            {
                if (item != null)
                {
                    item.SetInteractable(interactable);
                }
            }

            // 设置面板整体交互性
            panelCanvasGroup.interactable = interactable;
            panelCanvasGroup.blocksRaycasts = interactable;
        }

        /// <summary>
        /// 刷新所有道具显示
        /// </summary>
        public void RefreshAllItems()
        {
            var propManager = PropManager.Instance;
            if (propManager == null) return;

            foreach (var kvp in propItems)
            {
                if (kvp.Value != null)
                {
                    int count = propManager.GetPropCount(kvp.Key);
                    kvp.Value.UpdateCount(count);
                }
            }
        }

        /// <summary>
        /// 获取道具项
        /// </summary>
        /// <param name="propType">道具类型</param>
        /// <returns>道具项组件</returns>
        public PropItem GetPropItem(PropType propType)
        {
            propItems.TryGetValue(propType, out PropItem item);
            return item;
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 创建道具项
        /// </summary>
        private void CreatePropItems()
        {
            if (propItemPrefab == null)
            {
                Debug.LogError("PropPanel: 道具项预制体未设置");
                return;
            }

            // 根据配置创建道具项
            foreach (var propType in displayPropTypes)
            {
                if (propType == PropType.None) continue;

                var config = propSettings.GetConfig(propType);
                if (config != null)
                {
                    CreatePropItem(config);
                }
            }
        }

        /// <summary>
        /// 创建单个道具项
        /// </summary>
        private PropItem CreatePropItem(PropItemConfig config)
        {
            // 实例化道具项
            var itemGO = Instantiate(propItemPrefab, propContainer);
            var propItem = itemGO.GetComponent<PropItem>();

            if (propItem == null)
            {
                propItem = itemGO.AddComponent<PropItem>();
            }

            // 初始化道具项
            propItem.Initialize(config);

            // 绑定事件
            propItem.OnPropClicked += HandlePropClick;
            propItem.OnPropLongPressed += HandlePropLongPress;

            // 添加到字典
            propItems[config.propType] = propItem;

            // 更新数量显示
            UpdatePropCount(config.propType);

            return propItem;
        }

        /// <summary>
        /// 清除所有道具项
        /// </summary>
        private void ClearPropItems()
        {
            foreach (var item in propItems.Values)
            {
                if (item != null)
                {
                    // 取消事件绑定
                    item.OnPropClicked -= HandlePropClick;
                    item.OnPropLongPressed -= HandlePropLongPress;

                    Destroy(item.gameObject);
                }
            }

            propItems.Clear();
        }

        /// <summary>
        /// 处理道具点击
        /// </summary>
        private void HandlePropClick(PropType type)
        {
            var propManager = PropManager.Instance;
            if (propManager == null) return;

            int count = propManager.GetPropCount(type);

            if (count > 0)
            {
                // 有道具，使用
                bool success = propManager.UseProp(type);
                if (success)
                {
                    // 更新选中状态
                    UpdateSelection(type);

                    // 移除使用动画，保持道具原样
                    // if (propItems.TryGetValue(type, out PropItem item))
                    // {
                    //     item.PlayUseAnimation();
                    // }
                }
            }
            else
            {
                // 无道具，显示购买弹窗
                ShowPurchasePopup(type);
            }
        }

        /// <summary>
        /// 处理道具长按
        /// </summary>
        private void HandlePropLongPress(PropType type)
        {
            // 显示道具信息（可以创建一个提示框）
            ShowPropInfo(type);
        }

        /// <summary>
        /// 显示购买弹窗
        /// </summary>
        private void ShowPurchasePopup(PropType type)
        {
            // 尝试获取或创建购买弹窗
            if (purchasePopup == null)
            {
                // 从设置中获取预制体
                var popupPrefab = purchaseSettings?.propPurchasePopupPrefab;
                if (popupPrefab != null)
                {
                    var popupGO = Instantiate(popupPrefab);
                    purchasePopup = popupGO.GetComponent<PropPurchase>();
                }
            }

            if (purchasePopup != null)
            {
                purchasePopup.Initialize(type);
                purchasePopup.Show();
            }
            else
            {
                // 使用MenuManager显示弹窗
                var menuManager = MenuManager.Instance;
                if (menuManager != null)
                {
                    var popup = menuManager.ShowPopup<PropPurchase>();
                    if (popup != null)
                    {
                        popup.Initialize(type);
                    }
                }
                else
                {
                    Debug.LogWarning($"PropPanel: 无法显示购买弹窗 - {type}");
                }
            }
        }

        /// <summary>
        /// 显示道具信息
        /// </summary>
        private void ShowPropInfo(PropType type)
        {
            var config = propSettings.GetConfig(type);
            if (config != null)
            {
                Debug.Log($"道具：{config.propName}\n描述：{config.description}");
                // TODO: 可以创建一个信息提示框UI
            }
        }

        /// <summary>
        /// 更新道具数量
        /// </summary>
        private void UpdatePropCount(PropType type)
        {
            if (!propItems.TryGetValue(type, out PropItem item)) return;

            var propManager = PropManager.Instance;
            if (propManager != null)
            {
                int count = propManager.GetPropCount(type);
                item.UpdateCount(count);
            }
        }

        /// <summary>
        /// 更新选中状态
        /// </summary>
        private void UpdateSelection(PropType selectedType)
        {
            // 取消之前的选中
            if (currentSelectedProp != PropType.None)
            {
                if (propItems.TryGetValue(currentSelectedProp, out PropItem oldItem))
                {
                    oldItem.SetSelected(false);
                }
            }

            // 设置新的选中
            currentSelectedProp = selectedType;
            if (selectedType != PropType.None)
            {
                if (propItems.TryGetValue(selectedType, out PropItem newItem))
                {
                    newItem.SetSelected(true);
                }
            }
        }

        /// <summary>
        /// 播放出现动画（已禁用）
        /// </summary>
        // private void PlayAppearAnimation()
        // {
        //     int index = 0;
        //     foreach (var item in propItems.Values)
        //     {
        //         if (item != null)
        //         {
        //             item.transform.localScale = Vector3.zero;
        //             item.transform.DOScale(1, panelAnimDuration)
        //                 .SetDelay(index * itemAppearDelay)
        //                 .SetEase(Ease.OutBack);
        //             index++;
        //         }
        //     }
        // }

        #endregion

        #region 事件订阅

        /// <summary>
        /// 订阅事件
        /// </summary>
        private void SubscribeEvents()
        {
            // 订阅道具事件
            PropManager.OnPropCountChanged += OnPropCountChanged;
            PropManager.OnPropSelectionStart += OnPropSelectionStart;
            PropManager.OnPropSelectionEnd += OnPropSelectionEnd;

            // 订阅游戏状态事件
            EventManager.OnGameStateChanged += OnGameStateChanged;
        }

        /// <summary>
        /// 取消事件订阅
        /// </summary>
        private void UnsubscribeEvents()
        {
            PropManager.OnPropCountChanged -= OnPropCountChanged;
            PropManager.OnPropSelectionStart -= OnPropSelectionStart;
            PropManager.OnPropSelectionEnd -= OnPropSelectionEnd;
            EventManager.OnGameStateChanged -= OnGameStateChanged;
        }

        /// <summary>
        /// 道具数量变化处理
        /// </summary>
        private void OnPropCountChanged(PropType type, int count)
        {
            if (propItems.TryGetValue(type, out PropItem item))
            {
                item.UpdateCount(count);
            }
        }

        /// <summary>
        /// 道具选择开始处理
        /// </summary>
        private void OnPropSelectionStart(PropType type)
        {
            UpdateSelection(type);
        }

        /// <summary>
        /// 道具选择结束处理
        /// </summary>
        private void OnPropSelectionEnd(PropType type)
        {
            UpdateSelection(PropType.None);
        }

        /// <summary>
        /// 游戏状态变化处理
        /// </summary>
        private void OnGameStateChanged(EGameState newState)
        {
            // 根据游戏状态设置可交互性
            bool interactable = (newState == EGameState.Playing);
            SetPropsInteractable(interactable);
        }

        #endregion
    }
}