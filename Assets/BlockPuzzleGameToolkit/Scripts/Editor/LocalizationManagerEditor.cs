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

using UnityEngine;
using UnityEditor;
using System.Linq;
using BlockPuzzleGameToolkit.Scripts.Localization;

namespace BlockPuzzleGameToolkit.Scripts.Editor
{
    /// <summary>
    /// Custom editor for LocalizationManager - provides language selection in Inspector
    /// </summary>
    [CustomEditor(typeof(LocalizationManager))]
    public class LocalizationManagerEditor : UnityEditor.Editor
    {
        private bool useSystemLanguage;
        private SystemLanguage selectedLanguage;
        private SystemLanguage[] supportedLanguages;
        private string[] languageNames;
        private int selectedLanguageIndex;

        // Style settings
        private GUIStyle headerStyle;
        private GUIStyle boxStyle;
        private bool stylesInitialized = false;

        private void OnEnable()
        {
            // Load current settings
            RefreshSettings();
        }

        private void InitStyles()
        {
            if (stylesInitialized) return;

            headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter
            };

            boxStyle = new GUIStyle(UnityEngine.GUI.skin.box)
            {
                padding = new RectOffset(10, 10, 10, 10)
            };

            stylesInitialized = true;
        }

        private void RefreshSettings()
        {
            // Get supported languages
            supportedLanguages = LocalizationManager.GetSupportedLanguages();
            languageNames = supportedLanguages.Select(lang => GetLocalizedLanguageName(lang)).ToArray();

            // Get current settings
            useSystemLanguage = LocalizationManager.IsUsingSystemLanguage();
            var currentLanguage = LocalizationManager.GetCurrentLanguage();

            // Find current language index
            selectedLanguageIndex = System.Array.IndexOf(supportedLanguages, currentLanguage);
            if (selectedLanguageIndex < 0) selectedLanguageIndex = 0;
            selectedLanguage = supportedLanguages[selectedLanguageIndex];
        }

        /// <summary>
        /// Get localized language name for display
        /// </summary>
        private string GetLocalizedLanguageName(SystemLanguage language)
        {
            switch (language)
            {
                case SystemLanguage.English:
                    return "English";
                case SystemLanguage.Czech:
                    return "Čeština (Czech)";
                case SystemLanguage.Dutch:
                    return "Nederlands (Dutch)";
                case SystemLanguage.French:
                    return "Français (French)";
                case SystemLanguage.German:
                    return "Deutsch (German)";
                case SystemLanguage.Italian:
                    return "Italiano (Italian)";
                case SystemLanguage.Japanese:
                    return "日本語 (Japanese)";
                case SystemLanguage.Korean:
                    return "한국어 (Korean)";
                case SystemLanguage.Polish:
                    return "Polski (Polish)";
                case SystemLanguage.Portuguese:
                    return "Português (Portuguese)";
                case SystemLanguage.Romanian:
                    return "Română (Romanian)";
                case SystemLanguage.Russian:
                    return "Русский (Russian)";
                case SystemLanguage.Spanish:
                    return "Español (Spanish)";
                case SystemLanguage.Thai:
                    return "ไทย (Thai)";
                case SystemLanguage.Turkish:
                    return "Türkçe (Turkish)";
                case SystemLanguage.Vietnamese:
                    return "Tiếng Việt (Vietnamese)";
                case SystemLanguage.Indonesian:
                    return "Bahasa Indonesia (Indonesian)";
                default:
                    // 特殊处理其他语言（如 Hindi, Filipino, Malay）
                    if (language.ToString() == "Hindi")
                        return "हिन्दी (Hindi)";
                    return language.ToString();
            }
        }

        public override void OnInspectorGUI()
        {
            InitStyles();

            // Draw default inspector
            DrawDefaultInspector();

            EditorGUILayout.Space(20);

            // Language Settings Section
            EditorGUILayout.BeginVertical(boxStyle);

            // Header
            EditorGUILayout.LabelField("🌐 Language Settings", headerStyle);
            EditorGUILayout.Space(10);

            // Current language info
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Current Language:", EditorStyles.boldLabel, GUILayout.Width(120));
            var currentLang = LocalizationManager.GetCurrentLanguage();
            EditorGUILayout.LabelField(GetLocalizedLanguageName(currentLang));
            EditorGUILayout.EndHorizontal();

            // System language info
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("System Language:", EditorStyles.boldLabel, GUILayout.Width(120));
            EditorGUILayout.LabelField(Application.systemLanguage.ToString());
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // Use system language toggle
            EditorGUI.BeginChangeCheck();
            useSystemLanguage = EditorGUILayout.Toggle("Use System Language", useSystemLanguage);
            if (EditorGUI.EndChangeCheck())
            {
                ApplyLanguageSettings();
            }

            // Language selection dropdown
            EditorGUI.BeginDisabledGroup(useSystemLanguage);

            EditorGUILayout.Space(5);
            EditorGUI.BeginChangeCheck();
            selectedLanguageIndex = EditorGUILayout.Popup("Select Language", selectedLanguageIndex, languageNames);
            if (EditorGUI.EndChangeCheck())
            {
                selectedLanguage = supportedLanguages[selectedLanguageIndex];
                if (!useSystemLanguage)
                {
                    ApplyLanguageSettings();
                }
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(10);

            // Quick language buttons
            EditorGUILayout.LabelField("Quick Select:", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            int buttonsPerRow = 4; // 增加每行按钮数量
            int buttonCount = 0;
            int maxButtons = Mathf.Min(supportedLanguages.Length, 12); // 最多显示12个快速按钮

            for (int i = 0; i < maxButtons; i++)
            {
                if (buttonCount > 0 && buttonCount % buttonsPerRow == 0)
                {
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                }

                string buttonLabel = GetShortLanguageName(supportedLanguages[i]);
                bool isCurrentLang = (currentLang == supportedLanguages[i]);

                // Highlight current language button
                UnityEngine.GUI.backgroundColor = isCurrentLang ? Color.green : Color.white;

                if (GUILayout.Button(buttonLabel, GUILayout.Height(30)))
                {
                    selectedLanguageIndex = i;
                    selectedLanguage = supportedLanguages[i];
                    useSystemLanguage = false;
                    ApplyLanguageSettings();
                }

                UnityEngine.GUI.backgroundColor = Color.white;
                buttonCount++;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // Runtime test section
            if (Application.isPlaying)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.HelpBox("✅ You can change language in runtime. Changes will be applied immediately.", MessageType.Info);

                if (GUILayout.Button("Refresh All Texts", GUILayout.Height(30)))
                {
                    LocalizationManager.RefreshAllLocalizedTexts();
                    Debug.Log("[LocalizationManagerEditor] Refreshed all localized texts");
                }
            }
            else
            {
                EditorGUILayout.HelpBox("⚠️ Language changes in edit mode will be saved and applied when entering play mode.", MessageType.Warning);
            }

            // Debug info
            EditorGUILayout.Space(10);
            if (GUILayout.Button("Check Missing Localizations", GUILayout.Height(25)))
            {
                CheckMissingLocalizations();
            }

            EditorGUILayout.EndVertical();

            // Add help box
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "Language Settings Help:\n" +
                "• Use System Language: Automatically uses device language if supported\n" +
                "• Select Language: Manually choose a specific language\n" +
                "• Quick Select: Click buttons for fast language switching\n" +
                "• Changes are saved automatically\n\n" +
                "Note: Filipino and Malay language files exist but are not supported by Unity's SystemLanguage enum.",
                MessageType.None
            );
        }

        /// <summary>
        /// Get short language name for buttons
        /// </summary>
        private string GetShortLanguageName(SystemLanguage language)
        {
            switch (language)
            {
                case SystemLanguage.English:
                    return "EN";
                case SystemLanguage.Czech:
                    return "CZ";
                case SystemLanguage.Dutch:
                    return "NL";
                case SystemLanguage.French:
                    return "FR";
                case SystemLanguage.German:
                    return "DE";
                case SystemLanguage.Italian:
                    return "IT";
                case SystemLanguage.Japanese:
                    return "JP";
                case SystemLanguage.Korean:
                    return "KR";
                case SystemLanguage.Polish:
                    return "PL";
                case SystemLanguage.Portuguese:
                    return "PT";
                case SystemLanguage.Romanian:
                    return "RO";
                case SystemLanguage.Russian:
                    return "RU";
                case SystemLanguage.Spanish:
                    return "ES";
                case SystemLanguage.Thai:
                    return "TH";
                case SystemLanguage.Turkish:
                    return "TR";
                case SystemLanguage.Vietnamese:
                    return "VN";
                case SystemLanguage.Indonesian:
                    return "ID";
                default:
                    // 特殊处理其他语言
                    if (language.ToString() == "Hindi")
                        return "HI";
                    return language.ToString().Length >= 2 ? language.ToString().Substring(0, 2).ToUpper() : language.ToString();
            }
        }

        /// <summary>
        /// Apply language settings
        /// </summary>
        private void ApplyLanguageSettings()
        {
            if (useSystemLanguage)
            {
                LocalizationManager.SetUseSystemLanguage(true);
                Debug.Log($"[LocalizationManagerEditor] Set to use system language");
            }
            else
            {
                LocalizationManager.ChangeLanguage(selectedLanguage);
                Debug.Log($"[LocalizationManagerEditor] Changed language to: {selectedLanguage}");
            }

            // Refresh settings after applying
            RefreshSettings();

            // Mark scene as dirty if in edit mode
            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(target);
            }
        }

        /// <summary>
        /// Check for missing localizations
        /// </summary>
        private void CheckMissingLocalizations()
        {
            int missingCount = 0;
            var languages = LocalizationManager.GetSupportedLanguages();

            foreach (var language in languages)
            {
                if (!LocalizationManager.IsLanguageSupported(language))
                {
                    Debug.LogWarning($"[LocalizationManagerEditor] Missing localization file for: {language}");
                    missingCount++;
                }
                else
                {
                    Debug.Log($"[LocalizationManagerEditor] ✅ {language} localization found");
                }
            }

            if (missingCount == 0)
            {
                Debug.Log($"[LocalizationManagerEditor] All {languages.Length} language files are present!");
            }
            else
            {
                Debug.LogWarning($"[LocalizationManagerEditor] {missingCount} language files are missing!");
            }
        }
    }
}