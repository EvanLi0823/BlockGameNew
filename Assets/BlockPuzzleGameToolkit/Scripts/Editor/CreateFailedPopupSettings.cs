using UnityEngine;
using UnityEditor;
using BlockPuzzleGameToolkit.Scripts.Settings;
using BlockPuzzleGameToolkit.Scripts.Services.Ads.AdUnits;

namespace BlockPuzzleGameToolkit.Scripts.Editor
{
    /// <summary>
    /// 临时脚本用于创建 FailedPopupSettings 配置文件
    /// </summary>
    public class CreateFailedPopupSettings
    {
        [MenuItem("Tools/BlockPuzzleGameToolkit/Create Failed Popup Settings (Temp)")]
        public static void CreateSettings()
        {
            // 检查文件是否已存在
            var existingAsset = AssetDatabase.LoadAssetAtPath<FailedPopupSettings>(
                "Assets/BlockPuzzleGameToolkit/Resources/Settings/FailedPopupSettings.asset");

            if (existingAsset != null)
            {
                Debug.Log("FailedPopupSettings.asset 已存在!");
                Selection.activeObject = existingAsset;
                return;
            }

            // 创建新的配置实例
            var newSettings = ScriptableObject.CreateInstance<FailedPopupSettings>();

            // 设置默认值
            newSettings.allowFreeRevive = true;
            newSettings.refreshShapeCount = 3;
            newSettings.maxRevivesPerLevel = 1;
            newSettings.adType = EAdType.Rewarded;
            newSettings.allowReviveOnAdFail = true;
            newSettings.guaranteePlaceableShape = true;
            newSettings.smallShapePriority = 0.7f;
            newSettings.showProgressBar = true;
            newSettings.progressAnimationDuration = 0.5f;
            newSettings.debugFreeRevive = false;

            // 确保目录存在
            if (!AssetDatabase.IsValidFolder("Assets/BlockPuzzleGameToolkit/Resources"))
            {
                AssetDatabase.CreateFolder("Assets/BlockPuzzleGameToolkit", "Resources");
            }
            if (!AssetDatabase.IsValidFolder("Assets/BlockPuzzleGameToolkit/Resources/Settings"))
            {
                AssetDatabase.CreateFolder("Assets/BlockPuzzleGameToolkit/Resources", "Settings");
            }

            // 创建资源文件
            AssetDatabase.CreateAsset(newSettings,
                "Assets/BlockPuzzleGameToolkit/Resources/Settings/FailedPopupSettings.asset");

            // 标记为已修改并保存
            EditorUtility.SetDirty(newSettings);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[FailedPopupSettings] 配置文件创建成功!");
            Selection.activeObject = newSettings;
        }
    }
}