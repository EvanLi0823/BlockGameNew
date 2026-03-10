// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

using UnityEngine;
using BlockPuzzleGameToolkit.Scripts.CurrencySystem;

namespace BlockPuzzleGameToolkit.Scripts.Examples
{
    /// <summary>
    /// 大数货币显示测试示例
    /// 演示CurrencyFormatter中各种大数显示方法的使用
    /// </summary>
    public class LargeNumberDisplayExample : MonoBehaviour
    {
        [Header("测试设置")]
        [SerializeField]
        private CurrencyType testCurrencyType = CurrencyType.USD;

        [SerializeField]
        [Tooltip("测试值（显示值，会自动转换为内部值）")]
        private float[] testDisplayValues = new float[]
        {
            0.99f,
            10f,
            100f,
            999f,
            1000f,      // 1K
            5500f,      // 5.5K
            12345f,     // 12.3K
            99999f,     // 100K
            100000f,    // 100K
            250000f,    // 250K
            1234567f,   // 1.2M
            9999999f,   // 10M
            12345678f,  // 12.3M
            123456789f, // 123M
            1234567890f, // 1.2B
            12345678900f, // 12.3B
            123456789000f, // 123B
            1234567890000f // 1.2T
        };

        void Start()
        {
            Debug.Log("=== 大数货币显示测试开始 ===");
            TestAllMethods();
        }

        [ContextMenu("运行测试")]
        public void TestAllMethods()
        {
            Debug.Log($"\n测试货币类型: {testCurrencyType}");
            Debug.Log("=" .PadRight(80, '='));

            foreach (float displayValue in testDisplayValues)
            {
                TestSingleValue(displayValue);
            }

            TestBatchFormat();
            TestThresholdDisplay();
            TestMultiCurrency();
        }

        private void TestSingleValue(float displayValue)
        {
            // 转换为内部值
            int internalValue = CurrencyFormatter.ToInternalValue(displayValue);

            Debug.Log($"\n原始值: {displayValue:N2} ({testCurrencyType})");
            Debug.Log("-".PadRight(60, '-'));

            // 1. 简化值（不带符号）
            string simplified = CurrencyFormatter.GetSimplifiedValue(internalValue);
            Debug.Log($"GetSimplifiedValue: {simplified}");

            // 2. 智能显示（自动调整小数位）
            string smart = CurrencyFormatter.GetSmartCurrencyDisplay(internalValue, testCurrencyType);
            Debug.Log($"GetSmartCurrencyDisplay: {smart}");

            // 3. 紧凑显示（最小字符）
            string compact = CurrencyFormatter.GetCompactDisplay(internalValue, testCurrencyType);
            Debug.Log($"GetCompactDisplay: {compact}");

            // 4. 普通格式（对比）
            string normal = CurrencyFormatter.FormatCurrencyWithType(internalValue, testCurrencyType);
            Debug.Log($"普通格式（对比）: {normal}");
        }

        [ContextMenu("测试批量格式化")]
        private void TestBatchFormat()
        {
            Debug.Log($"\n\n=== 批量格式化测试 ===");

            // 准备测试数据
            int[] values = new int[testDisplayValues.Length];
            for (int i = 0; i < testDisplayValues.Length; i++)
            {
                values[i] = CurrencyFormatter.ToInternalValue(testDisplayValues[i]);
            }

            // 批量格式化
            string[] results = CurrencyFormatter.BatchFormatSimplified(values, testCurrencyType);

            Debug.Log("批量结果:");
            for (int i = 0; i < results.Length; i++)
            {
                Debug.Log($"  {testDisplayValues[i],15:N2} -> {results[i]}");
            }
        }

        [ContextMenu("测试阈值显示")]
        private void TestThresholdDisplay()
        {
            Debug.Log($"\n\n=== 阈值显示测试 ===");

            float[] thresholds = { 100f, 1000f, 10000f };
            float[] testValues = { 50f, 500f, 5000f, 50000f };

            foreach (float threshold in thresholds)
            {
                Debug.Log($"\n阈值: {threshold}");
                foreach (float value in testValues)
                {
                    int internalValue = CurrencyFormatter.ToInternalValue(value);
                    string result = CurrencyFormatter.GetSimplifiedWithThreshold(
                        internalValue, threshold, true, testCurrencyType);
                    Debug.Log($"  值 {value,8:N0}: {result}");
                }
            }
        }

        [ContextMenu("测试多币种显示")]
        private void TestMultiCurrency()
        {
            Debug.Log($"\n\n=== 多币种显示测试 ===");

            // 测试值：10000 USD
            float usdValue = 10000f;
            int internalValue = CurrencyFormatter.ToInternalValue(usdValue);

            Debug.Log($"基准值: {usdValue:N2} USD");
            Debug.Log("-".PadRight(60, '-'));

            CurrencyType[] currencies = new CurrencyType[]
            {
                CurrencyType.USD,
                CurrencyType.EUR,
                CurrencyType.JPY,
                CurrencyType.KRW,
                CurrencyType.INR,
                CurrencyType.RUB,
                CurrencyType.BRL,
                CurrencyType.PHP,
                CurrencyType.IDR,
                CurrencyType.MYR,
                CurrencyType.THB,
                CurrencyType.TRY,
                CurrencyType.VND
            };

            foreach (var currency in currencies)
            {
                string smart = CurrencyFormatter.GetSmartCurrencyDisplay(internalValue, currency);
                string compact = CurrencyFormatter.GetCompactDisplay(internalValue, currency);
                string normal = CurrencyFormatter.FormatCurrencyWithType(internalValue, currency);

                Debug.Log($"{currency,-4}: 智能={smart,-12} 紧凑={compact,-10} 普通={normal}");
            }
        }

        [ContextMenu("测试极限值")]
        public void TestExtremeCases()
        {
            Debug.Log($"\n\n=== 极限值测试 ===");

            float[] extremeValues = new float[]
            {
                0f,
                0.01f,
                0.1f,
                1f,
                float.MaxValue / CurrencyFormatter.SCALE,  // 最大可能值
            };

            foreach (float value in extremeValues)
            {
                if (float.IsInfinity(value) || float.IsNaN(value))
                {
                    Debug.LogWarning($"跳过无效值: {value}");
                    continue;
                }

                int internalValue = CurrencyFormatter.ToInternalValue(value);
                string result = CurrencyFormatter.GetSmartCurrencyDisplay(internalValue, testCurrencyType);
                Debug.Log($"值 {value:E2} -> {result}");
            }
        }

        [ContextMenu("生成排行榜示例")]
        public void GenerateLeaderboardExample()
        {
            Debug.Log($"\n\n=== 排行榜显示示例 ===");

            // 模拟排行榜数据
            string[] playerNames = { "玩家A", "玩家B", "玩家C", "玩家D", "玩家E" };
            float[] scores = { 9876543f, 1234567f, 987654f, 123456f, 98765f };

            int[] internalScores = new int[scores.Length];
            for (int i = 0; i < scores.Length; i++)
            {
                internalScores[i] = CurrencyFormatter.ToInternalValue(scores[i]);
            }

            // 批量格式化
            string[] formattedScores = CurrencyFormatter.BatchFormatSimplified(internalScores, testCurrencyType);

            Debug.Log("排行榜:");
            Debug.Log("排名  玩家    分数");
            Debug.Log("-".PadRight(30, '-'));
            for (int i = 0; i < playerNames.Length; i++)
            {
                Debug.Log($"{i + 1,2}.   {playerNames[i],-8} {formattedScores[i],10}");
            }
        }

#if UNITY_EDITOR
        [ContextMenu("在Inspector中测试")]
        private void TestInInspector()
        {
            // 这个方法可以在Inspector中右键调用
            TestAllMethods();
        }

        // 添加一个按钮在Inspector中
        [UnityEditor.CustomEditor(typeof(LargeNumberDisplayExample))]
        public class LargeNumberDisplayExampleEditor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();

                GUILayout.Space(10);

                if (GUILayout.Button("运行所有测试"))
                {
                    ((LargeNumberDisplayExample)target).TestAllMethods();
                }

                if (GUILayout.Button("测试批量格式化"))
                {
                    ((LargeNumberDisplayExample)target).TestBatchFormat();
                }

                if (GUILayout.Button("测试阈值显示"))
                {
                    ((LargeNumberDisplayExample)target).TestThresholdDisplay();
                }

                if (GUILayout.Button("测试多币种"))
                {
                    ((LargeNumberDisplayExample)target).TestMultiCurrency();
                }

                if (GUILayout.Button("测试极限值"))
                {
                    ((LargeNumberDisplayExample)target).TestExtremeCases();
                }

                if (GUILayout.Button("生成排行榜示例"))
                {
                    ((LargeNumberDisplayExample)target).GenerateLeaderboardExample();
                }
            }
        }
#endif
    }
}