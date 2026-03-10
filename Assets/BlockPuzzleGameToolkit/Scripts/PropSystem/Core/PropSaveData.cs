// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.PropSystem.Core
{
    /// <summary>
    /// 道具存储数据类 - 管理所有道具的持久化存储
    /// </summary>
    [Serializable]
    public class PropSaveData
    {
        /// <summary>
        /// 道具列表
        /// </summary>
        [SerializeField]
        public List<PropData> props = new List<PropData>();

        /// <summary>
        /// 道具购买记录（用于每日限购功能）
        /// </summary>
        [SerializeField]
        public Dictionary<string, int> purchaseRecords = new Dictionary<string, int>();

        /// <summary>
        /// 上次重置购买记录的日期
        /// </summary>
        [SerializeField]
        public string lastResetDate = "";

        /// <summary>
        /// 获取指定道具的数量
        /// </summary>
        /// <param name="type">道具类型</param>
        /// <returns>道具数量，如果不存在返回0</returns>
        public int GetPropCount(PropType type)
        {
            var prop = props.FirstOrDefault(p => p.propType == type);
            return prop?.propNum ?? 0;
        }

        /// <summary>
        /// 设置指定道具的数量
        /// </summary>
        /// <param name="type">道具类型</param>
        /// <param name="count">道具数量</param>
        public void SetPropCount(PropType type, int count)
        {
            count = Mathf.Max(0, count); // 确保数量不为负数

            var prop = props.FirstOrDefault(p => p.propType == type);
            if (prop != null)
            {
                prop.propNum = count;
            }
            else
            {
                props.Add(new PropData(type, count));
            }
        }

        /// <summary>
        /// 增加或减少指定道具的数量
        /// </summary>
        /// <param name="type">道具类型</param>
        /// <param name="amount">增加的数量（负数表示减少）</param>
        /// <returns>操作后的道具数量</returns>
        public int AddProp(PropType type, int amount)
        {
            int currentCount = GetPropCount(type);
            int newCount = Mathf.Max(0, currentCount + amount);
            SetPropCount(type, newCount);
            return newCount;
        }

        /// <summary>
        /// 检查是否有足够的道具
        /// </summary>
        /// <param name="type">道具类型</param>
        /// <param name="requiredAmount">需要的数量</param>
        /// <returns>是否有足够的道具</returns>
        public bool HasEnoughProp(PropType type, int requiredAmount = 1)
        {
            return GetPropCount(type) >= requiredAmount;
        }

        /// <summary>
        /// 初始化道具数据
        /// </summary>
        /// <param name="initialProps">初始道具列表</param>
        public void Initialize(List<PropData> initialProps)
        {
            props.Clear();
            if (initialProps != null)
            {
                foreach (var prop in initialProps)
                {
                    props.Add(prop.Clone());
                }
            }
        }

        /// <summary>
        /// 清除所有道具数据
        /// </summary>
        public void Clear()
        {
            props.Clear();
            purchaseRecords.Clear();
            lastResetDate = "";
        }

        /// <summary>
        /// 获取今日广告购买次数
        /// </summary>
        /// <param name="propType">道具类型</param>
        /// <returns>今日广告购买次数</returns>
        public int GetTodayAdPurchaseCount(PropType propType)
        {
            CheckAndResetDailyRecords();
            string key = $"ad_{propType}";
            return purchaseRecords.ContainsKey(key) ? purchaseRecords[key] : 0;
        }

        /// <summary>
        /// 获取今日金币购买次数
        /// </summary>
        /// <param name="propType">道具类型</param>
        /// <returns>今日金币购买次数</returns>
        public int GetTodayCoinPurchaseCount(PropType propType)
        {
            CheckAndResetDailyRecords();
            string key = $"coin_{propType}";
            return purchaseRecords.ContainsKey(key) ? purchaseRecords[key] : 0;
        }

        /// <summary>
        /// 记录广告购买
        /// </summary>
        /// <param name="propType">道具类型</param>
        public void RecordAdPurchase(PropType propType)
        {
            CheckAndResetDailyRecords();
            string key = $"ad_{propType}";
            if (purchaseRecords.ContainsKey(key))
            {
                purchaseRecords[key]++;
            }
            else
            {
                purchaseRecords[key] = 1;
            }
        }

        /// <summary>
        /// 记录金币购买
        /// </summary>
        /// <param name="propType">道具类型</param>
        public void RecordCoinPurchase(PropType propType)
        {
            CheckAndResetDailyRecords();
            string key = $"coin_{propType}";
            if (purchaseRecords.ContainsKey(key))
            {
                purchaseRecords[key]++;
            }
            else
            {
                purchaseRecords[key] = 1;
            }
        }

        /// <summary>
        /// 检查并重置每日记录
        /// </summary>
        private void CheckAndResetDailyRecords()
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            if (lastResetDate != today)
            {
                purchaseRecords.Clear();
                lastResetDate = today;
            }
        }

        /// <summary>
        /// 重写ToString方法，便于调试
        /// </summary>
        public override string ToString()
        {
            var propStrings = props.Select(p => p.ToString());
            return $"PropSaveData[Props: {string.Join(", ", propStrings)}]";
        }
    }
}