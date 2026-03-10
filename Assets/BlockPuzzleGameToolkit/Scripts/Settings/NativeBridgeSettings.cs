using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Settings
{
    /// <summary>
    /// NativeBridge 配置设置
    /// 用于配置Unity与原生平台交互的参数
    /// </summary>
    [CreateAssetMenu(fileName = "NativeBridgeSettings", menuName = "Block Puzzle/Settings/Native Bridge Settings")]
    public class NativeBridgeSettings : ScriptableObject
    {
        [Header("General Settings")]
        [Tooltip("是否启用 Native Bridge 功能")]
        public bool enableNativeBridge = true;

        [Header("Android Configuration")]
        [Tooltip("Android 原生类的完整包名路径")]
        public string androidPackageName = "com.blockpuzzle.game.NativeBridge";

        [Tooltip("Android 原生方法名")]
        public string androidMethodName = "callUnity";

        [Header("iOS Configuration")]
        [Tooltip("iOS 原生方法名")]
        public string iOSMethodName = "callNative";

        [Header("Debug Settings")]
        [Tooltip("是否在控制台输出调试日志")]
        public bool enableDebugLogs = true;

        [Tooltip("是否在编辑器中模拟原生响应")]
        public bool mockResponseInEditor = true;

        [Header("Initialization")]
        [Tooltip("是否在启动时自动初始化")]
        public bool autoInitialize = true;

        [Tooltip("初始化时自动调用的接口")]
        public bool autoRequestCommonParams = true;
        public bool autoRequestWhiteBao = true;
        public bool autoRequestCurrency = true;

        [Header("Timeout Settings")]
        [Tooltip("原生调用超时时间（秒）")]
        [Range(1f, 30f)]
        public float nativeCallTimeout = 5f;

        /// <summary>
        /// 验证设置的有效性
        /// </summary>
        public bool ValidateSettings()
        {
            if (string.IsNullOrEmpty(androidPackageName))
            {
                Debug.LogError("[NativeBridgeSettings] Android package name is empty!");
                return false;
            }

            if (string.IsNullOrEmpty(androidMethodName))
            {
                Debug.LogError("[NativeBridgeSettings] Android method name is empty!");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 获取平台特定的方法名
        /// </summary>
        public string GetPlatformMethodName()
        {
#if UNITY_ANDROID
            return androidMethodName;
#elif UNITY_IOS
            return iOSMethodName;
#else
            return "";
#endif
        }

        /// <summary>
        /// 输出调试日志
        /// </summary>
        public void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[NativeBridge] {message}");
            }
        }

        /// <summary>
        /// 输出警告日志
        /// </summary>
        public void LogWarning(string message)
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning($"[NativeBridge] {message}");
            }
        }

        /// <summary>
        /// 输出错误日志
        /// </summary>
        public void LogError(string message)
        {
            // 错误日志始终输出
            Debug.LogError($"[NativeBridge] {message}");
        }
    }
}