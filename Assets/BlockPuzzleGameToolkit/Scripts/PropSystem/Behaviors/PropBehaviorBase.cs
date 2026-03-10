// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using UnityEngine;
using BlockPuzzleGameToolkit.Scripts.PropSystem.Core;
using BlockPuzzleGameToolkit.Scripts.Settings;
using BlockPuzzleGameToolkit.Scripts.Audio;
using BlockPuzzleGameToolkit.Scripts.Gameplay;

namespace BlockPuzzleGameToolkit.Scripts.PropSystem.Behaviors
{
    /// <summary>
    /// 道具行为基类 - 提供道具行为的通用功能
    /// </summary>
    public abstract class PropBehaviorBase : IPropBehavior
    {
        #region 抽象属性

        /// <summary>
        /// 道具类型
        /// </summary>
        public abstract PropType PropType { get; }

        /// <summary>
        /// 是否需要选择目标
        /// </summary>
        public abstract bool RequiresTarget { get; }

        #endregion

        #region 保护字段

        /// <summary>
        /// 道具管理器引用
        /// </summary>
        protected PropManager propManager;

        /// <summary>
        /// 道具配置
        /// </summary>
        protected PropItemConfig config;

        /// <summary>
        /// 道具设置
        /// </summary>
        protected PropSettings settings;

        /// <summary>
        /// 是否正在选择
        /// </summary>
        protected bool isSelecting;

        /// <summary>
        /// 当前预览目标
        /// </summary>
        protected object currentPreviewTarget;

        /// <summary>
        /// 高亮管理器
        /// </summary>
        protected HighlightManager highlightManager;

        /// <summary>
        /// 格子管理器
        /// </summary>
        protected FieldManager fieldManager;

        /// <summary>
        /// 方块组管理器
        /// </summary>
        protected CellDeckManager cellDeckManager;

        #endregion

        #region 公共属性

        /// <summary>
        /// 是否正在选择目标
        /// </summary>
        public bool IsSelecting => isSelecting;

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化行为
        /// </summary>
        /// <param name="manager">道具管理器引用</param>
        public virtual void Initialize(PropManager manager)
        {
            // 保存管理器引用
            propManager = manager;

            // 获取配置
            settings = PropSettings.Instance;
            if (settings != null)
            {
                config = settings.GetConfig(PropType);
            }

            // 从PropManager获取管理器引用
            if (propManager != null)
            {
                highlightManager = propManager.HighlightManager;
                fieldManager = propManager.FieldManager;
                cellDeckManager = propManager.CellDeckManager;
            }

            if (config == null)
            {
                Debug.LogError($"PropBehaviorBase: 未找到 {PropType} 的配置");
            }
        }

        #endregion

        #region 选择管理

        /// <summary>
        /// 开始选择目标
        /// </summary>
        public virtual void StartSelection()
        {
            if (isSelecting) return;

            isSelecting = true;

            // 子类实现具体的选择逻辑
            OnStartSelection();

            Debug.Log($"PropBehaviorBase: 开始选择 {PropType} 道具目标");
        }

        /// <summary>
        /// 取消选择
        /// </summary>
        public virtual void CancelSelection()
        {
            if (!isSelecting) return;

            // 隐藏预览
            HidePreview();

            // 子类清理
            OnCancelSelection();

            isSelecting = false;
            currentPreviewTarget = null;

            Debug.Log($"PropBehaviorBase: 取消选择 {PropType} 道具");
        }

        /// <summary>
        /// 子类重写 - 开始选择时调用
        /// </summary>
        protected abstract void OnStartSelection();

        /// <summary>
        /// 子类重写 - 取消选择时调用
        /// </summary>
        protected abstract void OnCancelSelection();

        #endregion

        #region 执行

        /// <summary>
        /// 检查是否可以执行
        /// </summary>
        public virtual bool CanExecute(object target = null)
        {
            // 如果需要目标但未提供，返回false
            if (RequiresTarget && target == null)
            {
                return false;
            }

            // 如果不需要目标但提供了，忽略目标
            if (!RequiresTarget && target != null)
            {
                Debug.LogWarning($"PropBehaviorBase: {PropType} 不需要目标，忽略提供的目标");
            }

            return true;
        }

        /// <summary>
        /// 执行道具效果（子类必须实现）
        /// </summary>
        public abstract void Execute(object target = null);

        #endregion

        #region 预览

        /// <summary>
        /// 显示预览效果
        /// </summary>
        public virtual void ShowPreview(object target)
        {
            if (!RequiresTarget || target == null) return;

            // 如果目标改变，先隐藏之前的预览
            if (currentPreviewTarget != null && currentPreviewTarget != target)
            {
                HidePreview();
            }

            currentPreviewTarget = target;

            // 子类实现具体的预览逻辑
            OnShowPreview(target);
        }

        /// <summary>
        /// 隐藏预览效果
        /// </summary>
        public virtual void HidePreview()
        {
            if (currentPreviewTarget == null) return;

            // 子类实现具体的隐藏逻辑
            OnHidePreview();

            currentPreviewTarget = null;
        }

        /// <summary>
        /// 子类重写 - 显示预览时调用
        /// </summary>
        protected virtual void OnShowPreview(object target)
        {
            // 子类实现具体预览逻辑
        }

        /// <summary>
        /// 子类重写 - 隐藏预览时调用
        /// </summary>
        protected virtual void OnHidePreview()
        {
            // 子类实现具体隐藏逻辑
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 播放道具使用音效
        /// </summary>
        protected virtual void PlayUseSound()
        {
            if (config != null && config.useSound != null)
            {
                SoundBase.Instance?.PlaySound(config.useSound);
            }
            else
            {
                // 使用默认click音效
                SoundBase.Instance?.PlaySound(SoundBase.Instance.click);
            }
        }

        /// <summary>
        /// 播放道具特效
        /// </summary>
        /// <param name="position">特效位置</param>
        protected virtual void PlayEffect(Vector3 position)
        {
            if (config != null && config.effectPrefab != null)
            {
                var effect = Object.Instantiate(config.effectPrefab, position, Quaternion.identity);

                // 如果有设置，应用缩放
                if (settings != null)
                {
                    effect.transform.localScale = Vector3.one * settings.effectScale;
                    Object.Destroy(effect, settings.effectDuration);
                }
                else
                {
                    Object.Destroy(effect, 2f); // 默认2秒后销毁
                }
            }
        }

        #endregion

        #region 清理

        /// <summary>
        /// 清理资源
        /// </summary>
        public virtual void Cleanup()
        {
            CancelSelection();
            config = null;
            settings = null;
            highlightManager = null;
        }

        #endregion
    }
}
