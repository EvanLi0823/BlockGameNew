// ©2015 - 2025 Candy Smith
// All rights reserved
// Redistribution of this software is strictly not allowed.
// Copy of this software can be obtained from unity asset store only.

using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using BlockPuzzleGameToolkit.Scripts.Settings;

namespace BlockPuzzleGameToolkit.Scripts.Multiplier.UI
{
    /// <summary>
    /// 滑动倍率UI组件
    /// 负责滑块动画、区域检测、视觉展示
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class MultiplierSliderUI : MonoBehaviour
    {
        #region Zone Configuration

        /// <summary>
        /// 区域边界配置
        /// </summary>
        [Serializable]
        public class ZoneBounds
        {
            [Tooltip("区域名称（用于调试）")]
            public string zoneName = "Zone";

            [Tooltip("区域最小X坐标")]
            public float minX = -100f;

            [Tooltip("区域最大X坐标")]
            public float maxX = -60f;

            [Tooltip("区域颜色（可选，用于可视化）")]
            public Color zoneColor = Color.white;

            /// <summary>
            /// 检查X坐标是否在此区域内
            /// </summary>
            public bool ContainsX(float x)
            {
                return x >= minX && x <= maxX;
            }

            /// <summary>
            /// 获取区域中心X坐标
            /// </summary>
            public float GetCenterX()
            {
                return (minX + maxX) * 0.5f;
            }

            /// <summary>
            /// 获取区域宽度
            /// </summary>
            public float GetWidth()
            {
                return maxX - minX;
            }
        }

        #endregion

        #region Inspector Fields

        [Header("UI组件引用")]
        [Tooltip("滑块指针Transform")]
        [SerializeField] private RectTransform sliderPointer;

        [Tooltip("进度条背景Transform")]
        [SerializeField] private RectTransform progressBar;

        [Tooltip("倍率显示文本数组（对应各个区域）")]
        [SerializeField] private TextMeshProUGUI[] multiplierTexts;

        [Header("区域边界配置")]
        [Tooltip("5个区域的X坐标边界，在Inspector中手动设置")]
        [SerializeField] private ZoneBounds[] zoneBounds = new ZoneBounds[]
        {
            new() { zoneName = "Zone 0", minX = -350f, maxX = -210f, zoneColor = Color.red },      // 140宽度
            new() { zoneName = "Zone 1", minX = -210f, maxX = -70f,  zoneColor = Color.yellow },   // 140宽度
            new() { zoneName = "Zone 2", minX = -70f,  maxX = 78f,   zoneColor = Color.green },    // 148宽度（中心）
            new() { zoneName = "Zone 3", minX = 78f,   maxX = 218f,  zoneColor = Color.yellow },   // 140宽度
            new() { zoneName = "Zone 4", minX = 218f,  maxX = 358f,  zoneColor = Color.red }       // 140宽度
        };

        [Header("视觉效果")]
        [Tooltip("区域背景图片（可选）")]
        [SerializeField] private Image[] zoneBackgrounds;

        [Tooltip("是否显示区域高亮")]
        [SerializeField] private bool showZoneHighlight = true;

        [Tooltip("高亮颜色透明度")]
        [Range(0f, 1f)]
        [SerializeField] private float highlightAlpha = 0.3f;

        [Header("动画设置")]
        [Tooltip("使用平滑动画")]
        [SerializeField] private bool useSmoothAnimation = true;

        [Tooltip("动画平滑度")]
        [Range(1f, 10f)]
        [SerializeField] private float smoothness = 5f;

        #endregion

        #region Private Fields

        private float currentX = 0f;          // 当前X坐标
        private float targetX = 0f;           // 目标X坐标（用于平滑动画）
        private float sliderSpeed = 200f;     // 滑动速度（适配像素坐标）
        private bool isSliding = false;       // 是否正在滑动
        private float direction = 1f;         // 移动方向
        private Coroutine slideCoroutine;     // 滑动协程引用
        private int currentZoneIndex = 2;     // 当前所在区域索引

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            ValidateComponents();
            InitializeUI();
        }

        private void Start()
        {
            // 注册到管理器
            var manager = Core.MultiplierManager.Instance;
            if (manager != null)
            {
                manager.SetSliderUI(this);

                // 从配置读取滑动速度
                var settings = manager.GetSettings();
                if (settings != null)
                {
                    sliderSpeed = settings.sliderSpeed;
                    Debug.Log($"[MultiplierSliderUI] 从配置加载速度: {sliderSpeed} 像素/秒");
                }
                else
                {
                    Debug.LogWarning("[MultiplierSliderUI] 无法获取配置，使用默认速度");
                }
            }

            // 初始位置设置到中心
            ResetToCenter();
        }

        private void Update()
        {
            // 平滑更新指针位置
            if (useSmoothAnimation && sliderPointer != null)
            {
                currentX = Mathf.Lerp(currentX, targetX, Time.deltaTime * smoothness);
                UpdatePointerPosition(currentX);
            }
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// 验证组件引用
        /// </summary>
        private void ValidateComponents()
        {
            if (sliderPointer == null)
            {
                Debug.LogWarning("[MultiplierSliderUI] 滑块指针未设置，尝试查找子对象");
                var sliderTransform = transform.Find("SliderPointer");
                if (sliderTransform != null)
                {
                    sliderPointer = sliderTransform.GetComponent<RectTransform>();
                }
            }

            if (progressBar == null)
            {
                Debug.LogWarning("[MultiplierSliderUI] 进度条背景未设置，使用自身");
                progressBar = GetComponent<RectTransform>();
            }

            // 验证区域配置
            if (zoneBounds == null || zoneBounds.Length != 5)
            {
                Debug.LogError("[MultiplierSliderUI] 区域边界必须配置5个区域！");
            }
        }

        /// <summary>
        /// 初始化UI
        /// </summary>
        private void InitializeUI()
        {
            // 设置区域背景颜色
            if (zoneBackgrounds != null && showZoneHighlight)
            {
                for (int i = 0; i < zoneBackgrounds.Length && i < zoneBounds.Length; i++)
                {
                    if (zoneBackgrounds[i] != null)
                    {
                        Color color = zoneBounds[i].zoneColor;
                        color.a = highlightAlpha;
                        zoneBackgrounds[i].color = color;
                    }
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 开始滑动
        /// </summary>
        /// <param name="speed">滑动速度</param>
        public void StartSliding(float speed)
        {
            if (isSliding)
            {
                Debug.LogWarning("[MultiplierSliderUI] 滑动已经开始");
                return;
            }

            sliderSpeed = speed;
            // range 参数已弃用，滑动范围现在由 zoneBounds 配置决定
            isSliding = true;

            // 停止之前的协程
            if (slideCoroutine != null)
            {
                StopCoroutine(slideCoroutine);
            }

            slideCoroutine = StartCoroutine(SlideCoroutine());
        }

        /// <summary>
        /// 停止滑动并返回当前位置
        /// </summary>
        public float StopSliding()
        {
            if (!isSliding)
            {
                return currentX;
            }

            isSliding = false;

            // 停止协程
            if (slideCoroutine != null)
            {
                StopCoroutine(slideCoroutine);
                slideCoroutine = null;
            }

            // 更新当前区域
            currentZoneIndex = GetCurrentZone(currentX);

            Debug.Log($"[MultiplierSliderUI] 停止滑动 - 位置: {currentX}, 区域: {currentZoneIndex}");

            return currentX;
        }

        /// <summary>
        /// 获取当前所在区域索引
        /// </summary>
        public int GetCurrentZone(float xPosition)
        {
            // 遍历区域边界
            for (int i = 0; i < zoneBounds.Length; i++)
            {
                if (zoneBounds[i].ContainsX(xPosition))
                {
                    return i;
                }
            }

            // 边界处理
            if (xPosition < zoneBounds[0].minX)
                return 0;

            if (xPosition > zoneBounds[^1].maxX)
                return zoneBounds.Length - 1;

            // 默认返回中间区域
            return 2;
        }

        /// <summary>
        /// 重置到中心位置
        /// </summary>
        public void ResetToCenter()
        {
            currentX = 0f;
            targetX = 0f;
            UpdatePointerPosition(0f);
        }

        /// <summary>
        /// 设置指针位置（用于测试）
        /// </summary>
        public void SetPosition(float x)
        {
            currentX = x;
            targetX = x;
            UpdatePointerPosition(x);
        }

        /// <summary>
        /// 设置滑动速度（运行时调整）
        /// </summary>
        /// <param name="speed">新的滑动速度（像素/秒）</param>
        public void SetSliderSpeed(float speed)
        {
            sliderSpeed = Mathf.Max(10f, speed); // 最小速度为10
            Debug.Log($"[MultiplierSliderUI] 滑动速度已设置为: {sliderSpeed} 像素/秒");
        }

        /// <summary>
        /// 获取当前滑动速度
        /// </summary>
        public float GetSliderSpeed()
        {
            return sliderSpeed;
        }

        /// <summary>
        /// 设置各区域的倍率显示
        /// </summary>
        /// <param name="multipliers">倍率数组，对应各个区域</param>
        public void SetMultipliers(int[] multipliers)
        {
            if (multiplierTexts == null)
            {
                Debug.LogWarning("[MultiplierSliderUI] 倍率文本数组未设置");
                return;
            }

            if (multipliers == null)
            {
                Debug.LogWarning("[MultiplierSliderUI] 倍率数组为空");
                return;
            }

            // 设置每个文本框的倍率显示
            for (int i = 0; i < multiplierTexts.Length && i < multipliers.Length; i++)
            {
                if (multiplierTexts[i] != null)
                {
                    multiplierTexts[i].text = $"x{multipliers[i]}";
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 滑动协程
        /// </summary>
        private IEnumerator SlideCoroutine()
        {
            // 从区域配置获取移动范围
            float minX = zoneBounds[0].minX;
            float maxX = zoneBounds[^1].maxX;

            while (isSliding)
            {
                // 更新目标位置
                targetX += direction * sliderSpeed * Time.deltaTime;

                // 边界检测和反弹
                if (targetX >= maxX || targetX <= minX)
                {
                    targetX = Mathf.Clamp(targetX, minX, maxX);
                    direction *= -1f;
                }

                // 如果不使用平滑动画，直接更新
                if (!useSmoothAnimation)
                {
                    currentX = targetX;
                    UpdatePointerPosition(currentX);
                }

                // 更新当前区域
                int newZone = GetCurrentZone(currentX);
                if (newZone != currentZoneIndex)
                {
                    OnZoneChanged(currentZoneIndex, newZone);
                    currentZoneIndex = newZone;
                }

                yield return null;
            }
        }

        /// <summary>
        /// 更新指针位置
        /// </summary>
        private void UpdatePointerPosition(float x)
        {
            if (sliderPointer != null)
            {
                Vector3 pos = sliderPointer.anchoredPosition;
                pos.x = x;
                sliderPointer.anchoredPosition = pos;
            }

            UpdatePointerVisual(x);
        }

        /// <summary>
        /// 更新指针视觉效果
        /// </summary>
        private void UpdatePointerVisual(float x)
        {
            // 视觉效果更新（倍率文本由外部SetMultipliers方法设置）
        }

        /// <summary>
        /// 区域变化时的处理
        /// </summary>
        private void OnZoneChanged(int oldZone, int newZone)
        {
            // 高亮当前区域
            if (zoneBackgrounds != null && showZoneHighlight)
            {
                // 恢复旧区域颜色
                if (oldZone >= 0 && oldZone < zoneBackgrounds.Length && zoneBackgrounds[oldZone] != null)
                {
                    Color oldColor = zoneBounds[oldZone].zoneColor;
                    oldColor.a = highlightAlpha;
                    zoneBackgrounds[oldZone].color = oldColor;
                }

                // 高亮新区域
                if (newZone >= 0 && newZone < zoneBackgrounds.Length && zoneBackgrounds[newZone] != null)
                {
                    Color newColor = zoneBounds[newZone].zoneColor;
                    newColor.a = highlightAlpha * 1.5f;  // 稍微亮一些
                    zoneBackgrounds[newZone].color = newColor;
                }
            }
        }

        #endregion

        #region Editor Helpers

        /// <summary>
        /// 验证区域边界配置是否合法
        /// </summary>
        [ContextMenu("Validate Zone Bounds")]
        private void ValidateZoneBounds()
        {
            bool isValid = true;

            if (zoneBounds == null || zoneBounds.Length != 5)
            {
                Debug.LogError("[MultiplierSliderUI] 必须配置5个区域！");
                return;
            }

            // 检查每个区域的有效性
            for (int i = 0; i < zoneBounds.Length; i++)
            {
                if (zoneBounds[i].minX >= zoneBounds[i].maxX)
                {
                    Debug.LogError($"[MultiplierSliderUI] 区域 {i} 配置错误: minX({zoneBounds[i].minX}) >= maxX({zoneBounds[i].maxX})");
                    isValid = false;
                }

                // 检查是否有重叠
                for (int j = i + 1; j < zoneBounds.Length; j++)
                {
                    if (zoneBounds[i].maxX > zoneBounds[j].minX && zoneBounds[i].minX < zoneBounds[j].maxX)
                    {
                        Debug.LogWarning($"[MultiplierSliderUI] 区域 {i} 和区域 {j} 存在重叠");
                    }
                }
            }

            if (isValid)
            {
                Debug.Log("[MultiplierSliderUI] 区域边界配置验证通过");
            }
        }

        /// <summary>
        /// 在编辑器中绘制Gizmos
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (progressBar == null || zoneBounds == null) return;

            // 绘制区域边界
            Vector3 basePos = progressBar.transform.position;
            float height = 50f;

            for (int i = 0; i < zoneBounds.Length; i++)
            {
                Gizmos.color = zoneBounds[i].zoneColor;

                Vector3 minPos = basePos + new Vector3(zoneBounds[i].minX, 0, 0);
                Vector3 maxPos = basePos + new Vector3(zoneBounds[i].maxX, 0, 0);

                // 绘制区域边界线
                Gizmos.DrawLine(minPos + Vector3.up * height, minPos - Vector3.up * height);
                Gizmos.DrawLine(maxPos + Vector3.up * height, maxPos - Vector3.up * height);

                // 绘制区域范围
                Color areaColor = zoneBounds[i].zoneColor;
                areaColor.a = 0.2f;
                Gizmos.color = areaColor;
                Vector3 center = (minPos + maxPos) * 0.5f;
                Vector3 size = new(zoneBounds[i].GetWidth(), height * 2, 1f);
                Gizmos.DrawCube(center, size);
            }
        }

        #endregion
    }
}