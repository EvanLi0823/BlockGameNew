---
name: blockgame-prefab-manager
description: Manages BlockGame Prefab lifecycle - parses design documents to generate standardized Prefab structures, maintains documentation of existing prefabs, detects changes and auto-updates, provides intelligent queries. Use when creating Prefabs from design docs, querying Prefab structure, validating naming conventions, or tracking Prefab changes.
version: 3.0.0
author: BlockGame Team
---

# BlockGame Prefab Manager

Complete Prefab lifecycle management from design to implementation.

## Purpose

Comprehensive Prefab management covering the entire workflow:
- **Design → Structure**: Parse design docs to generate standardized node structures
- **Query & Reference**: Quick access to existing Prefab documentation
- **Change Tracking**: Auto-detect and update when Prefabs change
- **Standards Validation**: Ensure naming conventions compliance

## When to Use

Activate this skill when:
- Creating new Prefabs from design documents
- Querying existing Prefab structures and components
- Checking if Prefabs follow naming conventions
- Tracking Prefab changes and keeping docs updated
- Planning modifications to existing Prefabs

## Core Features

### 1. Design Document Parser 🆕
Parse natural language design docs → standardized Prefab structure:
```bash
python3 scripts/design_parser.py Documents/GetRewardPopup.md
# Output: references/Prefab_GetRewardPopup_Hierarchy.md
```

**Applies naming conventions automatically**:
- "奖励文本" → `tmp_amount` (TextMeshProUGUI)
- "背景图" → `img_background` (Image)
- "领取按钮" → `btn_claim` (Button + Image)

### 2. Prefab Documentation 📚
28 Prefabs documented with 233 GameObjects:
- [Prefab_Index.md](references/Prefab_Index.md) - Complete index
- Individual Prefab hierarchies with component info

### 3. Change Detection 🔄 🆕
Monitor and auto-update when Prefabs change:
```bash
./scripts/batch_update.sh --check-diff
```

### 4. Intelligent Query 🔍 🆕
Multi-dimensional search capabilities:
```bash
./scripts/query.py --component Button    # By component
./scripts/query.py --node "btn_claim"    # By node name
```

## Quick Start

**From Design Doc**:
```bash
# 1. Parse design → standard structure
python3 scripts/design_parser.py designs/MyPopup.md

# 2. Create Prefab
cd ../blockgame-prefab-builder
./scripts/auto_create_prefab.sh ../blockgame-prefab-manager/references/Prefab_MyPopup_Hierarchy.md
```

**Query Existing**:
```bash
grep -r "btn_claim" references/
```

**Update After Changes**:
```bash
./scripts/batch_update.sh --auto-update
```

## Integration

Works seamlessly with **blockgame-prefab-builder**:
- Prefab Manager: Design → Standard Structure
- Prefab Builder: Standard Structure → Unity Prefab

## Detailed Guides

- **Usage & Queries**: [usage-guide.md](references/usage-guide.md)
- **Update & Maintenance**: [regeneration-guide.md](references/regeneration-guide.md)
- **Naming Standards**: [naming-standards.md](references/naming-standards.md) 🆕

---

**Note**: Version 3.0 - Now with design parsing, change detection, and validation.
