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
using System.Collections.Generic;
using BlockPuzzleGameToolkit.Scripts.Gameplay;
using BlockPuzzleGameToolkit.Scripts.Gameplay.Managers;

namespace BlockPuzzleGameToolkit.Scripts.PostPlacementSystem.Processors
{
    /// <summary>
    /// 消除处理器
    /// 封装原AfterMoveProcessing逻辑，负责执行消除、得分、动画等
    /// 优先级：100（最先执行）
    /// </summary>
    public class EliminationProcessor : IPostPlacementProcessor
    {
        // ========== 优先级 ==========
        public int Priority => 100;  // 消除处理器优先级最高

        // ========== 依赖项 ==========
        private readonly LevelManager levelManager;

        // ========== 构造函数 ==========
        /// <summary>
        /// 创建消除处理器
        /// </summary>
        /// <param name="levelManager">LevelManager实例（依赖注入）</param>
        public EliminationProcessor(LevelManager levelManager)
        {
            this.levelManager = levelManager;
        }

        // ========== IPostPlacementProcessor实现 ==========
        /// <summary>
        /// 检查是否可以执行（消除处理器总是执行）
        /// </summary>
        public bool CanProcess(PostPlacementContext context)
        {
            // 消除处理器总是需要执行，因为即使没有消除也需要检查游戏状态
            return true;
        }

        /// <summary>
        /// 执行消除处理逻辑
        /// </summary>
        public IEnumerator Process(PostPlacementContext context)
        {
            // 检查是否有消除
            if (context.EliminatedLines <= 0)
            {
                // 没有消除，直接返回
                yield break;
            }

            // 委托给LevelManager的ProcessElimination方法
            yield return levelManager.ProcessElimination(context);
        }
    }
}
