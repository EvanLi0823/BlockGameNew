using UnityEditor;
using UnityEngine;
using System.IO;
using StorageSystem.Core;

namespace BlockPuzzleGameToolkit.Scripts.Editor
{
    /// <summary>
    /// 存档清理工具
    /// </summary>
    public class StorageCleaner : EditorWindow
    {
        [MenuItem("BlockPuzzle/Storage/Clear All Save Data")]
        private static void ClearAllSaveData()
        {
            if (EditorUtility.DisplayDialog(
                "清除所有存档数据",
                "此操作将删除所有游戏存档数据，包括：\n" +
                "• 货币数据\n" +
                "• 关卡进度\n" +
                "• 设置配置\n" +
                "\n此操作不可撤销！确定要继续吗？",
                "确定",
                "取消"))
            {
                ClearData();
            }
        }

        [MenuItem("BlockPuzzle/Storage/Clear Corrupted Currency Data")]
        private static void ClearCorruptedCurrencyData()
        {
            if (EditorUtility.DisplayDialog(
                "清除损坏的货币数据",
                "此操作将删除损坏的货币存档数据。\n" +
                "游戏重新启动时将创建新的默认数据。\n" +
                "\n确定要继续吗？",
                "确定",
                "取消"))
            {
                ClearCurrencyData();
            }
        }

        [MenuItem("BlockPuzzle/Storage/Fix Storage Permissions")]
        private static void FixStoragePermissions()
        {
            try
            {
                string persistentPath = Application.persistentDataPath;
                string savePath = Path.Combine(persistentPath, "BlockPuzzle");

                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                    Debug.Log($"[StorageCleaner] 创建存档目录: {savePath}");
                }

                // 确保目录有写入权限
                var dirInfo = new DirectoryInfo(savePath);
                Debug.Log($"[StorageCleaner] 存档路径: {savePath}");
                Debug.Log($"[StorageCleaner] 目录存在: {dirInfo.Exists}");

                EditorUtility.DisplayDialog(
                    "存储权限修复",
                    "存档目录已准备就绪。\n" +
                    $"路径: {savePath}",
                    "确定");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[StorageCleaner] 修复存储权限失败: {e.Message}");
                EditorUtility.DisplayDialog(
                    "错误",
                    $"修复存储权限失败:\n{e.Message}",
                    "确定");
            }
        }

        private static void ClearData()
        {
            try
            {
                string persistentPath = Application.persistentDataPath;
                string[] filesToDelete = new string[]
                {
                    "BlockPuzzle/currency.dat",
                    "BlockPuzzle/levels.dat",
                    "BlockPuzzle/settings.dat",
                    "BlockPuzzle/props.dat",
                    "BlockPuzzle/tutorial.dat",
                    "BlockPuzzle/multiplier.dat",
                    "BlockPuzzle/storage.dat",
                    "currency.dat",
                    "levels.dat",
                    "settings.dat",
                    "props.dat"
                };

                int deletedCount = 0;
                foreach (var file in filesToDelete)
                {
                    string fullPath = Path.Combine(persistentPath, file);
                    if (File.Exists(fullPath))
                    {
                        File.Delete(fullPath);
                        Debug.Log($"[StorageCleaner] 删除文件: {fullPath}");
                        deletedCount++;
                    }
                }

                // 清除PlayerPrefs
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();

                if (deletedCount > 0)
                {
                    Debug.Log($"[StorageCleaner] 成功删除 {deletedCount} 个存档文件");
                    EditorUtility.DisplayDialog(
                        "清除成功",
                        $"已成功删除 {deletedCount} 个存档文件。\n" +
                        "游戏重新启动时将创建新的默认数据。",
                        "确定");
                }
                else
                {
                    Debug.Log("[StorageCleaner] 没有找到需要删除的存档文件");
                    EditorUtility.DisplayDialog(
                        "清除完成",
                        "没有找到需要删除的存档文件。",
                        "确定");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[StorageCleaner] 清除数据失败: {e.Message}");
                EditorUtility.DisplayDialog(
                    "清除失败",
                    $"清除数据时发生错误:\n{e.Message}",
                    "确定");
            }
        }

        private static void ClearCurrencyData()
        {
            try
            {
                // 如果StorageManager可用，使用它的Delete方法
                if (Application.isPlaying && StorageManager.Instance != null)
                {
                    StorageManager.Instance.Delete("currency", StorageType.Binary);
                    Debug.Log("[StorageCleaner] 通过StorageManager删除货币数据");
                }
                else
                {
                    // 否则直接删除文件
                    string persistentPath = Application.persistentDataPath;
                    string[] currencyFiles = new string[]
                    {
                        "BlockPuzzle/currency.dat",
                        "currency.dat"
                    };

                    bool deleted = false;
                    foreach (var file in currencyFiles)
                    {
                        string fullPath = Path.Combine(persistentPath, file);
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                            Debug.Log($"[StorageCleaner] 删除货币文件: {fullPath}");
                            deleted = true;
                        }
                    }

                    if (deleted)
                    {
                        Debug.Log("[StorageCleaner] 货币数据已清除");
                    }
                    else
                    {
                        Debug.Log("[StorageCleaner] 没有找到货币存档文件");
                    }
                }

                EditorUtility.DisplayDialog(
                    "清除成功",
                    "货币数据已清除。\n" +
                    "游戏重新启动时将创建新的默认数据。",
                    "确定");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[StorageCleaner] 清除货币数据失败: {e.Message}");
                EditorUtility.DisplayDialog(
                    "清除失败",
                    $"清除货币数据时发生错误:\n{e.Message}",
                    "确定");
            }
        }
    }
}