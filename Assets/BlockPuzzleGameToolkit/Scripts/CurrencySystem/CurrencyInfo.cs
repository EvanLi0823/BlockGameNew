// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

using System;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.CurrencySystem
{
    /// <summary>
    /// 货币信息配置（包含汇率转换功能）
    /// </summary>
    [Serializable]
    public class CurrencyInfo
    {
        [Tooltip("货币类型")]
        public CurrencyType type;

        [Tooltip("货币代码（如USD、CNY）")]
        public string code;

        [Tooltip("货币符号（如$、¥）")]
        public string symbol;

        [Tooltip("货币名称")]
        public string name;

        [Tooltip("相对于USD的汇率（1 USD = ? 该货币）")]
        public float exchangeRate;

        [Tooltip("小数位数")]
        public int decimalPlaces;

        [Tooltip("符号位置（true=前置，false=后置）")]
        public bool symbolBeforeAmount;

        [Tooltip("千位分隔符")]
        public string thousandsSeparator;

        [Tooltip("小数分隔符")]
        public string decimalSeparator;

        /// <summary>
        /// 获取两种货币之间的汇率
        /// </summary>
        public static float GetExchangeRate(CurrencyType from, CurrencyType to)
        {
            if (from == to) return 1f;

            var fromInfo = CreateDefault(from);
            var toInfo = CreateDefault(to);

            // USD作为基准，直接计算
            // from -> USD -> to
            float fromToUsd = 1f / fromInfo.exchangeRate;
            float usdToTo = toInfo.exchangeRate;
            return fromToUsd * usdToTo;
        }

        /// <summary>
        /// 转换金额
        /// </summary>
        public static float ConvertAmount(float amount, CurrencyType from, CurrencyType to)
        {
            if (from == to) return amount;
            return amount * GetExchangeRate(from, to);
        }

        /// <summary>
        /// 转换内部值（考虑汇率）
        /// </summary>
        public static int ConvertInternalValue(int internalValue, CurrencyType from, CurrencyType to)
        {
            if (from == to) return internalValue;

            float rate = GetExchangeRate(from, to);
            return Mathf.RoundToInt(internalValue * rate);
        }

        /// <summary>
        /// 创建默认货币信息
        /// </summary>
        public static CurrencyInfo CreateDefault(CurrencyType type)
        {
            switch (type)
            {
                case CurrencyType.USD:
                    return new CurrencyInfo
                    {
                        type = CurrencyType.USD,
                        code = "USD",
                        symbol = "$",
                        name = "美元",
                        exchangeRate = 1f,
                        decimalPlaces = 2,
                        symbolBeforeAmount = true,
                        thousandsSeparator = ",",
                        decimalSeparator = "."
                    };

                case CurrencyType.EUR:
                    return new CurrencyInfo
                    {
                        type = CurrencyType.EUR,
                        code = "EUR",
                        symbol = "€",
                        name = "欧元",
                        exchangeRate = 0.93f,  // 1 USD = 0.93 EUR
                        decimalPlaces = 2,
                        symbolBeforeAmount = true,
                        thousandsSeparator = ".",
                        decimalSeparator = ","
                    };

                case CurrencyType.JPY:
                    return new CurrencyInfo
                    {
                        type = CurrencyType.JPY,
                        code = "JPY",
                        symbol = "¥",
                        name = "日元",
                        exchangeRate = 157f,  // 1 USD = 157 JPY
                        decimalPlaces = 0,    // 日元通常不显示小数
                        symbolBeforeAmount = true,
                        thousandsSeparator = ",",
                        decimalSeparator = "."
                    };

                case CurrencyType.KRW:
                    return new CurrencyInfo
                    {
                        type = CurrencyType.KRW,
                        code = "KRW",
                        symbol = "₩",
                        name = "韩元",
                        exchangeRate = 1345f,  // 1 USD = 1345 KRW
                        decimalPlaces = 0,
                        symbolBeforeAmount = true,
                        thousandsSeparator = ",",
                        decimalSeparator = "."
                    };

                case CurrencyType.INR:
                    return new CurrencyInfo
                    {
                        type = CurrencyType.INR,
                        code = "INR",
                        symbol = "₹",
                        name = "印度卢比",
                        exchangeRate = 83f,  // 1 USD = 83 INR
                        decimalPlaces = 2,
                        symbolBeforeAmount = true,
                        thousandsSeparator = ",",
                        decimalSeparator = "."
                    };

                case CurrencyType.RUB:
                    return new CurrencyInfo
                    {
                        type = CurrencyType.RUB,
                        code = "RUB",
                        symbol = "₽",
                        name = "俄罗斯卢布",
                        exchangeRate = 101f,  // 1 USD = 101 RUB
                        decimalPlaces = 2,
                        symbolBeforeAmount = false,  // 符号在后
                        thousandsSeparator = " ",
                        decimalSeparator = ","
                    };

                case CurrencyType.BRL:
                    return new CurrencyInfo
                    {
                        type = CurrencyType.BRL,
                        code = "BRL",
                        symbol = "R$",
                        name = "巴西雷亚尔",
                        exchangeRate = 5.1f,  // 1 USD = 5.1 BRL
                        decimalPlaces = 2,
                        symbolBeforeAmount = true,
                        thousandsSeparator = ".",
                        decimalSeparator = ","
                    };

                case CurrencyType.PHP:
                    return new CurrencyInfo
                    {
                        type = CurrencyType.PHP,
                        code = "PHP",
                        symbol = "₱",
                        name = "菲律宾比索",
                        exchangeRate = 56f,  // 1 USD = 56 PHP
                        decimalPlaces = 2,
                        symbolBeforeAmount = true,
                        thousandsSeparator = ",",
                        decimalSeparator = "."
                    };

                case CurrencyType.IDR:
                    return new CurrencyInfo
                    {
                        type = CurrencyType.IDR,
                        code = "IDR",
                        symbol = "Rp",
                        name = "印尼卢比",
                        exchangeRate = 16000f,  // 1 USD = 16000 IDR
                        decimalPlaces = 0,
                        symbolBeforeAmount = true,
                        thousandsSeparator = ".",
                        decimalSeparator = ","
                    };

                case CurrencyType.MYR:
                    return new CurrencyInfo
                    {
                        type = CurrencyType.MYR,
                        code = "MYR",
                        symbol = "RM",
                        name = "马来西亚林吉特",
                        exchangeRate = 4.7f,  // 1 USD = 4.7 MYR
                        decimalPlaces = 2,
                        symbolBeforeAmount = true,
                        thousandsSeparator = ",",
                        decimalSeparator = "."
                    };

                case CurrencyType.THB:
                    return new CurrencyInfo
                    {
                        type = CurrencyType.THB,
                        code = "THB",
                        symbol = "฿",
                        name = "泰铢",
                        exchangeRate = 36f,  // 1 USD = 36 THB
                        decimalPlaces = 2,
                        symbolBeforeAmount = true,
                        thousandsSeparator = ",",
                        decimalSeparator = "."
                    };

                case CurrencyType.TRY:
                    return new CurrencyInfo
                    {
                        type = CurrencyType.TRY,
                        code = "TRY",
                        symbol = "₺",
                        name = "土耳其里拉",
                        exchangeRate = 32f,  // 1 USD = 32 TRY
                        decimalPlaces = 2,
                        symbolBeforeAmount = true,
                        thousandsSeparator = ".",
                        decimalSeparator = ","
                    };

                case CurrencyType.VND:
                    return new CurrencyInfo
                    {
                        type = CurrencyType.VND,
                        code = "VND",
                        symbol = "₫",
                        name = "越南盾",
                        exchangeRate = 24500f,  // 1 USD = 24500 VND
                        decimalPlaces = 0,
                        symbolBeforeAmount = false,  // 符号在后
                        thousandsSeparator = ".",
                        decimalSeparator = ","
                    };

                case CurrencyType.MXN:
                    return new CurrencyInfo
                    {
                        type = CurrencyType.MXN,
                        code = "MXN",
                        symbol = "$",
                        name = "墨西哥比索",
                        exchangeRate = 17.5f,  // 1 USD = 17.5 MXN
                        decimalPlaces = 2,
                        symbolBeforeAmount = true,
                        thousandsSeparator = ",",
                        decimalSeparator = "."
                    };

                case CurrencyType.CZK:
                    return new CurrencyInfo
                    {
                        type = CurrencyType.CZK,
                        code = "CZK",
                        symbol = "Kč",
                        name = "捷克克朗",
                        exchangeRate = 23f,  // 1 USD = 23 CZK
                        decimalPlaces = 2,
                        symbolBeforeAmount = false,  // 符号在后
                        thousandsSeparator = " ",
                        decimalSeparator = ","
                    };

                case CurrencyType.PLN:
                    return new CurrencyInfo
                    {
                        type = CurrencyType.PLN,
                        code = "PLN",
                        symbol = "zł",
                        name = "波兰兹罗提",
                        exchangeRate = 4.1f,  // 1 USD = 4.1 PLN
                        decimalPlaces = 2,
                        symbolBeforeAmount = false,  // 符号在后
                        thousandsSeparator = " ",
                        decimalSeparator = ","
                    };

                case CurrencyType.RON:
                    return new CurrencyInfo
                    {
                        type = CurrencyType.RON,
                        code = "RON",
                        symbol = "lei",
                        name = "罗马尼亚列伊",
                        exchangeRate = 4.6f,  // 1 USD = 4.6 RON
                        decimalPlaces = 2,
                        symbolBeforeAmount = false,  // 符号在后
                        thousandsSeparator = ".",
                        decimalSeparator = ","
                    };

                default:
                    return CreateDefault(CurrencyType.USD);
            }
        }
    }
}