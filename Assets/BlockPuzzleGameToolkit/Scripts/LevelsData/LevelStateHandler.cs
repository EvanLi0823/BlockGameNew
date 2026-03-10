using UnityEngine;
using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.Gameplay;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using BlockPuzzleGameToolkit.Scripts.Popups;
using BlockPuzzleGameToolkit.Scripts.Popups.Reward;
using DG.Tweening;

namespace BlockPuzzleGameToolkit.Scripts.LevelsData
{
    public abstract class LevelStateHandler : ScriptableObject
    {
        public virtual void HandleState(EGameState state, LevelManager levelManager)
        {
            switch (state)
            {
                case EGameState.PrepareGame:
                    HandlePrepareGame(levelManager);
                    break;
                case EGameState.Playing:
                    HandlePlaying(levelManager);
                    break;
                case EGameState.PostPlacement:
                    HandlePostPlacement(levelManager);
                    break;
                case EGameState.PreFailed:
                    HandlePreFailed(levelManager);
                    break;
                case EGameState.Failed:
                    HandleFailed(levelManager);
                    break;
                case EGameState.PreWin:
                    HandlePreWin(levelManager);
                    break;
                case EGameState.Win:
                    HandleWin(levelManager);
                    break;
            }
        }
        
        private protected virtual void HandlePrepareGame(LevelManager levelManager)
        {
            var level = levelManager.GetCurrentLevel();
            var prePlayPopup = level.levelType.prePlayPopup;

            if (prePlayPopup != null)
            {
                MenuManager.Instance.ShowPopup(prePlayPopup, null, _ => EventManager.GameStatus = EGameState.Playing);
            }
            else
            {
                EventManager.GameStatus = EGameState.Playing;
            }
        }

        private protected virtual void HandlePlaying(LevelManager levelManager) {}

        /// <summary>
        /// 处理PostPlacement状态
        /// 此状态由PostPlacementProcessorManager自动管理，无需额外处理
        /// </summary>
        private protected virtual void HandlePostPlacement(LevelManager levelManager)
        {
            // PostPlacement状态下，游戏逻辑由PostPlacementProcessorManager统一处理
            // 包括：消除动画、得分计算、金币方块处理等
            // 处理完成后会自动恢复到Playing状态
            // 此方法留空，子类可根据需要重写
        }

        private protected virtual void HandlePreFailed(LevelManager levelManager)
        {
            // 简化的PreFailed处理：仅显示弹窗1.5秒后自动进入Failed阶段
            if (GameManager.Instance.GameSettings.enablePreFailedPopup)
            {
                // 使用非泛型方法加载弹窗，避免类型转换错误
                var preFailedPopup = Resources.Load<Popup>("Popups/PreFailed");
                if (preFailedPopup != null)
                {
                    MenuManager.Instance.ShowPopup(preFailedPopup);

                    // 1.5秒后自动关闭弹窗并进入Failed阶段
                    DOVirtual.DelayedCall(1.5f, () =>
                    {
                        // 关闭PreFailed弹窗
                        var popup = MenuManager.Instance.GetLastPopup();
                        if (popup != null && popup.GetType().Name.Contains("PreFailed"))
                        {
                            popup.Close();
                        }

                        // 进入Failed阶段
                        EventManager.GameStatus = EGameState.Failed;
                    });
                }
                else
                {
                    Debug.LogWarning("[LevelStateHandler] PreFailed弹窗预制体未找到，直接进入Failed阶段");
                    EventManager.GameStatus = EGameState.Failed;
                }
            }
            else
            {
                // 如果禁用了PreFailed弹窗，直接进入Failed阶段
                EventManager.GameStatus = EGameState.Failed;
            }
        }

        private protected virtual void HandleFailed(LevelManager levelManager)
        {
            // 获取当前关卡
            var level = levelManager.GetCurrentLevel();

            // 埋点上报：Failed (关卡失败时上报)
            if (BlockPuzzle.NativeBridge.NativeBridgeManager.Instance != null && level != null)
            {
                // p2只包含关卡ID
                BlockPuzzle.NativeBridge.NativeBridgeManager.Instance.SendMessageToPlatform(
                    BlockPuzzle.NativeBridge.Enums.BridgeMessageType.BuryPoint,
                    "Failed",
                    level.Number.ToString()
                );
                UnityEngine.Debug.Log($"[LevelStateHandler] 埋点上报：Failed = {level.Number}");
            }

            // 使用统一的Failed弹窗，替代各模式的独立失败弹窗
            MenuManager.Instance.ShowPopup<Failed>();
        }

        private protected virtual void HandlePreWin(LevelManager levelManager)
        {
            // 强制使用PreWin弹窗，而不是PreWinBonus或PreWinScore
            // 直接加载PreWin预制体
            var preWinPopup = Resources.Load<Popup>("Popups/PreWin");

            if (preWinPopup != null)
            {
                // 显示PreWin弹窗
                MenuManager.Instance.ShowPopupDelayed(preWinPopup);

                // 1.5秒后自动关闭弹窗并进入Win阶段
                DOVirtual.DelayedCall(1.5f, () =>
                {
                    // 关闭PreWin弹窗
                    var popup = MenuManager.Instance.GetLastPopup();
                    if (popup != null && popup.GetType().Name.StartsWith("PreWin"))
                    {
                        popup.Close();
                    }

                    // 进入Win阶段
                    EventManager.GameStatus = EGameState.Win;
                });
            }
            else
            {
                Debug.LogWarning("[LevelStateHandler] PreWin弹窗未找到，直接进入Win阶段");
                // 如果没有找到PreWin弹窗，直接进入Win阶段
                EventManager.GameStatus = EGameState.Win;
            }
        }

        private protected virtual void HandleWin(LevelManager levelManager)
        {
            // 获取当前关卡
            var level = levelManager.GetCurrentLevel();

            // 埋点上报：Level (通关一次上报1次)
            if (BlockPuzzle.NativeBridge.NativeBridgeManager.Instance != null && level != null)
            {
                BlockPuzzle.NativeBridge.NativeBridgeManager.Instance.SendMessageToPlatform(
                    BlockPuzzle.NativeBridge.Enums.BridgeMessageType.BuryPoint,
                    "Level",
                    level.Number.ToString()
                );
                Debug.Log($"[LevelStateHandler] 埋点上报：Level = {level.Number}");
            }

            // 检查是否配置了奖励弹窗
            if (level != null && level.rewardConfig != null)
            {
                // 直接显示奖励弹窗替代Win弹窗
                Debug.Log($"[LevelStateHandler] 关卡 {level.Number} 完成，显示奖励弹窗");

                if (RewardPopupManager.Instance != null)
                {
                    RewardPopupManager.Instance.ShowRewardPopupForCurrentLevel();
                }
                else
                {
                    Debug.LogError("[LevelStateHandler] RewardPopupManager未初始化");
                    // 如果奖励管理器不可用，打开地图
                    GameManager.Instance.OpenMap();
                }
            }
            else
            {
                // 如果没有配置奖励弹窗，则显示原有的Win弹窗（作为后备方案）
                var winPopup = level?.levelType?.winPopup;
                if (winPopup != null)
                {
                    Debug.Log("[LevelStateHandler] 没有配置奖励弹窗，显示默认Win弹窗");
                    MenuManager.Instance.ShowPopup(winPopup);
                }
                else
                {
                    // 没有任何弹窗配置，直接返回地图
                    GameManager.Instance.OpenMap();
                }
            }
        }
    }
} 