using BlockPuzzleGameToolkit.Scripts.Data;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using BlockPuzzleGameToolkit.Scripts.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace BlockPuzzleGameToolkit.Scripts.Gameplay
{
    /// <summary>
    /// ClassicModeHandler - 经典模式处理器
    /// 继承自BaseModeHandler，实现经典模式的具体逻辑
    /// 经典模式：无时间限制，持续游戏直到无法放置形状
    /// </summary>
    public class ClassicModeHandler : BaseModeHandler
    {
        /// <summary>
        /// 菱形图片UI组件（可能用于装饰或特效）
        /// </summary>
        public Image rhombusImage;

        /// <summary>
        /// 加载分数数据
        /// 从资源管理器和存档中恢复分数
        /// </summary>
        protected override void LoadScores()
        {
            // 从资源管理器加载历史最高分
            bestScore = ResourceManager.Instance.GetResource("Score").GetValue();
            bestScoreText.text = bestScore.ToString();

            // 从游戏存档加载当前分数（断线重连）
            var state = GameState.Load(EGameMode.Classic) as ClassicGameState;
            if (state != null)
            {
                score = state.score;
                bestScore = state.bestScore;
                scoreText.text = score.ToString();
            }
            else
            {
                // 没有存档，从零开始
                score = 0;
                scoreText.text = "0";
            }
        }

        /// <summary>
        /// 保存游戏状态
        /// 保存当前分数和棋盘状态到本地
        /// </summary>
        protected override void SaveGameState()
        {
            var fieldManager = _levelManager.GetFieldManager();
            if (fieldManager != null)
            {
                var state = new ClassicGameState
                {
                    score = score,
                    bestScore = bestScore,
                    gameMode = EGameMode.Classic,
                    gameStatus = EventManager.GameStatus
                };
                // 保存状态和棋盘数据
                GameState.Save(state, fieldManager);
            }
        }

        /// <summary>
        /// 删除游戏状态存档
        /// 用于重新开始或游戏结束
        /// </summary>
        protected override void DeleteGameState()
        {
            GameState.Delete(EGameMode.Classic);
        }

        /// <summary>
        /// 游戏失败时的处理
        /// 更新最高分记录
        /// </summary>
        public override void OnLose()
        {
            // 获取当前最高分
            bestScore = ResourceManager.Instance.GetResource("Score").GetValue();

            // 如果当前分数超过最高分，更新记录
            if (score > bestScore)
            {
                ResourceManager.Instance.GetResource("Score").Set(score);
            }

            // 调用基类的失败处理（删除存档）
            base.OnLose();
        }
    }
}