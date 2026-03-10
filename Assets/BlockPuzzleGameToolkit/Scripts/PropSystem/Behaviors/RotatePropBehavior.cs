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

using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using BlockPuzzleGameToolkit.Scripts.PropSystem.Core;
using BlockPuzzleGameToolkit.Scripts.Gameplay;
using BlockPuzzleGameToolkit.Scripts.Gameplay.Managers;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using BlockPuzzleGameToolkit.Scripts.Enums;

namespace BlockPuzzleGameToolkit.Scripts.PropSystem.Behaviors
{
    /// <summary>
    /// 旋转道具行为 - 顺时针旋转选中的方块90度
    /// </summary>
    public class RotatePropBehavior : PropBehaviorBase
    {
        /// <summary>
        /// 道具类型
        /// </summary>
        public override PropType PropType => PropType.Rotate;

        /// <summary>
        /// 需要选择目标Shape
        /// </summary>
        public override bool RequiresTarget => true;

        /// <summary>
        /// 可选择的CellDeck列表
        /// </summary>
        private List<CellDeck> selectableDecks;

        /// <summary>
        /// 当前高亮的Shape
        /// </summary>
        private Shape highlightedShape;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="manager">道具管理器引用</param>
        public override void Initialize(PropManager manager)
        {
            base.Initialize(manager);

            selectableDecks = new List<CellDeck>();

            if (cellDeckManager == null)
            {
                Debug.LogError("RotatePropBehavior: CellDeckManager未找到");
            }

            if (fieldManager == null)
            {
                Debug.LogError("RotatePropBehavior: FieldManager未找到");
            }
        }

        /// <summary>
        /// 开始选择时高亮可旋转的Shape
        /// </summary>
        protected override void OnStartSelection()
        {
            if (cellDeckManager == null) return;

            selectableDecks.Clear();

            // 遍历所有CellDeck，找出可以旋转的Shape
            foreach (var deck in cellDeckManager.cellDecks)
            {
                if (deck != null && !deck.IsEmpty)
                {
                    var shape = deck.shape;
                    if (shape != null)
                    {
                        // 检查Shape是否可以放置（只有可放置的Shape才能旋转）
                        bool canPlace = fieldManager != null && fieldManager.CanPlaceShape(shape);
                        
                        if (canPlace)
                        {
                            // 使用CanvasGroup设置透明度（完全不透明表示可选择）
                            var canvasGroup = shape.GetComponent<CanvasGroup>();
                            if (canvasGroup == null)
                            {
                                canvasGroup = shape.gameObject.AddComponent<CanvasGroup>();
                            }
                            canvasGroup.alpha = 1.0f;

                            // 添加发光效果（如果Shape有Outline组件）
                            var outline = shape.GetComponent<UnityEngine.UI.Outline>();
                            if (outline != null)
                            {
                                outline.enabled = true;
                                outline.effectColor = Color.yellow;
                                outline.effectDistance = new Vector2(2, 2);
                            }

                            // 添加脉冲动画
                            AddShapePulseAnimation(shape);

                            selectableDecks.Add(deck);
                        }
                        else
                        {
                            // 不可放置的Shape设为半透明
                            var canvasGroup = shape.GetComponent<CanvasGroup>();
                            if (canvasGroup == null)
                            {
                                canvasGroup = shape.gameObject.AddComponent<CanvasGroup>();
                            }
                            canvasGroup.alpha = 0.3f;
                        }
                    }
                }
            }

            if (selectableDecks.Count == 0)
            {
                Debug.Log("RotatePropBehavior: 没有可以旋转的方块");
            }
        }

        /// <summary>
        /// 取消选择时恢复所有Shape状态
        /// </summary>
        protected override void OnCancelSelection()
        {
            // 恢复所有Shape的状态
            foreach (var deck in selectableDecks)
            {
                if (deck != null && deck.shape != null)
                {
                    // 恢复透明度
                    var canvasGroup = deck.shape.GetComponent<CanvasGroup>();
                    if (canvasGroup != null)
                    {
                        canvasGroup.alpha = 1.0f;
                    }

                    // 移除发光效果
                    var outline = deck.shape.GetComponent<UnityEngine.UI.Outline>();
                    if (outline != null)
                    {
                        outline.enabled = false;
                    }

                    // 停止动画
                    deck.shape.transform.DOKill();
                    deck.shape.transform.localScale = Vector3.one;
                }
            }

            selectableDecks.Clear();
            highlightedShape = null;
        }

        /// <summary>
        /// 检查目标是否有效
        /// </summary>
        public override bool CanExecute(object target)
        {
            // 检查目标是否是Shape或CellDeck
            if (target is Shape shape)
            {
                // 检查是否在可选择列表中
                return selectableDecks.Exists(d => d.shape == shape);
            }
            else if (target is CellDeck deck)
            {
                // 检查是否在可选择列表中
                return selectableDecks.Contains(deck);
            }

            return false;
        }

        /// <summary>
        /// 执行旋转
        /// </summary>
        public override void Execute(object target = null)
        {
            Shape shapeToRotate = null;

            // 获取要旋转的Shape
            if (target is Shape shape)
            {
                shapeToRotate = shape;
            }
            else if (target is CellDeck deck && deck.shape != null)
            {
                shapeToRotate = deck.shape;
            }

            if (shapeToRotate == null || !CanExecute(target))
            {
                Debug.LogWarning("RotatePropBehavior: 无效的旋转目标");
                return;
            }

            // 先取消选择模式
            CancelSelection();

            // 执行旋转
            RotateShape(shapeToRotate);

            // 播放音效
            PlayUseSound();
        }

        /// <summary>
        /// 显示旋转预览
        /// </summary>
        protected override void OnShowPreview(object target)
        {
            Shape targetShape = null;

            if (target is Shape shape)
            {
                targetShape = shape;
            }
            else if (target is CellDeck deck)
            {
                targetShape = deck.shape;
            }

            if (targetShape == null) return;

            // 隐藏之前的预览
            if (highlightedShape != null && highlightedShape != targetShape)
            {
                ResetShapeHighlight(highlightedShape);
            }

            // 高亮新的Shape
            highlightedShape = targetShape;

            // 增强高亮效果
            var outline = highlightedShape.GetComponent<UnityEngine.UI.Outline>();
            if (outline != null)
            {
                outline.effectColor = Color.green;
                outline.effectDistance = new Vector2(3, 3);
            }

            // 显示旋转预览（轻微的旋转动画）
            highlightedShape.transform.DORotate(new Vector3(0, 0, -10), 0.3f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        /// <summary>
        /// 隐藏旋转预览
        /// </summary>
        protected override void OnHidePreview()
        {
            if (highlightedShape != null)
            {
                ResetShapeHighlight(highlightedShape);
                highlightedShape = null;
            }
        }

        /// <summary>
        /// 执行Shape旋转
        /// </summary>
        private void RotateShape(Shape shape)
        {
            if (shape == null) return;

            float animDuration = settings != null ? settings.useAnimationDuration : 0.3f;

            // 获取旋转中心（使用Shape的transform位置）
            var pivot = shape.transform.position;

            // 旋转动画（顺时针90度）
            shape.transform.DORotate(new Vector3(0, 0, -90), animDuration, RotateMode.LocalAxisAdd)
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    // 更新Shape内部数据结构
                    UpdateShapeData(shape);

                    // 检查旋转后是否还能放置
                    if (fieldManager != null)
                    {
                        bool canPlace = fieldManager.CanPlaceShape(shape);
                        if (!canPlace)
                        {
                            Debug.Log("RotatePropBehavior: 旋转后的形状无法放置");
                            // 可以选择再次旋转或其他处理
                        }
                    }

                    // 触发形状放置事件（用于更新）
                    EventManager.GetEvent(EGameEvent.ShapePlaced)?.Invoke();
                });

            // 播放特效
            PlayEffect(shape.transform.position);

            // 添加缩放反馈
            shape.transform.DOPunchScale(Vector3.one * 0.1f, animDuration * 0.5f, 5, 0.5f);
        }

        /// <summary>
        /// 更新Shape内部数据
        /// </summary>
        private void UpdateShapeData(Shape shape)
        {
            if (shape == null) return;

            // 旋转后，Shape的内部数据会自动更新
            // 如果需要手动更新，可以调用Shape的Update方法（如果有）
            // 这里暂时不需要额外操作，因为Unity的Transform已经处理了旋转
        }

        /// <summary>
        /// 添加Shape脉冲动画
        /// </summary>
        private void AddShapePulseAnimation(Shape shape)
        {
            if (shape == null) return;

            var sequence = DOTween.Sequence();
            sequence.Append(shape.transform.DOScale(Vector3.one * 1.05f, 0.5f));
            sequence.Append(shape.transform.DOScale(Vector3.one, 0.5f));
            sequence.SetLoops(-1);
        }

        /// <summary>
        /// 重置Shape高亮
        /// </summary>
        private void ResetShapeHighlight(Shape shape)
        {
            if (shape == null) return;

            // 停止动画
            shape.transform.DOKill();
            shape.transform.rotation = Quaternion.identity;

            // 恢复Outline
            var outline = shape.GetComponent<UnityEngine.UI.Outline>();
            if (outline != null)
            {
                outline.effectColor = Color.yellow;
                outline.effectDistance = new Vector2(2, 2);
            }
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        public override void Cleanup()
        {
            CancelSelection();
            selectableDecks?.Clear();
            highlightedShape = null;
            base.Cleanup();
        }
    }
}
