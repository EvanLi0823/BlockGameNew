using UnityEngine;
using System.IO;

namespace BlockPuzzleGameToolkit.Scripts.Utils
{
    /// <summary>
    /// 存档清理工具 - 运行时清理旧的加密存档
    /// </summary>
    public static class SaveDataCleaner
    {
        /// <summary>
        /// 清理所有旧的存档数据
        /// 当从加密存储切换到非加密存储时调用此方法
        /// </summary>
        public static void CleanOldSaveData()
        {
            Debug.Log("[SaveDataCleaner] 开始清理旧的存档数据...");

            try
            {
                string persistentPath = Application.persistentDataPath;
                string blockPuzzlePath = Path.Combine(persistentPath, "BlockPuzzle");

                // 要清理的文件列表
                string[] filesToClean = new string[]
                {
                    "currency.dat",
                    "levels.dat",
                    "settings.dat",
                    "props.dat",
                    "tutorial.dat",
                    "multiplier.dat",
                    "storage.dat",
                    "reward.dat",
                    "quest.dat"
                };

                int cleanedCount = 0;

                // 清理BlockPuzzle目录下的文件
                if (Directory.Exists(blockPuzzlePath))
                {
                    foreach (var file in filesToClean)
                    {
                        string fullPath = Path.Combine(blockPuzzlePath, file);
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                            Debug.Log($"[SaveDataCleaner] 删除文件: {fullPath}");
                            cleanedCount++;
                        }
                    }
                }

                // 清理根目录下的文件
                foreach (var file in filesToClean)
                {
                    string fullPath = Path.Combine(persistentPath, file);
                    if (File.Exists(fullPath))
                    {
                        File.Delete(fullPath);
                        Debug.Log($"[SaveDataCleaner] 删除文件: {fullPath}");
                        cleanedCount++;
                    }
                }

                if (cleanedCount > 0)
                {
                    Debug.Log($"[SaveDataCleaner] 清理完成，共删除 {cleanedCount} 个存档文件");
                }
                else
                {
                    Debug.Log("[SaveDataCleaner] 没有找到需要清理的存档文件");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveDataCleaner] 清理存档失败: {e.Message}");
            }
        }

        /// <summary>
        /// 检查是否存在旧的存档数据
        /// </summary>
        /// <returns>如果存在旧存档返回true</returns>
        public static bool HasOldSaveData()
        {
            string persistentPath = Application.persistentDataPath;
            string blockPuzzlePath = Path.Combine(persistentPath, "BlockPuzzle");

            string[] filesToCheck = new string[]
            {
                "currency.dat",
                "levels.dat",
                "reward.dat",
                "quest.dat"
            };

            // 检查BlockPuzzle目录
            if (Directory.Exists(blockPuzzlePath))
            {
                foreach (var file in filesToCheck)
                {
                    if (File.Exists(Path.Combine(blockPuzzlePath, file)))
                    {
                        return true;
                    }
                }
            }

            // 检查根目录
            foreach (var file in filesToCheck)
            {
                if (File.Exists(Path.Combine(persistentPath, file)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}