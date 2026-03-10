// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

using System;

namespace StorageSystem.Core
{
    /// <summary>
    /// 存储策略接口
    /// </summary>
    public interface IStorageStrategy
    {
        /// <summary>
        /// 保存数据到指定路径
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="data">要保存的数据</param>
        /// <param name="options">存储选项</param>
        /// <returns>是否保存成功</returns>
        bool Save(string path, object data, StorageOptions options);

        /// <summary>
        /// 从指定路径加载数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="path">文件路径</param>
        /// <returns>加载的数据</returns>
        T Load<T>(string path) where T : class;

        /// <summary>
        /// 删除指定路径的数据
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>是否删除成功</returns>
        bool Delete(string path);

        /// <summary>
        /// 检查文件是否存在
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>是否存在</returns>
        bool Exists(string path);

        /// <summary>
        /// 获取存储类型标识
        /// </summary>
        /// <returns>存储类型</returns>
        StorageType GetStorageType();

        /// <summary>
        /// 获取文件扩展名
        /// </summary>
        /// <returns>扩展名</returns>
        string GetFileExtension();
    }

    /// <summary>
    /// 存储类型枚举
    /// </summary>
    public enum StorageType
    {
        Binary,           // 二进制
        Json,             // JSON文本
        SecureBinary,     // 加密二进制
        CompressedBinary, // 压缩二进制
        PlayerPrefs,      // Unity PlayerPrefs
        Custom            // 自定义
    }

    /// <summary>
    /// 存储选项配置
    /// </summary>
    [Serializable]
    public class StorageOptions
    {
        public bool useEncryption = false;      // 是否加密
        public bool useCompression = false;     // 是否压缩
        public bool addChecksum = true;         // 是否添加校验和
        public int version = 1;                 // 数据版本
        public string encryptionKey = "";       // 加密密钥
        public StorageCompressionLevel compression = StorageCompressionLevel.Fast;
    }

    /// <summary>
    /// 存储压缩级别
    /// </summary>
    public enum StorageCompressionLevel
    {
        Fast,    // 快速压缩
        Normal,  // 标准压缩
        High     // 高压缩率
    }
}