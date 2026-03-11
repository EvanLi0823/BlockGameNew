#!/usr/bin/env python3
"""
组件映射规则配置

定义 GameObject 命名规则到 Unity 组件的映射关系
"""

# 命名前缀到组件的映射规则
COMPONENT_MAPPING = {
    # UI 文本（优先级: Text > TextMeshPro）
    "txt_": {
        "components": ["Text"],
        "namespace": "UnityEngine.UI",
        "description": "Text 文本组件（传统 UI）"
    },
    "tmp_": {
        "components": ["TextMeshProUGUI"],
        "namespace": "TMPro",
        "description": "TextMeshPro 文本组件"
    },

    # UI 图片
    "img_": {
        "components": ["Image"],
        "namespace": "UnityEngine.UI",
        "description": "图片组件"
    },

    # UI 按钮
    "btn_": {
        "components": ["Image", "Button"],
        "namespace": "UnityEngine.UI",
        "description": "按钮组件",
        "child_text": True  # 自动创建文本子节点
    },

    # UI 面板
    "panel_": {
        "components": ["CanvasGroup", "Image"],
        "namespace": "UnityEngine.UI",
        "description": "面板容器"
    },

    # UI 滚动视图
    "scroll_": {
        "components": ["ScrollRect", "Image"],
        "namespace": "UnityEngine.UI",
        "description": "滚动视图"
    },

    # UI 开关
    "toggle_": {
        "components": ["Toggle", "Image"],
        "namespace": "UnityEngine.UI",
        "description": "开关组件"
    },

    # UI 滑动条
    "slider_": {
        "components": ["Slider"],
        "namespace": "UnityEngine.UI",
        "description": "滑动条组件"
    },

    # UI 输入框
    "input_": {
        "components": ["TMP_InputField"],
        "namespace": "TMPro",
        "description": "TMP输入框"
    },
}

# 特殊节点名称到组件的映射
SPECIAL_NODES = {
    "Content": {
        "components": [],  # 只需要 RectTransform（默认有）
        "description": "内容容器"
    },
    "Viewport": {
        "components": ["RectMask2D"],
        "namespace": "UnityEngine.UI",
        "description": "ScrollView 视口"
    },
    "Scrollbar Horizontal": {
        "components": ["Scrollbar"],
        "namespace": "UnityEngine.UI",
        "description": "水平滚动条"
    },
    "Scrollbar Vertical": {
        "components": ["Scrollbar"],
        "namespace": "UnityEngine.UI",
        "description": "垂直滚动条"
    },
}

# 根据文档中的组件列表识别
COMPONENT_FROM_DOC = {
    "Animator": {
        "type": "Animator",
        "namespace": "UnityEngine"
    },
    "CanvasGroup": {
        "type": "CanvasGroup",
        "namespace": "UnityEngine"
    },
    "Canvas": {
        "type": "Canvas",
        "namespace": "UnityEngine"
    },
    "CanvasScaler": {
        "type": "CanvasScaler",
        "namespace": "UnityEngine.UI"
    },
    "GraphicRaycaster": {
        "type": "GraphicRaycaster",
        "namespace": "UnityEngine.UI"
    },
}

def get_components_for_node(node_name, doc_components=None):
    """
    根据节点名称和文档中的组件列表，推断需要添加的组件

    Args:
        node_name: GameObject 名称
        doc_components: 文档中列出的组件类型列表

    Returns:
        dict: 组件信息
        {
            "components": ["Component1", "Component2"],
            "namespaces": ["Namespace1", "Namespace2"],
            "description": "说明"
        }
    """
    result = {
        "components": [],
        "namespaces": set(),
        "description": "",
        "child_text": False
    }

    # 1. 检查特殊节点
    if node_name in SPECIAL_NODES:
        config = SPECIAL_NODES[node_name]
        result["components"] = config["components"]
        result["description"] = config["description"]
        if "namespace" in config:
            result["namespaces"].add(config["namespace"])
        return result

    # 2. 检查命名前缀
    for prefix, config in COMPONENT_MAPPING.items():
        if node_name.startswith(prefix):
            result["components"] = config["components"]
            result["description"] = config["description"]
            if "namespace" in config:
                result["namespaces"].add(config["namespace"])
            if config.get("child_text", False):
                result["child_text"] = True
            return result

    # 3. 如果有文档组件信息，使用文档信息
    if doc_components:
        for comp in doc_components:
            if comp in COMPONENT_FROM_DOC:
                comp_info = COMPONENT_FROM_DOC[comp]
                result["components"].append(comp_info["type"])
                result["namespaces"].add(comp_info["namespace"])

    # 4. 默认情况（只有 RectTransform，Unity UI 默认添加）
    if not result["components"]:
        result["description"] = "基础 GameObject"

    return result


def get_required_namespaces(components):
    """
    获取组件需要的命名空间

    Args:
        components: 组件列表

    Returns:
        set: 命名空间集合
    """
    namespaces = {"UnityEngine", "UnityEditor"}

    for comp in components:
        if comp in ["TextMeshProUGUI", "TMP_InputField"]:
            namespaces.add("TMPro")
        elif comp in ["Image", "Button", "Toggle", "Slider", "ScrollRect",
                      "CanvasGroup", "CanvasScaler", "GraphicRaycaster", "RectMask2D", "Scrollbar"]:
            namespaces.add("UnityEngine.UI")

    return namespaces
