#!/bin/bash
#
# 自动化 Prefab 创建脚本
#
# 功能：
# 1. 从文档生成 C# 脚本
# 2. 通过 Unity 命令行自动执行创建
#
# 用法：
#   ./auto_create_prefab.sh <prefab_doc_path>
#

set -e  # 遇到错误立即退出

# 颜色定义
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# 打印函数
print_info() {
    echo -e "${BLUE}ℹ${NC} $1"
}

print_success() {
    echo -e "${GREEN}✅${NC} $1"
}

print_error() {
    echo -e "${RED}❌${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}⚠️${NC} $1"
}

# 检查参数
if [ $# -lt 1 ]; then
    print_error "Usage: $0 <prefab_doc_path>"
    echo ""
    echo "Example:"
    echo "  $0 ../blockgame-scene-reference/references/Prefab_CommonRewardPopup_Hierarchy.md"
    exit 1
fi

DOC_PATH="$1"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/../../.." && pwd)"

# 检查文档是否存在
if [ ! -f "$DOC_PATH" ]; then
    print_error "文档不存在: $DOC_PATH"
    exit 1
fi

# 提取 Prefab 名称
PREFAB_NAME=$(basename "$DOC_PATH" | sed 's/Prefab_//g' | sed 's/_Hierarchy.md//g')
print_info "Prefab 名称: $PREFAB_NAME"

# 步骤 1: 生成 C# 脚本
print_info "步骤 1/3: 生成 C# 脚本..."
OUTPUT_SCRIPT="Assets/Editor/Generated/Create${PREFAB_NAME}.cs"
cd "$PROJECT_ROOT"

python3 "$SCRIPT_DIR/generate_prefab.py" "$DOC_PATH" "$OUTPUT_SCRIPT"

if [ ! -f "$OUTPUT_SCRIPT" ]; then
    print_error "脚本生成失败"
    exit 1
fi

print_success "C# 脚本已生成: $OUTPUT_SCRIPT"

# 步骤 2: 查找 Unity 可执行文件
print_info "步骤 2/3: 查找 Unity..."

# 尝试多个可能的 Unity 路径
UNITY_PATHS=(
    "/Applications/Unity/Hub/Editor/2021.3.45f2/Unity.app/Contents/MacOS/Unity"
    "/Applications/Unity/Hub/Editor/*/Unity.app/Contents/MacOS/Unity"
    "/Applications/Unity/Unity.app/Contents/MacOS/Unity"
)

UNITY_EXEC=""
for path in "${UNITY_PATHS[@]}"; do
    # 处理通配符
    for expanded_path in $path; do
        if [ -f "$expanded_path" ]; then
            UNITY_EXEC="$expanded_path"
            break 2
        fi
    done
done

if [ -z "$UNITY_EXEC" ]; then
    print_warning "未找到 Unity 可执行文件"
    print_info "请手动在 Unity Editor 中执行:"
    print_info "  菜单: Tools → Create Prefab → $PREFAB_NAME"
    exit 0
fi

print_success "找到 Unity: $UNITY_EXEC"

# 步骤 3: 通过 Unity 命令行执行
print_info "步骤 3/3: 执行 Prefab 创建..."
print_warning "Unity 将在后台执行，请稍候..."

# 创建日志文件
LOG_FILE="/tmp/unity_prefab_builder_${PREFAB_NAME}.log"

# 执行 Unity 命令
"$UNITY_EXEC" \
    -quit \
    -batchmode \
    -projectPath "$PROJECT_ROOT" \
    -executeMethod "Create${PREFAB_NAME}.CreatePrefab" \
    -logFile "$LOG_FILE" \
    2>&1 &

UNITY_PID=$!

# 等待 Unity 完成
print_info "Unity PID: $UNITY_PID"
print_info "日志文件: $LOG_FILE"

# 监控日志
timeout=60  # 60秒超时
elapsed=0

while kill -0 $UNITY_PID 2>/dev/null; do
    sleep 1
    elapsed=$((elapsed + 1))

    if [ $elapsed -ge $timeout ]; then
        print_error "执行超时"
        kill $UNITY_PID 2>/dev/null || true
        exit 1
    fi

    # 显示进度
    if [ $((elapsed % 5)) -eq 0 ]; then
        print_info "已执行 ${elapsed}s..."
    fi
done

# 等待进程完全结束
wait $UNITY_PID
EXIT_CODE=$?

# 检查结果
if [ -f "$LOG_FILE" ]; then
    if grep -q "✅ Prefab created" "$LOG_FILE"; then
        print_success "Prefab 创建成功!"
        PREFAB_PATH=$(grep "✅ Prefab created:" "$LOG_FILE" | sed 's/.*: //')
        print_success "Prefab 路径: $PREFAB_PATH"
    else
        print_error "Prefab 创建失败"
        print_info "查看日志: $LOG_FILE"
        tail -20 "$LOG_FILE"
        exit 1
    fi
else
    print_warning "未找到日志文件"
fi

print_success "完成！"
