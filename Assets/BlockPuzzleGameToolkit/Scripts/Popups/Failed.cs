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

using BlockPuzzleGameToolkit.Scripts.GUI;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using BlockPuzzleGameToolkit.Scripts.Gameplay;
using BlockPuzzleGameToolkit.Scripts.Gameplay.Managers;
using BlockPuzzleGameToolkit.Scripts.Settings;
using BlockPuzzleGameToolkit.Scripts.Localization;
using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzle.AdSystem.Managers;
using BlockPuzzle.NativeBridge;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using BlockPuzzleGameToolkit.Scripts.Audio;

namespace BlockPuzzleGameToolkit.Scripts.Popups
{
    /// <summary>
    /// 统一的关卡失败弹窗
    /// 显示进度，提供免费复活功能
    /// </summary>
    public class Failed : Popup
    {
        [Header("按钮组件")]
        public Button retryButton;        // 重新开始按钮 (btn_replay)
        public Button continueButton;     // 复活按钮 (btn_continue)

        [Header("文本组件")]
        public TextMeshProUGUI tmp_percent;     // 进度百分比文本
        public TextMeshProUGUI tmp_continue;    // 复活按钮文本

        [Header("进度显示")]
        public Image img_progress;              // 进度条图片

        [Header("广告配置")]
        [SerializeField]
        [Tooltip("复活广告入口名称，在Inspector中配置")]
        private string reviveAdEntryName = "";  // 复活广告入口名称

        private LevelManager levelManager;
        private FailedPopupSettings settings;
        private bool isProcessingRevive = false;  // 防止重复点击复活按钮
        private bool isProcessingRetry = false;   // 防止重复点击重试按钮

        // 缓存 WaitForSeconds 以提升性能
        private readonly WaitForSeconds waitReviveDelay = new(0.3f);
        private readonly WaitForSeconds waitAdExitDelay = new(0.2f);  // 广告退出延迟

        protected virtual void OnEnable()
        {
            // 获取管理器和配置
            levelManager = FindObjectOfType<LevelManager>();
            settings = FailedPopupSettings.Instance;

            // 绑定按钮事件
            if (retryButton != null)
                retryButton.onClick.AddListener(Retry);

            if (continueButton != null)
                continueButton.onClick.AddListener(OnContinueClick);

            if (closeButton != null)
                closeButton.onClick.AddListener(() => GameManager.Instance.MainMenu());

            // 初始化UI显示
            InitializeUI();
        }

        protected virtual void OnDisable()
        {
            // 清理事件监听
            if (retryButton != null)
                retryButton.onClick.RemoveAllListeners();

            if (continueButton != null)
                continueButton.onClick.RemoveAllListeners();

            if (closeButton != null)
                closeButton.onClick.RemoveAllListeners();

            // 重置防重复点击标志
            isProcessingRevive = false;
            isProcessingRetry = false;
        }

        /// <summary>
        /// 初始化UI显示
        /// </summary>
        private void InitializeUI()
        {
            // 计算并显示进度
            float progress = CalculateProgress();
            Debug.Log($"[Failed] 计算到的关卡进度: {progress}%");
            UpdateProgressDisplay(progress);

            if (tmp_continue != null)
                tmp_continue.text = LocalizationManager.GetText("Failed_FreeRevive", "Fee Revive");

            // 根据配置显示/隐藏复活按钮
            if (continueButton != null && settings != null)
            {
                continueButton.gameObject.SetActive(settings.allowFreeRevive);
            }
        }

        /// <summary>
        /// 计算当前关卡进度
        /// </summary>
        private float CalculateProgress()
        {
            if (levelManager == null)
                return 0f;

            // 调用LevelManager的进度获取方法
            return levelManager.GetCurrentProgress();
        }

        /// <summary>
        /// 更新进度显示
        /// </summary>
        private void UpdateProgressDisplay(float progress)
        {
            // 更新进度百分比文本（保留一位小数）
            if (tmp_percent != null)
            {
                tmp_percent.text = $"{progress:F1}%";
            }

            // 更新进度条
            if (img_progress != null)
            {
                img_progress.fillAmount = progress / 100f;
                Material material = img_progress.material;
                if (material != null)
                {
                    material.SetFloat("_Progress", progress / 100f);
                }
            }
        }

        public override void ShowAnimationSound()
        {
            // 播放胜利音效
            if (SoundBase.Instance != null)
            {
                // 优先播放win音效
                if (SoundBase.Instance.win != null)
                {
                    SoundBase.Instance.PlaySound(SoundBase.Instance.fail);
                }
                // 如果没有win音效，回退到基类的默认音效
                else
                {
                    base.ShowAnimationSound();
                }
            }
        }

        /// <summary>
        /// 重新开始游戏
        /// </summary>
        private void Retry()
        {
            // 防止重复点击
            if (isProcessingRetry)
            {
                Debug.Log("[Failed] 正在处理重试，忽略重复点击");
                return;
            }

            Debug.Log("[Failed] 点击重试按钮");
            isProcessingRetry = true;

            var replayAdManager = ReplayAdManager.Instance;
            if (replayAdManager != null)
            {
                replayAdManager.OnReplayClick((playedAd) =>
                {
                    // 检查对象是否仍然存在（防止广告播放期间弹窗被销毁）
                    if (this == null)
                    {
                        Debug.LogWarning("[Failed] Retry广告回调时弹窗对象已销毁，忽略重复回调");
                        return;
                    }

                    // 触发关卡失败事件（用于难度系统）
                    EventManager.GetEvent(EGameEvent.LevelFailed).Invoke();
                    Debug.Log("[Failed] 已触发LevelFailed事件（Retry算作一次失败）");

                    if (playedAd)
                    {
                        Debug.Log("[Failed] 已播放Replay广告");
                    }

                    // 禁用交互并关闭弹窗
                    StopInteration();
                    Close();

                    // 延迟重启关卡，确保弹窗完全关闭和场景状态稳定
                    StartCoroutine(DelayedRestartLevel());
                });
            }
            else
            {
                Debug.LogWarning("[Failed] ReplayAdManager未找到，直接重新开始");

                // 触发关卡失败事件（用于难度系统）
                EventManager.GetEvent(EGameEvent.LevelFailed).Invoke();
                Debug.Log("[Failed] 已触发LevelFailed事件（Retry算作一次失败）");

                // 禁用交互并关闭弹窗
                StopInteration();
                Close();

                // 延迟重启关卡，确保弹窗完全关闭和场景状态稳定
                StartCoroutine(DelayedRestartLevel());
            }
        }

        /// <summary>
        /// 延迟重启关卡
        /// 确保弹窗完全关闭、广告完全退出后再重启，避免状态不一致
        /// </summary>
        private IEnumerator DelayedRestartLevel()
        {
            // 等待0.5秒，确保：
            // 1. 弹窗完全关闭
            // 2. 广告完全退出（手机上可能需要时间）
            // 3. 场景状态稳定
            yield return new WaitForSeconds(0.5f);

            Debug.Log("[Failed] 延迟重启关卡");
            GameManager.Instance.RestartLevel();
        }

        /// <summary>
        /// 点击复活按钮
        /// </summary>
        private void OnContinueClick()
        {
            // 防止重复点击
            if (isProcessingRevive)
            {
                Debug.Log("[Failed] 正在处理复活，忽略重复点击");
                return;
            }

            Debug.Log("[Failed] 点击复活按钮");
            isProcessingRevive = true;

            // 显示广告
            ShowReviveAd();
        }

        /// <summary>
        /// 显示复活广告
        /// </summary>
        private void ShowReviveAd()
        {
            // 禁用按钮防止重复点击
            if (continueButton != null)
                continueButton.interactable = false;

            // 根据配置决定是否需要真正显示广告
            if (settings != null && settings.debugFreeRevive && Application.isEditor)
            {
                Debug.Log("[Failed] 调试模式：直接执行复活");
                // 调试模式下直接执行复活
                StartCoroutine(RevivePlayer());
                return;
            }

            // 使用ADSystemManager播放广告
            if (!string.IsNullOrEmpty(reviveAdEntryName) && AdSystemManager.Instance != null)
            {
                Debug.Log($"[Failed] 播放复活广告：{reviveAdEntryName}");

                AdSystemManager.Instance.PlayAd(reviveAdEntryName, (success) =>
                {
                    // ⚠️ 防止广告回调被重复调用（SDK可能触发多次回调）
                    // 必须在最开始检查，因为第二次调用时 this 可能已经是 null
                    if (this == null)
                    {
                        Debug.LogWarning("[Failed] 广告回调时弹窗对象已销毁，忽略重复回调");
                        return;
                    }

                    // 检查是否已经处理过（防止重复回调）
                    // 第一次回调时 isProcessingRevive=true，执行后设为false
                    // 第二次回调时 isProcessingRevive=false，直接返回
                    if (!isProcessingRevive)
                    {
                        Debug.LogWarning("[Failed] 广告回调重复调用，已处理过，忽略");
                        return;
                    }

                    // 立即标记为已处理，防止重复执行
                    isProcessingRevive = false;

                    if (success)
                    {
                        Debug.Log("[Failed] 广告播放成功，执行复活");
                    }
                    else
                    {
                        Debug.Log("[Failed] 广告播放失败，但仍然执行复活（不卡关）");
                    }

                    // 延迟0.2秒后执行复活，确保广告完全退出
                    StartCoroutine(DelayedRevivePlayer());
                });
            }
            else
            {
                // 如果没有配置广告入口或ADSystemManager不可用，直接执行复活
                Debug.LogWarning($"[Failed] 广告入口未配置或ADSystemManager不可用，直接执行复活。" +
                    $"reviveAdEntryName={reviveAdEntryName}, AdSystemManager={AdSystemManager.Instance}");
                StartCoroutine(RevivePlayer());
            }
        }

        /// <summary>
        /// 延迟执行复活（广告播放后）
        /// 等待0.2秒确保广告完全退出，提供平滑的过渡
        /// </summary>
        private IEnumerator DelayedRevivePlayer()
        {
            Debug.Log("[Failed] 广告播放完毕，等待0.2秒后执行复活");

            // 等待0.2秒，确保广告完全退出
            yield return waitAdExitDelay;

            // 执行复活逻辑
            yield return StartCoroutine(RevivePlayer());
        }

        /// <summary>
        /// 执行复活逻辑
        /// </summary>
        private IEnumerator RevivePlayer()
        {
            Debug.Log("[Failed] 开始执行复活逻辑");

            // 禁用所有交互（防止用户在关闭动画期间点击按钮）
            StopInteration();

            // 关闭弹窗
            Close();

            // 等待弹窗关闭动画
            yield return waitReviveDelay;

            // 先尝试使用ReviveManager执行复活（包含埋点上报）
            if (ReviveManager.Instance != null)
            {
                Debug.Log("[Failed] 调用ReviveManager.ExecuteRevive()");
                bool success = ReviveManager.Instance.ExecuteRevive();

                if (success)
                {
                    Debug.Log("[Failed] ReviveManager复活成功");
                }
                else
                {
                    Debug.LogWarning("[Failed] ReviveManager复活失败，尝试直接执行复活逻辑并上报埋点");

                    // 如果ReviveManager因为配置限制无法复活，我们仍然执行复活并上报埋点
                    // 直接上报Replay埋点
                    if (BlockPuzzle.NativeBridge.NativeBridgeManager.Instance != null)
                    {
                        int levelNumber = GameDataManager.LevelNum;
                        Debug.Log($"[Failed] 直接上报Replay埋点 - 关卡ID: {levelNumber}");
                        BlockPuzzle.NativeBridge.NativeBridgeManager.Instance.SendMessageToPlatform(
                            BlockPuzzle.NativeBridge.Enums.BridgeMessageType.BuryPoint,
                            "Replay",
                            levelNumber.ToString()
                        );
                    }

                    // 执行基本的复活逻辑
                    if (levelManager != null)
                    {
                        levelManager.ClearEmptyCells();

                        var cellDeckManager = FindObjectOfType<CellDeckManager>();
                        if (cellDeckManager != null && settings != null)
                        {
                            cellDeckManager.RefreshShapesForRevive(settings.refreshShapeCount, settings.guaranteePlaceableShape);
                        }

                        EventManager.GameStatus = EGameState.Playing;
                        Debug.Log("[Failed] 直接复活成功");
                    }
                }
            }
            else
            {
                Debug.LogError("[Failed] ReviveManager未找到，直接执行复活逻辑并上报埋点");

                // 直接上报Replay埋点
                if (BlockPuzzle.NativeBridge.NativeBridgeManager.Instance != null)
                {
                    int levelNumber = GameDataManager.LevelNum;
                    Debug.Log($"[Failed] 直接上报Replay埋点 - 关卡ID: {levelNumber}");
                    BlockPuzzle.NativeBridge.NativeBridgeManager.Instance.SendMessageToPlatform(
                        BlockPuzzle.NativeBridge.Enums.BridgeMessageType.BuryPoint,
                        "Replay",
                        levelNumber.ToString()
                    );
                }

                // 执行基本的复活逻辑
                if (levelManager != null)
                {
                    levelManager.ClearEmptyCells();

                    var cellDeckManager = FindObjectOfType<CellDeckManager>();
                    if (cellDeckManager != null && settings != null)
                    {
                        cellDeckManager.RefreshShapesForRevive(settings.refreshShapeCount, settings.guaranteePlaceableShape);
                    }

                    EventManager.GameStatus = EGameState.Playing;
                    Debug.Log("[Failed] 直接复活成功");
                }
            }
        }
    }
}