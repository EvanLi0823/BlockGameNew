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

using System;

namespace GameCore.DifficultySystem
{
    /// <summary>
    /// 游戏状态（用于动态难度控制）
    /// 新版本：关卡级难度调整（非方块级）
    /// </summary>
    [Serializable]
    public class GameState
    {
        // ========== 废弃字段（向后兼容） ==========
        /// <summary>当前放置次数</summary>
        [Obsolete("使用关卡级难度调整，此字段已废弃，将在v2.0移除")]
        public int placementCount;

        /// <summary>连续通关次数（方块级）</summary>
        [Obsolete("使用consecutiveCleanWins代替，此字段已废弃，将在v2.0移除")]
        public int consecutiveWins;

        /// <summary>连续失败次数（方块级）</summary>
        [Obsolete("使用currentLevelFailures代替，此字段已废弃，将在v2.0移除")]
        public int consecutiveFailures;

        // ========== 新字段（关卡级） ==========
        /// <summary>连续一次性通关次数（关卡级，用于难度提升）</summary>
        public int consecutiveCleanWins;

        /// <summary>当前关卡失败次数（每关独立，用于难度降低）</summary>
        public int currentLevelFailures;

        /// <summary>当前关卡是否失败过（标记位，判断是否一次性通关）</summary>
        public bool currentLevelHasFailure;

        // ========== 保留字段 ==========
        /// <summary>累计失败次数</summary>
        public int totalFailures;

        /// <summary>当前分数</summary>
        public int currentScore;

        /// <summary>已消耗时间（秒）</summary>
        public float elapsedTime;

        /// <summary>当前难度状态</summary>
        public DifficultyState currentState = DifficultyState.Normal;

        /// <summary>是否处于降低难度模式</summary>
        public bool isDifficultyDecreased;

        // ========== 关卡开始 ==========
        /// <summary>
        /// 关卡开始时调用
        /// 重置当前关卡的失败计数和标记
        /// </summary>
        public void OnLevelStarted()
        {
            currentLevelFailures = 0;
            currentLevelHasFailure = false;
        }

        // ========== 关卡失败 ==========
        /// <summary>
        /// 关卡失败时调用（Retry/Revive都算失败）
        /// </summary>
        public void OnLevelFailed()
        {
            currentLevelFailures++;
            currentLevelHasFailure = true;
            totalFailures++;
        }

        // ========== 关卡通关 ==========
        /// <summary>
        /// 关卡通关时调用
        /// 根据是否一次性通关决定连胜计数
        /// </summary>
        public void OnLevelCompleted()
        {
            // 如果本关有失败过，打断连胜
            if (currentLevelHasFailure)
            {
                consecutiveCleanWins = 0;

                // 退出提升模式
                if (currentState == DifficultyState.Increased)
                {
                    currentState = DifficultyState.Normal;
                }
            }
            else
            {
                // 一次性通关，增加连胜
                consecutiveCleanWins++;
            }

            // 无论如何，通关后退出降低模式
            if (currentState == DifficultyState.Decreased)
            {
                currentState = DifficultyState.Normal;
                isDifficultyDecreased = false;
            }
        }

        // ========== 模式切换 ==========
        /// <summary>
        /// 进入降低难度模式（连续失败M次后触发）
        /// </summary>
        public void EnterDecreasedMode()
        {
            currentState = DifficultyState.Decreased;
            isDifficultyDecreased = true;
        }

        /// <summary>
        /// 进入提升难度模式（连续N关一次性通关后触发）
        /// </summary>
        public void EnterIncreasedMode()
        {
            currentState = DifficultyState.Increased;
        }

        /// <summary>
        /// 退出提升难度模式（有失败时触发）
        /// </summary>
        public void ExitIncreasedMode()
        {
            if (currentState == DifficultyState.Increased)
            {
                currentState = DifficultyState.Normal;
            }
        }

        // ========== 重置方法 ==========
        /// <summary>
        /// 重置所有计数器（用于完全重置）
        /// </summary>
        public void ResetAllCounters()
        {
            consecutiveCleanWins = 0;
            currentLevelFailures = 0;
            currentLevelHasFailure = false;
            currentState = DifficultyState.Normal;
            isDifficultyDecreased = false;
        }

        // ========== 废弃方法（向后兼容） ==========
        [Obsolete("使用OnLevelFailed代替")]
        public void ResetFailureCount()
        {
            consecutiveFailures = 0;
        }

        [Obsolete("使用OnLevelFailed代替")]
        public void RecordFailure()
        {
            consecutiveWins = 0;
            consecutiveFailures++;
            totalFailures++;
        }

        [Obsolete("不再使用方块级记录")]
        public void RecordPlacement()
        {
            consecutiveWins++;
            consecutiveFailures = 0;
            placementCount++;
        }

        [Obsolete("使用OnLevelCompleted代替")]
        public void ExitDecreasedMode()
        {
            currentState = DifficultyState.Normal;
            isDifficultyDecreased = false;
        }
    }
}
