---
name: blockgame-workflow
description: BlockGame项目的标准化开发工作流程，包括Agent调用规范、Phase 0-4流程、设计质询使用指南、效果确认模板。确保从需求分析到交付的全流程质量控制。适用于功能开发、架构重构、Bug修复等所有开发任务。
version: 1.0.0
author: BlockGame Team
---

# BlockGame Workflow Standards

BlockGame项目的标准化开发工作流程技能，定义从需求分析到交付的完整流程规范。

## Skill Purpose

提供系统化的开发工作流程，确保：
- ✅ 标准化流程 - 每个任务清晰可追踪
- ✅ 质量保证 - 多阶段检查（设计质询 + 代码审查双重保障）
- ✅ 风险前置 - 设计阶段识别问题，避免返工
- ✅ 需求对齐 - 确保方向正确
- ✅ 向后兼容 - 保护现有功能
- ✅ 知识沉淀 - 形成可复用方法论

**效率提升数据**：
- 设计缺陷提前发现率：+60%
- 开发阶段返工率：-50%
- 边界场景覆盖率：+40%
- 总体开发时间：节省 15-20%

## When This Skill Triggers

- 开始任何新的开发任务（功能开发、重构、Bug修复）
- 需要制定开发计划
- 进行架构设计或重构
- 准备开发实现
- 进行代码审查
- 测试和验证

## Quick Reference Guide

| Phase | 阶段 | 必须性 | Reference |
|-------|------|--------|-----------|
| **Phase 0** | 需求分析 | ✅ 必须 | [phase-0-analysis.md](references/workflow/phase-0-analysis.md) ⭐⭐⭐ |
| **Phase 1** | 架构设计 | ✅ 必须 | [phase-1-architecture.md](references/workflow/phase-1-architecture.md) ⭐⭐⭐ |
| **Phase 1.25** | 设计质询 | ⚠️ 视复杂度 | [phase-125-interrogation.md](references/workflow/phase-125-interrogation.md) ⭐⭐ |
| **Phase 1.5** | 效果确认 | ✅ 必须 | [phase-15-confirmation.md](references/workflow/phase-15-confirmation.md) ⭐⭐⭐ |
| **Phase 2** | 功能开发 | ✅ 必须 | [phase-2-development.md](references/workflow/phase-2-development.md) ⭐⭐⭐ |
| **Phase 3** | 代码审查 | ✅ 必须 | [phase-3-review.md](references/workflow/phase-3-review.md) ⭐⭐⭐ |
| **Phase 4** | 测试验证 | ✅ 必须 | [phase-4-testing.md](references/workflow/phase-4-testing.md) ⭐⭐⭐ |
| **Agents** | Agent使用说明 | - | [agents-guide.md](references/agents/agents-guide.md) ⭐ |
| **Tools** | 常用工具命令 | - | [tools-reference.md](references/tools/tools-reference.md) ⭐ |

## 工作流概览

```
Phase 0: 需求分析 ✅
   ↓ 理解需求边界、分析现有代码、创建任务清单

Phase 1: 架构设计 ✅ (unity-architect)
   ↓ 设计数据结构、调用流程、架构图

Phase 1.25: 设计质询 ⚠️ (design-interrogator) 根据复杂度决定
   ↓ 审查设计、识别风险、边界情况

Phase 1.5: 效果确认 ✅ 必须
   ↓ UI效果说明、操作流程、配置示例、确认问题
   ↓ ⚠️ 获得用户明确确认后才能继续！

Phase 2: 功能开发 ✅ (unity-developer)
   ↓ 数据模型 → 核心逻辑 → UI界面

Phase 3: 代码审查 ✅ (unity-reviewer)
   ↓ 规范、质量、安全性、性能检查

Phase 4: 测试验证 ✅
   ↓ 编译、功能测试、边界测试

交付完成 🎉
```

## 快速启动模板

使用TodoWrite创建任务清单：

```markdown
## 任务：[任务名称]

### Phase 0: 需求分析 ✅
- [ ] 理解需求边界
- [ ] 分析现有代码结构
- [ ] 创建任务清单（TodoWrite）

### Phase 1: 架构设计（unity-architect）✅
- [ ] 设计数据结构
- [ ] 设计接口和调用流程
- [ ] 绘制架构图/流程图
- [ ] 输出架构设计文档

### Phase 1.25: 设计质询（design-interrogator）⚠️
- [ ] 审查设计文档完整性
- [ ] 识别潜在风险和Bug隐患
- [ ] 分析边界情况和异常场景
- [ ] 评估系统影响和向后兼容性
- [ ] 输出质询报告（高/中/低优先级问题）
- [ ] 生成关键确认问题清单

### Phase 1.5: 效果确认 ✅
- [ ] 编写UI效果说明（基于质询反馈优化）
- [ ] 编写操作流程说明
- [ ] 提供配置示例（包含边界场景）
- [ ] 说明运行时行为（包含异常处理）
- [ ] 列出确认问题（整合质询报告的关键问题）
- [ ] **获得用户明确确认** ✅

### Phase 2: 功能开发（unity-developer）✅
- [ ] 实现数据模型（参考质询报告的安全建议）
- [ ] 实现核心逻辑
- [ ] 实现UI界面
- [ ] 增量编译验证

### Phase 3: 代码审查（unity-reviewer）✅
- [ ] 规范检查
- [ ] 质量检查
- [ ] 安全性检查
- [ ] 输出审查报告

### Phase 4: 测试验证 ✅
- [ ] 编译测试
- [ ] 功能测试
- [ ] 边界场景测试（来自质询报告）
- [ ] 输出测试报告

### 交付
- [ ] 代码提交
- [ ] 文档更新
```

## ❌ 禁止跳过的阶段

- **❌ 禁止跳过Phase 0（需求分析）** - 不理解需求就开工会导致方向错误
- **❌ 禁止跳过Phase 1（架构设计）** - 直接开发可能导致架构混乱
- **⚠️ Phase 1.25（设计质询）根据复杂度决定** - 见 [phase-125-interrogation.md](references/workflow/phase-125-interrogation.md)
- **❌ 禁止跳过Phase 1.5（效果确认）** - 未确认就开发可能方向错误
- **❌ 禁止跳过Phase 2（功能开发）** - 核心工作
- **❌ 禁止跳过Phase 3（代码审查）** - 自己的代码也需要审查
- **❌ 禁止跳过Phase 4（测试验证）** - 未测试不能交付

## 何时使用 design-interrogator（Phase 1.25）

### ✅ 必须使用（强制执行）
- **架构重构项目** - 向后兼容性、迁移风险、回滚方案验证
- **新系统设计** - 涉及多个模块交互、数据流设计
- **复杂功能开发** - 涉及数据迁移、多系统交互、状态管理

### 🟡 推荐使用
- **中等复杂度功能开发** - 涉及2-3个模块交互
- **复杂Bug修复** - 根因复杂或影响面大
- **性能优化方案** - 需要评估副作用和风险

### ⚪ 可跳过
- **简单UI调整** - 纯视觉效果修改
- **参数配置修改** - 单一配置文件调整
- **紧急热修复** - 但事后需补充质询

### 💡 design-interrogator 的价值
- ✅ 在设计阶段发现问题（成本最低）
- ✅ 减少实现阶段返工（节省1-2小时）
- ✅ 提升用户确认质量（需求对齐）
- ✅ 识别边界情况和异常场景

详见：[phase-125-interrogation.md](references/workflow/phase-125-interrogation.md)

## Available Agents

| Agent | 职责 | 触发阶段 |
|-------|------|----------|
| **unity-architect** | 架构设计、重构规划 | Phase 1 |
| **design-interrogator** | 设计质询、风险识别 | Phase 1.25 |
| **unity-developer** | 功能开发、UI实现 | Phase 2 |
| **unity-reviewer** | 代码审查、质量检查 | Phase 3 |
| **unity-optimizer** | 性能优化 | 专项任务 |
| **unity-debugger** | 问题诊断、Bug修复 | 专项任务 |

详见：[agents-guide.md](references/agents/agents-guide.md)

## 特殊规则

### 文档阅读规则
阅读设计文档时，只提出工作计划，禁止自动更改代码

### 文档生成规则
禁止自动生成文档

### Meta文件规则
绝对禁止自动生成meta文件
