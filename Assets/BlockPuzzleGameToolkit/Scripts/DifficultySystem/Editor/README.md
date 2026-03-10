# 难度系统编辑器工具使用指南

## 概述

本目录包含了关卡难度系统的Unity Editor编辑器工具，为策划提供可视化的难度评估和配置界面。

## 组件列表

### P0 - 必须完成（核心功能）

#### 1. LevelDifficultyInspector
**文件:** `LevelDifficultyInspector.cs`
**功能:** 扩展Level资源的Inspector，显示难度评分
**使用方法:**
1. 在Project窗口中选择任意Level资源
2. 在Inspector面板中会看到"难度系统"折叠面板
3. **进入Play模式**（必须）
4. 点击"计算难度"按钮
5. 查看难度分析结果：
   - 总分和难度等级
   - 六维度分析（进度条可视化）
   - 动态难度预览

**显示内容:**
- 总分: 0-100分
- 等级: Tutorial/Easy/Normal/Hard/Expert/Master
- 六维度分数:
  - 空间压力 (Space Stress)
  - 方块复杂度 (Shape Complexity)
  - 目标压力 (Target Pressure)
  - 时间压力 (Time Pressure)
  - 资源限制 (Resource Constraint)
  - 策略深度 (Strategy Depth)
- 动态难度预览:
  - 基础块/异形块/大块概率
  - 失败阈值

#### 2. ShapeTemplateInspector
**文件:** `ShapeTemplateInspector.cs`
**功能:** 扩展ShapeTemplate资源的Inspector
**使用方法:**
1. 在Project窗口中选择任意ShapeTemplate资源
2. 在Inspector面板中会看到"方块分析"折叠面板
3. **进入Play模式**（必须）
4. 点击"分析方块"按钮
5. 查看分析结果：
   - 分类（Basic/Shaped/Large）
   - 统计数据（格子数、尺寸、是否矩形、是否对称）
   - 可视化预览（5x5网格）

**显示内容:**
- 分类: Basic(基础块)/Shaped(异形块)/Large(大块)
- 统计数据:
  - 格子数
  - 尺寸 (宽x高)
  - 是否矩形
  - 是否对称
- 可视化预览: 5x5网格显示方块形状

### P1 - 推荐完成（批量工具）

#### 3. DifficultyCalculatorWindow
**文件:** `DifficultyCalculatorWindow.cs`
**菜单位置:** `Window → Difficulty System → Batch Calculator`
**功能:** 批量计算关卡难度和方块分析
**使用方法:**

**关卡部分:**
1. 打开窗口: `Window → Difficulty System → Batch Calculator`
2. 点击"扫描所有关卡"按钮
3. 系统会显示找到的关卡数量
4. **进入Play模式**（必须）
5. 点击"批量计算难度"按钮
6. 等待进度条完成
7. 查看统计报告（各难度等级分布）

**方块部分:**
1. 在同一窗口中，点击"扫描所有方块"按钮
2. 系统会显示找到的方块数量
3. **进入Play模式**（必须）
4. 点击"批量分析方块"按钮
5. 等待完成
6. 查看统计报告（各分类分布）

**输出:**
- 关卡难度分布统计（带进度条可视化）
- 方块分类分布统计（带颜色区分）

#### 4. DifficultyAnalysisWindow
**文件:** `DifficultyAnalysisWindow.cs`
**菜单位置:** `Window → Difficulty System → Level Analysis`
**功能:** 单个关卡的深度分析
**使用方法:**
1. 打开窗口: `Window → Difficulty System → Level Analysis`
2. 在"选择关卡"字段中拖入或选择一个Level资源
3. **进入Play模式**（必须）
4. 点击"分析关卡"按钮
5. 查看详细分析结果：
   - 总体概览
   - 六维度详细分析（带描述）
   - 雷达图（六边形可视化）
   - 优化建议

**输出:**
- 总体概览: 总分、等级、进度条
- 六维度详细分析: 每个维度的分数、描述、彩色进度条
- 雷达图: 六边形雷达图展示各维度平衡性
- 优化建议:
  - 针对过高/过低的维度提供调整建议
  - 维度平衡性检查
  - 难度定位建议

## 重要注意事项

### ⚠️ 必须在运行模式下使用

**所有计算和分析功能都必须在Unity的Play模式下运行！**

原因:
- `LevelDifficultyCalculator` 和 `ShapeTemplateAnalyzer` 是运行时单例
- 它们需要通过GameManager初始化
- 在编辑器模式下，单例的Instance为null

**正确流程:**
1. 打开Unity编辑器
2. 点击Play按钮进入运行模式
3. 使用工具进行计算/分析
4. 退出Play模式后，数据会被保存到资源文件

### 📁 文件路径

所有编辑器脚本位于:
```
/Assets/BlockPuzzleGameToolkit/Scripts/DifficultySystem/Editor/
```

所有脚本使用命名空间:
```csharp
namespace GameCore.DifficultySystem.Editor
```

### 🔒 代码保护

所有编辑器代码都使用 `#if UNITY_EDITOR` 保护，确保不会被打包到最终游戏中。

## 工作流程建议

### 初始化设置
1. 首次使用时，先使用**批量工具**:
   - 打开 `Batch Calculator` 窗口
   - 扫描并分析所有方块模板
   - 扫描并计算所有关卡难度

### 日常使用
2. 创建/修改关卡后:
   - 在Inspector中使用 `LevelDifficultyInspector` 快速查看单个关卡
   - 或使用 `Level Analysis` 窗口进行深度分析

3. 创建/修改方块后:
   - 在Inspector中使用 `ShapeTemplateInspector` 快速分析

### 难度平衡
4. 使用 `Level Analysis` 窗口:
   - 查看雷达图检查维度平衡性
   - 根据优化建议调整关卡参数
   - 重新计算验证效果

## 数据存储

所有计算结果都会保存到资源文件中:

**Level资源保存的字段:**
- `difficultyScore` - 总分 (0-100)
- `difficultyLevel` - 难度等级
- `breakdown` - 六维度详细分数
- `dynamicPreview` - 动态难度预览

**ShapeTemplate资源保存的字段:**
- `category` - 分类 (Basic/Shaped/Large)
- `width`, `height` - 尺寸
- `cellCount` - 格子数
- `isRectangle` - 是否矩形
- `isSymmetrical` - 是否对称

## 故障排除

### 问题: 点击按钮后提示"未初始化"
**解决:** 确保已进入Play模式

### 问题: 计算结果不正确
**解决:**
1. 检查DifficultyWeights配置是否正确加载
2. 检查关卡数据是否完整
3. 查看Console是否有错误日志

### 问题: 批量计算卡住
**解决:**
1. 检查是否有大量关卡（可能需要等待）
2. 查看进度条是否在移动
3. 检查Console是否有错误

## 版本信息

- 创建日期: 2026-02-27
- Unity版本要求: Unity 2021.3 LTS+
- 依赖: GameCore.DifficultySystem 命名空间下的所有核心类

## 相关文档

- `/Assets/BlockPuzzleGameToolkit/Scripts/DifficultySystem/README.md` - 难度系统总体说明
- 完整架构设计文档
