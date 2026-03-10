using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using BlockPuzzle.NativeBridge;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using BlockPuzzleGameToolkit.Scripts.CurrencySystem;
using BlockPuzzleGameToolkit.Scripts.GUI;
using BlockPuzzleGameToolkit.Scripts;
using DG.Tweening;
using BlockPuzzleGameToolkit.Scripts.Enums;

namespace BlockPuzzleGameToolkit.Scripts.TopPanel
{
    /// <summary>
    /// 顶部面板控制器
    /// 负责显示关卡、货币信息，以及提现按钮功能
    /// </summary>
    public class TopPanel : SingletonBehaviour<TopPanel>
    {
        // 全局货币滚动动画事件
        public static event Action<float> OnGlobalRequestCurrencyRoll;

        /// <summary>
        /// 触发全局货币滚动动画
        /// </summary>
        public static void TriggerGlobalCurrencyRoll(float delay = 0f)
        {
            Debug.Log($"[TopPanel] 触发全局货币滚动动画，延迟: {delay}秒");
            OnGlobalRequestCurrencyRoll?.Invoke(delay);
        }

        [Header("UI Components")]
        [SerializeField]
        private CustomButton withDrawButton;
        [SerializeField]
        private Image icon; // 货币图标

        [SerializeField]
        private Sprite coin; // 金币图标，白包时显示此图片

        [SerializeField]
        private TextMeshProUGUI levelText;

        [SerializeField]
        private Text addText; // 货币增加时显示的文本（如 "+$1.00"）
       
        [SerializeField]
        private TextMeshProUGUI currencyText; //此文本设置为全局货币显示文本，外部系统可以访问

        [Header("Settings")]
        [SerializeField]
        private string levelFormat = "{0}"; // 关卡显示格式

        [SerializeField]
        private bool autoUpdateUI = true; // 是否自动更新UI

        [Header("Currency Display")]
        [SerializeField]
        private bool useLargeNumberDisplay = true; // 是否使用大数显示（K/M/B/T）

        [SerializeField]
        private float largeNumberThreshold = 1000f; // 开始使用大数显示的阈值

        [SerializeField]
        private bool useCompactDisplay = false; // 是否使用紧凑显示模式

        [Header("Currency Animation")]
        [SerializeField]
        private float currencyRollDuration = 0.5f; // 数字滚动动画时长

        [SerializeField]
        private Ease currencyRollEase = Ease.OutQuad; // 滚动动画曲线

        [SerializeField]
        private bool enableCurrencyRollAnimation = true; // 是否启用滚动动画

        [SerializeField]
        private float anidelay = 0f; // 是否启用滚动动画
        // 事件
        public static event Action OnWithdrawClicked;

        // 私有字段
        private int currentLevel;
        private int currentCurrencyInt; // 存储整数值
        private Tweener currentRollTween; // 当前的滚动动画
        private int displayedCurrencyInt; // 当前显示的货币值（用于动画）
        private Sequence addTextSequence; // 增加文本的动画序列

        #region Unity生命周期

        private void Start()
        {
            InitializeUI();
            RegisterEvents();
            UpdateUI();

            // 初始化显示的货币值
            displayedCurrencyInt = currentCurrencyInt;
        }

        private void OnEnable()
        {
            if (autoUpdateUI)
            {
                UpdateUI();
            }
        }

        private void OnDestroy()
        {
            UnregisterEvents();

            // 清理动画
            if (currentRollTween != null && currentRollTween.IsActive())
            {
                currentRollTween.Kill();
            }

            if (addTextSequence != null && addTextSequence.IsActive())
            {
                addTextSequence.Kill();
            }

            // 确保addText被隐藏
            if (addText != null && addText.gameObject != null)
            {
                addText.gameObject.SetActive(false);
            }
        }

        #endregion

        #region 初始化

        private void InitializeUI()
        {
            // 初始化提现按钮
            if (withDrawButton != null)
            {
                withDrawButton.onClick.RemoveAllListeners();
                withDrawButton.onClick.AddListener(OnWithDrawButtonClicked);

                // 白包模式下隐藏提现按钮
                bool isWhitePackageMode = false;
                if (NativeBridgeManager.Instance != null)
                {
                    isWhitePackageMode = NativeBridgeManager.Instance.IsWhitePackage();
                }

                if (isWhitePackageMode)
                {
                    withDrawButton.gameObject.SetActive(false);
                    Debug.Log("[TopPanel] 白包模式：隐藏提现按钮");
                }
                else
                {
                    withDrawButton.gameObject.SetActive(true);
                    Debug.Log("[TopPanel] 标准模式：显示提现按钮");
                }
            }
            else
            {
                Debug.LogWarning("[TopPanel] WithDraw button is not assigned!");
            }

            // 初始化货币图标
            UpdateCurrencyIcon();

            // 检查文本组件
            if (levelText == null)
            {
                Debug.LogWarning("[TopPanel] Level text is not assigned!");
            }

            if (currencyText == null)
            {
                Debug.LogWarning("[TopPanel] Currency text is not assigned!");
            }

            // 初始化加钱文本框（用于显示货币增加动画）
            if (addText != null)
            {
                addText.gameObject.SetActive(false); // 初始状态为隐藏

                // 获取并检查RectTransform
                RectTransform addTextRect = addText.GetComponent<RectTransform>();
                if (addTextRect != null)
                {
                    Debug.Log($"[TopPanel] AddText RectTransform info - Position: {addTextRect.anchoredPosition}, Size: {addTextRect.sizeDelta}");

                    // 如果位置看起来不对，尝试重置到货币文本附近
                    if (Mathf.Abs(addTextRect.anchoredPosition.x) > 500 || Mathf.Abs(addTextRect.anchoredPosition.y) > 300)
                    {
                        Debug.LogWarning($"[TopPanel] AddText position seems off-screen, consider adjusting in Unity Editor");
                    }
                }

                Debug.Log("[TopPanel] AddText initialized and hidden");
            }
            else
            {
                Debug.LogWarning("[TopPanel] AddText is not assigned! Currency increase animation will not show.");
            }
        }

        private void RegisterEvents()
        {
            //订阅货币变动事件，接收此事件时，进行数字滚动
            EventManager.GetEvent(EGameEvent.CurrencyChanged).Subscribe(OnCurrencyRollRequested);

            // 订阅关卡重启事件，在关卡重启时更新关卡显示（NextLevel会调用RestartLevel）
            EventManager.GetEvent(EGameEvent.RestartLevel).Subscribe(OnLevelRestarted);

            // 订阅关卡开始事件，确保新关卡开始时更新显示
            EventManager.GetEvent(EGameEvent.LevelStarted).Subscribe(OnLevelStarted);

            // 订阅 ExchangeRateManager 的货币类型变化事件（语言变化会触发货币类型变化）
            if (ExchangeRateManager.Instance != null)
            {
                ExchangeRateManager.Instance.OnCurrencyChanged += OnCurrencyTypeChanged;
            }
        }

        private void UnregisterEvents()
        {
            //取消订阅货币变动事件
            EventManager.GetEvent(EGameEvent.CurrencyChanged).Unsubscribe(OnCurrencyRollRequested);

            // 取消订阅关卡重启事件
            EventManager.GetEvent(EGameEvent.RestartLevel).Unsubscribe(OnLevelRestarted);

            // 取消订阅关卡开始事件
            EventManager.GetEvent(EGameEvent.LevelStarted).Unsubscribe(OnLevelStarted);

            // 取消订阅货币类型变化事件
            if (ExchangeRateManager.Instance != null)
            {
                ExchangeRateManager.Instance.OnCurrencyChanged -= OnCurrencyTypeChanged;
            }
        }

        #endregion

        #region UI更新

        /// <summary>
        /// 更新整个UI
        /// </summary>
        public void UpdateUI()
        {
            UpdateLevelDisplay();
            UpdateCurrencyDisplay();
            UpdateCurrencyIcon();
            UpdateWithDrawButtonVisibility();
        }

        /// <summary>
        /// 更新关卡显示
        /// </summary>
        public void UpdateLevelDisplay()
        {
            currentLevel = GameDataManager.GetLevelNum();

            if (levelText != null)
            {
                levelText.text = string.Format(levelFormat, currentLevel);
            }
        }

        /// <summary>
        /// 更新货币显示
        /// </summary>
        public void UpdateCurrencyDisplay()
        {
            // 始终从CurrencyManager获取最新值
            if (CurrencyManager.Instance != null && CurrencyManager.Instance.IsInitialized)
            {
                currentCurrencyInt = CurrencyManager.Instance.GetCoins();
            }
            else
            {
                currentCurrencyInt = 0;
            }

            // 同步显示值（如果没有动画在播放）
            if (currentRollTween == null || !currentRollTween.IsActive())
            {
                displayedCurrencyInt = currentCurrencyInt;
            }

            SetCurrencyText(currentCurrencyInt);
        }

        /// <summary>
        /// 更新货币图标（根据白包模式设置）
        /// </summary>
        public void UpdateCurrencyIcon()
        {
            if (icon == null)
            {
                Debug.LogWarning("[TopPanel] Icon image is not assigned!");
                return;
            }

            // 获取白包模式状态（在app启动时已设置，不会改变）
            bool isWhitePackageMode = false;
            if (NativeBridgeManager.Instance != null)
            {
                isWhitePackageMode = NativeBridgeManager.Instance.IsWhitePackage();
            }

            // 白包模式下使用金币图标
            if (isWhitePackageMode && coin != null)
            {
                icon.sprite = coin;
                icon.SetNativeSize();
                Debug.Log("[TopPanel] 白包模式：使用金币图标");
            }
            else
            {
                // 非白包模式：保持原有图标
                Debug.Log("[TopPanel] 标准模式：使用默认图标");
            }
        }

        /// <summary>
        /// 更新提现按钮的显示状态（根据白包模式）
        /// </summary>
        public void UpdateWithDrawButtonVisibility()
        {
            if (withDrawButton == null)
            {
                return;
            }

            // 获取白包模式状态
            bool isWhitePackageMode = false;
            if (NativeBridgeManager.Instance != null)
            {
                isWhitePackageMode = NativeBridgeManager.Instance.IsWhitePackage();
            }

            // 白包模式下隐藏提现按钮
            withDrawButton.gameObject.SetActive(!isWhitePackageMode);
        }

        /// <summary>
        /// 设置关卡文本
        /// </summary>
        public void SetLevelText(int level)
        {
            currentLevel = level;
            if (levelText != null)
            {
                levelText.text = string.Format(levelFormat, level);
            }
        }

        /// <summary>
        /// 设置货币文本
        /// </summary>
        /// <param name="amount">货币金额（内部整数值）</param>
        public void SetCurrencyText(int amount)
        {
            currentCurrencyInt = amount;
            if (currencyText != null)
            {
                // 检查白包模式
                bool isWhitePackageMode = false;
                if (NativeBridgeManager.Instance != null)
                {
                    isWhitePackageMode = NativeBridgeManager.Instance.IsWhitePackage();
                }

                // 白包模式下不显示货币数值
                if (isWhitePackageMode)
                {
                    currencyText.text = "";  // 清空文本
                    Debug.Log("[TopPanel] 白包模式：不显示货币数值");
                }
                else
                {
                    // 标准模式：使用简化的 API，自动处理货币类型和显示格式
                    string displayText = CurrencyFormatter.GetAdaptiveDisplay(
                        amount,
                        useLargeNumberDisplay,
                        useCompactDisplay,
                        largeNumberThreshold);

                    currencyText.text = displayText;
                }
            }
        }

        #endregion

        #region 按钮事件

        private void OnWithDrawButtonClicked()
        {
            Debug.Log("[TopPanel] Withdraw button clicked");

            // 调用NativeBridgeManager的展示提现界面方法
            var nativeBridge = NativeBridgeManager.Instance;
            if (nativeBridge != null)
            {
                // 直接调用，数据收集由NativeBridgeManager内部处理
                nativeBridge.ShowWithdrawInterface();
                Debug.Log("[TopPanel] ShowWithdraw interface called");
            }
            else
            {
                Debug.LogWarning("[TopPanel] NativeBridgeManager not found!");
            }

            // 触发事件
            OnWithdrawClicked?.Invoke();
        }

        #endregion

        #region 事件响应

        /// <summary>
        /// 响应关卡重启事件（包括进入下一关时）
        /// </summary>
        private void OnLevelRestarted()
        {
            // 更新关卡显示
            UpdateLevelDisplay();
            Debug.Log($"[TopPanel] 关卡重启，更新关卡显示: {GameDataManager.GetLevelNum()}");
        }

        /// <summary>
        /// 响应关卡开始事件
        /// </summary>
        private void OnLevelStarted()
        {
            // 更新关卡显示
            UpdateLevelDisplay();
            Debug.Log($"[TopPanel] 关卡开始，更新关卡显示: {GameDataManager.GetLevelNum()}");
        }

        /// <summary>
        /// 响应货币类型变化事件（语言变化会间接触发此事件）
        /// </summary>
        private void OnCurrencyTypeChanged(CurrencyType newCurrencyType)
        {
            Debug.Log($"[TopPanel] 检测到货币类型变化: {newCurrencyType}");

            // 更新货币显示（包括符号和数值）
            UpdateCurrencyDisplay();
            UpdateCurrencyIcon();

            Debug.Log($"[TopPanel] 货币类型更新完成: {newCurrencyType}");
        }

        /// <summary>
        /// 响应货币滚动动画请求
        /// </summary>
        private void OnCurrencyRollRequested()
        {
            if (!enableCurrencyRollAnimation)
                return;

            // 获取当前显示的值作为起始值
            int startValue = displayedCurrencyInt > 0 ? displayedCurrencyInt : currentCurrencyInt;

            // 从CurrencyManager获取最新的实际值作为目标值
            int targetValue = 0;
            if (CurrencyManager.Instance != null && CurrencyManager.Instance.IsInitialized)
            {
                targetValue = CurrencyManager.Instance.CurrentCoins;
                // 立即更新内部当前值，确保数据一致性
                currentCurrencyInt = targetValue;
            }

            // 如果起始值和目标值相同，不需要播放动画
            if (startValue == targetValue)
            {
                Debug.Log($"[TopPanel] 货币值未变化，跳过滚动动画: {startValue}");
                // 确保显示值同步
                displayedCurrencyInt = targetValue;
                return;
            }


            // 如果有延迟，先等待
            if (anidelay > 0)
            {
                StartCoroutine(DelayedCurrencyRoll(startValue, targetValue, currencyRollDuration, anidelay));
            }
            else
            {
                // 立即播放动画
                PlayCurrencyRollAnimation(startValue, targetValue, currencyRollDuration);
            }

            Debug.Log($"[TopPanel] 响应货币滚动请求: {startValue} -> {targetValue}, " +
                      $"时长: {currencyRollDuration:F1}秒, 延迟: {anidelay:F1}秒");
        }

        /// <summary>
        /// 延迟播放货币滚动动画
        /// </summary>
        private System.Collections.IEnumerator DelayedCurrencyRoll(int startValue, int targetValue, float duration, float delay)
        {
            yield return new UnityEngine.WaitForSeconds(delay);
            PlayCurrencyRollAnimation(startValue, targetValue, duration);
        }

        #endregion

        #region 公开接口

        /// <summary>
        /// 获取当前关卡
        /// </summary>
        public int GetCurrentLevel()
        {
            return currentLevel;
        }

        /// <summary>
        /// 获取当前货币（内部整数值）
        /// </summary>
        public int GetCurrentCurrency()
        {
            // 始终从CurrencyManager获取最新值，确保数据一致性
            if (CurrencyManager.Instance != null && CurrencyManager.Instance.IsInitialized)
            {
                currentCurrencyInt = CurrencyManager.Instance.GetCoins();
            }
            return currentCurrencyInt;
        }

        /// <summary>
        /// 获取当前货币（显示用美元值）
        /// </summary>
        public float GetCurrentCurrencyDisplay()
        {
            return CurrencyFormatter.ToDisplayValue(GetCurrentCurrency());
        }

        /// <summary>
        /// 刷新UI（强制更新）
        /// </summary>
        public void RefreshUI()
        {
            UpdateUI();
        }

        /// <summary>
        /// 设置提现按钮的可用性
        /// </summary>
        public void SetWithdrawButtonInteractable(bool interactable)
        {
            if (withDrawButton != null)
            {
                withDrawButton.interactable = interactable;
            }
        }


        public Transform GetCurrencyTextTransForm()
        {
            return currencyText != null ? currencyText.transform : null;
        }

        /// <summary>
        /// 播放货币数字滚动动画
        /// </summary>
        /// <param name="startValue">起始值（内部整数值）</param>
        /// <param name="endValue">目标值（内部整数值）</param>
        /// <param name="duration">动画时长（可选，使用负值则使用默认配置）</param>
        public void PlayCurrencyRollAnimation(int startValue, int endValue, float duration = -1f)
        {
            // 检查白包模式
            bool isWhitePackageMode = false;
            if (NativeBridgeManager.Instance != null)
            {
                isWhitePackageMode = NativeBridgeManager.Instance.IsWhitePackage();
            }

            // 白包模式下不播放滚动动画
            if (isWhitePackageMode)
            {
                Debug.Log("[TopPanel] 白包模式：跳过货币滚动动画");
                SetCurrencyText(endValue); // 直接设置最终值（会显示为空）
                return;
            }

            if (!enableCurrencyRollAnimation || currencyText == null)
            {
                // 如果禁用动画，直接设置最终值并同步内部状态
                if (CurrencyManager.Instance != null && CurrencyManager.Instance.IsInitialized)
                {
                    currentCurrencyInt = CurrencyManager.Instance.GetCoins();
                    displayedCurrencyInt = currentCurrencyInt;
                    SetCurrencyText(currentCurrencyInt);
                }
                else
                {
                    SetCurrencyText(endValue);
                }
                return;
            }

            // 停止当前的滚动动画
            if (currentRollTween != null && currentRollTween.IsActive())
            {
                currentRollTween.Kill();
                // 立即同步当前实际值
                if (CurrencyManager.Instance != null && CurrencyManager.Instance.IsInitialized)
                {
                    currentCurrencyInt = CurrencyManager.Instance.GetCoins();
                }
            }

            // 使用配置的时长或传入的时长
            float animDuration = duration > 0 ? duration : currencyRollDuration;

            // 设置起始值
            displayedCurrencyInt = startValue;
            SetCurrencyText(startValue);

            //开始滚动时，执行addText的显示动画
            //addText显示增加的数量，从下向上浮动出现，等待1s后隐藏并重置位置
            if (addText != null)
            {
                int difference = endValue - startValue;
                Debug.Log($"[TopPanel] Currency difference: {difference} (from {startValue} to {endValue})");

                if (difference > 0)
                {
                    Debug.Log($"[TopPanel] Showing addText with value: +{CurrencyFormatter.ToDisplayValue(difference)}");

                    // 停止之前的动画并确保文本隐藏
                    if (addTextSequence != null && addTextSequence.IsActive())
                    {
                        addTextSequence.Kill(true); // 传入true确保执行OnComplete回调
                        addTextSequence = null; // 清空引用
                    }

                    // 确保从干净的状态开始
                    if (addText != null)
                    {
                        addText.gameObject.SetActive(false);
                    }

                    // 设置文本内容和样式
                    addText.text = $"+{CurrencyFormatter.ToDisplayValue(difference):F2}";


                    // 保存初始位置
                    Vector3 originalPosition = addText.transform.localPosition;
                    Vector3 startPosition = originalPosition + new Vector3(0, -20, 0); // 初始位置在下方20单位（减少偏移量）

                    Debug.Log($"[TopPanel] AddText original position: {originalPosition}, start position: {startPosition}");
                    Transform parentTransform = addText.transform.parent;
                    Debug.Log($"[TopPanel] AddText parent: {(parentTransform != null ? parentTransform.name : "None")}, active: {addText.gameObject.activeInHierarchy}");

                    // 重置位置和透明度
                    addText.transform.localPosition = startPosition;
                    addText.gameObject.SetActive(true);
                    Debug.Log($"[TopPanel] AddText activated at position: {startPosition}");

                    // 检查并记录文本的其他属性
                    RectTransform addTextRect = addText.GetComponent<RectTransform>();
                    Debug.Log($"[TopPanel] AddText RectTransform - Size: {addTextRect.sizeDelta}, Scale: {addTextRect.localScale}, AnchoredPosition: {addTextRect.anchoredPosition}");
                    Debug.Log($"[TopPanel] AddText Text properties - Font Size: {addText.fontSize}, Color: {addText.color}");

                    // 设置初始透明度为0
                    Color textColor = addText.color;
                    textColor.a = 0f;
                    addText.color = textColor;

                    // 创建动画序列
                    addTextSequence = DOTween.Sequence();
                    addTextSequence.SetAutoKill(true); // 确保动画完成后自动销毁

                    // 阶段1：淡入并向上移动（0.8秒，原0.5秒）
                    addTextSequence.Append(
                        addText.transform.DOLocalMove(originalPosition, 0.8f)
                            .SetEase(Ease.OutBack)
                    );
                    addTextSequence.Join(
                        addText.DOFade(1f, 0.5f)  // 淡入时间延长到0.5秒（原0.3秒）
                            .SetEase(Ease.OutQuad)
                    );

                    // 阶段2：保持显示（2秒，原1秒）
                    addTextSequence.AppendInterval(2f);

                    // 阶段3：淡出并继续上移（0.8秒，原0.5秒）
                    addTextSequence.Append(
                        addText.transform.DOLocalMove(originalPosition + new Vector3(0, 15, 0), 0.8f) // 上移15单位（原10单位）
                            .SetEase(Ease.InQuad)
                    );
                    addTextSequence.Join(
                        addText.DOFade(0f, 0.8f)  // 淡出时间延长到0.8秒（原0.5秒）
                            .SetEase(Ease.InQuad)
                    );

                    // 动画完成后的清理
                    addTextSequence.OnComplete(() => {
                        if (addText != null && addText.gameObject != null)
                        {
                            addText.gameObject.SetActive(false);
                            addText.transform.localPosition = originalPosition; // 重置位置

                            // 重置透明度为1，以备下次使用
                            Color resetColor = addText.color;
                            resetColor.a = 1f;
                            addText.color = resetColor;

                            Debug.Log($"[TopPanel] AddText animation completed and hidden at {Time.time}");
                        }
                        else
                        {
                            Debug.LogWarning("[TopPanel] AddText is null when trying to hide");
                        }
                    });

                    // 如果动画被中断也要确保隐藏
                    addTextSequence.OnKill(() => {
                        if (addText != null && addText.gameObject != null && addText.gameObject.activeSelf)
                        {
                            addText.gameObject.SetActive(false);
                            Debug.Log("[TopPanel] AddText animation killed and hidden");
                        }
                    });

                    Debug.Log($"[TopPanel] AddText animation sequence started (duration: ~3.6s)");
                }
                else if (difference < 0)
                {
                    Debug.Log($"[TopPanel] Currency decreased by {difference}, not showing addText");
                }
            }
            else
            {
                Debug.LogWarning("[TopPanel] AddText is null, cannot show currency increase animation");
            }

            // 创建滚动动画
            currentRollTween = DOTween.To(
                () => displayedCurrencyInt,
                x => {
                    displayedCurrencyInt = x;
                    SetCurrencyText(x);
                },
                endValue,
                animDuration
            ).SetEase(currencyRollEase)
             .OnComplete(() => {
                 // 动画完成时，从CurrencyManager同步最新值（防止动画过程中有新的货币变化）
                 if (CurrencyManager.Instance != null && CurrencyManager.Instance.IsInitialized)
                 {
                     currentCurrencyInt = CurrencyManager.Instance.GetCoins();
                     displayedCurrencyInt = currentCurrencyInt;
                     SetCurrencyText(currentCurrencyInt);

                     Debug.Log($"[TopPanel] 动画完成，同步最新值: {currentCurrencyInt}");
                 }
                 else
                 {
                     // 如果CurrencyManager不可用，使用动画目标值
                     currentCurrencyInt = endValue;
                     displayedCurrencyInt = endValue;
                     SetCurrencyText(endValue);
                 }
             })
             .OnKill(() => {
                 // 动画被杀死时（例如RestartLevel时），确保显示正确的最终值
                 if (CurrencyManager.Instance != null && CurrencyManager.Instance.IsInitialized)
                 {
                     currentCurrencyInt = CurrencyManager.Instance.GetCoins();
                     displayedCurrencyInt = currentCurrencyInt;
                     SetCurrencyText(currentCurrencyInt);

                     Debug.Log($"[TopPanel] 动画被中断，强制同步最新值: {currentCurrencyInt}");
                 }
                 else
                 {
                     // 如果CurrencyManager不可用，至少设置到目标值
                     currentCurrencyInt = endValue;
                     displayedCurrencyInt = endValue;
                     SetCurrencyText(endValue);

                     Debug.Log($"[TopPanel] 动画被中断，设置到目标值: {endValue}");
                 }
             });
        }

        /// <summary>
        /// 播放货币数字滚动动画（从当前值到目标值）
        /// </summary>
        /// <param name="targetValue">目标值（内部整数值）</param>
        /// <param name="duration">动画时长（可选）</param>
        public void PlayCurrencyRollToTarget(int targetValue, float duration = -1f)
        {
            // 获取当前显示的值作为起始值
            int startValue = displayedCurrencyInt > 0 ? displayedCurrencyInt : currentCurrencyInt;
            PlayCurrencyRollAnimation(startValue, targetValue, duration);
        }

        /// <summary>
        /// 立即停止货币滚动动画
        /// </summary>
        public void StopCurrencyRollAnimation()
        {
            if (currentRollTween != null && currentRollTween.IsActive())
            {
                currentRollTween.Kill();
            }

            // 同时停止并隐藏addText动画
            if (addTextSequence != null && addTextSequence.IsActive())
            {
                addTextSequence.Kill(true); // 执行OnComplete回调
            }

            // 确保addText被隐藏
            if (addText != null && addText.gameObject != null && addText.gameObject.activeSelf)
            {
                addText.gameObject.SetActive(false);
                Debug.Log("[TopPanel] AddText manually hidden");
            }
        }

        /// <summary>
        /// 设置货币滚动动画时长
        /// </summary>
        public void SetCurrencyRollDuration(float duration)
        {
            currencyRollDuration = Mathf.Max(0.1f, duration);
        }

        /// <summary>
        /// 获取货币滚动动画时长
        /// </summary>
        public float GetCurrencyRollDuration()
        {
            return currencyRollDuration;
        }

        /// <summary>
        /// 确保addText被隐藏的备用协程
        /// </summary>
        private System.Collections.IEnumerator EnsureAddTextHidden(float delay)
        {
            yield return new UnityEngine.WaitForSeconds(delay);

            if (addText != null && addText.gameObject != null && addText.gameObject.activeSelf)
            {
                addText.gameObject.SetActive(false);
                Debug.LogWarning($"[TopPanel] AddText was still visible after {delay}s, force hiding it");
            }
        }
        #endregion

        #region Editor Testing

#if UNITY_EDITOR
        [Header("Editor Testing")]
        [SerializeField]
        private int testCurrencyValue = 12345678; // 测试用货币值（内部值）

        [ContextMenu("Test Large Number Display")]
        private void TestLargeNumberDisplay()
        {
            SetCurrencyText(testCurrencyValue);
            Debug.Log($"[TopPanel] Testing currency display with value: {testCurrencyValue}");
        }

        [ContextMenu("Test Normal Display")]
        private void TestNormalDisplay()
        {
            bool originalSetting = useLargeNumberDisplay;
            useLargeNumberDisplay = false;
            SetCurrencyText(testCurrencyValue);
            useLargeNumberDisplay = originalSetting;
            Debug.Log($"[TopPanel] Normal display test with value: {testCurrencyValue}");
        }

        [ContextMenu("Test Compact Display")]
        private void TestCompactDisplay()
        {
            bool originalSetting = useCompactDisplay;
            useCompactDisplay = true;
            SetCurrencyText(testCurrencyValue);
            useCompactDisplay = originalSetting;
            Debug.Log($"[TopPanel] Compact display test with value: {testCurrencyValue}");
        }

        [ContextMenu("Test Different Values")]
        private void TestDifferentValues()
        {
            int[] testValues = new int[]
            {
                100,        // 0.01
                10000,      // 1.00
                100000,     // 10.00
                1000000,    // 100.00
                10000000,   // 1,000.00 (1K)
                55000000,   // 5,500.00 (5.5K)
                100000000,  // 10,000.00 (10K)
                1234567890, // 123,456.789 (123K)
                int.MaxValue // Max value test
            };

            Debug.Log($"[TopPanel] Testing different currency values:");
            Debug.Log($"Current Settings - LargeNumber: {useLargeNumberDisplay}, Compact: {useCompactDisplay}, Threshold: {largeNumberThreshold}");

            foreach (int value in testValues)
            {
                SetCurrencyText(value);
                float displayValue = CurrencyFormatter.ToDisplayValue(value);
                Debug.Log($"  Value: {value} (${displayValue:N2}) -> Display: {currencyText.text}");
            }
        }
#endif

        #endregion
    }
}