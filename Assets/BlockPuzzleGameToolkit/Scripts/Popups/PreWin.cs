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

using UnityEngine;
using BlockPuzzleGameToolkit.Scripts.Audio;

namespace BlockPuzzleGameToolkit.Scripts.Popups
{
    /// <summary>
    /// 简化版PreWin弹窗
    /// 仅显示1.5秒胜利提示，无任何交互功能
    /// </summary>
    public class PreWin : Banner
    {
        protected override void Awake()
        {
            base.Awake(); // 调用基类的Awake

            // 设置此弹窗不使用背景遮罩
            fade = false;
        }

        protected virtual void OnEnable()
        {
            // 播放胜利音效（使用combo音效或其他合适的音效）
            // if (SoundBase.Instance != null && SoundBase.Instance.combo != null && SoundBase.Instance.combo.Length > 0)
            // {
            //     // 使用最高级的combo音效作为胜利音效
            //     SoundBase.Instance.PlaySound(SoundBase.Instance.combo[SoundBase.Instance.combo.Length - 1]);
            // }

            // 禁用所有交互
            StopInteration();
        }

        public override void AfterShowAnimation()
        {
            base.AfterShowAnimation();
            // 将由LevelStateHandler控制1.5秒后自动关闭
        }
    }
}