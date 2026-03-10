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

using UnityEngine;
using DG.Tweening;
using BlockPuzzleGameToolkit.Scripts.PropSystem.Core;
using BlockPuzzleGameToolkit.Scripts.Gameplay;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using BlockPuzzleGameToolkit.Scripts.Gameplay.Managers;using BlockPuzzleGameToolkit.Scripts.Enums;
using System.Collections;

namespace BlockPuzzleGameToolkit.Scripts.PropSystem.Behaviors
{
    /// <summary>
    /// 刷新道具行为 - 重新生成所有待放置的方块
    /// </summary>
    public class RefreshPropBehavior : PropBehaviorBase
    {
        /// <summary>
        /// 道具类型
        /// </summary>
        public override PropType PropType => PropType.Refresh;

        /// <summary>
        /// 不需要选择目标，立即执行
        /// </summary>
        public override bool RequiresTarget => false;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="manager">道具管理器引用</param>
        public override void Initialize(PropManager manager)
        {
            base.Initialize(manager);

            if (cellDeckManager == null)
            {
                Debug.LogError("RefreshPropBehavior: CellDeckManager未找到");
            }

            if (fieldManager == null)
            {
                Debug.LogError("RefreshPropBehavior: FieldManager未找到");
            }
        }

        /// <summary>
        /// 不需要选择，所以这个方法为空
        /// </summary>
        protected override void OnStartSelection()
        {
            // 刷新道具不需要选择，直接执行
            Execute();
        }

        /// <summary>
        /// 取消选择（刷新道具不需要）
        /// </summary>
        protected override void OnCancelSelection()
        {
            // 刷新道具不需要取消选择
        }

        /// <summary>
        /// 执行刷新
        /// </summary>
        public override void Execute(object target = null)
        {
            if (cellDeckManager == null)
            {
                Debug.LogError("RefreshPropBehavior: CellDeckManager未找到，无法刷新");
                return;
            }

            // 取消选择模式（如果有）
            CancelSelection();

            // 执行刷新动画和逻辑
            RefreshAllShapes();

            // 播放音效
            PlayUseSound();

            // 播放特效
            PlayRefreshEffect();
        }

        /// <summary>
        /// 刷新所有Shape
        /// </summary>
        private void RefreshAllShapes()
        {
            float animDuration = settings != null ? settings.useAnimationDuration : 0.5f;
            float staggerDelay = 0.1f;

            // 先收集所有当前的Shape
            var oldShapes = new System.Collections.Generic.List<Shape>();
            foreach (var deck in cellDeckManager.cellDecks)
            {
                if (deck != null && !deck.IsEmpty && deck.shape != null)
                {
                    oldShapes.Add(deck.shape);
                }
            }

            // 播放消失动画
            for (int i = 0; i < oldShapes.Count; i++)
            {
                var shape = oldShapes[i];
                if (shape != null)
                {
                    float delay = i * staggerDelay;

                    // 缩放消失
                    shape.transform.DOScale(Vector3.zero, animDuration)
                        .SetDelay(delay)
                        .SetEase(Ease.InBack);

                    // 旋转
                    shape.transform.DORotate(new Vector3(0, 0, 360), animDuration, RotateMode.FastBeyond360)
                        .SetDelay(delay);

                    // 淡出
                    var canvasGroup = shape.GetComponent<CanvasGroup>();
                    if (canvasGroup == null)
                    {
                        canvasGroup = shape.gameObject.AddComponent<CanvasGroup>();
                    }
                    canvasGroup.DOFade(0, animDuration).SetDelay(delay);
                }
            }

            // 延迟后生成新的Shape
            float totalAnimTime = animDuration + (oldShapes.Count - 1) * staggerDelay;
            DOVirtual.DelayedCall(totalAnimTime, () =>
            {
                // 销毁旧的Shape
                foreach (var shape in oldShapes)
                {
                    if (shape != null)
                    {
                        Object.Destroy(shape.gameObject);
                    }
                }

                // 生成新的Shape
                GenerateNewShapes();
            });
        }

        /// <summary>
        /// 生成新的Shape
        /// </summary>
        private void GenerateNewShapes()
        {
            if (cellDeckManager == null) return;

            // 调用CellDeckManager的刷新方法（使用RefreshShapesForRevive）
            cellDeckManager.RefreshShapesForRevive(3, true); // 刷新3个形状，确保至少一个可放置

            // 确保至少有一个可放置的Shape
            EnsurePlayability();

            // 播放出现动画
            PlayAppearAnimation();

            // 触发形状放置事件（表示形状更新了）
            EventManager.GetEvent(EGameEvent.ShapePlaced)?.Invoke();
        }

        /// <summary>
        /// 确保可玩性（至少有一个Shape可以放置）
        /// </summary>
        private void EnsurePlayability()
        {
            if (fieldManager == null || cellDeckManager == null) return;

            bool hasPlayableShape = false;
            int attempts = 0;
            const int maxAttempts = 10;

            // 检查是否有可放置的Shape
            while (!hasPlayableShape && attempts < maxAttempts)
            {
                foreach (var deck in cellDeckManager.cellDecks)
                {
                    if (deck != null && !deck.IsEmpty && deck.shape != null)
                    {
                        if (fieldManager.CanPlaceShape(deck.shape))
                        {
                            hasPlayableShape = true;
                            break;
                        }
                    }
                }

                if (!hasPlayableShape)
                {
                    // 如果没有可放置的Shape，重新生成
                    Debug.Log("RefreshPropBehavior: 没有可放置的Shape，重新生成");
                    
                    // 销毁现有Shape
                    foreach (var deck in cellDeckManager.cellDecks)
                    {
                        if (deck != null && deck.shape != null)
                        {
                            Object.Destroy(deck.shape.gameObject);
                            deck.shape = null;
                        }
                    }

                    // 重新生成
                    cellDeckManager.RefreshShapesForRevive(3, true);
                    attempts++;
                }
            }

            if (!hasPlayableShape)
            {
                Debug.LogWarning("RefreshPropBehavior: 无法生成可放置的Shape");
            }
        }

        /// <summary>
        /// 播放出现动画
        /// </summary>
        private void PlayAppearAnimation()
        {
            float animDuration = settings != null ? settings.useAnimationDuration : 0.5f;
            float staggerDelay = 0.1f;
            int index = 0;

            foreach (var deck in cellDeckManager.cellDecks)
            {
                if (deck != null && !deck.IsEmpty && deck.shape != null)
                {
                    var shape = deck.shape;
                    float delay = index * staggerDelay;

                    // 初始状态
                    shape.transform.localScale = Vector3.zero;
                    
                    var canvasGroup = shape.GetComponent<CanvasGroup>();
                    if (canvasGroup == null)
                    {
                        canvasGroup = shape.gameObject.AddComponent<CanvasGroup>();
                    }
                    canvasGroup.alpha = 0;

                    // 缩放出现
                    shape.transform.DOScale(Vector3.one, animDuration)
                        .SetDelay(delay)
                        .SetEase(Ease.OutBack);

                    // 淡入
                    canvasGroup.DOFade(1, animDuration).SetDelay(delay);

                    // 添加弹跳效果
                    shape.transform.DOPunchPosition(Vector3.up * 20, animDuration, 5, 0.5f)
                        .SetDelay(delay + animDuration);

                    index++;
                }
            }
        }

        /// <summary>
        /// 播放刷新特效
        /// </summary>
        private void PlayRefreshEffect()
        {
            // 获取屏幕中心位置
            var centerPosition = Vector3.zero;
            if (cellDeckManager != null && cellDeckManager.cellDecks.Length > 0)
            {
                // 计算所有CellDeck的中心位置
                Vector3 sum = Vector3.zero;
                int count = 0;
                foreach (var deck in cellDeckManager.cellDecks)
                {
                    if (deck != null)
                    {
                        sum += deck.transform.position;
                        count++;
                    }
                }
                if (count > 0)
                {
                    centerPosition = sum / count;
                }
            }

            // 播放特效
            PlayEffect(centerPosition);
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        public override void Cleanup()
        {
            base.Cleanup();
        }
    }
}
