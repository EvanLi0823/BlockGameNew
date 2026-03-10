using System;
using UnityEngine;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using BlockPuzzleGameToolkit.Scripts.Settings;
using StorageSystem.Core;
using BlockPuzzle.AdSystem.Constants;
using BlockPuzzle.AdSystem.Models;
using BlockPuzzle.NativeBridge;
using BlockPuzzle.NativeBridge.Enums;

namespace BlockPuzzle.AdSystem.Managers
{
    /// <summary>
    /// Replay广告管理器
    /// 负责管理关卡失败时Replay按钮的广告播放逻辑
    /// </summary>
    public class ReplayAdManager : SingletonBehaviour<ReplayAdManager>
    {
        #region 私有字段

        /// <summary>
        /// 配置缓存
        /// </summary>
        private FailedPopupSettings settings;

        /// <summary>
        /// 当前计数器数据（缓存）
        /// </summary>
        private ReplayAdCounterData counterData;

        /// <summary>
        /// 是否已加载数据
        /// </summary>
        private bool isDataLoaded = false;

        #endregion

        #region 生命周期

        public override void Awake()
        {
            base.Awake();
            settings = FailedPopupSettings.Instance;
            LoadCounterData();
        }

        #endregion

        #region 公开方法

        /// <summary>
        /// 当用户点击Replay按钮时调用
        /// </summary>
        /// <param name="onComplete">完成回调，参数为是否播放了广告</param>
        public void OnReplayClick(Action<bool> onComplete)
        {
            try
            {
                // 检查配置是否可用
                if (settings == null)
                {
                    Debug.LogWarning("[ReplayAdManager] FailedPopupSettings未找到，跳过广告逻辑");
                    onComplete?.Invoke(false);
                    return;
                }

                // 检查是否启用Replay广告
                if (!settings.enableReplayAd)
                {
                    if (settings.debugReplayAd)
                        Debug.Log("[ReplayAdManager] Replay广告功能已禁用");
                    onComplete?.Invoke(false);
                    return;
                }

                // 确保数据已加载
                if (!isDataLoaded)
                {
                    LoadCounterData();
                }

                // 递增计数器
                counterData.counter++;
                if (settings.debugReplayAd)
                    Debug.Log($"[ReplayAdManager] Replay计数器递增: {counterData.counter}");

                // 上报计数器递增埋点
                ReportBuryPoint("replay_counter_increment", new
                {
                    counter = counterData.counter,
                    level_id = GameDataManager.LevelNum
                });

                // 检查是否达到阈值
                if (counterData.counter >= settings.replayCountThreshold)
                {
                    if (settings.debugReplayAd)
                        Debug.Log($"[ReplayAdManager] 达到阈值 {settings.replayCountThreshold}，触发广告");

                    // 上报广告触发埋点
                    ReportBuryPoint("replay_ad_triggered", new
                    {
                        counter = counterData.counter,
                        threshold = settings.replayCountThreshold
                    });

                    // 播放广告
                    PlayReplayAd(settings, (success) =>
                    {
                        if (success)
                        {
                            // 广告成功，重置计数器
                            counterData.counter = 0;
                            counterData.totalPlayCount++;
                            if (settings.debugReplayAd)
                                Debug.Log($"[ReplayAdManager] 广告播放成功，计数器已重置，总播放次数: {counterData.totalPlayCount}");

                            // 上报总播放次数
                            ReportBuryPoint("replay_ad_total_count_update", new
                            {
                                total_count = counterData.totalPlayCount
                            });

                            // 上报计数器重置
                            ReportBuryPoint("replay_counter_reset", new
                            {
                                reason = "ad_success"
                            });
                        }
                        else if (settings.resetCounterOnAdFail)
                        {
                            // 广告失败且配置为重置
                            counterData.counter = 0;
                            if (settings.debugReplayAd)
                                Debug.Log("[ReplayAdManager] 广告播放失败，计数器已重置（根据配置）");

                            // 上报计数器重置
                            ReportBuryPoint("replay_counter_reset", new
                            {
                                reason = "ad_failed"
                            });
                        }

                        // 保存数据
                        SaveCounterData();

                        // 回调通知播放结果
                        onComplete?.Invoke(success);
                    });
                }
                else
                {
                    // 未达到阈值，保存计数器并继续
                    SaveCounterData();
                    onComplete?.Invoke(false);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ReplayAdManager] OnReplayClick异常: {ex.Message}\n{ex.StackTrace}");
                onComplete?.Invoke(false);
            }
        }

        /// <summary>
        /// 重置计数器（供外部调用，如新一轮游戏开始时）
        /// </summary>
        public void ResetCounter()
        {
            counterData.counter = 0;
            SaveCounterData();

            if (settings != null && settings.debugReplayAd)
                Debug.Log("[ReplayAdManager] 计数器已手动重置");

            ReportBuryPoint("replay_counter_reset", new
            {
                reason = "manual_reset"
            });
        }

        /// <summary>
        /// 获取当前计数器值（供调试或UI显示）
        /// </summary>
        public int GetCurrentCounter()
        {
            return counterData?.counter ?? 0;
        }

        /// <summary>
        /// 获取总广告播放次数
        /// </summary>
        public int GetTotalPlayCount()
        {
            return counterData?.totalPlayCount ?? 0;
        }

        #endregion

        #region 数据持久化

        /// <summary>
        /// 加载计数器数据
        /// </summary>
        private void LoadCounterData()
        {
            try
            {
                var storageManager = StorageManager.Instance;
                if (storageManager != null)
                {
                    counterData = storageManager.Load<ReplayAdCounterData>(
                        ReplayAdStorageKeys.REPLAY_AD_COUNTER,
                        StorageType.Json
                    );

                    if (counterData == null)
                    {
                        counterData = ReplayAdCounterData.CreateDefault();
                                    if (settings != null && settings.debugReplayAd)
                            Debug.Log("[ReplayAdManager] 首次加载，使用默认数据");
                    }
                    else
                    {
                                    if (settings != null && settings.debugReplayAd)
                            Debug.Log($"[ReplayAdManager] 加载成功: counter={counterData.counter}, totalPlayCount={counterData.totalPlayCount}");
                    }
                }
                else
                {
                    Debug.LogWarning("[ReplayAdManager] StorageManager未找到，使用默认数据");
                    counterData = ReplayAdCounterData.CreateDefault();
                }

                isDataLoaded = true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ReplayAdManager] 加载数据异常: {ex.Message}");
                counterData = ReplayAdCounterData.CreateDefault();
                isDataLoaded = true;
            }
        }

        /// <summary>
        /// 保存计数器数据
        /// </summary>
        private void SaveCounterData()
        {
            try
            {
                // 更新时间戳
                counterData.lastUpdateTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                var storageManager = StorageManager.Instance;
                if (storageManager != null)
                {
                    bool success = storageManager.Save(
                        ReplayAdStorageKeys.REPLAY_AD_COUNTER,
                        counterData,
                        StorageType.Json
                    );

                            if (settings != null && settings.debugReplayAd)
                    {
                        if (success)
                            Debug.Log($"[ReplayAdManager] 保存成功: counter={counterData.counter}");
                        else
                            Debug.LogWarning("[ReplayAdManager] 保存失败");
                    }
                }
                else
                {
                    Debug.LogWarning("[ReplayAdManager] StorageManager未找到，无法保存数据");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ReplayAdManager] 保存数据异常: {ex.Message}");
            }
        }

        #endregion

        #region 广告播放

        /// <summary>
        /// 播放Replay广告
        /// </summary>
        private void PlayReplayAd(FailedPopupSettings settings, Action<bool> onComplete)
        {
            try
            {
                var adSystemManager = AdSystemManager.Instance;
                if (adSystemManager == null)
                {
                    Debug.LogWarning("[ReplayAdManager] AdSystemManager未找到，无法播放广告");
                    ReportBuryPoint("replay_ad_show_failed", new
                    {
                        ad_entry = settings.replayAdEntryName,
                        reason = "ad_system_manager_null"
                    });
                    onComplete?.Invoke(false);
                    return;
                }

                if (settings.debugReplayAd)
                    Debug.Log($"[ReplayAdManager] 开始播放广告: {settings.replayAdEntryName}");

                // 调用广告系统播放广告
                adSystemManager.PlayAd(settings.replayAdEntryName, (success) =>
                {
                    if (success)
                    {
                        if (settings.debugReplayAd)
                            Debug.Log("[ReplayAdManager] 广告播放成功");

                        ReportBuryPoint("replay_ad_show_success", new
                        {
                            ad_entry = settings.replayAdEntryName,
                            ad_type = "interstitial"
                        });
                    }
                    else
                    {
                        Debug.LogWarning("[ReplayAdManager] 广告播放失败");
                        ReportBuryPoint("replay_ad_show_failed", new
                        {
                            ad_entry = settings.replayAdEntryName,
                            reason = "ad_play_failed"
                        });
                    }

                    onComplete?.Invoke(success);
                });
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ReplayAdManager] 播放广告异常: {ex.Message}\n{ex.StackTrace}");
                ReportBuryPoint("replay_ad_show_failed", new
                {
                    ad_entry = settings?.replayAdEntryName ?? "unknown",
                    reason = $"exception_{ex.GetType().Name}"
                });
                onComplete?.Invoke(false);
            }
        }

        #endregion

        #region 埋点上报

        /// <summary>
        /// 上报埋点事件
        /// </summary>
        private void ReportBuryPoint(string eventName, object data)
        {
            try
            {
                var nativeBridgeManager = NativeBridgeManager.Instance;
                if (nativeBridgeManager != null)
                {
                    // 将数据转换为JSON字符串
                    string jsonData = JsonUtility.ToJson(data);

                    // 使用BridgeMessageType.BuryPoint发送埋点
                    nativeBridgeManager.SendMessageToPlatform(
                        BridgeMessageType.BuryPoint,
                        eventName,
                        jsonData
                    );

                            if (settings != null && settings.debugReplayAd)
                        Debug.Log($"[ReplayAdManager] 埋点上报: {eventName} - {jsonData}");
                }
                else
                {
                    Debug.LogWarning($"[ReplayAdManager] NativeBridgeManager未找到，无法上报埋点: {eventName}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ReplayAdManager] 埋点上报异常: {ex.Message}");
            }
        }

        #endregion
    }
}
