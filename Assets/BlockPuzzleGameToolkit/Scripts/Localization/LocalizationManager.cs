// // ©2015 - 2025 Candy Smith
// // All rights reserved
// // Redistribution of this software is strictly not allowed.
// // Copy of this software can be obtained from unity asset store only.
// // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// // IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// // FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// // AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// // LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// // OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// // THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using BlockPuzzleGameToolkit.Scripts.Settings;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using BlockPuzzleGameToolkit.Scripts.CurrencySystem;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Localization
{
    public class LocalizationManager : SingletonBehaviour<LocalizationManager>
    {
        private static DebugSettings _debugSettings;
        public static Dictionary<string, string> _dic;
        private static SystemLanguage _currentLanguage;
        private static bool _useSystemLanguage = true;
        private static SystemLanguage _overrideLanguage = SystemLanguage.English;

        // PlayerPrefs key names
        private const string LANGUAGE_PREF_KEY = "GameLanguage";
        private const string USE_SYSTEM_LANGUAGE_KEY = "UseSystemLanguage";

        /// <summary>
        /// Language changed event
        /// </summary>
        public static event Action OnLanguageChanged;

        /// <summary>
        /// Initialization priority (localization should initialize early)
        /// </summary>
        public override int InitPriority => 5;

        public override void Awake()
        {
            base.Awake();
        }

        /// <summary>
        /// Initialize localization system
        /// </summary>
        public override void OnInit()
        {
            if (IsInitialized) return;

            Debug.Log("[LocalizationManager] Initializing localization system");
            InitializeLocalization();

            base.OnInit(); // Set IsInitialized = true
        }

        public static void InitializeLocalization()
        {
            _debugSettings = Resources.Load("Settings/DebugSettings") as DebugSettings;

            // Load saved language settings
            LoadLanguageSettings();

            // Use saved language or system language
            SystemLanguage targetLanguage = GetTargetLanguage();
            LoadLanguage(targetLanguage);
        }

        /// <summary>
        /// Load saved language settings
        /// </summary>
        private static void LoadLanguageSettings()
        {
            _useSystemLanguage = PlayerPrefs.GetInt(USE_SYSTEM_LANGUAGE_KEY, 1) == 1;

            if (PlayerPrefs.HasKey(LANGUAGE_PREF_KEY))
            {
                string savedLanguage = PlayerPrefs.GetString(LANGUAGE_PREF_KEY, "English");
                if (Enum.TryParse<SystemLanguage>(savedLanguage, out SystemLanguage language))
                {
                    _overrideLanguage = language;
                }
            }

            Debug.Log($"[LocalizationManager] Language settings - Use system: {_useSystemLanguage}, Override: {_overrideLanguage}");
        }

        /// <summary>
        /// Save language settings
        /// </summary>
        private static void SaveLanguageSettings()
        {
            PlayerPrefs.SetInt(USE_SYSTEM_LANGUAGE_KEY, _useSystemLanguage ? 1 : 0);
            PlayerPrefs.SetString(LANGUAGE_PREF_KEY, _overrideLanguage.ToString());
            PlayerPrefs.Save();
            Debug.Log($"[LocalizationManager] Saved language settings - Use system: {_useSystemLanguage}, Override: {_overrideLanguage}");
        }

        /// <summary>
        /// Get target language
        /// </summary>
        /// <returns>Language to use</returns>
        private static SystemLanguage GetTargetLanguage()
        {
            // If manually set language (not using system language), use the override language
            if (!_useSystemLanguage)
            {
                return _overrideLanguage;
            }

            // Use debug settings in editor mode only when using system language
            if (Application.isEditor && _debugSettings != null && _useSystemLanguage)
            {
                return _debugSettings.TestLanguage;
            }

            // If set to use system language
            if (_useSystemLanguage)
            {
                return Application.systemLanguage;
            }

            // Otherwise use user-selected language
            return _overrideLanguage;
        }

        public static void LoadLanguage(SystemLanguage language)
        {
            Debug.Log($"[LocalizationManager] LoadLanguage called with: {language}");
            _currentLanguage = language;
            var txt = Resources.Load<TextAsset>($"Localization/{language}");
            if (txt == null)
            {
                Debug.LogWarning($"Localization file for {language} not found. Falling back to English.");
                txt = Resources.Load<TextAsset>("Localization/English");
                _currentLanguage = SystemLanguage.English;
            }
            else
            {
                Debug.Log($"[LocalizationManager] Successfully loaded {language} localization file");
            }

            _dic = new Dictionary<string, string>();
            var lines = txt.text.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var inp_ln in lines)
            {
                var l = inp_ln.Split(new[] { ':' }, 2);
                if (l.Length == 2)
                {
                    var key = l[0].Trim();
                    var text = l[1].Trim();
                    _dic[key] = text;
                }
            }

            Debug.Log($"[LocalizationManager] Loaded {_dic.Count} localized strings for {_currentLanguage}");

            // Trigger language changed event
            OnLanguageChanged?.Invoke();
        }

        public static SystemLanguage GetSystemLanguage()
        {
            return GetTargetLanguage();
        }

        public static string GetText(string key, string defaultText)
        {
            // 如果字典未初始化或为空，加载当前语言
            if (_dic == null || _dic.Count == 0)
            {
                // 如果当前语言未设置，使用目标语言
                if (_currentLanguage == SystemLanguage.Unknown)
                {
                    _currentLanguage = GetTargetLanguage();
                    Debug.Log($"[LocalizationManager] GetText: Setting initial language to {_currentLanguage}");
                }
                LoadLanguage(_currentLanguage);
            }

            if (_dic.TryGetValue(key, out var localizedText) && !string.IsNullOrEmpty(localizedText))
            {
                return PlaceholderManager.ReplacePlaceholders(localizedText);
            }

            // Debug log for missing keys
            if (!_dic.ContainsKey(key) && !string.IsNullOrEmpty(key))
            {
                Debug.LogWarning($"[LocalizationManager] Key '{key}' not found in {_currentLanguage} dictionary");
            }

            return PlaceholderManager.ReplacePlaceholders(defaultText);
        }

        public static SystemLanguage GetCurrentLanguage()
        {
            return _currentLanguage;
        }

        /// <summary>
        /// Test method to verify language switching (internal use only)
        /// </summary>
        private static void TestLanguageSwitch()
        {
            Debug.Log($"[LocalizationManager] === Language Test ===");
            Debug.Log($"Current Language: {_currentLanguage}");
            Debug.Log($"Use System Language: {_useSystemLanguage}");
            Debug.Log($"Override Language: {_overrideLanguage}");
            Debug.Log($"Dictionary loaded: {_dic != null} with {_dic?.Count ?? 0} entries");

            // Test a few keys
            if (_dic != null && _dic.Count > 0)
            {
                int count = 0;
                foreach(var kvp in _dic)
                {
                    Debug.Log($"  Sample key: '{kvp.Key}' = '{kvp.Value}'");
                    if (++count >= 3) break;
                }
            }
            Debug.Log($"[LocalizationManager] === End Test ===");
        }

        /// <summary>
        /// Change language at runtime
        /// </summary>
        /// <param name="language">Target language</param>
        public static void ChangeLanguage(SystemLanguage language)
        {
            Debug.Log($"[LocalizationManager] ChangeLanguage called: {_currentLanguage} -> {language}");

            // Set to use custom language
            _useSystemLanguage = false;
            _overrideLanguage = language;

            // Save settings
            SaveLanguageSettings();

            // Clear existing dictionary to force reload
            _dic = null;

            // Load new language
            LoadLanguage(language);

            // Test to verify language loaded
            TestLanguageSwitch();

            // Refresh all texts
            RefreshAllLocalizedTexts();
        }

        /// <summary>
        /// Inspector 方法：切换到英语并同步货币
        /// </summary>
        [ContextMenu("切换到英语（含货币同步）")]
        public void SwitchToEnglishWithCurrencySync()
        {
            Debug.Log("[LocalizationManager] Inspector: 切换到英语并同步货币");
            ChangeLanguage(SystemLanguage.English);

            // 确保 ExchangeRateManager 同步货币
            if (ExchangeRateManager.Instance != null)
            {
                ExchangeRateManager.Instance.SyncCurrencyWithCurrentLanguage();
            }
        }

        /// <summary>
        /// Inspector 方法：强制刷新当前语言
        /// </summary>
        [ContextMenu("强制刷新当前语言")]
        public void ForceRefreshCurrentLanguage()
        {
            Debug.Log($"[LocalizationManager] Inspector: 强制刷新语言 {_currentLanguage}");
            LoadLanguage(_currentLanguage);
            RefreshAllLocalizedTexts();
        }

        /// <summary>
        /// Set whether to use system language
        /// </summary>
        /// <param name="useSystemLanguage">Whether to use system language</param>
        public static void SetUseSystemLanguage(bool useSystemLanguage)
        {
            _useSystemLanguage = useSystemLanguage;
            SaveLanguageSettings();

            // Apply settings immediately
            SystemLanguage targetLanguage = GetTargetLanguage();
            if (_currentLanguage != targetLanguage)
            {
                LoadLanguage(targetLanguage);
                RefreshAllLocalizedTexts();
            }
        }

        /// <summary>
        /// Get whether using system language
        /// </summary>
        public static bool IsUsingSystemLanguage()
        {
            return _useSystemLanguage;
        }

        /// <summary>
        /// Get all supported languages
        /// </summary>
        /// <summary>
        /// Get all supported languages by scanning Resources/Localization folder
        /// NOTE: Some languages like Filipino and Malay are not supported by Unity's SystemLanguage enum
        /// and therefore cannot be used with this system. Consider using a custom language system
        /// if you need to support these languages.
        /// </summary>
        public static SystemLanguage[] GetSupportedLanguages()
        {
            var supportedLanguages = new List<SystemLanguage>();

            // Define mapping of file names to SystemLanguage enum
            var languageMapping = new Dictionary<string, SystemLanguage>
            {
                { "Czech", SystemLanguage.Czech },
                { "Dutch", SystemLanguage.Dutch },
                { "English", SystemLanguage.English },
                { "French", SystemLanguage.French },
                { "German", SystemLanguage.German },
                { "Italian", SystemLanguage.Italian },
                { "Japanese", SystemLanguage.Japanese },
                { "Korean", SystemLanguage.Korean },
                { "Polish", SystemLanguage.Polish },
                { "Portuguese", SystemLanguage.Portuguese },
                { "Romanian", SystemLanguage.Romanian },
                { "Russian", SystemLanguage.Russian },
                { "Spanish", SystemLanguage.Spanish },
                { "Thai", SystemLanguage.Thai },
                { "Turkish", SystemLanguage.Turkish },
                { "Vietnamese", SystemLanguage.Vietnamese },
                { "Indonesian", SystemLanguage.Indonesian }
            };

            // Check each mapped language if its file exists
            foreach (var kvp in languageMapping)
            {
                var txt = Resources.Load<TextAsset>($"Localization/{kvp.Key}");
                if (txt != null)
                {
                    supportedLanguages.Add(kvp.Value);
                }
            }

            // Special handling for languages that might not have enum values or need special names
            // Hindi - SystemLanguage enum might have it in newer Unity versions
            var hindiFile = Resources.Load<TextAsset>("Localization/Hindi");
            if (hindiFile != null)
            {
                // Try to parse Hindi enum if it exists
                if (System.Enum.TryParse<SystemLanguage>("Hindi", out var hindi))
                {
                    supportedLanguages.Add(hindi);
                }
            }

            // For Filipino and Malay, we'll use closest alternatives or skip them
            // since SystemLanguage enum doesn't have these values

            // Sort alphabetically but keep English first
            var englishFirst = supportedLanguages.Where(l => l == SystemLanguage.English).ToList();
            var otherLanguages = supportedLanguages.Where(l => l != SystemLanguage.English).OrderBy(l => l.ToString()).ToList();
            englishFirst.AddRange(otherLanguages);

            return englishFirst.ToArray();
        }

        /// <summary>
        /// Check if language is supported
        /// </summary>
        public static bool IsLanguageSupported(SystemLanguage language)
        {
            var txt = Resources.Load<TextAsset>($"Localization/{language}");
            return txt != null;
        }

        /// <summary>
        /// Refresh all localized text components
        /// </summary>
        public static void RefreshAllLocalizedTexts()
        {
            // Refresh all LocalizedTextMeshProUGUI components
            var localizedTMPTexts = UnityEngine.Object.FindObjectsOfType<LocalizedTextMeshProUGUI>();
            foreach (var text in localizedTMPTexts)
            {
                if (text != null && text.gameObject.activeInHierarchy)
                {
                    text.UpdateText();
                }
            }
            Debug.Log($"[LocalizationManager] Refreshed {localizedTMPTexts.Length} LocalizedTextMeshProUGUI components");

            // Refresh all LocalizedText components
            var localizedTexts = UnityEngine.Object.FindObjectsOfType<LocalizedText>();
            foreach (var text in localizedTexts)
            {
                if (text != null && text.gameObject.activeInHierarchy)
                {
                    text.UpdateText();
                }
            }
            Debug.Log($"[LocalizationManager] Refreshed {localizedTexts.Length} LocalizedText components");
        }
    }
}