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
    /// 道具行为接口 - 定义所有道具行为的标准接口
    /// </summary>
    public interface IPropBehavior
    {
        /// <summary>
        /// 道具类型
        /// </summary>
        PropType PropType { get; }

        /// <summary>
        /// 是否需要选择目标
        /// </summary>
        bool RequiresTarget { get; }

        /// <summary>
        /// 初始化行为
        /// </summary>
        /// <param name="propManager">道具管理器引用</param>
        void Initialize(PropManager propManager);

        /// <summary>
        /// 开始选择目标
        /// </summary>
        void StartSelection();

        /// <summary>
        /// 取消选择
        /// </summary>
        void CancelSelection();

        /// <summary>
        /// 检查是否可以执行
        /// </summary>
        /// <param name="target">目标对象（可选）</param>
        /// <returns>是否可以执行</returns>
        bool CanExecute(object target = null);

        /// <summary>
        /// 执行道具效果
        /// </summary>
        /// <param name="target">目标对象（可选）</param>
        void Execute(object target = null);

        /// <summary>
        /// 显示预览效果
        /// </summary>
        /// <param name="target">预览目标</param>
        void ShowPreview(object target);

        /// <summary>
        /// 隐藏预览效果
        /// </summary>
        void HidePreview();

        /// <summary>
        /// 清理资源
        /// </summary>
        void Cleanup();
    }
}