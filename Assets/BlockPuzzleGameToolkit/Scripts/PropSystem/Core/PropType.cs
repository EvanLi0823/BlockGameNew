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

namespace BlockPuzzleGameToolkit.Scripts.PropSystem.Core
{
    /// <summary>
    /// 道具类型枚举
    /// </summary>
    public enum PropType
    {
        /// <summary>
        /// 无道具
        /// </summary>
        None = 0,

        /// <summary>
        /// 旋转道具 - 顺时针旋转选中的方块90度
        /// </summary>
        Rotate = 1,

        /// <summary>
        /// 刷新道具 - 重新生成所有待放置的方块
        /// </summary>
        Refresh = 2,

        /// <summary>
        /// 炸弹道具 - 清除选中位置3x3范围的格子
        /// </summary>
        Bomb = 3
    }
}