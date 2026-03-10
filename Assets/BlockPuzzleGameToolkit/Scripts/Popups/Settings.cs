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
using BlockPuzzleGameToolkit.Scripts.Enums;
using BlockPuzzleGameToolkit.Scripts.Gameplay;
using BlockPuzzleGameToolkit.Scripts.GUI;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using BlockPuzzleGameToolkit.Scripts.Audio;
using BlockPuzzle.NativeBridge;
using BlockPuzzle.NativeBridge.Enums;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

namespace BlockPuzzleGameToolkit.Scripts.Popups
{
    public class Settings : PopupWithCurrencyLabel
    {
        // privacypolicy button
        [SerializeField]
        private CustomButton privacypolicy;

        // terms of use button
        [SerializeField]
        private CustomButton termsOfUse;

        [SerializeField]
        private CustomButton retryButton;

        [SerializeField]
        private Toggle vibrationToggle;

        [SerializeField]
        private Toggle musicToggle;

        [SerializeField]
        private Toggle soundToggle;

        [SerializeField]
        private AudioMixer mixer;

        private const string VibrationPrefKey = "VibrationEnabled";
        private const string MusicPrefKey = "Music";
        private const string SoundPrefKey = "Sound";
        private const string musicParameter = "musicVolume";
        private const string soundParameter = "soundVolume";

        private void OnEnable()
        {
            var fieldManager = FindObjectOfType<FieldManager>();
            // Save current game state when settings is opened
            if (StateManager.Instance.CurrentState == EScreenStates.Game)
            {
                var currentMode = GameDataManager.GetGameMode();
                GameState currentState = null;

                // Create appropriate state based on game mode
                if (currentMode == EGameMode.Classic)
                {
                    var classicHandler = FindObjectOfType<ClassicModeHandler>();
                    if (classicHandler != null)
                    {
                        currentState = new ClassicGameState
                        {
                            score = classicHandler.score,
                            bestScore = classicHandler.bestScore,
                            gameMode = EGameMode.Classic,
                            gameStatus = EventManager.GameStatus
                        };
                    }
                }
                else if (currentMode == EGameMode.Timed)
                {
                    var timedHandler = FindObjectOfType<TimedModeHandler>();
                    if (timedHandler != null)
                    {
                        currentState = new TimedGameState
                        {
                            score = timedHandler.score,
                            bestScore = timedHandler.bestScore,
                            remainingTime = timedHandler.GetRemainingTime(),
                            gameMode = EGameMode.Timed,
                            gameStatus = EventManager.GameStatus
                        };
                    }
                }

                if (currentState != null && fieldManager != null)
                {
                    GameState.Save(currentState, fieldManager);
                }
            }

            // Setup button listeners
            privacypolicy.onClick.AddListener(PrivacyPolicy);
            if (termsOfUse != null)
            {
                termsOfUse.onClick.AddListener(TermsOfUse);
            }
            retryButton.onClick.AddListener(Retry);

            // Setup toggles
            SetupVibrationToggle();
            SetupMusicToggle();
            SetupSoundToggle();

            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(BackToGame);
        }

        private void BackToGame()
        {
            DisablePause();
            Close();
        }

        private void OnDisable()
        {
            // Unregister the event listeners
            vibrationToggle.onValueChanged.RemoveListener(ToggleVibration);
            musicToggle.onValueChanged.RemoveListener(ToggleMusic);
            soundToggle.onValueChanged.RemoveListener(ToggleSound);

            // Remove button listeners
            privacypolicy.onClick.RemoveListener(PrivacyPolicy);
            if (termsOfUse != null)
            {
                termsOfUse.onClick.RemoveListener(TermsOfUse);
            }
            retryButton.onClick.RemoveListener(Retry);
        }

        #region Vibration Settings
        private void SetupVibrationToggle()
        {
            // Load the saved vibration state
            LoadVibrationState();

            // Control checkmark node visibility
            if (vibrationToggle.graphic != null)
            {
                vibrationToggle.graphic.gameObject.SetActive(vibrationToggle.isOn);
            }

            // Register the OnValueChanged event
            vibrationToggle.onValueChanged.AddListener(ToggleVibration);
        }

        private void ToggleVibration(bool isOn)
        {
            PlayerPrefs.SetInt(VibrationPrefKey, isOn ? 1 : 0);
            PlayerPrefs.Save();

            // Control checkmark node visibility
            if (vibrationToggle.graphic != null)
            {
                vibrationToggle.graphic.gameObject.SetActive(isOn);
            }
        }

        private void LoadVibrationState()
        {
            if (PlayerPrefs.HasKey(VibrationPrefKey))
            {
                vibrationToggle.isOn = PlayerPrefs.GetInt(VibrationPrefKey, 1) == 1;
            }
            else
            {
                vibrationToggle.isOn = true;
                ToggleVibration(true);
            }

            // Ensure checkmark node visibility matches the state
            if (vibrationToggle.graphic != null)
            {
                vibrationToggle.graphic.gameObject.SetActive(vibrationToggle.isOn);
            }
        }
        #endregion

        #region Music Settings
        private void SetupMusicToggle()
        {
            // Load the saved music state
            var musicEnabled = PlayerPrefs.GetInt(MusicPrefKey, 1) != 0;
            musicToggle.isOn = musicEnabled;

            // Control checkmark node visibility
            if (musicToggle.graphic != null)
            {
                musicToggle.graphic.gameObject.SetActive(musicEnabled);
            }

            // Update mixer
            if (mixer != null)
            {
                mixer.SetFloat(musicParameter, musicEnabled ? 0 : -80);
            }

            // Register the OnValueChanged event
            musicToggle.onValueChanged.AddListener(ToggleMusic);
        }

        private void ToggleMusic(bool isOn)
        {
            // Play click sound
            if (SoundBase.Instance != null)
            {
                SoundBase.Instance.PlaySound(SoundBase.Instance.click);
            }

            // Save the setting
            PlayerPrefs.SetInt(MusicPrefKey, isOn ? 1 : 0);
            PlayerPrefs.Save();

            // Control checkmark node visibility
            if (musicToggle.graphic != null)
            {
                musicToggle.graphic.gameObject.SetActive(isOn);
            }

            // Update mixer
            if (mixer != null)
            {
                mixer.SetFloat(musicParameter, isOn ? 0 : -80);
            }
        }
        #endregion

        #region Sound Settings
        private void SetupSoundToggle()
        {
            // Load the saved sound state
            var soundEnabled = PlayerPrefs.GetInt(SoundPrefKey, 1) != 0;
            soundToggle.isOn = soundEnabled;

            // Control checkmark node visibility
            if (soundToggle.graphic != null)
            {
                soundToggle.graphic.gameObject.SetActive(soundEnabled);
            }

            // Update mixer
            if (mixer != null)
            {
                mixer.SetFloat(soundParameter, soundEnabled ? 0 : -80);
            }

            // Register the OnValueChanged event
            soundToggle.onValueChanged.AddListener(ToggleSound);
        }

        private void ToggleSound(bool isOn)
        {
            // Play click sound before changing the setting
            if (SoundBase.Instance != null && isOn)
            {
                SoundBase.Instance.PlaySound(SoundBase.Instance.click);
            }

            // Save the setting
            PlayerPrefs.SetInt(SoundPrefKey, isOn ? 1 : 0);
            PlayerPrefs.Save();

            // Control checkmark node visibility
            if (soundToggle.graphic != null)
            {
                soundToggle.graphic.gameObject.SetActive(isOn);
            }

            // Update mixer
            if (mixer != null)
            {
                mixer.SetFloat(soundParameter, isOn ? 0 : -80);
            }
        }
        #endregion

        private void Retry()
        {
            GameManager.Instance.RestartLevel();
            MenuManager.Instance.FadeOut();
        }

        private void PrivacyPolicy()
        {
            // 调用NativeBridgeManager的隐私政策接口
            var nativeBridgeManager = NativeBridgeManager.Instance;
            if (nativeBridgeManager != null)
            {
                nativeBridgeManager.SendMessageToPlatform(BridgeMessageType.PrivacyPolicy);
            }
            else
            {
                Debug.LogWarning("NativeBridgeManager not found");
            }
        }

        private void TermsOfUse()
        {
            // 调用NativeBridgeManager的使用条款接口
            var nativeBridgeManager = NativeBridgeManager.Instance;
            if (nativeBridgeManager != null)
            {
                nativeBridgeManager.SendMessageToPlatform(BridgeMessageType.TermsOfUse);
            }
            else
            {
                Debug.LogWarning("NativeBridgeManager not found");
            }
        }

        private void DisablePause()
        {
            if (StateManager.Instance.CurrentState == EScreenStates.Game)
            {
                EventManager.GameStatus = EGameState.Playing;
            }
        }
    }
}