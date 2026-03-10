using BlockPuzzleGameToolkit.Scripts.Data;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using BlockPuzzleGameToolkit.Scripts.Enums;
using TMPro;
using UnityEngine;
using System.Collections;

namespace BlockPuzzleGameToolkit.Scripts.Gameplay
{
    /// <summary>
    /// BaseModeHandler - 游戏模式处理器基类
    /// 提供所有游戏模式（经典、计时、冒险）的通用功能
    /// 管理分数显示、最高分记录、分数动画等
    /// </summary>
    public abstract class BaseModeHandler : MonoBehaviour
    {
        // ========== UI组件 ==========
        /// <summary>
        /// 当前分数文本显示
        /// </summary>
        public TextMeshProUGUI scoreText;

        /// <summary>
        /// 最高分文本显示
        /// </summary>
        public TextMeshProUGUI bestScoreText;

        // ========== 分数数据 ==========
        /// <summary>
        /// 历史最高分
        /// </summary>
        [HideInInspector]
        public int bestScore;

        /// <summary>
        /// 当前分数
        /// </summary>
        [HideInInspector]
        public int score;

        // ========== 内部变量 ==========
        /// <summary>
        /// 关卡管理器引用
        /// </summary>
        protected LevelManager _levelManager;

        /// <summary>
        /// 分数计数动画协程
        /// </summary>
        protected Coroutine _counterCoroutine;

        /// <summary>
        /// 当前显示的分数（用于动画）
        /// </summary>
        protected int _displayedScore = 0;

        /// <summary>
        /// 分数动画速度（秒/每次增加）
        /// </summary>
        [SerializeField]
        protected float counterSpeed = 0.01f;

        /// <summary>
        /// Unity生命周期 - 启用时初始化
        /// 订阅事件并加载分数
        /// </summary>
        protected virtual void OnEnable()
        {
            _levelManager = FindObjectOfType<LevelManager>(true);

            if (_levelManager == null)
            {
                Debug.LogError("LevelManager not found!");
                return;
            }

            // 订阅游戏事件
            _levelManager.OnLose += OnLose;
            _levelManager.OnScored += OnScored;

            // 加载历史分数
            LoadScores();
        }

        /// <summary>
        /// Unity生命周期 - 禁用时清理
        /// 取消事件订阅
        /// </summary>
        protected virtual void OnDisable()
        {
            if (_levelManager != null)
            {
                _levelManager.OnLose -= OnLose;
                _levelManager.OnScored -= OnScored;
            }
        }

        /// <summary>
        /// 应用程序暂停时保存游戏状态
        /// </summary>
        /// <param name="pauseStatus">是否暂停</param>
        protected virtual void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && EventManager.GameStatus == EGameState.Playing)
            {
                SaveGameState();
            }
        }

        /// <summary>
        /// 应用程序退出时保存游戏状态
        /// </summary>
        protected virtual void OnApplicationQuit()
        {
            if (EventManager.GameStatus == EGameState.Playing)
            {
                SaveGameState();
            }
        }

        /// <summary>
        /// 处理得分事件
        /// 更新分数并播放动画
        /// </summary>
        /// <param name="scoreToAdd">要添加的分数</param>
        public virtual void OnScored(int scoreToAdd)
        {
            int previousScore = this.score;
            this.score += scoreToAdd;

            // 立即更新UI（防止延迟）
            scoreText.text = score.ToString();

            // 启动分数动画
            if (_counterCoroutine != null)
            {
                StopCoroutine(_counterCoroutine);
            }
            _counterCoroutine = StartCoroutine(CountScore(previousScore, this.score));
        }

        /// <summary>
        /// 分数计数动画协程
        /// 创建从旧分数到新分数的递增动画
        /// </summary>
        /// <param name="startValue">起始分数</param>
        /// <param name="endValue">目标分数</param>
        protected IEnumerator CountScore(int startValue, int endValue)
        {
            _displayedScore = startValue;

            // 根据分数差值调整动画速度
            float actualSpeed = counterSpeed;
            if (endValue - startValue > 100)
                actualSpeed = counterSpeed * 0.5f;  // 大差值时加速
            else if (endValue - startValue > 500)
                actualSpeed = counterSpeed * 0.2f;  // 超大差值时更快

            // 递增显示分数
            while (_displayedScore < endValue)
            {
                _displayedScore++;
                scoreText.text = _displayedScore.ToString();
                yield return new WaitForSeconds(actualSpeed);
            }

            // 确保最终显示正确的分数
            _displayedScore = endValue;
            scoreText.text = endValue.ToString();
        }

        /// <summary>
        /// 游戏失败时的处理
        /// 删除游戏状态存档
        /// </summary>
        public virtual void OnLose()
        {
            DeleteGameState();
        }

        /// <summary>
        /// 直接更新分数
        /// 用于从存档恢复分数
        /// </summary>
        /// <param name="newScore">新的分数值</param>
        public virtual void UpdateScore(int newScore)
        {
            int previousScore = this.score;
            this.score = newScore;

            // 立即更新UI
            scoreText.text = score.ToString();

            // 播放变化动画
            if (_counterCoroutine != null)
            {
                StopCoroutine(_counterCoroutine);
            }
            _counterCoroutine = StartCoroutine(CountScore(previousScore, this.score));
        }

        /// <summary>
        /// 重置分数
        /// 清零分数并删除存档
        /// </summary>
        public virtual void ResetScore()
        {
            // 停止进行中的分数动画
            if (_counterCoroutine != null)
            {
                StopCoroutine(_counterCoroutine);
                _counterCoroutine = null;
            }

            // 重置分数
            score = 0;
            _displayedScore = 0;

            // 更新UI
            scoreText.text = "0";

            // 删除游戏状态存档
            DeleteGameState();
        }

        // ========== 抽象方法 - 子类必须实现 ==========
        /// <summary>
        /// 加载历史分数（最高分等）
        /// </summary>
        protected abstract void LoadScores();

        /// <summary>
        /// 保存游戏状态
        /// </summary>
        protected abstract void SaveGameState();

        /// <summary>
        /// 删除游戏状态存档
        /// </summary>
        protected abstract void DeleteGameState();
    }
}