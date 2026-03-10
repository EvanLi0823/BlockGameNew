// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

namespace BlockPuzzleGameToolkit.Scripts.Localization
{
    /// <summary>
    /// 游戏支持的语言枚举（替代Unity的SystemLanguage）
    /// 包含所有支持的语言和国家/地区
    /// </summary>
    public enum GameLanguage
    {
        // 英语系
        English,        // 英语（默认）

        // 欧洲语言
        Czech,          // 捷克语
        Dutch,          // 荷兰语
        French,         // 法语
        German,         // 德语
        Italian,        // 意大利语
        Polish,         // 波兰语
        Romanian,       // 罗马尼亚语
        Spanish,        // 西班牙语
        Portuguese,     // 葡萄牙语（欧洲）
        PortugueseBR,   // 葡萄牙语（巴西）

        // 亚洲语言
        Japanese,       // 日语
        Korean,         // 韩语
        Chinese,        // 中文（简体）
        ChineseTraditional, // 中文（繁体）
        Hindi,          // 印地语
        Indonesian,     // 印尼语
        Malay,          // 马来语
        Filipino,       // 菲律宾语
        Thai,           // 泰语
        Vietnamese,     // 越南语

        // 其他地区语言
        Russian,        // 俄语
        Turkish,        // 土耳其语
        Arabic,         // 阿拉伯语（预留）
        Hebrew,         // 希伯来语（预留）
    }

    /// <summary>
    /// 语言信息
    /// </summary>
    [System.Serializable]
    public class LanguageInfo
    {
        public GameLanguage language;
        public string languageCode;     // ISO 639-1 语言代码（如 "en", "zh"）
        public string countryCode;      // ISO 3166-1 国家代码（如 "US", "CN"）
        public string nativeName;       // 本地语言名称
        public string englishName;      // 英文名称
        public bool isRTL;             // 是否从右到左（阿拉伯语、希伯来语）

        public LanguageInfo(GameLanguage lang, string langCode, string country, string native, string english, bool rtl = false)
        {
            language = lang;
            languageCode = langCode;
            countryCode = country;
            nativeName = native;
            englishName = english;
            isRTL = rtl;
        }
    }

    /// <summary>
    /// 语言工具类
    /// </summary>
    public static class GameLanguageHelper
    {
        /// <summary>
        /// 获取语言信息
        /// </summary>
        public static LanguageInfo GetLanguageInfo(GameLanguage language)
        {
            switch (language)
            {
                // 英语系
                case GameLanguage.English:
                    return new LanguageInfo(language, "en", "US", "English", "English");

                // 欧洲语言
                case GameLanguage.Czech:
                    return new LanguageInfo(language, "cs", "CZ", "Čeština", "Czech");
                case GameLanguage.Dutch:
                    return new LanguageInfo(language, "nl", "NL", "Nederlands", "Dutch");
                case GameLanguage.French:
                    return new LanguageInfo(language, "fr", "FR", "Français", "French");
                case GameLanguage.German:
                    return new LanguageInfo(language, "de", "DE", "Deutsch", "German");
                case GameLanguage.Italian:
                    return new LanguageInfo(language, "it", "IT", "Italiano", "Italian");
                case GameLanguage.Polish:
                    return new LanguageInfo(language, "pl", "PL", "Polski", "Polish");
                case GameLanguage.Romanian:
                    return new LanguageInfo(language, "ro", "RO", "Română", "Romanian");
                case GameLanguage.Spanish:
                    return new LanguageInfo(language, "es", "ES", "Español", "Spanish");
                case GameLanguage.Portuguese:
                    return new LanguageInfo(language, "pt", "PT", "Português", "Portuguese");
                case GameLanguage.PortugueseBR:
                    return new LanguageInfo(language, "pt-BR", "BR", "Português (Brasil)", "Portuguese (Brazil)");

                // 亚洲语言
                case GameLanguage.Japanese:
                    return new LanguageInfo(language, "ja", "JP", "日本語", "Japanese");
                case GameLanguage.Korean:
                    return new LanguageInfo(language, "ko", "KR", "한국어", "Korean");
                case GameLanguage.Chinese:
                    return new LanguageInfo(language, "zh", "CN", "简体中文", "Chinese (Simplified)");
                case GameLanguage.ChineseTraditional:
                    return new LanguageInfo(language, "zh-TW", "TW", "繁體中文", "Chinese (Traditional)");
                case GameLanguage.Hindi:
                    return new LanguageInfo(language, "hi", "IN", "हिन्दी", "Hindi");
                case GameLanguage.Indonesian:
                    return new LanguageInfo(language, "id", "ID", "Bahasa Indonesia", "Indonesian");
                case GameLanguage.Malay:
                    return new LanguageInfo(language, "ms", "MY", "Bahasa Melayu", "Malay");
                case GameLanguage.Filipino:
                    return new LanguageInfo(language, "fil", "PH", "Filipino", "Filipino");
                case GameLanguage.Thai:
                    return new LanguageInfo(language, "th", "TH", "ไทย", "Thai");
                case GameLanguage.Vietnamese:
                    return new LanguageInfo(language, "vi", "VN", "Tiếng Việt", "Vietnamese");

                // 其他地区
                case GameLanguage.Russian:
                    return new LanguageInfo(language, "ru", "RU", "Русский", "Russian");
                case GameLanguage.Turkish:
                    return new LanguageInfo(language, "tr", "TR", "Türkçe", "Turkish");
                case GameLanguage.Arabic:
                    return new LanguageInfo(language, "ar", "SA", "العربية", "Arabic", true);
                case GameLanguage.Hebrew:
                    return new LanguageInfo(language, "he", "IL", "עברית", "Hebrew", true);

                default:
                    return GetLanguageInfo(GameLanguage.English);
            }
        }

        /// <summary>
        /// 根据语言代码获取GameLanguage
        /// </summary>
        public static GameLanguage GetLanguageFromCode(string languageCode)
        {
            if (string.IsNullOrEmpty(languageCode))
                return GameLanguage.English;

            // 处理主语言代码
            string mainLang = languageCode.Split('-')[0].ToLower();

            // 特殊处理葡萄牙语
            if (languageCode.ToLower() == "pt-br")
                return GameLanguage.PortugueseBR;

            // 特殊处理中文
            if (languageCode.ToLower().Contains("zh-tw") || languageCode.ToLower().Contains("zh-hk"))
                return GameLanguage.ChineseTraditional;

            switch (mainLang)
            {
                case "en": return GameLanguage.English;
                case "cs": return GameLanguage.Czech;
                case "nl": return GameLanguage.Dutch;
                case "fr": return GameLanguage.French;
                case "de": return GameLanguage.German;
                case "it": return GameLanguage.Italian;
                case "pl": return GameLanguage.Polish;
                case "ro": return GameLanguage.Romanian;
                case "es": return GameLanguage.Spanish;
                case "pt": return GameLanguage.Portuguese;
                case "ja": return GameLanguage.Japanese;
                case "ko": return GameLanguage.Korean;
                case "zh": return GameLanguage.Chinese;
                case "hi": return GameLanguage.Hindi;
                case "id": return GameLanguage.Indonesian;
                case "ms": return GameLanguage.Malay;
                case "fil": return GameLanguage.Filipino;
                case "tl": return GameLanguage.Filipino; // Tagalog映射到Filipino
                case "th": return GameLanguage.Thai;
                case "vi": return GameLanguage.Vietnamese;
                case "ru": return GameLanguage.Russian;
                case "tr": return GameLanguage.Turkish;
                case "ar": return GameLanguage.Arabic;
                case "he": return GameLanguage.Hebrew;
                default: return GameLanguage.English;
            }
        }

        /// <summary>
        /// 将Unity的SystemLanguage转换为GameLanguage
        /// </summary>
        public static GameLanguage FromSystemLanguage(UnityEngine.SystemLanguage systemLanguage)
        {
            switch (systemLanguage)
            {
                case UnityEngine.SystemLanguage.English: return GameLanguage.English;
                case UnityEngine.SystemLanguage.Czech: return GameLanguage.Czech;
                case UnityEngine.SystemLanguage.Dutch: return GameLanguage.Dutch;
                case UnityEngine.SystemLanguage.French: return GameLanguage.French;
                case UnityEngine.SystemLanguage.German: return GameLanguage.German;
                case UnityEngine.SystemLanguage.Italian: return GameLanguage.Italian;
                case UnityEngine.SystemLanguage.Polish: return GameLanguage.Polish;
                case UnityEngine.SystemLanguage.Romanian: return GameLanguage.Romanian;
                case UnityEngine.SystemLanguage.Spanish: return GameLanguage.Spanish;
                case UnityEngine.SystemLanguage.Portuguese: return GameLanguage.Portuguese;
                case UnityEngine.SystemLanguage.Japanese: return GameLanguage.Japanese;
                case UnityEngine.SystemLanguage.Korean: return GameLanguage.Korean;
                case UnityEngine.SystemLanguage.Chinese: return GameLanguage.Chinese;
                case UnityEngine.SystemLanguage.ChineseSimplified: return GameLanguage.Chinese;
                case UnityEngine.SystemLanguage.ChineseTraditional: return GameLanguage.ChineseTraditional;
                case UnityEngine.SystemLanguage.Indonesian: return GameLanguage.Indonesian;
                case UnityEngine.SystemLanguage.Thai: return GameLanguage.Thai;
                case UnityEngine.SystemLanguage.Vietnamese: return GameLanguage.Vietnamese;
                case UnityEngine.SystemLanguage.Russian: return GameLanguage.Russian;
                case UnityEngine.SystemLanguage.Turkish: return GameLanguage.Turkish;
                case UnityEngine.SystemLanguage.Arabic: return GameLanguage.Arabic;
                case UnityEngine.SystemLanguage.Hebrew: return GameLanguage.Hebrew;
                default: return GameLanguage.English;
            }
        }

        /// <summary>
        /// 将GameLanguage转换为Unity的SystemLanguage（如果可能）
        /// </summary>
        public static UnityEngine.SystemLanguage ToSystemLanguage(GameLanguage gameLanguage)
        {
            switch (gameLanguage)
            {
                case GameLanguage.English: return UnityEngine.SystemLanguage.English;
                case GameLanguage.Czech: return UnityEngine.SystemLanguage.Czech;
                case GameLanguage.Dutch: return UnityEngine.SystemLanguage.Dutch;
                case GameLanguage.French: return UnityEngine.SystemLanguage.French;
                case GameLanguage.German: return UnityEngine.SystemLanguage.German;
                case GameLanguage.Italian: return UnityEngine.SystemLanguage.Italian;
                case GameLanguage.Polish: return UnityEngine.SystemLanguage.Polish;
                case GameLanguage.Romanian: return UnityEngine.SystemLanguage.Romanian;
                case GameLanguage.Spanish: return UnityEngine.SystemLanguage.Spanish;
                case GameLanguage.Portuguese: return UnityEngine.SystemLanguage.Portuguese;
                case GameLanguage.PortugueseBR: return UnityEngine.SystemLanguage.Portuguese;
                case GameLanguage.Japanese: return UnityEngine.SystemLanguage.Japanese;
                case GameLanguage.Korean: return UnityEngine.SystemLanguage.Korean;
                case GameLanguage.Chinese: return UnityEngine.SystemLanguage.ChineseSimplified;
                case GameLanguage.ChineseTraditional: return UnityEngine.SystemLanguage.ChineseTraditional;
                case GameLanguage.Indonesian: return UnityEngine.SystemLanguage.Indonesian;
                case GameLanguage.Thai: return UnityEngine.SystemLanguage.Thai;
                case GameLanguage.Vietnamese: return UnityEngine.SystemLanguage.Vietnamese;
                case GameLanguage.Russian: return UnityEngine.SystemLanguage.Russian;
                case GameLanguage.Turkish: return UnityEngine.SystemLanguage.Turkish;
                case GameLanguage.Arabic: return UnityEngine.SystemLanguage.Arabic;
                case GameLanguage.Hebrew: return UnityEngine.SystemLanguage.Hebrew;
                // Unity不支持的语言映射到English
                case GameLanguage.Hindi:
                case GameLanguage.Malay:
                case GameLanguage.Filipino:
                default: return UnityEngine.SystemLanguage.English;
            }
        }
    }
}