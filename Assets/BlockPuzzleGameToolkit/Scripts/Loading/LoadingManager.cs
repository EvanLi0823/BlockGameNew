// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

using System;
using UnityEngine;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using BlockPuzzleGameToolkit.Scripts.Popups;
using BlockPuzzleGameToolkit.Scripts.Gameplay;
using BlockPuzzleGameToolkit.Scripts.Gameplay.Managers;

namespace BlockPuzzleGameToolkit.Scripts.Loading
{
    /// <summary>
    /// Loading管理器（单例）
    /// 负责管理假的Loading界面显示
    /// </summary>
    public class LoadingManager : SingletonBehaviour<LoadingManager>
    {
        [Header("Loading面板")]
        [SerializeField] private LoadingPanel loadingPanel;

        [Header("默认设置")]
        [SerializeField] private float defaultLoadingDuration = 3f;  // 默认Loading持续时间

        #region 属性

        /// <summary>
        /// 是否正在显示Loading
        /// </summary>
        public bool IsLoading => loadingPanel != null && loadingPanel.gameObject.activeSelf;

        #endregion

        #region 生命周期

        public override void Awake()
        {
            base.Awake();

            // 查找或创建LoadingPanel
            if (loadingPanel == null)
            {
                loadingPanel = GetComponentInChildren<LoadingPanel>();

                if (loadingPanel == null)
                {
                    Debug.LogWarning("[LoadingManager] No LoadingPanel found in children");
                }
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 显示Loading界面
        /// </summary>
        /// <param name="duration">持续时间（秒）</param>
        /// <param name="onComplete">完成回调</param>
        /// <param name="switchToLoadingState">是否切换到Loading状态</param>
        public void ShowLoading(float duration = 0, Action onComplete = null, bool switchToLoadingState = false)
        {
            if (loadingPanel == null)
            {
                Debug.LogError("[LoadingManager] LoadingPanel not found!");
                onComplete?.Invoke();
                return;
            }

            // 如果需要，切换到Loading状态
            if (switchToLoadingState && StateManager.Instance != null)
            {
                StateManager.Instance.CurrentState = EScreenStates.Loading;
            }

            // 使用默认或指定的持续时间
            float actualDuration = duration > 0 ? duration : defaultLoadingDuration;

            // 显示Loading
            loadingPanel.ShowLoading(actualDuration, onComplete);
        }

        /// <summary>
        /// 隐藏Loading界面
        /// </summary>
        public void HideLoading()
        {
            if (loadingPanel != null)
            {
                loadingPanel.HideLoading();
            }
        }

        /// <summary>
        /// 停止Loading
        /// </summary>
        public void StopLoading()
        {
            if (loadingPanel != null)
            {
                loadingPanel.StopLoading();
            }
        }

        /// <summary>
        /// 显示Loading界面并在完成后跳转到游戏场景
        /// </summary>
        /// <param name="duration">持续时间（秒）</param>
        public void ShowLoadingAndGoToGame(float duration = 0)
        {
            if (loadingPanel == null)
            {
                Debug.LogError("[LoadingManager] LoadingPanel not found!");
                return;
            }

            // 使用默认或指定的持续时间
            float actualDuration = duration > 0 ? duration : defaultLoadingDuration;

            // 如果当前不是Loading状态，切换到Loading状态
            if (StateManager.Instance != null && StateManager.Instance.CurrentState != EScreenStates.Loading)
            {
                StateManager.Instance.CurrentState = EScreenStates.Loading;
            }

            // 显示Loading，完成后判断是否需要显示引导
            loadingPanel.ShowLoading(actualDuration, () =>
            {
                // 检查是否需要显示引导
                if (!IsTutorialShown() && !GameDataManager.isTestPlay)
                {
                    // 需要显示引导
                    Debug.Log("[LoadingManager] First time player detected, starting tutorial");
                    GameManager.Instance.SetTutorialMode(true);

                    // 设置为第一关
                    GameDataManager.SetLevelNum(1);

                    // 切换到游戏状态并启动引导
                    if (StateManager.Instance != null)
                    {
                        StateManager.Instance.CurrentState = EScreenStates.Game;
                    }

                    // 跳转到游戏场景（第一关）
                    if (SceneLoader.Instance != null)
                    {
                        SceneLoader.Instance.StartGameScene(1); // 明确指定第一关
                    }

                    // 触发游戏场景加载完成事件
                    EventManager.GetEvent(BlockPuzzleGameToolkit.Scripts.Enums.EGameEvent.GameSceneReady).Invoke();
                    Debug.Log("[LoadingManager] 触发GameSceneReady事件");

                    // 注意：引导将在LevelManager中启动
                }
                else
                {
                    // 不需要引导，直接进入当前关卡
                    Debug.Log("[LoadingManager] Tutorial already completed, starting current level");

                    // 跳转到当前关卡场景
                    if (SceneLoader.Instance != null)
                    {
                        // 使用当前关卡号启动游戏场景
                        SceneLoader.Instance.StartGameScene();
                        Debug.Log("[LoadingManager] Loading complete, switching to Game scene");
                    }
                    else if (StateManager.Instance != null)
                    {
                        StateManager.Instance.CurrentState = EScreenStates.Game;
                        Debug.Log("[LoadingManager] Loading complete, switching to Game state");
                    }
                    else
                    {
                        Debug.LogError("[LoadingManager] No SceneLoader or StateManager found!");
                    }

                    // 触发游戏场景加载完成事件
                    EventManager.GetEvent(BlockPuzzleGameToolkit.Scripts.Enums.EGameEvent.GameSceneReady).Invoke();
                    Debug.Log("[LoadingManager] 触发GameSceneReady事件");
                }
            });
        }

        /// <summary>
        /// 检查引导是否已经显示过
        /// </summary>
        private bool IsTutorialShown()
        {
            return PlayerPrefs.GetInt("tutorial", 0) == 1;
        }

        /// <summary>
        /// 显示Loading界面并在完成后跳转到Map场景（保留此方法以兼容）
        /// </summary>
        /// <param name="duration">持续时间（秒）</param>
        public void ShowLoadingAndGoToMap(float duration = 0)
        {
            if (loadingPanel == null)
            {
                Debug.LogError("[LoadingManager] LoadingPanel not found!");
                return;
            }

            // 使用默认或指定的持续时间
            float actualDuration = duration > 0 ? duration : defaultLoadingDuration;

            // 如果当前不是Loading状态，切换到Loading状态
            if (StateManager.Instance != null && StateManager.Instance.CurrentState != EScreenStates.Loading)
            {
                StateManager.Instance.CurrentState = EScreenStates.Loading;
            }

            // 显示Loading，完成后跳转到Map场景
            loadingPanel.ShowLoading(actualDuration, () =>
            {
                // 跳转到Map场景
                if (SceneLoader.Instance != null)
                {
                    SceneLoader.Instance.StartMapScene();
                    Debug.Log("[LoadingManager] Loading complete, switching to Map scene");
                }
                else if (StateManager.Instance != null)
                {
                    StateManager.Instance.CurrentState = EScreenStates.Map;
                    Debug.Log("[LoadingManager] Loading complete, switching to Map state");
                }
                else
                {
                    Debug.LogError("[LoadingManager] No SceneLoader or StateManager found!");
                }
            });
        }

        #endregion

        #region 静态快捷方法

        /// <summary>
        /// 快速显示Loading
        /// </summary>
        /// <param name="duration">持续时间</param>
        public static void Show(float duration = 3f)
        {
            if (Instance != null)
            {
                Instance.ShowLoading(duration);
            }
            else
            {
                Debug.LogError("[LoadingManager] Instance not found");
            }
        }

        /// <summary>
        /// 快速隐藏Loading
        /// </summary>
        public static void Hide()
        {
            if (Instance != null)
            {
                Instance.HideLoading();
            }
        }

        #endregion
    }
}