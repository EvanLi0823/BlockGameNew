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

using System.Collections;
using System.Collections.Generic;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using UnityEngine;
using UnityEngine.Audio;

namespace BlockPuzzleGameToolkit.Scripts.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundBase : SingletonBehaviour<SoundBase>
    {
        [SerializeField]
        private AudioMixer mixer;

        [SerializeField]
        private string soundParameter = "soundVolume";

        public AudioClip click;
        public AudioClip[] swish;
        public AudioClip coins;
        public AudioClip coinsSpend;
        public AudioClip luckySpin;
        public AudioClip warningTime;
        public AudioClip placeShape;
        public AudioClip fillEmpty;
        public AudioClip alert;
        public AudioClip[] combo;
        public AudioClip rewardClaimed;
        public AudioClip fail;  // 失败音效
        public AudioClip win;   // 胜利音效

        // 鼓励词音效
        public AudioClip goodSound;      // "good" 音效
        public AudioClip greatSound;     // "great" 音效
        public AudioClip excellentSound; // "excellent" 音效（用于 fantastic 或其他）

        private AudioSource audioSource;

        // 缓存 WaitForSeconds 以优化性能
        private readonly WaitForSeconds waitForSoundComplete = new WaitForSeconds(0.1f);

        private readonly HashSet<AudioClip> clipsPlaying = new();

        public override void Awake()
        {
            base.Awake();
            audioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            mixer.SetFloat(soundParameter, PlayerPrefs.GetInt("Sound", 1) == 0 ? -80 : 0);
        }

        public void PlaySound(AudioClip clip)
        {
            if (clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        public void PlayDelayed(AudioClip clip, float delay)
        {
            StartCoroutine(PlayDelayedCoroutine(clip, delay));
        }

        private IEnumerator PlayDelayedCoroutine(AudioClip clip, float delay)
        {
            yield return new WaitForSeconds(delay);
            PlaySound(clip);
        }

        public void PlaySoundsRandom(AudioClip[] clip)
        {
            Instance.PlaySound(clip[Random.Range(0, clip.Length)]);
        }

        public void PlayLimitSound(AudioClip clip)
        {
            if (clipsPlaying.Add(clip))
            {
                PlaySound(clip);
                StartCoroutine(WaitForCompleteSound(clip));
            }
        }

        private IEnumerator WaitForCompleteSound(AudioClip clip)
        {
            yield return waitForSoundComplete;
            clipsPlaying.Remove(clip);
        }

        //停止播放Clip

        public void StopPlayingClip(AudioClip clip)
        {
            clipsPlaying.Remove(clip);
        }

        private void OnDestroy()
        {
            clipsPlaying.Clear();
        }
    }
}