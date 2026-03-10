using BlockPuzzleGameToolkit.Scripts.Data;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.Gameplay.Managers;
using DG.Tweening;
using TMPro;
using UnityEngine;
using System.Collections;

namespace BlockPuzzleGameToolkit.Scripts.Gameplay
{
    /// <summary>
    /// TimedModeHandler - 计时模式处理器
    /// 继承自BaseModeHandler，实现计时模式的具体逻辑
    /// 计时模式：限时游戏，倒计时结束或无法放置形状时游戏结束
    /// </summary>
    public class TimedModeHandler : BaseModeHandler
    {
        /// <summary>
        /// 游戏时长（秒）
        /// 默认180秒（3分钟）
        /// </summary>
        [SerializeField]
        private float gameDuration = 180f; // 3 minutes default game duration

        /// <summary>
        /// 计时器管理器引用
        /// </summary>
        private TimerManager _timerManager;

        /// <summary>
        /// 脉冲动画序列（可能用于时间即将耗尽的视觉效果）
        /// </summary>
        private Sequence _pulseSequence;

        /// <summary>
        /// 获取计时器管理器的属性
        /// 使用懒加载模式
        /// </summary>
        public TimerManager TimerManager
        {
            get
            {
                if (_timerManager == null)
                {
                    _timerManager = FindObjectOfType<TimerManager>();
                }
                return _timerManager;
            }
        }

        /// <summary>
        /// Unity生命周期 - 启用时初始化
        /// 订阅游戏状态变化事件
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            if (TimerManager == null)
            {
                Debug.LogError("TimerManager not found!");
                return;
            }

            // 订阅游戏状态变化，以便暂停/恢复计时器
            EventManager.OnGameStateChanged += HandleGameStateChange;
        }

        /// <summary>
        /// Unity生命周期 - 禁用时清理
        /// 取消事件订阅
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();
            EventManager.OnGameStateChanged -= HandleGameStateChange;
        }

        /// <summary>
        /// 加载分数数据
        /// 从资源管理器和存档中恢复分数和计时器
        /// </summary>
        protected override void LoadScores()
        {
            // 从资源管理器加载计时模式最高分
            bestScore = ResourceManager.Instance.GetResource("TimedBestScore").GetValue();
            bestScoreText.text = bestScore.ToString();

            // 从存档加载游戏状态
            var state = GameState.Load(EGameMode.Timed) as TimedGameState;
            if (state != null)
            {
                // 恢复分数和剩余时间
                score = state.score;
                bestScore = state.bestScore;
                scoreText.text = score.ToString();
                // 恢复计时器（如果有剩余时间则使用，否则重新开始）
                TimerManager.InitializeTimer(state.remainingTime > 0 ? state.remainingTime : gameDuration);
            }
            else
            {
                // 新游戏，从零开始
                score = 0;
                scoreText.text = "0";
                TimerManager.InitializeTimer(gameDuration);
            }
        }

        /// <summary>
        /// 保存游戏状态
        /// 保存分数、最高分、剩余时间和棋盘状态
        /// </summary>
        protected override void SaveGameState()
        {
            var fieldManager = _levelManager.GetFieldManager();
            if (fieldManager != null)
            {
                var state = new TimedGameState
                {
                    score = score,
                    bestScore = bestScore,
                    remainingTime = GetRemainingTime(),  // 保存剩余时间
                    gameMode = EGameMode.Timed,
                    gameStatus = EventManager.GameStatus
                };
                GameState.Save(state, fieldManager);
            }
        }

        /// <summary>
        /// 删除游戏状态存档
        /// </summary>
        protected override void DeleteGameState()
        {
            GameState.Delete(EGameMode.Timed);
        }

        /// <summary>
        /// 处理得分事件
        /// 可以在此处添加奖励时间机制
        /// </summary>
        /// <param name="scoreToAdd">获得的分数</param>
        public override void OnScored(int scoreToAdd)
        {
            base.OnScored(scoreToAdd);
            // AddBonusTime(scoreToAdd);  // 可选：根据分数增加奖励时间
        }

        /// <summary>
        /// 添加奖励时间
        /// 根据得分增加游戏时间（当前已禁用）
        /// </summary>
        /// <param name="scoreValue">获得的分数</param>
        private void AddBonusTime(int scoreValue)
        {
            // 每10分增加1秒奖励时间
            float bonusTime = scoreValue / 10f;
            float currentTime = TimerManager.RemainingTime;
            // 增加时间但不超过初始时长
            TimerManager.InitializeTimer(Mathf.Min(currentTime + bonusTime, gameDuration));
        }

        /// <summary>
        /// 游戏失败时的处理
        /// 只有在计时器归零时才更新最高分
        /// </summary>
        public override void OnLose()
        {
            // 只有计时器到期时才更新最高分（区分是时间到还是无法放置）
            if (TimerManager != null && TimerManager.RemainingTime <= 0)
            {
                bestScore = ResourceManager.Instance.GetResource("TimedBestScore").GetValue();
                if (score > bestScore)
                {
                    ResourceManager.Instance.GetResource("TimedBestScore").Set(score);
                }
            }

            base.OnLose();
        }

        /// <summary>
        /// 暂停游戏
        /// 暂停计时器
        /// </summary>
        public void PauseGame()
        {
            if (TimerManager != null)
            {
                TimerManager.PauseTimer(true);
            }
        }

        /// <summary>
        /// 恢复游戏
        /// 恢复计时器
        /// </summary>
        public void ResumeGame()
        {
            if (TimerManager != null)
            {
                TimerManager.PauseTimer(false);
            }
        }

        /// <summary>
        /// 获取剩余时间
        /// </summary>
        /// <returns>剩余秒数</returns>
        public float GetRemainingTime()
        {
            return TimerManager != null ? TimerManager.RemainingTime : 0f;
        }

        /// <summary>
        /// 处理游戏状态变化
        /// 根据游戏状态暂停或恢复计时器
        /// </summary>
        /// <param name="newState">新的游戏状态</param>
        private void HandleGameStateChange(EGameState newState)
        {
            if (TimerManager != null)
            {
                if (newState == EGameState.Playing)
                {
                    TimerManager.PauseTimer(false);  // 恢复计时
                }
                else if (newState == EGameState.Paused)
                {
                    TimerManager.PauseTimer(true);   // 暂停计时
                }
            }
        }
    }
}