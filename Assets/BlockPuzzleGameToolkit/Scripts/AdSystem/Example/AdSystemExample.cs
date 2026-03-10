using UnityEngine;
using UnityEngine.UI;
using BlockPuzzle.AdSystem.Managers;
using BlockPuzzle.AdSystem.Models;
using System.Collections.Generic;

namespace BlockPuzzle.AdSystem.Example
{
    /// <summary>
    /// 广告系统使用示例
    /// 演示如何调用广告系统的各种功能
    /// </summary>
    public class AdSystemExample : MonoBehaviour
    {
        [Header("UI 组件")]
        [SerializeField] private Button _levelCompleteButton;
        [SerializeField] private Button _dailyRewardButton;
        [SerializeField] private Button _extraMovesButton;
        [SerializeField] private Button _doubleCoinsButton;
        [SerializeField] private Text _statusText;
        [SerializeField] private Text _logText;

        [Header("奖励设置")]
        [SerializeField] private int _baseCoins = 100;
        [SerializeField] private int _baseMoves = 5;

        private List<string> _logs = new List<string>();

        private void Start()
        {
            // 绑定按钮事件
            if (_levelCompleteButton != null)
                _levelCompleteButton.onClick.AddListener(OnLevelCompleteClicked);

            if (_dailyRewardButton != null)
                _dailyRewardButton.onClick.AddListener(OnDailyRewardClicked);

            if (_extraMovesButton != null)
                _extraMovesButton.onClick.AddListener(OnExtraMovesClicked);

            if (_doubleCoinsButton != null)
                _doubleCoinsButton.onClick.AddListener(OnDoubleCoinsClicked);

            // 订阅广告事件
            SubscribeToAdEvents();

            // 更新按钮状态
            UpdateButtonStates();

            AddLog("广告系统示例已初始化");
        }

        private void OnDestroy()
        {
            UnsubscribeFromAdEvents();
        }

        #region 按钮点击处理

        /// <summary>
        /// 关卡完成广告
        /// </summary>
        private void OnLevelCompleteClicked()
        {
            AddLog("请求播放关卡完成广告...");
            UpdateStatus("正在播放广告...");

            AdSystemManager.Instance.PlayAd(AdEntryNames.LEVEL_COMPLETE, (success) =>
            {
                if (success)
                {
                    int reward = _baseCoins;
                    AddLog($"广告播放成功！获得 {reward} 金币");
                    UpdateStatus($"获得 {reward} 金币！");

                    // 实际游戏中这里应该调用增加金币的方法
                    // GameManager.Instance.AddCoins(reward);
                }
                else
                {
                    AddLog("广告播放失败或被跳过");
                    UpdateStatus("广告播放失败");
                }

                UpdateButtonStates();
            });
        }

        /// <summary>
        /// 每日任务奖励广告
        /// </summary>
        private void OnDailyRewardClicked()
        {
            AddLog("请求播放每日任务奖励广告...");
            UpdateStatus("正在播放广告...");

            AdSystemManager.Instance.PlayAd(AdEntryNames.DAILY_TASK_REWARD, (success) =>
            {
                if (success)
                {
                    AddLog($"广告播放成功！获得每日奖励");
                    UpdateStatus($"获得每日奖励！");
                }
                else
                {
                    AddLog("广告播放失败");
                    UpdateStatus("广告播放失败");
                }

                UpdateButtonStates();
            });
        }

        /// <summary>
        /// 额外步数广告
        /// </summary>
        private void OnExtraMovesClicked()
        {
            AddLog("请求播放额外步数广告...");
            UpdateStatus("正在播放广告...");

            AdSystemManager.Instance.PlayAd(AdEntryNames.EXTRA_MOVES, (success) =>
            {
                if (success)
                {
                    int extraMoves = _baseMoves;
                    AddLog($"广告播放成功！获得 {extraMoves} 额外步数");
                    UpdateStatus($"获得 {extraMoves} 步！");
                }
                else
                {
                    AddLog("广告播放失败");
                    UpdateStatus("广告播放失败");
                }

                UpdateButtonStates();
            });
        }

        /// <summary>
        /// 双倍金币广告
        /// </summary>
        private void OnDoubleCoinsClicked()
        {
            AddLog("请求播放双倍金币广告...");
            UpdateStatus("正在播放广告...");

            AdSystemManager.Instance.PlayAd(AdEntryNames.DOUBLE_COINS, (success) =>
            {
                if (success)
                {
                    int reward = _baseCoins * 2;  // 固定双倍
                    AddLog($"广告播放成功！获得 {reward} 金币");
                    UpdateStatus($"获得 {reward} 金币！");
                }
                else
                {
                    AddLog("广告播放失败");
                    UpdateStatus("广告播放失败");
                }

                UpdateButtonStates();
            });
        }

        #endregion

        #region 事件订阅

        private void SubscribeToAdEvents()
        {
            AdSystemManager.OnAdStartPlaying += OnAdStartPlaying;
            AdSystemManager.OnAdPlaySuccess += OnAdPlaySuccess;
            AdSystemManager.OnAdPlayFailed += OnAdPlayFailed;
            AdSystemManager.OnAdPlayComplete += OnAdPlayComplete;
        }

        private void UnsubscribeFromAdEvents()
        {
            AdSystemManager.OnAdStartPlaying -= OnAdStartPlaying;
            AdSystemManager.OnAdPlaySuccess -= OnAdPlaySuccess;
            AdSystemManager.OnAdPlayFailed -= OnAdPlayFailed;
            AdSystemManager.OnAdPlayComplete -= OnAdPlayComplete;
        }

        private void OnAdStartPlaying(string entryName)
        {
            AddLog($"[事件] 广告开始播放: {entryName}");
        }

        private void OnAdPlaySuccess(string entryName)
        {
            AddLog($"[事件] 广告播放成功: {entryName}");
        }

        private void OnAdPlayFailed(string entryName, string error)
        {
            AddLog($"[事件] 广告播放失败: {entryName}, 错误: {error}");
        }

        private void OnAdPlayComplete(AdPlayResult result)
        {
            AddLog($"[事件] 广告播放完成: {result.entryName}, 成功: {result.success}");
        }

        #endregion

        #region UI更新

        private void UpdateButtonStates()
        {
            // 检查各个广告是否准备好
            if (_levelCompleteButton != null)
            {
                bool ready = AdSystemManager.Instance.IsAdReady(AdEntryNames.LEVEL_COMPLETE);
                _levelCompleteButton.interactable = ready;
                UpdateButtonText(_levelCompleteButton, "关卡完成奖励", ready);
            }

            if (_dailyRewardButton != null)
            {
                bool ready = AdSystemManager.Instance.IsAdReady(AdEntryNames.DAILY_TASK_REWARD);
                _dailyRewardButton.interactable = ready;
                UpdateButtonText(_dailyRewardButton, "每日任务奖励", ready);
            }

            if (_extraMovesButton != null)
            {
                bool ready = AdSystemManager.Instance.IsAdReady(AdEntryNames.EXTRA_MOVES);
                _extraMovesButton.interactable = ready;
                UpdateButtonText(_extraMovesButton, "额外步数", ready);
            }

            if (_doubleCoinsButton != null)
            {
                bool ready = AdSystemManager.Instance.IsAdReady(AdEntryNames.DOUBLE_COINS);
                _doubleCoinsButton.interactable = ready;
                UpdateButtonText(_doubleCoinsButton, "双倍金币", ready);
            }
        }

        private void UpdateButtonText(Button button, string baseText, bool ready)
        {
            var text = button.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.text = ready ? baseText : $"{baseText} (未就绪)";
            }
        }

        private void UpdateStatus(string status)
        {
            if (_statusText != null)
            {
                _statusText.text = status;
            }
        }

        private void AddLog(string message)
        {
            _logs.Add($"[{System.DateTime.Now:HH:mm:ss}] {message}");

            // 保持日志数量
            if (_logs.Count > 10)
            {
                _logs.RemoveAt(0);
            }

            // 更新显示
            if (_logText != null)
            {
                _logText.text = string.Join("\n", _logs);
            }

            Debug.Log($"[AdSystemExample] {message}");
        }

        #endregion

        #region 测试方法

#if UNITY_EDITOR
        [ContextMenu("列出所有广告入口")]
        private void ListAllAdEntries()
        {
            var entries = AdSystemManager.Instance.GetActiveEntries();
            Debug.Log($"=== 活跃的广告入口 ({entries.Count}) ===");

            foreach (var entry in entries)
            {
                Debug.Log($"名称: {entry.Name}");
                Debug.Log($"  类型: {entry.Type}");
                Debug.Log($"  描述: {entry.Description}");
                Debug.Log($"  就绪: {AdSystemManager.Instance.IsAdReady(entry.Name)}");
                Debug.Log("---");
            }
        }

        [ContextMenu("测试所有广告")]
        private void TestAllAds()
        {
            var entries = AdSystemManager.Instance.GetActiveEntries();

            foreach (var entry in entries)
            {
                Debug.Log($"测试广告: {entry.Name}");

                AdSystemManager.Instance.PlayAd(entry.Name, (success) =>
                {
                    Debug.Log($"  结果 - 成功: {success}");
                });
            }
        }
#endif

        #endregion
    }
}