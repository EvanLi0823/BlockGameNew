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
using BlockPuzzleGameToolkit.Scripts.Audio;
using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Gameplay.Managers
{
    /// <summary>
    /// TimerManager - 计时器管理器
    /// 负责管理游戏倒计时功能，用于计时模式或限时关卡
    /// 包含倒计时显示、警告效果、暂停恢复等功能
    /// </summary>
    public class TimerManager : MonoBehaviour
    {
        // ========== UI组件 ==========
        /// <summary>
        /// 计时器文本显示组件
        /// </summary>
        [SerializeField] private TextMeshProUGUI timerText;

        /// <summary>
        /// 计时器面板GameObject
        /// </summary>
        [SerializeField] private GameObject timerPanel;

        /// <summary>
        /// 计时器正常状态的颜色（青色）
        /// </summary>
        [SerializeField] private Color timerColor = new Color(0.612f, 0.988f, 1f, 1f);

        // ========== 计时器状态 ==========
        /// <summary>
        /// 剩余时间（秒）
        /// </summary>
        private float remainingTime;

        /// <summary>
        /// 计时器是否激活
        /// </summary>
        private bool isTimerActive;

        /// <summary>
        /// 计时器是否暂停
        /// </summary>
        private bool isTimerPaused;

        /// <summary>
        /// 警告效果是否激活（倒计时最后5秒）
        /// </summary>
        private bool isWarningActive;

        /// <summary>
        /// 文本原始颜色缓存
        /// </summary>
        private Color originalTextColor;

        /// <summary>
        /// 警告动画序列（跳动效果）
        /// </summary>
        private Sequence bounceSequence;

        /// <summary>
        /// 计时器到期事件
        /// </summary>
        public Action OnTimerExpired;

        /// <summary>
        /// 是否等待教学完成
        /// 教学模式下计时器会延迟启动
        /// </summary>
        private bool waitingForTutorial = false;

        /// <summary>
        /// 初始持续时间（用于教学后重新初始化）
        /// </summary>
        private float initialDuration;

        /// <summary>
        /// 获取剩余时间（整数秒）
        /// </summary>
        public int RemainingTime => Mathf.FloorToInt(remainingTime);
        
        /// <summary>
        /// Unity生命周期 - 启用时订阅事件
        /// </summary>
        private void OnEnable()
        {
            EventManager.OnGameStateChanged += HandleGameStateChange;
            EventManager.GetEvent(EGameEvent.TutorialCompleted).Subscribe(OnTutorialCompleted);
        }

        /// <summary>
        /// Unity生命周期 - 禁用时取消订阅
        /// </summary>
        private void OnDisable()
        {
            EventManager.OnGameStateChanged -= HandleGameStateChange;
            EventManager.GetEvent(EGameEvent.TutorialCompleted).Unsubscribe(OnTutorialCompleted);
        }

        /// <summary>
        /// 处理游戏状态变化
        /// 暂停或恢复计时器
        /// </summary>
        /// <param name="state">新的游戏状态</param>
        private void HandleGameStateChange(EGameState state)
        {
            if (state == EGameState.Paused)
            {
                PauseTimer(true);
            }
            else if (state == EGameState.Playing)
            {
                PauseTimer(false);
            }
        }

        /// <summary>
        /// 初始化计时器
        /// 设置倒计时时长并启动
        /// </summary>
        /// <param name="duration">倒计时时长（秒）</param>
        public void InitializeTimer(float duration)
        {
            // 教学模式下延迟启动
            if (GameManager.Instance.IsTutorialMode())
            {
                waitingForTutorial = true;
                if (timerPanel != null)
                {
                    timerPanel.SetActive(false);
                }
                return;
            }

            // 初始化计时器状态
            initialDuration = duration;
            remainingTime = duration;
            isTimerActive = true;
            isTimerPaused = false;
            enabled = true;

            // 显示计时器UI
            if (timerPanel != null)
            {
                timerPanel.SetActive(true);
                if (timerText != null)
                {
                    timerText.color = timerColor;
                    originalTextColor = timerColor;
                }
                UpdateTimerDisplay();
            }
        }

        /// <summary>
        /// 暂停或恢复计时器
        /// </summary>
        /// <param name="pause">true为暂停，false为恢复</param>
        public void PauseTimer(bool pause)
        {
            isTimerPaused = pause;
        }

        /// <summary>
        /// 更新计时器显示
        /// 格式为 MM:SS
        /// </summary>
        private void UpdateTimerDisplay()
        {
            if (timerText != null)
            {
                float timeToDisplay = Mathf.Max(0, remainingTime);
                int minutes = Mathf.FloorToInt(timeToDisplay / 60);
                int seconds = Mathf.FloorToInt(timeToDisplay % 60);
                timerText.text = $"{minutes:00}:{seconds:00}";

                // 最后5秒启动警告效果
                if (timeToDisplay <= 5f && !isWarningActive && isTimerActive)
                {
                    StartWarningEffect();
                }
                else if (timeToDisplay > 5f && isWarningActive)
                {
                    StopWarningEffect();
                }
            }
        }
        
        /// <summary>
        /// Unity生命周期 - 初始化
        /// </summary>
        private void Start()
        {
            if (timerText != null)
            {
                originalTextColor = timerText.color;
            }
        }

        /// <summary>
        /// 启动警告效果
        /// 最后5秒时文本变红并跳动
        /// </summary>
        private void StartWarningEffect()
        {
            if (timerText == null) return;

            // 播放警告音效
            SoundBase.Instance.PlaySound(SoundBase.Instance.alert);
            isWarningActive = true;
            originalTextColor = timerText.color;

            // 清除旧动画
            if (bounceSequence != null)
            {
                bounceSequence.Kill();
                bounceSequence = null;
            }

            // 创建跳动动画序列
            bounceSequence = DOTween.Sequence();
            bounceSequence.Append(timerText.transform.DOScale(1.2f, 0.3f));  // 放大
            bounceSequence.Append(timerText.transform.DOScale(1f, 0.3f));    // 缩小
            timerText.color = Color.red;  // 变红色
            bounceSequence.SetLoops(-1);  // 无限循环
        }

        /// <summary>
        /// 停止警告效果
        /// 恢复正常显示状态
        /// </summary>
        private void StopWarningEffect()
        {
            if (timerText == null) return;

            isWarningActive = false;

            // 清除动画
            if (bounceSequence != null)
            {
                bounceSequence.Kill();
                bounceSequence = null;
            }

            // 恢复原始状态
            timerText.transform.localScale = Vector3.one;
            timerText.color = originalTextColor;
        }

        /// <summary>
        /// Unity生命周期 - 销毁时清理
        /// </summary>
        private void OnDestroy()
        {
            if (bounceSequence != null)
            {
                bounceSequence.Kill();
                bounceSequence = null;
            }
        }

        /// <summary>
        /// 停止计时器
        /// 完全停止并隐藏计时器
        /// </summary>
        public void StopTimer()
        {
            isTimerActive = false;
            enabled = false;
            StopWarningEffect();
            if (timerPanel != null)
            {
                timerPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Unity生命周期 - 每帧更新
        /// 倒计时逻辑和到期检测
        /// </summary>
        private void Update()
        {
            // 只在游戏进行中且计时器激活时倒计时
            if (isTimerActive && !isTimerPaused && EventManager.GameStatus == EGameState.Playing)
            {
                remainingTime -= Time.deltaTime;
                UpdateTimerDisplay();

                // 时间到达0时触发事件
                if (RemainingTime <= 0)
                {
                    isTimerActive = false;
                    EventManager.GetEvent(EGameEvent.TimerExpired).Invoke();
                    OnTimerExpired?.Invoke();
                    StopTimer();
                }
            }
        }

        /// <summary>
        /// 教学完成后的处理
        /// 教学模式结束后启动之前延迟的计时器
        /// </summary>
        private void OnTutorialCompleted()
        {
            if (waitingForTutorial)
            {
                waitingForTutorial = false;
                InitializeTimer(initialDuration);
            }
        }
    }
}