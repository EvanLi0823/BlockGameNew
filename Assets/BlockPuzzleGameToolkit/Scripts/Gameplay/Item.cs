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

using BlockPuzzleGameToolkit.Scripts.LevelsData;
using UnityEngine;
using UnityEngine.UI;

namespace BlockPuzzleGameToolkit.Scripts.Gameplay
{
    /// <summary>
    /// Item 类表示游戏中的方块单元
    /// 采用独特的7层渲染系统，每层可独立控制颜色和精灵
    /// 支持奖励道具的显示
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class Item : FillAndPreview
    {
        // 物品的数据模板，定义外观属性
        public ItemTemplate itemTemplate;

        // ========== 七层渲染系统 ==========
        // 每一层都是独立的Image组件，可以单独控制颜色、精灵和透明度

        // 第1层：背景层 - 最底层的基础颜色
        public Image backgroundColor;

        // 第2层：底层装饰 - 在背景之上的装饰层
        public Image underlayColor;

        // 第3层：底部边缘 - 底部边缘效果
        public Image bottomColor;

        // 第4层：顶部边缘 - 顶部边缘效果
        public Image topColor;

        // 第5层：左侧边缘 - 左侧边缘效果
        public Image leftColor;

        // 第6层：右侧边缘 - 右侧边缘效果
        public Image rightColor;

        // 第7层：覆盖层 - 最顶层的覆盖效果
        public Image overlayColor;

        // 物品在Shape中的位置坐标（x,y）
        private Vector2Int position;

        // 奖励道具的显示组件
        public Bonus bonus;

        // 奖励道具的数据模板
        public BonusItemTemplate bonusItemTemplate;

        /// <summary>
        /// Unity生命周期 - 初始化
        /// </summary>
        private void Awake()
        {
            // 默认隐藏奖励道具显示
            bonus?.gameObject.SetActive(false);
            // 如果已有模板数据，立即更新显示
            if (itemTemplate != null)
            {
                UpdateColor(itemTemplate);
            }
        }

        /// <summary>
        /// 更新物品的颜色和精灵 - 核心渲染方法
        /// 根据ItemTemplate的数据更新所有7层的显示
        /// </summary>
        /// <param name="itemTemplate">包含颜色和精灵数据的模板</param>
        public void UpdateColor(ItemTemplate itemTemplate)
        {
            // 如果使用自定义预制体，跳过颜色更新
            if(itemTemplate.HasCustomPrefab())
                return;

            this.itemTemplate = itemTemplate;

            // 更新每一层的颜色
            backgroundColor.color = itemTemplate.backgroundColor;
            underlayColor.color = itemTemplate.underlayColor;
            bottomColor.color = itemTemplate.bottomColor;
            topColor.color = itemTemplate.topColor;
            leftColor.color = itemTemplate.leftColor;
            rightColor.color = itemTemplate.rightColor;
            overlayColor.color = itemTemplate.overlayColor;

            // 根据启用标记控制每层的显示/隐藏
            UpdateEnableColors(itemTemplate);

            // 更新每一层的精灵图片
            backgroundColor.sprite = itemTemplate.backgroundSprite;
            underlayColor.sprite = itemTemplate.underlaySprite;
            bottomColor.sprite = itemTemplate.bottomSprite;
            topColor.sprite = itemTemplate.topSprite;
            leftColor.sprite = itemTemplate.leftSprite;
            rightColor.sprite = itemTemplate.rightSprite;
            overlayColor.sprite = itemTemplate.overlaySprite;
        }

        /// <summary>
        /// 根据启用标记更新每层的透明度
        /// colorEnable数组控制每层是否显示
        /// </summary>
        /// <param name="itemTemplate">包含启用标记的模板</param>
        private void UpdateEnableColors(ItemTemplate itemTemplate)
        {
            // 通过设置alpha值为0或1来控制层的显示/隐藏
            backgroundColor.color = new Color(backgroundColor.color.r, backgroundColor.color.g, backgroundColor.color.b,
                itemTemplate.colorEnable[0] ? 1f : 0f);
            underlayColor.color = new Color(underlayColor.color.r, underlayColor.color.g, underlayColor.color.b,
                itemTemplate.colorEnable[1] ? 1f : 0f);
            bottomColor.color = new Color(bottomColor.color.r, bottomColor.color.g, bottomColor.color.b,
                itemTemplate.colorEnable[2] ? 1f : 0f);
            topColor.color = new Color(topColor.color.r, topColor.color.g, topColor.color.b,
                itemTemplate.colorEnable[3] ? 1f : 0f);
            leftColor.color = new Color(leftColor.color.r, leftColor.color.g, leftColor.color.b,
                itemTemplate.colorEnable[4] ? 1f : 0f);
            rightColor.color = new Color(rightColor.color.r, rightColor.color.g, rightColor.color.b,
                itemTemplate.colorEnable[5] ? 1f : 0f);
            overlayColor.color = new Color(overlayColor.color.r, overlayColor.color.g, overlayColor.color.b,
                itemTemplate.colorEnable[6] ? 1f : 0f);
        }

        /// <summary>
        /// 设置奖励道具
        /// </summary>
        /// <param name="template">奖励道具模板</param>
        public void SetBonus(BonusItemTemplate template)
        {
            bonusItemTemplate = template;
            bonus.gameObject.SetActive(true);
            bonus.FillIcon(template);
        }

        /// <summary>
        /// 重写父类方法 - 填充图标
        /// </summary>
        /// <param name="iconScriptable">图标数据</param>
        public override void FillIcon(ScriptableData iconScriptable)
        {
            UpdateColor((ItemTemplate)iconScriptable);
        }

        /// <summary>
        /// 设置物品在Shape中的位置
        /// </summary>
        /// <param name="vector2Int">二维坐标位置</param>
        public void SetPosition(Vector2Int vector2Int)
        {
            position = vector2Int;
        }

        /// <summary>
        /// 获取物品在Shape中的位置
        /// </summary>
        /// <returns>二维坐标位置</returns>
        public Vector2Int GetPosition()
        {
            return position;
        }

        /// <summary>
        /// 检查是否有奖励道具
        /// </summary>
        /// <returns>是否包含奖励道具</returns>
        public bool HasBonusItem()
        {
            return bonusItemTemplate != null;
        }

        /// <summary>
        /// 清除奖励道具
        /// </summary>
        public void ClearBonus()
        {
            bonusItemTemplate = null;
            bonus.gameObject.SetActive(false);
        }

        /// <summary>
        /// 设置物品的整体透明度
        /// 用于预览、高亮等效果
        /// </summary>
        /// <param name="alpha">透明度值（0-1）</param>
        public void SetTransparency(float alpha)
        {
            // 先更新启用状态
            UpdateEnableColors(itemTemplate);

            // 只对原本不透明的层设置透明度
            // 保持原本透明的层不变
            var color = backgroundColor.color;
            color.a = alpha;
            if(backgroundColor.color.a != 0f)
                backgroundColor.color = color;

            color = underlayColor.color;
            color.a = alpha;
            if(underlayColor.color.a != 0f)
                underlayColor.color = color;

            color = bottomColor.color;
            color.a = alpha;
            if(bottomColor.color.a != 0f)
                bottomColor.color = color;

            color = topColor.color;
            color.a = alpha;
            if(topColor.color.a != 0f)
                topColor.color = color;

            color = leftColor.color;
            color.a = alpha;
            if(leftColor.color.a != 0f)
                leftColor.color = color;

            color = rightColor.color;
            color.a = alpha;
            if(rightColor.color.a != 0f)
                rightColor.color = color;

            color = overlayColor.color;
            color.a = alpha;
            if(overlayColor.color.a != 0f)
                overlayColor.color = color;

            // 同步设置奖励道具的透明度
            bonus?.SetTransparency(alpha);
        }
    }
}