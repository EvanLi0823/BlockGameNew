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
    public class PreFailed : PopupWithCurrencyLabel
    {
        protected override void Awake()
        {
            base.Awake(); // 调用基类的Awake

            // 设置此弹窗不使用背景遮罩
            fade = false;
        }

        protected virtual void OnEnable()
        {
            // 播放警告音效（可选）
            // if (SoundBase.Instance != null && SoundBase.Instance.warningTime != null)
            // {
            //     SoundBase.Instance.PlaySound(SoundBase.Instance.warningTime);
            // }

            // 禁用所有交互
            StopInteration();
        }

        /// <summary>
        /// 重写AfterShowAnimation，不做任何额外处理
        /// </summary>
        public override void AfterShowAnimation()
        {
            base.AfterShowAnimation();
            // 弹窗显示后不需要做任何事情
            // 将由LevelStateHandler控制关闭时机
        }
    }
}