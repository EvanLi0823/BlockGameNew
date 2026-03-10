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

namespace BlockPuzzleGameToolkit.Scripts.GameCore
{
    /// <summary>
    /// 泛型事件类
    /// 支持传递一个参数的事件系统
    /// 与 EventManager 配合使用，实现解耦的事件通信
    /// </summary>
    /// <typeparam name="T">事件参数的类型</typeparam>
    public class Event<T>
    {
        /// <summary>
        /// 私有事件委托，存储所有订阅者
        /// 订阅者方法需要接收一个类型为 T 的参数
        /// </summary>
        private event Action<T> EventDelegate;

        /// <summary>
        /// 订阅事件
        /// 将指定的方法添加到事件订阅者列表中
        /// </summary>
        /// <param name="subscriber">要订阅的方法，必须接收一个类型为 T 的参数</param>
        public void Subscribe(Action<T> subscriber)
        {
            EventDelegate += subscriber;
        }

        /// <summary>
        /// 取消订阅事件
        /// 从事件订阅者列表中移除指定的方法
        /// </summary>
        /// <param name="subscriber">要取消订阅的方法</param>
        public void Unsubscribe(Action<T> subscriber)
        {
            EventDelegate -= subscriber;
        }

        /// <summary>
        /// 触发事件
        /// 通知所有订阅者并传递参数
        /// 使用 ?. 运算符避免空引用异常
        /// </summary>
        /// <param name="arg">要传递给订阅者的参数</param>
        public void Invoke(T arg)
        {
            EventDelegate?.Invoke(arg);
        }
    }
}