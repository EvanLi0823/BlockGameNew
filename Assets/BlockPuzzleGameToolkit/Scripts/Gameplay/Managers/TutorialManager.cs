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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.Gameplay.FX;
using BlockPuzzleGameToolkit.Scripts.LevelsData;
using BlockPuzzleGameToolkit.Scripts.Settings;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using BlockPuzzleGameToolkit.Scripts.Utils;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Gameplay.Managers
{
    public class TutorialManager : MonoBehaviour
    {
        private const float offsethand = .5f;

        [SerializeField]
        private TutorialSettings tutorialSettings;

        [SerializeField]
        private CellDeckManager cellDeckManager;

        [SerializeField]
        private ItemFactory itemFactory;

        [SerializeField]
        private LevelManager levelManager;

        [SerializeField]
        private Transform handSprite;

        private ShapeTemplate[] tutorialShapesQueue;
        private int currentPhase;

        public Outline outline;

        private Vector3 deckPosition;
        private Vector3 centerPosition;

        public bool IsTutorialActive { get; private set; }

        private Coroutine handAnimationCoroutine;
        private bool subscribed;

        private void OnEnable()
        {
            if (GameManager.Instance.IsTutorialMode())
            {
                subscribed = true;
                EventManager.GetEvent<Shape>(EGameEvent.ShapePlaced).Subscribe(OnShapePlaced);
                EventManager.GetEvent<Shape>(EGameEvent.LineDestroyed).Subscribe(OnLineDestroyed);
            }
        }

        private void OnDisable()
        {
            if (!subscribed)
            {
                return;
            }

            EventManager.GetEvent<Shape>(EGameEvent.ShapePlaced).Unsubscribe(OnShapePlaced);
            EventManager.GetEvent<Shape>(EGameEvent.LineDestroyed).Unsubscribe(OnLineDestroyed);
        }

        public void StartTutorial()
        {
            FillCellDecks();
            StartCoroutine(DelayedBoundsCalculation());
        }

        private void FillCellDecks()
        {
            if (cellDeckManager == null || tutorialSettings == null)
            {
                return;
            }

            tutorialShapesQueue = tutorialSettings.tutorialShapes
                .Skip(currentPhase * 3).Take(3).ToArray();
            cellDeckManager.ClearCellDecks();
            cellDeckManager.FillCellDecksWithShapes(tutorialShapesQueue);
        }

        public void EndTutorial()
        {
            IsTutorialActive = false;
            GameManager.Instance.SetTutorialCompleted();
            StopHandAnimation();

            if (outline != null)
            {
                outline.gameObject.SetActive(false);
            }

            GameManager.Instance.SetTutorialMode(false);

            // Trigger tutorial completed event before starting first level
            EventManager.GetEvent(EGameEvent.TutorialCompleted).Invoke();

            // 设置为第一关（关卡号为1）
            GameDataManager.SetLevelNum(1);

            // 重新加载第一关（正常模式）
            GameManager.Instance.RestartLevel();
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 测试 Outline 显示
        /// 用于调试 outline 组件是否正常工作
        /// </summary>
        [ContextMenu("Test Outline Display")]
        public void TestOutlineDisplay()
        {
            if (outline == null)
            {
                Debug.LogError("Outline is not assigned!");
                return;
            }

            outline.gameObject.SetActive(true);

            // 在屏幕中心显示一个测试轮廓
            Color testColor = Color.yellow;
            Vector2 testCenter = Vector2.zero;
            Vector2 testSize = new Vector2(200, 200);

            outline.Play(testCenter, testSize, testColor);
            Debug.Log($"Test Outline displayed at center: {testCenter}, size: {testSize}");
        }

        private void OnShapePlaced(Shape obj)
        {
            StopHandAnimation();
            cellDeckManager.AddShapeToFreeCell(tutorialShapesQueue[0]);
            StopHandAnimation();
        }

        private void OnLineDestroyed(Shape obj)
        {
            currentPhase++;
            StartCoroutine(DelayedNextPhase());
        }

        private IEnumerator DelayedNextPhase()
        {
            yield return new WaitForSeconds(0.5f);
            if (currentPhase <= tutorialSettings.tutorialLevels.Length - 1)
            {
                CheckPhase(currentPhase);
            }
            else
            {
                EndTutorial();
            }
        }

        public void CheckPhase(int phase)
        {
            if (phase > 0)
            {
                GameManager.Instance.RestartLevel();
            }
        }

        private IEnumerator DelayedBoundsCalculation()
        {
            yield return new WaitForSeconds(0.1f);

            // 获取牌组位置
            deckPosition = cellDeckManager.cellDecks[1].shape.GetActiveItems()[0].transform.position + Vector3.right * offsethand;

            var fieldManager = FindObjectOfType<FieldManager>();
            if (fieldManager == null)
            {
                Debug.LogError("TutorialManager: FieldManager not found!");
                yield break;
            }

            // 获取中心格子位置
            var centerCell = fieldManager.GetCenterCell();
            if (centerCell == null || centerCell.item == null)
            {
                Debug.LogError("TutorialManager: Center cell or item is null!");
                yield break;
            }
            centerPosition = centerCell.item.transform.position + Vector3.right * offsethand + Vector3.down * offsethand;

            // 获取教程高亮格子
            var tutorialCells = fieldManager.GetTutorialLine();

            if (tutorialCells == null || tutorialCells.Count == 0)
            {
                Debug.LogWarning("TutorialManager: No highlighted cells found! Using default tutorial line.");
                tutorialCells = GetDefaultTutorialLine(fieldManager);
            }
            else if (tutorialCells.Count > 10)
            {
                // 如果高亮格子太多，说明配置有问题，使用默认配置
                Debug.LogWarning($"TutorialManager: Too many highlighted cells ({tutorialCells.Count})! This might be a configuration error. Using default tutorial line instead.");

                // 先清除所有错误的高亮
                ClearAllHighlights(fieldManager);

                // 使用默认配置
                tutorialCells = GetDefaultTutorialLine(fieldManager);
            }

            Debug.Log($"TutorialManager: Using {tutorialCells.Count} cells for tutorial outline");

            // 打印每个高亮格子的位置，帮助调试
            if (tutorialCells.Count <= 10)
            {
                for (int i = 0; i < tutorialCells.Count; i++)
                {
                    var cell = tutorialCells[i];
                    if (cell != null)
                    {
                        Debug.Log($"  Cell {i}: Position = {cell.transform.position}, Name = {cell.name}");
                    }
                }
            }

            // 启动手势动画
            StartHandAnimation();

            // 显示轮廓
            if (outline != null)
            {
                outline.gameObject.SetActive(true);

                // 计算高亮区域的边界
                var canvas = transform.parent.GetComponent<Canvas>();
                if (canvas == null)
                {
                    canvas = FindObjectOfType<Canvas>();
                }

                if (canvas != null && tutorialCells.Count > 0)
                {
                    var value = RectTransformUtils.GetMinMaxAndSizeForCanvas(tutorialCells, canvas);

                    // 添加一些内边距
                    value.size += new Vector2(50, 50);

                    Debug.Log($"TutorialManager: Outline bounds - Center: {value.center}, Size: {value.size}");

                    Color hexColor;
                    if (ColorUtility.TryParseHtmlString("#609FFF", out hexColor))
                    {
                        outline.Play(value.center, value.size, hexColor);
                        Debug.Log($"TutorialManager: Outline activated at position {value.center} with size {value.size}");
                    }
                }
                else
                {
                    Debug.LogError("TutorialManager: Canvas not found or no tutorial cells!");
                }
            }
            else
            {
                Debug.LogError("TutorialManager: Outline component is not assigned!");
            }
        }

        /// <summary>
        /// 清除所有格子的高亮状态
        /// </summary>
        private void ClearAllHighlights(FieldManager fieldManager)
        {
            for (int i = 0; i < fieldManager.cells.GetLength(0); i++)
            {
                for (int j = 0; j < fieldManager.cells.GetLength(1); j++)
                {
                    if (fieldManager.cells[i, j] != null)
                    {
                        // 这里需要一个方法来清除高亮，如果Cell类有这样的方法的话
                        // fieldManager.cells[i, j].ClearHighlight();
                    }
                }
            }
        }

        /// <summary>
        /// 获取默认的教程高亮行/列
        /// 当关卡没有设置高亮时使用
        /// </summary>
        private List<Cell> GetDefaultTutorialLine(FieldManager fieldManager)
        {
            var cells = new List<Cell>();

            // 根据当前阶段决定是高亮行还是列
            if (currentPhase == 0)
            {
                // 第一阶段：高亮第5列（索引4）
                int columnIndex = 4;
                for (int i = 0; i < 8; i++)
                {
                    if (i < fieldManager.cells.GetLength(0) && columnIndex < fieldManager.cells.GetLength(1))
                    {
                        var cell = fieldManager.cells[i, columnIndex];
                        if (cell != null)
                        {
                            cells.Add(cell);
                        }
                    }
                }
                Debug.Log($"TutorialManager: Using default column {columnIndex + 1} (index {columnIndex}) for phase 0");
            }
            else
            {
                // 第二阶段：高亮中间行（第4行，索引3）
                int rowIndex = 3;
                for (int j = 0; j < 8; j++)
                {
                    if (rowIndex < fieldManager.cells.GetLength(0) && j < fieldManager.cells.GetLength(1))
                    {
                        var cell = fieldManager.cells[rowIndex, j];
                        if (cell != null)
                        {
                            cells.Add(cell);
                        }
                    }
                }
                Debug.Log($"TutorialManager: Using default row {rowIndex + 1} (index {rowIndex}) for phase 1");
            }

            return cells;
        }

        public Level GetLevelForPhase()
        {
            return tutorialSettings.tutorialLevels[currentPhase];
        }

        private void StartHandAnimation()
        {
            handSprite.gameObject.SetActive(true);
            if (handAnimationCoroutine != null)
            {
                StopCoroutine(handAnimationCoroutine);
            }

            handAnimationCoroutine = StartCoroutine(HandAnimationCoroutine());
        }

        private IEnumerator HandAnimationCoroutine()
        {
            while (true)
            {
                handSprite.position = deckPosition;
                var elapsedTime = 0f;
                var duration = 1f;

                while (elapsedTime < duration)
                {
                    handSprite.position = Vector3.Lerp(deckPosition, centerPosition, elapsedTime / duration);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }

                handSprite.position = centerPosition;
                yield return new WaitForSeconds(0.5f); // Pause before restarting the animation
            }
        }

        private void StopHandAnimation()
        {
            if (handAnimationCoroutine != null)
            {
                StopCoroutine(handAnimationCoroutine);
                handAnimationCoroutine = null;
            }

            handSprite.gameObject.SetActive(false);
        }
    }
}