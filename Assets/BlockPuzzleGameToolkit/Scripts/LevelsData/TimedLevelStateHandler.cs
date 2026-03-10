using UnityEngine;
using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.Gameplay;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using BlockPuzzleGameToolkit.Scripts.Popups;

namespace BlockPuzzleGameToolkit.Scripts.LevelsData
{
    // [CreateAssetMenu(fileName = "TimedStateHandler", menuName = "BlockPuzzleGameToolkit/Levels/TimedStateHandler")]
    public class TimedLevelStateHandler : LevelStateHandler
    {
        private protected override void HandlePreFailed(LevelManager levelManager)
        {
            // 停止计时器
            levelManager.timerManager?.StopTimer();

            // 如果是因为时间到了，直接失败
            if (levelManager.timerManager != null && levelManager.timerManager.RemainingTime <= 0)
            {
                EventManager.GameStatus = EGameState.Failed;
                return;
            }

            // 调用基类的统一处理逻辑
            base.HandlePreFailed(levelManager);
        }

        private protected override void HandlePreWin(LevelManager levelManager)
        {
            // 停止计时器
            levelManager.timerManager?.StopTimer();
            // 调用基类的统一处理（显示1.5秒后自动进入Win）
            base.HandlePreWin(levelManager);
        }
    }
}