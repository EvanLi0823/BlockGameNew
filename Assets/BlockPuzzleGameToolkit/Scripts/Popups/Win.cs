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

using BlockPuzzleGameToolkit.Scripts.GUI;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using BlockPuzzleGameToolkit.Scripts.Gameplay;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Popups
{
    public class Win : Popup
    {
        public CustomButton nextLevelButton;

        protected override void Awake()
        {
            base.Awake();
            nextLevelButton.onClick.AddListener(() =>
            {
                StopInteration();

                if (GameDataManager.HasMoreLevels())
                {
                    GameManager.Instance.NextLevel();
                }
                else
                {
                    GameManager.Instance.MainMenu();
                }
                Close();
            });
            closeButton.onClick.AddListener(() => GameManager.Instance.OpenMap());
        }

        // 注意：奖励弹窗现在直接替代Win弹窗，所以这里不再需要显示奖励弹窗的逻辑
        // 如果需要作为后备方案使用，此弹窗仅显示基本的胜利信息
    }
}