# Phase 0: 需求分析

## 阶段目标

理解需求边界、分析现有代码结构、创建任务清单

## 必须完成的任务

### 1. 理解需求边界

**问题清单**：
- 用户想要什么功能？
- 功能的使用场景是什么？
- 是否有类似的现有功能？
- 功能的范围是什么（不包括什么）？
- 是否有特定的技术约束？
- 是否有性能要求？

### 2. 分析现有代码结构

**必须分析**：
```bash
# 查找相关Manager
grep -r "class.*Manager" Assets/BlockPuzzleGameToolkit/Scripts --include="*.cs"

# 查找相关配置
ls Assets/BlockPuzzleGameToolkit/Resources/Settings/

# 查找类似功能实现
grep -r "SimilarFeature" Assets/BlockPuzzleGameToolkit/Scripts --include="*.cs"
```

**分析内容**：
- 现有架构是否支持新功能？
- 需要修改哪些现有模块？
- 有哪些可以复用的代码？
- 有哪些依赖关系？

### 3. 创建任务清单

使用 **TodoWrite** 工具创建完整的任务清单：

```markdown
## 任务：[功能名称]

### Phase 0: 需求分析 ✅
- [x] 理解需求边界
- [x] 分析现有代码结构
- [x] 创建任务清单

### Phase 1: 架构设计
- [ ] 设计数据结构
- [ ] 设计接口和调用流程
- [ ] 绘制架构图
...
```

## 输出物

- ✅ 需求理解确认
- ✅ 代码结构分析报告
- ✅ TodoWrite任务清单

## 常见错误

❌ **跳过需求分析，直接开始设计**
- 后果：方向错误，需要大量返工

❌ **不分析现有代码结构**
- 后果：破坏现有架构，引入技术债

❌ **不创建任务清单**
- 后果：进度不可控，容易遗漏任务

## 下一步

完成Phase 0后，进入 **Phase 1: 架构设计**
