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
using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using BlockPuzzleGameToolkit.Scripts.LevelsData;

namespace BlockPuzzleGameToolkit.Scripts.PropSystem.Behaviors
{
    /// <summary>
    /// 炸弹道具行为 - 清除选中位置3x3范围的格子
    /// </summary>
    public class BombPropBehavior : PropBehaviorBase
    {
        /// <summary>
        /// 道具类型
        /// </summary>
        public override PropType PropType => PropType.Bomb;

        /// <summary>
        /// 需要选择目标格子
        /// </summary>
        public override bool RequiresTarget => true;

        /// <summary>
        /// 爆炸半径（1表示3x3，2表示5x5）
        /// </summary>
        private const int BOMB_RADIUS = 1;

        /// <summary>
        /// 可选择的格子列表
        /// </summary>
        private List<Cell> selectableCells;

        /// <summary>
        /// 当前预览的爆炸范围
        /// </summary>
        private List<Cell> previewCells;

        /// <summary>
        /// 高亮模板（用于爆炸范围预览）
        /// </summary>
        private ItemTemplate bombHighlightTemplate;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="manager">道具管理器引用</param>
        public override void Initialize(PropManager manager)
        {
            base.Initialize(manager);

            selectableCells = new List<Cell>();
            previewCells = new List<Cell>();

            if (fieldManager == null)
            {
                Debug.LogError("BombPropBehavior: FieldManager未找到");
            }

            // 加载高亮模板
            bombHighlightTemplate = Resources.Load<ItemTemplate>("Items/ItemTemplate 0");
        }

        /// <summary>
        /// 开始选择时高亮可选择的格子
        /// </summary>
        protected override void OnStartSelection()
        {
            if (fieldManager == null) return;

            selectableCells.Clear();

            // 遍历所有格子，高亮填充的格子
            var cells = fieldManager.cells;
            if (cells == null) return;

            for (int row = 0; row < cells.GetLength(0); row++)
            {
                for (int col = 0; col < cells.GetLength(1); col++)
                {
                    var cell = cells[row, col];
                    if (cell != null && !cell.IsEmpty())
                    {
                        // 这个格子可以作为炸弹目标
                        selectableCells.Add(cell);

                        // 高亮显示
                        if (bombHighlightTemplate != null)
                        {
                            cell.HighlightCell(bombHighlightTemplate);
                        }

                        // 添加脉冲动画
                        AddCellPulseAnimation(cell);
                    }
                }
            }

            if (selectableCells.Count == 0)
            {
                Debug.Log("BombPropBehavior: 没有可以轰炸的格子");
            }
        }

        /// <summary>
        /// 取消选择时清除高亮
        /// </summary>
        protected override void OnCancelSelection()
        {
            // 清除所有高亮
            foreach (var cell in selectableCells)
            {
                if (cell != null)
                {
                    cell.ClearCell();
                    cell.transform.DOKill();
                    cell.transform.localScale = Vector3.one;
                }
            }

            selectableCells.Clear();
            HidePreview();
        }

        /// <summary>
        /// 检查目标是否有效
        /// </summary>
        public override bool CanExecute(object target)
        {
            if (target is Cell cell)
            {
                // 检查格子是否在可选择列表中
                return selectableCells.Contains(cell);
            }

            return false;
        }

        /// <summary>
        /// 执行爆炸
        /// </summary>
        public override void Execute(object target = null)
        {
            if (!(target is Cell centerCell) || !CanExecute(target))
            {
                Debug.LogWarning("BombPropBehavior: 无效的爆炸目标");
                return;
            }

            // 先取消选择模式
            CancelSelection();

            // 获取爆炸范围内的所有格子
            var bombCells = GetCellsInRadius(centerCell, BOMB_RADIUS);

            // 执行爆炸
            ExecuteBombExplosion(bombCells, centerCell);

            // 播放音效
            PlayUseSound();
        }

        /// <summary>
        /// 显示预览
        /// </summary>
        protected override void OnShowPreview(object target)
        {
            if (!(target is Cell centerCell)) return;

            // 隐藏之前的预览
            HidePreview();

            // 获取爆炸范围
            previewCells = GetCellsInRadius(centerCell, BOMB_RADIUS);

            // 使用HighlightManager高亮爆炸范围
            if (highlightManager != null && bombHighlightTemplate != null)
            {
                highlightManager.HighlightFill(new List<List<Cell>> { previewCells }, bombHighlightTemplate);
            }

            // 为中心格子添加特殊标记
            centerCell.transform.localScale = Vector3.one * 1.2f;
        }

        /// <summary>
        /// 隐藏预览
        /// </summary>
        protected override void OnHidePreview()
        {
            // 清除预览高亮
            if (highlightManager != null)
            {
                highlightManager.ClearAllHighlights();
            }

            // 恢复中心格子缩放
            if (currentPreviewTarget is Cell cell)
            {
                cell.transform.localScale = Vector3.one;
            }

            // 重新显示可选择格子的高亮
            if (IsSelecting)
            {
                OnStartSelection();
            }

            previewCells.Clear();
        }

        /// <summary>
        /// 获取指定半径内的所有格子
        /// </summary>
        private List<Cell> GetCellsInRadius(Cell centerCell, int radius)
        {
            var result = new List<Cell>();

            if (fieldManager == null || centerCell == null) return result;

            // 获取中心格子的位置
            var (centerRow, centerCol) = GetCellPosition(centerCell);

            if (centerRow == -1 || centerCol == -1) return result;

            var cells = fieldManager.cells;

            // 遍历半径范围内的格子
            for (int row = centerRow - radius; row <= centerRow + radius; row++)
            {
                for (int col = centerCol - radius; col <= centerCol + radius; col++)
                {
                    // 检查边界
                    if (row >= 0 && row < cells.GetLength(0) &&
                        col >= 0 && col < cells.GetLength(1))
                    {
                        var cell = cells[row, col];
                        if (cell != null)
                        {
                            result.Add(cell);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 获取格子在数组中的位置
        /// </summary>
        private (int row, int col) GetCellPosition(Cell targetCell)
        {
            if (fieldManager == null || targetCell == null) return (-1, -1);

            var cells = fieldManager.cells;

            for (int row = 0; row < cells.GetLength(0); row++)
            {
                for (int col = 0; col < cells.GetLength(1); col++)
                {
                    if (cells[row, col] == targetCell)
                    {
                        return (row, col);
                    }
                }
            }

            return (-1, -1);
        }

        /// <summary>
        /// 执行爆炸效果
        /// </summary>
        private void ExecuteBombExplosion(List<Cell> bombCells, Cell centerCell)
        {
            if (bombCells == null || bombCells.Count == 0) return;

            float animDuration = settings != null ? settings.useAnimationDuration : 0.3f;

            // 计算延迟（从中心向外扩散）
            foreach (var cell in bombCells)
            {
                if (cell != null && !cell.IsEmpty())
                {
                    // 计算到中心的距离
                    float distance = Vector3.Distance(cell.transform.position, centerCell.transform.position);
                    float delay = distance * 0.05f; // 根据距离计算延迟

                    // 延迟执行爆炸
                    DOVirtual.DelayedCall(delay, () =>
                    {
                        // 爆炸动画
                        ExplodeCell(cell);
                    });
                }
            }

            // 中心位置播放主爆炸特效
            PlayEffect(centerCell.transform.position);

            // 屏幕震动
            AddScreenShake(0.5f, 0.3f);

            // 延迟后触发连消检测
            DOVirtual.DelayedCall(animDuration + 0.5f, () =>
            {
                // 触发形状放置事件，检测连消
                EventManager.GetEvent(EGameEvent.ShapePlaced).Invoke();
            });
        }

        /// <summary>
        /// 爆炸单个格子
        /// </summary>
        private void ExplodeCell(Cell cell)
        {
            if (cell == null || cell.IsEmpty()) return;

            float animDuration = settings != null ? settings.useAnimationDuration : 0.3f;

            // 缩放爆炸
            var sequence = DOTween.Sequence();
            sequence.Append(cell.transform.DOScale(Vector3.one * 1.3f, animDuration * 0.3f));
            sequence.Append(cell.transform.DOScale(Vector3.zero, animDuration * 0.7f));

            // 旋转
            cell.transform.DORotate(new Vector3(0, 0, 360), animDuration, RotateMode.LocalAxisAdd);

            // 淡出
            var image = cell.GetComponent<UnityEngine.UI.Image>();
            if (image != null)
            {
                image.DOFade(0, animDuration);
            }

            // 播放爆炸特效
            if (config != null && config.effectPrefab != null)
            {
                var effect = Object.Instantiate(config.effectPrefab, cell.transform.position, Quaternion.identity);
                if (settings != null)
                {
                    // 缩放特效
                    effect.transform.localScale = Vector3.one * settings.effectScale * 0.5f;
                    Object.Destroy(effect, settings.effectDuration);
                }
            }

            // 延迟清除格子数据
            DOVirtual.DelayedCall(animDuration, () =>
            {
                cell.ClearCell();
                cell.transform.localScale = Vector3.one;

                var img = cell.GetComponent<UnityEngine.UI.Image>();
                if (img != null)
                {
                    img.color = new Color(img.color.r, img.color.g, img.color.b, 1f);
                }
            });
        }

        /// <summary>
        /// 添加格子脉冲动画
        /// </summary>
        private void AddCellPulseAnimation(Cell cell)
        {
            if (cell == null) return;

            var sequence = DOTween.Sequence();
            sequence.Append(cell.transform.DOScale(Vector3.one * 1.05f, 0.5f));
            sequence.Append(cell.transform.DOScale(Vector3.one, 0.5f));
            sequence.SetLoops(-1);
        }

        /// <summary>
        /// 添加屏幕震动
        /// </summary>
        private void AddScreenShake(float strength = 0.3f, float duration = 0.3f)
        {
            var camera = Camera.main;
            if (camera != null)
            {
                camera.transform.DOShakePosition(duration, strength, 20, 90, false, true);
            }
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        public override void Cleanup()
        {
            CancelSelection();
            selectableCells?.Clear();
            previewCells?.Clear();
            bombHighlightTemplate = null;
            base.Cleanup();
        }
    }
}