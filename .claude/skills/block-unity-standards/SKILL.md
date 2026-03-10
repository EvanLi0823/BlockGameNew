---
name: block-unity-standards
description: Enforces BlockGame project Unity development standards including mandatory constraints (batch modification, API verification, method signature changes, using statements), code quality rules (singleton pattern, access modifiers, exception handling), project architecture (SingletonBehaviour, Manager system, ScriptableObject), and performance optimization. Triggers when writing, reviewing, or refactoring Unity C# code, implementing features, fixing bugs, or optimizing performance.
version: 1.0.0
author: BlockGame Team
---

# Block Unity Development Standards

此技能为BlockGame项目制定的Unity开发标准，确保代码质量、可维护性和团队协作的一致性。

## Skill Purpose

**优先级分层：**
1. **P0: 硬性约束** (违反将导致编译错误)
2. **P1: 代码质量** (确保代码健壮性)
3. **P2: 项目架构** (遵循现有架构)
4. **P3: 性能优化**
5. **P4: 代码审查**

## When This Skill Triggers

编写/重构Unity C#代码、实现新功能、修复Bug、代码审查、批量修改代码、修改方法签名

## Quick Reference Guide

| Priority | Task | Reference |
|----------|------|-----------|
| **🚫 P0: Mandatory Constraints (MUST ENFORCE)** | | |
| 0 | 批量修改规范、防止误改第三方库 | [batch-modification.md](references/constraints/batch-modification.md) ⭐⭐⭐ |
| 0 | API调用验证、防止编译错误 | [api-verification.md](references/constraints/api-verification.md) ⭐⭐⭐ |
| 0 | 方法签名修改、防止影响范围遗漏 | [method-signature.md](references/constraints/method-signature.md) ⭐⭐⭐ |
| 0 | Using语句管理、防止类型未定义 | [using-statements.md](references/constraints/using-statements.md) ⭐⭐⭐ |
| 0 | 代码块修改、防止括号错误 | [code-blocks.md](references/constraints/code-blocks.md) ⭐⭐⭐ |
| 0 | 命名空间规范、防止系统冲突 | [namespaces.md](references/constraints/namespaces.md) ⭐⭐⭐ |
| **🔴 P1: Code Quality** | | |
| 1 | 单例规范、访问修饰符、异常处理 | [code-quality.md](references/csharp/code-quality.md) ⭐ |
| 1 | 命名规范、注释规范、代码组织 | [code-style.md](references/csharp/code-style.md) ⭐ |
| **🟡 P2: Project Architecture** | | |
| 2 | SingletonBehaviour使用规范 | [singleton-pattern.md](references/unity/singleton-pattern.md) |
| 2 | Manager系统设计、事件系统 | [manager-architecture.md](references/unity/manager-architecture.md) |
| 2 | ScriptableObject配置系统 | [settings-system.md](references/unity/settings-system.md) |
| **🟢 P3: Performance** | | |
| 3 | Unity性能最佳实践 | [unity-performance.md](references/performance/unity-performance.md) |
| 3 | 内存管理、对象池 | [memory-management.md](references/performance/memory-management.md) |
| **🔵 P4: Code Review** | | |
| 4 | 代码审查清单、常见问题 | [code-review.md](references/review/code-review.md) |

## 🚫 P0 Quick Checklist

**批量修改前：**
- [ ] 只修改 `/Scripts` 目录
- [ ] 使用排除模式保护第三方库
- [ ] 精确匹配替换

**API调用前：**
```bash
grep "public" TargetClass.cs | grep -v "class"
grep "namespace" TargetFile.cs
```

**修改方法签名：**
```bash
grep -r "override.*MethodName" Scripts --include="*.cs"
grep -r "\.MethodName\(" Scripts --include="*.cs"
```

**新增类型引用：**
```bash
grep -r "class TypeName" Scripts --include="*.cs"
grep "namespace" TargetFile.cs
```

详见 [P0硬性约束文档](references/constraints/)

## 🔴 P1 Code Quality Rules

1. **Instance（大写）** - 所有单例
2. **明确访问修饰符** - private默认
3. **零警告** - 修复所有编译器警告
4. **异常处理** - try-catch外部调用，throw内部错误
5. **readonly/const** - 标记不可变
6. **nameof** - 避免硬编码字符串
7. **中文注释** - 核心逻辑说明

详见 [代码质量规范](references/csharp/code-quality.md)

## Common Mistakes vs Best Practices

| ❌ Don't | ✅ Do |
|---------|------|
| `instance`（小写） | `Instance`（大写） |
| 省略访问修饰符 | 明确`private`/`public` |
| 忽略警告 | 修复所有警告 |
| 硬编码字符串 | 使用`nameof()` |
| Awake访问单例 | Start访问单例 |
| 忘记取消订阅 | OnDestroy取消订阅 |

## Pre-Submission Checklist

**P0 Constraints:**
- [ ] 批量修改仅限Scripts目录
- [ ] API调用已grep验证
- [ ] 方法签名修改全覆盖
- [ ] Using语句完整

**P1 Quality:**
- [ ] Instance（大写）
- [ ] 访问修饰符明确
- [ ] 无警告
- [ ] readonly/const正确

**P2 Architecture:**
- [ ] Manager继承SingletonBehaviour
- [ ] 事件正确订阅/取消
- [ ] Settings继承SettingsBase

**P3 Performance:**
- [ ] Update无内存分配
- [ ] 组件引用已缓存
