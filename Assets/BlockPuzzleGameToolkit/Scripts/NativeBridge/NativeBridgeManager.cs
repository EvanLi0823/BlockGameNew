using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Newtonsoft.Json;
using BlockPuzzle.NativeBridge.Enums;
using BlockPuzzle.NativeBridge.Models;
using BlockPuzzleGameToolkit.Scripts.Settings;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using BlockPuzzleGameToolkit.Scripts.CurrencySystem;
using BlockPuzzleGameToolkit.Scripts.RewardSystem;
using BlockPuzzleGameToolkit.Scripts.Enums;

namespace BlockPuzzle.NativeBridge
{
    /// <summary>
    /// Unity与原生平台交互的桥接管理器
    /// 实现Unity C#代码与原生代码的双向通信
    /// 采用SingletonBehaviour单例模式
    /// </summary>
    public class NativeBridgeManager : SingletonBehaviour<NativeBridgeManager>
    {
        #region iOS Native Interface
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string callNative(string msg);
#endif
        #endregion

        #region Private Fields

        // 配置设置
        private NativeBridgeSettings _settings;

        // Android原生对象
#if UNITY_ANDROID && !UNITY_EDITOR
        private AndroidJavaObject _androidJavaObject;
#endif

        // 初始化状态
        private bool _commonParamReturn = false;
        private bool _isUnifyCurrencyRef = false;
        private bool _h5InitResult = false;
        //是否是白包
        private bool _isWhitePackage = false;
        private int _h5UserType = 0;
        private int _notifyReward = 0;


        // 缓存数据
        private CommonParamResponse _commonParam;
        private string _currencySymbol = "$";

        // 消息处理器字典
        private Dictionary<BridgeMessageType, Action<Dictionary<string, object>>> _messageHandlers;

        // 方法名映射
        private readonly Dictionary<BridgeMessageType, string> _methodNameMap = new Dictionary<BridgeMessageType, string>
        {
            { BridgeMessageType.CommonParam, "getCommonParm" },
            { BridgeMessageType.PrivacyPolicy, "PrivacyPolicy" },
            { BridgeMessageType.TermsOfUse, "TermsofUse" },
            { BridgeMessageType.PlayMergeAudio, "playMergeAudio" },
            { BridgeMessageType.ShowWithdraw, "showWithdraw" },
            { BridgeMessageType.GetMergeReward, "getMergeReward" },
            { BridgeMessageType.BuryPoint, "buryPoint" },
            { BridgeMessageType.ShowVideo, "showVideo" },
            { BridgeMessageType.RequestIsWhiteBao, "requestIsWhiteBao" },
            { BridgeMessageType.GetUnifyCurrency, "getUnifyCurrency" },
            { BridgeMessageType.FeedBack, "feedback" },
            { BridgeMessageType.ShowPromotion, "showPromotion" },
            { BridgeMessageType.EnterGame, "enterGame" },
            { BridgeMessageType.UpdateLevel, "updateLevel" },
            { BridgeMessageType.UserAmount, "userAmount" },
            { BridgeMessageType.IsInterADReady, "isInterReady" },
            { BridgeMessageType.IsRewardADReady, "isRewardReady" },
            { BridgeMessageType.IsAdMobADReady, "isAdMobReady" },
            { BridgeMessageType.ShowWithdrawGuide, "showWithdrawGuide" },
            { BridgeMessageType.IsWithdrawReward, "isWithdrawReward" }
        };

        #endregion

        #region Public Events

        /// <summary>
        /// 视频广告播放结束事件
        /// </summary>
        public static event Action OnVideoPlayEnd;

        /// <summary>
        /// H5初始化成功事件
        /// </summary>
        public static event Action<bool> OnH5InitSuccess;

        /// <summary>
        /// H5退出事件
        /// </summary>
        public static event Action OnH5Exit;

        /// <summary>
        /// 公共参数接收事件
        /// </summary>
        public static event Action<CommonParamResponse> OnCommonParamReceived;

        /// <summary>
        /// 货币符号接收事件
        /// </summary>
        public static event Action<string> OnCurrencySymbolReceived;

        #endregion

        #region Lifecycle

        public override void Awake()
        {
            base.Awake();
            // DontDestroyOnLoad(gameObject); // 移除重复调用，基类已处理
        }

        /// <summary>
        /// 加载配置设置
        /// </summary>
        private void LoadSettings()
        {
            _settings = Resources.Load<NativeBridgeSettings>("Settings/NativeBridgeSettings");
            if (_settings == null)
            {
                Debug.LogError("[NativeBridge] NativeBridgeSettings not found in Resources/Settings/! Please create one using Assets > Create > Block Puzzle > Settings > Native Bridge Settings");
            }
        }

        public override void OnInit()
        {
             if (!IsInitialized)
            {
                Debug.Log("[NativeBridge] Starting NativeBridge initialization...");

                // 加载配置设置
                LoadSettings();

                if (_settings != null && _settings.enableNativeBridge)
                {
                    InitializeHandlers();
                    if (_settings.autoInitialize)
                    {
                        Init();
                    }
                    else
                    {
                        Debug.Log("[NativeBridge] Auto-initialization is disabled. Call Init() manually when ready.");
                    }
                }
                else
                {
                    Debug.LogWarning("[NativeBridge] Native Bridge is disabled or settings not found");
                };
            }
            base.OnInit(); // 设置IsInitialized = true
        }

        /// <summary>
        /// 初始化平台管理器
        /// </summary>
        private void Init()
        {
            if (_settings == null || !_settings.ValidateSettings())
            {
                Debug.LogError("[NativeBridge] Invalid settings, initialization aborted");
                return;
            }

            _settings.LogDebug("Initializing...");

#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                _androidJavaObject = new AndroidJavaObject(_settings.androidPackageName);
                _settings.LogDebug("Android initialization successful");
                Debug.Log($"[NativeBridge] ✓ Android platform initialized successfully (Package: {_settings.androidPackageName})");
            }
            catch (Exception e)
            {
                _settings.LogError($"Android initialization failed: {e.Message}");
                Debug.LogError($"[NativeBridge] ✗ Android initialization failed: {e.Message}");
                return;
            }
#elif UNITY_IOS && !UNITY_EDITOR
            _settings.LogDebug("iOS platform detected");
            Debug.Log($"[NativeBridge] ✓ iOS platform initialized successfully (Method: {_settings.iOSMethodName})");
#endif

#if !UNITY_EDITOR
            // 根据配置自动调用初始化接口
            if (_settings.autoRequestCommonParams)
                SendMessageToPlatform(BridgeMessageType.CommonParam);
            if (_settings.autoRequestWhiteBao)
                SendMessageToPlatform(BridgeMessageType.RequestIsWhiteBao);
            if (_settings.autoRequestCurrency)
                SendMessageToPlatform(BridgeMessageType.GetUnifyCurrency);
#else
            // 编辑器模式下模拟初始化成功
            if (_settings.mockResponseInEditor)
            {
                _commonParamReturn = true;
                _isUnifyCurrencyRef = true;
                Debug.Log("[NativeBridge] ✓ Editor mode initialized successfully (Mock mode enabled)");
            }
            else
            {
                Debug.Log("[NativeBridge] ✓ Editor mode initialized successfully (Mock mode disabled)");
            }
#endif

            // 输出初始化总结
            Debug.Log($"[NativeBridge] ========================================");
            Debug.Log($"[NativeBridge] NativeBridge Initialization Complete");
            Debug.Log($"[NativeBridge] Platform: {GetCurrentPlatformName()}");
            Debug.Log($"[NativeBridge] Auto Request: CommonParams={_settings.autoRequestCommonParams}, WhiteBao={_settings.autoRequestWhiteBao}, Currency={_settings.autoRequestCurrency}");
            Debug.Log($"[NativeBridge] Debug Logs: {(_settings.enableDebugLogs ? "Enabled" : "Disabled")}");
            Debug.Log($"[NativeBridge] ========================================");
        }

        /// <summary>
        /// 获取当前平台名称
        /// </summary>
        private string GetCurrentPlatformName()
        {
#if UNITY_EDITOR
            return "Unity Editor";
#elif UNITY_ANDROID
            return "Android";
#elif UNITY_IOS
            return "iOS";
#else
            return "Unknown";
#endif
        }

        /// <summary>
        /// 初始化消息处理器
        /// </summary>
        private void InitializeHandlers()
        {
            _messageHandlers = new Dictionary<BridgeMessageType, Action<Dictionary<string, object>>>
            {
                { BridgeMessageType.CommonParam, HandleCommonParam },
                { BridgeMessageType.PrivacyPolicy, HandlePrivacyPolicy },
                { BridgeMessageType.TermsOfUse, HandleTermsOfUse },
                { BridgeMessageType.PlayMergeAudio, HandlePlayMergeAudio },
                { BridgeMessageType.ShowWithdraw, HandleShowWithdraw },
                { BridgeMessageType.GetMergeReward, HandleGetMergeReward },
                { BridgeMessageType.BuryPoint, HandleBuryPoint },
                { BridgeMessageType.ShowVideo, HandleShowVideo },
                { BridgeMessageType.RequestIsWhiteBao, HandleRequestIsWhiteBao },
                { BridgeMessageType.GetUnifyCurrency, HandleGetUnifyCurrency },
                { BridgeMessageType.FeedBack, HandleFeedBack },
                { BridgeMessageType.ShowPromotion, HandleShowPromotion },
                { BridgeMessageType.EnterGame, HandleEnterGame },
                { BridgeMessageType.UpdateLevel, HandleUpdateLevel },
                { BridgeMessageType.UserAmount, HandleUserAmount },
                // 广告查询消息处理器
                { BridgeMessageType.IsRewardADReady, HandleAdReadyQuery },
                { BridgeMessageType.IsInterADReady, HandleAdReadyQuery },
                { BridgeMessageType.IsAdMobADReady, HandleAdReadyQuery }
            };
        }

        private void OnDestroy()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (_androidJavaObject != null)
            {
                _androidJavaObject.Dispose();
                _androidJavaObject = null;
            }
#endif
        }

        #endregion

        #region Public API - Unity to Native

        /// <summary>
        /// 向原生平台发送消息
        /// </summary>
        /// <param name="type">消息类型</param>
        /// <param name="param1">参数1</param>
        /// <param name="param2">参数2</param>
        public void SendMessageToPlatform(BridgeMessageType type, object param1 = null, object param2 = null)
        {
            if (_settings == null || !_settings.enableNativeBridge)
            {
                Debug.LogWarning("[NativeBridge] Native Bridge is not enabled or configured");
                return;
            }

            if (type == BridgeMessageType.None)
            {
                _settings.LogWarning("Cannot send None type message");
                return;
            }

            if (!_methodNameMap.TryGetValue(type, out string methodName))
            {
                _settings.LogError($"No method name mapping for {type}");
                return;
            }

            // 构建消息
            var message = new BridgeMessage(methodName, param1, param2);
            string jsonData = JsonConvert.SerializeObject(message);

            _settings.LogDebug($"SendMessageToPlatform: {methodName} with data: {jsonData}");

            string result = GetMessageFromPlatform(jsonData);

            if (!string.IsNullOrEmpty(result))
            {
                // 传递消息类型给DecodeMessage，用于处理同步响应
                DecodeMessage(result, type);
            }
        }

        /// <summary>
        /// 向原生平台发送消息并同步获取结果
        /// </summary>
        /// <param name="type">消息类型</param>
        /// <param name="param1">参数1</param>
        /// <param name="param2">参数2</param>
        /// <returns>返回解析后的结果字典</returns>
        public Dictionary<string, object> SendMessageAndGetResult(BridgeMessageType type, object param1 = null, object param2 = null)
        {
            if (_settings == null || !_settings.enableNativeBridge)
            {
                Debug.LogWarning("[NativeBridge] Native Bridge is not enabled or configured");
                return null;
            }

            if (type == BridgeMessageType.None)
            {
                _settings.LogWarning("Cannot send None type message");
                return null;
            }

            if (!_methodNameMap.TryGetValue(type, out string methodName))
            {
                _settings.LogError($"No method name mapping for {type}");
                return null;
            }

            // 构建消息
            var message = new BridgeMessage(methodName, param1, param2);
            string jsonData = JsonConvert.SerializeObject(message);

            _settings.LogDebug($"SendMessageAndGetResult: {methodName} with data: {jsonData}");

            string result = GetMessageFromPlatform(jsonData);

            if (!string.IsNullOrEmpty(result))
            {
                try
                {
                    var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(result);
                    _settings.LogDebug($"SendMessageAndGetResult received: {result}");
                    return data;
                }
                catch (Exception e)
                {
                    _settings.LogError($"Failed to parse result: {e.Message}");
                    return null;
                }
            }

            return null;
        }

        /// <summary>
        /// 调用原生平台方法并获取返回值
        /// </summary>
        private string GetMessageFromPlatform(string jsonData)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (_androidJavaObject != null)
            {
                try
                {
                    return _androidJavaObject.Call<string>(_settings.androidMethodName, jsonData);
                }
                catch (Exception e)
                {
                    _settings.LogError($"Android call failed: {e.Message}");
                    return null;
                }
            }
#elif UNITY_IOS && !UNITY_EDITOR
            try
            {
                return callNative(jsonData);
            }
            catch (Exception e)
            {
                _settings.LogError($"iOS call failed: {e.Message}");
                return null;
            }
#endif
            // 编辑器模式返回模拟数据
            if (_settings != null && _settings.mockResponseInEditor)
            {
                return GetMockResponse(jsonData);
            }
            return "{}";
        }

        /// <summary>
        /// 编辑器模式下的模拟响应
        /// </summary>
        private static string GetMockResponse(string jsonData)
        {
#if UNITY_EDITOR
            var message = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonData);
            if (message != null && message.ContainsKey("m"))
            {
                string method = message["m"].ToString();
                switch (method)
                {
                    case "getCommonParm":
                        return "{\"language\":\"en\",\"country\":\"us\",\"numberGK\":1}";
                    case "getUnifyCurrency":
                        return "{\"amount\":\"$\"}";
                    case "isRewardReady":
                    case "isInterReady":
                    case "isAdMobReady":
                        // 返回广告就绪状态：1表示准备好，0表示未准备好
                        return "{\"amount\":1}";
                    default:
                        return "{}";
                }
            }
#endif
            return "{}";
        }

        #endregion

        #region Public API - Native to Unity

        /// <summary>
        /// H5初始化结果回调（由原生调用）
        /// </summary>
        public void H5InitResult(string msg)
        {
            if (_settings != null)
                _settings.LogDebug($"H5InitResult: {msg}");

            var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(msg);
            if (data != null && data.ContainsKey("amount"))
            {
                _h5UserType = Convert.ToInt32(data["amount"]);
                _h5InitResult = true;
                OnH5InitSuccess?.Invoke(true);
            }
        }

        /// <summary>
        /// H5增加现金回调（由原生调用）
        /// </summary>
        public void H5AddCash(string msg)
        {
            if (_settings != null)
                _settings.LogDebug($"H5AddCash: {msg}");

            var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(msg);
            if (data != null && data.ContainsKey("amount"))
            {
                float amount = Convert.ToSingle(data["amount"]);
                // TODO: 处理现金增加逻辑
                if (_settings != null)
                    _settings.LogDebug($"Cash added: {amount}");
            }
        }

        /// <summary>
        /// H5状态变化（由原生调用）
        /// </summary>
        public void H5State(string msg)
        {
            if (_settings != null)
                _settings.LogDebug($"H5State: {msg}");
            OnH5Exit?.Invoke();
        }

        /// <summary>
        /// 设置屏幕方向（由原生调用）
        /// </summary>
        public void SetOrientation(string msg)
        {
            if (_settings != null)
                _settings.LogDebug($"SetOrientation: {msg}");

            var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(msg);
            if (data != null && data.ContainsKey("amount"))
            {
                int orientation = Convert.ToInt32(data["amount"]);
                UnityEngine.ScreenOrientation screenOrientation = orientation == 0
                    ? UnityEngine.ScreenOrientation.LandscapeLeft
                    : UnityEngine.ScreenOrientation.Portrait;

                Screen.orientation = screenOrientation;
                if (_settings != null)
                    _settings.LogDebug($"Screen orientation set to: {screenOrientation}");
            }
        }

        /// <summary>
        /// 广告播放结果回调（由原生调用）
        /// </summary>
        public void ADPlayResult(string msg)
        {
            if (_settings != null)
                _settings.LogDebug($"ADPlayResult: {msg}");

            try
            {
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(msg);
                if (data != null)
                {
                    // 解析广告类型（amount字段表示广告类型）
                    int adType = 0;
                    if (data.ContainsKey("amount"))
                    {
                        adType = Convert.ToInt32(data["amount"]);
                        // 广告类型：0-激励视频，1-插屏，2-AdMob
                    }

                    // 解析是否成功（默认为成功，因为原生调用了ADPlayResult）
                    bool isSuccess = true;
                    if (data.ContainsKey("success"))
                    {
                        isSuccess = Convert.ToBoolean(data["success"]);
                    }
                    else if (data.ContainsKey("result"))
                    {
                        // 兼容不同的字段名
                        isSuccess = Convert.ToBoolean(data["result"]);
                    }
                    else if (data.ContainsKey("status"))
                    {
                        // 兼容status字段（0失败，1成功）
                        isSuccess = Convert.ToInt32(data["status"]) == 1;
                    }

                    // 解析错误信息（如果有）
                    string errorMsg = "";
                    if (data.ContainsKey("error"))
                    {
                        errorMsg = Convert.ToString(data["error"]);
                    }
                    else if (data.ContainsKey("message"))
                    {
                        errorMsg = Convert.ToString(data["message"]);
                    }

                    if (_settings != null)
                    {
                        _settings.LogDebug($"Ad play result - Type: {adType}, Success: {isSuccess}, Error: {errorMsg}");
                    }

                    // 根据结果触发相应事件
                    if (isSuccess)
                    {
                        // 成功：触发视频播放结束事件，通知AdSystemManager
                        OnVideoPlayEnd?.Invoke();

                        // 记录具体的广告类型
                        switch (adType)
                        {
                            case 0: // 激励视频
                                if (_settings != null)
                                    _settings.LogDebug("Rewarded video ad completed successfully");
                                break;
                            case 1: // 插屏广告
                                if (_settings != null)
                                    _settings.LogDebug("Interstitial ad completed successfully");
                                break;
                            case 2: // AdMob
                                if (_settings != null)
                                    _settings.LogDebug("AdMob ad completed successfully");
                                break;
                            default:
                                if (_settings != null)
                                    _settings.LogDebug($"Unknown ad type: {adType} completed successfully");
                                break;
                        }
                    }
                    else
                    {
                        // 失败：记录错误但不触发OnVideoPlayEnd
                        if (_settings != null)
                        {
                            _settings.LogWarning($"Ad play failed - Type: {adType}, Error: {errorMsg}");
                        }

                        // TODO: 可能需要添加失败事件，如 OnVideoPlayFailed
                    }
                }
                else
                {
                    if (_settings != null)
                        _settings.LogError("ADPlayResult: Failed to parse data");
                }
            }
            catch (Exception ex)
            {
                if (_settings != null)
                    _settings.LogError($"ADPlayResult exception: {ex.Message}");

                // 注意：异常时不应触发成功事件，因为这是错误状态
                // 如果需要处理广告失败，应该触发失败事件而不是成功事件
                // 当前的 Failed.cs 等业务层已经有防护机制，不会因为缺少事件而卡死
            }
        }

        /// <summary>
        /// 通知奖励到达（由原生调用）
        /// </summary>
        public void NotifyReward()
        {
            if (_settings != null)
                _settings.LogDebug("NotifyReward");
            _notifyReward++;
        }

        #endregion

        #region Public API - Query Methods

        /// <summary>
        /// 检查初始化是否成功
        /// </summary>
        public bool IsInitSuccess()
        {
#if UNITY_EDITOR
            // 编辑器模式下总是返回 true
            if (_settings != null && _settings.mockResponseInEditor)
            {
                return true;
            }
            // 确保变量被读取，避免编译器警告
            return _commonParamReturn && _isUnifyCurrencyRef;
#else
            return _commonParamReturn && _isUnifyCurrencyRef;
#endif
        }

        /// <summary>
        /// 查询广告就绪状态
        /// </summary>
        /// <param name="type">广告类型：0-激励视频，1-插屏，2-AdMob</param>
        public bool IsADReady(AdType type)
        {
#if UNITY_EDITOR
            return true;
#else
            BridgeMessageType messageType = BridgeMessageType.None;
            switch (type)
            {
                case AdType.RewardVideo:
                    messageType = BridgeMessageType.IsRewardADReady;
                    break;
                case AdType.Interstitial:
                    messageType = BridgeMessageType.IsInterADReady;
                    break;
                case AdType.AdMob:
                    messageType = BridgeMessageType.IsAdMobADReady;
                    break;
            }

            if (messageType != BridgeMessageType.None)
            {
                // 同步获取查询结果
                var result = SendMessageAndGetResult(messageType);

                if (result != null && result.ContainsKey("amount"))
                {
                    // 解析返回的amount值：1表示准备好，0表示未准备好
                    if (result["amount"] is int intValue)
                    {
                        return intValue == 1;
                    }
                    else if (int.TryParse(result["amount"].ToString(), out int parsedValue))
                    {
                        return parsedValue == 1;
                    }
                }

                // 如果没有明确的结果，默认返回false
                return false;
            }
            return false;
#endif
        }

         /// <summary>
        /// 用户已提现，切换分组H5界面（由原生调用）
        /// </summary>
        public void withdrawAction(string msg)
        {
            if (_settings != null)
                _settings.LogDebug($"ADPlayResult: {msg}");

            var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(msg);
            if (data != null && data.ContainsKey("amount"))
            {
                int amount = Convert.ToInt32(data["amount"]);
                //货币系统减去提现的金额
                CurrencyManager.Instance.SpendCoins(amount);
                //广播金币变化事件
                EventManager.GetEvent(EGameEvent.CurrencyChanged).Invoke();
            }
            if (data != null && data.ContainsKey("id"))
            {
                string id = Convert.ToString(data["id"]);
                //奖励系统用户的分组切换
                RewardCalculator.Instance.SwitchToRange(id);
            }
            //广播事件用户已提现
            EventManager.GetEvent(EGameEvent.HasWithDraw).Invoke();
        }

        /// <summary>
        /// 检查是否可以显示H5界面
        /// </summary>
        public bool CheckCanShowH5()
        {
            return _h5InitResult;
        }

        /// <summary>
        /// 获取H5用户类型
        /// </summary>
        public int GetH5UserType()
        {
            return _h5UserType;
        }

        /// <summary>
        /// 是否有通知奖励
        /// </summary>
        public bool HaveNotifyReward()
        {
            return _notifyReward > 0;
        }

        /// <summary>
        /// 消耗通知奖励
        /// </summary>
        public void PushNotifyReward()
        {
            if (_notifyReward > 0)
            {
                _notifyReward--;
            }
        }

        /// <summary>
        /// 获取公共参数
        /// </summary>
        public CommonParamResponse GetCommonParam()
        {
            return _commonParam;
        }

        /// <summary>
        /// 获取货币符号
        /// </summary>
        public string GetCurrencySymbol()
        {
            return _currencySymbol;
        }

        public bool IsWhitePackage()
        {
            return _isWhitePackage;
        }


        /// <summary>
        /// 打开提现界面（自动收集数据）
        /// </summary>
        public void ShowWithdrawInterface()
        {
            // 自动从各个模块收集数据
            var withdrawParams = CollectWithdrawParams();

            // 调用带参数的重载方法
            ShowWithdrawInterface(withdrawParams);
        }

        /// <summary>
        /// 打开提现界面（使用提供的参数）
        /// </summary>
        /// <param name="withdrawParams">提现参数，如果为null则自动收集</param>
        public void ShowWithdrawInterface(WithdrawParams withdrawParams)
        {
            // 如果没有提供参数，自动收集
            if (withdrawParams == null)
            {
                withdrawParams = CollectWithdrawParams();
            }

            // 将参数序列化为JSON字符串
            string jsonParams = JsonConvert.SerializeObject(withdrawParams);

            if (_settings != null)
            {
                _settings.LogDebug($"ShowWithdraw called with params: {jsonParams}");
            }

            // 调用原生接口
            SendMessageToPlatform(BridgeMessageType.ShowWithdraw, jsonParams);
        }

        /// <summary>
        /// 从各个游戏模块收集提现参数
        /// </summary>
        private WithdrawParams CollectWithdrawParams()
        {
            var withdrawParams = new WithdrawParams();
            withdrawParams.CurrentCoin = "0";
            // 1. 获取当前货币（内部整数值）
            try
            {
                var currencyManager = BlockPuzzleGameToolkit.Scripts.CurrencySystem.CurrencyManager.Instance;
                if (currencyManager != null && currencyManager.IsInitialized)
                {
                    int coinsInt = currencyManager.GetCoins();
                    withdrawParams.CurrentAmount = coinsInt.ToString();
                }
                else
                {
                    withdrawParams.CurrentAmount = "0";
                }
            }
            catch (Exception e)
            {
                if (_settings != null)
                    _settings.LogDebug($"Error getting currency: {e.Message}");
                withdrawParams.CurrentAmount = "0";

            }

            // 2. 获取万能方块数量
            try
            {
                // TODO: 从道具系统获取万能方块数量
                // var propManager = BlockPuzzleGameToolkit.Scripts.PropSystem.Core.PropManager.Instance;
                // if (propManager != null)
                // {
                //     int universalBlockCount = propManager.GetPropCount(PropType.UniversalBlock);
                //     withdrawParams.CurrentBlock = universalBlockCount.ToString();
                // }
                withdrawParams.CurrentBlock = "0"; // 暂时使用默认值
            }
            catch (Exception e)
            {
                if (_settings != null)
                    _settings.LogDebug($"Error getting blocks: {e.Message}");
                withdrawParams.CurrentBlock = "0";
            }

            // 3. 获取当前关卡
            try
            {
                int levelNum = BlockPuzzleGameToolkit.Scripts.GameCore.GameDataManager.GetLevelNum();
                withdrawParams.CurrentLevel = levelNum.ToString();
            }
            catch (Exception e)
            {
                if (_settings != null)
                    _settings.LogDebug($"Error getting level: {e.Message}");
                withdrawParams.CurrentLevel = "1";
            }

            // 4. 获取看广告次数
            try
            {
                // TODO: 从广告管理器获取统计数据
                // var adsManager = BlockPuzzleGameToolkit.Scripts.Services.AdsManager.Instance;
                // if (adsManager != null)
                // {
                //     int adWatchCount = adsManager.GetWatchedAdsCount();
                //     withdrawParams.AdCount = adWatchCount.ToString();
                // }
                withdrawParams.AdCount = "0"; // 暂时使用默认值
            }
            catch (Exception e)
            {
                if (_settings != null)
                    _settings.LogDebug($"Error getting ad count: {e.Message}");
                withdrawParams.AdCount = "0";
            }

            // 5. 获取方块消除次数
            try
            {
                // TODO: 从游戏统计系统获取
                // var gameStats = BlockPuzzleGameToolkit.Scripts.GameCore.GameStats.Instance;
                // if (gameStats != null)
                // {
                //     int matchCount = gameStats.GetTotalMatchCount();
                //     withdrawParams.MatchCount = matchCount.ToString();
                // }
                withdrawParams.MatchCount = "0"; // 暂时使用默认值
            }
            catch (Exception e)
            {
                if (_settings != null)
                    _settings.LogDebug($"Error getting match count: {e.Message}");
                withdrawParams.MatchCount = "0";
            }

            if (_settings != null)
            {
                _settings.LogDebug($"Collected withdraw params: Amount={withdrawParams.CurrentAmount}, " +
                    $"Coin={withdrawParams.CurrentCoin}, Block={withdrawParams.CurrentBlock}, " +
                    $"Level={withdrawParams.CurrentLevel}, AdCount={withdrawParams.AdCount}, " +
                    $"MatchCount={withdrawParams.MatchCount}");
            }

            return withdrawParams;
        }

        /// <summary>
        /// 打开提现界面（使用具体参数）
        /// </summary>
        public void ShowWithdrawInterface(string currentAmount, string currentCoin, string currentBlock,
            string currentLevel, string adCount, string matchCount)
        {
            var withdrawParams = new WithdrawParams
            {
                CurrentAmount = currentAmount,
                CurrentCoin = currentCoin,
                CurrentBlock = currentBlock,
                CurrentLevel = currentLevel,
                AdCount = adCount,
                MatchCount = matchCount
            };

            ShowWithdrawInterface(withdrawParams);
        }

        #endregion

        #region Message Decoding

        /// <summary>
        /// 解码并处理消息
        /// </summary>
        /// <param name="msg">消息字符串</param>
        /// <param name="requestType">发送请求时的消息类型（用于处理同步响应）</param>
        private void DecodeMessage(string msg, BridgeMessageType requestType = BridgeMessageType.None)
        {
            if (string.IsNullOrEmpty(msg))
            {
                return;
            }

            if (_settings != null)
                _settings.LogDebug($"DecodeMessage: {msg}");

            try
            {
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(msg);
                Debug.Log($"Decoded message data: {JsonConvert.SerializeObject(data)}");
                if (data != null)
                {
                    ProcessMessage(data, requestType);
                }
            }
            catch (Exception e)
            {
                if (_settings != null)
                    _settings.LogError($"Failed to decode message: {e.Message}");
            }
        }

        /// <summary>
        /// 处理解码后的消息
        /// </summary>
        /// <param name="data">解码后的数据</param>
        /// <param name="requestType">发送请求时的消息类型（用于处理同步响应）</param>
        private void ProcessMessage(Dictionary<string, object> data, BridgeMessageType requestType = BridgeMessageType.None)
        {
            // 根据不同的数据结构处理
            // 尝试通过方法名查找对应的处理器
            if (data.ContainsKey("m"))
            {
                string methodName = data["m"].ToString();
                var type = GetMessageTypeByMethodName(methodName);
                if (type != BridgeMessageType.None && _messageHandlers.ContainsKey(type))
                {
                    _messageHandlers[type]?.Invoke(data);
                }
            }
            else
            {
                // 直接处理数据（用于同步响应，没有包含方法名的情况）
                HandleDirectMessage(data, requestType);
            }
        }

        /// <summary>
        /// 根据方法名获取消息类型
        /// </summary>
        private BridgeMessageType GetMessageTypeByMethodName(string methodName)
        {
            foreach (var kvp in _methodNameMap)
            {
                if (kvp.Value == methodName)
                {
                    return kvp.Key;
                }
            }
            return BridgeMessageType.None;
        }

        /// <summary>
        /// 处理直接消息
        /// </summary>
        /// <param name="data">消息数据</param>
        /// <param name="requestType">发送请求时的消息类型</param>
        private void HandleDirectMessage(Dictionary<string, object> data, BridgeMessageType requestType)
        {
            // 处理没有方法名的直接响应
            // 对于同步请求，原生平台返回的数据不包含"m"键
            // 根据请求类型直接路由到对应的处理器
            if (requestType != BridgeMessageType.None && _messageHandlers.ContainsKey(requestType))
            {
                if (_settings != null)
                {
                    _settings.LogDebug($"Processing direct response for {requestType}");
                }
                _messageHandlers[requestType]?.Invoke(data);
            }
            else if (_settings != null)
            {
                _settings.LogWarning($"Received direct message but no valid request type. Data: {JsonConvert.SerializeObject(data)}");
            }
        }

        #endregion

        #region Message Handlers

        private void HandleCommonParam(Dictionary<string, object> data)
        {
            Debug.Log($"HandleCommonParam called with data: {JsonConvert.SerializeObject(data)}");
            _commonParam = new CommonParamResponse();

            if (data.TryGetValue("language", out object language))
                _commonParam.language = language.ToString();
            if (data.TryGetValue("country", out object country))
                _commonParam.country = country.ToString();
            if (data.TryGetValue("numberGK", out object numberGK))
                _commonParam.numberGK = Convert.ToInt32(numberGK);

            _commonParamReturn = true;
            OnCommonParamReceived?.Invoke(_commonParam);

            if (_settings != null)
                _settings.LogDebug($"Common params received - Language: {_commonParam.language}, Country: {_commonParam.country}, NumberGK: {_commonParam.numberGK}");
        }

        private void HandleGetUnifyCurrency(Dictionary<string, object> data)
        {
            Debug.Log($"HandleGetUnifyCurrency called with data: {JsonConvert.SerializeObject(data)}");
            if (data.TryGetValue("amount", out object amount))
            {
                _currencySymbol = amount.ToString();
                _isUnifyCurrencyRef = true;
                OnCurrencySymbolReceived?.Invoke(_currencySymbol);

                if (_settings != null)
                    _settings.LogDebug($"Currency symbol received: {_currencySymbol}");
            }
        }

        private void HandleShowVideo(Dictionary<string, object> data)
        {
            OnVideoPlayEnd?.Invoke();
            if (_settings != null)
                _settings.LogDebug("Video play ended");
        }

        private void HandlePrivacyPolicy(Dictionary<string, object> data)
        {
            if (_settings != null)
                _settings.LogDebug("Privacy policy handled");
        }

        private void HandleTermsOfUse(Dictionary<string, object> data)
        {
            if (_settings != null)
                _settings.LogDebug("Terms of use handled");
        }

        private void HandlePlayMergeAudio(Dictionary<string, object> data)
        {
            if (_settings != null)
                _settings.LogDebug("Play merge audio handled");
        }

        private void HandleShowWithdraw(Dictionary<string, object> data)
        {
            if (_settings != null)
                _settings.LogDebug("Show withdraw handled");
        }

        private void HandleGetMergeReward(Dictionary<string, object> data)
        {
            if (_settings != null)
                _settings.LogDebug("Get merge reward handled");
        }

        private void HandleBuryPoint(Dictionary<string, object> data)
        {
            if (_settings != null)
                _settings.LogDebug("Bury point handled");
        }

        private void HandleRequestIsWhiteBao(Dictionary<string, object> data)
        {
            if (data.TryGetValue("amount", out object amount))
            {
                _isWhitePackage = amount.ToString() == "1";
                if (_settings != null)
                    _settings.LogDebug($"Is white package: {_isWhitePackage }");
            }
        }

        private void HandleFeedBack(Dictionary<string, object> data)
        {
            if (_settings != null)
                _settings.LogDebug("Feedback handled");
        }

        private void HandleShowPromotion(Dictionary<string, object> data)
        {
            if (_settings != null)
                _settings.LogDebug("Show promotion handled");
        }

        private void HandleEnterGame(Dictionary<string, object> data)
        {
            if (_settings != null)
                _settings.LogDebug("Enter game handled");
        }

        private void HandleUpdateLevel(Dictionary<string, object> data)
        {
            if (_settings != null)
                _settings.LogDebug("Update level handled");
        }

        private void HandleUserAmount(Dictionary<string, object> data)
        {
            if (_settings != null)
                _settings.LogDebug("User amount handled");
        }

        private void HandleAdReadyQuery(Dictionary<string, object> data)
        {
            // 广告查询结果处理
            if (_settings != null)
            {
                if (data != null && data.ContainsKey("amount"))
                {
                    _settings.LogDebug($"Ad ready query result - amount: {data["amount"]}");
                }
                else
                {
                    _settings.LogDebug("Ad ready query handled but no amount value found");
                }
            }
        }

        #endregion
    }
}