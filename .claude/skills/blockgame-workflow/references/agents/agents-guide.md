# BlockGame项目Agent使用指南

## 可用的Agent代理

| Agent | 职责 | 触发阶段 | 位置 | 使用场景 |
|-------|------|----------|------|----------|
| **unity-architect** | 架构设计、重构规划 | Phase 1 | `.claude/agents/unity-architect.md` | 新系统设计、架构重构 |
| **design-interrogator** | 设计质询、风险识别 | Phase 1.25 | `.claude/agents/design-interrogator.md` | 复杂功能设计审查 |
| **unity-developer** | 功能开发、UI实现 | Phase 2 | `.claude/agents/unity-developer.md` | 代码实现 |
| **unity-reviewer** | 代码审查、质量检查 | Phase 3 | `.claude/agents/unity-reviewer.md` | 代码提交前审查 |
| **unity-optimizer** | 性能优化 | 专项任务 | `.claude/agents/unity-optimizer.md` | 性能瓶颈优化 |
| **unity-debugger** | 问题诊断、Bug修复 | 专项任务 | `.claude/agents/unity-debugger.md` | Bug定位和修复 |

## Agent职责详解

### unity-architect（架构师）

**职责**：
- 分析现有架构
- 设计新系统架构
- 规划重构方案
- 绘制架构图和流程图
- 定义接口和数据结构

**输入**：
- 需求描述
- 现有代码结构
- 技术约束

**输出**：
- 架构设计文档
- 类图/流程图（Mermaid格式）
- 数据结构定义
- 接口规范

**使用方式**：
```
"请使用 unity-architect 代理设计这个功能的架构"
```

**何时使用**：
- ✅ 新增系统模块
- ✅ 架构重构
- ✅ 复杂功能设计
- ❌ 简单UI调整（不需要）
- ❌ 参数配置修改（不需要）

---

### design-interrogator（设计质询师）

**职责**：
- 审查设计文档
- 识别潜在风险
- 发现设计缺陷
- 评估边界情况
- 提出改进建议

**输入**：
- Phase 1的架构设计文档
- 相关代码上下文

**输出**：
- 质询报告（高/中/低优先级问题）
- 关键确认问题清单
- 边界场景测试用例
- 后续行动建议

**使用方式**：
```
"请使用 design-interrogator 代理审查这个设计方案"
```

**何时使用**：
- ✅ 架构重构项目（必须）
- ✅ 新系统设计（必须）
- ✅ 复杂功能开发（必须）
- 🟡 中等复杂度功能（推荐）
- ⚪ 简单UI调整（可跳过）

详见：[phase-125-interrogation.md](../workflow/phase-125-interrogation.md)

---

### unity-developer（开发工程师）

**职责**：
- 实现设计方案
- 编写代码
- 增量编译验证
- 遵守开发规范

**输入**：
- Phase 1的架构设计文档
- Phase 1.5的用户确认
- Phase 1.25的质询报告（如有）

**输出**：
- 实现代码
- 必要的注释
- 编译通过的项目

**使用方式**：
```
"请使用 unity-developer 代理实现这个功能"
```

**开发顺序**：
1. 数据模型（ScriptableObject、数据类）
2. 核心逻辑（Manager、Controller）
3. UI界面（MonoBehaviour、UI组件）

**每完成一个模块**：
- 立即刷新Unity并编译
- 检查编译错误和警告
- 修复后再继续下一个模块

---

### unity-reviewer（代码审查师）

**职责**：
- 检查代码规范
- 评估代码质量
- 识别安全隐患
- 评估性能影响

**输入**：
- Phase 2实现的代码

**输出**：
- 代码审查报告
- 问题清单（P0/P1/P2/P3）
- 改进建议
- 审查评分

**使用方式**：
```
"请使用 unity-reviewer 代理审查这些代码"
```

**审查维度**：
- 🚫 P0: 硬性约束（Instance大写、API验证等）
- 🔴 P1: 代码质量（访问修饰符、异常处理等）
- 🟡 P2: 项目架构（单例模式、Manager系统等）
- 🟢 P3: 性能优化（Update优化、内存管理等）

---

### unity-optimizer（性能优化师）

**职责**：
- 诊断性能瓶颈
- 提出优化方案
- 实施性能优化
- 验证优化效果

**输入**：
- 性能Profiler数据
- 性能问题描述
- 代码实现

**输出**：
- 性能分析报告
- 优化方案
- 优化后的代码
- 性能对比数据

**使用方式**：
```
"请使用 unity-optimizer 代理优化这段代码的性能"
```

**优化重点**：
- Update/FixedUpdate优化
- GC Alloc减少
- Draw Call优化
- 内存占用优化

---

### unity-debugger（调试工程师）

**职责**：
- 诊断Bug根因
- 提出修复方案
- 实施Bug修复
- 防止回归

**输入**：
- Bug描述
- 错误日志
- 复现步骤
- 相关代码

**输出**：
- 根因分析报告
- 修复方案
- 修复后的代码
- 回归测试用例

**使用方式**：
```
"请使用 unity-debugger 代理诊断这个Bug"
```

**诊断流程**：
1. 重现Bug
2. 分析错误日志
3. 定位问题代码
4. 分析根本原因
5. 提出修复方案
6. 实施修复
7. 验证修复效果

## Agent协作示例

### 示例1：开发新功能（完整流程）

```
Phase 0: 需求分析
  ↓ [主Agent]

Phase 1: 架构设计
  ↓ [unity-architect]
  输出：架构设计文档

Phase 1.25: 设计质询
  ↓ [design-interrogator]
  输出：质询报告

Phase 1.5: 效果确认
  ↓ [主Agent]
  获得用户确认

Phase 2: 功能开发
  ↓ [unity-developer]
  输出：实现代码

Phase 3: 代码审查
  ↓ [unity-reviewer]
  输出：审查报告

Phase 4: 测试验证
  ↓ [主Agent]
  输出：测试报告
```

### 示例2：修复Bug（专项任务）

```
Bug报告
  ↓ [unity-debugger]
  诊断根因

  ↓ 如果是简单修复
  直接修复并验证

  ↓ 如果是复杂修复
Phase 1: 设计修复方案
  ↓ [unity-architect]

Phase 1.25: 质询修复方案
  ↓ [design-interrogator]

Phase 2: 实施修复
  ↓ [unity-developer]

Phase 3: 代码审查
  ↓ [unity-reviewer]

Phase 4: 验证修复
  ↓ [主Agent]
```

### 示例3：性能优化（专项任务）

```
性能问题
  ↓ [unity-optimizer]
  分析性能瓶颈
  提出优化方案

  ↓ 如果涉及架构调整
Phase 1: 设计优化方案
  ↓ [unity-architect]

Phase 2: 实施优化
  ↓ [unity-developer]

Phase 3: 代码审查
  ↓ [unity-reviewer]

Phase 4: 验证优化效果
  ↓ [unity-optimizer]
```

## Agent调用最佳实践

### 1. 明确指定Agent

✅ **正确**：
```
"请使用 unity-architect 代理设计这个功能的架构"
```

❌ **错误**：
```
"帮我设计一下架构"  // 不明确，可能不会调用Agent
```

### 2. Agent之间传递信息

每个Agent的输出应该作为下一个Agent的输入：

```
unity-architect → 架构设计文档 → design-interrogator
design-interrogator → 质询报告 → 主Agent（Phase 1.5）
主Agent（Phase 1.5） → 用户确认 → unity-developer
unity-developer → 实现代码 → unity-reviewer
```

### 3. 不要跳过关键Agent

- ❌ 跳过 unity-architect 直接开发 → 架构混乱
- ❌ 跳过 unity-reviewer 直接提交 → 代码质量无保障
- ⚠️ 跳过 design-interrogator 要评估风险

### 4. 专项任务使用专项Agent

- 性能问题 → unity-optimizer
- Bug诊断 → unity-debugger
- 不要让 unity-developer 去优化性能（职责不符）

## 常见问题

### Q1: 是否必须使用Agent？

**简单任务**：不需要
- 单文件修改
- 简单参数调整
- UI微调

**复杂任务**：必须使用
- 新功能开发
- 架构设计
- 代码审查

### Q2: 可以同时使用多个Agent吗？

❌ 不推荐
- Agent是顺序工作的，不是并行的
- 每个Agent需要前一个Agent的输出

### Q3: Agent输出的内容需要保存吗？

✅ 是的
- 架构设计文档 → 保存到 `/Documents/`
- 质询报告 → 整合到Phase 1.5
- 审查报告 → 修复后归档

### Q4: Agent能否修改第三方库？

❌ 不能
- 所有Agent都遵守项目规范
- 禁止修改第三方库
- 如需修改，使用适配器模式
