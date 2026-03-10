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

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using BlockPuzzleGameToolkit.Scripts.PropSystem.Core;
using BlockPuzzleGameToolkit.Scripts.Settings;
// using BlockPuzzleGameToolkit.Scripts.Services; // AdsManager已移除
using BlockPuzzleGameToolkit.Scripts.Audio;

namespace BlockPuzzleGameToolkit.Scripts.Popups
{
    /// <summary>
    /// 道具购买弹窗 - 显示道具购买选项（简化版）
    /// </summary>
    public class PropPurchase : Popup
    {
        #region UI引用

        [Header("UI组件")]
        [Tooltip("道具图标")]
        [SerializeField] private Image propIcon;

        [Tooltip("广告按钮")]
        [SerializeField] private Button adButton;

        [Tooltip("当前道具数量文本")]
        [SerializeField] private TextMeshProUGUI propCountText;

        #endregion

        #region 私有字段

        /// <summary>
        /// 当前道具类型
        /// </summary>
        private PropType currentPropType;

        /// <summary>
        /// 道具配置
        /// </summary>
        private PropItemConfig itemConfig;

        /// <summary>
        /// 购买配置
        /// </summary>
        private PropPurchaseSettings.PropPurchaseConfig purchaseConfig;

        /// <summary>
        /// 道具管理器
        /// </summary>
        private PropManager propManager;

        // 广告管理器已移除

        /// <summary>
        /// 是否正在处理购买
        /// </summary>
        private bool isPurchasing = false;

        #endregion

        #region Unity生命周期

        protected override void Awake()
        {
            base.Awake();

            // 获取管理器引用
            propManager = PropManager.Instance;
            // adsManager已移除

            // 绑定按钮事件
            if (adButton != null)
            {
                adButton.onClick.AddListener(OnAdButtonClick);
            }
        }

        private void OnDestroy()
        {
            // 解绑事件
            if (adButton != null)
            {
                adButton.onClick.RemoveListener(OnAdButtonClick);
            }
        }

        private void OnEnable()
        {
            // 订阅道具数量变化事件
            PropManager.OnPropCountChanged += OnPropCountChanged;
        }

        private void OnDisable()
        {
            // 取消订阅
            PropManager.OnPropCountChanged -= OnPropCountChanged;
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 初始化弹窗
        /// </summary>
        /// <param name="propType">道具类型</param>
        public void Initialize(PropType propType)
        {
            currentPropType = propType;

            // 获取配置
            var propSettings = PropSettings.Instance;
            var purchaseSettings = PropPurchaseSettings.Instance;

            if (propSettings == null || purchaseSettings == null)
            {
                Debug.LogError("PropPurchase: 设置未找到");
                Close();
                return;
            }

            itemConfig = propSettings.GetConfig(propType);
            purchaseConfig = purchaseSettings.GetPurchaseConfig(propType);

            if (itemConfig == null || purchaseConfig == null)
            {
                Debug.LogError($"PropPurchase: 道具 {propType} 配置未找到");
                Close();
                return;
            }

            // 设置UI
            SetupUI();
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 设置UI显示
        /// </summary>
        private void SetupUI()
        {
            // 设置道具图标
            if (propIcon != null && itemConfig.propIcon != null)
            {
                propIcon.sprite = itemConfig.propIcon;
            }

            // 更新道具数量显示
            UpdatePropCount();

            // 设置广告按钮状态
            SetupAdButton();

            // 播放出现动画
            PlayShowAnimation();
        }

        /// <summary>
        /// 设置广告按钮
        /// </summary>
        private void SetupAdButton()
        {
            if (adButton == null) return;

            // 检查是否可以通过广告购买
            bool canPurchaseWithAds = purchaseConfig.canPurchaseWithAds;
            adButton.gameObject.SetActive(canPurchaseWithAds);

            if (!canPurchaseWithAds) return;

            // 检查每日限制（如果有的话）
            if (purchaseConfig.dailyAdLimit > 0)
            {
                // TODO: 实际应该获取今日已购买次数
                // 这里简化处理，暂时总是启用
                adButton.interactable = true;
            }
            else
            {
                adButton.interactable = true;
            }
        }

        /// <summary>
        /// 更新道具数量显示
        /// </summary>
        private void UpdatePropCount()
        {
            if (propCountText != null && propManager != null)
            {
                int count = propManager.GetPropCount(currentPropType);
                propCountText.text = $"当前数量: {count}";
            }
        }

        /// <summary>
        /// 广告按钮点击
        /// </summary>
        private void OnAdButtonClick()
        {
            if (isPurchasing) return;

            isPurchasing = true;

            // 播放点击音效
            SoundBase.Instance?.PlaySound(SoundBase.Instance.click);

            // 广告功能已移除，直接给予奖励
            Debug.Log("PropPurchase: 广告功能已移除，直接给予奖励");
            OnAdComplete(true);
        }

        /// <summary>
        /// 广告完成回调
        /// </summary>
        private void OnAdComplete(bool success)
        {
            if (success)
            {
                // 发放奖励
                if (propManager != null)
                {
                    propManager.PurchasePropWithAds(currentPropType);
                }

                // 播放成功音效
                SoundBase.Instance?.PlaySound(SoundBase.Instance.coins);

                // 播放成功动画
                PlayPurchaseSuccessAnimation();

                // 延迟关闭
                DOVirtual.DelayedCall(0.5f, Close);
            }
            else
            {
                Debug.Log("广告观看失败或被取消");
            }

            isPurchasing = false;
        }

        /// <summary>
        /// 道具数量变化回调
        /// </summary>
        private void OnPropCountChanged(PropType type, int count)
        {
            if (type == currentPropType)
            {
                UpdatePropCount();
            }
        }

        #endregion

        #region 动画

        /// <summary>
        /// 播放显示动画
        /// </summary>
        private void PlayShowAnimation()
        {
            // 简单的缩放弹出动画
            transform.localScale = Vector3.zero;
            transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        }

        /// <summary>
        /// 播放购买成功动画
        /// </summary>
        private void PlayPurchaseSuccessAnimation()
        {
            // 道具图标动画
            if (propIcon != null)
            {
                var sequence = DOTween.Sequence();

                // 放大缩小
                sequence.Append(propIcon.transform.DOScale(1.3f, 0.15f));
                sequence.Append(propIcon.transform.DOScale(1f, 0.15f));

                // 旋转
                sequence.Join(propIcon.transform.DORotate(new Vector3(0, 0, 360), 0.3f, RotateMode.FastBeyond360));
            }
        }

        /// <summary>
        /// 重写关闭方法
        /// </summary>
        public override void Close()
        {
            // 缩小消失动画
            transform.DOScale(0f, 0.2f)
                .SetEase(Ease.InBack)
                .OnComplete(() => base.Close());
        }

        #endregion
    }
}