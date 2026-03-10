// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

using UnityEngine;
using UnityEditor;
using BlockPuzzleGameToolkit.Scripts.Settings;

namespace BlockPuzzleGameToolkit.Scripts.LevelsData.Editor
{
    /// <summary>
    /// 自定义PropertyDrawer用于在Inspector中美化LevelRewardConfig的显示
    /// </summary>
    [CustomPropertyDrawer(typeof(LevelRewardConfig))]
    public class LevelRewardConfigDrawer : PropertyDrawer
    {
        private const float LineHeight = 20f;
        private const float Spacing = 5f;
        private const float Indent = 15f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // 获取所有属性
            var popupTypeProp = property.FindPropertyRelative("popupType");
            var skipMultiplierProp = property.FindPropertyRelative("skipMultiplier");
            var showRewardAdProp = property.FindPropertyRelative("showRewardAd");
            var showInterstitialAdProp = property.FindPropertyRelative("showInterstitialAd");
            var fixedMultiplierConfigIdProp = property.FindPropertyRelative("fixedMultiplierConfigId");

            float yOffset = position.y;

            // 绘制折叠标题
            property.isExpanded = EditorGUI.Foldout(
                new Rect(position.x, yOffset, position.width, LineHeight),
                property.isExpanded,
                "奖励弹窗配置",
                true
            );
            yOffset += LineHeight + Spacing;

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                // 弹窗类型
                EditorGUI.PropertyField(
                    new Rect(position.x, yOffset, position.width, LineHeight),
                    popupTypeProp,
                    new GUIContent("弹窗类型", "选择奖励弹窗的类型")
                );
                yOffset += LineHeight + Spacing;

                // 通用配置标题
                EditorGUI.LabelField(
                    new Rect(position.x, yOffset, position.width, LineHeight),
                    "通用配置",
                    EditorStyles.boldLabel
                );
                yOffset += LineHeight;

                // 不领奖倍率
                EditorGUI.Slider(
                    new Rect(position.x + Indent, yOffset, position.width - Indent, LineHeight),
                    skipMultiplierProp,
                    0.01f,
                    1f,
                    new GUIContent("不领奖倍率", "点击不领奖按钮时的奖励倍率")
                );
                yOffset += LineHeight + Spacing;

                // 广告配置标题
                EditorGUI.LabelField(
                    new Rect(position.x, yOffset, position.width, LineHeight),
                    "广告配置",
                    EditorStyles.boldLabel
                );
                yOffset += LineHeight;

                // 激励广告开关
                EditorGUI.PropertyField(
                    new Rect(position.x + Indent, yOffset, position.width - Indent, LineHeight),
                    showRewardAdProp,
                    new GUIContent("激励广告", "是否显示激励广告（false时显示免费领奖）")
                );
                yOffset += LineHeight;

                // 插屏广告开关
                EditorGUI.PropertyField(
                    new Rect(position.x + Indent, yOffset, position.width - Indent, LineHeight),
                    showInterstitialAdProp,
                    new GUIContent("插屏广告", "点击不领奖时是否显示插屏广告")
                );
                yOffset += LineHeight + Spacing;

                // 固定倍率配置（仅在Fixed类型时显示）
                if (popupTypeProp.enumValueIndex == 0) // Fixed
                {
                    // 固定倍率配置标题
                    EditorGUI.LabelField(
                        new Rect(position.x, yOffset, position.width, LineHeight),
                        "固定倍率配置",
                        EditorStyles.boldLabel
                    );
                    yOffset += LineHeight;

                    // 尝试加载配置文件
                    var settings = Resources.Load<LevelRewardMultiplierSettings>("Settings/LevelRewardMultiplierSettings");
                    if (settings != null)
                    {
                        // 获取所有配置ID和名称
                        var configIds = settings.GetAllConfigIds();
                        var configNames = settings.GetAllConfigNames();

                        if (configIds != null && configIds.Length > 0)
                        {
                            // 查找当前选中的索引
                            int currentIndex = System.Array.IndexOf(configIds, fixedMultiplierConfigIdProp.stringValue);
                            if (currentIndex < 0) currentIndex = 0;

                            // 显示下拉列表
                            int newIndex = EditorGUI.Popup(
                                new Rect(position.x + Indent, yOffset, position.width - Indent, LineHeight),
                                "倍率配置",
                                currentIndex,
                                configNames
                            );

                            // 更新选中的配置ID
                            if (newIndex != currentIndex && newIndex >= 0 && newIndex < configIds.Length)
                            {
                                fixedMultiplierConfigIdProp.stringValue = configIds[newIndex];
                            }
                            yOffset += LineHeight + Spacing;

                            // 显示选中配置的预览
                            if (currentIndex >= 0 && currentIndex < configIds.Length)
                            {
                                var config = settings.GetConfig(configIds[currentIndex]);
                                if (config != null && config.Multipliers != null)
                                {
                                    string multiplierStr = string.Join(", ", config.Multipliers);
                                    EditorGUI.HelpBox(
                                        new Rect(position.x + Indent, yOffset, position.width - Indent, 40),
                                        $"倍率序列: {multiplierStr}\n" +
                                        $"每日重置: {(config.ResetDaily ? "是" : "否")} | " +
                                        $"提现重置: {(config.ResetOnWithdraw ? "是" : "否")}",
                                        MessageType.Info
                                    );
                                    yOffset += 45;
                                }
                            }
                        }
                        else
                        {
                            EditorGUI.HelpBox(
                                new Rect(position.x + Indent, yOffset, position.width - Indent, 40),
                                "配置文件中没有任何配置！请点击配置文件添加默认配置。",
                                MessageType.Warning
                            );
                            yOffset += 45;
                        }
                    }
                    else
                    {
                        EditorGUI.HelpBox(
                            new Rect(position.x + Indent, yOffset, position.width - Indent, 40),
                            "未找到倍率配置文件！请通过菜单创建：\nTools > BlockPuzzleGameToolkit > Settings > Level Reward Multiplier",
                            MessageType.Error
                        );
                        yOffset += 45;
                    }
                }
                else // Sliding
                {
                    EditorGUI.HelpBox(
                        new Rect(position.x + Indent, yOffset, position.width - Indent, 40),
                        "滑动倍率将使用MultiplierManager模块\n倍率由滑块实时决定",
                        MessageType.Info
                    );
                    yOffset += 45;
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
            {
                return LineHeight + Spacing;
            }

            float height = LineHeight + Spacing; // 标题
            height += LineHeight + Spacing; // 弹窗类型
            height += LineHeight; // 通用配置标题
            height += LineHeight + Spacing; // 不领奖倍率
            height += LineHeight; // 广告配置标题
            height += LineHeight; // 激励广告
            height += LineHeight + Spacing; // 插屏广告

            var popupTypeProp = property.FindPropertyRelative("popupType");
            if (popupTypeProp.enumValueIndex == 0) // Fixed
            {
                height += LineHeight; // 固定倍率配置标题
                height += LineHeight + Spacing; // 下拉列表
                height += 45; // 配置预览或错误提示
            }
            else // Sliding
            {
                height += 45; // 滑动倍率提示
            }

            return height;
        }
    }
}