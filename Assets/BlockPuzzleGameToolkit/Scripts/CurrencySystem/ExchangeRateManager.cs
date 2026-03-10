// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

using UnityEngine;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using BlockPuzzleGameToolkit.Scripts.Localization;
using BlockPuzzle.NativeBridge;
using BlockPuzzle.NativeBridge.Models;

namespace BlockPuzzleGameToolkit.Scripts.CurrencySystem
{
    /// <summary>
    /// 汇率管理器（单例）
    /// 负责管理当前用户的货币设置和汇率转换
    /// </summary>
    public class ExchangeRateManager : SingletonBehaviour<ExchangeRateManager>
    {
        [Header("当前设置")]
        [SerializeField]
        private CurrencyType _currentDisplayCurrency = CurrencyType.USD;

        [SerializeField]
        private bool _autoDetectCurrency = true;

        [Header("调试信息")]
        [SerializeField]
        private string _detectedCountry = "";

        [SerializeField]
        private string _detectedLanguage = "";

        private bool _currencySetFromNative = false;

        /// <summary>
        /// 当前显示货币类型
        /// </summary>
        public CurrencyType CurrentDisplayCurrency
        {
            get => _currentDisplayCurrency;
            set
            {
                if (_currentDisplayCurrency != value)
                {
                    _currentDisplayCurrency = value;
                    SaveUserPreference();
                    OnCurrencyChanged?.Invoke(value);
                }
            }
        }

        /// <summary>
        /// 货币变更事件
        /// </summary>
        public event System.Action<CurrencyType> OnCurrencyChanged;

        public override void Awake()
        {
            base.Awake();
            Initialize();
        }

        private void Initialize()
        {
            // 尝试监听NativeBridge事件
            RegisterNativeBridgeEvents();

            // 监听语言变化事件
            LocalizationManager.OnLanguageChanged += OnLanguageChanged;

            // 加载用户偏好或自动检测
            LoadUserPreference();

            Debug.Log($"[ExchangeRateManager] 初始化完成，当前货币: {_currentDisplayCurrency}");
        }

        /// <summary>
        /// 注册原生桥接事件
        /// </summary>
        private void RegisterNativeBridgeEvents()
        {
            // 订阅通用参数接收事件（静态事件）
            NativeBridgeManager.OnCommonParamReceived += OnNativeCommonParamReceived;
            Debug.Log("[ExchangeRateManager] 已订阅NativeBridge事件");

            // 查找NativeBridgeManager实例
            var nativeBridge = NativeBridgeManager.Instance;
            if (nativeBridge != null)
            {
                // 如果已经有数据，立即处理
                var commonParam = nativeBridge.GetCommonParam();
                if (commonParam != null)
                {
                    OnNativeCommonParamReceived(commonParam);
                }
            }
            else
            {
                Debug.LogWarning("[ExchangeRateManager] NativeBridgeManager未找到，将使用系统语言检测");
            }
        }

        /// <summary>
        /// 处理从原生平台接收的通用参数
        /// </summary>
        private void OnNativeCommonParamReceived(CommonParamResponse param)
        {
            if (param == null)
            {
                Debug.LogWarning("[ExchangeRateManager] 收到空的CommonParam");
                return;
            }

            Debug.Log($"[ExchangeRateManager] 收到原生参数 - Country: {param.country}, Language: {param.language}");

            // 保存调试信息
            _detectedCountry = param.country;
            _detectedLanguage = param.language;

            // 如果已经设置过或用户已手动选择，不再自动切换
            if (_currencySetFromNative || !_autoDetectCurrency)
            {
                Debug.Log("[ExchangeRateManager] 货币已设置或自动检测已禁用，跳过");
                return;
            }

            // 根据国家和语言代码推荐货币
            CurrencyType recommendedCurrency = CountryCurrencyMapper.GetRecommendedCurrency(
                param.country,
                param.language
            );

            // 设置推荐的货币
            if (recommendedCurrency != _currentDisplayCurrency)
            {
                Debug.Log($"[ExchangeRateManager] 基于原生参数切换货币: {_currentDisplayCurrency} → {recommendedCurrency}");
                _currentDisplayCurrency = recommendedCurrency;
                _currencySetFromNative = true;
                SaveUserPreference();
                OnCurrencyChanged?.Invoke(recommendedCurrency);
            }
        }

        /// <summary>
        /// 加载用户货币偏好
        /// </summary>
        private void LoadUserPreference()
        {
            string savedCurrency = PlayerPrefs.GetString("UserCurrency", "");

            if (!string.IsNullOrEmpty(savedCurrency))
            {
                // 使用保存的货币
                if (System.Enum.TryParse<CurrencyType>(savedCurrency, out var currency))
                {
                    _currentDisplayCurrency = currency;
                    Debug.Log($"[ExchangeRateManager] 加载用户货币偏好: {currency}");
                }
            }
            else if (_autoDetectCurrency && !_currencySetFromNative)
            {
                // 如果没有原生数据，使用系统语言检测
                _currentDisplayCurrency = GetRecommendedCurrencyBySystemLanguage();
                Debug.Log($"[ExchangeRateManager] 基于系统语言自动检测货币: {_currentDisplayCurrency}");
                SaveUserPreference();
            }
        }

        /// <summary>
        /// 根据系统语言获取推荐货币
        /// </summary>
        private CurrencyType GetRecommendedCurrencyBySystemLanguage()
        {
            var language = Application.systemLanguage;

            // 使用新的 GameLanguage 枚举系统
            GameLanguage gameLanguage = GameLanguageHelper.FromSystemLanguage(language);
            return CountryCurrencyMapper.GetCurrencyByGameLanguage(gameLanguage);
        }

        /// <summary>
        /// 保存用户货币偏好
        /// </summary>
        private void SaveUserPreference()
        {
            PlayerPrefs.SetString("UserCurrency", _currentDisplayCurrency.ToString());
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 转换内部值到当前显示货币
        /// </summary>
        /// <param name="usdInternalValue">USD基准的内部值</param>
        /// <returns>当前货币的内部值</returns>
        public int ConvertToCurrentCurrency(int usdInternalValue)
        {
            if (_currentDisplayCurrency == CurrencyType.USD)
            {
                return usdInternalValue;
            }

            return CurrencyInfo.ConvertInternalValue(usdInternalValue, CurrencyType.USD, _currentDisplayCurrency);
        }

        /// <summary>
        /// 从当前显示货币转换回USD内部值
        /// </summary>
        /// <param name="currentCurrencyValue">当前货币的内部值</param>
        /// <returns>USD基准的内部值</returns>
        public int ConvertFromCurrentCurrency(int currentCurrencyValue)
        {
            if (_currentDisplayCurrency == CurrencyType.USD)
            {
                return currentCurrencyValue;
            }

            return CurrencyInfo.ConvertInternalValue(currentCurrencyValue, _currentDisplayCurrency, CurrencyType.USD);
        }

        /// <summary>
        /// 格式化当前货币显示
        /// </summary>
        /// <param name="usdInternalValue">USD基准的内部值</param>
        /// <returns>格式化的货币字符串</returns>
        public string FormatCurrentCurrency(int usdInternalValue)
        {
            return CurrencyFormatter.FormatCurrencyWithType(usdInternalValue, _currentDisplayCurrency, true);
        }

        /// <summary>
        /// 获取当前货币信息
        /// </summary>
        public CurrencyInfo GetCurrentCurrencyInfo()
        {
            return CurrencyInfo.CreateDefault(_currentDisplayCurrency);
        }

        /// <summary>
        /// 获取汇率（从USD到当前货币）
        /// </summary>
        public float GetCurrentExchangeRate()
        {
            if (_currentDisplayCurrency == CurrencyType.USD)
            {
                return 1f;
            }

            var info = GetCurrentCurrencyInfo();
            return info.exchangeRate;
        }

        /// <summary>
        /// 切换到指定货币
        /// </summary>
        public void SwitchToCurrency(CurrencyType newCurrency)
        {
            CurrentDisplayCurrency = newCurrency;
            Debug.Log($"[ExchangeRateManager] 切换到货币: {newCurrency}");
        }

        /// <summary>
        /// 根据国家代码设置货币
        /// </summary>
        public void SetCurrencyByCountryCode(string countryCode)
        {
            if (string.IsNullOrEmpty(countryCode))
            {
                Debug.LogWarning("[ExchangeRateManager] 国家代码为空");
                return;
            }

            CurrencyType currency = CountryCurrencyMapper.GetCurrencyByCountryCode(countryCode);
            if (currency != _currentDisplayCurrency)
            {
                Debug.Log($"[ExchangeRateManager] 根据国家代码 {countryCode} 设置货币: {currency}");
                SwitchToCurrency(currency);
            }
        }

        /// <summary>
        /// 获取所有支持的货币列表
        /// </summary>
        public CurrencyType[] GetSupportedCurrencies()
        {
            return (CurrencyType[])System.Enum.GetValues(typeof(CurrencyType));
        }

        /// <summary>
        /// 重置为自动检测
        /// </summary>
        public void ResetToAutoDetect()
        {
            _currencySetFromNative = false;
            PlayerPrefs.DeleteKey("UserCurrency");
            PlayerPrefs.Save();

            // 重新检测
            var nativeBridge = NativeBridgeManager.Instance;
            if (nativeBridge != null)
            {
                var commonParam = nativeBridge.GetCommonParam();
                if (commonParam != null)
                {
                    OnNativeCommonParamReceived(commonParam);
                }
                else
                {
                    _currentDisplayCurrency = GetRecommendedCurrencyByGameLanguage();
                    OnCurrencyChanged?.Invoke(_currentDisplayCurrency);
                }
            }
            else
            {
                _currentDisplayCurrency = GetRecommendedCurrencyByGameLanguage();
                OnCurrencyChanged?.Invoke(_currentDisplayCurrency);
            }
        }

        /// <summary>
        /// 处理语言变化事件
        /// </summary>
        private void OnLanguageChanged()
        {
            Debug.Log("[ExchangeRateManager] 检测到语言变化，更新显示货币类型");

            // 获取当前语言
            SystemLanguage currentLanguage = LocalizationManager.GetCurrentLanguage();

            // 转换为 GameLanguage
            GameLanguage gameLanguage = GameLanguageHelper.FromSystemLanguage(currentLanguage);

            // 获取推荐的货币类型
            CurrencyType recommendedCurrency = CountryCurrencyMapper.GetCurrencyByGameLanguage(gameLanguage);

            // 检查是否需要更新货币
            if (recommendedCurrency != _currentDisplayCurrency)
            {
                Debug.Log($"[ExchangeRateManager] 基于语言变化切换货币: {_currentDisplayCurrency} → {recommendedCurrency}");
                CurrentDisplayCurrency = recommendedCurrency;
                // CurrentDisplayCurrency 的 setter 会触发 OnCurrencyChanged 事件，TopPanel 会自动响应
            }
            else
            {
                Debug.Log($"[ExchangeRateManager] 货币类型已正确: {_currentDisplayCurrency}");
            }
        }

        /// <summary>
        /// 根据 GameLanguage 设置货币
        /// </summary>
        public void SetCurrencyByGameLanguage(GameLanguage language, bool savePreference = true)
        {
            CurrencyType recommendedCurrency = CountryCurrencyMapper.GetCurrencyByGameLanguage(language);

            if (recommendedCurrency != _currentDisplayCurrency)
            {
                Debug.Log($"[ExchangeRateManager] 设置货币基于语言 {language}: {_currentDisplayCurrency} → {recommendedCurrency}");
                _currentDisplayCurrency = recommendedCurrency;

                if (savePreference)
                {
                    SaveUserPreference();
                }

                OnCurrencyChanged?.Invoke(recommendedCurrency);
            }
        }

        /// <summary>
        /// 手动同步货币与当前语言设置
        /// 这个方法可以在 Inspector 中通过按钮调用来强制同步
        /// </summary>
        [ContextMenu("同步货币与当前语言")]
        public void SyncCurrencyWithCurrentLanguage()
        {
            Debug.Log("[ExchangeRateManager] 手动同步货币与当前语言");

            // 获取当前语言
            SystemLanguage currentLanguage = LocalizationManager.GetCurrentLanguage();
            Debug.Log($"[ExchangeRateManager] 当前语言: {currentLanguage}");

            // 转换为 GameLanguage
            GameLanguage gameLanguage = GameLanguageHelper.FromSystemLanguage(currentLanguage);

            // 获取推荐的货币类型
            CurrencyType recommendedCurrency = CountryCurrencyMapper.GetCurrencyByGameLanguage(gameLanguage);
            Debug.Log($"[ExchangeRateManager] 推荐货币: {recommendedCurrency}");

            // 强制设置货币
            if (recommendedCurrency != _currentDisplayCurrency)
            {
                Debug.Log($"[ExchangeRateManager] 强制切换货币: {_currentDisplayCurrency} → {recommendedCurrency}");
                _currentDisplayCurrency = recommendedCurrency;
                SaveUserPreference();
                OnCurrencyChanged?.Invoke(recommendedCurrency);
                // OnCurrencyChanged 事件会通知所有监听者（包括 TopPanel）更新显示
            }
            else
            {
                Debug.Log($"[ExchangeRateManager] 货币已正确设置为: {_currentDisplayCurrency}");
            }
        }

        /// <summary>
        /// 根据语言代码和国家代码智能设置货币
        /// </summary>
        public void SetCurrencyByLocale(string languageCode, string countryCode)
        {
            CurrencyType recommendedCurrency = CountryCurrencyMapper.GetRecommendedCurrency(countryCode, languageCode);

            if (recommendedCurrency != _currentDisplayCurrency)
            {
                Debug.Log($"[ExchangeRateManager] 设置货币基于地区 ({languageCode}/{countryCode}): {_currentDisplayCurrency} → {recommendedCurrency}");
                CurrentDisplayCurrency = recommendedCurrency;
            }
        }

        /// <summary>
        /// 获取GameLanguage对应的货币推荐
        /// </summary>
        private CurrencyType GetRecommendedCurrencyByGameLanguage()
        {
            // 尝试从LocalizationManager获取当前语言
            if (LocalizationManager.Instance != null)
            {
                SystemLanguage systemLang = LocalizationManager.GetCurrentLanguage();
                GameLanguage gameLang = GameLanguageHelper.FromSystemLanguage(systemLang);
                return CountryCurrencyMapper.GetCurrencyByGameLanguage(gameLang);
            }

            // 否则使用系统语言
            GameLanguage currentGameLang = GameLanguageHelper.FromSystemLanguage(Application.systemLanguage);
            return CountryCurrencyMapper.GetCurrencyByGameLanguage(currentGameLang);
        }

        /// <summary>
        /// 获取当前GameLanguage
        /// </summary>
        public GameLanguage GetCurrentGameLanguage()
        {
            if (LocalizationManager.Instance != null)
            {
                SystemLanguage systemLang = LocalizationManager.GetCurrentLanguage();
                return GameLanguageHelper.FromSystemLanguage(systemLang);
            }

            return GameLanguageHelper.FromSystemLanguage(Application.systemLanguage);
        }

        /// <summary>
        /// 获取货币的本地化显示名称
        /// </summary>
        public string GetCurrencyLocalizedName(CurrencyType currency)
        {
            var info = CurrencyInfo.CreateDefault(currency);
            return info.name;
        }

        /// <summary>
        /// 批量转换价格
        /// </summary>
        /// <param name="usdPrices">USD价格数组</param>
        /// <returns>当前货币价格数组</returns>
        public int[] ConvertPricesToCurrentCurrency(int[] usdPrices)
        {
            if (usdPrices == null || usdPrices.Length == 0)
                return usdPrices;

            int[] convertedPrices = new int[usdPrices.Length];
            for (int i = 0; i < usdPrices.Length; i++)
            {
                convertedPrices[i] = ConvertToCurrentCurrency(usdPrices[i]);
            }
            return convertedPrices;
        }

        private void OnDestroy()
        {
            // 取消订阅静态事件
            NativeBridgeManager.OnCommonParamReceived -= OnNativeCommonParamReceived;

            // 取消监听语言变化事件
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.OnLanguageChanged -= OnLanguageChanged;
            }
        }

#if UNITY_EDITOR
        [ContextMenu("测试 - 设置国家为美国")]
        private void TestSetCountryUSA()
        {
            SetCurrencyByCountryCode("US");
        }

        [ContextMenu("测试 - 设置国家为日本")]
        private void TestSetCountryJapan()
        {
            SetCurrencyByCountryCode("JP");
        }

        [ContextMenu("测试 - 设置国家为德国")]
        private void TestSetCountryGermany()
        {
            SetCurrencyByCountryCode("DE");
        }

        [ContextMenu("测试 - 设置国家为巴西")]
        private void TestSetCountryBrazil()
        {
            SetCurrencyByCountryCode("BR");
        }

        [ContextMenu("测试 - 设置国家为印度")]
        private void TestSetCountryIndia()
        {
            SetCurrencyByCountryCode("IN");
        }

        [ContextMenu("测试 - 设置语言为Hindi")]
        private void TestSetLanguageHindi()
        {
            SetCurrencyByGameLanguage(GameLanguage.Hindi);
        }

        [ContextMenu("测试 - 设置语言为Malay")]
        private void TestSetLanguageMalay()
        {
            SetCurrencyByGameLanguage(GameLanguage.Malay);
        }

        [ContextMenu("测试 - 设置语言为Filipino")]
        private void TestSetLanguageFilipino()
        {
            SetCurrencyByGameLanguage(GameLanguage.Filipino);
        }

        [ContextMenu("测试 - 设置语言为中文简体")]
        private void TestSetLanguageChinese()
        {
            SetCurrencyByGameLanguage(GameLanguage.Chinese);
        }

        [ContextMenu("测试 - 设置语言为中文繁体")]
        private void TestSetLanguageChineseTraditional()
        {
            SetCurrencyByGameLanguage(GameLanguage.ChineseTraditional);
        }

        [ContextMenu("测试 - 显示所有货币信息")]
        private void TestShowAllCurrencies()
        {
            Debug.Log("===== 所有支持的货币 =====");
            foreach (CurrencyType currency in GetSupportedCurrencies())
            {
                var info = CurrencyInfo.CreateDefault(currency);
                Debug.Log($"{currency}: {info.symbol} {info.name} (汇率: 1 USD = {info.exchangeRate} {info.code})");
            }
        }

        [ContextMenu("测试 - 显示所有语言映射")]
        private void TestShowAllLanguageMappings()
        {
            Debug.Log("===== 所有语言到货币的映射 =====");
            foreach (GameLanguage language in System.Enum.GetValues(typeof(GameLanguage)))
            {
                var languageInfo = GameLanguageHelper.GetLanguageInfo(language);
                var currency = CountryCurrencyMapper.GetCurrencyByGameLanguage(language);
                Debug.Log($"{language} ({languageInfo.nativeName}/{languageInfo.englishName}) -> {currency}");
            }
        }

        [ContextMenu("测试 - 转换100美元到所有货币")]
        private void TestConvert100USD()
        {
            int usdValue = 1000000; // 100美元的内部值
            Debug.Log("===== 100美元转换到各种货币 =====");

            foreach (CurrencyType currency in GetSupportedCurrencies())
            {
                int converted = CurrencyInfo.ConvertInternalValue(usdValue, CurrencyType.USD, currency);
                string formatted = CurrencyFormatter.FormatCurrencyWithType(usdValue, currency, true);
                Debug.Log($"100 USD = {formatted}");
            }
        }

        [ContextMenu("测试 - 模拟原生参数（美国）")]
        private void TestSimulateNativeUSA()
        {
            var param = new CommonParamResponse
            {
                country = "US",
                language = "en"
            };
            OnNativeCommonParamReceived(param);
        }

        [ContextMenu("测试 - 模拟原生参数（日本）")]
        private void TestSimulateNativeJapan()
        {
            var param = new CommonParamResponse
            {
                country = "JP",
                language = "ja"
            };
            OnNativeCommonParamReceived(param);
        }

        [ContextMenu("测试 - 模拟原生参数（印度）")]
        private void TestSimulateNativeIndia()
        {
            var param = new CommonParamResponse
            {
                country = "IN",
                language = "hi"
            };
            OnNativeCommonParamReceived(param);
        }

        [ContextMenu("测试 - 模拟原生参数（马来西亚）")]
        private void TestSimulateNativeMalaysia()
        {
            var param = new CommonParamResponse
            {
                country = "MY",
                language = "ms"
            };
            OnNativeCommonParamReceived(param);
        }

        [ContextMenu("测试 - 模拟原生参数（菲律宾）")]
        private void TestSimulateNativePhilippines()
        {
            var param = new CommonParamResponse
            {
                country = "PH",
                language = "fil"
            };
            OnNativeCommonParamReceived(param);
        }

        [ContextMenu("测试 - 切换到欧元")]
        private void TestSwitchToEUR()
        {
            SwitchToCurrency(CurrencyType.EUR);
        }

        [ContextMenu("测试 - 重置为自动检测")]
        private void TestResetToAutoDetect()
        {
            ResetToAutoDetect();
        }

        [ContextMenu("测试 - 切换到日元")]
        private void TestSwitchToJPY()
        {
            SwitchToCurrency(CurrencyType.JPY);
        }

        [ContextMenu("测试 - 切换到韩元")]
        private void TestSwitchToKRW()
        {
            SwitchToCurrency(CurrencyType.KRW);
        }

        [ContextMenu("打印当前汇率信息")]
        private void PrintExchangeRateInfo()
        {
            var info = GetCurrentCurrencyInfo();
            Debug.Log($"当前货币: {info.name} ({info.code})");
            Debug.Log($"符号: {info.symbol}");
            Debug.Log($"汇率: 1 USD = {info.exchangeRate} {info.code}");
            Debug.Log($"小数位数: {info.decimalPlaces}");
            Debug.Log($"检测的国家: {_detectedCountry}");
            Debug.Log($"检测的语言: {_detectedLanguage}");

            // 测试转换
            int testValue = 1000000; // 100 USD
            int converted = ConvertToCurrentCurrency(testValue);
            string formatted = FormatCurrentCurrency(testValue);
            Debug.Log($"100 USD = {formatted}");
        }
#endif
    }
}