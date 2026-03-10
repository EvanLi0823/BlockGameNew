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

namespace BlockPuzzleGameToolkit.Scripts.Enums
{
    public enum EGameEvent
    {
        LevelLoaded,
        Win,
        Play,
        ItemDestroyed,
        Uncover,
        Fall,
        ToDestroy,
        WaitForTutorial,
        ShapePlaced,
        LineDestroyed,
        RestartLevel,
        TutorialCompleted,
        LevelAboutToComplete,
        TimerExpired,
        LevelStarted,
        LevelCompleted,      // 关卡通关（用于难度系统）
        LevelFailed,         // 关卡失败（Retry/Revive触发，用于难度系统）
        PlayerRevived,

        // 道具系统事件
        OnPropUsed,          // 道具使用
        OnPropPurchased,     // 道具购买
        OnPropCountChanged,  // 道具数量变化
        OnPropSelectionStart, // 道具选择开始
        OnPropSelectionEnd,  // 道具选择结束

        // 游戏状态事件
        GameStateChanged,     // 游戏状态变化
        HasWithDraw,         // 用户提现过
        CurrencyChanged,     // 货币数量变化

        // 飞行奖励系统事件
        PlayFlyReward,       // 播放飞行奖励动画
        FlyRewardStarted,    // 飞行奖励开始
        FlyRewardCompleted,  // 飞行奖励完成

        // 场景加载事件
        GameSceneReady,      // 游戏场景加载完成（Loading结束，进入GameCanvas）
    }
}