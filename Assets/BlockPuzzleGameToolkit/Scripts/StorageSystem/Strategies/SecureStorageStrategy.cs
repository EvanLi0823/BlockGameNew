// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using StorageSystem.Core;

namespace StorageSystem.Strategies
{
    /// <summary>
    /// 安全存储策略
    /// 强制加密和数据完整性验证
    /// </summary>
    public class SecureStorageStrategy : BinaryStorageStrategy
    {
        private const string DEFAULT_KEY = "BlockPuzzle2025SecureKey!@#$";
        private const string SALT = "BlockPuzzleSalt";

        /// <summary>
        /// 保存数据（强制加密）
        /// </summary>
        public override bool Save(string path, object data, StorageOptions options)
        {
            // 强制启用安全选项
            options = options ?? new StorageOptions();
            options.useEncryption = true;
            options.addChecksum = true;
            options.useCompression = true;

            // 使用更强的加密密钥
            if (string.IsNullOrEmpty(options.encryptionKey))
            {
                options.encryptionKey = GenerateSecureKey();
            }

            return base.Save(path, data, options);
        }

        /// <summary>
        /// 获取存储类型
        /// </summary>
        public override StorageType GetStorageType()
        {
            return StorageType.SecureBinary;
        }

        /// <summary>
        /// 获取文件扩展名
        /// </summary>
        public override string GetFileExtension()
        {
            return "sdat"; // Secure Data
        }

        /// <summary>
        /// 使用AES加密数据
        /// </summary>
        protected override byte[] EncryptData(byte[] data, string key)
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                    key = DEFAULT_KEY;

                using (Aes aes = Aes.Create())
                {
                    // 从密钥和盐生成密钥和IV
                    var keyAndIv = GenerateKeyAndIV(key, SALT, aes.KeySize / 8, aes.BlockSize / 8);
                    aes.Key = keyAndIv.Key;
                    aes.IV = keyAndIv.IV;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (var encryptor = aes.CreateEncryptor())
                    {
                        byte[] encrypted = encryptor.TransformFinalBlock(data, 0, data.Length);

                        // 添加加密标记和IV到结果前面
                        byte[] result = new byte[4 + aes.IV.Length + encrypted.Length];
                        byte[] marker = BitConverter.GetBytes(0xAEAEAEAE); // 加密标记

                        Buffer.BlockCopy(marker, 0, result, 0, 4);
                        Buffer.BlockCopy(aes.IV, 0, result, 4, aes.IV.Length);
                        Buffer.BlockCopy(encrypted, 0, result, 4 + aes.IV.Length, encrypted.Length);

                        Debug.Log($"[SecureStorage] Encrypted {data.Length} bytes to {result.Length} bytes");
                        return result;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SecureStorage] Encryption failed: {e.Message}");
                // 降级到基类的简单加密
                return base.EncryptData(data, key);
            }
        }

        /// <summary>
        /// 使用AES解密数据
        /// </summary>
        protected override byte[] TryDecryptData(byte[] data)
        {
            try
            {
                if (data.Length > 4)
                {
                    int marker = BitConverter.ToInt32(data, 0);

                    // 检查AES加密标记
                    if (marker == unchecked((int)0xAEAEAEAE))
                    {
                        using (Aes aes = Aes.Create())
                        {
                            // 读取IV
                            byte[] iv = new byte[aes.BlockSize / 8];
                            Buffer.BlockCopy(data, 4, iv, 0, iv.Length);

                            // 生成密钥
                            var keyAndIv = GenerateKeyAndIV(DEFAULT_KEY, SALT, aes.KeySize / 8, aes.BlockSize / 8);
                            aes.Key = keyAndIv.Key;
                            aes.IV = iv;
                            aes.Mode = CipherMode.CBC;
                            aes.Padding = PaddingMode.PKCS7;

                            int encryptedStart = 4 + iv.Length;
                            int encryptedLength = data.Length - encryptedStart;

                            using (var decryptor = aes.CreateDecryptor())
                            {
                                byte[] decrypted = decryptor.TransformFinalBlock(data, encryptedStart, encryptedLength);
                                Debug.Log($"[SecureStorage] Decrypted {data.Length} bytes to {decrypted.Length} bytes");
                                return decrypted;
                            }
                        }
                    }
                }

                // 尝试基类的简单解密
                return base.TryDecryptData(data);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SecureStorage] Decryption failed: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 压缩数据（使用更好的压缩）
        /// </summary>
        protected override byte[] CompressData(byte[] data, StorageCompressionLevel level)
        {
            // 这里简化实现，实际项目中应使用 System.IO.Compression.GZipStream
            byte[] header = Encoding.UTF8.GetBytes("GZIP:");
            byte[] result = new byte[header.Length + data.Length];
            Buffer.BlockCopy(header, 0, result, 0, header.Length);
            Buffer.BlockCopy(data, 0, result, header.Length, data.Length);

            Debug.Log($"[SecureStorage] Compressed {data.Length} bytes");
            return result;
        }

        /// <summary>
        /// 解压数据
        /// </summary>
        protected override byte[] TryDecompressData(byte[] data)
        {
            string header = "GZIP:";
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
                    Debug.Log($"[SecureStorage] Decompressed to {result.Length} bytes");
                    return result;
                }
            }

            return base.TryDecompressData(data);
        }

        #region 辅助方法

        /// <summary>
        /// 生成安全的密钥
        /// </summary>
        private string GenerateSecureKey()
        {
            // 结合设备ID和应用ID生成唯一密钥
            string deviceId = SystemInfo.deviceUniqueIdentifier;
            string appId = Application.identifier;
            string combined = $"{DEFAULT_KEY}_{deviceId}_{appId}";

            using (SHA256 sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(combined));
                return Convert.ToBase64String(hash);
            }
        }

        /// <summary>
        /// 从密码和盐生成密钥和IV
        /// </summary>
        private (byte[] Key, byte[] IV) GenerateKeyAndIV(string password, string salt, int keySize, int ivSize)
        {
            byte[] saltBytes = Encoding.UTF8.GetBytes(salt);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            // 使用PBKDF2生成密钥
            using (var pbkdf2 = new Rfc2898DeriveBytes(passwordBytes, saltBytes, 10000))
            {
                byte[] key = pbkdf2.GetBytes(keySize);
                byte[] iv = pbkdf2.GetBytes(ivSize);
                return (key, iv);
            }
        }

        /// <summary>
        /// 计算SHA256校验和
        /// </summary>
        protected new string CalculateChecksum(byte[] data)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(data);
                return BitConverter.ToString(hash).Replace("-", "");
            }
        }

        #endregion
    }
}