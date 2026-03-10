// 金钱方块系统 - 刷新器
// 创建日期: 2026-03-05

using UnityEngine;

namespace BlockPuzzleGameToolkit.Scripts.MoneyBlockSystem
{
    /// <summary>
    /// 金钱方块刷新器
    /// 职责:
    /// - 刷新条件判定(放置计数、关卡上限)
    /// - 形状选择和格子检测
    /// - 优先级处理(宝石>金钱)
    /// - 金钱图标创建和添加
    /// </summary>
    public class MoneyBlockSpawner
    {
        private readonly MoneyBlockSettings settings;
        private bool enableDebugLog;

        public MoneyBlockSpawner(MoneyBlockSettings settings)
        {
            this.settings = settings;
            this.enableDebugLog = settings != null && settings.enableDebugLog;
        }

        /// <summary>
        /// 检查是否可以刷新
        /// </summary>
        public bool CanSpawn(MoneyBlockSaveData data, MoneyBlockSettings settings)
        {
            if (data == null || settings == null)
            {
                Debug.LogError("[MoneyBlockSpawner] 数据或配置为null");
                return false;
            }

            // 检查关卡上限
            if (data.spawnCountInLevel >= settings.maxMoneyBlocksPerLevel)
            {
                if (enableDebugLog)
                {
                    Debug.Log($"[MoneyBlockSpawner] 已达关卡上限: {data.spawnCountInLevel}/{settings.maxMoneyBlocksPerLevel}");
                }
                return false;
            }

            // 检查放置计数（必须大于0，避免关卡加载时就满足条件）
            bool shouldSpawn = data.shapePlacementCount > 0 &&
                              (data.shapePlacementCount % settings.shapePlacementTrigger) == 0;

            if (enableDebugLog && shouldSpawn)
            {
                Debug.Log($"[MoneyBlockSpawner] 满足刷新条件: 放置计数={data.shapePlacementCount}, 触发间隔={settings.shapePlacementTrigger}");
            }

            return shouldSpawn;
        }

        /// <summary>
        /// 尝试在下一个形状上刷新金钱方块
        /// </summary>
        /// <param name="targetShape">目标形状GameObject</param>
        /// <param name="cellIndex">选中的格子索引</param>
        /// <returns>是否成功选择刷新位置</returns>
        public bool TrySpawnOnNextShape(out GameObject targetShape, out int cellIndex)
        {
            targetShape = null;
            cellIndex = -1;

            // TODO: 这里需要根据项目实际的形状管理系统来实现
            // 示例逻辑:
            // 1. 获取待放置的形状列表
            // 2. 随机选择一个形状
            // 3. 获取该形状的所有格子
            // 4. 检查每个格子是否已有宝石
            // 5. 从无宝石的格子中随机选择一个

            if (enableDebugLog)
            {
                Debug.LogWarning("[MoneyBlockSpawner] TrySpawnOnNextShape未实现，需要集成形状管理系统");
            }

            return false;
        }

        /// <summary>
        /// 添加金钱图标到Item（复用Bonus系统）
        /// </summary>
        public void AddMoneyIconToCell(GameObject itemObject)
        {
            if (itemObject == null)
            {
                Debug.LogError("[MoneyBlockSpawner] 目标Item为null");
                return;
            }

            if (settings.moneyBonusTemplate == null)
            {
                Debug.LogError("[MoneyBlockSpawner] 金钱图标模板未设置");
                return;
            }

            // 获取Item组件（Shape上的方块是Item，不是Cell）
            var item = itemObject.GetComponent<BlockPuzzleGameToolkit.Scripts.Gameplay.Item>();
            if (item == null)
            {
                Debug.LogError("[MoneyBlockSpawner] 目标对象没有Item组件");
                return;
            }

            // 使用Item的Bonus系统设置金钱图标（和宝石一样）
            item.SetBonus(settings.moneyBonusTemplate);

            // 添加MoneyBlock组件用于标识和状态管理
            var moneyBlock = itemObject.GetComponent<MoneyBlock>();
            if (moneyBlock == null)
            {
                moneyBlock = itemObject.AddComponent<MoneyBlock>();
            }
            moneyBlock.Initialize();

            if (enableDebugLog)
            {
                Debug.Log($"[MoneyBlockSpawner] 成功添加金钱图标到Item: {itemObject.name}");
            }
        }

        /// <summary>
        /// 检查宝石优先级
        /// 如果格子已有宝石组件，返回true（跳过该格子）
        /// </summary>
        public bool CheckBonusItemPriority(GameObject cell)
        {
            if (cell == null)
                return false;

            // 检查是否已有宝石组件
            // TODO: 需要确认项目中宝石组件的实际类名
            // 这里先用通用检查逻辑

            // 方式1: 通过组件名检查
            var components = cell.GetComponents<Component>();
            foreach (var comp in components)
            {
                if (comp != null && comp.GetType().Name.Contains("Bonus"))
                {
                    if (enableDebugLog)
                    {
                        Debug.Log($"[MoneyBlockSpawner] 格子{cell.name}已有宝石组件{comp.GetType().Name}，跳过");
                    }
                    return true;
                }
            }

            // 方式2: 通过Tag检查
            if (cell.CompareTag("BonusItem"))
            {
                if (enableDebugLog)
                {
                    Debug.Log($"[MoneyBlockSpawner] 格子{cell.name}标记为宝石，跳过");
                }
                return true;
            }

            return false; // 无宝石，可以刷新金钱
        }
    }
}
