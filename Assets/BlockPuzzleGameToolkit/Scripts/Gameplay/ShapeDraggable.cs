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

using System.Collections.Generic;
using System.Linq;
using BlockPuzzleGameToolkit.Scripts.Audio;
using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using BlockPuzzleGameToolkit.Scripts.GameCore.Haptic;
using BlockPuzzleGameToolkit.Scripts.Gameplay.Managers;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace BlockPuzzleGameToolkit.Scripts.Gameplay
{
    /// <summary>
    /// ShapeDraggable - 形状拖拽控制器
    /// 处理形状的拖拽操作，包括触摸、鼠标和虚拟鼠标输入
    /// 负责形状的拖动、高亮预览、放置验证等核心交互逻辑
    /// </summary>
    public class ShapeDraggable : MonoBehaviour
    {
        // ========== 基础组件 ==========
        /// <summary>
        /// RectTransform组件引用
        /// </summary>
        private RectTransform rectTransform;

        /// <summary>
        /// 原始位置（拖拽前的位置）
        /// </summary>
        private Vector2 originalPosition;

        /// <summary>
        /// 触摸偏移量（触摸点与形状中心的偏移）
        /// </summary>
        private Vector2 touchOffset;

        /// <summary>
        /// 拖拽时的垂直偏移量（让形状显示在手指上方）
        /// </summary>
        private readonly float verticalOffset = 300;

        /// <summary>
        /// 原始缩放值
        /// </summary>
        private Vector3 originalScale;

        /// <summary>
        /// 是否正在拖拽
        /// </summary>
        private bool isDragging;

        /// <summary>
        /// 当前活动的触摸ID（用于多点触摸识别）
        /// </summary>
        private int activeTouchId = -1;

        /// <summary>
        /// Canvas引用
        /// </summary>
        private Canvas canvas;

        /// <summary>
        /// 事件相机引用
        /// </summary>
        private Camera eventCamera;

        // ========== 游戏对象引用 ==========
        /// <summary>
        /// 形状组件引用
        /// </summary>
        private Shape shape;

        /// <summary>
        /// 形状包含的所有Item列表
        /// </summary>
        private List<Item> _items = new();

        /// <summary>
        /// 高亮管理器引用
        /// </summary>
        private HighlightManager highlightManager;

        /// <summary>
        /// 棋盘管理器引用
        /// </summary>
        private FieldManager field;

        /// <summary>
        /// 物品工厂引用
        /// </summary>
        private ItemFactory itemFactory;

        /// <summary>
        /// 虚拟鼠标输入引用（手柄支持）
        /// </summary>
        private VirtualMouseInput virtualMouseInput;

        /// <summary>
        /// 虚拟鼠标上一帧的按下状态
        /// </summary>
        private bool wasVirtualMousePressed = false;

        /// <summary>
        /// 计时器管理器引用
        /// </summary>
        private TimerManager timerManager;

        /// <summary>
        /// Unity生命周期 - 启用时初始化
        /// 获取组件引用并订阅事件
        /// </summary>
        private void OnEnable()
        {
            // 获取管理器引用（使用空合并赋值运算符）
            itemFactory ??= FindObjectOfType<ItemFactory>();
            rectTransform = GetComponent<RectTransform>();
            shape = GetComponent<Shape>();
            shape.OnShapeUpdated += UpdateItems;
            UpdateItems();
            highlightManager ??= FindObjectOfType<HighlightManager>();
            field ??= FindObjectOfType<FieldManager>();
            timerManager ??= FindObjectOfType<TimerManager>();

            // 获取Canvas和相机引用
            canvas = GetComponentInParent<Canvas>();
            eventCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

            // 查找虚拟鼠标输入（如果可用）
            virtualMouseInput ??= FindObjectOfType<VirtualMouseInput>();

            // 订阅可以取消拖拽的事件
            EventManager.GetEvent(EGameEvent.TimerExpired).Subscribe(CancelDragIfActive);
            EventManager.GetEvent(EGameEvent.LevelAboutToComplete).Subscribe(CancelDragIfActive);
            EventManager.GetEvent(EGameEvent.TutorialCompleted).Subscribe(CancelDragIfActive);
        }

        /// <summary>
        /// Unity生命周期 - 禁用时清理
        /// 取消事件订阅并结束拖拽
        /// </summary>
        private void OnDisable()
        {
            shape.OnShapeUpdated -= UpdateItems;
            EventManager.GetEvent(EGameEvent.TimerExpired).Unsubscribe(CancelDragIfActive);
            EventManager.GetEvent(EGameEvent.LevelAboutToComplete).Unsubscribe(CancelDragIfActive);
            EventManager.GetEvent(EGameEvent.TutorialCompleted).Unsubscribe(CancelDragIfActive);
            EndDrag();
        }

        /// <summary>
        /// 如果正在拖拽则取消
        /// 用于响应游戏状态变化
        /// </summary>
        private void CancelDragIfActive()
        {
            if (isDragging)
            {
                CancelDragWithReturn();
            }
        }

        /// <summary>
        /// Unity生命周期 - 每帧更新
        /// 处理多种输入方式：触摸、鼠标、虚拟鼠标（手柄）
        /// </summary>
        private void Update()
        {
            // 只在游戏进行中或教学模式下处理输入
            if (EventManager.GameStatus != EGameState.Playing && EventManager.GameStatus != EGameState.Tutorial)
            {
                return;
            }

            // 处理触摸输入（移动设备）
            if (Touchscreen.current != null)
            {
                // Handle existing active touch
                if (isDragging && activeTouchId != -1)
                {
                    bool foundActiveTouch = false;
                    for (int i = 0; i < Touchscreen.current.touches.Count; i++)
                    {
                        var touch = Touchscreen.current.touches[i];
                        if (touch.touchId.ReadValue() == activeTouchId)
                        {
                            HandleDrag(touch.position.ReadValue());
                            foundActiveTouch = true;
                    
                            // Check if touch has ended
                            if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended || 
                                touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Canceled)
                            {
                                EndDrag();
                            }
                            break;
                        }
                    }
            
                    if (!foundActiveTouch)
                    {
                        EndDrag();
                    }
                }
                // Check for new touches if not already dragging
                else if (!isDragging)
                {
                    for (int i = 0; i < Touchscreen.current.touches.Count; i++)
                    {
                        var touch = Touchscreen.current.touches[i];
                        if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
                        {
                            if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, touch.position.ReadValue(), eventCamera))
                            {
                                activeTouchId = touch.touchId.ReadValue();
                                BeginDrag(touch.position.ReadValue());
                                break;
                            }
                        }
                    }
                }
            }
            
            // Track virtual mouse state if available - works on ALL platforms
            bool virtualMouseHandled = false;
            bool isVirtualMousePressed = false;
            Vector2 virtualMousePosition = Vector2.zero;
            
            if (virtualMouseInput != null && virtualMouseInput.virtualMouse != null)
            {
                isVirtualMousePressed = virtualMouseInput.virtualMouse.leftButton.isPressed;
                virtualMousePosition = virtualMouseInput.virtualMouse.position.value;
                
                // Handle virtual mouse input
                if (activeTouchId == -1)
                {
                    // Virtual mouse button down this frame
                    if (isVirtualMousePressed && !wasVirtualMousePressed && !isDragging)
                    {
                        if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, virtualMousePosition, eventCamera))
                        {
                            BeginDrag(virtualMousePosition);
                            virtualMouseHandled = true;
                        }
                    }
                    // Continue dragging with virtual mouse
                    else if (isVirtualMousePressed && isDragging)
                    {
                        HandleDrag(virtualMousePosition);
                        virtualMouseHandled = true;
                    }
                    // Release with virtual mouse
                    else if (!isVirtualMousePressed && wasVirtualMousePressed && isDragging)
                    {
                        EndDrag();
                        virtualMouseHandled = true;
                    }
                }
                
                wasVirtualMousePressed = isVirtualMousePressed;
            }
            
            // Handle regular mouse input using the new Input System if not already handled
            if (!virtualMouseHandled && activeTouchId == -1)
            {
                if (Mouse.current != null)
                {
                    if (Mouse.current.leftButton.wasPressedThisFrame && !isDragging)
                    {
                        if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Mouse.current.position.ReadValue(), eventCamera))
                        {
                            BeginDrag(Mouse.current.position.ReadValue());
                        }
                    }
                    else if (Mouse.current.leftButton.isPressed && isDragging)
                    {
                        HandleDrag(Mouse.current.position.ReadValue());
                    }
                    else if (Mouse.current.leftButton.wasReleasedThisFrame && isDragging)
                    {
                        EndDrag();
                    }
                }
            }

            // Additional safety check to ensure EndDrag is called if dragging unexpectedly stops
            if (isDragging && activeTouchId == -1 && 
                (Mouse.current == null || !Mouse.current.leftButton.isPressed) &&
                !(virtualMouseInput != null && virtualMouseInput.virtualMouse != null && 
                  virtualMouseInput.virtualMouse.leftButton.isPressed))
            {
                EndDrag();
            }
        }

        /// <summary>
        /// 更新Item列表
        /// 当形状更新时刷新Item缓存
        /// </summary>
        private void UpdateItems()
        {
            _items = shape.GetActiveItems();
        }

        /// <summary>
        /// 开始拖拽
        /// 记录初始状态并调整显示层级
        /// </summary>
        /// <param name="position">拖拽起始位置</param>
        private void BeginDrag(Vector2 position)
        {
            isDragging = true;
            originalPosition = rectTransform.anchoredPosition;
            originalScale = transform.localScale;

            // 将形状置于最上层
            transform.SetAsLastSibling();
            // 恢复到正常大小
            transform.localScale = Vector3.one;

            // 计算触摸偏移
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform, position, eventCamera, out touchOffset);
        }

        /// <summary>
        /// 取消拖拽并返回原位
        /// 用于游戏状态变化时强制取消
        /// </summary>
        private void CancelDragWithReturn()
        {
            rectTransform.anchoredPosition = originalPosition;
            transform.localScale = originalScale;
            highlightManager.ClearAllHighlights();
            highlightManager.OnDragEndedWithoutPlacement();
            isDragging = false;
        }

        /// <summary>
        /// 处理拖拽过程
        /// 更新形状位置和高亮预览
        /// </summary>
        /// <param name="position">当前触摸/鼠标位置</param>
        private void HandleDrag(Vector2 position)
        {
            if (!isDragging)
            {
                return;
            }

            // 计算缩放因子（让形状大小与棋盘格子匹配）
            var cellSize = field.GetCellSize();
            var shapeOriginalWidth = 126f;
            var scaleFactor = cellSize / shapeOriginalWidth;

            transform.localScale = new Vector3(scaleFactor, scaleFactor, 1);

            // 更新形状位置
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectTransform.parent as RectTransform, position, eventCamera, out var localPoint))
            {
                var canvasWidth = canvas.GetComponent<RectTransform>().rect.width;
                var normalizedX = localPoint.x / canvasWidth;
                var scaleFactorY = rectTransform.rect.height / canvas.GetComponent<RectTransform>().rect.height * 2.5f;

                // 设置位置（加上垂直偏移让形状显示在手指上方）
                rectTransform.anchoredPosition = new Vector2(
                    normalizedX * canvasWidth,
                    localPoint.y / scaleFactorY + verticalOffset + scaleFactorY
                );
            }

            // 检查是否有格子被占用或正在销毁
            if (AnyBusyCellsOrNoneCells())
            {
                // 如果形状离高亮格子太远，清除高亮
                if (IsDistancesToHighlightedCellsTooHigh())
                {
                    highlightManager.ClearAllHighlights();
                }

                return;
            }

            // 更新格子高亮
            UpdateCellHighlights();
        }

        /// <summary>
        /// 结束拖拽
        /// 验证放置并执行放置逻辑或返回原位
        /// </summary>
        private void EndDrag()
        {
            if (!isDragging)
            {
                return;
            }

            isDragging = false;
            activeTouchId = -1;

            // 如果没有高亮格子，返回原位
            if (highlightManager.GetHighlightedCells().Count == 0)
            {
                rectTransform.anchoredPosition = originalPosition;
                transform.localScale = originalScale;
                highlightManager.ClearAllHighlights();
                highlightManager.OnDragEndedWithoutPlacement();
                return;
            }

            // 成功放置：触觉反馈和音效
            HapticFeedback.TriggerHapticFeedback(HapticFeedback.HapticForce.Light);
            SoundBase.Instance.PlaySound(SoundBase.Instance.placeShape);

            // 填充所有高亮的格子
            foreach (var kvp in highlightManager.GetHighlightedCells())
            {
                kvp.Key.FillCell(kvp.Value.itemTemplate);
                kvp.Key.AnimateFill();
                // 如果有奖励道具，设置奖励
                if (kvp.Value.bonusItemTemplate != null)
                {
                    kvp.Key.SetBonus(kvp.Value.bonusItemTemplate);
                }
            }

            // 发布形状放置事件
            EventManager.GetEvent<Shape>(EGameEvent.ShapePlaced).Invoke(shape);

            // 注意：难度调整已改为关卡级，不再在方块放置时触发
        }

        /// <summary>
        /// 检查到高亮格子的距离是否过大
        /// 用于判断是否应该清除高亮
        /// </summary>
        /// <returns>距离是否过大</returns>
        private bool IsDistancesToHighlightedCellsTooHigh()
        {
            var firstOrDefault = highlightManager.GetHighlightedCells().FirstOrDefault();
            return firstOrDefault.Key != null &&
                   Vector3.Distance(_items[0].transform.position, firstOrDefault.Key.transform.position) > 1f;
        }

        /// <summary>
        /// 检查是否有格子被占用或无效
        /// </summary>
        /// <returns>是否有不可放置的格子</returns>
        private bool AnyBusyCellsOrNoneCells()
        {
            return _items.Any(item =>
            {
                var cell = GetCellUnderShape(item);
                var cellComponent = cell?.GetComponent<Cell>();
                // 格子不存在、不为空或正在销毁都不可放置
                return cell == null || !cellComponent.IsEmpty() || cellComponent.IsDestroying();
            });
        }

        /// <summary>
        /// 更新格子高亮显示
        /// 显示形状预览和可能的消除行列
        /// </summary>
        private void UpdateCellHighlights()
        {
            highlightManager.ClearAllHighlights();

            // 高亮每个Item对应的格子
            foreach (var item in _items)
            {
                var cell = GetCellUnderShape(item);
                if (cell != null)
                {
                    highlightManager.HighlightCell(cell, item);
                }
            }

            // 高亮即将被消除的行列
            if (itemFactory._oneColorMode)
            {
                // 单色模式：使用统一颜色
                highlightManager.HighlightFill(field.GetFilledLines(true), itemFactory.GetColor());
            }
            else
            {
                // 多色模式：使用形状的颜色
                highlightManager.HighlightFill(field.GetFilledLines(true), _items[0].itemTemplate);
            }
        }

        /// <summary>
        /// 获取Item下方的格子
        /// 使用2D射线检测
        /// </summary>
        /// <param name="item">要检测的Item</param>
        /// <returns>检测到的格子Transform，如果没有则返回null</returns>
        private Transform GetCellUnderShape(Item item)
        {
            var hit = Physics2D.Raycast(item.transform.position, Vector2.zero, 1);
            return hit.collider != null && hit.collider.CompareTag("Cell") ? hit.collider.transform : null;
        }
    }
}