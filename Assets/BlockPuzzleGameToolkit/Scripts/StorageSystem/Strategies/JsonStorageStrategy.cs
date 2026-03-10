// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

using System;
using System.IO;
using UnityEngine;
using StorageSystem.Core;
using StorageSystem.Data;

namespace StorageSystem.Strategies
{
    /// <summary>
    /// JSON存储策略
    /// 便于调试和查看数据
    /// </summary>
    public class JsonStorageStrategy : IStorageStrategy
    {
        /// <summary>
        /// 保存数据
        /// </summary>
        public bool Save(string path, object data, StorageOptions options)
        {
            try
            {
                // 更新元数据
                if (data is SaveDataContainer container)
                {
                    container.UpdateMetadata(options?.version ?? 1);
                }

                // 序列化为JSON
                string json = JsonUtility.ToJson(data, true);

                // 应用选项
                if (options?.addChecksum == true && data is SaveDataContainer saveContainer)
                {
                    string checksum = CalculateChecksum(json);
                    saveContainer.SetChecksum(checksum);
                    // 重新序列化包含校验和的数据
                    json = JsonUtility.ToJson(data, true);
                }

                if (options?.useEncryption == true)
                {
                    json = EncryptString(json, options.encryptionKey);
                }

                // 写入文件
                File.WriteAllText(path, json);
                Debug.Log($"[JsonStorage] Saved to {path}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[JsonStorage] Save failed: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 加载数据
        /// </summary>
        public T Load<T>(string path) where T : class
        {
            try
            {
                if (!File.Exists(path))
                {
                    Debug.LogWarning($"[JsonStorage] File not found: {path}");
                    return null;
                }

                string json = File.ReadAllText(path);

                // 尝试解密
                string decrypted = TryDecryptString(json);
                if (!string.IsNullOrEmpty(decrypted))
                {
                    json = decrypted;
                }

                // 反序列化
                T data = JsonUtility.FromJson<T>(json);

                // 验证校验和
                if (data is SaveDataContainer container)
                {
                    if (!container.ValidateChecksum())
                    {
                        Debug.LogWarning("[JsonStorage] Checksum validation failed");
                    }
                }

                Debug.Log($"[JsonStorage] Loaded from {path}");
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"[JsonStorage] Load failed: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        public bool Delete(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                    Debug.Log($"[JsonStorage] Deleted {path}");
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"[JsonStorage] Delete failed: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 检查文件是否存在
        /// </summary>
        public bool Exists(string path)
        {
            return File.Exists(path);
        }

        /// <summary>
        /// 获取存储类型
        /// </summary>
        public StorageType GetStorageType()
        {
            return StorageType.Json;
        }

        /// <summary>
        /// 获取文件扩展名
        /// </summary>
        public string GetFileExtension()
        {
            return "json";
        }

        #region 辅助方法

        /// <summary>
        /// 加密字符串（简单实现）
        /// </summary>
        private string EncryptString(string text, string key)
        {
            if (string.IsNullOrEmpty(key))
                key = "DefaultJsonKey";

            // 添加加密标记
            string encrypted = "ENCRYPTED:" + Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes(text)
            );

            return encrypted;
        }

        /// <summary>
        /// 尝试解密字符串
        /// </summary>
        private string TryDecryptString(string text)
        {
            if (text.StartsWith("ENCRYPTED:"))
            {
                string base64 = text.Substring("ENCRYPTED:".Length);
                try
                {
                    byte[] bytes = Convert.FromBase64String(base64);
                    return System.Text.Encoding.UTF8.GetString(bytes);
                }
                catch
                {
                    Debug.LogWarning("[JsonStorage] Failed to decrypt data");
                }
            }

            return text;
        }

        /// <summary>
        /// 计算校验和
        /// </summary>
        private string CalculateChecksum(string data)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(data);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "");
            }
        }

        #endregion
    }
}