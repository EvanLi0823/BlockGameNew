# Changelog

## v2.1.0 - 2026-03-11

### ✨ 批量生成Prefab文档

#### 新增功能
- ✅ 批量生成28个Popups目录下所有Prefab的文档
- ✅ 创建Prefab索引文档（Prefab_Index.md）
- ✅ 自动统计和分类Prefab

#### 文档统计
- **Prefab文档数量**: 28个
- **总GameObject数**: 233个
- **平均复杂度**: 8.3 GameObjects/Prefab

#### 按复杂度排序
**最复杂的Prefab**:
1. Settings (34 GameObjects)
2. LuckySpin (28 GameObjects)
3. Win_Slider, Win_Fixed, CommonRewardPopup (17 GameObjects)

**按类型分类**:
- 胜利弹窗: Win_Slider, Win_Fixed, PreWin, PreWin_Score, PreWin_Bonus
- 失败弹窗: Failed, Failed_Classic, Failed_Score, Failed_Bonus, PreFailed
- 奖励类: CommonRewardPopup, RewardPopupCoins, Reward_Small, DailyBonus
- 设置类: Settings, SettingsGame, Quit
- 其他: Loading, LuckySpin, PropPurchase, NoAds

#### 索引功能
- 按复杂度排序的Prefab列表
- 按类型分类（Win/Failed/Reward/Settings/其他）
- 快速搜索指南
- 批量操作命令

#### 更新的文档
- `SKILL.md` - 添加Prefab索引引用，保持88行
- `README.md` - 添加Prefab查询示例和统计信息
- `Prefab_Index.md` - 新增索引文档

---

## v2.0.0 - 2026-03-11

### ✨ 新功能

#### Prefab支持
- ✅ 扩展 `generate_scene_doc.py` 脚本以支持 `.prefab` 文件
- ✅ 添加 CommonRewardPopup prefab 文档作为示例
- ✅ 自动识别文件类型（场景 vs Prefab）并生成相应文档

#### 文档重构
- ✅ 将详细内容从 SKILL.md 移至独立参考文档
- ✅ 创建 `usage-guide.md` - 常见查询和示例工作流
- ✅ 创建 `regeneration-guide.md` - 文档重新生成指南
- ✅ SKILL.md 从 141 行精简至 80 行（符合 <100 行要求）

### 📝 文档更新

#### 新增文档
- `references/Prefab_CommonRewardPopup_Hierarchy.md` - CommonRewardPopup prefab 结构文档（17 GameObjects）
- `references/usage-guide.md` - 使用指南和工作流示例
- `references/regeneration-guide.md` - 文档生成和维护指南

#### 更新文档
- `SKILL.md` v2.0.0 - 添加 prefab 支持，精简为 80 行
- `scripts/generate_scene_doc.py` - 支持场景和 Prefab 文件

### 🔧 技术改进

#### 脚本增强
- 自动检测文件类型（.unity / .prefab）
- 根据文件类型生成相应的文档标题和说明
- 统一的输出格式和命名规范

#### 文档组织
- 采用渐进式披露原则
- SKILL.md 提供核心功能和快速开始
- 详细内容移至专门的参考文档

### 📊 统计数据

**文件数量**：
- 场景文档：1 个（Scene_Main_Hierarchy.md）
- Prefab文档：1 个（Prefab_CommonRewardPopup_Hierarchy.md）
- 参考指南：2 个（usage-guide.md, regeneration-guide.md）
- 脚本文件：1 个（generate_scene_doc.py）

**代码行数**：
- SKILL.md：80 行（从 141 行优化）
- generate_scene_doc.py：263 行
- usage-guide.md：~200 行
- regeneration-guide.md：~280 行

### 🎯 使用场景扩展

现在支持以下场景：

**场景查询**：
- 查询 Main 场景的 GameObject 层级结构
- 查找 UI 面板、Manager 组件
- 了解场景根节点和系统组件

**Prefab查询**：
- 查询 Prefab 的 GameObject 层级结构
- 在修改脚本时快速了解对应 prefab 结构
- 查找 UI 元素（按钮、文本、图片）
- 规划 prefab 修改

### ✅ 验证结果

- ✅ SKILL.md 行数检查：80 行 < 100 行
- ✅ 技能验证：`Skill is valid!`
- ✅ 文档生成测试：成功生成 CommonRewardPopup 文档
- ✅ 功能完整性：场景 + Prefab 双重支持

---

## v1.0.0 - 2026-03-11 (初始版本)

### ✨ 初始功能

- ✅ Main 场景文档生成
- ✅ Python 脚本自动生成场景 Hierarchy 文档
- ✅ 支持 grep 搜索
- ✅ 完整的 GameObject 和组件信息

### 📝 初始文档

- `SKILL.md` - 技能说明（初始 88 行）
- `README.md` - 使用说明
- `references/Scene_Main_Hierarchy.md` - Main 场景文档
- `scripts/generate_scene_doc.py` - 文档生成脚本

---

**维护者**: BlockGame Team
**最后更新**: 2026-03-11
