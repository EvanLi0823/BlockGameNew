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

using UnityEditor;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Localization.Editor
{
    /// <summary>
    /// LocalizedText组件的自定义编辑器
    /// </summary>
    [CustomEditor(typeof(LocalizedText), true)]
    [CanEditMultipleObjects]
    public class LocalizedTextEditor : UnityEditor.UI.TextEditor
    {
        private SerializedProperty instanceIDProp;
        private LocalizedText localizedText;
        private string lastKnownName;

        protected override void OnEnable()
        {
            base.OnEnable();
            instanceIDProp = serializedObject.FindProperty("instanceID");
            localizedText = (LocalizedText)target;
            lastKnownName = localizedText.gameObject.name;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // 自动使用GameObject名称作为文本（如果文本发生变化）
            if (localizedText != null && localizedText.gameObject.name != lastKnownName)
            {
                localizedText.text = localizedText.gameObject.name;
                lastKnownName = localizedText.gameObject.name;
                EditorUtility.SetDirty(localizedText);
            }

            // 显示本地化ID字段
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(instanceIDProp, new GUIContent("Localization Key"));
            if (EditorGUI.EndChangeCheck() || Event.current.type == EventType.Layout)
            {
                serializedObject.ApplyModifiedProperties();
                UpdateLocalizedText();
            }

            EditorGUILayout.Space();

            // 显示基类的Inspector
            base.OnInspectorGUI();

            // 应用修改并更新本地化文本
            if (serializedObject.ApplyModifiedProperties())
            {
                UpdateLocalizedText();
            }
        }

        /// <summary>
        /// 更新本地化文本显示
        /// </summary>
        private void UpdateLocalizedText()
        {
            if (localizedText != null && !string.IsNullOrEmpty(localizedText.instanceID))
            {
                var originalText = localizedText.text;
                var localizedString = LocalizationManager.GetText(localizedText.instanceID, originalText);

                if (localizedText.text != localizedString)
                {
                    localizedText.text = localizedString;

                    // 记录预制体修改
                    if (PrefabUtility.IsPartOfPrefabInstance(localizedText))
                    {
                        PrefabUtility.RecordPrefabInstancePropertyModifications(localizedText);
                    }

                    EditorUtility.SetDirty(localizedText);
                }
            }
        }
    }
}