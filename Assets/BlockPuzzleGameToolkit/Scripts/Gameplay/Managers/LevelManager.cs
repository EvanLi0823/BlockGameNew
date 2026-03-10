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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BlockPuzzleGameToolkit.Scripts.Audio;
using BlockPuzzleGameToolkit.Scripts.Data;
using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.Gameplay.FX;
using BlockPuzzleGameToolkit.Scripts.Gameplay.Managers;
using BlockPuzzleGameToolkit.Scripts.Gameplay.Pool;
using BlockPuzzleGameToolkit.Scripts.LevelsData;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using BlockPuzzleGameToolkit.Scripts.Utils;
using BlockPuzzleGameToolkit.Scripts.PostPlacementSystem;
using BlockPuzzleGameToolkit.Scripts.PostPlacementSystem.Processors;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;
using Random = UnityEngine.Random;
using UnityEngine.InputSystem;

namespace BlockPuzzleGameToolkit.Scripts.Gameplay
{
    /// <summary>
    /// LevelManager - 游戏核心控制器
    /// 管理游戏的主要流程，包括关卡加载、消除检测、得分系统、连消系统等
    /// 是整个游戏的状态机和事件中心
    /// </summary>
    public partial class LevelManager : MonoBehaviour
    {
        // ========== 关卡配置 ==========
        /// <summary>
        /// 当前关卡编号
        /// </summary>
        public int currentLevel;

        // ========== 特效预制体 ==========
        /// <summary>
        /// 行消除爆炸特效预制体
        /// </summary>
        public LineExplosion lineExplosionPrefab;

        /// <summary>
        /// 连消文字特效预制体
        /// </summary>
        public ComboText comboTextPrefab;

        /// <summary>
        /// 对象池父节点
        /// </summary>
        public Transform pool;

        /// <summary>
        /// 特效对象池父节点
        /// </summary>
        public Transform fxPool;

        // ========== 游戏计数器 ==========
        /// <summary>
        /// 连消计数器 - 记录连续消除的次数
        /// </summary>
        public int comboCounter;

        /// <summary>
        /// 失误计数器 - 记录未消除的放置次数
        /// </summary>
        private int missCounter;

        // ========== UI组件 ==========
        [SerializeField]
        private RectTransform gameCanvas;  // 游戏画布

        [SerializeField]
        private RectTransform shakeCanvas;  // 震动画布（用于屏幕震动效果）

        [SerializeField]
        private GameObject scorePrefab;  // 得分显示预制体

        [SerializeField]
        private GameObject[] words;  // 鼓励文字数组（如"Great!"、"Excellent!"）

        [SerializeField]
        private TutorialManager tutorialManager;  // 教学管理器

        [SerializeField]
        private GameObject timerPanel;  // 计时器面板

        // ========== 游戏数据 ==========
        /// <summary>
        /// 游戏模式（经典/计时/冒险）
        /// </summary>
        public EGameMode gameMode;

        /// <summary>
        /// 当前关卡数据
        /// </summary>
        public Level _levelData;

        /// <summary>
        /// 空格子数组（游戏失败时填充用）
        /// </summary>
        private Cell[] emptyCells;

        // ========== 事件和回调 ==========
        /// <summary>
        /// 关卡加载完成事件
        /// </summary>
        public UnityEvent<Level> OnLevelLoaded;

        /// <summary>
        /// 得分事件
        /// </summary>
        public Action<int> OnScored;

        /// <summary>
        /// 游戏失败事件
        /// </summary>
        public Action OnLose;

        // ========== 管理器引用 ==========
        private FieldManager field;           // 棋盘管理器
        public CellDeckManager cellDeck;      // 形状队列管理器
        private ItemFactory itemFactory;      // 物品工厂
        private TargetManager targetManager;  // 目标管理器

        // ========== 对象池 ==========
        private ObjectPool<ComboText> comboTextPool;           // 连消文字对象池
        private ObjectPool<LineExplosion> lineExplosionPool;   // 爆炸特效对象池
        private ObjectPool<ScoreText> scoreTextPool;           // 得分文字对象池
        // 注意：鼓励文字不使用对象池，因为需要随机显示不同的词

        // ========== 游戏模式处理器 ==========
        private ClassicModeHandler classicModeHandler;  // 经典模式处理器
        private TimedModeHandler timedModeHandler;      // 计时模式处理器
        public TimerManager timerManager;               // 计时器管理器
        private int timerDuration;                      // 计时器时长

        // ========== 缓存优化 ==========
        private Vector3 cachedFieldCenter;     // 缓存的棋盘中心位置
        private bool isFieldCenterCached;      // 是否已缓存中心位置

        // WaitForSeconds 缓存（用于优化协程性能）
        private readonly WaitForSeconds waitForProcessing = new WaitForSeconds(0.1f);
        private readonly WaitForSeconds waitForCheckLose = new WaitForSeconds(0.5f);
        private readonly WaitForSeconds waitForFillEmpty = new WaitForSeconds(0.01f);

        // ========== PostPlacement处理器注册状态 ==========
        /// <summary>
        /// 是否已注册PostPlacement处理器（防止重复注册）
        /// </summary>
        private bool hasRegisteredProcessors = false;

        /// <summary>
        /// Unity生命周期 - 启用时初始化
        /// 订阅事件、初始化管理器引用、创建对象池
        /// </summary>
        private void OnEnable()
        {
            // 设置游戏状态
            StateManager.Instance.CurrentState = EScreenStates.Game;

            // 订阅游戏事件
            EventManager.GetEvent(EGameEvent.RestartLevel).Subscribe(RestartLevel);
            EventManager.GetEvent<Shape>(EGameEvent.ShapePlaced).Subscribe(CheckLines);
            EventManager.OnGameStateChanged += HandleGameStateChange;

            // 获取管理器引用
            targetManager = FindObjectOfType<TargetManager>();
            itemFactory = FindObjectOfType<ItemFactory>();
            cellDeck = FindObjectOfType<CellDeckManager>();
            field = FindObjectOfType<FieldManager>();

            // 获取或添加计时器组件
            timerManager = GetComponent<TimerManager>();
            if (timerManager == null)
            {
                timerManager = gameObject.AddComponent<TimerManager>();
            }

            // 订阅计时器到期事件
            if (timerManager != null && timerPanel != null)
            {
                timerManager.OnTimerExpired += OnTimerExpired;
            }

            // 初始化连消文字对象池
            comboTextPool = new ObjectPool<ComboText>(
                () => Instantiate(comboTextPrefab, fxPool),
                obj => obj.gameObject.SetActive(true),
                obj => obj.gameObject.SetActive(false),
                Destroy
            );

            // 初始化爆炸特效对象池
            lineExplosionPool = new ObjectPool<LineExplosion>(
                () => Instantiate(lineExplosionPrefab, pool),
                obj => obj.gameObject.SetActive(true),
                obj => obj.gameObject.SetActive(false),
                Destroy
            );

            // 初始化得分文字对象池
            scoreTextPool = new ObjectPool<ScoreText>(
                () => Instantiate(scorePrefab, fxPool).GetComponent<ScoreText>(),
                obj => obj.gameObject.SetActive(true),
                obj => obj.gameObject.SetActive(false),
                Destroy
            );

            // 注意：鼓励文字不使用对象池，而是每次随机创建，以确保显示不同的词

            // 注册PostPlacement处理器（只注册一次）
            if (!hasRegisteredProcessors)
            {
                RegisterPostPlacementProcessors();
                hasRegisteredProcessors = true;
            }

            // 重启关卡
            RestartLevel();

            // 根据游戏模式恢复游戏状态
            if (gameMode == EGameMode.Classic)
                RestoreGameState();
            else if (gameMode == EGameMode.Timed)
                RestoreTimedGameState();
        }

        /// <summary>
        /// 注册PostPlacement处理器
        /// </summary>
        private void RegisterPostPlacementProcessors()
        {
            var processorManager = PostPlacementProcessorManager.Instance;
            if (processorManager == null)
            {
                Debug.LogWarning("[LevelManager] PostPlacementProcessorManager未找到，无法注册处理器");
                return;
            }

            // 注册EliminationProcessor（优先级100）
            var eliminationProcessor = new EliminationProcessor(this);
            processorManager.RegisterProcessor(eliminationProcessor);

            // 注册MoneyBlockProcessor（优先级200）
            var moneyBlockProcessor = new MoneyBlockProcessor();
            processorManager.RegisterProcessor(moneyBlockProcessor);

            Debug.Log("[LevelManager] PostPlacement处理器注册完成");
        }

        private void RestoreGameState()
        {
            var state = GameState.Load(EGameMode.Classic) as ClassicGameState;
            if (state != null)
            {
                GameManager.Instance.Score = state.score;

                if (state.levelRows != null)
                {
                    var fieldManager = FindObjectOfType<FieldManager>();
                    if (fieldManager != null)
                    {
                        fieldManager.RestoreFromState(state.levelRows);
                    }
                }
            }
        }

        private void RestoreTimedGameState()
        {
            var state = GameState.Load(EGameMode.Timed) as TimedGameState;
            if (state != null)
            {
                Debug.Log($"Restoring timed game state with {(state.levelRows?.Length ?? 0)} rows");
                timedModeHandler = FindObjectOfType<TimedModeHandler>();
                if (timedModeHandler != null)
                {
                    timedModeHandler.score = state.score;
                    timedModeHandler.bestScore = state.bestScore;
                    
                    // Initialize timer with saved remaining time
                    if (timerManager != null)
                    {
                        timerManager.InitializeTimer(state.remainingTime);
                    }

                    // Restore field state if we have saved rows
                    if (state.levelRows != null && state.levelRows.Length > 0)
                    {
                        var fieldManager = FindObjectOfType<FieldManager>();
                        if (fieldManager != null)
                        {
                            Debug.Log("Restoring field state from saved state");
                            fieldManager.RestoreFromState(state.levelRows);
                        }
                        else
                        {
                            Debug.LogError("Could not find FieldManager component to restore field state");
                        }
                    }

                    // Let TimedModeHandler handle the timer start
                    timedModeHandler.ResumeGame();
                }
            }
            else
            {
                // If no saved state, start fresh timer
                if (timerManager != null && timedModeHandler != null)
                {
                    timerManager.InitializeTimer(GameManager.Instance.GameSettings.globalTimedModeSeconds);
                }
            }
        }

        private void RestartLevel()
        {
            comboCounter = 0;
            missCounter = 0;
            field.ShowOutline(false);
            Load();
        }

        private void SaveGameState()
        {
            if (gameMode == EGameMode.Classic)
            {
                classicModeHandler = FindObjectOfType<ClassicModeHandler>();
                var state = new ClassicGameState
                {
                    score = classicModeHandler.score,
                    bestScore = classicModeHandler.bestScore,
                    gameMode = EGameMode.Classic,
                    gameStatus = EventManager.GameStatus
                };
                GameState.Save(state, field);
            }
            else if (gameMode == EGameMode.Timed)
            {
                timedModeHandler = FindObjectOfType<TimedModeHandler>();
                if (timedModeHandler != null)
                {
                    var state = new TimedGameState
                    {
                        score = timedModeHandler.score,
                        bestScore = timedModeHandler.bestScore,
                        remainingTime = timedModeHandler.GetRemainingTime(),
                        gameMode = EGameMode.Timed,
                        gameStatus = EventManager.GameStatus
                    };
                    GameState.Save(state, field);
                }
            }
        }

        private void OnDisable()
        {
            EventManager.GetEvent(EGameEvent.RestartLevel).Unsubscribe(RestartLevel);
            EventManager.GetEvent<Shape>(EGameEvent.ShapePlaced).Unsubscribe(CheckLines);
            EventManager.OnGameStateChanged -= HandleGameStateChange;

            // Unsubscribe from timer events
            if (timerManager != null)
            {
                timerManager.OnTimerExpired -= OnTimerExpired;
            }
        }

        private void OnTimerExpired()
        {
            // Check if level is complete before triggering a loss
            if (targetManager != null && targetManager.IsLevelComplete())
            {
                // Level complete, trigger win
                SetWin();
            }
            else
            {
                // Level not complete, trigger loss
                SetLose();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if ((gameMode == EGameMode.Classic || gameMode == EGameMode.Timed) && EventManager.GameStatus == EGameState.Playing)
                SaveGameState();

            PauseTimer(pauseStatus);
        }

        private void OnApplicationQuit()
        {
            if ((gameMode == EGameMode.Classic || gameMode == EGameMode.Timed) && EventManager.GameStatus == EGameState.Playing)
                SaveGameState();
        }

        private void Load()
        {
            if (GameManager.Instance.IsTutorialMode())
            {
                _levelData = tutorialManager.GetLevelForPhase();
            }
            else
            {
                gameMode = GameDataManager.GetGameMode();
                _levelData = GameDataManager.GetLevel();
                currentLevel = _levelData.Number;
            }
            if(_levelData == null)
            {
                Debug.LogError("Level data is null");
                return;
            }

            // Apply global time settings if timed mode is enabled
            if (GameManager.Instance.GameSettings.enableTimedMode && _levelData.enableTimer)
            {
                timerDuration = _levelData.timerDuration;
                if(_levelData.timerDuration == 0)
                    timerDuration = GameManager.Instance.GameSettings.globalTimedModeSeconds;
            }

            FindObjectsOfType<MonoBehaviour>().OfType<IBeforeLevelLoadable>().ToList().ForEach(x => x.OnLevelLoaded(_levelData));
            LoadLevel(_levelData);
            FindObjectsOfType<MonoBehaviour>().OfType<ILevelLoadable>().ToList().ForEach(x => x.OnLevelLoaded(_levelData));
            Invoke(nameof(StartGame), 0.5f);
            if (GameManager.Instance.IsTutorialMode())
            {
                tutorialManager.StartTutorial();
            }

            // Initialize timer if enabled for this level or if global timed mode is enabled
            if (_levelData.enableTimer && timerManager != null)
            {
                timerManager.InitializeTimer(timerDuration);
                if (timerPanel != null)
                {
                    timerPanel.SetActive(true);
                }
            }
            else if (timerManager != null)
            {
                timerManager.StopTimer();
                if (timerPanel != null)
                {
                    timerPanel.SetActive(false);
                }
            }
        }

        private void StartGame()
        {
            EventManager.GameStatus = EGameState.PrepareGame;
            classicModeHandler = FindObjectOfType<ClassicModeHandler>();
        }

        private void LoadLevel(Level levelData)
        {
            field.Generate(levelData);
            // Reset field center cache when loading new level
            isFieldCenterCached = false;
            EventManager.GetEvent<Level>(EGameEvent.LevelLoaded).Invoke(levelData);
            OnLevelLoaded?.Invoke(levelData);
        }

        /// <summary>
        /// 检查消除 - 核心游戏逻辑
        /// 在形状放置后检查是否有可消除的行/列
        /// 处理连消、得分、特效等
        /// 新版本：进入PostPlacement状态，使用统一的后处理系统
        /// </summary>
        /// <param name="obj">刚放置的形状</param>
        private void CheckLines(Shape obj)
        {
            // 保存放置前的游戏状态
            var previousState = EventManager.GameStatus;

            // 获取所有填满的行和列
            var lines = field.GetFilledLines(false, false);
            if (lines.Count > 0)
            {
                // 有消除时：增加连消计数
                comboCounter++;
                // 屏幕震动效果（参数：持续时间，强度，频率）
                shakeCanvas.DOShakePosition(0.2f, 35f, 50);

                // 连消大于1时显示边框特效
                if (comboCounter > 1)
                {
                    field.ShowOutline(true);
                }

                // 进入PostPlacement状态
                EventManager.GameStatus = EGameState.PostPlacement;

                // 开始放置后处理流程
                StartCoroutine(ProcessPostPlacement(obj, lines, previousState));
            }
            else
            {
                // 没有消除时：增加失误计数
                missCounter++;
                // 失误次数达到阈值，重置连消
                if (missCounter >= GameManager.Instance.GameSettings.ResetComboAfterMoves)
                {
                    field.ShowOutline(false);
                    missCounter = 0;
                    comboCounter = 0;
                }

                // 检查是否游戏失败
                StartCoroutine(CheckLose());
            }
        }

        /// <summary>
        /// 放置后处理流程（新版本）
        /// 使用PostPlacementProcessor系统统一处理消除、金币方块等逻辑
        /// </summary>
        /// <param name="shape">刚放置的形状</param>
        /// <param name="lines">要消除的行列集合</param>
        /// <param name="previousState">放置前的游戏状态</param>
        private IEnumerator ProcessPostPlacement(Shape shape, List<List<Cell>> lines, EGameState previousState)
        {
            // 创建上下文对象
            var context = new PostPlacementContext(shape, previousState);

            // 分别获取水平和垂直的消除行列（用于填充行列索引）
            var horizontalLines = field.GetFilledLinesHorizontal(false);
            var verticalLines = field.GetFilledLinesVertical(false);

            // 填充消除相关数据
            context.EliminatedLines = lines.Count;

            // 填充行索引（遍历水平消除的行）
            foreach (var line in horizontalLines)
            {
                if (line.Count == 0) continue;

                // 从第一个Cell的位置找到行索引
                var firstCell = line[0];
                for (int row = 0; row < field.cells.GetLength(0); row++)
                {
                    bool found = false;
                    for (int col = 0; col < field.cells.GetLength(1); col++)
                    {
                        if (field.cells[row, col] == firstCell)
                        {
                            context.EliminatedRowIndices.Add(row);
                            found = true;
                            break;
                        }
                    }
                    if (found) break;
                }
            }

            // 填充列索引（遍历垂直消除的列）
            foreach (var line in verticalLines)
            {
                if (line.Count == 0) continue;

                // 从第一个Cell的位置找到列索引
                var firstCell = line[0];
                for (int row = 0; row < field.cells.GetLength(0); row++)
                {
                    bool found = false;
                    for (int col = 0; col < field.cells.GetLength(1); col++)
                    {
                        if (field.cells[row, col] == firstCell)
                        {
                            context.EliminatedColumnIndices.Add(col);
                            found = true;
                            break;
                        }
                    }
                    if (found) break;
                }
            }

            context.IsCombo = comboCounter > 1;

            // 获取PostPlacementProcessorManager
            var processorManager = PostPlacementProcessorManager.Instance;
            if (processorManager != null)
            {
                // 执行所有注册的处理器
                yield return processorManager.ProcessAll(context);
            }
            else
            {
                Debug.LogWarning("[LevelManager] PostPlacementProcessorManager未找到，降级使用旧逻辑");
                // 降级处理：使用旧的AfterMoveProcessing
                yield return AfterMoveProcessing(shape, lines);
            }

            // 恢复游戏状态并检查游戏结束条件
            yield return CheckGameState(context, previousState);
        }

        /// <summary>
        /// 执行消除处理（供EliminationProcessor调用）
        /// 封装原AfterMoveProcessing的消除逻辑
        /// </summary>
        /// <param name="context">放置后处理上下文</param>
        public IEnumerator ProcessElimination(PostPlacementContext context)
        {
            // 重新获取填满的行列（因为上下文中没有存储Cell列表）
            var lines = field.GetFilledLines(false, false);
            if (lines.Count == 0)
            {
                yield break;
            }

            var shape = context.PlacedShape;
            Vector3 center = GetFieldCenter();
            Vector3 scorePosition = center + new Vector3(-0.15f, 1f, 0);
            Vector3 gratzPosition = center + new Vector3(0, 0.45f, 0);

            yield return waitForProcessing;

            // 冒险模式：动画目标
            if (gameMode == EGameMode.Adventure)
            {
                StartCoroutine(targetManager.AnimateTarget(lines));
            }

            // 执行消除
            yield return StartCoroutine(DestroyLines(lines, shape));

            // 计算得分
            var scoreTarget = GameManager.Instance.GameSettings.ScorePerLine * lines.Count * comboCounter;
            OnScored?.Invoke(scoreTarget);

            if (gameMode == EGameMode.Adventure)
            {
                targetManager.UpdateScoreTarget(scoreTarget);
            }

            // 更新上下文得分
            context.TotalScore = scoreTarget;

            // 显示连消文字
            if (comboCounter > 1)
            {
                ShowComboText(comboCounter);
                yield return new WaitForSeconds(0.5f);
            }

            // 显示得分文字
            var scoreText = scoreTextPool.Get();
            scoreText.transform.position = scorePosition;
            scoreText.ShowScore(scoreTarget, scorePosition);
            DOVirtual.DelayedCall(0.75f, () => { scoreTextPool.Release(scoreText); });

            // 显示鼓励文字
            if (Random.Range(0, 3) == 0 && words != null && words.Length > 0)
            {
                int randomIndex = Random.Range(0, words.Length);
                var txt = Instantiate(words[randomIndex], fxPool);
                txt.transform.position = gratzPosition;

                PlayEncouragementSound(txt);

                // 确保文字在画布范围内
                var canvasCorners = new Vector3[4];
                gameCanvas.GetWorldCorners(canvasCorners);

                var txtPosition = txt.transform.position;
                txtPosition.x = Mathf.Clamp(txtPosition.x, canvasCorners[0].x, canvasCorners[2].x);
                txtPosition.y = Mathf.Clamp(txtPosition.y, canvasCorners[0].y, canvasCorners[2].y);
                txt.transform.position = txtPosition;

                DOVirtual.DelayedCall(1.5f, () => {
                    if (txt != null)
                        Destroy(txt);
                });
            }
        }

        /// <summary>
        /// 检查游戏状态（恢复状态并检查游戏结束条件）
        /// </summary>
        /// <param name="context">放置后处理上下文</param>
        /// <param name="previousState">放置前的游戏状态</param>
        private IEnumerator CheckGameState(PostPlacementContext context, EGameState previousState)
        {
            // 恢复到放置前的游戏状态（通常是Playing）
            if (EventManager.GameStatus == EGameState.PostPlacement)
            {
                EventManager.GameStatus = previousState;
            }

            // 检查游戏失败条件（与原逻辑保持一致）
            if (EventManager.GameStatus == EGameState.Playing)
            {
                yield return StartCoroutine(CheckLose());
            }
        }

        /// <summary>
        /// 获取棋盘中心位置（带缓存优化）
        /// 用于特效显示位置计算
        /// </summary>
        /// <returns>棋盘中心的世界坐标</returns>
        private Vector3 GetFieldCenter()
        {
            // 如果已缓存，直接返回
            if (isFieldCenterCached)
                return cachedFieldCenter;

            Vector3 fieldCenter = Vector3.zero;
            int rowCount = field.cells.GetLength(0);
            int colCount = field.cells.GetLength(1);

            // 获取中心格子的位置
            if (rowCount > 0 && colCount > 0)
            {
                Cell centerCell = field.cells[rowCount/2, colCount/2];
                if (centerCell != null)
                {
                    fieldCenter = centerCell.transform.position;
                }
            }

            // 缓存结果
            cachedFieldCenter = fieldCenter;
            isFieldCenterCached = true;
            return fieldCenter;
        }

        /// <summary>
        /// 显示连消文字特效
        /// </summary>
        /// <param name="comboCount">连消次数</param>
        private void ShowComboText(int comboCount)
        {
            Vector3 center = GetFieldCenter();
            Vector3 comboPosition = center + new Vector3(0, 0.75f, 0); // 与得分文字同高度
            var comboText = comboTextPool.Get();
            comboText.transform.position = comboPosition;
            comboText.Show(comboCount);
            DOVirtual.DelayedCall(0.75f, () => { comboTextPool.Release(comboText); }); // 动画结束后回收
        }

        /// <summary>
        /// 放置后的处理流程 - 处理消除、得分、动画
        /// 这是消除后的完整处理流程
        ///
        /// ⚠️ 已过时：请使用PostPlacement状态系统
        /// - 新逻辑：CheckLines() → ProcessPostPlacement() → PostPlacementProcessorManager
        /// - 旧逻辑：CheckLines() → AfterMoveProcessing()（仅作为降级方案保留）
        /// </summary>
        /// <param name="shape">触发消除的形状</param>
        /// <param name="lines">要消除的行列集合</param>
        [Obsolete("已过时，请使用PostPlacement状态系统。此方法仅作为降级方案保留。")]
        private IEnumerator AfterMoveProcessing(Shape shape, List<List<Cell>> lines)
        {
            Vector3 center = GetFieldCenter();
            Vector3 scorePosition = center + new Vector3(-0.15f, 1f, 0); // Move score higher
            Vector3 gratzPosition = center + new Vector3(0, 0.45f, 0); // Position gratz between score and center

            yield return waitForProcessing;
            if (gameMode == EGameMode.Adventure)
            {
                StartCoroutine(targetManager.AnimateTarget(lines));
            }

            yield return StartCoroutine(DestroyLines(lines, shape));

            var scoreTarget = GameManager.Instance.GameSettings.ScorePerLine * lines.Count * comboCounter;
            OnScored?.Invoke(scoreTarget);
            if (gameMode == EGameMode.Adventure)
            {
                targetManager.UpdateScoreTarget(scoreTarget);
            }
            
            // Show combo first if active
            if (comboCounter > 1)
            {
                ShowComboText(comboCounter);
                yield return new WaitForSeconds(0.5f);
            }

            // Then show score at higher position
            var scoreText = scoreTextPool.Get();
            scoreText.transform.position = scorePosition;
            scoreText.ShowScore(scoreTarget, scorePosition);
            DOVirtual.DelayedCall(0.75f, () => { scoreTextPool.Release(scoreText); }); // Halved from 1.5f to match faster animation

            // Show congratulatory words below score
            if (Random.Range(0, 3) == 0 && words != null && words.Length > 0)
            {
                // 随机选择一个鼓励词预制体并实例化
                int randomIndex = Random.Range(0, words.Length);
                var txt = Instantiate(words[randomIndex], fxPool);
                txt.transform.position = gratzPosition;

                // 播放对应的鼓励词音效
                PlayEncouragementSound(txt);

                // Ensure txt is within the bounds of the gameCanvas
                var canvasCorners = new Vector3[4];
                gameCanvas.GetWorldCorners(canvasCorners);

                var txtPosition = txt.transform.position;
                txtPosition.x = Mathf.Clamp(txtPosition.x, canvasCorners[0].x, canvasCorners[2].x);
                txtPosition.y = Mathf.Clamp(txtPosition.y, canvasCorners[0].y, canvasCorners[2].y);
                txt.transform.position = txtPosition;

                // 1.5秒后销毁鼓励词对象
                DOVirtual.DelayedCall(1.5f, () => {
                    if (txt != null)
                        Destroy(txt);
                });
            }

            if (EventManager.GameStatus == EGameState.Playing)
                yield return StartCoroutine(CheckLose());
        }

        /// <summary>
        /// 检查游戏是否失败
        /// 检查是否还有可放置的位置
        /// </summary>
        private IEnumerator CheckLose()
        {
            // 冒险模式下，如果即将完成关卡，设置等待胜利状态
            if (gameMode != EGameMode.Classic && targetManager != null && targetManager.WillLevelBeComplete())
            {
                EventManager.GameStatus = EGameState.WinWaiting;
            }

            yield return waitForCheckLose; // 保留小延迟以保证游戏流畅

            // 检查是否有形状可以放置
            var lose = true;
            var availableShapes = cellDeck.GetShapes();
            foreach (var shape in availableShapes)
            {
                if (field.CanPlaceShape(shape))
                {
                    lose = false;  // 还有可放置的位置
                    break;
                }
            }

            // 冒险模式下，如果即将完成，触发胜利
            if (gameMode != EGameMode.Classic && targetManager != null && targetManager.WillLevelBeComplete())
            {
                yield return waitForCheckLose;
                SetWin();
                lose = false;
            }

            // 没有可放置位置，触发失败
            if (lose)
            {
                // 注意：关卡失败事件(LevelFailed)由Retry/Revive触发，这里不触发
                SetLose();
            }

            yield return null;
        }

        /// <summary>
        /// 设置游戏胜利
        /// </summary>
        private void SetWin()
        {
            // 解锁下一关
            GameDataManager.UnlockLevel(currentLevel + 1);

            // 触发关卡通关事件（用于难度系统）
            EventManager.GetEvent(EGameEvent.LevelCompleted).Invoke();

            // 设置游戏状态为预胜利
            EventManager.GameStatus = EGameState.PreWin;
        }

        /// <summary>
        /// 设置游戏失败
        /// </summary>
        private void SetLose()
        {
            // 根据模式删除存档
            if (gameMode == EGameMode.Classic)
                GameState.Delete(EGameMode.Classic);
            else if (gameMode == EGameMode.Timed)
                GameState.Delete(EGameMode.Timed);

            // 触发失败事件
            OnLose?.Invoke();
            // 设置游戏状态为预失败
            EventManager.GameStatus = EGameState.PreFailed;
        }

        /// <summary>
        /// 游戏结束动画
        /// 失败时填充所有空格子
        /// </summary>
        /// <param name="action">动画完成后的回调</param>
        public IEnumerator EndAnimations(Action action)
        {
            yield return StartCoroutine(FillEmptyCellsFailed());
            action?.Invoke();
        }

        /// <summary>
        /// 失败时填充空格子动画
        /// </summary>
        private IEnumerator FillEmptyCellsFailed()
        {
            // 播放填充音效
            SoundBase.Instance.PlaySound(SoundBase.Instance.fillEmpty);
            // 加载默认模板
            var template = Resources.Load<ItemTemplate>("Items/ItemTemplate 0");
            // 获取所有空格子
            emptyCells = field.GetEmptyCells();
            // 逐个填充动画
            foreach (var cell in emptyCells)
            {
                cell.FillCellFailed(template);
                yield return waitForFillEmpty;  // 延迟创建连续动画效果
            }
        }

        /// <summary>
        /// 清除空格子（用于重置）
        /// </summary>
        public void ClearEmptyCells()
        {
            // 添加null检查，防止复活时出现空引用异常
            if (emptyCells == null || emptyCells.Length == 0)
            {
                Debug.Log("[LevelManager] ClearEmptyCells: emptyCells为空，跳过清除操作");
                return;
            }

            foreach (var cell in emptyCells)
            {
                if (cell != null)  // 额外的安全检查
                {
                    cell.ClearCell();
                }
            }
        }

        /// <summary>
        /// 销毁行列 - 执行消除动画和逻辑
        /// </summary>
        /// <param name="lines">要消除的行列集合</param>
        /// <param name="shape">触发消除的形状</param>
        private IEnumerator DestroyLines(List<List<Cell>> lines, Shape shape)
        {
            // 播放连消音效（根据连消数选择不同音效）
            SoundBase.Instance.PlayLimitSound(SoundBase.Instance.combo[Mathf.Min(comboCounter, SoundBase.Instance.combo.Length - 1)]);
            // 发布行销毁事件
            EventManager.GetEvent<Shape>(EGameEvent.LineDestroyed).Invoke(shape);

            // 立即标记所有格子为正在销毁状态
            foreach (var line in lines)
            {
                foreach (var cell in line)
                {
                    cell.SetDestroying(true);
                }
            }

            // 为每行播放爆炸特效并销毁格子
            foreach (var line in lines)
            {
                if (line.Count == 0) continue;

                // 从对象池获取爆炸特效
                var lineExplosion = lineExplosionPool.Get();
                // 播放爆炸动画
                lineExplosion.Play(line, shape, RectTransformUtils.GetMinMaxAndSizeForCanvas(line, gameCanvas.GetComponent<Canvas>()), GetExplosionColor(shape));
                // 1.5秒后回收特效
                DOVirtual.DelayedCall(1.5f, () => { lineExplosionPool.Release(lineExplosion); });

                // 销毁行中的所有格子
                foreach (var cell in line)
                {
                    cell.DestroyCell();
                }
            }

            yield return null;
        }

        /// <summary>
        /// 获取爆炸特效的颜色
        /// </summary>
        /// <param name="shape">形状</param>
        /// <returns>爆炸特效使用的颜色</returns>
        private Color GetExplosionColor(Shape shape)
        {
            // 获取形状第一个Item的覆盖层颜色
            var itemTemplateTopColor = shape.GetActiveItems()[0].itemTemplate.overlayColor;
            // 单色模式下使用统一颜色
            if (_levelData.levelType.singleColorMode)
            {
                itemTemplateTopColor = itemFactory.GetOneColor().overlayColor;
            }

            return itemTemplateTopColor;
        }

        /// <summary>
        /// 播放鼓励词音效
        /// 根据显示的鼓励词预制体名称播放对应的音效
        /// </summary>
        /// <param name="wordGameObject">鼓励词GameObject</param>
        private void PlayEncouragementSound(GameObject wordGameObject)
        {
            if (wordGameObject == null || SoundBase.Instance == null)
                return;

            // 根据GameObject名称判断并播放对应音效
            string wordName = wordGameObject.name.ToLower();

            // 移除"(Clone)"后缀（如果存在）
            if (wordName.Contains("(clone)"))
            {
                wordName = wordName.Replace("(clone)", "").Trim();
            }

            // 根据名称播放对应音效
            if (wordName.Contains("good") && SoundBase.Instance.goodSound != null)
            {
                SoundBase.Instance.PlaySound(SoundBase.Instance.goodSound);
            }
            else if (wordName.Contains("great") && SoundBase.Instance.greatSound != null)
            {
                SoundBase.Instance.PlaySound(SoundBase.Instance.greatSound);
            }
            else if ((wordName.Contains("fantastic") || wordName.Contains("excellent"))
                     && SoundBase.Instance.excellentSound != null)
            {
                SoundBase.Instance.PlaySound(SoundBase.Instance.excellentSound);
            }
        }

        private void Update()
        {
            if (Keyboard.current != null)
            {
                // Debug keys for win/lose
                if(Keyboard.current[GameManager.Instance.debugSettings.Win].wasPressedThisFrame)
                {
                    SetWin();
                }

                if(Keyboard.current[GameManager.Instance.debugSettings.Lose].wasPressedThisFrame)
                {
                    SetLose();
                }

                // Other debug keys
                if (Keyboard.current.spaceKey.wasPressedThisFrame)
                {
                    // Fill the first row with tiles
                    var rowCells = new List<Cell>();
                    for (int col = 0; col < field.cells.GetLength(1); col++)
                    {
                        rowCells.Add(field.cells[0, col]);
                    }

                    var itemTemplate = Resources.Load<ItemTemplate>("Items/ItemTemplate 0");
                    
                    // Get all available bonus items from the level data
                    var availableBonuses = _levelData.targetInstance
                        .Where(t => t.targetScriptable.bonusItem != null)
                        .Select(t => t.targetScriptable.bonusItem)
                        .ToList();

                    foreach (var cell in rowCells)
                    {
                        if (cell != null && cell.IsEmpty())
                        {
                            cell.FillCell(itemTemplate);
                            
                            // 30% chance to add a bonus to the cell
                            if (availableBonuses.Count > 0 && Random.Range(0f, 1f) < 0.3f)
                            {
                                var randomBonus = availableBonuses[Random.Range(0, availableBonuses.Count)];
                                cell.SetBonus(randomBonus);
                            }
                        }
                    }

                    // Increment combo and show effects
                    comboCounter++;
                    field.ShowOutline(true);
                    
                    // Calculate score for a full row
                    int scoreToAdd = GameManager.Instance.GameSettings.ScorePerLine * comboCounter;
                    
                    // Add score based on game mode
                    if (gameMode == EGameMode.Classic)
                    {
                        if (classicModeHandler != null)
                            classicModeHandler.UpdateScore(classicModeHandler.score + scoreToAdd);
                    }
                    else if (gameMode == EGameMode.Timed)
                    {
                        if (timedModeHandler != null)
                            timedModeHandler.UpdateScore(timedModeHandler.score + scoreToAdd);
                    }

                    // Create a dummy shape for the animation position
                    var dummyShape = itemFactory.CreateRandomShape(null, PoolObject.GetObject(cellDeck.shapePrefab.gameObject));
                    dummyShape.transform.position = rowCells[0].transform.position;
                    
                    // Screen shake effect
                    shakeCanvas.DOShakePosition(0.2f, 35f, 50);
                    
                    // Process the row destruction with proper animations
                    StartCoroutine(AfterMoveProcessing(dummyShape, new List<List<Cell>> { rowCells }));
                    
                    // Clean up the dummy shape
                    Destroy(dummyShape.gameObject);
                }

                // Use the configurable UpdateDeck key from debug settings instead of hardcoded dKey
                if (Keyboard.current[GameManager.Instance.debugSettings.UpdateDeck].wasPressedThisFrame)
                {
                    cellDeck.ClearCellDecks();
                    cellDeck.FillCellDecks();
                }

                if (Keyboard.current.aKey.wasPressedThisFrame)
                {
                    StartCoroutine(CheckLose());
                }

                if (Keyboard.current.rKey.wasPressedThisFrame)
                {
                    GameManager.Instance.RestartLevel();
                }
            }
        }

        public Level GetCurrentLevel()
        {
            return _levelData;
        }

        public EGameMode GetGameMode()
        {
            return gameMode;
        }

        public FieldManager GetFieldManager()
        {
            return field;
        }

        /// <summary>
        /// 获取当前关卡进度百分比
        /// </summary>
        /// <returns>进度百分比（0-100）</returns>
        public float GetCurrentProgress()
        {
            switch (gameMode)
            {
                case EGameMode.Classic:
                    // 经典模式：当前分数与历史最高分的比例
                    if (classicModeHandler != null)
                    {
                        int currentScore = classicModeHandler.score;
                        int bestScore = classicModeHandler.bestScore;
                        if (bestScore > 0)
                        {
                            return Mathf.Clamp((float)currentScore / bestScore * 100f, 0f, 100f);
                        }
                        // 如果没有历史最高分，使用默认目标1000分
                        return Mathf.Clamp((float)currentScore / 1000f * 100f, 0f, 100f);
                    }
                    break;

                case EGameMode.Timed:
                    // 计时模式：当前分数与历史最高分的比例
                    if (timedModeHandler != null)
                    {
                        int currentScore = timedModeHandler.score;
                        int bestScore = timedModeHandler.bestScore;
                        if (bestScore > 0)
                        {
                            return Mathf.Clamp((float)currentScore / bestScore * 100f, 0f, 100f);
                        }
                        // 如果没有历史最高分，使用默认目标2000分
                        return Mathf.Clamp((float)currentScore / 2000f * 100f, 0f, 100f);
                    }
                    break;

                case EGameMode.Adventure:
                    // 冒险模式：根据关卡类型计算进度
                    if (_levelData != null && _levelData.levelType != null)
                    {
                        // 根据关卡类型处理
                        switch (_levelData.levelType.elevelType)
                        {
                            case ELevelType.Score:
                                // Score类型关卡：检查分数目标
                                if (_levelData.targetInstance != null)
                                {
                                    // 查找分数目标
                                    foreach (var target in _levelData.targetInstance)
                                    {
                                        if (target.targetScriptable is ScoreTargetScriptable)
                                        {
                                            // 对于分数目标，totalAmount是初始目标分数，amount是剩余需要的分数
                                            int targetScore = target.totalAmount > 0 ? target.totalAmount : target.amount;

                                            // 计算当前已获得的分数
                                            int currentScore = 0;
                                            if (targetScore > 0)
                                            {
                                                // 如果是冒险模式，从TargetManager获取实际的目标实例
                                                if (targetManager != null)
                                                {
                                                    var targets = targetManager.GetTargets();
                                                    var scoreTarget = targets?.FirstOrDefault(t => t.targetScriptable is ScoreTargetScriptable);
                                                    if (scoreTarget != null)
                                                    {
                                                        // 已获得分数 = 总目标分数 - 剩余需要分数
                                                        int totalTargetScore = scoreTarget.totalAmount > 0 ? scoreTarget.totalAmount : targetScore;
                                                        currentScore = totalTargetScore - scoreTarget.amount;
                                                        return Mathf.Clamp((float)currentScore / totalTargetScore * 100f, 0f, 100f);
                                                    }
                                                }

                                                // 如果无法从TargetManager获取，尝试使用GameManager（作为后备方案）
                                                if (GameManager.Instance != null)
                                                {
                                                    currentScore = GameManager.Instance.Score;
                                                    return Mathf.Clamp((float)currentScore / targetScore * 100f, 0f, 100f);
                                                }
                                            }
                                        }
                                    }
                                }
                                // 如果没有找到分数目标，返回0
                                return 0f;

                            case ELevelType.CollectItems:
                                // CollectItems类型：计算总收集进度
                                // 进度 = (所有已收集物品总数 / 所有物品目标总数) * 100
                                if (targetManager != null && targetManager.GetTargets() != null)
                                {
                                    var targets = targetManager.GetTargets();
                                    int totalCollected = 0;
                                    int totalRequired = 0;

                                    foreach (var target in targets)
                                    {
                                        if (target.targetScriptable == null)
                                            continue;

                                        // 获取该目标的总需求量
                                        int requiredAmount = target.totalAmount > 0 ? target.totalAmount : 100;
                                        totalRequired += requiredAmount;

                                        if (target.targetScriptable.descending)
                                        {
                                            // 倒计目标：已收集 = 总量 - 剩余量
                                            int collected = requiredAmount - target.amount;
                                            // 确保收集数不超过目标数
                                            collected = Mathf.Clamp(collected, 0, requiredAmount);
                                            totalCollected += collected;
                                        }
                                        else
                                        {
                                            // 正计目标：amount就是已收集数量
                                            // 确保收集数不超过目标数
                                            int collected = Mathf.Clamp(target.amount, 0, requiredAmount);
                                            totalCollected += collected;
                                        }
                                    }

                                    if (totalRequired > 0)
                                    {
                                        return Mathf.Clamp((float)totalCollected / totalRequired * 100f, 0f, 100f);
                                    }
                                }
                                // 如果无法计算，返回0
                                return 0f;

                            case ELevelType.Classic:
                            default:
                                // 其他类型：使用目标管理器的进度（平均值）
                                if (targetManager != null)
                                {
                                    return targetManager.GetOverallProgress();
                                }
                                break;
                        }
                    }
                    break;

                default:
                    Debug.LogWarning($"[LevelManager] GetCurrentProgress: Unknown game mode {gameMode}");
                    break;
            }

            // 默认返回0
            return 0f;
        }

        public void PauseTimer(bool pause)
        {
            if (timerManager != null)
            {
                timerManager.PauseTimer(pause);
            }
        }
    }
}