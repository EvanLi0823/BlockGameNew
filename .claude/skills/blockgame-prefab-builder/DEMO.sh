#!/bin/bash
#
# Blockgame Prefab Builder - 演示脚本
#
# 快速演示技能的核心功能
#

set -e

# 颜色定义
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${BLUE}╔═══════════════════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║  Blockgame Prefab Builder - 技能演示                 ║${NC}"
echo -e "${BLUE}╚═══════════════════════════════════════════════════════╝${NC}"
echo ""

# 演示 1: 文档解析
echo -e "${GREEN}━━━ 演示 1: 文档解析器 ━━━${NC}"
echo ""
echo "解析 CommonRewardPopup Prefab 文档..."
echo ""

python3 scripts/parse_prefab_doc.py \
    ../blockgame-scene-reference/references/Prefab_CommonRewardPopup_Hierarchy.md \
    2>&1 | head -20

echo ""
echo -e "${YELLOW}按 Enter 继续...${NC}"
read

# 演示 2: C# 脚本生成
echo ""
echo -e "${GREEN}━━━ 演示 2: C# 脚本生成 ━━━${NC}"
echo ""
echo "生成 CommonRewardPopup 创建脚本..."
echo ""

python3 scripts/generate_prefab.py \
    ../blockgame-scene-reference/references/Prefab_CommonRewardPopup_Hierarchy.md \
    /tmp/demo_create_popup.cs

echo ""
echo "查看生成的代码（前30行）:"
echo ""
head -30 /tmp/demo_create_popup.cs

echo ""
echo -e "${YELLOW}按 Enter 继续...${NC}"
read

# 演示 3: 组件推断
echo ""
echo -e "${GREEN}━━━ 演示 3: 组件推断规则 ━━━${NC}"
echo ""
echo "展示命名规则如何推断组件类型:"
echo ""

python3 -c "
import sys
sys.path.insert(0, 'scripts')
from component_rules import get_components_for_node

examples = [
    'btn_claim',
    'img_reward',
    'tmp_title',
    'panel_main',
    'Content',
    'scroll_list'
]

print('┌─────────────────┬────────────────────────────────┐')
print('│ 节点名称        │ 推断的组件                     │')
print('├─────────────────┼────────────────────────────────┤')

for name in examples:
    info = get_components_for_node(name)
    components = ', '.join(info['components']) if info['components'] else 'RectTransform'
    print(f'│ {name:<15} │ {components:<30} │')

print('└─────────────────┴────────────────────────────────┘')
"

echo ""
echo -e "${YELLOW}按 Enter 继续...${NC}"
read

# 演示 4: 查看示例文档
echo ""
echo -e "${GREEN}━━━ 演示 4: 示例文档 ━━━${NC}"
echo ""
echo "查看简单弹窗示例文档:"
echo ""

head -30 examples/SimpleRewardPopup.md

echo ""
echo "..."
echo ""

# 演示 5: 技能信息
echo -e "${GREEN}━━━ 演示 5: 技能信息 ━━━${NC}"
echo ""
echo "技能名称: blockgame-prefab-builder"
echo "版本: 1.0.0"
echo ""
echo "文件统计:"
echo "  - SKILL.md: 98行"
echo "  - README.md: 411行"
echo "  - 核心脚本: 849行（Python + Shell）"
echo "  - 参考文档: 472行"
echo "  - 总计: 1830行"
echo ""
echo "支持的命名规则: 9种"
echo "  txt_, tmp_, img_, btn_, panel_, scroll_, toggle_, slider_, input_"
echo ""
echo "特殊节点: 3种"
echo "  Content, Viewport, Scrollbar"
echo ""

# 完成
echo -e "${GREEN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo ""
echo -e "${GREEN}✅ 演示完成！${NC}"
echo ""
echo "下一步:"
echo "  1. 查看 QUICKSTART.md 快速上手"
echo "  2. 查看 README.md 了解详细用法"
echo "  3. 尝试生成您的第一个 Prefab:"
echo ""
echo "     ./scripts/auto_create_prefab.sh \\"
echo "         ../blockgame-scene-reference/references/Prefab_CommonRewardPopup_Hierarchy.md"
echo ""
