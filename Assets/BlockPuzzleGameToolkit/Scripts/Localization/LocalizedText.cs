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
using UnityEngine.UI;

namespace BlockPuzzleGameToolkit.Scripts.Localization
{
    /// <summary>
    /// 本地化Text组件
    /// 继承自Unity的Text组件，自动根据语言设置更新文本内容
    /// </summary>
    [AddComponentMenu("UI/Localized Text")]
    public class LocalizedText : Text
    {
        [SerializeField]
        [Tooltip("本地化文本的键值，用于从本地化管理器获取对应语言的文本")]
        public string instanceID;

        private string originalText;

        protected override void OnEnable()
        {
            base.OnEnable();
            originalText = text;
            UpdateText();

            // Subscribe to language change event
            LocalizationManager.OnLanguageChanged += UpdateText;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            // Unsubscribe from language change event
            LocalizationManager.OnLanguageChanged -= UpdateText;
        }

        /// <summary>
        /// 更新显示的文本内容
        /// </summary>
        public void UpdateText()
        {
            if (string.IsNullOrEmpty(instanceID))
            {
                Debug.LogWarning($"[LocalizedText] Instance ID is empty for {gameObject.name}");
                return;
            }

            var newText = LocalizationManager.GetText(instanceID, originalText);
            if (text != newText)
            {
                Debug.Log($"[LocalizedText] Updating text for {gameObject.name}: '{instanceID}' -> '{newText}'");
                text = newText;
            }
            else
            {
                Debug.Log($"[LocalizedText] Text unchanged for {gameObject.name}: '{instanceID}' = '{text}'");
            }
        }
    }
}