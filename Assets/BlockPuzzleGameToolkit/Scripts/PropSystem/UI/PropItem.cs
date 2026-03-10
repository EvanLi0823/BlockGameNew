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
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using BlockPuzzleGameToolkit.Scripts.PropSystem.Core;
using BlockPuzzleGameToolkit.Scripts.Settings;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using BlockPuzzleGameToolkit.Scripts.Popups;

namespace BlockPuzzleGameToolkit.Scripts.PropSystem.UI
{
    /// <summary>
    /// 道具项UI组件 - 显示单个道具的图标、数量和交互
    /// </summary>
    public class PropItem : MonoBehaviour
    {
        #region UI引用

        [Header("UI组件")]
        [Tooltip("道具图标")]
        [SerializeField] private Image propIcon;

        [Tooltip("道具数量文本")]
        [SerializeField] private TextMeshProUGUI countText;

        [Tooltip("加号图标（数量为0时显示）")]
        [SerializeField] private GameObject addIcon;

        [Tooltip("点击按钮")]
        [SerializeField] private Button clickButton;

        #endregion

        #region 私有字段

        /// <summary>
        /// 道具类型
        /// </summary>
        private PropType propType;

        /// <summary>
        /// 道具配置
        /// </summary>
        private PropItemConfig config;

        /// <summary>
        /// 当前数量
        /// </summary>
        private int currentCount;

        /// <summary>
        /// 是否可交互
        /// </summary>
        private bool isInteractable = true;

        /// <summary>
        /// 缩放动画序列
        /// </summary>
        private Sequence scaleSequence;

        #endregion

        #region 事件

        /// <summary>
        /// 道具点击事件
        /// </summary>
        public event Action<PropType> OnPropClicked;

        /// <summary>
        /// 道具长按事件（显示信息）
        /// </summary>
        public event Action<PropType> OnPropLongPressed;

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            // 绑定按钮事件
            if (clickButton != null)
            {
                clickButton.onClick.AddListener(OnButtonClick);
            }
            else
            {
                // 如果没有指定按钮，尝试获取组件上的按钮
                clickButton = GetComponent<Button>();
                if (clickButton != null)
                {
                    clickButton.onClick.AddListener(OnButtonClick);
                }
            }

            // 初始化UI状态
            if (addIcon != null) addIcon.SetActive(false);
            if (countText != null) countText.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            // 清理事件
            if (clickButton != null)
            {
                clickButton.onClick.RemoveListener(OnButtonClick);
            }

            // 停止动画
            scaleSequence?.Kill();

            // 取消事件订阅
            PropManager.OnPropCountChanged -= OnPropCountChanged;
        }

        private void OnEnable()
        {
            // 订阅道具数量变化事件
            PropManager.OnPropCountChanged += OnPropCountChanged;

            // 刷新显示
            if (config != null)
            {
                UpdateCount(PropManager.Instance.GetPropCount(propType));
            }
        }

        private void OnDisable()
        {
            // 取消订阅
            PropManager.OnPropCountChanged -= OnPropCountChanged;
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 初始化道具项
        /// </summary>
        /// <param name="itemConfig">道具配置</param>
        public void Initialize(PropItemConfig itemConfig)
        {
            if (itemConfig == null)
            {
                Debug.LogError("PropItem: 配置为空");
                return;
            }

            config = itemConfig;
            propType = config.propType;

            // 设置图标
            if (propIcon != null && config.propIcon != null)
            {
                propIcon.sprite = config.propIcon;
            }

            // 获取并显示当前数量
            UpdateCount(PropManager.Instance.GetPropCount(propType));
        }

        /// <summary>
        /// 通过道具类型初始化
        /// </summary>
        /// <param name="type">道具类型</param>
        public void Initialize(PropType type)
        {
            var settings = PropSettings.Instance;
            if (settings != null)
            {
                var itemConfig = settings.GetConfig(type);
                if (itemConfig != null)
                {
                    Initialize(itemConfig);
                }
                else
                {
                    Debug.LogError($"PropItem: 未找到道具 {type} 的配置");
                }
            }
        }

        /// <summary>
        /// 更新数量显示
        /// </summary>
        /// <param name="count">道具数量</param>
        public void UpdateCount(int count)
        {
            currentCount = count;

            if (count > 0)
            {
                // 有道具：显示数量，隐藏加号
                if (countText != null)
                {
                    countText.gameObject.SetActive(true);
                    // 超过99显示99+
                    countText.text = count > 99 ? "99+" : count.ToString();
                }

                if (addIcon != null)
                {
                    addIcon.SetActive(false);
                }

                // 恢复图标透明度
                if (propIcon != null)
                {
                    SetIconAlpha(1f);
                }
            }
            else
            {
                // 无道具：显示加号，隐藏数量
                if (countText != null)
                {
                    countText.gameObject.SetActive(false);
                }

                if (addIcon != null)
                {
                    addIcon.SetActive(true);
                }

                // 图标保持原样，不改变透明度
            }

            // 移除更新动画，直接显示
            // PlayUpdateAnimation();
        }

        /// <summary>
        /// 设置是否可交互
        /// </summary>
        /// <param name="interactable">是否可交互</param>
        public void SetInteractable(bool interactable)
        {
            isInteractable = interactable;

            if (clickButton != null)
            {
                clickButton.interactable = interactable;
            }

            // 调整透明度
            if (!interactable)
            {
                SetIconAlpha(0.3f);
            }
            else
            {
                SetIconAlpha(1f);  // 可交互时恢复完全不透明
            }
        }

        /// <summary>
        /// 显示选中状态
        /// </summary>
        /// <param name="selected">是否选中</param>
        public void SetSelected(bool selected)
        {
            // 选中时添加脉冲动画
            if (selected)
            {
                PlayPulseAnimation();
            }
            else
            {
                StopPulseAnimation();
            }
        }

        /// <summary>
        /// 播放使用动画
        /// </summary>
        public void PlayUseAnimation()
        {
            // 移除缩放动画，保持道具原样
            // transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f);

            // 只保留数量文本的动画（可选）
            if (countText != null)
            {
                countText.transform.DOPunchScale(Vector3.one * 0.3f, 0.3f);
            }
        }

        /// <summary>
        /// 播放购买成功动画
        /// </summary>
        public void PlayPurchaseAnimation()
        {
            // 旋转动画
            transform.DORotate(new Vector3(0, 0, 360), 0.5f, RotateMode.FastBeyond360)
                .SetEase(Ease.OutBack);

            // 闪光效果
            if (propIcon != null)
            {
                var sequence = DOTween.Sequence();
                sequence.Append(propIcon.DOColor(Color.white, 0.1f));
                sequence.Append(propIcon.DOColor(Color.white, 0.1f));
                sequence.SetLoops(2, LoopType.Yoyo);
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 按钮点击处理
        /// </summary>
        private void OnButtonClick()
        {
            if (!isInteractable) return;

            // 触发点击事件
            OnPropClicked?.Invoke(propType);

            // 根据数量决定行为
            if (currentCount > 0)
            {
                // 有道具，尝试使用（不播放缩放动画）
                TryUseProp();
            }
            else
            {
                // 无道具，显示购买弹窗（不播放缩放动画）
                ShowPurchasePopup();
            }
        }

        /// <summary>
        /// 尝试使用道具
        /// </summary>
        private void TryUseProp()
        {
            var propManager = PropManager.Instance;
            if (propManager != null)
            {
                bool success = propManager.UseProp(propType);
                if (success)
                {
                    // 移除使用动画，保持道具原样
                    // PlayUseAnimation();
                }
            }
        }

        /// <summary>
        /// 显示购买弹窗
        /// </summary>
        private void ShowPurchasePopup()
        {
            Debug.Log($"PropItem: 显示 {propType} 购买弹窗");

            // 使用MenuManager显示购买弹窗
            var menuManager = MenuManager.Instance;
            if (menuManager != null)
            {
                var popup = menuManager.ShowPopup<PropPurchase>();
                if (popup != null)
                {
                    popup.Initialize(propType);
                    Debug.Log($"PropItem: 成功打开 {propType} 购买弹窗");
                }
                else
                {
                    Debug.LogWarning($"PropItem: 无法创建购买弹窗实例 - {propType}");
                }
            }
            else
            {
                Debug.LogWarning($"PropItem: MenuManager未找到，无法显示购买弹窗 - {propType}");

                // 备用方案：尝试从设置中直接实例化购买弹窗
                var purchaseSettings = PropPurchaseSettings.Instance;
                if (purchaseSettings != null && purchaseSettings.propPurchasePopupPrefab != null)
                {
                    var popupGO = GameObject.Instantiate(purchaseSettings.propPurchasePopupPrefab);
                    var purchasePopup = popupGO.GetComponent<PropPurchase>();
                    if (purchasePopup != null)
                    {
                        purchasePopup.Initialize(propType);
                        purchasePopup.Show();
                        Debug.Log($"PropItem: 使用备用方案打开 {propType} 购买弹窗");
                    }
                }
            }
        }

        /// <summary>
        /// 道具数量变化回调
        /// </summary>
        private void OnPropCountChanged(PropType type, int count)
        {
            if (type == propType)
            {
                UpdateCount(count);
            }
        }

        /// <summary>
        /// 播放更新动画（已禁用）
        /// </summary>
        // private void PlayUpdateAnimation()
        // {
        //     if (countText != null)
        //     {
        //         countText.transform.localScale = Vector3.zero;
        //         countText.transform.DOScale(1f, 0.2f)
        //             .SetEase(Ease.OutBack);
        //     }
        // }

        /// <summary>
        /// 播放脉冲动画
        /// </summary>
        private void PlayPulseAnimation()
        {
            StopPulseAnimation();

            scaleSequence = DOTween.Sequence();
            scaleSequence.Append(transform.DOScale(1.1f, 0.5f));
            scaleSequence.Append(transform.DOScale(1f, 0.5f));
            scaleSequence.SetLoops(-1);
        }

        /// <summary>
        /// 停止脉冲动画
        /// </summary>
        private void StopPulseAnimation()
        {
            scaleSequence?.Kill();
            transform.localScale = Vector3.one;
        }

        /// <summary>
        /// 设置图标透明度
        /// </summary>
        private void SetIconAlpha(float alpha)
        {
            if (propIcon != null)
            {
                var color = propIcon.color;
                color.a = alpha;
                propIcon.color = color;
            }
        }

        #endregion
    }
}