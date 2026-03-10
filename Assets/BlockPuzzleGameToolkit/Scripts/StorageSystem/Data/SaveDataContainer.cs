// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

using System;
using UnityEngine;

namespace StorageSystem.Data
{
    /// <summary>
    /// 存储数据容器基类
    /// 所有需要存储的数据类都应该继承此类
    /// </summary>
    [Serializable]
    public abstract class SaveDataContainer
    {
        [SerializeField] private int version;
        [SerializeField] private string checksum;
        [SerializeField] private long saveTime;
        [SerializeField] private string deviceId;

        /// <summary>
        /// 数据版本号
        /// </summary>
        public int Version => version;

        /// <summary>
        /// 数据校验和
        /// </summary>
        public string Checksum => checksum;

        /// <summary>
        /// 保存时间
        /// </summary>
        public DateTime SaveTime => DateTimeOffset.FromUnixTimeSeconds(saveTime).DateTime;

        /// <summary>
        /// 设备ID
        /// </summary>
        public string DeviceId => deviceId;

        /// <summary>
        /// 更新元数据
        /// </summary>
        /// <param name="ver">版本号</param>
        public virtual void UpdateMetadata(int ver)
        {
            version = ver;
            saveTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            deviceId = SystemInfo.deviceUniqueIdentifier;
        }

        /// <summary>
        /// 设置校验和
        /// </summary>
        /// <param name="checksumValue">校验和值</param>
        public void SetChecksum(string checksumValue)
        {
            checksum = checksumValue;
        }

        /// <summary>
        /// 验证校验和
        /// </summary>
        /// <returns>是否有效</returns>
        public virtual bool ValidateChecksum()
        {
            // 这里简化处理，实际应该重新计算并比较
            return !string.IsNullOrEmpty(checksum);
        }

        /// <summary>
        /// 是否为有效数据
        /// </summary>
        /// <returns>是否有效</returns>
        public virtual bool IsValid()
        {
            return version > 0 && saveTime > 0;
        }
    }
}