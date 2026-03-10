// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using DG.Tweening;

namespace BlockPuzzleGameToolkit.Scripts.Utils
{
    /// <summary>
    /// 通用工具类
    /// 提供常用的类型转换、数值格式化、字典操作等工具方法
    /// </summary>
    public static class Utilities
    {
        #region Type Casting Methods

        /// <summary>
        /// 安全地将对象转换为int类型
        /// </summary>
        public static int CastValueInt(object o, int defaultValue = 0)
        {
            if (o == null) return defaultValue;

            if (o is int) return (int)o;
            if (o is long) return (int)(long)o;
            if (o is double) return (int)(double)o;
            if (o is float) return (int)(float)o;

            if (int.TryParse(o.ToString(), out int result))
            {
                return result;
            }

            return defaultValue;
        }

        /// <summary>
        /// 安全地将对象转换为bool类型
        /// </summary>
        public static bool CastValueBool(object o, bool defaultValue = false)
        {
            if (o == null) return defaultValue;

            if (o is bool) return (bool)o;
            if (o is int) return ((int)o) > 0;
            if (o is long) return ((long)o) > 0;
            if (o is double) return ((double)o) > 0;
            if (o is float) return ((float)o) > 0;

            if (bool.TryParse(o.ToString(), out bool result))
            {
                return result;
            }

            return defaultValue;
        }

        /// <summary>
        /// 安全地将对象转换为float类型
        /// </summary>
        public static float CastValueFloat(object o, float defaultValue = 0f)
        {
            if (o == null) return defaultValue;

            if (o is float) return (float)o;
            if (o is int) return (float)(int)o;
            if (o is long) return (float)(long)o;
            if (o is double) return (float)(double)o;

            if (float.TryParse(o.ToString(), out float result))
            {
                return result;
            }

            return defaultValue;
        }

        /// <summary>
        /// 安全地将对象转换为long类型
        /// </summary>
        public static long CastValueLong(object o, long defaultValue = 0)
        {
            if (o == null) return defaultValue;

            if (o is long) return (long)o;
            if (o is int) return (long)(int)o;
            if (o is double) return (long)(double)o;
            if (o is float) return (long)(float)o;

            if (long.TryParse(o.ToString(), out long result))
            {
                return result;
            }

            return defaultValue;
        }

        /// <summary>
        /// 安全地将对象转换为double类型
        /// </summary>
        public static double CastValueDouble(object o, double defaultValue = 0)
        {
            if (o == null) return defaultValue;

            if (o is double) return (double)o;
            if (o is int) return (double)(int)o;
            if (o is long) return (double)(long)o;
            if (o is float) return (double)(float)o;

            if (double.TryParse(o.ToString(), out double result))
            {
                return result;
            }

            return defaultValue;
        }

        #endregion

        #region Dictionary Helper Methods

        /// <summary>
        /// 从字典中获取float值
        /// </summary>
        public static float GetFloat(Dictionary<string, object> dict, string key, float defaultValue = 0f)
        {
            if (dict != null && dict.ContainsKey(key))
            {
                return CastValueFloat(dict[key], defaultValue);
            }
            return defaultValue;
        }

        /// <summary>
        /// 从字典中获取double值
        /// </summary>
        public static double GetDouble(Dictionary<string, object> dict, string key, double defaultValue = 0)
        {
            if (dict != null && dict.ContainsKey(key))
            {
                return CastValueDouble(dict[key], defaultValue);
            }
            return defaultValue;
        }

        /// <summary>
        /// 从字典中获取long值
        /// </summary>
        public static long GetLong(Dictionary<string, object> dict, string key, long defaultValue = 0)
        {
            if (dict != null && dict.ContainsKey(key))
            {
                return CastValueLong(dict[key], defaultValue);
            }
            return defaultValue;
        }

        /// <summary>
        /// 从字典中获取int值
        /// </summary>
        public static int GetInt(Dictionary<string, object> dict, string key, int defaultValue = 0)
        {
            if (dict != null && dict.ContainsKey(key))
            {
                return CastValueInt(dict[key], defaultValue);
            }
            return defaultValue;
        }

        /// <summary>
        /// 从字典中获取bool值
        /// </summary>
        public static bool GetBool(Dictionary<string, object> dict, string key, bool defaultValue = false)
        {
            if (dict != null && dict.ContainsKey(key))
            {
                return CastValueBool(dict[key], defaultValue);
            }
            return defaultValue;
        }

        /// <summary>
        /// 从字典中获取string值
        /// </summary>
        public static string GetString(Dictionary<string, object> dict, string key, string defaultValue = "")
        {
            if (dict != null && dict.ContainsKey(key) && dict[key] != null)
            {
                return dict[key].ToString();
            }
            return defaultValue;
        }

        /// <summary>
        /// 从字典中获取泛型值
        /// </summary>
        public static T GetValue<T>(Dictionary<string, object> dict, string key, T defaultValue)
        {
            if (dict != null && dict.ContainsKey(key))
            {
                try
                {
                    return (T)dict[key];
                }
                catch (Exception)
                {
                    return defaultValue;
                }
            }
            return defaultValue;
        }

        #endregion

        #region String List Conversion

        /// <summary>
        /// 将逗号分隔的字符串转换为List
        /// </summary>
        public static List<T> StrToList<T>(string str)
        {
            List<T> lists = new List<T>();
            if (!string.IsNullOrEmpty(str))
            {
                string[] strs = str.Split(',');
                foreach (string s in strs)
                {
                    if (typeof(T) == typeof(int))
                    {
                        int n = int.Parse(s);
                        lists.Add((T)(object)n);
                    }
                    else if (typeof(T) == typeof(float))
                    {
                        float n = float.Parse(s);
                        lists.Add((T)(object)n);
                    }
                    else if (typeof(T) == typeof(string))
                    {
                        lists.Add((T)(object)s);
                    }
                    else if (typeof(T) == typeof(bool))
                    {
                        bool n = bool.Parse(s);
                        lists.Add((T)(object)n);
                    }
                }
            }
            return lists;
        }

        /// <summary>
        /// 将List转换为逗号分隔的字符串
        /// </summary>
        public static string ListToStr<T>(List<T> list)
        {
            if (list == null || list.Count == 0)
                return string.Empty;

            return string.Join(",", list.Select(x => x.ToString()).ToArray());
        }

        #endregion

        #region Number Formatting

        /// <summary>
        /// 千分位分隔符格式化
        /// </summary>
        public static string ThousandSeparator(long number)
        {
            if (number > 99)
            {
                return string.Format("{0:N0}", number);
            }
            return number.ToString();
        }

        /// <summary>
        /// 大数字缩写显示 (K, M, B, T, Q)
        /// </summary>
        public static string GetBigNumberShow(long num, float ignoreNumber = 1000, int saveDecimal = 1)
        {
            double tmp = Convert.ToDouble(num);
            double thousand = tmp / 1000f;

            if (tmp < ignoreNumber)
            {
                return ThousandSeparator((long)tmp);
            }
            else if (thousand < 1000)
            {
                double thousandShow = Math.Round(thousand, saveDecimal);
                return thousandShow + "K";
            }
            else if (thousand < 1000000)
            {
                double million = Math.Round(thousand / 1000, saveDecimal);
                return million + "M";
            }
            else if (thousand < 1000000000)
            {
                double billion = Math.Round(thousand / 1000000, saveDecimal);
                return billion + "B";
            }
            else if (thousand < 1000000000000)
            {
                double trillion = Math.Round(thousand / 1000000000, saveDecimal);
                return trillion + "T";
            }
            else
            {
                double quadrillion = Math.Round(thousand / 1000000000000, saveDecimal);
                return quadrillion + "Q";
            }
        }

        /// <summary>
        /// 获取百分比字符串
        /// </summary>
        public static string GetPercentageString(float value, int decimalPlaces = 0)
        {
            var provider = new System.Globalization.NumberFormatInfo
            {
                PercentDecimalDigits = decimalPlaces,
                PercentPositivePattern = 1
            };
            return value.ToString("P", provider);
        }

        #endregion

        #region Math Utilities

        /// <summary>
        /// 四舍五入取整
        /// </summary>
        public static int GetRoundInt(float value)
        {
            return Convert.ToInt32(Math.Round(value, MidpointRounding.AwayFromZero));
        }

        /// <summary>
        /// 根据类型取整
        /// </summary>
        public static double GetRoundByType(double number, int type)
        {
            switch (type)
            {
                case 0: // 四舍五入
                    return Math.Round(number);
                case 1: // 向下取整
                    return Math.Floor(number);
                case 2: // 向上取整
                    return Math.Ceiling(number);
                default:
                    return number;
            }
        }

        /// <summary>
        /// 限制long值在最小和最大值之间
        /// </summary>
        public static long ClampLong(long curValue, long minValue, long maxValue)
        {
            if (curValue <= minValue) return minValue;
            if (curValue >= maxValue) return maxValue;
            return curValue;
        }

        #endregion

        #region Animation Utilities

        /// <summary>
        /// 数值动画过渡 (long类型)
        /// </summary>
        public static Tweener AnimationTo(long from, long targetNum, float duration,
            Action<long> updateCB, Action startCB = null, Action finishCB = null)
        {
            bool bIncrease = (from < targetNum);
            return DOTween.To(() => from, x => from = x, targetNum, duration)
                .OnStart(() => startCB?.Invoke())
                .OnUpdate(() =>
                {
                    if (bIncrease)
                        updateCB?.Invoke(Math.Min(from, targetNum));
                    else
                        updateCB?.Invoke(Math.Max(from, targetNum));
                })
                .OnComplete(() =>
                {
                    updateCB?.Invoke(targetNum);
                    finishCB?.Invoke();
                })
                .SetUpdate(true);
        }

        /// <summary>
        /// 数值动画过渡 (int类型)
        /// </summary>
        public static Tweener AnimationTo(int from, int targetNum, float duration,
            Action<int> updateCB, Action startCB = null, Action finishCB = null)
        {
            bool bIncrease = (from < targetNum);
            return DOTween.To(() => from, x => from = x, targetNum, duration)
                .OnStart(() => startCB?.Invoke())
                .OnUpdate(() =>
                {
                    if (bIncrease)
                        updateCB?.Invoke(Math.Min(from, targetNum));
                    else
                        updateCB?.Invoke(Math.Max(from, targetNum));
                })
                .OnComplete(() =>
                {
                    updateCB?.Invoke(targetNum);
                    finishCB?.Invoke();
                })
                .SetUpdate(true);
        }

        /// <summary>
        /// 数值动画过渡 (double类型，带缓动)
        /// </summary>
        public static Tweener AnimationToEase(double from, double targetNum, float duration,
            Action<double> updateCB, Action startCB = null, Action finishCB = null, Ease easeType = Ease.Linear)
        {
            bool bIncrease = (from < targetNum);
            return DOTween.To(() => from, x => from = x, targetNum, duration)
                .OnStart(() => startCB?.Invoke())
                .OnUpdate(() =>
                {
                    if (bIncrease)
                        updateCB?.Invoke(Math.Min(from, targetNum));
                    else
                        updateCB?.Invoke(Math.Max(from, targetNum));
                })
                .OnComplete(() =>
                {
                    updateCB?.Invoke(targetNum);
                    finishCB?.Invoke();
                })
                .SetEase(easeType)
                .SetUpdate(true);
        }

        #endregion

        #region Validation Utilities

        /// <summary>
        /// 检查对象是否为数字类型
        /// </summary>
        public static bool IsNumber(object o)
        {
            if (o == null) return false;
            return (o is int) || (o is long) || (o is double) || (o is float);
        }

        /// <summary>
        /// 检查字符串是否为整数
        /// </summary>
        public static bool IsInteger(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            Regex reg = new Regex("^[0-9]+$");
            Match ma = reg.Match(text);
            return ma.Success;
        }

        #endregion

        #region Collection Utilities

        /// <summary>
        /// 随机打乱List顺序
        /// </summary>
        public static List<T> RandomSortList<T>(List<T> listT)
        {
            if (listT == null) return new List<T>();

            System.Random ran = new System.Random();
            List<T> newList = new List<T>();
            foreach (T item in listT)
            {
                newList.Insert(ran.Next(newList.Count + 1), item);
            }
            return newList;
        }

        /// <summary>
        /// 将object转换为int字典
        /// </summary>
        public static Dictionary<int, int> CastObjToIntDict(object o)
        {
            Dictionary<int, int> ret = new Dictionary<int, int>();
            if (o is Dictionary<string, object> dict)
            {
                foreach (KeyValuePair<string, object> keyValue in dict)
                {
                    int key = CastValueInt(keyValue.Key);
                    int v = CastValueInt(keyValue.Value);
                    ret[key] = v;
                }
            }
            return ret;
        }

        /// <summary>
        /// 将object转换为int列表
        /// </summary>
        public static List<int> CastObjToIntList(object o)
        {
            List<int> ret = new List<int>();
            if (o is List<object> list)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    ret.Add(CastValueInt(list[i]));
                }
            }
            return ret;
        }

        /// <summary>
        /// 将object转换为float列表
        /// </summary>
        public static List<float> CastObjToFloatList(object o)
        {
            List<float> ret = new List<float>();
            if (o is List<object> list)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    ret.Add(CastValueFloat(list[i]));
                }
            }
            return ret;
        }

        #endregion

        #region Unity Extensions

        /// <summary>
        /// 获取或添加组件
        /// </summary>
        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            if (go == null) return null;
            T component = go.GetComponent<T>();
            if (component == null)
            {
                component = go.AddComponent<T>();
            }
            return component;
        }

        /// <summary>
        /// 获取或添加组件 (Transform版本)
        /// </summary>
        public static T GetOrAddComponent<T>(this Transform trans) where T : Component
        {
            if (trans == null) return null;
            return trans.gameObject.GetOrAddComponent<T>();
        }

        /// <summary>
        /// 查找子对象并获取组件
        /// </summary>
        /// <param name="trans">当前transform</param>
        /// <param name="path">子对象路径</param>
        /// <typeparam name="T">组件类型</typeparam>
        /// <returns>找到的组件，如果未找到则返回null</returns>
        public static T RealFindObj<T>(this Transform trans, string path) where T : Component
        {
            if (trans == null) return null;

            // 查找子对象
            Transform childTransform = trans.Find(path);
            if (childTransform == null)
            {
                Debug.LogWarning($"[RealFindObj] 未找到子对象: {path}");
                return null;
            }

            // 获取或添加组件
            return childTransform.GetOrAddComponent<T>();
        }

        /// <summary>
        /// 查找子对象（仅查找，不添加组件）
        /// </summary>
        /// <param name="trans">当前transform</param>
        /// <param name="path">子对象路径</param>
        /// <typeparam name="T">组件类型</typeparam>
        /// <returns>找到的组件，如果未找到则返回null</returns>
        public static T FindComponent<T>(this Transform trans, string path) where T : Component
        {
            if (trans == null) return null;

            Transform childTransform = trans.Find(path);
            if (childTransform == null)
            {
                return null;
            }

            return childTransform.GetComponent<T>();
        }

        /// <summary>
        /// 递归查找子对象
        /// </summary>
        /// <param name="trans">当前transform</param>
        /// <param name="name">要查找的对象名称</param>
        /// <returns>找到的Transform，如果未找到则返回null</returns>
        public static Transform FindDeepChild(this Transform trans, string name)
        {
            if (trans == null) return null;

            // 先尝试直接查找
            Transform result = trans.Find(name);
            if (result != null) return result;

            // 递归查找所有子对象
            for (int i = 0; i < trans.childCount; i++)
            {
                result = trans.GetChild(i).FindDeepChild(name);
                if (result != null) return result;
            }

            return null;
        }

        #endregion

        #region DateTime Extensions

        /// <summary>
        /// 获取Unix时间戳（秒）
        /// </summary>
        public static long TotalSeconds(this DateTime dateTime)
        {
            TimeSpan t = dateTime - new DateTime(1970, 1, 1);
            return (long)t.TotalSeconds;
        }

        #endregion

        #region Logging Utilities

        /// <summary>
        /// 带颜色的日志输出（仅在编辑器和调试模式）
        /// </summary>
        public static void LogInfo(string msg, string color = "green")
        {
#if UNITY_EDITOR || DEBUG
            Debug.Log($"<color={color}>{msg}</color>");
#endif
        }

        /// <summary>
        /// 错误日志输出（仅在编辑器和调试模式）
        /// </summary>
        public static void LogError(string errorMessage, string errorType = "")
        {
#if UNITY_EDITOR || DEBUG
            Debug.LogError($"[{errorType}] {errorMessage}");
#endif
        }

        #endregion

        #region Nearest Value Utilities

        /// <summary>
        /// 从列表中查找最接近的目标值
        /// </summary>
        public static long GetNearestValue(long targetValue, List<long> list, NearestType type = NearestType.None)
        {
            if (list == null || list.Count == 0)
            {
                throw new ArgumentException("List cannot be null or empty");
            }

            int count = list.Count;

            // 如果目标值小于等于第一个值
            if (targetValue <= list[0])
                return list[0];

            // 如果目标值大于等于最后一个值
            if (targetValue >= list[count - 1])
                return list[count - 1];

            // 在列表中查找位置
            for (int i = 1; i < list.Count; i++)
            {
                if (targetValue > list[i - 1] && targetValue <= list[i])
                {
                    switch (type)
                    {
                        case NearestType.None:
                            // 返回最近的值
                            return (targetValue - list[i - 1]) < (list[i] - targetValue)
                                ? list[i - 1]
                                : list[i];
                        case NearestType.GreaterThanTarget:
                            // 返回大于目标的值
                            return list[i];
                        case NearestType.LessThanTarget:
                            // 返回小于目标的值
                            return list[i - 1];
                    }
                    break;
                }
            }

            return list[0];
        }

        #endregion
    }

    /// <summary>
    /// 最近值查找类型
    /// </summary>
    public enum NearestType
    {
        None,               // 返回最接近的值
        GreaterThanTarget,  // 返回大于目标的值
        LessThanTarget      // 返回小于目标的值
    }
}