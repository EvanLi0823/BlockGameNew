// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

using System.Collections.Generic;
using UnityEngine;
using BlockPuzzleGameToolkit.Scripts.Localization;

namespace BlockPuzzleGameToolkit.Scripts.CurrencySystem
{
    /// <summary>
    /// 国家/地区代码到货币映射工具（精简版）
    /// 根据ISO 3166-1 alpha-2国家代码映射到对应的货币
    /// </summary>
    public static class CountryCurrencyMapper
    {
        /// <summary>
        /// 国家代码到货币类型的映射表
        /// 基于支持的语言对应的主要国家
        /// </summary>
        private static readonly Dictionary<string, CurrencyType> CountryToCurrencyMap = new Dictionary<string, CurrencyType>
        {
            // 英语国家（English）
            { "US", CurrencyType.USD }, // 美国
            { "GB", CurrencyType.USD }, // 英国（使用USD作为默认）
            { "UK", CurrencyType.USD }, // 英国（备用代码）

            // 欧洲国家（使用欧元）
            { "NL", CurrencyType.EUR }, // 荷兰 (Dutch)
            { "FR", CurrencyType.EUR }, // 法国 (French)
            { "DE", CurrencyType.EUR }, // 德国 (German)
            { "IT", CurrencyType.EUR }, // 意大利 (Italian)
            { "ES", CurrencyType.EUR }, // 西班牙 (Spanish)
            { "PT", CurrencyType.EUR }, // 葡萄牙 (Portuguese)

            // 欧洲国家（使用本国货币）
            { "CZ", CurrencyType.CZK }, // 捷克 (Czech)
            { "PL", CurrencyType.PLN }, // 波兰 (Polish)
            { "RO", CurrencyType.RON }, // 罗马尼亚 (Romanian)

            // 美洲国家
            { "MX", CurrencyType.MXN }, // 墨西哥 (Spanish - Mexico)
            { "BR", CurrencyType.BRL }, // 巴西 (Portuguese)

            // 亚洲国家
            { "JP", CurrencyType.JPY }, // 日本 (Japanese)
            { "KR", CurrencyType.KRW }, // 韩国 (Korean)
            { "IN", CurrencyType.INR }, // 印度 (Hindi)
            { "ID", CurrencyType.IDR }, // 印度尼西亚 (Indonesian)
            { "MY", CurrencyType.MYR }, // 马来西亚 (Malay)
            { "PH", CurrencyType.PHP }, // 菲律宾 (Filipino)
            { "TH", CurrencyType.THB }, // 泰国 (Thai)
            { "VN", CurrencyType.VND }, // 越南 (Vietnamese)

            // 其他地区
            { "RU", CurrencyType.RUB }, // 俄罗斯 (Russian)
            { "TR", CurrencyType.TRY }, // 土耳其 (Turkish)
        };

        /// <summary>
        /// 语言代码到货币类型的备用映射表
        /// </summary>
        private static readonly Dictionary<string, CurrencyType> LanguageToCurrencyMap = new Dictionary<string, CurrencyType>
        {
            { "en", CurrencyType.USD },     // 英语
            { "cs", CurrencyType.CZK },     // 捷克语
            { "nl", CurrencyType.EUR },     // 荷兰语
            { "fil", CurrencyType.PHP },    // 菲律宾语
            { "fr", CurrencyType.EUR },     // 法语
            { "de", CurrencyType.EUR },     // 德语
            { "hi", CurrencyType.INR },     // 印地语
            { "id", CurrencyType.IDR },     // 印尼语
            { "it", CurrencyType.EUR },     // 意大利语
            { "ja", CurrencyType.JPY },     // 日语
            { "ko", CurrencyType.KRW },     // 韩语
            { "ms", CurrencyType.MYR },     // 马来语
            { "pl", CurrencyType.PLN },     // 波兰语
            { "pt", CurrencyType.BRL },     // 葡萄牙语（默认巴西）
            { "pt-BR", CurrencyType.BRL },  // 巴西葡萄牙语
            { "pt-PT", CurrencyType.EUR },  // 欧洲葡萄牙语
            { "ro", CurrencyType.RON },     // 罗马尼亚语
            { "ru", CurrencyType.RUB },     // 俄语
            { "es", CurrencyType.EUR },     // 西班牙语（默认欧洲）
            { "es-MX", CurrencyType.MXN },  // 墨西哥西班牙语
            { "es-ES", CurrencyType.EUR },  // 西班牙西班牙语
            { "th", CurrencyType.THB },     // 泰语
            { "tr", CurrencyType.TRY },     // 土耳其语
            { "vi", CurrencyType.VND },     // 越南语
        };

        /// <summary>
        /// 根据国家代码获取对应的货币类型
        /// </summary>
        /// <param name="countryCode">ISO 3166-1 alpha-2 国家代码（如 CN、US）</param>
        /// <returns>对应的货币类型</returns>
        public static CurrencyType GetCurrencyByCountryCode(string countryCode)
        {
            if (string.IsNullOrEmpty(countryCode))
            {
                Debug.LogWarning("[CountryCurrencyMapper] 国家代码为空，使用默认USD");
                return CurrencyType.USD;
            }

            // 转换为大写
            countryCode = countryCode.ToUpperInvariant();

            // 查找映射
            if (CountryToCurrencyMap.TryGetValue(countryCode, out CurrencyType currency))
            {
                Debug.Log($"[CountryCurrencyMapper] 国家 {countryCode} 映射到货币 {currency}");
                return currency;
            }

            Debug.LogWarning($"[CountryCurrencyMapper] 未找到国家 {countryCode} 的货币映射，使用默认USD");
            return CurrencyType.USD;
        }

        /// <summary>
        /// 根据语言代码获取对应的货币类型（备用方法）
        /// </summary>
        /// <param name="languageCode">语言代码（如 zh-CN、en-US）</param>
        /// <returns>对应的货币类型</returns>
        public static CurrencyType GetCurrencyByLanguageCode(string languageCode)
        {
            if (string.IsNullOrEmpty(languageCode))
            {
                Debug.LogWarning("[CountryCurrencyMapper] 语言代码为空，使用默认USD");
                return CurrencyType.USD;
            }

            // 尝试完整语言代码
            if (LanguageToCurrencyMap.TryGetValue(languageCode, out CurrencyType currency))
            {
                Debug.Log($"[CountryCurrencyMapper] 语言 {languageCode} 映射到货币 {currency}");
                return currency;
            }

            // 尝试主语言代码（去掉地区部分）
            string mainLanguage = languageCode.Split('-')[0].ToLowerInvariant();
            if (LanguageToCurrencyMap.TryGetValue(mainLanguage, out currency))
            {
                Debug.Log($"[CountryCurrencyMapper] 主语言 {mainLanguage} 映射到货币 {currency}");
                return currency;
            }

            Debug.LogWarning($"[CountryCurrencyMapper] 未找到语言 {languageCode} 的货币映射，使用默认USD");
            return CurrencyType.USD;
        }

        /// <summary>
        /// 智能获取货币类型（优先使用国家代码，其次使用语言代码）
        /// </summary>
        /// <param name="countryCode">国家代码</param>
        /// <param name="languageCode">语言代码</param>
        /// <returns>推荐的货币类型</returns>
        public static CurrencyType GetRecommendedCurrency(string countryCode, string languageCode)
        {
            // 优先使用国家代码
            if (!string.IsNullOrEmpty(countryCode))
            {
                countryCode = countryCode.ToUpperInvariant();
                if (CountryToCurrencyMap.TryGetValue(countryCode, out CurrencyType currency))
                {
                    Debug.Log($"[CountryCurrencyMapper] 基于国家 {countryCode} 推荐货币 {currency}");
                    return currency;
                }
            }

            // 其次使用语言代码
            if (!string.IsNullOrEmpty(languageCode))
            {
                CurrencyType langCurrency = GetCurrencyByLanguageCode(languageCode);
                if (langCurrency != CurrencyType.USD)
                {
                    Debug.Log($"[CountryCurrencyMapper] 基于语言 {languageCode} 推荐货币 {langCurrency}");
                    return langCurrency;
                }
            }

            // 默认使用USD
            Debug.Log($"[CountryCurrencyMapper] 使用默认货币 USD（country: {countryCode}, language: {languageCode}）");
            return CurrencyType.USD;
        }

        /// <summary>
        /// 检查国家代码是否支持
        /// </summary>
        public static bool IsCountrySupported(string countryCode)
        {
            if (string.IsNullOrEmpty(countryCode))
                return false;

            return CountryToCurrencyMap.ContainsKey(countryCode.ToUpperInvariant());
        }

        /// <summary>
        /// 获取所有支持的国家代码列表
        /// </summary>
        public static string[] GetSupportedCountryCodes()
        {
            string[] codes = new string[CountryToCurrencyMap.Count];
            CountryToCurrencyMap.Keys.CopyTo(codes, 0);
            return codes;
        }

        /// <summary>
        /// 获取指定货币类型对应的所有国家代码
        /// </summary>
        public static List<string> GetCountriesForCurrency(CurrencyType currency)
        {
            List<string> countries = new List<string>();
            foreach (var kvp in CountryToCurrencyMap)
            {
                if (kvp.Value == currency)
                {
                    countries.Add(kvp.Key);
                }
            }
            return countries;
        }

        /// <summary>
        /// 根据GameLanguage获取对应的货币类型
        /// </summary>
        /// <param name="language">游戏语言枚举</param>
        /// <returns>对应的货币类型</returns>
        public static CurrencyType GetCurrencyByGameLanguage(GameLanguage language)
        {
            // 获取语言信息
            var languageInfo = GameLanguageHelper.GetLanguageInfo(language);

            // 先尝试使用国家代码
            if (!string.IsNullOrEmpty(languageInfo.countryCode))
            {
                if (CountryToCurrencyMap.TryGetValue(languageInfo.countryCode, out CurrencyType currency))
                {
                    Debug.Log($"[CountryCurrencyMapper] GameLanguage {language} ({languageInfo.countryCode}) 映射到货币 {currency}");
                    return currency;
                }
            }

            // 再尝试使用语言代码
            if (!string.IsNullOrEmpty(languageInfo.languageCode))
            {
                if (LanguageToCurrencyMap.TryGetValue(languageInfo.languageCode, out CurrencyType currency))
                {
                    Debug.Log($"[CountryCurrencyMapper] GameLanguage {language} ({languageInfo.languageCode}) 映射到货币 {currency}");
                    return currency;
                }
            }

            // 特殊处理某些语言
            switch (language)
            {
                case GameLanguage.Hindi:
                    return CurrencyType.INR;
                case GameLanguage.Malay:
                    return CurrencyType.MYR;
                case GameLanguage.Filipino:
                    return CurrencyType.PHP;
                case GameLanguage.Arabic:
                    return CurrencyType.USD; // 阿拉伯地区使用USD作为默认
                case GameLanguage.Hebrew:
                    return CurrencyType.USD; // 以色列通常使用新谢克尔，这里用USD作为默认
                default:
                    Debug.LogWarning($"[CountryCurrencyMapper] GameLanguage {language} 没有特定货币映射，使用默认USD");
                    return CurrencyType.USD;
            }
        }

        /// <summary>
        /// 根据GameLanguage和国家代码智能获取货币类型
        /// </summary>
        /// <param name="language">游戏语言</param>
        /// <param name="countryCodeOverride">可选的国家代码覆盖</param>
        /// <returns>推荐的货币类型</returns>
        public static CurrencyType GetRecommendedCurrencyForGameLanguage(GameLanguage language, string countryCodeOverride = null)
        {
            // 优先使用提供的国家代码
            if (!string.IsNullOrEmpty(countryCodeOverride))
            {
                countryCodeOverride = countryCodeOverride.ToUpperInvariant();
                if (CountryToCurrencyMap.TryGetValue(countryCodeOverride, out CurrencyType currency))
                {
                    Debug.Log($"[CountryCurrencyMapper] 使用覆盖国家代码 {countryCodeOverride} -> {currency}");
                    return currency;
                }
            }

            // 使用GameLanguage的默认货币
            return GetCurrencyByGameLanguage(language);
        }
    }
}