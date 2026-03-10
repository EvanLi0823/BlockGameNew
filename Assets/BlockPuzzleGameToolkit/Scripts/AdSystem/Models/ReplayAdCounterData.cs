using System;

namespace BlockPuzzle.AdSystem.Models
{
    /// <summary>
    /// Replay广告计数器数据模型
    /// 用于持久化存储计数器状态
    /// </summary>
    [Serializable]
    public class ReplayAdCounterData
    {
        /// <summary>
        /// 当前Replay次数计数器
        /// </summary>
        public int counter;

        /// <summary>
        /// 累计广告播放总次数
        /// </summary>
        public int totalPlayCount;

        /// <summary>
        /// 最后更新时间戳（Unix时间戳，单位：秒）
        /// </summary>
        public long lastUpdateTime;

        /// <summary>
        /// 构造函数，初始化默认值
        /// </summary>
        public ReplayAdCounterData()
        {
            counter = 0;
            totalPlayCount = 0;
            lastUpdateTime = 0;
        }

        /// <summary>
        /// 创建默认数据
        /// </summary>
        public static ReplayAdCounterData CreateDefault()
        {
            return new ReplayAdCounterData();
        }
    }
}
