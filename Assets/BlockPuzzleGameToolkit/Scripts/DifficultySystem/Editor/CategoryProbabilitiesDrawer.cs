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

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace GameCore.DifficultySystem.Editor
{
    /// <summary>
    /// CategoryProbabilities的自定义PropertyDrawer
    /// 提供纯文本输入框方式
    /// </summary>
    [CustomPropertyDrawer(typeof(CategoryProbabilities))]
    public class CategoryProbabilitiesDrawer : PropertyDrawer
    {
        private const float LabelWidth = 80f;
        private const float TextFieldWidth = 60f;
        private const float Spacing = 5f;
        private const float LineHeight = 18f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // 标题行 + 3个字段行
            return LineHeight * 4 + Spacing * 3;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // 绘制标题
            Rect labelRect = new Rect(position.x, position.y, position.width, LineHeight);
            EditorGUI.LabelField(labelRect, label, EditorStyles.boldLabel);

            // 缩进
            EditorGUI.indentLevel++;

            float yOffset = position.y + LineHeight + Spacing;

            // 获取字段
            SerializedProperty basicProp = property.FindPropertyRelative("basic");
            SerializedProperty shapedProp = property.FindPropertyRelative("shaped");
            SerializedProperty largeProp = property.FindPropertyRelative("large");

            // 绘制三个字段（纯文本输入）
            yOffset = DrawProbabilityField(position, yOffset, "Basic", basicProp);
            yOffset = DrawProbabilityField(position, yOffset, "Shaped", shapedProp);
            yOffset = DrawProbabilityField(position, yOffset, "Large", largeProp);

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }

        /// <summary>
        /// 绘制单个概率字段（纯文本框）
        /// </summary>
        private float DrawProbabilityField(Rect position, float yOffset, string label, SerializedProperty property)
        {
            Rect lineRect = new Rect(position.x, yOffset, position.width, LineHeight);

            // 标签
            Rect labelRect = new Rect(lineRect.x, lineRect.y, LabelWidth, lineRect.height);
            EditorGUI.LabelField(labelRect, label);

            // 文本输入框
            Rect textRect = new Rect(labelRect.xMax + Spacing, lineRect.y, TextFieldWidth, lineRect.height);
            property.floatValue = EditorGUI.FloatField(textRect, property.floatValue);

            return yOffset + LineHeight + Spacing;
        }
    }
}
#endif
