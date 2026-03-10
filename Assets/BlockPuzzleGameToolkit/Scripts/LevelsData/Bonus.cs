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
using UnityEngine.UI;

namespace BlockPuzzleGameToolkit.Scripts.LevelsData
{
    public class Bonus : FillAndPreview
    {
        public Image image;

        [HideInInspector]
        public BonusItemTemplate bonusItemTemplate;

        [SerializeField]
        private int _side = 62;

        public override void FillIcon(ScriptableData iconScriptable)
        {
            if (iconScriptable == null)
            {
                Debug.LogWarning("[Bonus] FillIcon: iconScriptable is null");
                return;
            }

            UpdateColor(iconScriptable as BonusItemTemplate);
        }

        private void UpdateColor(BonusItemTemplate bonusItemTemplate)
        {
            if (bonusItemTemplate == null)
            {
                Debug.LogWarning("[Bonus] UpdateColor: bonusItemTemplate is null");
                return;
            }

            if (image == null)
            {
                Debug.LogError("[Bonus] UpdateColor: image component is missing");
                return;
            }

            this.bonusItemTemplate = bonusItemTemplate;

            if (bonusItemTemplate.sprite != null)
            {
                image.sprite = bonusItemTemplate.sprite;
                image.SetNativeSize();

                if (image.rectTransform.sizeDelta.x > _side || image.rectTransform.sizeDelta.y > _side)
                {
                    if (image.rectTransform.sizeDelta.x > image.rectTransform.sizeDelta.y)
                    {
                        image.rectTransform.sizeDelta = new Vector2(_side, _side * image.rectTransform.sizeDelta.y / image.rectTransform.sizeDelta.x);
                    }
                    else
                    {
                        image.rectTransform.sizeDelta = new Vector2(_side * image.rectTransform.sizeDelta.x / image.rectTransform.sizeDelta.y, _side);
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[Bonus] UpdateColor: sprite is null for {bonusItemTemplate.name}");
            }
        }

        public void SetTransparency(float alpha)
        {
            var color = image.color;
            color.a = alpha;
            image.color = color;
        }
    }
}