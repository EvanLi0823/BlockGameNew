using UnityEngine;
using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.Gameplay;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using BlockPuzzleGameToolkit.Scripts.Popups;

namespace BlockPuzzleGameToolkit.Scripts.LevelsData
{
    // [CreateAssetMenu(fileName = "AdventureStateHandler", menuName = "BlockPuzzleGameToolkit/Levels/AdventureStateHandler")]
    public class AdventureLevelStateHandler : LevelStateHandler
    {
        private protected override void HandlePreFailed(LevelManager levelManager)
        {
            // 停止计时器（如果有）
            levelManager.timerManager?.StopTimer();

            // 调用基类的统一处理逻辑
            base.HandlePreFailed(levelManager);
        }
    }
}