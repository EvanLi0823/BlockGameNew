// 活动系统 - 日志工具
// 创建日期: 2026-03-09

using UnityEngine;
using BlockPuzzleGameToolkit.Scripts.Activity.Data;

namespace BlockPuzzleGameToolkit.Scripts.Activity.Core
{
    /// <summary>
    /// 活动系统统一日志工具
    /// 通过ActivitySettings.enableDebugLog控制日志输出
    /// </summary>
    public static class ActivityLogger
    {
        private const string LOG_PREFIX = "[ActivitySystem]";

        private static bool IsDebugEnabled()
        {
            var settings = ActivitySettings.Instance;
            return settings != null && settings.EnableDebugLog;
        }

        /// <summary>
        /// 普通日志
        /// </summary>
        public static void Log(string message)
        {
            if (IsDebugEnabled())
            {
                Debug.Log($"{LOG_PREFIX} {message}");
            }
        }

        /// <summary>
        /// 带标签的日志
        /// </summary>
        public static void Log(string tag, string message)
        {
            if (IsDebugEnabled())
            {
                Debug.Log($"{LOG_PREFIX}[{tag}] {message}");
            }
        }

        /// <summary>
        /// 警告日志（始终输出）
        /// </summary>
        public static void LogWarning(string message)
        {
            Debug.LogWarning($"{LOG_PREFIX} {message}");
        }

        /// <summary>
        /// 带标签的警告日志
        /// </summary>
        public static void LogWarning(string tag, string message)
        {
            Debug.LogWarning($"{LOG_PREFIX}[{tag}] {message}");
        }

        /// <summary>
        /// 错误日志（始终输出）
        /// </summary>
        public static void LogError(string message)
        {
            Debug.LogError($"{LOG_PREFIX} {message}");
        }

        /// <summary>
        /// 带标签的错误日志
        /// </summary>
        public static void LogError(string tag, string message)
        {
            Debug.LogError($"{LOG_PREFIX}[{tag}] {message}");
        }

        /// <summary>
        /// 事件日志
        /// </summary>
        public static void LogEvent(string eventName, string detail = null)
        {
            if (IsDebugEnabled())
            {
                string message = string.IsNullOrEmpty(detail)
                    ? $"Event: {eventName}"
                    : $"Event: {eventName} | {detail}";
                Debug.Log($"{LOG_PREFIX} {message}");
            }
        }
    }
}
