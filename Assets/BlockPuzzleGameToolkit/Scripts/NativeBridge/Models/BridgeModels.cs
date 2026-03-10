using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BlockPuzzle.NativeBridge.Models
{
    /// <summary>
    /// 桥接消息基类
    /// </summary>
    [Serializable]
    public class BridgeMessage
    {
        /// <summary>
        /// 方法名
        /// </summary>
        [JsonProperty("m")]
        public string m;

        /// <summary>
        /// 参数1
        /// </summary>
        [JsonProperty("p1")]
        public string p1;

        /// <summary>
        /// 参数2
        /// </summary>
        [JsonProperty("p2")]
        public string p2;

        public BridgeMessage(string methodName, object param1 = null, object param2 = null)
        {
            m = methodName;
            p1 = param1?.ToString();
            p2 = param2?.ToString();
        }
    }

    /// <summary>
    /// 桥接响应基类
    /// </summary>
    [Serializable]
    public class BridgeResponse
    {
        /// <summary>
        /// 响应数据
        /// </summary>
        public Dictionary<string, object> data;

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool success;

        /// <summary>
        /// 错误消息
        /// </summary>
        public string error;

        public BridgeResponse()
        {
            data = new Dictionary<string, object>();
            success = true;
        }
    }

    /// <summary>
    /// 公共参数响应
    /// </summary>
    [Serializable]
    public class CommonParamResponse
    {
        [JsonProperty("language")]
        public string language = "en";

        [JsonProperty("country")]
        public string country = "cn";

        [JsonProperty("numberGK")]
        public int numberGK = 0;
    }

    /// <summary>
    /// H5初始化结果
    /// </summary>
    [Serializable]
    public class H5InitResult
    {
        [JsonProperty("amount")]
        public string amount;
    }

    /// <summary>
    /// 广告播放结果
    /// </summary>
    [Serializable]
    public class AdPlayResult
    {
        [JsonProperty("amount")]
        public string amount;

        [JsonProperty("success")]
        public bool success;
    }

    /// <summary>
    /// 货币符号响应
    /// </summary>
    [Serializable]
    public class CurrencyResponse
    {
        [JsonProperty("amount")]
        public string amount = "$";
    }
}