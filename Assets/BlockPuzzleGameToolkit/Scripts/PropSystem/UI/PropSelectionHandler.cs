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
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using BlockPuzzleGameToolkit.Scripts.PropSystem.Core;
using BlockPuzzleGameToolkit.Scripts.Gameplay;
using BlockPuzzleGameToolkit.Scripts.Audio;
using System.Collections.Generic;

namespace BlockPuzzleGameToolkit.Scripts.PropSystem.UI
{
    /// <summary>
    /// 道具选择输入处理器 - 处理道具选择时的输入交互
    /// </summary>
    public class PropSelectionHandler : MonoBehaviour
    {
        #region 配置

        [Header("检测配置")]
        [Tooltip("射线检测的层级")]
        [SerializeField] private LayerMask raycastLayers = -1;

        [Tooltip("是否启用鼠标悬停预览")]
        [SerializeField] private bool enableHoverPreview = true;

        [Tooltip("悬停预览延迟（秒）")]
        [Range(0f, 1f)]
        [SerializeField] private float hoverPreviewDelay = 0.1f;

        [Header("输入配置")]
        [Tooltip("取消选择的按键")]
        [SerializeField] private Key cancelKey = Key.Escape;

        [Tooltip("是否支持右键取消")]
        [SerializeField] private bool rightClickCancel = true;

        [Header("反馈配置")]
        [Tooltip("选择时显示的光标")]
        [SerializeField] private Texture2D selectionCursor;

        [Tooltip("选择提示文本")]
        [SerializeField] private GameObject selectionHintText;

        #endregion

        #region 私有字段

        /// <summary>
        /// 道具管理器
        /// </summary>
        private PropManager propManager;

        /// <summary>
        /// 当前悬停的目标
        /// </summary>
        private GameObject currentHoverTarget;

        /// <summary>
        /// 上一个悬停的目标
        /// </summary>
        private GameObject lastHoverTarget;

        /// <summary>
        /// 悬停计时器
        /// </summary>
        private float hoverTimer;

        /// <summary>
        /// 是否正在选择
        /// </summary>
        private bool isSelecting;

        /// <summary>
        /// EventSystem引用
        /// </summary>
        private EventSystem eventSystem;

        /// <summary>
        /// 输入系统
        /// </summary>
        private PlayerInput playerInput;

        /// <summary>
        /// 主相机
        /// </summary>
        private Camera mainCamera;

        /// <summary>
        /// 原始光标
        /// </summary>
        private Texture2D originalCursor;

        /// <summary>
        /// 原始光标热点
        /// </summary>
        private Vector2 originalCursorHotspot;

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            propManager = PropManager.Instance;
            eventSystem = EventSystem.current;
            mainCamera = Camera.main;

            // 获取或创建PlayerInput
            playerInput = GetComponent<PlayerInput>();
            if (playerInput == null)
            {
                playerInput = gameObject.AddComponent<PlayerInput>();
            }

            // 保存原始光标（Unity没有GetCurrentTextureCursor方法，先设为null）
            originalCursor = null;
        }

        private void OnEnable()
        {
            // 订阅道具选择事件
            if (propManager != null)
            {
                PropManager.OnPropSelectionStart += OnPropSelectionStart;
                PropManager.OnPropSelectionEnd += OnPropSelectionEnd;
            }
        }

        private void OnDisable()
        {
            // 取消订阅
            if (propManager != null)
            {
                PropManager.OnPropSelectionStart -= OnPropSelectionStart;
                PropManager.OnPropSelectionEnd -= OnPropSelectionEnd;
            }

            // 恢复光标
            RestoreCursor();
        }

        private void Update()
        {
            if (!isSelecting) return;

            // 处理取消输入
            HandleCancelInput();

            // 处理选择输入
            HandleSelectionInput();

            // 处理悬停预览
            if (enableHoverPreview)
            {
                HandleHoverPreview();
            }
        }

        #endregion

        #region 输入处理

        /// <summary>
        /// 处理取消输入
        /// </summary>
        private void HandleCancelInput()
        {
            // ESC键取消
            if (Keyboard.current != null && Keyboard.current[cancelKey].wasPressedThisFrame)
            {
                CancelSelection();
                return;
            }

            // 右键取消
            if (rightClickCancel && Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
            {
                CancelSelection();
                return;
            }
        }

        /// <summary>
        /// 处理选择输入
        /// </summary>
        private void HandleSelectionInput()
        {
            // 检查是否点击了UI
            if (IsPointerOverUI()) return;

            bool clicked = false;
            Vector2 pointerPosition = Vector2.zero;

            // 鼠标输入
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                clicked = true;
                pointerPosition = Mouse.current.position.ReadValue();
            }

            // 触摸输入
            if (!clicked && Touchscreen.current != null)
            {
                var touch = Touchscreen.current.primaryTouch;
                if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    clicked = true;
                    pointerPosition = touch.position.ReadValue();
                }
            }

            if (clicked)
            {
                ProcessSelection(pointerPosition);
            }
        }

        /// <summary>
        /// 处理悬停预览
        /// </summary>
        private void HandleHoverPreview()
        {
            GameObject hoverTarget = null;
            Vector2 pointerPosition = Vector2.zero;

            // 获取当前指针位置
            if (Mouse.current != null)
            {
                pointerPosition = Mouse.current.position.ReadValue();
            }
            else if (Touchscreen.current != null)
            {
                var touch = Touchscreen.current.primaryTouch;
                if (touch.phase.ReadValue() != UnityEngine.InputSystem.TouchPhase.None)
                {
                    pointerPosition = touch.position.ReadValue();
                }
            }

            // 射线检测
            if (pointerPosition != Vector2.zero)
            {
                hoverTarget = GetTargetAtPosition(pointerPosition);
            }

            // 更新悬停目标
            UpdateHoverTarget(hoverTarget);
        }

        /// <summary>
        /// 更新悬停目标
        /// </summary>
        private void UpdateHoverTarget(GameObject newTarget)
        {
            if (newTarget != currentHoverTarget)
            {
                // 目标改变，重置计时器
                hoverTimer = 0f;
                lastHoverTarget = currentHoverTarget;
                currentHoverTarget = newTarget;

                // 隐藏之前的预览
                if (propManager != null && lastHoverTarget != null)
                {
                    propManager.HidePropPreview();
                }
            }
            else if (currentHoverTarget != null)
            {
                // 累加悬停时间
                hoverTimer += Time.deltaTime;

                // 达到延迟时间后显示预览
                if (hoverTimer >= hoverPreviewDelay)
                {
                    ShowPreviewForTarget(currentHoverTarget);
                }
            }
        }

        #endregion

        #region 选择处理

        /// <summary>
        /// 处理选择
        /// </summary>
        private void ProcessSelection(Vector2 screenPosition)
        {
            var target = GetTargetAtPosition(screenPosition);
            if (target == null) return;

            object selectionTarget = GetSelectionTarget(target);
            if (selectionTarget == null) return;

            // 尝试确认使用道具
            if (propManager != null)
            {
                propManager.ConfirmPropUse(selectionTarget);

                // 播放选择音效
                SoundBase.Instance?.PlaySound(SoundBase.Instance.click);
            }
        }

        /// <summary>
        /// 获取指定位置的目标
        /// </summary>
        private GameObject GetTargetAtPosition(Vector2 screenPosition)
        {
            if (mainCamera == null) return null;

            // 3D射线检测
            Ray ray = mainCamera.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit3D, Mathf.Infinity, raycastLayers))
            {
                return hit3D.collider.gameObject;
            }

            // 2D射线检测
            RaycastHit2D hit2D = Physics2D.GetRayIntersection(ray, Mathf.Infinity, raycastLayers);
            if (hit2D.collider != null)
            {
                return hit2D.collider.gameObject;
            }

            // UI射线检测
            PointerEventData pointerData = new PointerEventData(eventSystem)
            {
                position = screenPosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            eventSystem.RaycastAll(pointerData, results);

            if (results.Count > 0)
            {
                return results[0].gameObject;
            }

            return null;
        }

        /// <summary>
        /// 获取选择目标
        /// </summary>
        private object GetSelectionTarget(GameObject target)
        {
            if (target == null) return null;

            // 根据当前选择的道具类型判断目标
            if (propManager == null) return null;

            var currentProp = propManager.CurrentSelectingProp;

            switch (currentProp)
            {
                case PropType.Rotate:
                    // 旋转道具：查找Shape或CellDeck
                    var shape = target.GetComponentInParent<Shape>();
                    if (shape != null) return shape;

                    var cellDeck = target.GetComponentInParent<CellDeck>();
                    if (cellDeck != null && !cellDeck.IsEmpty) return cellDeck;
                    break;

                case PropType.Bomb:
                    // 炸弹道具：查找Cell
                    var cell = target.GetComponent<Cell>();
                    if (cell != null && !cell.IsEmpty()) return cell;
                    break;

                case PropType.Refresh:
                    // 刷新道具不需要选择
                    break;
            }

            return null;
        }

        /// <summary>
        /// 显示目标预览
        /// </summary>
        private void ShowPreviewForTarget(GameObject target)
        {
            if (propManager == null || target == null) return;

            var selectionTarget = GetSelectionTarget(target);
            if (selectionTarget != null)
            {
                propManager.PreviewPropEffect(selectionTarget);
            }
        }

        /// <summary>
        /// 取消选择
        /// </summary>
        private void CancelSelection()
        {
            if (propManager != null)
            {
                propManager.CancelPropSelection();

                // 播放取消音效（使用click音效代替）
                SoundBase.Instance?.PlaySound(SoundBase.Instance.click);
            }
        }

        #endregion

        #region 事件处理

        /// <summary>
        /// 道具选择开始
        /// </summary>
        private void OnPropSelectionStart(PropType propType)
        {
            isSelecting = true;

            // 更改光标
            if (selectionCursor != null)
            {
                Cursor.SetCursor(selectionCursor, new Vector2(selectionCursor.width / 2, selectionCursor.height / 2), CursorMode.Auto);
            }

            // 显示提示
            if (selectionHintText != null)
            {
                selectionHintText.SetActive(true);
            }

            // 禁用其他交互（如拖拽）
            DisableOtherInteractions();
        }

        /// <summary>
        /// 道具选择结束
        /// </summary>
        private void OnPropSelectionEnd(PropType propType)
        {
            isSelecting = false;

            // 恢复光标
            RestoreCursor();

            // 隐藏提示
            if (selectionHintText != null)
            {
                selectionHintText.SetActive(false);
            }

            // 清理悬停状态
            currentHoverTarget = null;
            lastHoverTarget = null;
            hoverTimer = 0f;

            // 恢复其他交互
            EnableOtherInteractions();
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 检查是否在UI上
        /// </summary>
        private bool IsPointerOverUI()
        {
            if (eventSystem == null) return false;

            // 检查鼠标
            if (Mouse.current != null)
            {
                var pointerData = new PointerEventData(eventSystem)
                {
                    position = Mouse.current.position.ReadValue()
                };

                List<RaycastResult> results = new List<RaycastResult>();
                eventSystem.RaycastAll(pointerData, results);

                // 过滤掉道具相关的UI
                foreach (var result in results)
                {
                    // 如果是非道具UI，返回true
                    if (result.gameObject.GetComponent<PropItem>() == null &&
                        result.gameObject.GetComponentInParent<PropItem>() == null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 恢复光标
        /// </summary>
        private void RestoreCursor()
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        /// <summary>
        /// 禁用其他交互
        /// </summary>
        private void DisableOtherInteractions()
        {
            // 禁用形状拖拽
            var draggables = FindObjectsOfType<ShapeDraggable>();
            foreach (var draggable in draggables)
            {
                draggable.enabled = false;
            }
        }

        /// <summary>
        /// 恢复其他交互
        /// </summary>
        private void EnableOtherInteractions()
        {
            // 恢复形状拖拽
            var draggables = FindObjectsOfType<ShapeDraggable>();
            foreach (var draggable in draggables)
            {
                draggable.enabled = true;
            }
        }

        #endregion
    }
}