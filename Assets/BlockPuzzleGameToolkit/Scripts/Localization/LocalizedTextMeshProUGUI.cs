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

using TMPro;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Localization
{
    public class LocalizedTextMeshProUGUI : TextMeshProUGUI
    {
        [SerializeField]
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

        public void UpdateText()
        {
            if (string.IsNullOrEmpty(instanceID))
            {
                Debug.LogWarning($"[LocalizedTextMeshProUGUI] Instance ID is empty for {gameObject.name}");
                return;
            }

            var newText = LocalizationManager.GetText(instanceID, originalText);
            if (text != newText)
            {
                Debug.Log($"[LocalizedTextMeshProUGUI] Updating text for {gameObject.name}: '{instanceID}' -> '{newText}'");
                text = newText;
            }
        }
    }
}