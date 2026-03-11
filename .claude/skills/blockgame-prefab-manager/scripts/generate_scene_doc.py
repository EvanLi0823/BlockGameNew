#!/usr/bin/env python3
"""
Unity Scene/Prefab Hierarchy Documentation Generator

Parses Unity scene files (.unity) and prefab files (.prefab) and generates
comprehensive documentation of GameObject hierarchies, component attachments,
and structure information.

Usage:
    python3 generate_scene_doc.py <unity_file_path> [output_file]

Examples:
    # Scene files
    python3 generate_scene_doc.py Assets/Scenes/Main.unity
    python3 generate_scene_doc.py Assets/Scenes/Main.unity custom_output.md

    # Prefab files
    python3 generate_scene_doc.py Assets/Prefabs/CommonRewardPopup.prefab
    python3 generate_scene_doc.py Assets/Prefabs/CommonRewardPopup.prefab custom_output.md
"""

import re
import sys
import os
from datetime import datetime
from pathlib import Path

def parse_scene_file(scene_path):
    """Parse Unity scene file and extract GameObject and Transform data."""
    with open(scene_path, 'r', encoding='utf-8') as f:
        content = f.read()

    # Extract GameObjects with components
    go_matches = re.findall(
        r'--- !u!1 &(\d+)\nGameObject:.*?m_Component:\n(.*?)m_Layer:',
        content, re.DOTALL
    )

    gameobjects = {}
    for go_id, components_section in go_matches:
        name_match = re.search(
            r'--- !u!1 &' + go_id + r'\nGameObject:.*?m_Name: (.*?)$',
            content, re.MULTILINE | re.DOTALL
        )
        name = name_match.group(1).strip() if name_match else "Unnamed"
        components = re.findall(r'component: \{fileID: (\d+)\}', components_section)
        gameobjects[go_id] = {'name': name, 'components': components}

    # Extract Transform hierarchy
    transform_matches = re.findall(
        r'--- !u!4 &(\d+)\nTransform:.*?m_Father: \{fileID: (\d+)\}.*?m_RootOrder: (\d+)',
        content, re.DOTALL
    )
    rect_matches = re.findall(
        r'--- !u!224 &(\d+)\nRectTransform:.*?m_Father: \{fileID: (\d+)\}.*?m_RootOrder: (\d+)',
        content, re.DOTALL
    )

    transforms = {}
    for trans_id, parent_id, root_order in transform_matches + rect_matches:
        transforms[trans_id] = {'parent': parent_id, 'order': int(root_order)}

    # Map GameObjects to Transforms
    go_to_transform = {}
    for go_id, go_data in gameobjects.items():
        for comp_id in go_data['components']:
            if comp_id in transforms:
                go_to_transform[go_id] = comp_id
                break

    # Find root nodes
    root_nodes = [
        (go_id, trans_id) for go_id, trans_id in go_to_transform.items()
        if transforms[trans_id]['parent'] == '0'
    ]
    root_nodes.sort(key=lambda x: transforms[x[1]]['order'])

    return gameobjects, transforms, go_to_transform, root_nodes, content

def get_component_types(go_id, gameobjects, content):
    """Extract component types for a GameObject."""
    component_types = []
    for comp_id in gameobjects[go_id]['components']:
        comp_match = re.search(
            r'--- !u!(\d+) &' + comp_id + r'\n(\w+):',
            content
        )
        if comp_match:
            comp_type = comp_match.group(2)
            if comp_type not in ['Transform', 'RectTransform']:
                component_types.append(comp_type)
    return component_types

def generate_hierarchy_markdown(go_id, trans_id, gameobjects, transforms,
                                 go_to_transform, content, level=0):
    """Generate markdown hierarchy for a GameObject and its children."""
    indent = "  " * level
    marker = "●" if level == 0 else "├─"
    go_name = gameobjects[go_id]['name']

    # Get component types
    component_types = get_component_types(go_id, gameobjects, content)

    # Format component list
    if component_types:
        if len(component_types) > 4:
            comp_str = f" `[{', '.join(component_types[:4])}... +{len(component_types)-4}]`"
        else:
            comp_str = f" `[{', '.join(component_types)}]`"
    else:
        comp_str = ""

    result = f"{indent}{marker} **{go_name}**{comp_str}\n"

    # Find and process children
    children = []
    for child_go_id, child_trans_id in go_to_transform.items():
        if (child_go_id != go_id and
            transforms[child_trans_id]['parent'] == trans_id):
            children.append((
                child_go_id,
                child_trans_id,
                transforms[child_trans_id]['order']
            ))

    children.sort(key=lambda x: x[2])

    for child_go_id, child_trans_id, _ in children:
        result += generate_hierarchy_markdown(
            child_go_id, child_trans_id, gameobjects, transforms,
            go_to_transform, content, level + 1
        )

    return result

def generate_documentation(file_path, output_path=None):
    """Generate complete scene/prefab documentation."""
    file_name = Path(file_path).stem
    file_ext = Path(file_path).suffix
    is_prefab = file_ext == '.prefab'

    # Parse file
    gameobjects, transforms, go_to_transform, root_nodes, content = parse_scene_file(file_path)

    # Generate markdown based on file type
    if is_prefab:
        doc_type = "Prefab"
        doc_title = f"{file_name} Prefab 结构文档"
        overview_text = f"{file_name} Prefab 包含以下主要组件。"
    else:
        doc_type = "场景"
        doc_title = f"{file_name.title()} 场景结构文档"
        overview_text = f"{file_name.title()} 场景包含以下主要系统组件。"

    # Generate markdown
    doc = f"""# {doc_title}

**生成时间**: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}
**文件路径**: `{file_path}`
**文件类型**: {doc_type}
**GameObject总数**: {len(gameobjects)}
**根节点数量**: {len(root_nodes)}

---

## {'Prefab' if is_prefab else '场景'}概览

{overview_text}

---

## 完整Hierarchy层级结构

"""

    # Generate hierarchy for all root nodes
    for i, (go_id, trans_id) in enumerate(root_nodes):
        doc += f"\n### 根节点 {i+1}: {gameobjects[go_id]['name']}\n\n```\n"
        doc += generate_hierarchy_markdown(
            go_id, trans_id, gameobjects, transforms,
            go_to_transform, content
        ).rstrip() + "\n```\n"

    # Add usage section
    doc += """

---

## 使用说明

### 如何查找GameObject

1. **使用Ctrl+F搜索GameObject名称**
2. **查看层级结构了解父子关系**
3. **查看组件列表了解功能**

### 常见查询

**查找UI面板**:
- 搜索 "Panel", "Canvas", "Menu"

**查找Manager**:
- 搜索 "Manager"

**查找Button**:
- 搜索 "Button", "Btn"

---

**注意**: 此文档由脚本自动生成，场景结构可能会随开发变化。建议定期更新此文档。
"""

    # Determine output path
    if output_path is None:
        prefix = "Prefab" if is_prefab else "Scene"
        output_path = f"Documents/{prefix}_{file_name}_Hierarchy.md"

    # Ensure output directory exists
    os.makedirs(os.path.dirname(output_path), exist_ok=True)

    # Write documentation
    with open(output_path, 'w', encoding='utf-8') as f:
        f.write(doc)

    print(f"✅ 场景文档已生成: {output_path}")
    print(f"📊 统计信息:")
    print(f"   - GameObject总数: {len(gameobjects)}")
    print(f"   - 根节点数量: {len(root_nodes)}")
    print(f"   - 文档大小: {len(doc)} 字符")

    return output_path

def main():
    """Main entry point."""
    if len(sys.argv) < 2:
        print("Usage: python3 generate_scene_doc.py <unity_file_path> [output_file]")
        print("\nExamples:")
        print("  # Scene files")
        print("  python3 generate_scene_doc.py Assets/Scenes/Main.unity")
        print("  python3 generate_scene_doc.py Assets/Scenes/Main.unity custom_output.md")
        print("\n  # Prefab files")
        print("  python3 generate_scene_doc.py Assets/Prefabs/CommonRewardPopup.prefab")
        print("  python3 generate_scene_doc.py Assets/Prefabs/CommonRewardPopup.prefab custom_output.md")
        sys.exit(1)

    file_path = sys.argv[1]
    output_path = sys.argv[2] if len(sys.argv) > 2 else None

    if not os.path.exists(file_path):
        print(f"❌ 错误: 文件不存在: {file_path}")
        sys.exit(1)

    file_ext = Path(file_path).suffix
    if file_ext not in ['.unity', '.prefab']:
        print(f"❌ 错误: 不支持的文件类型: {file_ext}")
        print("支持的文件类型: .unity (场景), .prefab (预制体)")
        sys.exit(1)

    generate_documentation(file_path, output_path)

if __name__ == "__main__":
    main()
