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
using BlockPuzzleGameToolkit.Scripts.Gameplay.FX;
using BlockPuzzleGameToolkit.Scripts.GUI;
using BlockPuzzleGameToolkit.Scripts.LevelsData;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using BlockPuzzleGameToolkit.Scripts.Enums;
using UnityEngine;
using UnityEngine.Pool;
using DG.Tweening;

namespace BlockPuzzleGameToolkit.Scripts.Gameplay
{
    /// <summary>
    /// TargetManager - 目标管理器
    /// 负责管理冒险模式中的关卡目标（如收集特定道具、达到分数等）
    /// 处理目标UI更新、完成检测、奖励道具收集动画
    /// </summary>
    public class TargetManager : MonoBehaviour
    {
        // ========== 关卡数据 ==========
        /// <summary>
        /// 当前关卡数据
        /// </summary>
        private Level level;

        /// <summary>
        /// 关卡目标实例列表
        /// 存储当前关卡的所有目标及其进度
        /// </summary>
        private List<Target> _levelTargetInstance;

        /// <summary>
        /// 目标UI元素字典
        /// 映射目标类型到对应的UI显示组件
        /// </summary>
        private Dictionary<TargetScriptable, TargetGUIElement> _targetGuiElements;

        // ========== UI组件 ==========
        /// <summary>
        /// 目标面板UI预制体
        /// </summary>
        public TargetsUIHandler targetPanel;

        /// <summary>
        /// 目标UI的父节点
        /// </summary>
        public Transform targetParent;

        // ========== 动画系统 ==========
        /// <summary>
        /// 活动动画与目标的映射
        /// 用于追踪哪个动画对应哪个目标
        /// </summary>
        private Dictionary<BonusAnimation, TargetScriptable> _activeAnimationTargets;

        /// <summary>
        /// BonusAnimationManager引用（统一管理所有bonus动画）
        /// </summary>
        private BonusAnimationManager _animationManager;

        // ========== Bonus收集系统 ==========
        /// <summary>
        /// 注册的Bonus收集者（MoneyBlockManager等）
        /// 注意：收集者会在运行时自动注册，此字段为可选配置（用于手动指定或覆盖）
        /// </summary>
        [Header("Bonus收集配置")]
        [SerializeField, Tooltip("【可选】手动指定Bonus收集者（如MoneyBlockManager）\n收集者会在运行时自动调用RegisterCollector()注册，此字段仅作为备用配置")]
        private List<MonoBehaviour> bonusCollectorObjects = new List<MonoBehaviour>();

        /// <summary>
        /// IBonusCollector接口列表
        /// </summary>
        private readonly List<IBonusCollector> _bonusCollectors = new List<IBonusCollector>();

        /// <summary>
        /// Unity生命周期 - 启用时初始化
        /// 创建数据结构
        /// </summary>
        private void OnEnable()
        {
            _activeAnimationTargets = new Dictionary<BonusAnimation, TargetScriptable>();

            // 获取BonusAnimationManager
            if (_animationManager == null)
            {
                _animationManager = BonusAnimationManager.Instance;
                if (_animationManager == null)
                {
                    Debug.LogWarning("[TargetManager] 未找到BonusAnimationManager，动画功能将不可用");
                }
            }

            // 初始化Bonus收集者
            InitializeBonusCollectors();
        }

        /// <summary>
        /// 初始化Bonus收集者列表（从Inspector配置）
        /// 注意：这是可选的fallback机制，收集者通常通过代码调用RegisterCollector()自动注册
        /// </summary>
        private void InitializeBonusCollectors()
        {
            _bonusCollectors.Clear();

            // 从Inspector配置初始化（可选）
            foreach (var obj in bonusCollectorObjects)
            {
                if (obj == null) continue;

                if (obj is IBonusCollector collector)
                {
                    _bonusCollectors.Add(collector);
                    Debug.Log($"[TargetManager] 从Inspector注册Bonus收集者: {obj.GetType().Name}");
                }
                else
                {
                    Debug.LogWarning($"[TargetManager] {obj.GetType().Name} 未实现IBonusCollector接口");
                }
            }

            // 注意：其他收集者会在运行时通过RegisterCollector()自动注册
            if (_bonusCollectors.Count == 0)
            {
                Debug.Log("[TargetManager] Inspector未配置Bonus收集者，等待运行时自动注册");
            }
        }

        /// <summary>
        /// 检查关卡是否完成
        /// 所有目标都达成时返回true
        /// </summary>
        /// <returns>关卡是否完成</returns>
        public bool IsLevelComplete()
        {
            if (_levelTargetInstance == null || _levelTargetInstance.Count == 0)
                return false;

            foreach (var t in _levelTargetInstance)
            {
                if (!t.OnCompleted())
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 预测关卡是否即将完成
        /// 考虑正在飞行的奖励道具动画，预判是否能完成所有目标
        /// </summary>
        /// <returns>关卡是否即将完成</returns>
        public bool WillLevelBeComplete()
        {
            if (_levelTargetInstance == null || _levelTargetInstance.Count == 0) return false;

            // 检查金钱方块累计弹窗状态，如果正在处理，延迟通关判定
            var moneyBlockManager = MoneyBlockSystem.MoneyBlockManager.Instance;
            if (moneyBlockManager != null && moneyBlockManager.IsProcessingCumulative)
            {
                Debug.Log("[TargetManager] 金钱方块累计弹窗正在显示，延迟通关判定");
                return false;  // 延迟通关，等待弹窗关闭
            }

            // 统计正在飞行中的奖励道具
            var pendingDeductions = new Dictionary<TargetScriptable, int>();
            foreach (var targetScriptable in _activeAnimationTargets.Values)
            {
                pendingDeductions[targetScriptable] = pendingDeductions.GetValueOrDefault(targetScriptable, 0) + 1;
            }

            // 检查每个目标的预测完成状态
            foreach (var target in _levelTargetInstance)
            {
                int currentAmount = target.amount;
                int deductions = pendingDeductions.GetValueOrDefault(target.targetScriptable, 0);
                int predictedAmount = currentAmount - deductions;

                if (predictedAmount > 0)
                {
                    return false;  // 还有目标未完成
                }
            }

            // 通知即将完成关卡
            EventManager.GetEvent(EGameEvent.LevelAboutToComplete).Invoke();
            return true;
        }

        /// <summary>
        /// 获取关卡总体进度百分比
        /// 计算所有目标的综合完成度
        /// </summary>
        /// <returns>进度百分比（0-100）</returns>
        public float GetOverallProgress()
        {
            if (_levelTargetInstance == null || _levelTargetInstance.Count == 0)
                return 0f;

            float totalProgress = 0f;
            int targetCount = 0;

            foreach (var target in _levelTargetInstance)
            {
                if (target.targetScriptable == null)
                    continue;

                targetCount++;
                float targetProgress = 0f;

                // 获取目标的总量（初始值）
                int totalAmount = target.totalAmount > 0 ? target.totalAmount : 100;

                if (target.targetScriptable.descending)
                {
                    // 倒计目标：从totalAmount开始，减少到0为完成
                    // 进度 = (总量 - 剩余量) / 总量
                    int completed = totalAmount - target.amount;
                    targetProgress = (float)completed / totalAmount * 100f;
                }
                else
                {
                    // 正计目标：从0开始，增加到totalAmount为完成
                    // 进度 = 当前量 / 总量
                    // 注意：正计目标的amount存储的是已完成的数量
                    targetProgress = (float)target.amount / totalAmount * 100f;
                }

                // 处理分数目标的特殊情况
                if (target.targetScriptable is ScoreTargetScriptable)
                {
                    // 对于分数目标，totalAmount是初始目标分数，amount是剩余需要的分数
                    // 已获得分数 = 总目标分数 - 剩余需要分数
                    int currentScore = totalAmount - target.amount;
                    targetProgress = (float)currentScore / totalAmount * 100f;
                }

                // 确保进度在0-100之间
                targetProgress = Mathf.Clamp(targetProgress, 0f, 100f);
                totalProgress += targetProgress;
            }

            // 计算平均进度
            if (targetCount > 0)
            {
                return totalProgress / targetCount;
            }

            return 0f;
        }

        /// <summary>
        /// 关卡加载时初始化目标系统
        /// 创建目标实例和UI
        /// </summary>
        /// <param name="obj">关卡数据</param>
        public void OnLevelLoaded(Level obj)
        {
            level = obj;
            if (level == null)
            {
                return;
            }

            // 克隆关卡目标实例（只保留数量大于0的目标）
            _levelTargetInstance = new List<Target>();
            foreach (var t in level.targetInstance)
            {
                if (t.amount > 0)
                {
                    var target = t.Clone();
                    target.totalAmount = t.amount;  // 记录初始总量
                    _levelTargetInstance.Add(target);
                }
            }
            _targetGuiElements = new Dictionary<TargetScriptable, TargetGUIElement>();
            _activeAnimationTargets?.Clear();

            // 清除旧的目标UI
            var findObjectOfType = FindObjectOfType<TargetsUIHandler>();
            if (findObjectOfType != null)
            {
                Destroy(findObjectOfType.gameObject);
            }
            // 非教学模式下创建新的目标UI
            if (!GameManager.Instance.IsTutorialMode())
            {
                var t = Instantiate(targetPanel, targetParent);
                t.OnLevelLoaded(level.levelType.elevelType);
            }
        }

        /// <summary>
        /// 注册目标GUI元素
        /// 将目标类型与其UI组件关联起来
        /// </summary>
        /// <param name="target">目标类型</param>
        /// <param name="targetGuiElement">目标UI元素</param>
        public void RegisterTargetGuiElement(TargetScriptable target, TargetGUIElement targetGuiElement)
        {
            _targetGuiElements[target] = targetGuiElement;
            var newCount = _levelTargetInstance.Find(t => t.targetScriptable == target).amount;
            // 倒计目标显示当前数量，正计目标显示0
            targetGuiElement.UpdateCount(target.descending ? newCount : 0, false);
        }

        /// <summary>
        /// 更新目标计数显示
        /// </summary>
        /// <param name="targetScriptable">要更新的目标</param>
        public void UpdateTargetCount(Target targetScriptable)
        {
            if (_targetGuiElements.TryGetValue(targetScriptable.targetScriptable, out var targetGuiElement))
            {
                targetGuiElement.UpdateCount(targetScriptable.amount, IsTargetCompleted(targetScriptable));
            }
        }

        /// <summary>
        /// 检查单个目标是否完成
        /// </summary>
        /// <param name="targetScriptable">目标实例</param>
        /// <returns>目标是否完成</returns>
        private bool IsTargetCompleted(Target targetScriptable)
        {
            return targetScriptable.amount <= 0;
        }

        /// <summary>
        /// 获取所有目标
        /// </summary>
        /// <returns>目标列表</returns>
        public List<Target> GetTargets()
        {
            return _levelTargetInstance;
        }

        /// <summary>
        /// 获取目标GUI元素字典
        /// </summary>
        /// <returns>目标与UI元素的映射</returns>
        public Dictionary<TargetScriptable, TargetGUIElement> GetTargetGuiElements()
        {
            return _targetGuiElements;
        }

        /// <summary>
        /// 播放目标道具收集动画
        /// 从消除的格子中收集奖励道具，飞向对应的目标UI
        /// </summary>
        /// <param name="lines">被消除的行列</param>
        /// <returns>协程</returns>
        public IEnumerator AnimateTarget(List<List<Cell>> lines)
        {
            // 确保BonusAnimationManager已初始化（防止OnEnable时还未初始化）
            if (_animationManager == null)
            {
                _animationManager = BonusAnimationManager.Instance;
            }

            if (_animationManager == null)
            {
                Debug.LogWarning("[TargetManager] BonusAnimationManager不可用，跳过动画");
                yield break;
            }

            // Phase 1: 处理目标宝石bonus
            yield return StartCoroutine(AnimateTargetBonus(lines));

            // Phase 2: 处理其他注册的bonus（金钱方块等）
            yield return StartCoroutine(AnimateOtherBonus(lines));
        }

        /// <summary>
        /// 处理目标相关的宝石bonus
        /// </summary>
        private IEnumerator AnimateTargetBonus(List<List<Cell>> lines)
        {
            // 收集所有宝石bonus位置
            var bonusItems = new Dictionary<BonusItemTemplate, List<Vector3>>();

            foreach (var cellList in lines)
            {
                foreach (var cell in cellList)
                {
                    if (cell == null) continue;

                    if (cell.HasBonusItem())
                    {
                        var bonusItem = cell.GetBonusItem();
                        if (!bonusItems.ContainsKey(bonusItem))
                        {
                            bonusItems[bonusItem] = new List<Vector3>();
                        }

                        bonusItems[bonusItem].Add(cell.transform.position);
                    }
                }
            }

            // 准备批量动画数据
            var animations = new List<BonusAnimationData>();

            foreach (var target in _levelTargetInstance)
            {
                if (target.targetScriptable.bonusItem == null) continue;

                if (bonusItems.TryGetValue(target.targetScriptable.bonusItem, out var positions))
                {
                    if (!_targetGuiElements.TryGetValue(target.targetScriptable, out var targetUI))
                        continue;

                    var targetPos = targetUI.transform.position;

                    foreach (var pos in positions)
                    {
                        var capturedTarget = target;  // 闭包捕获
                        var animData = new BonusAnimationData
                        {
                            startPos = pos,
                            targetPos = targetPos,
                            bonusTemplate = target.targetScriptable.bonusItem,
                            onComplete = () =>
                            {
                                // 更新目标计数（只在还需要收集时减少）
                                if (capturedTarget.amount > 0)
                                {
                                    capturedTarget.amount--;

                                    // 播放UI反馈动画（放大缩小效果）
                                    var targetTransform = _targetGuiElements[capturedTarget.targetScriptable].transform;
                                    targetTransform.DOScale(Vector3.one * 1.2f, 0.1f)
                                        .SetEase(Ease.OutQuad)
                                        .OnComplete(() => {
                                            targetTransform.DOScale(Vector3.one, 0.1f)
                                                .SetEase(Ease.InQuad);
                                        });

                                    UpdateTargetCount(capturedTarget);
                                }
                            }
                        };
                        animations.Add(animData);
                    }
                }
            }

            // 播放批量动画
            if (animations.Count > 0)
            {
                yield return StartCoroutine(_animationManager.PlayBatchAnimations(animations));
            }
        }

        /// <summary>
        /// 处理其他bonus收集者（金钱方块等）
        /// </summary>
        private IEnumerator AnimateOtherBonus(List<List<Cell>> lines)
        {
            var animations = new List<BonusAnimationData>();

            foreach (var collector in _bonusCollectors)
            {
                if (collector == null || !collector.IsEnabled()) continue;

                var targetPos = collector.GetFlyTargetPosition();
                var template = collector.GetBonusTemplate();

                if (template == null) continue;

                foreach (var cellList in lines)
                {
                    foreach (var cell in cellList)
                    {
                        if (cell == null) continue;

                        if (collector.HasBonus(cell))
                        {
                            var capturedCollector = collector;  // 闭包捕获
                            var capturedCell = cell;
                            var animData = new BonusAnimationData
                            {
                                startPos = cell.transform.position,
                                targetPos = targetPos,
                                bonusTemplate = template,
                                onComplete = () => capturedCollector.OnBonusCollected(capturedCell)
                            };
                            animations.Add(animData);
                        }
                    }
                }
            }

            // 播放批量动画
            if (animations.Count > 0)
            {
                yield return StartCoroutine(_animationManager.PlayBatchAnimations(animations));
            }
        }

        /// <summary>
        /// 更新分数目标
        /// 用于处理分数类型的关卡目标
        /// </summary>
        /// <param name="score">获得的分数</param>
        public void UpdateScoreTarget(int score)
        {
            // 查找分数目标
            var targetScriptable = _levelTargetInstance.Find(t => t.targetScriptable.GetType() == typeof(ScoreTargetScriptable));
            if (targetScriptable != null && _targetGuiElements.TryGetValue(targetScriptable.targetScriptable, out var targetGuiElement))
            {
                var target = _levelTargetInstance.Find(t => t.targetScriptable == targetScriptable.targetScriptable);
                target.amount -= score;  // 减少所需分数
                targetGuiElement.UpdateCount(score, IsTargetCompleted(targetScriptable));
            }
        }

        /// <summary>
        /// 检查是否有动画正在播放
        /// 用于防止过早判断关卡完成
        /// </summary>
        /// <returns>是否有动画播放中</returns>
        public bool IsAnimationPlaying()
        {
            // 由于动画现在由BonusAnimationManager管理，这里简化检查
            // 可以通过延迟或其他方式检测
            return false;  // TODO: 如果需要，可以让BonusAnimationManager提供IsPlaying接口
        }

        /// <summary>
        /// 运行时动态注册Bonus收集者
        /// </summary>
        /// <param name="collector">Bonus收集者</param>
        public void RegisterCollector(IBonusCollector collector)
        {
            if (collector != null && !_bonusCollectors.Contains(collector))
            {
                _bonusCollectors.Add(collector);
                Debug.Log($"[TargetManager] 动态注册Bonus收集者: {collector.GetType().Name}");
            }
        }

        /// <summary>
        /// 取消注册Bonus收集者
        /// </summary>
        /// <param name="collector">Bonus收集者</param>
        public void UnregisterCollector(IBonusCollector collector)
        {
            if (collector != null)
            {
                _bonusCollectors.Remove(collector);
                Debug.Log($"[TargetManager] 取消注册Bonus收集者: {collector.GetType().Name}");
            }
        }
    }
}