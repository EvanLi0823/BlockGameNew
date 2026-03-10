// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using BlockPuzzleGameToolkit.Scripts.PropSystem.Core;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Settings
{
    /// <summary>
    /// 道具项配置 - 定义单个道具的基本信息和视觉资源
    /// </summary>
    [CreateAssetMenu(fileName = "PropItemConfig", menuName = "BlockPuzzle/Props/ItemConfig", order = 100)]
    public class PropItemConfig : ScriptableObject
    {
        [Header("基础信息")]
        [Tooltip("道具类型")]
        public PropType propType;

        [Tooltip("道具名称")]
        public string propName;

        [Tooltip("道具描述")]
        [TextArea(2, 4)]
        public string description;

        [Header("视觉资源")]
        [Tooltip("道具图标")]
        public Sprite propIcon;

        [Tooltip("道具使用时的特效预制体")]
        public GameObject effectPrefab;

        [Header("音效资源")]
        [Tooltip("道具使用时的音效")]
        public AudioClip useSound;

        [Header("使用配置")]
        [Tooltip("使用冷却时间（秒）")]
        [Range(0f, 5f)]
        public float cooldownTime = 0f;

        [Tooltip("最大堆叠数量")]
        [Range(1, 999)]
        public int maxStack = 99;

        [Header("高亮配置")]
        [Tooltip("选择模式下的高亮颜色")]
        public Color highlightColor = new Color(1f, 1f, 0f, 0.5f);

        [Tooltip("选择模式下的边框颜色")]
        public Color outlineColor = Color.yellow;

        /// <summary>
        /// 验证配置是否有效
        /// </summary>
        /// <returns>配置是否有效</returns>
        public bool IsValid()
        {
            if (propType == PropType.None)
            {
                Debug.LogError($"PropItemConfig {name}: 道具类型不能为None");
                return false;
            }

            if (string.IsNullOrEmpty(propName))
            {
                Debug.LogError($"PropItemConfig {name}: 道具名称不能为空");
                return false;
            }

            if (propIcon == null)
            {
                Debug.LogWarning($"PropItemConfig {name}: 道具图标未设置");
            }

            return true;
        }

        /// <summary>
        /// 在Inspector中值改变时调用
        /// </summary>
        private void OnValidate()
        {
            // 确保最大堆叠数量至少为1
            if (maxStack < 1)
            {
                maxStack = 1;
            }

            // 确保冷却时间不为负数
            if (cooldownTime < 0)
            {
                cooldownTime = 0;
            }
        }
    }
}