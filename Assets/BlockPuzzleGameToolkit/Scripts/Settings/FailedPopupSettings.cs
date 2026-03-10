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

using BlockPuzzleGameToolkit.Scripts.GameCore;
using BlockPuzzleGameToolkit.Scripts.Services.Ads.AdUnits;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Settings
{
    /// <summary>
    /// 关卡失败弹窗配置设置
    /// 用于配置复活功能、广告类型等参数
    /// </summary>
    [CreateAssetMenu(fileName = "FailedPopupSettings", menuName = "BlockPuzzleGameToolkit/Settings/FailedPopupSettings")]
    public class FailedPopupSettings : SingletonScriptableSettings<FailedPopupSettings>
    {
        [Header("复活功能设置")]
        [Tooltip("是否允许免费复活功能")]
        public bool allowFreeRevive = true;

        [Tooltip("每关最大复活次数（0表示无限制）")]
        public int maxRevivesPerLevel = 0;  // 默认无限制，符合免费复活设计

        [Tooltip("复活时刷新的方块数量")]
        [Range(1, 5)]
        public int refreshShapeCount = 3;

        [Header("广告设置")]
        [Tooltip("复活功能使用的广告类型")]
        public EAdType adType = EAdType.Rewarded;

        [Tooltip("广告加载失败时是否仍然允许复活")]
        public bool allowReviveOnAdFail = true;

        [Header("复活后的方块生成")]
        [Tooltip("是否保证至少一个方块可以放置")]
        public bool guaranteePlaceableShape = true;

        [Tooltip("优先生成小型方块的概率 (0-1)")]
        [Range(0f, 1f)]
        public float smallShapePriority = 0.7f;

        [Header("进度显示设置")]
        [Tooltip("是否显示进度条")]
        public bool showProgressBar = true;

        [Tooltip("进度条动画持续时间")]
        [Range(0.1f, 2f)]
        public float progressAnimationDuration = 0.5f;

        [Header("调试选项")]
        [Tooltip("调试模式下无需观看广告即可复活")]
        public bool debugFreeRevive = false;

        [Header("Replay广告设置")]
        [Tooltip("是否启用Replay广告功能")]
        public bool enableReplayAd = true;

        [Tooltip("触发广告的Replay次数阈值")]
        [Range(2, 10)]
        public int replayCountThreshold = 3;

        [Tooltip("Replay广告入口名称")]
        public string replayAdEntryName = "level_failed_replay";

        [Tooltip("广告播放失败后是否重置计数器")]
        public bool resetCounterOnAdFail = true;

        [Header("Replay广告调试")]
        [Tooltip("调试模式:打印详细日志")]
        public bool debugReplayAd = false;

        /// <summary>
        /// 获取实际使用的广告类型
        /// </summary>
        /// <returns>广告类型</returns>
        public EAdType GetEffectiveAdType()
        {
            // 调试模式下不需要广告
            if (debugFreeRevive && Application.isEditor)
                return EAdType.Banner; // 返回一个不会真正显示的类型

            return adType;
        }

        /// <summary>
        /// 检查是否可以复活
        /// </summary>
        /// <param name="currentReviveCount">当前已复活次数</param>
        /// <returns>是否可以复活</returns>
        public bool CanRevive(int currentReviveCount)
        {
            // 如果不允许复活，直接返回false
            if (!allowFreeRevive)
                return false;

            // 如果无限制（maxRevivesPerLevel == 0），返回true
            if (maxRevivesPerLevel == 0)
                return true;

            // 检查是否超过最大次数
            return currentReviveCount < maxRevivesPerLevel;
        }
    }
}