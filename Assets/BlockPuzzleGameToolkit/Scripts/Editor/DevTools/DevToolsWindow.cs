// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BlockPuzzleGameToolkit.Scripts.CurrencySystem;
using BlockPuzzleGameToolkit.Scripts.PropSystem.Core;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using BlockPuzzleGameToolkit.Scripts.RewardSystem;
using BlockPuzzleGameToolkit.Scripts.Settings;
using System.Linq;
using StorageSystem.Core;
using StorageSystem.Data;

namespace BlockPuzzleGameToolkit.Scripts.Editor.DevTools
{
    /// <summary>
    /// 开发者工具窗口 - 用于快速修改游戏数据
    /// 支持编辑器模式和运行时模式
    /// </summary>
    public class DevToolsWindow : EditorWindow
    {
        // 窗口滚动位置
        private Vector2 scrollPosition;

        // 货币系统
        private float newCoinsAmount = 0f;

        // 关卡系统
        private int newLevelNumber = 1;

        // 道具系统
        private int newRotateCount = 0;
        private int newRefreshCount = 0;
        private int newBombCount = 0;

        // 奖励系统
        private string selectedRangeId = "no_withdraw";
        private List<string> availableRanges = new List<string>();
        private int selectedRangeIndex = 0;

        // 刷新计时器
        private double lastRefreshTime;
        private const float REFRESH_INTERVAL = 0.5f; // 每0.5秒刷新一次

        // 存储键常量
        private const string COINS_KEY = "Coins";
        private const string LEVEL_KEY = "Level";
        private const string PROP_ROTATE_KEY = "Prop_Rotate";
        private const string PROP_REFRESH_KEY = "Prop_Refresh";
        private const string PROP_BOMB_KEY = "Prop_Bomb";

        [MenuItem("Tools/BlockPuzzleGameToolkit/Developer/Dev Tools Window &d", priority = 10)]
        public static void ShowWindow()
        {
            var window = GetWindow<DevToolsWindow>("开发者工具");
            window.minSize = new Vector2(400, 700);
            window.Show();
        }

        private void OnEnable()
        {
            LoadAvailableRanges();
        }

        private void OnDisable()
        {
            // 清理临时创建的StorageManager
            if (!Application.isPlaying)
            {
                var tempManager = GameObject.Find("TempStorageManager");
                if (tempManager != null)
                {
                    DestroyImmediate(tempManager);
                }
            }
        }

        private void OnGUI()
        {
            // 自动刷新
            if (EditorApplication.timeSinceStartup - lastRefreshTime > REFRESH_INTERVAL)
            {
                Repaint();
                lastRefreshTime = EditorApplication.timeSinceStartup;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // 标题
            EditorGUILayout.Space(10);
            GUILayout.Label("开发者工具", new GUIStyle(UnityEngine.GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            });
            EditorGUILayout.Space(10);

            // 模式提示
            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox("✅ 运行模式 - 所有功能可用", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("📝 编辑器模式 - 直接修改存档数据", MessageType.Warning);
            }
            EditorGUILayout.Space(5);

            // 货币系统模块
            DrawCurrencySection();
            EditorGUILayout.Space(10);

            // 关卡进度模块
            DrawLevelSection();
            EditorGUILayout.Space(10);

            // 道具系统模块
            DrawPropSection();
            EditorGUILayout.Space(10);

            // 奖励系统模块
            DrawRewardSection();
            EditorGUILayout.Space(10);

            // 危险操作区
            DrawDangerSection();

            EditorGUILayout.EndScrollView();
        }

        #region 货币系统

        private void DrawCurrencySection()
        {
            EditorGUILayout.BeginVertical(UnityEngine.GUI.skin.box);
            GUILayout.Label("💰 货币系统", EditorStyles.boldLabel);

            float currentCoins = 0f;

            // 优先使用运行时Manager，否则使用StorageManager或PlayerPrefs
            if (Application.isPlaying)
            {
                var currencyManager = CurrencyManager.Instance;
                if (currencyManager != null)
                {
                    // 获取内部整数值并转换为显示值
                    int coinsInt = currencyManager.GetCoins();
                    currentCoins = BlockPuzzleGameToolkit.Scripts.CurrencySystem.CurrencyFormatter.ToDisplayValue(coinsInt);
                }
                else
                {
                    currentCoins = PlayerPrefs.GetFloat(COINS_KEY, 0f);
                }
            }
            else
            {
                // 编辑器模式下，尝试从StorageManager读取
                try
                {
                    // 确保StorageManager已初始化
                    if (StorageSystem.Core.StorageManager.Instance == null)
                    {
                        // 尝试查找现有的StorageManager
                        var existing = GameObject.FindObjectOfType<StorageSystem.Core.StorageManager>();
                        if (existing == null)
                        {
                            // 创建临时的StorageManager
                            var tempGO = new GameObject("TempStorageManager");
                            tempGO.hideFlags = HideFlags.HideAndDontSave;
                            var storageManager = tempGO.AddComponent<StorageSystem.Core.StorageManager>();
                            storageManager.OnInit();
                        }
                    }

                    // 尝试从二进制存储加载数据
                    var data = StorageSystem.Core.StorageManager.Instance.Load<BlockPuzzleGameToolkit.Scripts.CurrencySystem.CurrencySaveData>(
                        "currency_data",
                        StorageSystem.Core.StorageType.Binary
                    );

                    if (data != null && data.IsValid())
                    {
                        currentCoins = BlockPuzzleGameToolkit.Scripts.CurrencySystem.CurrencyFormatter.ToDisplayValue(data.CoinsInt);
                    }
                    else
                    {
                        // 如果没有二进制存储数据，回退到PlayerPrefs
                        currentCoins = PlayerPrefs.GetFloat(COINS_KEY, 0f);
                    }
                }
                catch
                {
                    // 如果加载失败，使用PlayerPrefs
                    currentCoins = PlayerPrefs.GetFloat(COINS_KEY, 0f);
                }
            }

            // 显示当前金币
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("当前金币:", GUILayout.Width(80));
            EditorGUILayout.LabelField($"${currentCoins:F3}", new GUIStyle(UnityEngine.GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.yellow }
            });
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // 设置金币
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("设置金币:", GUILayout.Width(80));
            newCoinsAmount = EditorGUILayout.FloatField(newCoinsAmount, GUILayout.Width(100));
            if (GUILayout.Button("设置", GUILayout.Width(60)))
            {
                if (newCoinsAmount >= 0)
                {
                    SetCoins(newCoinsAmount);
                    Debug.Log($"[DevTools] 金币已设置为: ${newCoinsAmount:F3}");
                }
                else
                {
                    EditorUtility.DisplayDialog("错误", "金币数量不能为负数", "确定");
                }
            }
            EditorGUILayout.EndHorizontal();

            // 快捷操作
            EditorGUILayout.LabelField("快捷操作:");
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+$10")) AddCoins(10f);
            if (GUILayout.Button("+$100")) AddCoins(100f);
            if (GUILayout.Button("+$1000")) AddCoins(1000f);
            if (GUILayout.Button("重置"))
            {
                if (EditorUtility.DisplayDialog("确认", "确定要重置金币为0吗？", "确定", "取消"))
                {
                    SetCoins(0f);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void SetCoins(float amount)
        {
            if (Application.isPlaying)
            {
                var currencyManager = CurrencyManager.Instance;
                if (currencyManager != null)
                {
                    // 转换美元值为内部整数值
                    int coinsInt = BlockPuzzleGameToolkit.Scripts.CurrencySystem.CurrencyFormatter.ToInternalValue(amount);
                    currencyManager.SetCoins(coinsInt);
                }
                else
                {
                    PlayerPrefs.SetFloat(COINS_KEY, amount);
                    PlayerPrefs.Save();
                }
            }
            else
            {
                // 编辑器模式下需要同时设置两个存储位置
                // 1. 设置PlayerPrefs（兼容旧代码）
                PlayerPrefs.SetFloat(COINS_KEY, amount);
                PlayerPrefs.Save();

                // 2. 直接修改StorageManager的二进制存储文件
                try
                {
                    // 创建或更新CurrencySaveData
                    var data = new BlockPuzzleGameToolkit.Scripts.CurrencySystem.CurrencySaveData();
                    int coinsInt = BlockPuzzleGameToolkit.Scripts.CurrencySystem.CurrencyFormatter.ToInternalValue(amount);
                    data.CoinsInt = coinsInt;
                    data.UpdateMetadata(1);

                    // 确保StorageManager已初始化
                    if (StorageSystem.Core.StorageManager.Instance == null)
                    {
                        // 尝试查找现有的StorageManager
                        var existing = GameObject.FindObjectOfType<StorageSystem.Core.StorageManager>();
                        if (existing == null)
                        {
                            // 创建临时的StorageManager
                            var tempGO = new GameObject("TempStorageManager");
                            tempGO.hideFlags = HideFlags.HideAndDontSave;
                            var storageManager = tempGO.AddComponent<StorageSystem.Core.StorageManager>();
                            storageManager.OnInit();
                        }
                    }

                    // 保存到二进制存储
                    bool success = StorageSystem.Core.StorageManager.Instance.Save(
                        "currency_data",
                        data,
                        StorageSystem.Core.StorageType.Binary
                    );

                    if (success)
                    {
                        Debug.Log($"[DevTools] 成功设置金币为 ${amount:F3}（内部值: {coinsInt}）");
                    }
                    else
                    {
                        Debug.LogWarning("[DevTools] 保存金币数据到StorageManager失败");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[DevTools] 设置金币失败: {e.Message}");
                }
            }
        }

        private void AddCoins(float amount)
        {
            if (Application.isPlaying)
            {
                var currencyManager = CurrencyManager.Instance;
                if (currencyManager != null)
                {
                    // 转换美元值为内部整数值
                    int coinsInt = BlockPuzzleGameToolkit.Scripts.CurrencySystem.CurrencyFormatter.ToInternalValue(amount);
                    currencyManager.AddCoins(coinsInt);
                }
                else
                {
                    float current = PlayerPrefs.GetFloat(COINS_KEY, 0f);
                    PlayerPrefs.SetFloat(COINS_KEY, current + amount);
                    PlayerPrefs.Save();
                }
            }
            else
            {
                // 编辑器模式下，先读取当前值
                float current = 0f;

                try
                {
                    // 确保StorageManager已初始化
                    if (StorageSystem.Core.StorageManager.Instance == null)
                    {
                        var existing = GameObject.FindObjectOfType<StorageSystem.Core.StorageManager>();
                        if (existing == null)
                        {
                            var tempGO = new GameObject("TempStorageManager");
                            tempGO.hideFlags = HideFlags.HideAndDontSave;
                            var storageManager = tempGO.AddComponent<StorageSystem.Core.StorageManager>();
                            storageManager.OnInit();
                        }
                    }

                    // 尝试从二进制存储加载当前数据
                    var data = StorageSystem.Core.StorageManager.Instance.Load<BlockPuzzleGameToolkit.Scripts.CurrencySystem.CurrencySaveData>(
                        "currency_data",
                        StorageSystem.Core.StorageType.Binary
                    );

                    if (data != null && data.IsValid())
                    {
                        current = BlockPuzzleGameToolkit.Scripts.CurrencySystem.CurrencyFormatter.ToDisplayValue(data.CoinsInt);
                    }
                    else
                    {
                        current = PlayerPrefs.GetFloat(COINS_KEY, 0f);
                    }
                }
                catch
                {
                    current = PlayerPrefs.GetFloat(COINS_KEY, 0f);
                }

                // 设置新值
                SetCoins(current + amount);
            }
        }

        #endregion

        #region 关卡进度

        private void DrawLevelSection()
        {
            EditorGUILayout.BeginVertical(UnityEngine.GUI.skin.box);
            GUILayout.Label("📊 关卡进度", EditorStyles.boldLabel);

            // 获取当前关卡
            int currentLevel = PlayerPrefs.GetInt(LEVEL_KEY, 1);

            // 显示当前关卡
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("当前关卡:", GUILayout.Width(80));
            EditorGUILayout.LabelField($"第 {currentLevel} 关", new GUIStyle(UnityEngine.GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.cyan }
            });
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // 设置关卡
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("设置关卡:", GUILayout.Width(80));
            newLevelNumber = EditorGUILayout.IntField(newLevelNumber, GUILayout.Width(100));
            if (GUILayout.Button("设置", GUILayout.Width(60)))
            {
                if (newLevelNumber >= 1)
                {
                    SetLevel(newLevelNumber);
                    Debug.Log($"[DevTools] 关卡已设置为: {newLevelNumber}");
                }
                else
                {
                    EditorUtility.DisplayDialog("错误", "关卡编号必须大于等于1", "确定");
                }
            }
            EditorGUILayout.EndHorizontal();

            // 快捷操作
            EditorGUILayout.LabelField("快捷操作:");
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+1关")) SetLevel(currentLevel + 1);
            if (GUILayout.Button("+5关")) SetLevel(currentLevel + 5);
            if (GUILayout.Button("+10关")) SetLevel(currentLevel + 10);
            if (GUILayout.Button("重置到第1关"))
            {
                if (EditorUtility.DisplayDialog("确认", "确定要重置到第1关吗？", "确定", "取消"))
                {
                    SetLevel(1);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void SetLevel(int level)
        {
            PlayerPrefs.SetInt(LEVEL_KEY, level);
            PlayerPrefs.Save();

            // 解锁到目标关卡（无论是否运行中都要执行）
            // 因为游戏逻辑假定：设置到关卡N = 解锁1到N的所有关卡
            for (int i = 1; i <= level; i++)
            {
                if (Application.isPlaying)
                {
                    // 运行中：使用GameDataManager
                    GameDataManager.UnlockLevel(i);
                }
                else
                {
                    // 编辑器模式：直接设置PlayerPrefs
                    // UnlockLevel方法会检查并只更新更高的关卡值
                    int currentSaved = PlayerPrefs.GetInt(LEVEL_KEY, 1);
                    if (currentSaved < level)
                    {
                        PlayerPrefs.SetInt(LEVEL_KEY, level);
                    }
                }
            }

            // 确保保存
            PlayerPrefs.Save();
        }

        #endregion

        #region 道具系统

        private void DrawPropSection()
        {
            EditorGUILayout.BeginVertical(UnityEngine.GUI.skin.box);
            GUILayout.Label("🎁 道具系统", EditorStyles.boldLabel);

            // 检查道具系统是否启用
            bool propSystemEnabled = true;
            var propSettings = Resources.Load<PropSettings>("Settings/PropSettings");
            if (propSettings != null && !propSettings.enablePropSystem)
            {
                propSystemEnabled = false;
                EditorGUILayout.HelpBox("道具系统已被禁用 (PropSettings.enablePropSystem = false)", MessageType.Warning);
            }

            // 道具类型
            DrawPropItem("旋转道具", "🔄", PROP_ROTATE_KEY, ref newRotateCount, propSystemEnabled);
            EditorGUILayout.Space(5);
            DrawPropItem("刷新道具", "🔀", PROP_REFRESH_KEY, ref newRefreshCount, propSystemEnabled);
            EditorGUILayout.Space(5);
            DrawPropItem("炸弹道具", "💣", PROP_BOMB_KEY, ref newBombCount, propSystemEnabled);

            EditorGUILayout.Space(10);

            // 批量操作
            EditorGUI.BeginDisabledGroup(!propSystemEnabled);

            EditorGUILayout.LabelField("批量操作:");
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("全部+10"))
            {
                AddProp(PROP_ROTATE_KEY, 10);
                AddProp(PROP_REFRESH_KEY, 10);
                AddProp(PROP_BOMB_KEY, 10);
                Debug.Log("[DevTools] 所有道具+10");
            }

            if (GUILayout.Button("全部设为99"))
            {
                SetProp(PROP_ROTATE_KEY, 99);
                SetProp(PROP_REFRESH_KEY, 99);
                SetProp(PROP_BOMB_KEY, 99);
                Debug.Log("[DevTools] 所有道具设为99");
            }

            if (GUILayout.Button("全部清零"))
            {
                if (EditorUtility.DisplayDialog("确认", "确定要清空所有道具吗？", "确定", "取消"))
                {
                    SetProp(PROP_ROTATE_KEY, 0);
                    SetProp(PROP_REFRESH_KEY, 0);
                    SetProp(PROP_BOMB_KEY, 0);
                    Debug.Log("[DevTools] 所有道具已清零");
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndVertical();
        }

        private void DrawPropItem(string propName, string icon, string key, ref int newCount, bool enabled)
        {
            EditorGUI.BeginDisabledGroup(!enabled);

            // 获取当前数量
            int currentCount = GetPropCount(key);

            EditorGUILayout.BeginHorizontal();

            // 显示道具名称和图标
            EditorGUILayout.LabelField($"{icon} {propName}:", GUILayout.Width(100));

            // 显示当前数量
            EditorGUILayout.LabelField(currentCount.ToString(), new GUIStyle(UnityEngine.GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.green },
                alignment = TextAnchor.MiddleCenter
            }, GUILayout.Width(50));

            // 输入新数量
            newCount = EditorGUILayout.IntField(newCount, GUILayout.Width(60));

            // 设置按钮
            if (GUILayout.Button("设置", GUILayout.Width(50)))
            {
                if (newCount >= 0)
                {
                    SetProp(key, newCount);
                    Debug.Log($"[DevTools] {propName}已设置为: {newCount}");
                }
                else
                {
                    EditorUtility.DisplayDialog("错误", "道具数量不能为负数", "确定");
                }
            }

            // 快捷增加
            if (GUILayout.Button("+5", GUILayout.Width(40)))
            {
                AddProp(key, 5);
            }

            if (GUILayout.Button("+10", GUILayout.Width(40)))
            {
                AddProp(key, 10);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUI.EndDisabledGroup();
        }

        private int GetPropCount(string key)
        {
            if (Application.isPlaying)
            {
                var propManager = PropManager.Instance;
                if (propManager != null)
                {
                    PropType type = GetPropTypeFromKey(key);
                    return propManager.GetPropCount(type);
                }
            }

            // 使用PlayerPrefs
            return PlayerPrefs.GetInt(key, 0);
        }

        private void SetProp(string key, int count)
        {
            if (Application.isPlaying)
            {
                var propManager = PropManager.Instance;
                if (propManager != null)
                {
                    PropType type = GetPropTypeFromKey(key);
                    propManager.SetPropCount(type, count);
                    return;
                }
            }

            // 直接设置PlayerPrefs
            PlayerPrefs.SetInt(key, count);
            PlayerPrefs.Save();
        }

        private void AddProp(string key, int amount)
        {
            int current = GetPropCount(key);
            SetProp(key, current + amount);
        }

        private PropType GetPropTypeFromKey(string key)
        {
            switch (key)
            {
                case PROP_ROTATE_KEY: return PropType.Rotate;
                case PROP_REFRESH_KEY: return PropType.Refresh;
                case PROP_BOMB_KEY: return PropType.Bomb;
                default: return PropType.None;
            }
        }

        #endregion

        #region 奖励系统

        private void DrawRewardSection()
        {
            EditorGUILayout.BeginVertical(UnityEngine.GUI.skin.box);
            GUILayout.Label("🏆 奖励系统", EditorStyles.boldLabel);

            // 获取当前配置
            string currentRange = GetCurrentRewardGroup();

            // 显示当前分组
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("当前分组:", GUILayout.Width(80));
            EditorGUILayout.LabelField(currentRange, new GUIStyle(UnityEngine.GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.magenta }
            });
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // 加载可用分组
            if (availableRanges.Count == 0)
            {
                LoadAvailableRanges();
            }

            // 分组选择
            if (availableRanges.Count > 0)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("选择分组:", GUILayout.Width(80));

                string[] rangeArray = availableRanges.ToArray();
                selectedRangeIndex = EditorGUILayout.Popup(selectedRangeIndex, rangeArray);

                if (GUILayout.Button("切换", GUILayout.Width(60)))
                {
                    if (selectedRangeIndex >= 0 && selectedRangeIndex < availableRanges.Count)
                    {
                        SetRewardGroup(availableRanges[selectedRangeIndex]);
                        Debug.Log($"[DevTools] 奖励分组已切换为: {availableRanges[selectedRangeIndex]}");
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("未找到奖励配置文件", MessageType.Warning);
            }

            EditorGUILayout.Space(5);

            // 快捷切换按钮
            EditorGUILayout.LabelField("快捷切换:");
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("未提现"))
            {
                SetRewardGroup("no_withdraw");
            }

            if (GUILayout.Button("档位1"))
            {
                SetRewardGroup("tier_1");
            }

            if (GUILayout.Button("档位2"))
            {
                SetRewardGroup("tier_2");
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void LoadAvailableRanges()
        {
            availableRanges.Clear();

            var rewardSettings = Resources.Load<RewardSystemSettings>("Settings/RewardSystemSettings");
            if (rewardSettings != null && rewardSettings.RangeGroups != null)
            {
                availableRanges = rewardSettings.RangeGroups.Select(r => r.RangeId).ToList();

                // 设置默认选中项为第一个
                if (availableRanges.Count > 0)
                {
                    selectedRangeIndex = 0;
                    // 如果运行中，获取当前实际的区间
                    if (Application.isPlaying)
                    {
                        var calculator = RewardCalculator.Instance;
                        if (calculator != null)
                        {
                            string currentRange = calculator.CurrentRangeId;
                            int index = availableRanges.IndexOf(currentRange);
                            if (index >= 0) selectedRangeIndex = index;
                        }
                    }
                }
            }

            // 添加默认分组
            if (availableRanges.Count == 0)
            {
                availableRanges.Add("no_withdraw");
                availableRanges.Add("tier_1");
                availableRanges.Add("tier_2");
            }
        }

        private string GetCurrentRewardGroup()
        {
            // 优先从运行时的RewardCalculator获取
            if (Application.isPlaying)
            {
                var calculator = RewardCalculator.Instance;
                if (calculator != null)
                {
                    return calculator.CurrentRangeId;
                }
            }

            // 否则返回默认值
            return "no_withdraw";
        }

        private void SetRewardGroup(string groupId)
        {
            // 如果游戏运行中，使用RewardCalculator切换
            if (Application.isPlaying)
            {
                var calculator = RewardCalculator.Instance;
                if (calculator != null)
                {
                    bool success = calculator.SwitchToRange(groupId);
                    if (success)
                    {
                        Debug.Log($"[DevTools] 成功切换到奖励分组: {groupId}");
                    }
                    else
                    {
                        Debug.LogWarning($"[DevTools] 切换奖励分组失败: {groupId}");
                    }
                }
                else
                {
                    Debug.LogError("[DevTools] RewardCalculator未找到");
                }
            }
            else
            {
                // 编辑器模式下只能提示
                Debug.LogWarning("[DevTools] 奖励分组切换需要在游戏运行时进行");
                EditorUtility.DisplayDialog("提示", "奖励分组切换需要在游戏运行时进行", "确定");
            }
        }

        #endregion

        #region 危险操作

        private void DrawDangerSection()
        {
            EditorGUILayout.BeginVertical(UnityEngine.GUI.skin.box);

            GUILayout.Label("⚠️ 危险操作", new GUIStyle(EditorStyles.boldLabel)
            {
                normal = { textColor = Color.red }
            });

            EditorGUILayout.HelpBox("以下操作会永久删除数据，请谨慎使用！", MessageType.Error);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("清除所有PlayerPrefs", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("危险操作确认",
                    "确定要清除所有PlayerPrefs数据吗？\n此操作不可恢复！",
                    "确定清除", "取消"))
                {
                    PlayerPrefs.DeleteAll();
                    PlayerPrefs.Save();
                    Debug.LogWarning("[DevTools] 所有PlayerPrefs已清除");

                    if (Application.isPlaying)
                    {
                        EditorUtility.DisplayDialog("提示", "PlayerPrefs已清除，建议重启游戏以重新加载数据", "确定");
                    }
                }
            }

            if (GUILayout.Button("重置所有数据", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("危险操作确认",
                    "确定要重置所有游戏数据吗？\n包括金币、关卡、道具等所有数据！\n此操作不可恢复！",
                    "确定重置", "取消"))
                {
                    ResetAllData();
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void ResetAllData()
        {
            // 重置金币
            SetCoins(0f);

            // 重置关卡
            SetLevel(1);

            // 重置道具
            SetProp(PROP_ROTATE_KEY, 0);
            SetProp(PROP_REFRESH_KEY, 0);
            SetProp(PROP_BOMB_KEY, 0);

            // 重置奖励分组
            SetRewardGroup("no_withdraw");

            Debug.LogWarning("[DevTools] 所有游戏数据已重置");

            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("提示", "所有数据已重置，建议重启游戏", "确定");
            }
        }

        #endregion
    }
}
#endif