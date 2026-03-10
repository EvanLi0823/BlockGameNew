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

namespace BlockPuzzleGameToolkit.Scripts.PostPlacementSystem
{
    /// <summary>
    /// 放置后处理器接口
    /// 所有放置后需要执行的逻辑（消除、金币方块等）都实现此接口
    /// </summary>
    public interface IPostPlacementProcessor
    {
        /// <summary>
        /// 处理器优先级（数值越小越先执行）
        /// 典型优先级：
        /// - 消除检测：100
        /// - 金币方块：200
        /// - 其他特殊方块：300+
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// 是否可以执行（用于条件检查）
        /// </summary>
        /// <param name="context">上下文对象</param>
        /// <returns>true表示可以执行</returns>
        bool CanProcess(PostPlacementContext context);

        /// <summary>
        /// 执行处理逻辑（协程）
        /// </summary>
        /// <param name="context">上下文对象</param>
        /// <returns>协程枚举器</returns>
        IEnumerator Process(PostPlacementContext context);
    }
}
