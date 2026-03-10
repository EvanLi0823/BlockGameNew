---
name: design-interrogation
description: 提供设计文档审查框架,包括质询问题库、边界情况清单、常见设计缺陷识别。支持功能开发说明、架构设计方案、重构方案、Bug修复方案的全面审查。用于设计阶段的查漏补缺,风险识别,影响评估。
version: 1.0.0
author: BlockGame Team
---

# Design Interrogation Framework

设计质询技能,为设计文档审查提供系统化的质询框架、问题库和检查清单。

## Skill Purpose

帮助审查者对设计文档进行全面深入的质询,发现潜在问题、遗漏和风险,确保设计方案的完整性和可靠性。

## When This Skill Triggers

- 审查功能开发说明文档
- 审查架构设计方案
- 审查重构方案
- 审查Bug修复方案
- 设计阶段的风险识别
- 设计方案的查漏补缺

## Quick Reference Guide

| 文档类型 | 审查重点 | Reference |
|---------|---------|-----------|
| **功能开发说明** | UI设计、配置系统、数据流、集成点 | [functional-design.md](references/interrogation/functional-design.md) ⭐⭐⭐ |
| **架构设计方案** | 模块职责、接口设计、依赖关系、扩展性 | [architecture-design.md](references/interrogation/architecture-design.md) ⭐⭐⭐ |
| **重构方案** | 向后兼容、迁移步骤、风险控制、回滚方案 | [refactoring-design.md](references/interrogation/refactoring-design.md) ⭐⭐⭐ |
| **Bug修复方案** | 根因分析、修复方案、回归测试、验证方法 | [bug-fix-design.md](references/interrogation/bug-fix-design.md) ⭐⭐⭐ |
| **通用检查** | 常见设计缺陷识别 | [common-pitfalls.md](references/interrogation/common-pitfalls.md) ⭐⭐ |
| **边界情况** | 数据边界、时序边界、状态边界、平台边界 | [boundary-cases.md](references/interrogation/boundary-cases.md) ⭐⭐ |
| **质询问题库** | 分类问题模板、提问技巧 | [question-bank.md](references/interrogation/question-bank.md) ⭐ |

## 质询维度框架

### 维度1: 实现方案审查
- 如何实现? 使用哪些组件/技术?
- 为什么这样实现? 有其他方案吗?
- 实现细节: 数据结构、生命周期、API设计
- 参考: [对应文档类型的reference]

### 维度2: 系统影响分析
- 向后兼容性: 现有功能是否受影响?
- 性能影响: 是否有性能瓶颈?
- 依赖变化: 模块依赖关系变化?
- 数据迁移: 旧数据如何处理?
- 参考: [common-pitfalls.md](references/interrogation/common-pitfalls.md)

### 维度3: Bug风险识别
- 空引用风险: NullReferenceException可能性
- 时序问题: 初始化顺序、生命周期
- 并发问题: 协程/异步竞争条件
- 边界条件: 空值、极值、异常场景
- 参考: [boundary-cases.md](references/interrogation/boundary-cases.md)

### 维度4: 边界情况验证
- 数据边界: null、空集合、极端数值
- 时序边界: 未初始化、场景切换、暂停/恢复
- 状态边界: 已销毁对象、禁用GameObject
- 平台边界: 分辨率、性能、版本
- 参考: [boundary-cases.md](references/interrogation/boundary-cases.md)

### 维度5: 可维护性审查
- 代码可读性: 命名、注释
- 可测试性: 验证方法、调试便利性
- 可扩展性: 未来需求扩展
- 配置灵活性: 可配置参数
- 参考: [对应文档类型的reference]

## 质询流程

```
阶段1: 文档理解
  ↓ 识别文档类型
  ↓ 分析相关代码上下文
  ↓ 检查依赖关系

阶段2: 多维度质询 (对话式)
  ↓ 维度1: 实现方案审查
  ↓ 维度2: 系统影响分析
  ↓ 维度3: Bug风险识别
  ↓ 维度4: 边界情况验证
  ↓ 维度5: 可维护性审查

阶段3: 生成质询报告
  ↓ 高优先级问题 (必须解决)
  ↓ 中优先级问题 (建议解决)
  ↓ 低优先级建议 (可选优化)
  ↓ 后续行动建议
```

## 使用示例

### 审查功能开发说明
```bash
# 1. 读取设计文档
Read: Documents/开发计划/XXX_开发说明.md

# 2. 加载对应审查清单
参考: references/interrogation/functional-design.md

# 3. 执行多维度质询
使用: references/interrogation/question-bank.md
检查: references/interrogation/boundary-cases.md

# 4. 生成质询报告
输出: 对话式质询报告
```

## 输出格式

质询报告采用**对话式**格式:
- 分轮次提问,逐层深入
- 每个问题说明目的和风险
- 提供建议方案和替代选项
- 最终总结高/中/低优先级问题
- 列出后续行动建议

详细模板参考各文档类型的reference文件。
