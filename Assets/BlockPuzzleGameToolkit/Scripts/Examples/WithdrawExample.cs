using UnityEngine;
using BlockPuzzle.NativeBridge;
using BlockPuzzle.NativeBridge.Models;

namespace BlockPuzzleGameToolkit.Scripts.Examples
{
    /// <summary>
    /// 提现接口调用示例
    /// 演示如何调用NativeBridge的ShowWithdraw接口
    /// </summary>
    public class WithdrawExample : MonoBehaviour
    {
        /// <summary>
        /// 使用默认参数打开提现界面
        /// </summary>
        public void ShowWithdrawWithDefaults()
        {
            var nativeBridge = NativeBridgeManager.Instance;
            if (nativeBridge != null)
            {
                // 使用默认参数（全部为"0"）
                nativeBridge.ShowWithdrawInterface();
                Debug.Log("ShowWithdraw called with default parameters");
            }
            else
            {
                Debug.LogWarning("NativeBridgeManager not found");
            }
        }

        /// <summary>
        /// 使用自定义参数打开提现界面
        /// </summary>
        public void ShowWithdrawWithCustomParams()
        {
            var nativeBridge = NativeBridgeManager.Instance;
            if (nativeBridge != null)
            {
                // 创建自定义参数
                var withdrawParams = new WithdrawParams
                {
                    CurrentAmount = "600",
                    CurrentCoin = "600000",
                    CurrentBlock = "6000",
                    CurrentLevel = "10",
                    AdCount = "10",
                    MatchCount = "20"
                };

                nativeBridge.ShowWithdrawInterface(withdrawParams);
                Debug.Log("ShowWithdraw called with custom parameters");
            }
            else
            {
                Debug.LogWarning("NativeBridgeManager not found");
            }
        }

        /// <summary>
        /// 使用具体参数值打开提现界面
        /// </summary>
        public void ShowWithdrawWithDirectParams()
        {
            var nativeBridge = NativeBridgeManager.Instance;
            if (nativeBridge != null)
            {
                // 直接传递参数
                nativeBridge.ShowWithdrawInterface(
                    currentAmount: "600",
                    currentCoin: "600000",
                    currentBlock: "6000",
                    currentLevel: "10",
                    adCount: "10",
                    matchCount: "20"
                );
                Debug.Log("ShowWithdraw called with direct parameters");
            }
            else
            {
                Debug.LogWarning("NativeBridgeManager not found");
            }
        }

        /// <summary>
        /// 从游戏数据获取参数并打开提现界面（示例）
        /// 注意：这里使用占位数据，实际使用时需要从游戏系统获取真实数据
        /// </summary>
        public void ShowWithdrawWithGameData()
        {
            var nativeBridge = NativeBridgeManager.Instance;
            if (nativeBridge != null)
            {
                // TODO: 从游戏系统获取实际数据
                // 例如：
                // string currentAmount = PlayerData.GetCurrency().ToString();
                // string currentCoin = PlayerData.GetCoins().ToString();
                // string currentBlock = PlayerData.GetUniversalBlocks().ToString();
                // string currentLevel = GameManager.GetCurrentLevel().ToString();
                // string adCount = AdsManager.GetWatchedAdsCount().ToString();
                // string matchCount = GameStats.GetMatchCount().ToString();

                // 当前使用占位数据
                var withdrawParams = new WithdrawParams
                {
                    CurrentAmount = "0",  // TODO: 替换为实际货币数量
                    CurrentCoin = "0",    // TODO: 替换为实际金币数量
                    CurrentBlock = "0",   // TODO: 替换为实际万能方块数量
                    CurrentLevel = "0",   // TODO: 替换为实际关卡
                    AdCount = "0",        // TODO: 替换为实际看广告次数
                    MatchCount = "0"      // TODO: 替换为实际方块消除次数
                };

                nativeBridge.ShowWithdrawInterface(withdrawParams);
                Debug.Log($"ShowWithdraw called with game data: {JsonUtility.ToJson(withdrawParams)}");
            }
            else
            {
                Debug.LogWarning("NativeBridgeManager not found");
            }
        }
    }
}