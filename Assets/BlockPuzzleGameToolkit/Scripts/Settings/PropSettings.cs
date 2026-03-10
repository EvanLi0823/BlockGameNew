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

using System.Collections.Generic;
using System.Linq;
using BlockPuzzleGameToolkit.Scripts.PropSystem.Core;
using BlockPuzzleGameToolkit.Scripts.GameCore;
using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.Settings
{
    /// <summary>
    /// 道具系统设置 - 管理道具的初始化配置
    /// </summary>
    [CreateAssetMenu(fileName = "PropSettings", menuName = "BlockPuzzle/Props/Settings", order = 99)]
    public class PropSettings : SingletonScriptableSettings<PropSettings>
    {
        [Header("道具系统总开关")]
        [Tooltip("是否启用道具系统功能")]
        public bool enablePropSystem = true;

        [Header("道具配置列表")]
        [Tooltip("所有道具的配置列表")]
        public List<PropItemConfig> propConfigs = new List<PropItemConfig>();

        [Header("玩家初始道具配置")]
        [Tooltip("配置玩家初始化时拥有的道具类型和数量")]
        public List<PropData> initialProps = new List<PropData>
        {
            new PropData(PropType.Rotate, 3),
            new PropData(PropType.Refresh, 3),
            new PropData(PropType.Bomb, 1)
        };

        [Header("特效配置")]
        [Tooltip("特效持续时间（秒）")]
        [Range(0.1f, 3f)]
        public float effectDuration = 1f;

        [Tooltip("特效缩放系数")]
        [Range(0.5f, 2f)]
        public float effectScale = 1f;

        [Header("动画配置")]
        [Tooltip("道具使用动画时长（秒）")]
        [Range(0.1f, 1f)]
        public float useAnimationDuration = 0.3f;

        [Tooltip("道具选择时的缩放系数")]
        [Range(1f, 1.5f)]
        public float selectionScale = 1.1f;

        /// <summary>
        /// 根据道具类型获取配置
        /// </summary>
        /// <param name="type">道具类型</param>
        /// <returns>道具配置，如果不存在返回null</returns>
        public PropItemConfig GetConfig(PropType type)
        {
            return propConfigs.FirstOrDefault(c => c != null && c.propType == type);
        }

        /// <summary>
        /// 检查是否有指定类型的配置
        /// </summary>
        /// <param name="type">道具类型</param>
        /// <returns>是否存在配置</returns>
        public bool HasConfig(PropType type)
        {
            return GetConfig(type) != null;
        }

        /// <summary>
        /// 获取初始道具数量
        /// </summary>
        /// <param name="type">道具类型</param>
        /// <returns>初始数量</returns>
        public int GetInitialCount(PropType type)
        {
            var initialProp = initialProps.FirstOrDefault(p => p.propType == type);
            return initialProp?.propNum ?? 0;
        }

        /// <summary>
        /// 验证配置是否完整
        /// </summary>
        /// <returns>配置是否有效</returns>
        public bool ValidateConfiguration()
        {
            bool isValid = true;

            // 检查道具配置列表
            if (propConfigs == null || propConfigs.Count == 0)
            {
                Debug.LogError("PropSettings: 道具配置列表为空");
                isValid = false;
            }
            else
            {
                // 检查每个配置的有效性
                foreach (var config in propConfigs)
                {
                    if (config == null)
                    {
                        Debug.LogError("PropSettings: 道具配置列表中存在null配置");
                        isValid = false;
                    }
                    else if (!config.IsValid())
                    {
                        isValid = false;
                    }
                }

                // 检查是否有重复的道具类型
                var duplicateTypes = propConfigs
                    .Where(c => c != null)
                    .GroupBy(c => c.propType)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key);

                foreach (var type in duplicateTypes)
                {
                    Debug.LogError($"PropSettings: 道具类型 {type} 有重复的配置");
                    isValid = false;
                }
            }

            // 检查初始道具配置
            if (initialProps != null && initialProps.Count > 0)
            {
                foreach (var prop in initialProps)
                {
                    if (prop.propType == PropType.None)
                    {
                        Debug.LogWarning("PropSettings: 初始道具中包含None类型");
                    }
                    else if (!HasConfig(prop.propType))
                    {
                        Debug.LogWarning($"PropSettings: 初始道具类型 {prop.propType} 没有对应的配置");
                    }
                }
            }

            return isValid;
        }

        /// <summary>
        /// 在Inspector中值改变时调用
        /// </summary>
        private void OnValidate()
        {
            // 确保特效持续时间在合理范围内
            effectDuration = Mathf.Clamp(effectDuration, 0.1f, 3f);

            // 确保动画时长在合理范围内
            useAnimationDuration = Mathf.Clamp(useAnimationDuration, 0.1f, 1f);

            // 确保缩放系数在合理范围内
            effectScale = Mathf.Clamp(effectScale, 0.5f, 2f);
            selectionScale = Mathf.Clamp(selectionScale, 1f, 1.5f);
        }
    }
}