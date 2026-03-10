// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

using System;
using UnityEngine;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using BlockPuzzleGameToolkit.Scripts.Localization;
using BlockPuzzle.NativeBridge;
using StorageSystem.Core;
using StorageSystem.Data;
using Formatter = BlockPuzzleGameToolkit.Scripts.CurrencySystem.CurrencyFormatter;

namespace BlockPuzzleGameToolkit.Scripts.CurrencySystem
{
    /// <summary>
    /// 金币管理器（整数版）
    /// 内部以整数形式存储和操作金币（放大10000倍）
    /// 例如：300美元存储为3000000（300 * 10000）
    /// </summary>
    public class CurrencyManager : SingletonBehaviour<CurrencyManager>
    {
        private const string SAVE_KEY = "currency_data";
        private CurrencySaveData data;
        private StorageOptions storageOptions;
        private CurrencyType currentCurrencyType;  // 当前货币类型

        // 事件（使用整数值）
        public event Action<int> OnCoinsChanged;
        public event Action<int> OnCoinsAdded;
        public event Action<int> OnCoinsSpent;

        // 属性（返回内部整数值）
        public int CurrentCoins => data?.CoinsInt ?? 0;
        // IsInitialized 属性已经在基类 SingletonBehaviour 中定义，无需重复定义
        public CurrencyType CurrentCurrencyType => currentCurrencyType;  // 公开当前货币类型

        // 初始化优先级（在StorageManager之后）
        public override int InitPriority => 10;

        public override void Awake()
        {
            base.Awake();
            // 不再在Awake中初始化，等待SingletonInitializer调用OnInit
        }

        /// <summary>
        /// 单例初始化入口，由SingletonInitializer调用
        /// </summary>
        public override void OnInit()
        {
            if (!IsInitialized)
            {
                Initialize();
            }
            base.OnInit(); // 设置SingletonBehaviour的IsInitialized = true
        }

        private void Start()
        {
            // 备用初始化，如果SingletonInitializer没有调用
            if (!IsInitialized)
            {
                Initialize();
            }
        }

        private void Initialize()
        {
            // 配置存储选项
            storageOptions = new StorageOptions
            {
                useEncryption = false,      // 不使用加密
                useCompression = false,     // 数据量小，不需要压缩
                addChecksum = true,         // 数据完整性验证
                version = 1,
                encryptionKey = "Currency_" + Application.identifier
            };

            // 从NativeBridgeManager获取语言和国家信息
            InitializeCurrencyType();

            LoadData();

            // 监听语言变化事件
            LocalizationManager.OnLanguageChanged += OnLanguageChanged;

            // IsInitialized 会在基类的 OnInit 中设置，无需在此重复设置
            Debug.Log($"[CurrencyManager] 初始化完成，当前金币: {Formatter.FormatCurrency(CurrentCoins)} (内部值: {CurrentCoins})");
        }

        /// <summary>
        /// 初始化货币类型
        /// </summary>
        private void InitializeCurrencyType()
        {
            string languageCode = null;
            string countryCode = null;

            // 从NativeBridgeManager获取
            if (NativeBridgeManager.Instance != null)
            {
                var commonParam = NativeBridgeManager.Instance.GetCommonParam();
                if (commonParam != null)
                {
                    languageCode = commonParam.language;
                    countryCode = commonParam.country?.ToUpperInvariant();
                    Debug.Log($"[CurrencyManager] 从NativeBridge获取: language={languageCode}, country={countryCode}");
                }
            }

            // 如果NativeBridge没有提供，使用系统语言作为备用
            if (string.IsNullOrEmpty(languageCode) && string.IsNullOrEmpty(countryCode))
            {
                languageCode = Application.systemLanguage.ToString();
                Debug.Log($"[CurrencyManager] 使用系统语言作为备用: {languageCode}");
            }

            // 获取推荐的货币类型
            currentCurrencyType = CountryCurrencyMapper.GetRecommendedCurrency(countryCode, languageCode);
            Debug.Log($"[CurrencyManager] 使用货币类型: {currentCurrencyType}");
        }

        /// <summary>
        /// 处理语言变化事件
        /// </summary>
        private void OnLanguageChanged()
        {
            Debug.Log("[CurrencyManager] 检测到语言变化，更新货币类型");

            // 获取当前语言
            SystemLanguage currentLanguage = LocalizationManager.GetCurrentLanguage();
            string languageCode = GetLanguageCode(currentLanguage);
            string countryCode = GetCountryCodeFromLanguage(currentLanguage);

            // 更新货币类型
            CurrencyType newCurrencyType = CountryCurrencyMapper.GetRecommendedCurrency(countryCode, languageCode);

            if (newCurrencyType != currentCurrencyType)
            {
                currentCurrencyType = newCurrencyType;
                Debug.Log($"[CurrencyManager] 货币类型已更新为: {currentCurrencyType}");

                // 触发货币变化事件，UI会自动刷新
                OnCoinsChanged?.Invoke(CurrentCoins);
            }
        }

        /// <summary>
        /// 将SystemLanguage转换为语言代码
        /// </summary>
        private string GetLanguageCode(SystemLanguage language)
        {
            switch (language)
            {
                case SystemLanguage.Czech: return "cs";
                case SystemLanguage.Dutch: return "nl";
                case SystemLanguage.English: return "en";
                case SystemLanguage.French: return "fr";
                case SystemLanguage.German: return "de";
                case SystemLanguage.Italian: return "it";
                case SystemLanguage.Japanese: return "ja";
                case SystemLanguage.Korean: return "ko";
                case SystemLanguage.Polish: return "pl";
                case SystemLanguage.Portuguese: return "pt";
                case SystemLanguage.Romanian: return "ro";
                case SystemLanguage.Russian: return "ru";
                case SystemLanguage.Spanish: return "es";
                case SystemLanguage.Thai: return "th";
                case SystemLanguage.Turkish: return "tr";
                case SystemLanguage.Vietnamese: return "vi";
                case SystemLanguage.Indonesian: return "id";
                case SystemLanguage.Chinese: return "zh";  // 中文
                case SystemLanguage.ChineseSimplified: return "zh";  // 简体中文
                case SystemLanguage.ChineseTraditional: return "zh";  // 繁体中文
                // 注意：Unity的SystemLanguage没有Hindi、Malay、Filipino等
                // 这些语言需要通过国家代码来识别
                default: return "en";
            }
        }

        /// <summary>
        /// 根据语言获取对应的国家代码
        /// </summary>
        private string GetCountryCodeFromLanguage(SystemLanguage language)
        {
            switch (language)
            {
                case SystemLanguage.Czech: return "CZ";
                case SystemLanguage.Dutch: return "NL";
                case SystemLanguage.English: return "US";
                case SystemLanguage.French: return "FR";
                case SystemLanguage.German: return "DE";
                case SystemLanguage.Italian: return "IT";
                case SystemLanguage.Japanese: return "JP";
                case SystemLanguage.Korean: return "KR";
                case SystemLanguage.Polish: return "PL";
                case SystemLanguage.Portuguese: return "BR";  // 默认巴西
                case SystemLanguage.Romanian: return "RO";
                case SystemLanguage.Russian: return "RU";
                case SystemLanguage.Spanish: return "ES";
                case SystemLanguage.Thai: return "TH";
                case SystemLanguage.Turkish: return "TR";
                case SystemLanguage.Vietnamese: return "VN";
                case SystemLanguage.Indonesian: return "ID";
                case SystemLanguage.Chinese: return "CN";  // 中国
                case SystemLanguage.ChineseSimplified: return "CN";  // 中国
                case SystemLanguage.ChineseTraditional: return "TW";  // 台湾
                // 注意：对于Hindi、Malay、Filipino等语言，
                // 需要在实际应用中通过NativeBridge获取准确的国家代码
                default: return "US";
            }
        }

        /// <summary>
        /// 清理事件监听
        /// </summary>

        #region 公开接口

        /// <summary>
        /// 获取当前金币（内部整数值）
        /// </summary>
        /// <returns>放大10000倍的整数值</returns>
        public int GetCoins()
        {
            return data?.CoinsInt ?? 0;
        }

        /// <summary>
        /// 获取显示用的金币值（美元）
        /// </summary>
        /// <returns>实际美元值</returns>
        public float GetDisplayCoins()
        {
            return Formatter.ToDisplayValue(GetCoins());
        }

        /// <summary>
        /// 获取格式化的货币字符串
        /// </summary>
        /// <param name="includeSymbol">是否包含货币符号</param>
        /// <returns>格式化的字符串</returns>
        public string GetFormattedCoins(bool includeSymbol = true)
        {
            int coins = GetCoins();

            // 白包模式下强制不显示货币符号
            bool isWhitePackage = false;
            if (BlockPuzzle.NativeBridge.NativeBridgeManager.Instance != null)
            {
                isWhitePackage = BlockPuzzle.NativeBridge.NativeBridgeManager.Instance.IsWhitePackage();
            }

            // 白包模式下忽略includeSymbol参数，强制不显示符号
            if (isWhitePackage)
            {
                return Formatter.FormatValue(coins);
            }

            // 标准模式下根据参数决定
            return includeSymbol
                ? Formatter.FormatCurrency(coins)
                : Formatter.FormatValue(coins);
        }

        /// <summary>
        /// 格式化指定的货币值（使用当前货币类型）
        /// </summary>
        /// <param name="internalValue">内部整数值（放大10000倍）</param>
        /// <param name="useExchangeRate">是否应用汇率默认应用</param>
        /// <returns>格式化的货币字符串</returns>
        public string FormatCurrency(int internalValue, bool useExchangeRate = true)
        {
            // 白包模式下不显示货币符号
            bool isWhitePackage = false;
            if (BlockPuzzle.NativeBridge.NativeBridgeManager.Instance != null)
            {
                isWhitePackage = BlockPuzzle.NativeBridge.NativeBridgeManager.Instance.IsWhitePackage();
            }

            if (isWhitePackage)
            {
                // 白包模式：只返回数值，不带货币符号
                return Formatter.FormatValue(internalValue);
            }
            else
            {
                // 标准模式：返回带货币符号的格式
                return Formatter.FormatCurrencyWithType(internalValue, currentCurrencyType, useExchangeRate);
            }
        }

        /// <summary>
        /// 添加金币
        /// </summary>
        /// <param name="amount">金额（放大10000倍的整数值）</param>
        /// <returns>是否成功</returns>
        public bool AddCoins(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"[CurrencyManager] 尝试添加无效金额: {amount}");
                return false;
            }

            if (data == null)
            {
                Debug.LogError("[CurrencyManager] 数据未初始化");
                return false;
            }

            int oldAmount = data.CoinsInt;

            // 检查整数溢出
            if (int.MaxValue - data.CoinsInt < amount)
            {
                Debug.LogError($"[CurrencyManager] 金币数量将溢出，操作被拒绝");
                return false;
            }

            data.CoinsInt += amount;

            if (SaveData())
            {
                Debug.Log($"[CurrencyManager] 添加金币: {Formatter.FormatCurrency(amount)}, 当前总计: {Formatter.FormatCurrency(data.CoinsInt)}");
                OnCoinsAdded?.Invoke(amount);
                OnCoinsChanged?.Invoke(data.CoinsInt);
                ReportCashBuryPoint(data.CoinsInt);  // 上报埋点
                return true;
            }
            else
            {
                // 回滚
                data.CoinsInt = oldAmount;
                Debug.LogError("[CurrencyManager] 保存失败，金币添加已回滚");
                return false;
            }
        }

        /// <summary>
        /// 消耗金币
        /// </summary>
        /// <param name="amount">金额（放大10000倍的整数值）</param>
        /// <returns>是否成功</returns>
        public bool SpendCoins(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"[CurrencyManager] 尝试消费无效金额: {amount}");
                return false;
            }

            if (data == null)
            {
                Debug.LogError("[CurrencyManager] 数据未初始化");
                return false;
            }

            if (data.CoinsInt < amount)
            {
                Debug.LogWarning($"[CurrencyManager] 金币不足: 需要 {Formatter.FormatCurrency(amount)}, 当前 {Formatter.FormatCurrency(data.CoinsInt)}");
                return false;
            }

            int oldAmount = data.CoinsInt;
            data.CoinsInt -= amount;

            if (SaveData())
            {
                Debug.Log($"[CurrencyManager] 消费金币: {Formatter.FormatCurrency(amount)}, 剩余: {Formatter.FormatCurrency(data.CoinsInt)}");
                OnCoinsSpent?.Invoke(amount);
                OnCoinsChanged?.Invoke(data.CoinsInt);
                ReportCashBuryPoint(data.CoinsInt);  // 上报埋点
                return true;
            }
            else
            {
                // 回滚
                data.CoinsInt = oldAmount;
                Debug.LogError("[CurrencyManager] 保存失败，金币消费已回滚");
                return false;
            }
        }

        /// <summary>
        /// 设置金币（仅用于调试或初始化）
        /// </summary>
        /// <param name="amount">金额（放大10000倍的整数值）</param>
        public void SetCoins(int amount)
        {
            if (data == null)
            {
                Debug.LogError("[CurrencyManager] 数据未初始化");
                return;
            }

            amount = Mathf.Max(0, amount);
            data.CoinsInt = amount;

            if (SaveData())
            {
                Debug.Log($"[CurrencyManager] 设置金币: {Formatter.FormatCurrency(amount)}");
                OnCoinsChanged?.Invoke(data.CoinsInt);
                ReportCashBuryPoint(data.CoinsInt);  // 上报埋点
            }
        }

        /// <summary>
        /// 检查是否有足够的金币
        /// </summary>
        /// <param name="amount">需要的金额（放大10000倍的整数值）</param>
        public bool HasEnoughCoins(int amount)
        {
            return data != null && data.CoinsInt >= amount;
        }

        /// <summary>
        /// 上报货币变化埋点
        /// </summary>
        /// <param name="newAmount">新的货币值（放大10000倍的整数值）</param>
        private void ReportCashBuryPoint(int newAmount)
        {
            // 埋点上报：Cash (货币变化时上报)
            if (BlockPuzzle.NativeBridge.NativeBridgeManager.Instance != null)
            {
                // p2是除以放大倍数后的数值
                float displayValue = newAmount / 10000f;
                BlockPuzzle.NativeBridge.NativeBridgeManager.Instance.SendMessageToPlatform(
                    BlockPuzzle.NativeBridge.Enums.BridgeMessageType.BuryPoint,
                    "Cash",
                    displayValue.ToString("F4")
                );
                Debug.Log($"[CurrencyManager] 埋点上报：Cash = {displayValue:F4}");
            }
        }

        /// <summary>
        /// 重置金币为0
        /// </summary>
        public void Reset()
        {
            if (data == null)
            {
                Debug.LogError("[CurrencyManager] 数据未初始化");
                return;
            }

            data.Reset();

            if (SaveData())
            {
                Debug.Log("[CurrencyManager] 金币已重置");
                OnCoinsChanged?.Invoke(0);
            }
        }

        /// <summary>
        /// 强制重新加载数据
        /// </summary>
        public void ReloadData()
        {
            LoadData();
            OnCoinsChanged?.Invoke(data?.CoinsInt ?? 0);
        }

        #endregion

        #region 存储操作

        private void LoadData()
        {
            try
            {
                data = StorageManager.Instance.Load<CurrencySaveData>(
                    SAVE_KEY,
                    StorageType.Binary
                );

                if (data != null)
                {
                    // 验证数据完整性
                    if (!data.ValidateChecksum())
                    {
                        Debug.LogWarning("[CurrencyManager] 数据校验失败，使用默认数据");
                        data = CurrencySaveData.CreateDefault();
                        SaveData();
                    }
                    else if (!data.IsValid())
                    {
                        Debug.LogWarning("[CurrencyManager] 数据无效，重置为默认");
                        data = CurrencySaveData.CreateDefault();
                        SaveData();
                    }
                }
                else
                {
                    // 首次运行，创建默认数据
                    data = CurrencySaveData.CreateDefault();
                    SaveData();
                }

                Debug.Log($"[CurrencyManager] 数据加载成功: {data}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[CurrencyManager] 加载数据失败: {e.Message}");

                // 尝试删除损坏的存档文件
                try
                {
                    if (StorageManager.Instance != null)
                    {
                        StorageManager.Instance.Delete(SAVE_KEY, StorageType.Binary);
                        Debug.Log("[CurrencyManager] 已删除损坏的存档文件");
                    }
                }
                catch (Exception deleteEx)
                {
                    Debug.LogWarning($"[CurrencyManager] 删除损坏存档失败: {deleteEx.Message}");
                }

                // 创建并保存默认数据
                data = CurrencySaveData.CreateDefault();
                SaveData();
            }
        }

        private bool SaveData()
        {
            if (data == null)
            {
                Debug.LogError("[CurrencyManager] 无数据可保存");
                return false;
            }

            try
            {
                data.UpdateMetadata(storageOptions.version);

                bool success = StorageManager.Instance.Save(
                    SAVE_KEY,
                    data,
                    StorageType.Binary,
                    storageOptions
                );

                if (success)
                {
                    Debug.Log($"[CurrencyManager] 数据保存成功: {data}");
                }
                else
                {
                    Debug.LogError("[CurrencyManager] 数据保存失败");
                }

                return success;
            }
            catch (Exception e)
            {
                Debug.LogError($"[CurrencyManager] 保存数据异常: {e.Message}");
                return false;
            }
        }

        #endregion

        #region Unity生命周期

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && IsInitialized)
            {
                SaveData(); // 暂停时保存
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && IsInitialized)
            {
                SaveData(); // 失去焦点时保存
            }
        }

        private void OnDestroy()
        {
            // 取消监听语言变化事件
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.OnLanguageChanged -= OnLanguageChanged;
            }

            // 在应用退出时不要保存，避免访问已销毁的单例
            if (IsInitialized && !isApplicationQuitting)
            {
                SaveData(); // 销毁时保存
            }
        }

        private bool isApplicationQuitting = false;

        private void OnApplicationQuit()
        {
            isApplicationQuitting = true;
            // 应用退出时最后保存一次
            if (IsInitialized)
            {
                SaveDataSafely();
            }
        }

        /// <summary>
        /// 安全保存数据（不创建新的单例实例）
        /// </summary>
        private void SaveDataSafely()
        {
            // 先检查StorageManager实例是否存在
            var storageManager = FindObjectOfType<StorageManager>();
            if (storageManager != null && data != null)
            {
                try
                {
                    data.UpdateMetadata(storageOptions.version);
                    storageManager.Save(
                        SAVE_KEY,
                        data,
                        StorageType.Binary,
                        storageOptions
                    );
                    Debug.Log("[CurrencyManager] 应用退出时数据保存成功");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[CurrencyManager] 应用退出时保存数据异常: {e.Message}");
                }
            }
        }

        #endregion

        #region 调试功能

#if UNITY_EDITOR
        [ContextMenu("添加100美元")]
        private void Debug_Add100()
        {
            // 100美元 = 100 * 10000 = 1000000
            AddCoins(1000000);
        }

        [ContextMenu("消费50美元")]
        private void Debug_Spend50()
        {
            // 50美元 = 50 * 10000 = 500000
            SpendCoins(500000);
        }

        [ContextMenu("重置金币")]
        private void Debug_Reset()
        {
            Reset();
        }

        [ContextMenu("打印当前状态")]
        private void Debug_PrintStatus()
        {
            Debug.Log($"[CurrencyManager] 当前状态:\n" +
                     $"- 内部值: {GetCoins()}\n" +
                     $"- 显示值: ${GetDisplayCoins():F3}\n" +
                     $"- 格式化: {GetFormattedCoins()}\n" +
                     $"- 初始化: {IsInitialized}");
        }

        [ContextMenu("添加0.001美元")]
        private void Debug_AddSmall()
        {
            // 0.001美元 = 0.001 * 10000 = 10
            AddCoins(10);
        }
#endif

        #endregion
    }
}