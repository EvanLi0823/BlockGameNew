# Main场景结构文档

**生成时间**: 2026-03-11 12:18:59
**场景路径**: `Assets/BlockPuzzleGameToolkit/Scenes/main.unity`
**GameObject总数**: 83
**根节点数量**: 13

---

## 场景概览

Main场景是BlockGame项目的主要游戏场景，包含以下主要系统：

- **GamePlay系统** - 游戏玩法核心逻辑
- **Loading界面** - 加载进度显示
- **Main菜单** - 主菜单UI
- **Map系统** - 地图和关卡选择
- **Canvas系统** - UI画布和面板

---

## 完整Hierarchy层级结构


### 根节点 1: ---GamePlay---

```
● **---GamePlay---**
```

### 根节点 2: --Loading--

```
● **--Loading--** `[Canvas, MonoBehaviour, MonoBehaviour, MonoBehaviour]`
  ├─ **BackGround** `[CanvasRenderer, MonoBehaviour, Canvas]`
  ├─ **Slider** `[CanvasRenderer, MonoBehaviour]`
    ├─ **Fill** `[CanvasRenderer, MonoBehaviour]`
  ├─ **ProgressText** `[CanvasRenderer, MonoBehaviour]`
```

### 根节点 3: ---Main-------

```
● **---Main-------** `[Canvas, MonoBehaviour, MonoBehaviour]`
  ├─ **MainMenu** `[MonoBehaviour, Animator, CanvasGroup, PlayableDirector... +2]`
    ├─ **Buttons**
      ├─ **Game Modes Buttons** `[Animator, MonoBehaviour]`
      ├─ **spin** `[CanvasRenderer, MonoBehaviour, Animator, CanvasGroup]`
        ├─ **lucky-spin-icon-part4** `[CanvasRenderer, MonoBehaviour]`
        ├─ **lucky-spin-icon-part2** `[CanvasRenderer, MonoBehaviour]`
        ├─ **lucky-spin-icon-part3** `[CanvasRenderer, MonoBehaviour]`
        ├─ **lucky-spin-icon-part1** `[CanvasRenderer, MonoBehaviour]`
```

### 根节点 4: ---Map--------

```
● **---Map--------** `[Canvas, MonoBehaviour, MonoBehaviour, MonoBehaviour]`
  ├─ **TiledMap**
    ├─ **Background** `[CanvasRenderer, MonoBehaviour]`
    ├─ **ArcadeMode** `[CanvasRenderer, MonoBehaviour]`
      ├─ **crown-icon** `[CanvasRenderer, MonoBehaviour]`
    ├─ **STAGE** `[CanvasRenderer, MonoBehaviour]`
    ├─ **map-field** `[CanvasRenderer, MonoBehaviour, MonoBehaviour]`
  ├─ **ScrollableMap** `[MonoBehaviour, MonoBehaviour, MonoBehaviour]`
    ├─ **ScrollRect** `[CanvasRenderer, MonoBehaviour]`
      ├─ **Viewport** `[CanvasRenderer]`
        ├─ **Content** `[MonoBehaviour, MonoBehaviour]`
          ├─ **map_background** `[CanvasRenderer, MonoBehaviour, MonoBehaviour]`
          ├─ **footer_background** `[CanvasRenderer, MonoBehaviour, MonoBehaviour]`
          ├─ **Levels** `[Canvas, MonoBehaviour, MonoBehaviour]`
            ├─ **item-2** `[CanvasRenderer, MonoBehaviour]`
            ├─ **item-3** `[CanvasRenderer, MonoBehaviour]`
            ├─ **item-5** `[CanvasRenderer, MonoBehaviour]`
            ├─ **item-4** `[CanvasRenderer, MonoBehaviour]`
    ├─ **Buttons** `[Canvas, MonoBehaviour]`
```

### 根节点 5: CanvasBack

```
● **CanvasBack** `[Canvas, MonoBehaviour, MonoBehaviour]`
  ├─ **main-background** `[CanvasRenderer, MonoBehaviour, MonoBehaviour]`
```

### 根节点 6: EventSystem

```
● **EventSystem** `[MonoBehaviour, MonoBehaviour]`
```

### 根节点 7: FXCanvas

```
● **FXCanvas** `[Canvas, MonoBehaviour, MonoBehaviour, MonoBehaviour]`
  ├─ **FXPool**
    ├─ **BonusSparkle** `[MonoBehaviour]`
    ├─ **Coins** `[MonoBehaviour]`
    ├─ **PopupText** `[MonoBehaviour]`
```

### 根节点 8: GameCanvas

```
● **GameCanvas** `[Canvas, MonoBehaviour, MonoBehaviour, MonoBehaviour... +8]`
  ├─ **SafeArea**
    ├─ **Field** `[CanvasRenderer, MonoBehaviour, MonoBehaviour, MonoBehaviour]`
      ├─ **Timer** `[CanvasRenderer, MonoBehaviour]`
        ├─ **timer-icon** `[CanvasRenderer, MonoBehaviour]`
        ├─ **Time** `[CanvasRenderer, MonoBehaviour]`
      ├─ **gamefield-border** `[CanvasRenderer, MonoBehaviour, MonoBehaviour]`
      ├─ **outline** `[CanvasRenderer, MonoBehaviour, MonoBehaviour]`
        ├─ **outline_1** `[CanvasRenderer, MonoBehaviour]`
    ├─ **TopPanel** `[MonoBehaviour, MonoBehaviour, CanvasRenderer, MonoBehaviour]`
      ├─ **LevelPanel** `[CanvasRenderer]`
        ├─ **img_bg** `[CanvasRenderer, MonoBehaviour]`
        ├─ **img_levelicon** `[CanvasRenderer, MonoBehaviour]`
        ├─ **tmp_cashlevel** `[CanvasRenderer, MonoBehaviour]`
      ├─ **CashPanel** `[CanvasRenderer]`
        ├─ **img_bg** `[CanvasRenderer, MonoBehaviour]`
        ├─ **img_cashicon** `[CanvasRenderer, MonoBehaviour]`
        ├─ **tmp_cashtext** `[CanvasRenderer, MonoBehaviour]`
        ├─ **tmp_addtext** `[CanvasRenderer, MonoBehaviour, MonoBehaviour]`
      ├─ **WithDrawBtn** `[CanvasRenderer, MonoBehaviour, Animator, MonoBehaviour]`
        ├─ **Text (TMP)** `[CanvasRenderer, MonoBehaviour, MonoBehaviour]`
    ├─ **MiddlePanel**
      ├─ **PropPanel** `[MonoBehaviour, MonoBehaviour, CanvasGroup]`
    ├─ **BottomPanel**
  ├─ **Pool**
    ├─ **Outline** `[MonoBehaviour]`
    ├─ **LineExplosionPool**
  ├─ **TutorialManager** `[MonoBehaviour]`
    ├─ **hand_icon** `[CanvasRenderer, MonoBehaviour, Canvas]`
```

### 根节点 9: ItemsCanvas

```
● **ItemsCanvas** `[Canvas, MonoBehaviour, MonoBehaviour]`
  ├─ **Pool**
    ├─ **Shapes** `[MonoBehaviour]`
  ├─ **ItemDeck** `[MonoBehaviour, CanvasRenderer, MonoBehaviour, MonoBehaviour... +1]`
    ├─ **Cell** `[MonoBehaviour]`
    ├─ **Cell (1)** `[MonoBehaviour]`
    ├─ **Cell (2)** `[MonoBehaviour]`
```

### 根节点 10: ActivityCanvas

```
● **ActivityCanvas** `[Canvas, MonoBehaviour, MonoBehaviour]`
```

### 根节点 11: Main Camera

```
● **Main Camera** `[Camera, AudioListener]`
```

### 根节点 12: OrientationManager

```
● **OrientationManager** `[MonoBehaviour]`
```

### 根节点 13: RewardCanvas

```
● **RewardCanvas** `[Canvas, MonoBehaviour, MonoBehaviour, MonoBehaviour]`
```


---

## 关键GameObject说明

### GamePlay系统
- **---GamePlay---**: 游戏玩法根节点，包含游戏核心逻辑

### UI系统

#### Main菜单
- **---Main-------**: 主菜单Canvas
  - **MainMenu**: 主菜单面板
    - **Buttons**: 按钮容器
    - **spin**: 幸运转盘图标

#### Map系统
- **---Map--------**: 地图Canvas
  - **TiledMap**: 瓦片地图容器
    - **Background**: 背景图
    - **ArcadeMode**: 街机模式标识
    - **STAGE**: 关卡标识
    - **map-field**: 地图场地
  - **ScrollableMap**: 可滚动地图系统
    - **ScrollRect**: 滚动视图
    - **Levels**: 关卡列表

#### Canvas系统
- **CanvasBack**: 背景Canvas
- **RewardCanvas**: 奖励Canvas
- **ActivityCanvas**: 活动Canvas

### 管理器
- **TutorialManager**: 教程管理器
- **OrientationManager**: 屏幕方向管理器
- **LineExplosionPool**: 消除特效对象池

### 相机和事件
- **Main Camera**: 主相机
- **EventSystem**: UI事件系统

---

## 组件类型统计

常见组件类型：
- **Canvas**: UI画布组件
- **CanvasRenderer**: UI渲染组件
- **MonoBehaviour**: 自定义脚本组件
- **Animator**: 动画控制器
- **CanvasGroup**: UI组控制

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
