// // ©2015 - 2025 Candy Smith
// // All rights reserved
// // Redistribution of this software is strictly not allowed.
// // Copy of this software can be obtained from unity asset store only.
// // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// // IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// // FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// // AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// // LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// // OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// // THE SOFTWARE.

using System.Collections.Generic;
using BlockPuzzleGameToolkit.Scripts.Data;
using BlockPuzzleGameToolkit.Scripts.GUI;
// using BlockPuzzleGameToolkit.Scripts.Services.IAP; // IAP功能已移除
using TMPro;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Popups
{
    public class ItemPurchase : MonoBehaviour
    {
        public CustomButton BuyItemButton;
        public TextMeshProUGUI price;
        public TextMeshProUGUI count;
        public TextMeshProUGUI discountPercent;

        // IAP相关字段已简化
        [HideInInspector]
        public KeyValuePair<object, int> settingsShopItem;

        public string productID; // 简化为string类型

        [SerializeField]
        public ResourceObject resource;

        private void OnEnable()
        {
            BuyItemButton?.onClick.AddListener(BuyCoins);
            // IAP价格显示功能已移除
            if (!string.IsNullOrEmpty(productID))
            {
                price.text = "$0.99"; // 默认价格显示
            }
        }

        private void BuyCoins()
        {
            // CoinsShop 功能已移除
            // 原本会调用父组件的 CoinsShop 进行购买
            Debug.Log($"[ItemPurchase] CoinsShop功能已移除，无法购买: {productID}");
        }
    }

    internal class NoAdsItemPurchase : ItemPurchase
    {
    }
}