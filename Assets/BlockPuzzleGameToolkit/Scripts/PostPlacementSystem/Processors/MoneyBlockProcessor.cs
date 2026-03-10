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

using System.Collections;
using BlockPuzzleGameToolkit.Scripts.MoneyBlockSystem;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.PostPlacementSystem.Processors
{
    /// <summary>
    /// 金钱方块处理器
    /// 适配MoneyBlockManager到PostPlacement系统
    /// 优先级：200（在消除处理器之后执行）
    ///
    /// 注意：
    /// - MoneyBlockManager已经通过IBonusCollector接口集成到TargetManager
    /// - 金钱方块的消除和飞行动画由TargetManager和BonusAnimationManager处理
    /// - 此处理器主要确保PostPlacement状态期间MoneyBlockManager能正常工作
    /// - 等待累计奖励弹窗完成，避免与其他系统冲突
    /// </summary>
    public class MoneyBlockProcessor : IPostPlacementProcessor
    {
        // ========== 优先级 ==========
        public int Priority => 200;  // 在消除处理器之后执行

        // ========== IPostPlacementProcessor实现 ==========
        /// <summary>
        /// 检查是否可以执行
        /// </summary>
        public bool CanProcess(PostPlacementContext context)
        {
            // 检查MoneyBlockManager是否存在且已初始化
            var moneyBlockManager = MoneyBlockManager.Instance;
            if (moneyBlockManager == null || !moneyBlockManager.IsInitialized)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 执行金钱方块处理逻辑
        /// </summary>
        public IEnumerator Process(PostPlacementContext context)
        {
            var moneyBlockManager = MoneyBlockManager.Instance;
            if (moneyBlockManager == null)
            {
                yield break;
            }

            // 等待MoneyBlockManager处理完累计奖励弹窗
            // MoneyBlockManager.IsProcessingCumulative 表示是否正在显示累计奖励弹窗
            // 需要等待弹窗完成，避免与游戏状态判定冲突
            float timeout = 10f;  // 10秒超时保护
            float elapsedTime = 0f;
            while (moneyBlockManager.IsProcessingCumulative)
            {
                yield return null;
                elapsedTime += Time.deltaTime;

                // 超时保护：避免死锁
                if (elapsedTime > timeout)
                {
                    Debug.LogWarning("[MoneyBlockProcessor] 等待累计奖励弹窗超时，强制继续游戏流程");
                    break;
                }
            }

            // 金钱方块的实际处理由MoneyBlockManager通过IBonusCollector接口完成
            // 这包括：
            // 1. 消除动画（BonusAnimationManager负责）
            // 2. 飞行动画（BonusAnimationManager负责）
            // 3. 奖励计算（MoneyBlockManager.OnBonusCollected回调）
            // 4. 累计奖励弹窗（MoneyBlockManager.TriggerCumulativeReward）

            // 此处理器的主要作用是确保PostPlacement状态下等待所有异步操作完成
            Debug.Log("[MoneyBlockProcessor] 金钱方块处理完成");
        }
    }
}
