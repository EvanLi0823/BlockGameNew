#!/usr/bin/env python3
"""
Prefab 文档解析器

解析 blockgame-scene-reference 生成的 Prefab 文档，
提取 GameObject 层级结构和组件信息
"""

import re
from pathlib import Path
from typing import List, Dict, Optional


class GameObject:
    """GameObject 节点"""

    def __init__(self, name: str, components: List[str] = None, level: int = 0):
        self.name = name
        self.components = components or []
        self.level = level  # 缩进层级
        self.children = []
        self.parent = None

    def add_child(self, child):
        """添加子节点"""
        child.parent = self
        self.children.append(child)

    def __repr__(self):
        return f"GameObject(name='{self.name}', level={self.level}, children={len(self.children)})"


class PrefabHierarchy:
    """Prefab 层级结构"""

    def __init__(self, prefab_name: str):
        self.prefab_name = prefab_name
        self.root = None
        self.all_nodes = []

    def __repr__(self):
        return f"PrefabHierarchy(name='{self.prefab_name}', nodes={len(self.all_nodes)})"


def parse_hierarchy_line(line: str) -> Optional[tuple]:
    """
    解析层级结构的一行

    格式示例：
    ● **CommonRewardPopup** `[Animator, CanvasGroup, CanvasRenderer, MonoBehaviour]`
      ├─ **Content** `[Animator, CanvasRenderer]`
        ├─ **img_guang** `[CanvasRenderer, MonoBehaviour]`

    Returns:
        (level, name, components) 或 None
    """
    # 匹配层级结构的行
    # ● 表示根节点（level 0）
    # ├─ 表示子节点，前面的空格数量决定层级
    pattern = r'^(\s*)(●|├─|└─)\s*\*\*([^*]+)\*\*(?:\s*`\[([^\]]+)\]`)?'
    match = re.match(pattern, line)

    if not match:
        return None

    spaces = match.group(1)
    marker = match.group(2)
    name = match.group(3).strip()
    components_str = match.group(4)

    # 计算层级（每2个空格为1级）
    if marker == "●":
        level = 0
    else:
        level = len(spaces) // 2

    # 解析组件列表
    components = []
    if components_str:
        components = [c.strip() for c in components_str.split(',')]

    return (level, name, components)


def build_hierarchy_from_lines(lines: List[str], prefab_name: str) -> PrefabHierarchy:
    """
    从文档行构建层级结构

    Args:
        lines: 文档行列表
        prefab_name: Prefab 名称

    Returns:
        PrefabHierarchy 对象
    """
    hierarchy = PrefabHierarchy(prefab_name)
    node_stack = []  # 用于跟踪当前路径的节点栈

    for line in lines:
        parsed = parse_hierarchy_line(line)
        if not parsed:
            continue

        level, name, components = parsed

        # 创建节点
        node = GameObject(name, components, level)
        hierarchy.all_nodes.append(node)

        # 建立父子关系
        if level == 0:
            # 根节点
            hierarchy.root = node
            node_stack = [node]
        else:
            # 找到父节点（层级比当前节点小1）
            while len(node_stack) > level:
                node_stack.pop()

            if node_stack:
                parent = node_stack[-1]
                parent.add_child(node)

            node_stack.append(node)

    return hierarchy


def parse_prefab_document(doc_path: str) -> PrefabHierarchy:
    """
    解析 Prefab 文档

    Args:
        doc_path: 文档路径

    Returns:
        PrefabHierarchy 对象
    """
    doc_path = Path(doc_path)

    if not doc_path.exists():
        raise FileNotFoundError(f"文档不存在: {doc_path}")

    # 从文件名提取 Prefab 名称
    # 例如: Prefab_CommonRewardPopup_Hierarchy.md -> CommonRewardPopup
    prefab_name = doc_path.stem.replace('Prefab_', '').replace('_Hierarchy', '')

    with open(doc_path, 'r', encoding='utf-8') as f:
        content = f.read()

    # 查找层级结构部分
    # 寻找包含层级结构的代码块
    hierarchy_section = None
    in_code_block = False
    hierarchy_lines = []

    for line in content.split('\n'):
        # 检测代码块开始
        if line.strip().startswith('```') and not in_code_block:
            in_code_block = True
            hierarchy_lines = []
            continue

        # 检测代码块结束
        if line.strip().startswith('```') and in_code_block:
            in_code_block = False
            # 检查这个代码块是否包含层级结构
            if any('●' in l or '├─' in l for l in hierarchy_lines):
                hierarchy_section = hierarchy_lines
                break
            continue

        # 收集代码块内的行
        if in_code_block:
            hierarchy_lines.append(line)

    if not hierarchy_section:
        raise ValueError(f"未找到层级结构: {doc_path}")

    # 构建层级结构
    hierarchy = build_hierarchy_from_lines(hierarchy_section, prefab_name)

    if not hierarchy.root:
        raise ValueError(f"未找到根节点: {doc_path}")

    return hierarchy


def print_hierarchy(node: GameObject, indent: int = 0):
    """打印层级结构（用于调试）"""
    prefix = "  " * indent
    marker = "●" if indent == 0 else "├─"
    components_str = f" [{', '.join(node.components)}]" if node.components else ""
    print(f"{prefix}{marker} {node.name}{components_str}")

    for child in node.children:
        print_hierarchy(child, indent + 1)


def main():
    """测试解析器"""
    import sys

    if len(sys.argv) < 2:
        print("Usage: python3 parse_prefab_doc.py <prefab_doc_path>")
        print("\nExample:")
        print("  python3 parse_prefab_doc.py ../references/Prefab_CommonRewardPopup_Hierarchy.md")
        sys.exit(1)

    doc_path = sys.argv[1]

    try:
        hierarchy = parse_prefab_document(doc_path)
        print(f"✅ 解析成功: {hierarchy.prefab_name}")
        print(f"📊 节点总数: {len(hierarchy.all_nodes)}")
        print(f"\n层级结构:")
        print_hierarchy(hierarchy.root)

    except Exception as e:
        print(f"❌ 解析失败: {e}")
        import traceback
        traceback.print_exc()
        sys.exit(1)


if __name__ == "__main__":
    main()
