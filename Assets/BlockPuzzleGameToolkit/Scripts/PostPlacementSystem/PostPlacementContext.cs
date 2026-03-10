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
using BlockPuzzleGameToolkit.Scripts.Gameplay;
using BlockPuzzleGameToolkit.Scripts.Enums;

namespace BlockPuzzleGameToolkit.Scripts.PostPlacementSystem
{
    /// <summary>
    /// 放置后处理上下文
    /// 封装所有处理器需要的数据和状态
    /// 替代原SharedData，提供更好的封装性和类型安全
    /// </summary>
    public class PostPlacementContext
    {
        // ========== 基础数据 ==========
        /// <summary>
        /// 刚放置的Shape对象
        /// </summary>
        public Shape PlacedShape { get; private set; }

        /// <summary>
        /// 放置前的游戏状态（用于恢复）
        /// </summary>
        public EGameState PreviousState { get; private set; }

        // ========== 消除相关数据 ==========
        /// <summary>
        /// 本次消除的总行数
        /// </summary>
        public int EliminatedLines { get; set; }

        /// <summary>
        /// 被消除的行索引列表
        /// </summary>
        public List<int> EliminatedRowIndices { get; private set; }

        /// <summary>
        /// 被消除的列索引列表
        /// </summary>
        public List<int> EliminatedColumnIndices { get; private set; }

        // ========== 得分相关数据 ==========
        /// <summary>
        /// 本次操作获得的总得分
        /// </summary>
        public int TotalScore { get; set; }

        /// <summary>
        /// 是否触发了连消
        /// </summary>
        public bool IsCombo { get; set; }

        // ========== 处理器共享数据 ==========
        /// <summary>
        /// 自定义数据字典（用于处理器之间传递数据）
        /// 例如：MoneyBlockProcessor可以将生成的金币数量存入
        /// </summary>
        private readonly Dictionary<string, object> customData = new Dictionary<string, object>();

        // ========== 构造函数 ==========
        /// <summary>
        /// 创建放置后处理上下文
        /// </summary>
        /// <param name="placedShape">刚放置的Shape</param>
        /// <param name="previousState">放置前的游戏状态</param>
        public PostPlacementContext(Shape placedShape, EGameState previousState)
        {
            PlacedShape = placedShape;
            PreviousState = previousState;
            EliminatedRowIndices = new List<int>();
            EliminatedColumnIndices = new List<int>();
            EliminatedLines = 0;
            TotalScore = 0;
            IsCombo = false;
        }

        // ========== 自定义数据方法 ==========
        /// <summary>
        /// 设置自定义数据
        /// </summary>
        public void SetData<T>(string key, T value)
        {
            customData[key] = value;
        }

        /// <summary>
        /// 获取自定义数据
        /// </summary>
        public T GetData<T>(string key, T defaultValue = default)
        {
            if (customData.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }
            return defaultValue;
        }

        /// <summary>
        /// 检查是否包含某个键
        /// </summary>
        public bool HasData(string key)
        {
            return customData.ContainsKey(key);
        }

        /// <summary>
        /// 清除自定义数据
        /// </summary>
        public void ClearCustomData()
        {
            customData.Clear();
        }
    }
}
