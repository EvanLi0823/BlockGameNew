// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

using System;
using UnityEngine;
using BlockPuzzleGameToolkit.Scripts.Settings;
using BlockPuzzleGameToolkit.Scripts.Multiplier.Storage;
using BlockPuzzleGameToolkit.Scripts.GameCore;

namespace BlockPuzzleGameToolkit.Scripts.Multiplier.Core
{
    /// <summary>
    /// 滑动倍率模块核心管理器
    /// 负责管理配置加载、状态切换、事件派发等核心功能
    /// </summary>
    public class MultiplierManager : SingletonBehaviour<MultiplierManager>
    {
        #region Events

        /// <summary>
        /// 倍率变化事件
        /// </summary>
        public event Action<int> OnMultiplierChanged;

        /// <summary>
        /// 配置切换事件（切换到提现后配置时触发）
        /// </summary>
        public event Action OnConfigSwitched;

        /// <summary>
        /// 配置索引变化事件
        /// </summary>
        public event Action<int> OnConfigIndexChanged;

        /// <summary>
        /// 滑动开始事件
        /// </summary>
        public event Action OnSlideStarted;

        /// <summary>
        /// 滑动停止事件
        /// </summary>
        public event Action<int> OnSlideStopped;

        #endregion

        #region Fields

        [Header("配置")]
        [SerializeField] private SliderMultiplierSettings settings;

        [Header("速度控制（运行时调试）")]
        [Tooltip("滑块移动速度 - 运行时可调整")]
        [Range(10f, 200f)]
        [SerializeField] private float debugSliderSpeed = 50f;

        [Header("状态")]
        [SerializeField] private bool isWithdrawn = false;
        [SerializeField] private int currentConfigIndex = 0;
        [SerializeField] private int currentMultiplier = 1;
        [SerializeField] private bool isSliding = false;

        // 当前使用的配置集
        private SliderMultiplierSettings.MultiplierConfigSet currentConfig;

        // UI组件引用（动态获取）
        private UI.MultiplierSliderUI sliderUI;

        #endregion

        #region Initialization

        /// <summary>
        /// 初始化优先级（倍率系统在货币系统后初始化）
        /// </summary>
        public override int InitPriority => 30;

        /// <summary>
        /// 是否在场景切换时保持单例
        /// </summary>
        protected override bool DontDestroyOnSceneChange => true;

        /// <summary>
        /// 统一初始化入口
        /// </summary>
        public override void OnInit()
        {
            if (IsInitialized) return;

            Debug.Log("[MultiplierManager] 开始初始化倍率系统");

            LoadSettings();
            LoadSavedData();
            LoadCurrentConfig();
            // 订阅EGameEvent.HasWithDraw事件
            EventManager.GetEvent(Enums.EGameEvent.HasWithDraw).Subscribe(SwitchToWithdrawConfig);

            base.OnInit(); // 设置IsInitialized = true
        }

        #endregion

        #region Unity Lifecycle

        public override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            // 启动时检查是否需要每日重置
            CheckAndPerformDailyReset();
        }

        private void OnDestroy()
        {
            // 取消订阅
            EventManager.GetEvent(Enums.EGameEvent.HasWithDraw).Unsubscribe(SwitchToWithdrawConfig);
        }

#if UNITY_EDITOR
        /// <summary>
        /// 在编辑器中修改值时调用
        /// </summary>
        private void OnValidate()
        {
            // 同步调试速度到设置
            if (Application.isPlaying && settings != null)
            {
                settings.sliderSpeed = debugSliderSpeed;

                // 如果正在滑动，立即应用新速度
                if (isSliding && sliderUI != null)
                {
                    sliderUI.SetSliderSpeed(debugSliderSpeed);
                }

                Debug.Log($"[MultiplierManager] Inspector速度更新: {debugSliderSpeed} 像素/秒");
            }
        }
#endif

        #endregion

        #region Settings and Configuration

        /// <summary>
        /// 加载配置文件
        /// </summary>
        private void LoadSettings()
        {
            if (settings == null)
            {
                settings = Resources.Load<SliderMultiplierSettings>("Settings/SliderMultiplierSettings");

                if (settings == null)
                {
                    Debug.LogError("[MultiplierManager] 无法加载SliderMultiplierSettings配置文件！");
                    return;
                }
            }

            // 同步速度值到Inspector
            debugSliderSpeed = settings.sliderSpeed;

            // 验证配置
            if (!settings.ValidateAll())
            {
                Debug.LogWarning("[MultiplierManager] 配置验证失败");
#if UNITY_EDITOR
                // 只在编辑器中自动添加默认配置
                Debug.LogWarning("[MultiplierManager] 在编辑器中使用默认配置");
                settings.AddDefaultConfigs();
#else
                // 运行时如果配置无效，创建临时默认配置
                Debug.LogWarning("[MultiplierManager] 配置无效，使用临时默认配置");
                // 继续运行，ValidateAll已经输出了具体的错误信息
#endif
            }

            Debug.Log($"[MultiplierManager] 配置加载成功，速度: {debugSliderSpeed} 像素/秒");
        }

        /// <summary>
        /// 加载保存的数据
        /// </summary>
        private void LoadSavedData()
        {
            // 如果是第一次使用，初始化数据
            if (!MultiplierDataStorage.IsInitialized())
            {
                MultiplierDataStorage.MarkAsInitialized();
                MultiplierDataStorage.SaveLastResetDate(DateTime.Now);
                Debug.Log("[MultiplierManager] 首次初始化倍率模块");
            }

            // 加载提现状态
            isWithdrawn = MultiplierDataStorage.LoadWithdrawStatus();

            // 加载当前配置索引
            currentConfigIndex = MultiplierDataStorage.LoadConfigIndex(isWithdrawn);

            Debug.Log($"[MultiplierManager] 加载数据 - 提现状态: {isWithdrawn}, 配置索引: {currentConfigIndex}");
        }

        /// <summary>
        /// 加载当前配置
        /// </summary>
        private void LoadCurrentConfig()
        {
            if (settings == null) return;

            currentConfig = settings.GetConfig(isWithdrawn, currentConfigIndex);

            if (currentConfig == null)
            {
                Debug.LogError($"[MultiplierManager] 无法加载配置: isWithdrawn={isWithdrawn}, index={currentConfigIndex}");
                return;
            }

            Debug.Log($"[MultiplierManager] 加载配置成功: 倍率={string.Join(",", currentConfig.multipliers)}");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 获取当前倍率
        /// </summary>
        public int GetCurrentMultiplier()
        {
            return currentMultiplier;
        }

        /// <summary>
        /// 开始滑动
        /// </summary>
        public void StartSliding()
        {
            if (isSliding)
            {
                Debug.LogWarning("[MultiplierManager] 滑动已经开始");
                return;
            }

            isSliding = true;

            if (sliderUI != null)
            {
                // 使用debugSliderSpeed（可在Inspector中调整）
                sliderUI.StartSliding(debugSliderSpeed);
            }
            else
            {
                Debug.LogWarning("[MultiplierManager] sliderUI未设置，请先调用SetSliderUI设置UI组件");
            }

            OnSlideStarted?.Invoke();
            Debug.Log($"[MultiplierManager] 开始滑动，速度: {debugSliderSpeed} 像素/秒");
        }

        /// <summary>
        /// 停止滑动并返回倍率
        /// </summary>
        public int StopSliding()
        {
            if (!isSliding)
            {
                Debug.LogWarning("[MultiplierManager] 滑动尚未开始");
                return currentMultiplier;
            }

            isSliding = false;

            // 获取停止位置的倍率
            if (sliderUI != null)
            {
                float position = sliderUI.StopSliding();
                int zoneIndex = sliderUI.GetCurrentZone(position);

                if (currentConfig != null && currentConfig.multipliers.Length > zoneIndex)
                {
                    currentMultiplier = Mathf.RoundToInt(currentConfig.multipliers[zoneIndex]);
                }
            }

            OnSlideStopped?.Invoke(currentMultiplier);
            OnMultiplierChanged?.Invoke(currentMultiplier);

            Debug.Log($"[MultiplierManager] 停止滑动，倍率: {currentMultiplier}");
            return currentMultiplier;
        }

        /// <summary>
        /// 移动到下一个配置
        /// </summary>
        public void MoveToNextConfig()
        {
            int maxIndex = settings.GetConfigCount(isWithdrawn) - 1;

            if (currentConfigIndex < maxIndex)
            {
                currentConfigIndex++;
            }
            // 如果已经是最后一个配置，保持使用最后一个

            MultiplierDataStorage.SaveConfigIndex(isWithdrawn, currentConfigIndex);
            LoadCurrentConfig();

            OnConfigIndexChanged?.Invoke(currentConfigIndex);
            Debug.Log($"[MultiplierManager] 移动到配置 {currentConfigIndex}");
        }

        /// <summary>
        /// 切换到提现后配置
        /// </summary>
        public void SwitchToWithdrawConfig()
        {
            if (isWithdrawn)
            {
                Debug.Log("[MultiplierManager] 已经在使用提现后配置");
                return;
            }

            isWithdrawn = true;
            currentConfigIndex = 0;  // 从第一个提现后配置开始

            MultiplierDataStorage.SaveWithdrawStatus(true);
            MultiplierDataStorage.SaveConfigIndex(true, 0);

            LoadCurrentConfig();

            OnConfigSwitched?.Invoke();
            Debug.Log("[MultiplierManager] 切换到提现后配置");
        }

        /// <summary>
        /// 重置配置索引
        /// </summary>
        /// <param name="resetAll">是否重置所有配置</param>
        public void ResetConfig(bool resetAll = true)
        {
            MultiplierDataStorage.ResetConfigIndexes(resetAll);

            // 重新加载配置
            currentConfigIndex = 0;
            LoadCurrentConfig();

            Debug.Log($"[MultiplierManager] 重置配置 (resetAll={resetAll})");
        }

        /// <summary>
        /// 重置提现后配置（提现时调用）
        /// </summary>
        public void ResetPostWithdrawConfig()
        {
            if (!isWithdrawn) return;

            ResetConfig(false);  // 仅重置提现后配置
        }

        /// <summary>
        /// 获取当前配置信息
        /// </summary>
        public SliderMultiplierSettings.MultiplierConfigSet GetCurrentConfig()
        {
            return currentConfig;
        }

        /// <summary>
        /// 获取配置设置
        /// </summary>
        public SliderMultiplierSettings GetSettings()
        {
            return settings;
        }

        /// <summary>
        /// 设置UI组件引用
        /// </summary>
        public void SetSliderUI(UI.MultiplierSliderUI ui)
        {
            sliderUI = ui;

            // 设置UI时同时更新倍率显示
            UpdateSliderMultipliers();
        }

        /// <summary>
        /// 设置滑动速度（运行时调整）
        /// </summary>
        /// <param name="speed">新的滑动速度（像素/秒，范围：10-200）</param>
        public void SetSliderSpeed(float speed)
        {
            debugSliderSpeed = Mathf.Clamp(speed, 10f, 200f);

            if (settings != null)
            {
                settings.sliderSpeed = debugSliderSpeed;
            }

            // 如果正在滑动，立即应用新速度
            if (isSliding && sliderUI != null)
            {
                sliderUI.SetSliderSpeed(debugSliderSpeed);
            }

            Debug.Log($"[MultiplierManager] 滑动速度已设置为: {debugSliderSpeed} 像素/秒");
        }

        /// <summary>
        /// 获取当前滑动速度
        /// </summary>
        public float GetSliderSpeed()
        {
            return debugSliderSpeed;
        }

        /// <summary>
        /// 更新滑动条的倍率显示
        /// </summary>
        private void UpdateSliderMultipliers()
        {
            if (sliderUI == null || currentConfig == null) return;

            // 将float数组转换为int数组
            int[] intMultipliers = new int[currentConfig.multipliers.Length];
            for (int i = 0; i < currentConfig.multipliers.Length; i++)
            {
                intMultipliers[i] = Mathf.RoundToInt(currentConfig.multipliers[i]);
            }

            sliderUI.SetMultipliers(intMultipliers);
        }

        #endregion

        #region Reset Logic

        /// <summary>
        /// 检查并执行每日重置
        /// </summary>
        private void CheckAndPerformDailyReset()
        {
            if (settings == null || !settings.enableDailyReset)
                return;

            TimeSpan resetTime = settings.GetResetTimeSpan();

            if (MultiplierDataStorage.ShouldDailyReset(resetTime))
            {
                Debug.Log("[MultiplierManager] 执行每日重置");
                ResetConfig(true);  // 重置所有配置
                MultiplierDataStorage.SaveLastResetDate(DateTime.Now);
            }
        }

        /// <summary>
        /// 手动触发每日重置检查（可在游戏恢复焦点时调用）
        /// </summary>
        public void CheckDailyReset()
        {
            CheckAndPerformDailyReset();
        }

        #endregion

        #region Debug

        /// <summary>
        /// 调试：输出当前状态
        /// </summary>
        [ContextMenu("Debug - Print Status")]
        public void DebugPrintStatus()
        {
            Debug.Log("========== MultiplierManager Status ==========");
            Debug.Log($"Is Withdrawn: {isWithdrawn}");
            Debug.Log($"Current Config Index: {currentConfigIndex}");
            Debug.Log($"Current Multiplier: {currentMultiplier}");
            Debug.Log($"Is Sliding: {isSliding}");

            if (currentConfig != null)
            {
                Debug.Log($"Current Config Multipliers: {string.Join(",", currentConfig.multipliers)}");
            }

            MultiplierDataStorage.DebugPrintAllData();
        }

        /// <summary>
        /// 调试：强制切换提现状态
        /// </summary>
        [ContextMenu("Debug - Toggle Withdraw Status")]
        public void DebugToggleWithdrawStatus()
        {
            if (isWithdrawn)
            {
                isWithdrawn = false;
                currentConfigIndex = MultiplierDataStorage.LoadConfigIndex(false);
            }
            else
            {
                SwitchToWithdrawConfig();
            }

            LoadCurrentConfig();
            Debug.Log($"[MultiplierManager] 调试：切换提现状态到 {isWithdrawn}");
        }

        /// <summary>
        /// 调试：强制重置
        /// </summary>
        [ContextMenu("Debug - Force Reset")]
        public void DebugForceReset()
        {
            ResetConfig(true);
        }

        #endregion
    }
}