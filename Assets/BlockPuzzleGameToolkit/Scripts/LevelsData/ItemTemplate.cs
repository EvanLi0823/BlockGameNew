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

using BlockPuzzleGameToolkit.Scripts.Gameplay;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.LevelsData
{
    /// <summary>
    /// ItemTemplate - 物品模板数据类
    /// 定义方块的外观属性，包括7层渲染系统的颜色和精灵
    /// 作为ScriptableObject可以在Unity编辑器中创建和配置
    /// </summary>
    [CreateAssetMenu(fileName = "ItemTemplate", menuName = "BlockPuzzleGameToolkit/Items/ItemTemplate", order = 1)]
    public class ItemTemplate : ScriptableData
    {
        // ========== 七层颜色系统 ==========
        // 每层都有独立的颜色配置，可以组合出丰富的视觉效果

        /// <summary>
        /// 第1层：背景色 - 最底层的基础颜色
        /// </summary>
        public Color backgroundColor;

        /// <summary>
        /// 第2层：底层装饰色 - 在背景之上的装饰层
        /// </summary>
        public Color underlayColor;

        /// <summary>
        /// 第3层：底部边缘色 - 底部边缘效果
        /// </summary>
        public Color bottomColor;

        /// <summary>
        /// 第4层：顶部边缘色 - 顶部边缘效果
        /// </summary>
        public Color topColor;

        /// <summary>
        /// 第5层：左侧边缘色 - 左侧边缘效果
        /// </summary>
        public Color leftColor;

        /// <summary>
        /// 第6层：右侧边缘色 - 右侧边缘效果
        /// </summary>
        public Color rightColor;

        /// <summary>
        /// 第7层：覆盖层色 - 最顶层的覆盖效果
        /// </summary>
        public Color overlayColor;

        // ========== 七层精灵系统 ==========
        // 每层都可以有独立的精灵图片，用于创建复杂的视觉效果

        /// <summary>
        /// 背景精灵
        /// </summary>
        public Sprite backgroundSprite;

        /// <summary>
        /// 底层装饰精灵
        /// </summary>
        public Sprite underlaySprite;

        /// <summary>
        /// 底部边缘精灵
        /// </summary>
        public Sprite bottomSprite;

        /// <summary>
        /// 顶部边缘精灵
        /// </summary>
        public Sprite topSprite;

        /// <summary>
        /// 左侧边缘精灵
        /// </summary>
        public Sprite leftSprite;

        /// <summary>
        /// 右侧边缘精灵
        /// </summary>
        public Sprite rightSprite;

        /// <summary>
        /// 覆盖层精灵
        /// </summary>
        public Sprite overlaySprite;

        /// <summary>
        /// 层级启用控制数组
        /// 控制每一层是否显示（true=显示，false=隐藏）
        /// 索引对应：0=背景，1=底层装饰，2=底部，3=顶部，4=左侧，5=右侧，6=覆盖层
        /// </summary>
        public bool[] colorEnable = new bool[7] { true, true, true, true, true, true, true };

        /// <summary>
        /// 自定义Item预制体（可选）
        /// 如果设置了此项，将使用自定义预制体替代标准的7层渲染系统
        /// </summary>
        public Item customItemPrefab;

        /// <summary>
        /// 检查是否使用自定义预制体
        /// </summary>
        /// <returns>是否有自定义预制体</returns>
        public bool HasCustomPrefab() => customItemPrefab != null;
    }
}