// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace StorageSystem.Utils
{
    /// <summary>
    /// 存储路径助手
    /// 处理不同平台的路径管理
    /// </summary>
    public static class StoragePathHelper
    {
        private const string SAVE_FOLDER = "SaveData";

        /// <summary>
        /// 获取保存文件的完整路径
        /// </summary>
        /// <param name="filename">文件名（不包含扩展名）</param>
        /// <param name="extension">文件扩展名</param>
        /// <returns>完整路径</returns>
        public static string GetSavePath(string filename, string extension = "dat")
        {
            string directory = GetSaveDirectory();
            EnsureDirectoryExists(directory);

            // 清理文件名，移除非法字符
            filename = SanitizeFilename(filename);

            // 添加扩展名
            if (!string.IsNullOrEmpty(extension))
            {
                if (!extension.StartsWith("."))
                    extension = "." + extension;
                filename = Path.ChangeExtension(filename, extension);
            }

            return Path.Combine(directory, filename);
        }

        /// <summary>
        /// 获取存储目录
        /// </summary>
        /// <returns>存储目录路径</returns>
        public static string GetSaveDirectory()
        {
#if UNITY_EDITOR
            // 编辑器下保存到项目目录
            return Path.Combine(Application.dataPath, "..", SAVE_FOLDER);
#elif UNITY_IOS
            // iOS使用Documents目录
            return Path.Combine(Application.persistentDataPath, SAVE_FOLDER);
#elif UNITY_ANDROID
            // Android使用持久化目录
            return Path.Combine(Application.persistentDataPath, SAVE_FOLDER);
#elif UNITY_STANDALONE_WIN
            // Windows独立版本
            return Path.Combine(Application.dataPath, "..", SAVE_FOLDER);
#elif UNITY_STANDALONE_OSX
            // Mac独立版本
            return Path.Combine(Application.dataPath, "..", "..", SAVE_FOLDER);
#elif UNITY_WEBGL
            // WebGL使用PlayerPrefs（文件系统受限）
            return Application.persistentDataPath;
#else
            // 其他平台使用默认持久化路径
            return Path.Combine(Application.persistentDataPath, SAVE_FOLDER);
#endif
        }

        /// <summary>
        /// 确保目录存在
        /// </summary>
        /// <param name="directory">目录路径</param>
        public static void EnsureDirectoryExists(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                Debug.Log($"[StoragePathHelper] Created directory: {directory}");
            }
        }

        /// <summary>
        /// 清理文件名，移除非法字符
        /// </summary>
        /// <param name="filename">原始文件名</param>
        /// <returns>清理后的文件名</returns>
        public static string SanitizeFilename(string filename)
        {
            // 移除路径中的非法字符
            char[] invalidChars = Path.GetInvalidFileNameChars();
            foreach (char c in invalidChars)
            {
                filename = filename.Replace(c.ToString(), "_");
            }

            // 限制文件名长度
            if (filename.Length > 100)
            {
                filename = filename.Substring(0, 100);
            }

            return filename;
        }

        /// <summary>
        /// 获取所有已保存的文件键
        /// </summary>
        /// <returns>文件键列表</returns>
        public static List<string> GetAllSavedKeys()
        {
            var keys = new List<string>();
            string directory = GetSaveDirectory();

            if (Directory.Exists(directory))
            {
                var files = Directory.GetFiles(directory, "*.*", SearchOption.TopDirectoryOnly);
                foreach (var file in files)
                {
                    // 获取不带扩展名的文件名作为键
                    string key = Path.GetFileNameWithoutExtension(file);
                    if (!keys.Contains(key))
                    {
                        keys.Add(key);
                    }
                }
            }

            return keys;
        }

        /// <summary>
        /// 删除所有保存文件
        /// </summary>
        public static void DeleteAllSaveFiles()
        {
            string directory = GetSaveDirectory();

            if (Directory.Exists(directory))
            {
                try
                {
                    Directory.Delete(directory, true);
                    Debug.Log($"[StoragePathHelper] Deleted all save files in: {directory}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[StoragePathHelper] Failed to delete save files: {e.Message}");
                }
            }
        }

        /// <summary>
        /// 获取文件大小
        /// </summary>
        /// <param name="filename">文件名</param>
        /// <param name="extension">扩展名</param>
        /// <returns>文件大小（字节）</returns>
        public static long GetFileSize(string filename, string extension = "dat")
        {
            string path = GetSavePath(filename, extension);

            if (File.Exists(path))
            {
                FileInfo fileInfo = new FileInfo(path);
                return fileInfo.Length;
            }

            return 0;
        }

        /// <summary>
        /// 获取总存储大小
        /// </summary>
        /// <returns>总大小（字节）</returns>
        public static long GetTotalStorageSize()
        {
            long totalSize = 0;
            string directory = GetSaveDirectory();

            if (Directory.Exists(directory))
            {
                var files = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    FileInfo fileInfo = new FileInfo(file);
                    totalSize += fileInfo.Length;
                }
            }

            return totalSize;
        }

        /// <summary>
        /// 格式化文件大小为可读字符串
        /// </summary>
        /// <param name="bytes">字节数</param>
        /// <returns>格式化的字符串</returns>
        public static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            double size = bytes;

            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }

            return $"{size:0.##} {sizes[order]}";
        }

        /// <summary>
        /// 创建备份
        /// </summary>
        /// <param name="filename">文件名</param>
        /// <param name="extension">扩展名</param>
        /// <returns>是否成功</returns>
        public static bool CreateBackup(string filename, string extension = "dat")
        {
            try
            {
                string sourcePath = GetSavePath(filename, extension);
                if (File.Exists(sourcePath))
                {
                    string backupPath = GetSavePath($"{filename}_backup_{DateTime.Now:yyyyMMddHHmmss}", extension);
                    File.Copy(sourcePath, backupPath);
                    Debug.Log($"[StoragePathHelper] Created backup: {backupPath}");
                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[StoragePathHelper] Backup failed: {e.Message}");
            }
            return false;
        }
    }
}