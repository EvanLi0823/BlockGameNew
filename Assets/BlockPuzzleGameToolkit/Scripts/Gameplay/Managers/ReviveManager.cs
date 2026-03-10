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

using System.Collections;
using System.Collections.Generic;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using BlockPuzzleGameToolkit.Scripts.Settings;
using BlockPuzzleGameToolkit.Scripts.Enums;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Gameplay.Managers
{
    /// <summary>
    /// 复活管理器
    /// 负责管理关卡失败后的复活功能，包括复活次数、方块刷新等
    /// </summary>
    public class ReviveManager : MonoBehaviour
    {
        // ========== 单例模式 ==========
        private static ReviveManager _instance;
        public static ReviveManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ReviveManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("ReviveManager");
                        _instance = go.AddComponent<ReviveManager>();
                    }
                }
                return _instance;
            }
        }

        // ========== 复活数据 ==========
        /// <summary>
        /// 当前关卡的复活次数
        /// </summary>
        private int currentReviveCount = 0;

        /// <summary>
        /// 复活次数记录（按关卡）
        /// </summary>
        private Dictionary<int, int> reviveCountPerLevel = new Dictionary<int, int>();

        // ========== 管理器引用 ==========
        private LevelManager levelManager;
        private CellDeckManager cellDeckManager;
        private FieldManager fieldManager;
        private ItemFactory itemFactory;
        private FailedPopupSettings settings;

        // ========== Unity生命周期 ==========
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            // 监听关卡开始事件，重置复活次数
            EventManager.GetEvent(EGameEvent.LevelStarted).Subscribe(OnLevelStarted);
        }

        private void OnDisable()
        {
            EventManager.GetEvent(EGameEvent.LevelStarted).Unsubscribe(OnLevelStarted);
        }

        /// <summary>
        /// 关卡开始时重置复活次数
        /// </summary>
        private void OnLevelStarted()
        {
            currentReviveCount = 0;
            UpdateReferences();
        }

        /// <summary>
        /// 更新管理器引用
        /// </summary>
        private void UpdateReferences()
        {
            levelManager = FindObjectOfType<LevelManager>();
            cellDeckManager = FindObjectOfType<CellDeckManager>();
            fieldManager = FindObjectOfType<FieldManager>();
            itemFactory = FindObjectOfType<ItemFactory>();
            settings = FailedPopupSettings.Instance;
        }

        /// <summary>
        /// 检查是否可以复活
        /// </summary>
        /// <returns>是否允许复活</returns>
        public bool CanRevive()
        {
            if (settings == null)
                settings = FailedPopupSettings.Instance;

            if (settings == null || !settings.allowFreeRevive)
                return false;

            return settings.CanRevive(currentReviveCount);
        }

        /// <summary>
        /// 获取当前关卡的复活次数
        /// </summary>
        public int GetReviveCount()
        {
            return currentReviveCount;
        }

        /// <summary>
        /// 获取剩余复活次数
        /// </summary>
        public int GetRemainingRevives()
        {
            if (settings == null)
                settings = FailedPopupSettings.Instance;

            if (settings == null || settings.maxRevivesPerLevel == 0)
                return -1; // 无限制

            return Mathf.Max(0, settings.maxRevivesPerLevel - currentReviveCount);
        }

        /// <summary>
        /// 执行复活
        /// </summary>
        /// <returns>复活是否成功</returns>
        public bool ExecuteRevive()
        {
            Debug.Log($"[ReviveManager] ExecuteRevive调用 - CanRevive: {CanRevive()}");

            if (!CanRevive())
            {
                Debug.LogWarning("[ReviveManager] 无法复活：已达到最大复活次数或复活被禁用");
                return false;
            }

            // 触发关卡失败事件（用于难度系统）
            EventManager.GetEvent(EGameEvent.LevelFailed).Invoke();
            Debug.Log("[ReviveManager] 已触发LevelFailed事件（复活算作一次失败）");

            // 确保引用有效
            UpdateReferences();

            if (levelManager == null || cellDeckManager == null)
            {
                Debug.LogError("[ReviveManager] 缺少必要的管理器引用");
                return false;
            }

            // 增加复活计数
            currentReviveCount++;

            // 埋点上报：Replay (复活时上报)
            Debug.Log($"[ReviveManager] 准备上报Replay埋点 - NativeBridgeManager存在: {BlockPuzzle.NativeBridge.NativeBridgeManager.Instance != null}");
            if (BlockPuzzle.NativeBridge.NativeBridgeManager.Instance != null)
            {
                int levelNumber = GameDataManager.LevelNum;
                Debug.Log($"[ReviveManager] 上报Replay埋点 - 关卡ID: {levelNumber}");
                // p2只包含关卡ID（与Level上报相同）
                BlockPuzzle.NativeBridge.NativeBridgeManager.Instance.SendMessageToPlatform(
                    BlockPuzzle.NativeBridge.Enums.BridgeMessageType.BuryPoint,
                    "Replay",
                    levelNumber.ToString()
                );
                Debug.Log($"[ReviveManager] 埋点上报完成：Replay = {levelNumber}");
            }
            else
            {
                Debug.LogWarning("[ReviveManager] NativeBridgeManager.Instance为null，无法上报Replay埋点");
            }

            // 记录关卡复活次数
            int levelNum = levelManager.currentLevel;
            if (!reviveCountPerLevel.ContainsKey(levelNum))
                reviveCountPerLevel[levelNum] = 0;
            reviveCountPerLevel[levelNum]++;

            // 清除失败状态的填充方块
            levelManager.ClearEmptyCells();

            // 刷新方块
            RefreshShapesForRevive();

            // 恢复游戏状态
            EventManager.GameStatus = EGameState.Playing;

            Debug.Log($"[ReviveManager] 复活成功！当前关卡已复活 {currentReviveCount} 次");

            // 触发复活事件（可用于统计等）
            EventManager.GetEvent(EGameEvent.PlayerRevived)?.Invoke();

            return true;
        }

        /// <summary>
        /// 刷新复活用的方块
        /// </summary>
        private void RefreshShapesForRevive()
        {
            if (cellDeckManager == null || settings == null)
                return;

            int shapeCount = settings.refreshShapeCount;

            // 调用CellDeckManager的刷新方法
            cellDeckManager.RefreshShapesForRevive(shapeCount, settings.guaranteePlaceableShape);
        }

        /// <summary>
        /// 获取关卡的历史复活次数
        /// </summary>
        /// <param name="levelNum">关卡编号</param>
        /// <returns>该关卡的总复活次数</returns>
        public int GetLevelReviveHistory(int levelNum)
        {
            return reviveCountPerLevel.ContainsKey(levelNum) ? reviveCountPerLevel[levelNum] : 0;
        }

        /// <summary>
        /// 清除所有复活记录
        /// </summary>
        public void ClearReviveHistory()
        {
            reviveCountPerLevel.Clear();
            currentReviveCount = 0;
        }

        /// <summary>
        /// 保存复活数据到PlayerPrefs
        /// </summary>
        public void SaveReviveData()
        {
            // 保存当前复活次数
            PlayerPrefs.SetInt("CurrentReviveCount", currentReviveCount);

            // 保存历史记录（简化版，只保存最近10个关卡）
            List<int> recentLevels = new List<int>();
            foreach (var kvp in reviveCountPerLevel)
            {
                if (recentLevels.Count < 10)
                {
                    PlayerPrefs.SetInt($"ReviveHistory_Level_{kvp.Key}", kvp.Value);
                    recentLevels.Add(kvp.Key);
                }
            }

            // 保存关卡列表
            PlayerPrefs.SetString("ReviveHistoryLevels", string.Join(",", recentLevels));
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 从PlayerPrefs加载复活数据
        /// </summary>
        public void LoadReviveData()
        {
            // 加载当前复活次数
            currentReviveCount = PlayerPrefs.GetInt("CurrentReviveCount", 0);

            // 加载历史记录
            reviveCountPerLevel.Clear();
            string levelListStr = PlayerPrefs.GetString("ReviveHistoryLevels", "");
            if (!string.IsNullOrEmpty(levelListStr))
            {
                string[] levels = levelListStr.Split(',');
                foreach (string levelStr in levels)
                {
                    if (int.TryParse(levelStr, out int level))
                    {
                        int count = PlayerPrefs.GetInt($"ReviveHistory_Level_{level}", 0);
                        if (count > 0)
                        {
                            reviveCountPerLevel[level] = count;
                        }
                    }
                }
            }
        }
    }
}