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

using System;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.PropSystem.Core
{
    /// <summary>
    /// 道具数据类 - 存储道具类型和数量
    /// </summary>
    [Serializable]
    public class PropData
    {
        /// <summary>
        /// 道具类型
        /// </summary>
        [SerializeField]
        public PropType propType;

        /// <summary>
        /// 道具数量
        /// </summary>
        [SerializeField]
        public int propNum;

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public PropData()
        {
            propType = PropType.None;
            propNum = 0;
        }

        /// <summary>
        /// 带参数构造函数
        /// </summary>
        /// <param name="type">道具类型</param>
        /// <param name="num">道具数量</param>
        public PropData(PropType type, int num)
        {
            propType = type;
            propNum = Mathf.Max(0, num); // 确保数量不为负数
        }

        /// <summary>
        /// 克隆当前道具数据
        /// </summary>
        /// <returns>道具数据的深拷贝</returns>
        public PropData Clone()
        {
            return new PropData(propType, propNum);
        }

        /// <summary>
        /// 重写ToString方法，便于调试
        /// </summary>
        public override string ToString()
        {
            return $"PropData[Type: {propType}, Count: {propNum}]";
        }
    }
}