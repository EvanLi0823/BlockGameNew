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

using BlockPuzzleGameToolkit.Scripts.Gameplay.Pool;
using DG.Tweening;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Gameplay
{
    /// <summary>
    /// CellDeck - 形状槽位类
    /// 管理游戏底部单个形状槽位的显示和交互
    /// 负责形状的显示、透明度控制、填充和清空动画
    /// </summary>
    public class CellDeck : MonoBehaviour
    {
        /// <summary>
        /// 当前槽位中的形状
        /// </summary>
        public Shape shape;

        /// <summary>
        /// 填充时的特效预制体
        /// </summary>
        public GameObject prefabFX;

        /// <summary>
        /// 棋盘管理器引用
        /// 用于检查形状是否可放置
        /// </summary>
        [SerializeField]
        private FieldManager field;

        /// <summary>
        /// 槽位是否为空
        /// </summary>
        public bool IsEmpty => shape == null;

        /// <summary>
        /// Unity生命周期 - 每帧更新
        /// 根据形状是否可放置来调整透明度
        /// </summary>
        private void Update()
        {
            if (shape != null)
            {
                if (field != null)
                {
                    // 检查形状是否可以放置到棋盘上
                    if (field.CanPlaceShape(shape))
                    {
                        SetShapeTransparency(shape, 1.0f);  // 可放置：完全不透明
                    }
                    else
                    {
                        SetShapeTransparency(shape, 0.1f);  // 不可放置：半透明
                    }
                }
            }
        }

        /// <summary>
        /// 填充槽位
        /// 将形状放入槽位并播放动画
        /// </summary>
        /// <param name="randomShape">要放入的形状</param>
        public void FillCell(Shape randomShape)
        {
            shape = randomShape;
            if (shape != null)
            {
                // 设置父节点和位置
                shape.transform.SetParent(transform);
                shape.transform.localPosition = Vector3.zero;
                shape.transform.localScale = Vector3.one * 0.5f;  // 缩小到一半大小

                // 播放弹出动画
                var scale = shape.transform.localScale;
                shape.transform.localScale = Vector3.zero;
                // 生成填充特效
                PoolObject.GetObject(prefabFX, shape.transform.position);
                // 缩放动画（弹性效果）
                shape.transform.DOScale(scale, 0.5f)
                    .SetEase(Ease.OutBack)
                    .OnComplete(() => { shape.transform.localScale = scale; });
            }
        }

        /// <summary>
        /// 设置形状的透明度
        /// 用于指示形状是否可放置
        /// </summary>
        /// <param name="shape">目标形状</param>
        /// <param name="alpha">透明度值（0-1）</param>
        private void SetShapeTransparency(Shape shape, float alpha)
        {
            foreach (var item in shape.GetActiveItems())
            {
                item.SetTransparency(alpha);
            }
        }

        /// <summary>
        /// 清空槽位
        /// 将形状返回对象池
        /// </summary>
        public void ClearCell()
        {
            if (shape != null)
            {
                PoolObject.Return(shape.gameObject);
                shape = null;
            }
        }

        /// <summary>
        /// 移除形状
        /// 直接销毁形状对象（不使用对象池）
        /// </summary>
        public void RemoveShape()
        {
            if (shape != null)
            {
                Destroy(shape.gameObject);
                shape = null;
            }
        }
    }
}