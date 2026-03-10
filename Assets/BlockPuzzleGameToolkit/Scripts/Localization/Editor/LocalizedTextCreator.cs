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
using UnityEngine.UI;

namespace BlockPuzzleGameToolkit.Scripts.Localization.Editor
{
    /// <summary>
    /// 创建LocalizedText组件的菜单项
    /// </summary>
    public class LocalizedTextCreator
    {
        [MenuItem("GameObject/UI/Localized Text", false, 2001)]
        private static void CreateLocalizedText()
        {
            // 创建新的GameObject
            var go = new GameObject("Localized Text");

            // 添加RectTransform（UI组件必需）
            var rectTransform = go.AddComponent<RectTransform>();

            // 添加LocalizedText组件
            var localizedText = go.AddComponent<LocalizedText>();

            // 设置默认属性
            localizedText.text = "New Text";
            localizedText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            localizedText.fontSize = 14;
            localizedText.alignment = TextAnchor.MiddleCenter;
            localizedText.color = Color.black;

            // 设置RectTransform的默认大小
            rectTransform.sizeDelta = new Vector2(160, 30);

            // 获取或创建Canvas
            Canvas canvas = null;

            // 检查当前选中的对象
            if (Selection.activeGameObject != null)
            {
                // 检查选中对象或其父对象是否有Canvas
                canvas = Selection.activeGameObject.GetComponentInParent<Canvas>();

                if (canvas != null)
                {
                    // 设置为选中对象的子对象
                    go.transform.SetParent(Selection.activeGameObject.transform, false);
                }
            }

            // 如果没有找到Canvas，创建一个新的
            if (canvas == null)
            {
                canvas = Object.FindObjectOfType<Canvas>();

                if (canvas == null)
                {
                    // 创建新的Canvas
                    var canvasGO = new GameObject("Canvas");
                    canvas = canvasGO.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvasGO.AddComponent<CanvasScaler>();
                    canvasGO.AddComponent<GraphicRaycaster>();

                    // 创建EventSystem（如果不存在）
                    if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
                    {
                        var eventSystemGO = new GameObject("EventSystem");
                        eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
                        eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                    }
                }

                // 设置为Canvas的子对象
                go.transform.SetParent(canvas.transform, false);
            }

            // 注册撤销操作
            Undo.RegisterCreatedObjectUndo(go, "Create Localized Text");

            // 选中新创建的对象
            Selection.activeGameObject = go;
        }

        /// <summary>
        /// 将现有的Text组件转换为LocalizedText
        /// </summary>
        [MenuItem("CONTEXT/Text/Convert to Localized Text")]
        private static void ConvertToLocalizedText(MenuCommand command)
        {
            var originalText = (Text)command.context;
            var go = originalText.gameObject;

            // 保存原始Text的属性
            var text = originalText.text;
            var font = originalText.font;
            var fontSize = originalText.fontSize;
            var fontStyle = originalText.fontStyle;
            var lineSpacing = originalText.lineSpacing;
            var supportRichText = originalText.supportRichText;
            var alignment = originalText.alignment;
            var alignByGeometry = originalText.alignByGeometry;
            var horizontalOverflow = originalText.horizontalOverflow;
            var verticalOverflow = originalText.verticalOverflow;
            var resizeTextForBestFit = originalText.resizeTextForBestFit;
            var resizeTextMinSize = originalText.resizeTextMinSize;
            var resizeTextMaxSize = originalText.resizeTextMaxSize;
            var color = originalText.color;
            var material = originalText.material;
            var raycastTarget = originalText.raycastTarget;

            // 记录撤销操作
            Undo.RecordObject(go, "Convert to Localized Text");

            // 删除原始Text组件
            Undo.DestroyObjectImmediate(originalText);

            // 添加LocalizedText组件
            var localizedText = Undo.AddComponent<LocalizedText>(go);

            // 恢复属性
            localizedText.text = text;
            localizedText.font = font;
            localizedText.fontSize = fontSize;
            localizedText.fontStyle = fontStyle;
            localizedText.lineSpacing = lineSpacing;
            localizedText.supportRichText = supportRichText;
            localizedText.alignment = alignment;
            localizedText.alignByGeometry = alignByGeometry;
            localizedText.horizontalOverflow = horizontalOverflow;
            localizedText.verticalOverflow = verticalOverflow;
            localizedText.resizeTextForBestFit = resizeTextForBestFit;
            localizedText.resizeTextMinSize = resizeTextMinSize;
            localizedText.resizeTextMaxSize = resizeTextMaxSize;
            localizedText.color = color;
            localizedText.material = material;
            localizedText.raycastTarget = raycastTarget;

            // 设置instanceID为GameObject的名称（作为默认值）
            localizedText.instanceID = go.name;

            EditorUtility.SetDirty(go);
        }
    }
}