using System;
using Newtonsoft.Json;

namespace BlockPuzzle.NativeBridge.Models
{
    /// <summary>
    /// 提现接口参数模型
    /// </summary>
    [Serializable]
    public class WithdrawParams
    {
        /// <summary>
        /// 当前货币数量
        /// </summary>
        [JsonProperty("currentAmount")]
        public string CurrentAmount { get; set; }

        /// <summary>
        /// 当前金币
        /// </summary>
        [JsonProperty("currentCoin")]
        public string CurrentCoin { get; set; }

        /// <summary>
        /// 万能方块数量
        /// </summary>
        [JsonProperty("currentBlock")]
        public string CurrentBlock { get; set; }

        /// <summary>
        /// 当前关卡
        /// </summary>
        [JsonProperty("currentLevel")]
        public string CurrentLevel { get; set; }

        /// <summary>
        /// 看广告次数
        /// </summary>
        [JsonProperty("adCount")]
        public string AdCount { get; set; }

        /// <summary>
        /// 方块消除次数
        /// </summary>
        [JsonProperty("matchCount")]
        public string MatchCount { get; set; }

        /// <summary>
        /// 创建默认参数实例
        /// </summary>
        public static WithdrawParams CreateDefault()
        {
            return new WithdrawParams
            {
                CurrentAmount = "0",
                CurrentCoin = "0",
                CurrentBlock = "0",
                CurrentLevel = "0",
                AdCount = "0",
                MatchCount = "0"
            };
        }
    }
}