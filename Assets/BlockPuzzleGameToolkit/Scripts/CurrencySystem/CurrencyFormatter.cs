// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

using UnityEngine;
using System.Globalization;
using System.Text;

namespace BlockPuzzleGameToolkit.Scripts.CurrencySystem
{
    /// <summary>
    /// 货币格式化工具类
    /// 负责货币值的显示转换、格式化和多币种支持
    /// </summary>
    public static class CurrencyFormatter
    {
        /// <summary>
        /// 精度倍数（内部存储值与显示值的倍数）
        /// </summary>
        public const int SCALE = 10000;

        /// <summary>
        /// 默认货币符号
        /// </summary>
        public const string DEFAULT_CURRENCY_SYMBOL = "$";

        /// <summary>
        /// 将内部整数值转换为显示用的美元值
        /// </summary>
        /// <param name="internalValue">内部存储的整数值（放大10000倍）</param>
        /// <returns>实际美元值</returns>
        public static float ToDisplayValue(int internalValue)
        {
            return internalValue / (float)SCALE;
        }

        /// <summary>
        /// 将美元值转换为内部存储的整数值
        /// </summary>
        /// <param name="displayValue">显示的美元值</param>
        /// <returns>内部存储的整数值（放大10000倍）</returns>
        public static int ToInternalValue(float displayValue)
        {
            return Mathf.RoundToInt(displayValue * SCALE);
        }

        /// <summary>
        /// 格式化货币显示（使用默认美元）
        /// </summary>
        /// <param name="internalValue">内部存储的整数值</param>
        /// <param name="currencySymbol">货币符号（默认$）</param>
        /// <param name="decimalPlaces">小数位数（默认2位）</param>
        /// <returns>格式化的货币字符串</returns>
        public static string FormatCurrency(int internalValue, string currencySymbol = DEFAULT_CURRENCY_SYMBOL, int decimalPlaces = 2)
        {
            float displayValue = ToDisplayValue(internalValue);
            string format = $"F{decimalPlaces}";
            return $"{currencySymbol}{displayValue.ToString(format)}";
        }

        /// <summary>
        /// 使用指定货币类型格式化显示
        /// </summary>
        /// <param name="internalValue">内部存储的整数值（USD基准）</param>
        /// <param name="currencyType">目标货币类型</param>
        /// <param name="useExchangeRate">是否应用汇率转换</param>
        /// <returns>格式化的货币字符串</returns>
        public static string FormatCurrencyWithType(int internalValue, CurrencyType currencyType, bool useExchangeRate = true)
        {
            var currencyInfo = CurrencyInfo.CreateDefault(currencyType);

            // 转换汇率
            int convertedValue = internalValue;
            if (useExchangeRate && currencyType != CurrencyType.USD)
            {
                convertedValue = CurrencyInfo.ConvertInternalValue(internalValue, CurrencyType.USD, currencyType);
            }

            // 转换为显示值
            float displayValue = convertedValue / (float)SCALE;

            // 格式化数字
            string formattedNumber = FormatNumberWithSeparators(
                displayValue,
                currencyInfo.decimalPlaces,
                currencyInfo.thousandsSeparator,
                currencyInfo.decimalSeparator
            );

            // 组合符号和数字
            if (currencyInfo.symbolBeforeAmount)
            {
                return $"{currencyInfo.symbol}{formattedNumber}";
            }
            else
            {
                return $"{formattedNumber} {currencyInfo.symbol}";
            }
        }

        /// <summary>
        /// 格式化货币显示（不带符号）
        /// </summary>
        /// <param name="internalValue">内部存储的整数值</param>
        /// <param name="decimalPlaces">小数位数（默认2位）</param>
        /// <returns>格式化的数值字符串</returns>
        public static string FormatValue(int internalValue, int decimalPlaces = 2)
        {
            float displayValue = ToDisplayValue(internalValue);
            string format = $"F{decimalPlaces}";
            return displayValue.ToString(format);
        }

        /// <summary>
        /// 格式化货币显示（带千位分隔符）
        /// </summary>
        /// <param name="internalValue">内部存储的整数值</param>
        /// <param name="currencySymbol">货币符号</param>
        /// <returns>格式化的货币字符串（带千位分隔符）</returns>
        public static string FormatCurrencyWithSeparator(int internalValue, string currencySymbol = DEFAULT_CURRENCY_SYMBOL)
        {
            float displayValue = ToDisplayValue(internalValue);
            return $"{currencySymbol}{displayValue:N2}";
        }

        /// <summary>
        /// 格式化数字（带自定义分隔符）
        /// </summary>
        private static string FormatNumberWithSeparators(float value, int decimalPlaces, string thousandsSeparator, string decimalSeparator)
        {
            // 格式化为标准字符串
            string format = $"F{decimalPlaces}";
            string standardFormatted = value.ToString(format, CultureInfo.InvariantCulture);

            // 分离整数和小数部分
            string[] parts = standardFormatted.Split('.');
            string integerPart = parts[0];
            string decimalPart = parts.Length > 1 ? parts[1] : "";

            // 添加千位分隔符
            if (thousandsSeparator != "")
            {
                StringBuilder sb = new StringBuilder();
                int digitCount = 0;

                for (int i = integerPart.Length - 1; i >= 0; i--)
                {
                    if (digitCount > 0 && digitCount % 3 == 0 && integerPart[i] != '-')
                    {
                        sb.Insert(0, thousandsSeparator);
                    }
                    sb.Insert(0, integerPart[i]);
                    if (integerPart[i] != '-')
                    {
                        digitCount++;
                    }
                }
                integerPart = sb.ToString();
            }

            // 组合结果
            if (decimalPlaces > 0 && decimalPart.Length > 0)
            {
                return $"{integerPart}{decimalSeparator}{decimalPart}";
            }
            else
            {
                return integerPart;
            }
        }

        /// <summary>
        /// 转换内部值到指定货币
        /// </summary>
        /// <param name="internalValue">内部存储的整数值（USD基准）</param>
        /// <param name="targetCurrency">目标货币</param>
        /// <returns>转换后的内部值</returns>
        public static int ConvertToTargetCurrency(int internalValue, CurrencyType targetCurrency)
        {
            if (targetCurrency == CurrencyType.USD)
            {
                return internalValue;
            }

            return CurrencyInfo.ConvertInternalValue(internalValue, CurrencyType.USD, targetCurrency);
        }

        /// <summary>
        /// 从目标货币转换回USD内部值
        /// </summary>
        /// <param name="targetValue">目标货币的内部值</param>
        /// <param name="sourceCurrency">源货币类型</param>
        /// <returns>USD基准的内部值</returns>
        public static int ConvertFromTargetCurrency(int targetValue, CurrencyType sourceCurrency)
        {
            if (sourceCurrency == CurrencyType.USD)
            {
                return targetValue;
            }

            return CurrencyInfo.ConvertInternalValue(targetValue, sourceCurrency, CurrencyType.USD);
        }

        /// <summary>
        /// 验证内部值是否有效
        /// </summary>
        /// <param name="internalValue">内部存储的整数值</param>
        /// <returns>是否有效（非负）</returns>
        public static bool IsValidValue(int internalValue)
        {
            return internalValue >= 0;
        }

        /// <summary>
        /// 计算变化后的值
        /// </summary>
        /// <param name="currentValue">当前内部值</param>
        /// <param name="changeAmount">变化的内部值（可为负）</param>
        /// <returns>变化后的内部值</returns>
        public static int CalculateChange(int currentValue, int changeAmount)
        {
            int result = currentValue + changeAmount;
            return Mathf.Max(0, result); // 确保不为负
        }

        /// <summary>
        /// 检查是否有足够的货币
        /// </summary>
        /// <param name="currentValue">当前内部值</param>
        /// <param name="requiredValue">需要的内部值</param>
        /// <returns>是否足够</returns>
        public static bool HasSufficient(int currentValue, int requiredValue)
        {
            return currentValue >= requiredValue && requiredValue >= 0;
        }

        /// <summary>
        /// 获取简化的数值显示（K/M/B/T）- 不带货币符号
        /// </summary>
        /// <param name="internalValue">内部存储的整数值</param>
        /// <param name="decimalPlaces">小数位数（默认2位）</param>
        /// <returns>简化的数值字符串</returns>
        public static string GetSimplifiedValue(int internalValue, int decimalPlaces = 2)
        {
            float displayValue = ToDisplayValue(internalValue);
            return FormatWithSuffix(displayValue, decimalPlaces);
        }

        /// <summary>
        /// 格式化数值带后缀（K/M/B/T）
        /// </summary>
        /// <param name="value">要格式化的值</param>
        /// <param name="decimalPlaces">小数位数</param>
        /// <returns>格式化后的字符串</returns>
        private static string FormatWithSuffix(float value, int decimalPlaces)
        {
            string format = $"F{decimalPlaces}";

            if (value >= 1000000000000) // T (trillion)
            {
                return (value / 1000000000000).ToString(format) + "T";
            }
            else if (value >= 1000000000) // B (billion)
            {
                return (value / 1000000000).ToString(format) + "B";
            }
            else if (value >= 1000000) // M (million)
            {
                return (value / 1000000).ToString(format) + "M";
            }
            else if (value >= 1000) // K (thousand)
            {
                return (value / 1000).ToString(format) + "K";
            }
            else
            {
                // 小于1000直接显示
                if (decimalPlaces > 0)
                {
                    return value.ToString(format);
                }
                else
                {
                    return Mathf.FloorToInt(value).ToString();
                }
            }
        }

        /// <summary>
        /// 获取智能简化的货币显示
        /// </summary>
        /// <param name="internalValue">内部存储的整数值</param>
        /// <param name="currencyType">货币类型</param>
        /// <param name="autoDecimalPlaces">是否自动调整小数位（大数0位，小数2位）</param>
        /// <returns>简化的货币字符串</returns>
        public static string GetSmartCurrencyDisplay(int internalValue, CurrencyType currencyType = CurrencyType.USD, bool autoDecimalPlaces = true)
        {
            var currencyInfo = CurrencyInfo.CreateDefault(currencyType);

            // 应用汇率
            if (currencyType != CurrencyType.USD)
            {
                internalValue = CurrencyInfo.ConvertInternalValue(internalValue, CurrencyType.USD, currencyType);
            }

            float displayValue = ToDisplayValue(internalValue);

            // 自动决定小数位数
            int decimalPlaces;
            if (autoDecimalPlaces)
            {
                // 所有大数都显示2位小数
                if (displayValue >= 1000) // 1K及以上显示2位小数
                {
                    decimalPlaces = 2;
                }
                else // 小于1K显示货币默认小数位
                {
                    decimalPlaces = currencyInfo.decimalPlaces;
                }
            }
            else
            {
                decimalPlaces = 2; // 默认2位小数
            }

            string result = FormatWithSuffix(displayValue, decimalPlaces);

            // 添加货币符号
            if (currencyInfo.symbolBeforeAmount)
            {
                return $"{currencyInfo.symbol}{result}";
            }
            else
            {
                return $"{result} {currencyInfo.symbol}";
            }
        }

        /// <summary>
        /// 获取简化的货币显示（K/M/B/T）
        /// </summary>
        /// <param name="internalValue">内部存储的整数值</param>
        /// <param name="currencyType">货币类型</param>
        /// <returns>简化的显示字符串</returns>
        public static string GetSimplifiedDisplay(int internalValue, CurrencyType currencyType = CurrencyType.USD)
        {
            return GetSmartCurrencyDisplay(internalValue, currencyType, true);
        }

        /// <summary>
        /// 获取带自定义阈值的简化显示
        /// </summary>
        /// <param name="internalValue">内部存储的整数值</param>
        /// <param name="threshold">开始简化的阈值（默认1000）</param>
        /// <param name="includeSymbol">是否包含货币符号</param>
        /// <param name="currencyType">货币类型</param>
        /// <returns>格式化的字符串</returns>
        public static string GetSimplifiedWithThreshold(int internalValue, float threshold = 1000, bool includeSymbol = true, CurrencyType currencyType = CurrencyType.USD)
        {
            // 应用汇率转换，获得目标货币的内部值
            int convertedInternalValue = internalValue;
            if (currencyType != CurrencyType.USD)
            {
                convertedInternalValue = CurrencyInfo.ConvertInternalValue(internalValue, CurrencyType.USD, currencyType);
            }

            // 转换为目标货币的显示值
            float displayValue = ToDisplayValue(convertedInternalValue);

            // 使用转换后的值与阈值比较
            if (displayValue < threshold)
            {
                if (includeSymbol)
                {
                    return FormatCurrencyWithType(internalValue, currencyType, true);
                }
                else
                {
                    var currencyInfo = CurrencyInfo.CreateDefault(currencyType);
                    return displayValue.ToString($"F{currencyInfo.decimalPlaces}");
                }
            }

            // 高于阈值，使用简化格式
            if (includeSymbol)
            {
                return GetSmartCurrencyDisplay(internalValue, currencyType, true);
            }
            else
            {
                // 不包含符号的简化格式，使用转换后的值
                float convertedDisplayValue = ToDisplayValue(convertedInternalValue);

                // 生成简化的数值显示（K/M/B/T）
                if (convertedDisplayValue >= 1_000_000_000_000) // T (trillion)
                {
                    return $"{(convertedDisplayValue / 1_000_000_000_000):F2}T";
                }
                else if (convertedDisplayValue >= 1_000_000_000) // B (billion)
                {
                    return $"{(convertedDisplayValue / 1_000_000_000):F2}B";
                }
                else if (convertedDisplayValue >= 1_000_000) // M (million)
                {
                    return $"{(convertedDisplayValue / 1_000_000):F2}M";
                }
                else if (convertedDisplayValue >= 1_000) // K (thousand)
                {
                    return $"{(convertedDisplayValue / 1_000):F2}K";
                }
                else
                {
                    var currencyInfo = CurrencyInfo.CreateDefault(currencyType);
                    return convertedDisplayValue.ToString($"F{currencyInfo.decimalPlaces}");
                }
            }
        }

        /// <summary>
        /// 批量格式化多个值（用于排行榜等场景）
        /// </summary>
        /// <param name="values">内部值数组</param>
        /// <param name="currencyType">货币类型</param>
        /// <returns>格式化后的字符串数组</returns>
        public static string[] BatchFormatSimplified(int[] values, CurrencyType currencyType = CurrencyType.USD)
        {
            string[] results = new string[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                results[i] = GetSmartCurrencyDisplay(values[i], currencyType, true);
            }
            return results;
        }

        /// <summary>
        /// 获取紧凑格式（最小化字符数）
        /// </summary>
        /// <param name="internalValue">内部存储的整数值</param>
        /// <param name="currencyType">货币类型</param>
        /// <returns>紧凑格式字符串</returns>
        public static string GetCompactDisplay(int internalValue, CurrencyType currencyType = CurrencyType.USD)
        {
            var currencyInfo = CurrencyInfo.CreateDefault(currencyType);

            // 应用汇率
            if (currencyType != CurrencyType.USD)
            {
                internalValue = CurrencyInfo.ConvertInternalValue(internalValue, CurrencyType.USD, currencyType);
            }

            float displayValue = ToDisplayValue(internalValue);
            string result;

            // 超紧凑格式，大数不显示小数，小数显示两位
            if (displayValue >= 1000000000000) // T
            {
                result = (displayValue / 1000000000000).ToString("F2") + "T";
            }
            else if (displayValue >= 1000000000) // B
            {
                result = (displayValue / 1000000000).ToString("F2") + "B";
            }
            else if (displayValue >= 1000000) // M
            {
                result = (displayValue / 1000000).ToString("F2") + "M";
            }
            else if (displayValue >= 1000) // K
            {
                result = (displayValue / 1000).ToString("F2") + "K";
            }
            else
            {
                // 小于1000，根据货币类型决定
                if (currencyInfo.decimalPlaces > 0)
                {
                    result = displayValue.ToString($"F{currencyInfo.decimalPlaces}");
                }
                else
                {
                    result = Mathf.FloorToInt(displayValue).ToString();
                }
            }

            // 紧凑符号（使用缩写或更小的符号）
            string compactSymbol = GetCompactSymbol(currencyInfo.symbol);
            if (currencyInfo.symbolBeforeAmount)
            {
                return compactSymbol + result;
            }
            else
            {
                return result + compactSymbol;
            }
        }

        /// <summary>
        /// 获取紧凑的货币符号
        /// </summary>
        private static string GetCompactSymbol(string fullSymbol)
        {
            // 对于某些货币使用更短的符号
            switch (fullSymbol)
            {
                case "HK$":
                case "NT$":
                case "S$":
                case "A$":
                case "C$":
                case "Mex$":
                case "R$":
                    return "$";
                case "Rp":
                    return "R";
                case "RM":
                    return "M";
                default:
                    return fullSymbol;
            }
        }

        /// <summary>
        /// 获取自适应的货币显示格式（自动获取当前货币类型）
        /// 根据配置参数自动选择最合适的显示方式
        /// </summary>
        /// <param name="internalValue">内部存储的整数值</param>
        /// <param name="displaySettings">显示设置</param>
        /// <returns>格式化的货币字符串</returns>
        public static string GetAdaptiveDisplay(int internalValue, CurrencyDisplaySettings displaySettings)
        {
            // 检查白包模式
            bool isWhitePackage = false;
            if (BlockPuzzle.NativeBridge.NativeBridgeManager.Instance != null)
            {
                isWhitePackage = BlockPuzzle.NativeBridge.NativeBridgeManager.Instance.IsWhitePackage();
            }

            // 白包模式下只返回纯数值
            if (isWhitePackage)
            {
                // 白包模式下不显示货币符号，只显示数值
                return FormatValue(internalValue, 3);
            }

            // 标准模式的原有逻辑
            // 自动获取当前货币类型
            CurrencyType currentCurrency = CurrencyType.USD;
            if (ExchangeRateManager.Instance != null)
            {
                currentCurrency = ExchangeRateManager.Instance.CurrentDisplayCurrency;
            }

            // 处理空设置
            if (displaySettings == null)
            {
                return FormatCurrencyWithType(internalValue, currentCurrency, true);
            }

            // 根据设置选择显示模式
            if (displaySettings.UseLargeNumberDisplay)
            {
                // 使用大数显示
                if (displaySettings.UseCompactDisplay)
                {
                    // 紧凑模式
                    return GetCompactDisplay(internalValue, currentCurrency);
                }
                else if (displaySettings.LargeNumberThreshold > 0)
                {
                    // 带阈值的智能显示
                    return GetSimplifiedWithThreshold(
                        internalValue,
                        displaySettings.LargeNumberThreshold,
                        true,
                        currentCurrency);
                }
                else
                {
                    // 标准智能显示
                    return GetSmartCurrencyDisplay(internalValue, currentCurrency, true);
                }
            }
            else
            {
                // 使用普通格式
                return FormatCurrencyWithType(internalValue, currentCurrency, true);
            }
        }

        /// <summary>
        /// 获取自适应的货币显示格式（简化版，自动获取货币类型）
        /// 使用默认参数快速格式化
        /// </summary>
        /// <param name="internalValue">内部存储的整数值</param>
        /// <param name="useLargeNumber">是否使用大数显示</param>
        /// <param name="useCompact">是否使用紧凑模式</param>
        /// <param name="threshold">大数阈值（0表示不使用）</param>
        /// <returns>格式化的货币字符串</returns>
        public static string GetAdaptiveDisplay(
            int internalValue,
            bool useLargeNumber = false,
            bool useCompact = false,
            float threshold = 0)
        {
            var settings = new CurrencyDisplaySettings
            {
                UseLargeNumberDisplay = useLargeNumber,
                UseCompactDisplay = useCompact,
                LargeNumberThreshold = threshold
            };

            return GetAdaptiveDisplay(internalValue, settings);
        }

        /// <summary>
        /// 获取当前货币的显示文本（最简单的API）
        /// 自动处理货币类型和格式
        /// </summary>
        /// <param name="internalValue">内部存储的整数值</param>
        /// <returns>格式化的货币字符串</returns>
        public static string GetCurrentCurrencyDisplay(int internalValue)
        {
            // 检查白包模式
            bool isWhitePackage = false;
            if (BlockPuzzle.NativeBridge.NativeBridgeManager.Instance != null)
            {
                isWhitePackage = BlockPuzzle.NativeBridge.NativeBridgeManager.Instance.IsWhitePackage();
            }

            // 白包模式下只返回纯数值
            if (isWhitePackage)
            {
                return FormatValue(internalValue, 3);
            }

            // 标准模式：自动获取当前货币类型
            CurrencyType currentCurrency = CurrencyType.USD;
            if (ExchangeRateManager.Instance != null)
            {
                currentCurrency = ExchangeRateManager.Instance.CurrentDisplayCurrency;
            }

            return FormatCurrencyWithType(internalValue, currentCurrency, true);
        }
    }

    /// <summary>
    /// 货币显示设置类
    /// 用于封装货币显示的各种配置参数
    /// </summary>
    [System.Serializable]
    public class CurrencyDisplaySettings
    {
        /// <summary>
        /// 是否使用大数显示（K/M/B/T格式）
        /// </summary>
        public bool UseLargeNumberDisplay { get; set; }

        /// <summary>
        /// 是否使用紧凑显示模式
        /// </summary>
        public bool UseCompactDisplay { get; set; }

        /// <summary>
        /// 大数显示阈值（开始使用K/M/B/T格式的数值）
        /// </summary>
        public float LargeNumberThreshold { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public CurrencyDisplaySettings()
        {
            UseLargeNumberDisplay = false;
            UseCompactDisplay = false;
            LargeNumberThreshold = 0;
        }

        /// <summary>
        /// 创建默认设置
        /// </summary>
        public static CurrencyDisplaySettings CreateDefault()
        {
            return new CurrencyDisplaySettings();
        }

        /// <summary>
        /// 创建大数显示设置
        /// </summary>
        public static CurrencyDisplaySettings CreateLargeNumberSettings(bool compact = false, float threshold = 0)
        {
            return new CurrencyDisplaySettings
            {
                UseLargeNumberDisplay = true,
                UseCompactDisplay = compact,
                LargeNumberThreshold = threshold
            };
        }
    }
}