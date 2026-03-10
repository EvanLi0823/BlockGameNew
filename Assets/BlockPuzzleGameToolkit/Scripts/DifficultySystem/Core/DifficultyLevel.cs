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

namespace GameCore.DifficultySystem
{
    /// <summary>
    /// 关卡难度等级
    /// </summary>
    public enum DifficultyLevel
    {
        /// <summary>教学关卡 (0-15分)</summary>
        Tutorial = 0,

        /// <summary>简单关卡 (16-30分)</summary>
        Easy = 1,

        /// <summary>普通关卡 (31-50分)</summary>
        Normal = 2,

        /// <summary>困难关卡 (51-70分)</summary>
        Hard = 3,

        /// <summary>专家关卡 (71-85分)</summary>
        Expert = 4,

        /// <summary>大师关卡 (86-100分)</summary>
        Master = 5
    }
}
