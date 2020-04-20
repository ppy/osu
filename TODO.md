# 格式:
```
### 时间(yyyy/mm/dd)
* 描述 : 描述文本...
* 相关文件:
    *   [文件名1](相对路径1)
    *   [文件名2](相对路径2)
```

### 2020/03/28
* 描述 : ~~切换自定义ui选项时的相关逻辑仍需改进~~(已解决)
* 相关文件:
    *   [Toolbar.cs](osu.Game/Overlays/Toolbar/Toolbar.cs)

### 2020/04/13
* 描述 : 动画逻辑等仍需改进 → 需要想办法让`LogoTrackingContainer`在bg不可见时追踪他
* 相关文件:
    *   [PlayerLoader.cs](osu.Game/Screens/Play/PlayerLoader.cs)
    *   [BeatmapMetadataDisplay.cs](osu.Game/Screens/Play/BeatmapMetadataDisplay.cs)

### 2020/04/16
* 描述 : ~~需要让`UpdateBarEffects()`仅在`barHovered`为`false`,`mouseIdle`为`true`时隐藏HostOverlay~~(因设计方案有变, 该代办事项已不再有效)
* 相关文件:
    *   [MvisScreen.cs](osu.Game/Screens/MvisScreen.cs)

### 2020/04/17
* 描述 : ~~修复当`HideHostOverlay()`被`TryHideHostOverlay()`调用时,`ShowHostOverlay()`无效的问题~~(已有临时解决方案)
* 相关文件
    *   [MvisScreen.cs](osu.Game/Screens/MvisScreen.cs)

### 2020/04/20
* 描述 : 修复`MvisScreen`在Tests中的音频播放问题
* 相关文件
    *   [MvisScreen.cs](osu.Game/Screens/MvisScreen.cs)
    *   [TestSceneMvisScreen.cs](osu.Game.Tests/Visual/UserInterface/TestSceneMvisScreen.cs)

* 描述 : 添加`MvisScreen`的键位绑定支持
* 相关文件
    *   [MvisScreen.cs](osu.Game/Screens/MvisScreen.cs)