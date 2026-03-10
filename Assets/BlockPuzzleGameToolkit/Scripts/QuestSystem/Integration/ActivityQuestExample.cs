// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

using System;
using UnityEngine;
using UnityEngine.UI;
using QuestSystem.Core;
using QuestSystem.Config;

namespace QuestSystem.Integration
{
    /// <summary>
    /// 活动系统集成示例
    /// 展示如何在活动中使用任务系统
    /// </summary>
    public class ActivityQuestExample : MonoBehaviour
    {
        [Header("活动配置")]
        [SerializeField] private int collectQuestId = 1001;  // 只存储任务ID
        [SerializeField] private string activityTag = "spring_2025";

        [Header("UI引用")]
        [SerializeField] private GameObject activityPopup;
        [SerializeField] private BadgeIcon badgeIcon;
        [SerializeField] private Text progressText;
        [SerializeField] private Button claimButton;
        [SerializeField] private GameObject rewardEffect;

        private Quest collectQuest;

        #region 生命周期

        void Start()
        {
            CreateActivityQuest();
            SetupUI();
        }

        void OnDestroy()
        {
            // 活动结束时可选择是否移除任务
            // QuestManager.Instance.RemoveQuest(collectQuestId, activityTag);
        }

        #endregion

        #region 任务创建

        private void CreateActivityQuest()
        {
            // 通过ID创建任务实例，使用活动标签区分不同活动的相同任务
            collectQuest = QuestManager.Instance.CreateQuestFromId(
                collectQuestId,
                activityTag,      // 实例标签，用于区分
                OnProgressUpdate,  // 进度回调
                OnQuestComplete    // 完成回调
            );

            // 初始化显示
            if (collectQuest != null)
            {
                UpdateDisplay();
            }
            else
            {
                Debug.LogError($"[ActivityQuest] Failed to create quest {collectQuestId}");
            }
        }

        #endregion

        #region 回调处理

        // 进度更新时自动调用
        private void OnProgressUpdate(Quest quest, float progress)
        {
            Debug.Log($"[ActivityQuest] Progress updated: {progress:P0}");
            UpdateDisplay();
        }

        // 任务完成时自动调用
        private void OnQuestComplete(Quest quest)
        {
            Debug.Log($"[ActivityQuest] Quest completed! Reward: {quest.Data.RewardCoins} coins");
            ShowCompletionAnimation();
            UpdateDisplay();
        }

        #endregion

        #region UI更新

        private void SetupUI()
        {
            if (claimButton != null)
            {
                claimButton.onClick.AddListener(OnClaimButtonClick);
            }
        }

        private void UpdateDisplay()
        {
            if (collectQuest == null) return;

            // 更新进度文本
            if (progressText != null)
            {
                progressText.text = collectQuest.GetProgressText();
            }

            // 更新角标
            if (badgeIcon != null)
            {
                UpdateBadge(collectQuest.ProgressPercentage);
            }

            // 更新领奖按钮
            if (claimButton != null)
            {
                bool canClaim = collectQuest.IsCompleted && !collectQuest.IsRewardClaimed;
                claimButton.interactable = canClaim;
                claimButton.GetComponentInChildren<Text>().text =
                    collectQuest.IsRewardClaimed ? "已领取" :
                    collectQuest.IsCompleted ? "领取奖励" :
                    "进行中";
            }
        }

        private void UpdateBadge(float progress)
        {
            if (badgeIcon == null) return;

            if (progress >= 1f)
            {
                badgeIcon.ShowCompleted();
            }
            else if (progress > 0)
            {
                badgeIcon.ShowProgress($"{collectQuest.CurrentProgress}/{collectQuest.Data.TargetValue}");
            }
            else
            {
                badgeIcon.ShowNew();
            }
        }

        #endregion

        #region UI交互

        // 打开活动弹窗
        public void OpenActivityPopup()
        {
            if (activityPopup != null)
            {
                activityPopup.SetActive(true);
                UpdateDisplay();
            }
        }

        // 关闭活动弹窗
        public void CloseActivityPopup()
        {
            if (activityPopup != null)
            {
                activityPopup.SetActive(false);
            }
        }

        // 领奖按钮点击
        private void OnClaimButtonClick()
        {
            if (collectQuest != null && collectQuest.IsCompleted && !collectQuest.IsRewardClaimed)
            {
                bool success = QuestManager.Instance.ClaimReward(collectQuestId, activityTag);
                if (success)
                {
                    ShowRewardAnimation(collectQuest.Data.RewardCoins);
                    UpdateDisplay();
                }
            }
        }

        #endregion

        #region 动画效果

        private void ShowCompletionAnimation()
        {
            // 播放完成动画
            if (badgeIcon != null)
            {
                // 可以添加动画效果
                Debug.Log("[ActivityQuest] Playing completion animation");
            }
        }

        private void ShowRewardAnimation(int coins)
        {
            // 播放奖励动画
            if (rewardEffect != null)
            {
                rewardEffect.SetActive(true);
                Invoke(nameof(HideRewardEffect), 2f);
            }

            Debug.Log($"[ActivityQuest] Reward animation: +{coins} coins");
        }

        private void HideRewardEffect()
        {
            if (rewardEffect != null)
            {
                rewardEffect.SetActive(false);
            }
        }

        #endregion
    }

    /// <summary>
    /// 角标图标组件
    /// </summary>
    public class BadgeIcon : MonoBehaviour
    {
        [SerializeField] private GameObject newBadge;
        [SerializeField] private GameObject progressBadge;
        [SerializeField] private Text progressText;
        [SerializeField] private GameObject completedBadge;

        public void ShowNew()
        {
            SetBadgeState(true, false, false);
        }

        public void ShowProgress(string text)
        {
            SetBadgeState(false, true, false);
            if (progressText != null)
                progressText.text = text;
        }

        public void ShowCompleted()
        {
            SetBadgeState(false, false, true);
        }

        public void Hide()
        {
            SetBadgeState(false, false, false);
        }

        private void SetBadgeState(bool showNew, bool showProgress, bool showCompleted)
        {
            if (newBadge) newBadge.SetActive(showNew);
            if (progressBadge) progressBadge.SetActive(showProgress);
            if (completedBadge) completedBadge.SetActive(showCompleted);
        }
    }

    /// <summary>
    /// 游戏集成示例
    /// 展示如何在游戏中触发任务进度更新
    /// </summary>
    public class GameQuestIntegration : MonoBehaviour
    {
        // 收集物品时调用
        public void OnItemCollected(string itemId)
        {
            // 更新所有收集此物品的任务
            QuestManager.Instance.UpdateProgressByType(QuestType.Collect, 1, itemId);
        }

        // 游戏结束时调用
        public void OnGameOver(int finalScore, int linesCleared)
        {
            // 更新分数任务（取最高分）
            QuestManager.Instance.UpdateProgressByType(QuestType.Score, finalScore);

            // 更新消行任务（累加）
            if (linesCleared > 0)
            {
                QuestManager.Instance.UpdateProgressByType(QuestType.Lines, linesCleared);
            }

            // 更新游戏次数任务
            QuestManager.Instance.UpdateProgressByType(QuestType.PlayCount, 1);
        }

        // 达成连击时调用
        public void OnComboAchieved(int comboCount)
        {
            // 更新连击任务（取最高值）
            QuestManager.Instance.UpdateProgressByType(QuestType.Combo, comboCount);
        }

        // 完美通关时调用
        public void OnPerfectClear()
        {
            // 更新完美通关任务
            QuestManager.Instance.UpdateProgressByType(QuestType.Perfect, 1);
        }
    }

    /// <summary>
    /// 收集物品示例
    /// </summary>
    public class CollectibleItem : MonoBehaviour
    {
        [SerializeField] private string itemId = "red_packet";
        [SerializeField] private GameObject collectEffect;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                // 更新所有收集此物品的任务
                QuestManager.Instance.UpdateProgressByType(QuestType.Collect, 1, itemId);

                // 播放收集特效
                if (collectEffect != null)
                {
                    Instantiate(collectEffect, transform.position, Quaternion.identity);
                }

                // 销毁物品
                Destroy(gameObject);
            }
        }
    }
}