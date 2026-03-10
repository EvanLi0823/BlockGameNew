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
using System.Collections.Generic;
using System.Linq;

namespace BlockPuzzleGameToolkit.Scripts.Localization
{
    /// <summary>
    /// Language selector component for runtime language switching
    /// Provides language selection dropdown in Inspector panel
    /// </summary>
    [AddComponentMenu("Localization/Language Selector")]
    public class LanguageSelector : MonoBehaviour
    {
        [Header("Language Settings")]
        [SerializeField]
        [Tooltip("Use system language")]
        private bool useSystemLanguage = true;

        [SerializeField]
        [Tooltip("Select language to use")]
        private SystemLanguage selectedLanguage = SystemLanguage.English;

        [Header("Runtime Settings")]
        [SerializeField]
        [Tooltip("Apply language settings on start")]
        private bool applyOnStart = true;

        [Header("Debug Info")]
        [SerializeField]
        [ReadOnly]
        private SystemLanguage currentLanguage = SystemLanguage.English;

        [SerializeField]
        [ReadOnly]
        private bool isLanguageSupported = true;

        private void Start()
        {
            if (applyOnStart)
            {
                ApplyLanguageSettings();
            }

            UpdateDebugInfo();
        }

        /// <summary>
        /// Apply language settings
        /// </summary>
        public void ApplyLanguageSettings()
        {
            if (useSystemLanguage)
            {
                LocalizationManager.SetUseSystemLanguage(true);
                Debug.Log($"[LanguageSelector] Set to use system language");
            }
            else
            {
                LocalizationManager.ChangeLanguage(selectedLanguage);
                Debug.Log($"[LanguageSelector] Changed language to: {selectedLanguage}");
            }

            UpdateDebugInfo();
        }

        /// <summary>
        /// Set whether to use system language
        /// </summary>
        public void SetUseSystemLanguage(bool use)
        {
            useSystemLanguage = use;
            if (use)
            {
                LocalizationManager.SetUseSystemLanguage(true);
            }
            UpdateDebugInfo();
        }

        /// <summary>
        /// Set specific language
        /// </summary>
        public void SetLanguage(SystemLanguage language)
        {
            selectedLanguage = language;
            useSystemLanguage = false;
            LocalizationManager.ChangeLanguage(language);
            UpdateDebugInfo();
        }

        /// <summary>
        /// Set language by index (for UI dropdown)
        /// </summary>
        public void SetLanguageByIndex(int index)
        {
            var supportedLanguages = LocalizationManager.GetSupportedLanguages();
            if (index >= 0 && index < supportedLanguages.Length)
            {
                SetLanguage(supportedLanguages[index]);
            }
        }

        /// <summary>
        /// Get list of supported language names (for UI dropdown)
        /// </summary>
        public List<string> GetLanguageNames()
        {
            var supportedLanguages = LocalizationManager.GetSupportedLanguages();
            return supportedLanguages.Select(lang => lang.ToString()).ToList();
        }

        /// <summary>
        /// Get current language index (for UI dropdown)
        /// </summary>
        public int GetCurrentLanguageIndex()
        {
            var supportedLanguages = LocalizationManager.GetSupportedLanguages();
            var current = LocalizationManager.GetCurrentLanguage();

            for (int i = 0; i < supportedLanguages.Length; i++)
            {
                if (supportedLanguages[i] == current)
                {
                    return i;
                }
            }

            return 0; // Default to first language
        }

        /// <summary>
        /// Refresh all localized texts
        /// </summary>
        public void RefreshAllTexts()
        {
            LocalizationManager.RefreshAllLocalizedTexts();
        }

        /// <summary>
        /// Update debug information
        /// </summary>
        private void UpdateDebugInfo()
        {
            currentLanguage = LocalizationManager.GetCurrentLanguage();
            isLanguageSupported = LocalizationManager.IsLanguageSupported(currentLanguage);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Preview language switch in editor
            if (Application.isPlaying)
            {
                ApplyLanguageSettings();
            }
        }
#endif
    }

    /// <summary>
    /// ReadOnly attribute for Inspector display
    /// </summary>
    public class ReadOnlyAttribute : PropertyAttribute { }

#if UNITY_EDITOR
    /// <summary>
    /// Custom PropertyDrawer for ReadOnly attribute
    /// </summary>
    [UnityEditor.CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : UnityEditor.PropertyDrawer
    {
        public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
        {
            UnityEngine.GUI.enabled = false;
            UnityEditor.EditorGUI.PropertyField(position, property, label);
            UnityEngine.GUI.enabled = true;
        }
    }
#endif
}