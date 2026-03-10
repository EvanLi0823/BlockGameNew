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
using System.Collections.Generic;
using BlockPuzzleGameToolkit.Scripts.Enums;

namespace BlockPuzzleGameToolkit.Scripts.GameCore
{
    /// <summary>
    /// 事件管理器 - 游戏全局事件系统的核心管理类
    /// 负责事件的注册、订阅、取消订阅和触发
    /// 使用静态类确保全局唯一性
    /// </summary>
    public static class EventManager
    {
        /// <summary>
        /// 存储所有事件的字典容器
        /// Key: 事件类型枚举 EGameEvent
        /// Value: 事件对象（可以是 Event 或 Event<T>）
        /// </summary>
        private static readonly Dictionary<EGameEvent, object> events = new();

        /// <summary>
        /// 获取或创建一个带参数的泛型事件
        /// </summary>
        /// <typeparam name="T">事件参数类型</typeparam>
        /// <param name="eventName">事件名称枚举</param>
        /// <returns>返回对应的事件对象，如果不存在则创建新的</returns>
        public static Event<T> GetEvent<T>(EGameEvent eventName)
        {
            // 尝试从字典中获取已存在的事件，并转换为正确的类型
            if (events.TryGetValue(eventName, out var e) && e is Event<T> typedEvent)
            {
                return typedEvent;
            }

            // 如果事件不存在，创建新的泛型事件并存入字典
            var newEvent = new Event<T>();
            events[eventName] = newEvent;
            return newEvent;
        }

        /// <summary>
        /// 获取或创建一个无参数的事件
        /// </summary>
        /// <param name="eventName">事件名称枚举</param>
        /// <returns>返回对应的无参事件对象，如果不存在则创建新的</returns>
        public static Event GetEvent(EGameEvent eventName)
        {
            // 尝试从字典中获取已存在的事件，并转换为无参事件类型
            if (events.TryGetValue(eventName, out var e) && e is Event typedEvent)
            {
                return typedEvent;
            }

            // 如果事件不存在，创建新的无参事件并存入字典
            var newEvent = new Event();
            events[eventName] = newEvent;
            return newEvent;
        }

        /// <summary>
        /// 获取所有已订阅的事件字典
        /// 主要用于调试和监控
        /// </summary>
        /// <returns>返回包含所有事件的字典</returns>
        public static Dictionary<EGameEvent, object> GetSubscribedEvents()
        {
            return events;
        }

        /// <summary>
        /// 游戏状态变化时的回调委托
        /// 当 GameStatus 属性值改变时触发
        /// </summary>
        public static Action<EGameState> OnGameStateChanged;

        /// <summary>
        /// 私有字段，存储当前游戏状态
        /// </summary>
        private static EGameState gameStatus;

        /// <summary>
        /// 游戏状态属性
        /// 设置新值时会自动触发 OnGameStateChanged 事件
        /// </summary>
        public static EGameState GameStatus
        {
            get => gameStatus;
            set
            {
                // 更新游戏状态并触发状态变化事件
                gameStatus = value;
                OnGameStateChanged?.Invoke(gameStatus);
            }
        }
    }

    /// <summary>
    /// 无参数事件类
    /// 提供基本的事件订阅、取消订阅和触发功能
    /// </summary>
    public class Event
    {
        /// <summary>
        /// 私有事件委托，存储所有订阅者
        /// </summary>
        private event Action EventDelegate;

        /// <summary>
        /// 订阅事件
        /// </summary>
        /// <param name="subscriber">要订阅的方法</param>
        public void Subscribe(Action subscriber)
        {
            EventDelegate += subscriber;
        }

        /// <summary>
        /// 取消订阅事件
        /// </summary>
        /// <param name="subscriber">要取消订阅的方法</param>
        public void Unsubscribe(Action subscriber)
        {
            EventDelegate -= subscriber;
        }

        /// <summary>
        /// 触发事件，通知所有订阅者
        /// 使用 ?. 运算符避免空引用异常
        /// </summary>
        public void Invoke()
        {
            EventDelegate?.Invoke();
        }
    }
}