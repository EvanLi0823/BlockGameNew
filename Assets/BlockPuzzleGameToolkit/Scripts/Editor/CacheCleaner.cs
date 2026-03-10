using UnityEditor;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Editor
{
    /// <summary>
    /// Unity缓存清理工具
    /// </summary>
    public static class CacheCleaner
    {
        [MenuItem("Tools/Clear Unity Cache")]
        public static void ClearCache()
        {
            Debug.Log("开始清理Unity缓存...");

            // 重新导入所有资源
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            // 清理脚本编译缓存
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();

            Debug.Log("缓存清理完成！");
        }

        [MenuItem("Tools/Reimport Obfuscator Scripts")]
        public static void ReimportObfuscatorScripts()
        {
            Debug.Log("重新导入Obfuscator脚本...");

            // 重新导入Obfuscator相关脚本
            string[] paths = new string[]
            {
                "Assets/BlockPuzzleGameToolkit/Scripts/Beebyte",
            };

            foreach (string path in paths)
            {
                if (AssetDatabase.IsValidFolder(path))
                {
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ImportRecursive);
                    Debug.Log($"已重新导入: {path}");
                }
            }

            AssetDatabase.Refresh();
            Debug.Log("Obfuscator脚本重新导入完成！");
        }
    }
}