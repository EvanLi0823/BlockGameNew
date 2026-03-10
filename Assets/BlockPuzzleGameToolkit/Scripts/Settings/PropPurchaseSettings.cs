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
using BlockPuzzleGameToolkit.Scripts.PropSystem.Core;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Settings
{
    /// <summary>
    /// 道具购买设置 - 管理道具的购买配置
    /// </summary>
    [CreateAssetMenu(fileName = "PropPurchaseSettings", menuName = "BlockPuzzle/Props/PurchaseSettings", order = 101)]
    public class PropPurchaseSettings : SingletonScriptableSettings<PropPurchaseSettings>
    {
        /// <summary>
        /// 道具购买配置项
        /// </summary>
        [Serializable]
        public class PropPurchaseConfig
        {
            [Header("道具类型")]
            [Tooltip("要购买的道具类型")]
            public PropType propType;

            [Header("广告购买配置")]
            [Tooltip("是否可以通过观看广告购买")]
            public bool canPurchaseWithAds = true;

            [Tooltip("观看广告获得的道具数量")]
            [Range(1, 10)]
            public int adsRewardAmount = 3;

            [Tooltip("每日广告购买限制（-1表示无限制）")]
            public int dailyAdLimit = -1;

            [Header("金币购买配置")]
            [Tooltip("是否可以通过金币购买")]
            public bool canPurchaseWithCoins = true;

            [Tooltip("金币价格")]
            [Range(10, 1000)]
            public int coinPrice = 100;

            [Tooltip("金币购买获得的道具数量")]
            [Range(1, 20)]
            public int coinPurchaseAmount = 5;

            [Tooltip("每日金币购买限制（-1表示无限制）")]
            public int dailyCoinLimit = -1;

            [Header("其他配置")]
            [Tooltip("是否在商店中显示")]
            public bool showInShop = true;

            [Tooltip("商店中的显示顺序")]
            public int shopOrder = 0;

            /// <summary>
            /// 验证配置是否有效
            /// </summary>
            public bool IsValid()
            {
                if (propType == PropType.None)
                {
                    Debug.LogError("PropPurchaseConfig: 道具类型不能为None");
                    return false;
                }

                if (!canPurchaseWithAds && !canPurchaseWithCoins)
                {
                    Debug.LogError($"PropPurchaseConfig: 道具 {propType} 没有任何购买方式");
                    return false;
                }

                if (canPurchaseWithAds && adsRewardAmount <= 0)
                {
                    Debug.LogError($"PropPurchaseConfig: 道具 {propType} 的广告奖励数量必须大于0");
                    return false;
                }

                if (canPurchaseWithCoins)
                {
                    if (coinPrice <= 0)
                    {
                        Debug.LogError($"PropPurchaseConfig: 道具 {propType} 的金币价格必须大于0");
                        return false;
                    }
                    if (coinPurchaseAmount <= 0)
                    {
                        Debug.LogError($"PropPurchaseConfig: 道具 {propType} 的金币购买数量必须大于0");
                        return false;
                    }
                }

                return true;
            }
        }

        [Header("道具购买配置")]
        [Tooltip("配置每种道具通过广告或金币购买时获得的数量")]
        public List<PropPurchaseConfig> purchaseConfigs = new List<PropPurchaseConfig>
        {
            new PropPurchaseConfig
            {
                propType = PropType.Rotate,
                canPurchaseWithAds = true,
                adsRewardAmount = 3,
                canPurchaseWithCoins = true,
                coinPrice = 100,
                coinPurchaseAmount = 5,
                dailyAdLimit = -1,
                dailyCoinLimit = -1,
                shopOrder = 1
            },
            new PropPurchaseConfig
            {
                propType = PropType.Refresh,
                canPurchaseWithAds = true,
                adsRewardAmount = 3,
                canPurchaseWithCoins = true,
                coinPrice = 100,
                coinPurchaseAmount = 5,
                dailyAdLimit = -1,
                dailyCoinLimit = -1,
                shopOrder = 2
            },
            new PropPurchaseConfig
            {
                propType = PropType.Bomb,
                canPurchaseWithAds = true,
                adsRewardAmount = 1,
                canPurchaseWithCoins = true,
                coinPrice = 200,
                coinPurchaseAmount = 3,
                dailyAdLimit = -1,
                dailyCoinLimit = -1,
                shopOrder = 3
            }
        };

        [Header("弹窗UI配置")]
        [Tooltip("道具购买弹窗预制体")]
        public GameObject propPurchasePopupPrefab;

        [Header("购买限制")]
        [Tooltip("是否启用每日购买限制")]
        public bool enableDailyLimits = false;

        [Tooltip("每日限制重置时间（小时，0-23）")]
        [Range(0, 23)]
        public int dailyResetHour = 0;

        /// <summary>
        /// 根据道具类型获取购买配置
        /// </summary>
        /// <param name="type">道具类型</param>
        /// <returns>购买配置，如果不存在返回null</returns>
        public PropPurchaseConfig GetPurchaseConfig(PropType type)
        {
            return purchaseConfigs.FirstOrDefault(c => c != null && c.propType == type);
        }

        /// <summary>
        /// 检查是否可以通过广告购买
        /// </summary>
        /// <param name="type">道具类型</param>
        /// <returns>是否可以通过广告购买</returns>
        public bool CanPurchaseWithAds(PropType type)
        {
            var config = GetPurchaseConfig(type);
            return config != null && config.canPurchaseWithAds;
        }

        /// <summary>
        /// 检查是否可以通过金币购买
        /// </summary>
        /// <param name="type">道具类型</param>
        /// <returns>是否可以通过金币购买</returns>
        public bool CanPurchaseWithCoins(PropType type)
        {
            var config = GetPurchaseConfig(type);
            return config != null && config.canPurchaseWithCoins;
        }

        /// <summary>
        /// 获取广告奖励数量
        /// </summary>
        /// <param name="type">道具类型</param>
        /// <returns>广告奖励数量</returns>
        public int GetAdsRewardAmount(PropType type)
        {
            var config = GetPurchaseConfig(type);
            return config?.adsRewardAmount ?? 0;
        }

        /// <summary>
        /// 获取金币价格
        /// </summary>
        /// <param name="type">道具类型</param>
        /// <returns>金币价格</returns>
        public int GetCoinPrice(PropType type)
        {
            var config = GetPurchaseConfig(type);
            return config?.coinPrice ?? 0;
        }

        /// <summary>
        /// 获取金币购买数量
        /// </summary>
        /// <param name="type">道具类型</param>
        /// <returns>金币购买数量</returns>
        public int GetCoinPurchaseAmount(PropType type)
        {
            var config = GetPurchaseConfig(type);
            return config?.coinPurchaseAmount ?? 0;
        }

        /// <summary>
        /// 获取按商店顺序排序的购买配置
        /// </summary>
        /// <returns>排序后的配置列表</returns>
        public List<PropPurchaseConfig> GetSortedShopConfigs()
        {
            return purchaseConfigs
                .Where(c => c != null && c.showInShop)
                .OrderBy(c => c.shopOrder)
                .ToList();
        }

        /// <summary>
        /// 验证配置是否完整
        /// </summary>
        /// <returns>配置是否有效</returns>
        public bool ValidateConfiguration()
        {
            bool isValid = true;

            // 检查购买配置列表
            if (purchaseConfigs == null || purchaseConfigs.Count == 0)
            {
                Debug.LogError("PropPurchaseSettings: 购买配置列表为空");
                isValid = false;
            }
            else
            {
                // 检查每个配置的有效性
                foreach (var config in purchaseConfigs)
                {
                    if (config == null)
                    {
                        Debug.LogError("PropPurchaseSettings: 购买配置列表中存在null配置");
                        isValid = false;
                    }
                    else if (!config.IsValid())
                    {
                        isValid = false;
                    }
                }

                // 检查是否有重复的道具类型
                var duplicateTypes = purchaseConfigs
                    .Where(c => c != null)
                    .GroupBy(c => c.propType)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key);

                foreach (var type in duplicateTypes)
                {
                    Debug.LogError($"PropPurchaseSettings: 道具类型 {type} 有重复的购买配置");
                    isValid = false;
                }
            }

            // 检查UI配置
            if (propPurchasePopupPrefab == null)
            {
                Debug.LogWarning("PropPurchaseSettings: 购买弹窗预制体未设置");
            }

            return isValid;
        }

        /// <summary>
        /// 在Inspector中值改变时调用
        /// </summary>
        private void OnValidate()
        {
            // 确保每日重置时间在有效范围内
            dailyResetHour = Mathf.Clamp(dailyResetHour, 0, 23);

            // 验证每个购买配置
            if (purchaseConfigs != null)
            {
                foreach (var config in purchaseConfigs)
                {
                    if (config != null)
                    {
                        // 确保数量在合理范围内
                        config.adsRewardAmount = Mathf.Clamp(config.adsRewardAmount, 1, 10);
                        config.coinPurchaseAmount = Mathf.Clamp(config.coinPurchaseAmount, 1, 20);
                        config.coinPrice = Mathf.Clamp(config.coinPrice, 10, 1000);

                        // 如果不启用每日限制，确保限制值为-1
                        if (!enableDailyLimits)
                        {
                            config.dailyAdLimit = -1;
                            config.dailyCoinLimit = -1;
                        }
                    }
                }
            }
        }
    }
}