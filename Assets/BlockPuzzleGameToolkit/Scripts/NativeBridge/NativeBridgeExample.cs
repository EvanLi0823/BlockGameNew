using UnityEngine;
using UnityEngine.UI;
using BlockPuzzle.NativeBridge;
using BlockPuzzle.NativeBridge.Enums;
using BlockPuzzle.NativeBridge.Models;

namespace BlockPuzzle.NativeBridge.Example
{
    /// <summary>
    /// NativeBridge使用示例
    /// 演示如何使用NativeBridge进行Unity与原生平台交互
    /// </summary>
    public class NativeBridgeExample : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Text _statusText;
        [SerializeField] private Text _logText;
        [SerializeField] private Button _initButton;
        [SerializeField] private Button _showVideoButton;
        [SerializeField] private Button _checkAdButton;
        [SerializeField] private Button _getParamsButton;
        [SerializeField] private Button _trackEventButton;

        private void Start()
        {
            // 设置按钮监听
            if (_initButton != null)
                _initButton.onClick.AddListener(OnInitButtonClicked);

            if (_showVideoButton != null)
                _showVideoButton.onClick.AddListener(OnShowVideoClicked);

            if (_checkAdButton != null)
                _checkAdButton.onClick.AddListener(OnCheckAdClicked);

            if (_getParamsButton != null)
                _getParamsButton.onClick.AddListener(OnGetParamsClicked);

            if (_trackEventButton != null)
                _trackEventButton.onClick.AddListener(OnTrackEventClicked);

            // 订阅事件
            SubscribeToEvents();

            // 检查初始化状态
            CheckInitStatus();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        #region 事件订阅

        private void SubscribeToEvents()
        {
            NativeBridgeManager.OnVideoPlayEnd += OnVideoPlayEnd;
            NativeBridgeManager.OnH5InitSuccess += OnH5InitSuccess;
            NativeBridgeManager.OnH5Exit += OnH5Exit;
            NativeBridgeManager.OnCommonParamReceived += OnCommonParamReceived;
            NativeBridgeManager.OnCurrencySymbolReceived += OnCurrencySymbolReceived;
        }

        private void UnsubscribeFromEvents()
        {
            NativeBridgeManager.OnVideoPlayEnd -= OnVideoPlayEnd;
            NativeBridgeManager.OnH5InitSuccess -= OnH5InitSuccess;
            NativeBridgeManager.OnH5Exit -= OnH5Exit;
            NativeBridgeManager.OnCommonParamReceived -= OnCommonParamReceived;
            NativeBridgeManager.OnCurrencySymbolReceived -= OnCurrencySymbolReceived;
        }

        #endregion

        #region 按钮点击事件

        private void OnInitButtonClicked()
        {
            LogMessage("正在初始化NativeBridge...");

            // 初始化会自动进行，这里只是检查状态
            if (NativeBridgeManager.Instance != null && NativeBridgeManager.Instance.IsInitSuccess())
            {
                LogMessage("NativeBridge已初始化成功！");
                UpdateStatus("已初始化");
            }
            else
            {
                LogMessage("NativeBridge初始化中...");
                UpdateStatus("初始化中");
            }
        }

        private void OnShowVideoClicked()
        {
            LogMessage("请求显示视频广告...");

            // 先检查广告是否就绪
            if (NativeBridgeManager.Instance.IsADReady(AdType.RewardVideo))
            {
                // 显示视频广告
                NativeBridgeManager.Instance.SendMessageToPlatform(BridgeMessageType.ShowVideo);
                LogMessage("正在显示视频广告...");
            }
            else
            {
                LogMessage("视频广告未就绪！");
            }
        }

        private void OnCheckAdClicked()
        {
            LogMessage("检查广告状态...");

            // 检查各类广告状态
            bool rewardReady = NativeBridgeManager.Instance.IsADReady(AdType.RewardVideo);
            bool interReady = NativeBridgeManager.Instance.IsADReady(AdType.Interstitial);
            bool admobReady = NativeBridgeManager.Instance.IsADReady(AdType.AdMob);

            LogMessage($"激励视频: {(rewardReady ? "就绪" : "未就绪")}");
            LogMessage($"插屏广告: {(interReady ? "就绪" : "未就绪")}");
            LogMessage($"AdMob广告: {(admobReady ? "就绪" : "未就绪")}");
        }

        private void OnGetParamsClicked()
        {
            LogMessage("获取公共参数...");

            // 发送获取公共参数请求
            NativeBridgeManager.Instance.SendMessageToPlatform(BridgeMessageType.CommonParam);

            // 获取已缓存的参数
            var commonParam = NativeBridgeManager.Instance.GetCommonParam();
            if (commonParam != null)
            {
                LogMessage($"语言: {commonParam.language}");
                LogMessage($"国家: {commonParam.country}");
                LogMessage($"编号GK: {commonParam.numberGK}");
            }

            // 获取货币符号
            string currency = NativeBridgeManager.Instance.GetCurrencySymbol();
            LogMessage($"货币符号: {currency}");
        }

        private void OnTrackEventClicked()
        {
            // 发送埋点事件
            string eventName = "test_event";
            string eventValue = "test_value_" + Random.Range(1, 100);

            LogMessage($"发送埋点事件: {eventName} = {eventValue}");

            NativeBridgeManager.Instance.SendMessageToPlatform(
                BridgeMessageType.BuryPoint,
                eventName,
                eventValue
            );

            LogMessage("埋点事件已发送");
        }

        #endregion

        #region 事件回调

        private void OnVideoPlayEnd()
        {
            LogMessage("视频广告播放结束！");
            UpdateStatus("视频播放完成");
        }

        private void OnH5InitSuccess(bool success)
        {
            LogMessage($"H5初始化{(success ? "成功" : "失败")}！");

            if (success)
            {
                int userType = NativeBridgeManager.Instance.GetH5UserType();
                LogMessage($"H5用户类型: {userType}");
            }
        }

        private void OnH5Exit()
        {
            LogMessage("H5已退出");
        }

        private void OnCommonParamReceived(CommonParamResponse param)
        {
            if (param != null)
            {
                LogMessage($"接收到公共参数 - 语言: {param.language}, 国家: {param.country}");
            }
        }

        private void OnCurrencySymbolReceived(string symbol)
        {
            LogMessage($"接收到货币符号: {symbol}");
        }

        #endregion

        #region 其他功能示例

        /// <summary>
        /// 显示隐私政策
        /// </summary>
        public void ShowPrivacyPolicy()
        {
            LogMessage("显示隐私政策...");
            NativeBridgeManager.Instance.SendMessageToPlatform(BridgeMessageType.PrivacyPolicy);
        }

        /// <summary>
        /// 显示使用条款
        /// </summary>
        public void ShowTermsOfUse()
        {
            LogMessage("显示使用条款...");
            NativeBridgeManager.Instance.SendMessageToPlatform(BridgeMessageType.TermsOfUse);
        }

        /// <summary>
        /// 显示反馈界面
        /// </summary>
        public void ShowFeedback()
        {
            LogMessage("显示反馈界面...");
            NativeBridgeManager.Instance.SendMessageToPlatform(BridgeMessageType.FeedBack);
        }

        /// <summary>
        /// 显示提现界面
        /// </summary>
        public void ShowWithdraw()
        {
            LogMessage("显示提现界面...");
            NativeBridgeManager.Instance.SendMessageToPlatform(BridgeMessageType.ShowWithdraw);
        }

        /// <summary>
        /// 显示促销活动
        /// </summary>
        public void ShowPromotion()
        {
            LogMessage("显示促销活动...");
            NativeBridgeManager.Instance.SendMessageToPlatform(BridgeMessageType.ShowPromotion);
        }

        /// <summary>
        /// 更新游戏等级
        /// </summary>
        public void UpdateGameLevel(int level)
        {
            LogMessage($"更新等级: {level}");
            NativeBridgeManager.Instance.SendMessageToPlatform(BridgeMessageType.UpdateLevel, level);
        }

        /// <summary>
        /// 进入游戏
        /// </summary>
        public void EnterGame()
        {
            LogMessage("进入游戏...");
            NativeBridgeManager.Instance.SendMessageToPlatform(BridgeMessageType.EnterGame);
        }

        /// <summary>
        /// 获取用户金额
        /// </summary>
        public void GetUserAmount()
        {
            LogMessage("获取用户金额...");
            NativeBridgeManager.Instance.SendMessageToPlatform(BridgeMessageType.UserAmount);
        }

        #endregion

        #region 辅助方法

        private void CheckInitStatus()
        {
            bool isInit = NativeBridgeManager.Instance != null && NativeBridgeManager.Instance.IsInitSuccess();
            UpdateStatus(isInit ? "已初始化" : "未初始化");
            LogMessage($"NativeBridge初始化状态: {(isInit ? "成功" : "未完成")}");
        }

        private void UpdateStatus(string status)
        {
            if (_statusText != null)
            {
                _statusText.text = $"状态: {status}";
            }
        }

        private void LogMessage(string message)
        {
            Debug.Log($"[NativeBridgeExample] {message}");

            if (_logText != null)
            {
                // 添加时间戳
                string logEntry = $"{System.DateTime.Now:HH:mm:ss} - {message}\n";

                // 保持最近的10条日志
                string[] lines = _logText.text.Split('\n');
                if (lines.Length >= 10)
                {
                    // 移除最早的日志
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    for (int i = 1; i < lines.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(lines[i]))
                        {
                            sb.AppendLine(lines[i]);
                        }
                    }
                    _logText.text = sb.ToString() + logEntry;
                }
                else
                {
                    _logText.text += logEntry;
                }
            }
        }

        #endregion
    }
}