// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using StorageSystem.Core;
using StorageSystem.Data;

namespace StorageSystem.Strategies
{
    /// <summary>
    /// 二进制存储策略
    /// </summary>
    public class BinaryStorageStrategy : IStorageStrategy
    {
        /// <summary>
        /// 保存数据
        /// </summary>
        public virtual bool Save(string path, object data, StorageOptions options)
        {
            try
            {
                // 序列化为二进制
                byte[] bytes = SerializeToBinary(data);

                // 应用选项
                if (options?.useCompression == true)
                {
                    bytes = CompressData(bytes, options.compression);
                }

                if (options?.addChecksum == true && data is SaveDataContainer container)
                {
                    string checksum = CalculateChecksum(bytes);
                    container.SetChecksum(checksum);
                    // 重新序列化包含校验和的数据
                    bytes = SerializeToBinary(data);
                }

                if (options?.useEncryption == true)
                {
                    bytes = EncryptData(bytes, options.encryptionKey);
                }

                // 写入文件
                File.WriteAllBytes(path, bytes);
                Debug.Log($"[BinaryStorage] Saved {bytes.Length} bytes to {path}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[BinaryStorage] Save failed: {e.Message}");
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
                    Debug.LogWarning($"[BinaryStorage] File not found: {path}");
                    return null;
                }

                byte[] bytes = File.ReadAllBytes(path);

                // 尝试解密（如果数据是加密的）
                byte[] decrypted = TryDecryptData(bytes);
                if (decrypted != null)
                {
                    bytes = decrypted;
                }

                // 尝试解压（如果数据是压缩的）
                byte[] decompressed = TryDecompressData(bytes);
                if (decompressed != null)
                {
                    bytes = decompressed;
                }

                // 反序列化
                T data = DeserializeFromBinary<T>(bytes);

                // 验证校验和
                if (data is SaveDataContainer container)
                {
                    if (!container.ValidateChecksum())
                    {
                        Debug.LogWarning("[BinaryStorage] Checksum validation failed");
                    }
                }

                Debug.Log($"[BinaryStorage] Loaded {bytes.Length} bytes from {path}");
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"[BinaryStorage] Load failed: {e.Message}");
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
                    Debug.Log($"[BinaryStorage] Deleted {path}");
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"[BinaryStorage] Delete failed: {e.Message}");
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
        public virtual StorageType GetStorageType()
        {
            return StorageType.Binary;
        }

        /// <summary>
        /// 获取文件扩展名
        /// </summary>
        public virtual string GetFileExtension()
        {
            return "dat";
        }

        #region 辅助方法

        /// <summary>
        /// 序列化为二进制
        /// </summary>
        protected byte[] SerializeToBinary(object obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// 从二进制反序列化
        /// </summary>
        protected T DeserializeFromBinary<T>(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return (T)formatter.Deserialize(ms);
            }
        }

        /// <summary>
        /// 压缩数据（简化实现）
        /// </summary>
        protected virtual byte[] CompressData(byte[] data, StorageCompressionLevel level)
        {
            // 这里使用简单的标记，实际项目中应使用如 System.IO.Compression
            // 添加压缩标记头
            byte[] header = Encoding.UTF8.GetBytes("COMPRESSED:");
            byte[] result = new byte[header.Length + data.Length];
            Buffer.BlockCopy(header, 0, result, 0, header.Length);
            Buffer.BlockCopy(data, 0, result, header.Length, data.Length);
            return result;
        }

        /// <summary>
        /// 尝试解压数据
        /// </summary>
        protected virtual byte[] TryDecompressData(byte[] data)
        {
            string header = "COMPRESSED:";
            byte[] headerBytes = Encoding.UTF8.GetBytes(header);

            if (data.Length > headerBytes.Length)
            {
                bool isCompressed = true;
                for (int i = 0; i < headerBytes.Length; i++)
                {
                    if (data[i] != headerBytes[i])
                    {
                        isCompressed = false;
                        break;
                    }
                }

                if (isCompressed)
                {
                    byte[] result = new byte[data.Length - headerBytes.Length];
                    Buffer.BlockCopy(data, headerBytes.Length, result, 0, result.Length);
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// 加密数据（简化实现）
        /// </summary>
        protected virtual byte[] EncryptData(byte[] data, string key)
        {
            if (string.IsNullOrEmpty(key))
                key = "DefaultKey123456"; // 默认密钥

            // 简单XOR加密（实际项目中应使用AES等强加密）
            byte[] encrypted = new byte[data.Length + 4];
            byte[] marker = BitConverter.GetBytes(0xEEEEEEEE); // 加密标记
            Buffer.BlockCopy(marker, 0, encrypted, 0, 4);

            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            for (int i = 0; i < data.Length; i++)
            {
                encrypted[i + 4] = (byte)(data[i] ^ keyBytes[i % keyBytes.Length]);
            }

            return encrypted;
        }

        /// <summary>
        /// 尝试解密数据
        /// </summary>
        protected virtual byte[] TryDecryptData(byte[] data)
        {
            if (data.Length > 4)
            {
                int marker = BitConverter.ToInt32(data, 0);
                if (marker == unchecked((int)0xEEEEEEEE)) // 检查加密标记
                {
                    string key = "DefaultKey123456";
                    byte[] keyBytes = Encoding.UTF8.GetBytes(key);
                    byte[] decrypted = new byte[data.Length - 4];

                    for (int i = 4; i < data.Length; i++)
                    {
                        decrypted[i - 4] = (byte)(data[i] ^ keyBytes[(i - 4) % keyBytes.Length]);
                    }

                    return decrypted;
                }
            }

            return null;
        }

        /// <summary>
        /// 计算校验和
        /// </summary>
        protected string CalculateChecksum(byte[] data)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(data);
                return BitConverter.ToString(hash).Replace("-", "");
            }
        }

        #endregion
    }
}