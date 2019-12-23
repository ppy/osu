# Changelog
## 2019/12/21
### 其他
* 添加Changelog.md
### 游戏界面
* 补全了以下界面的翻译
    *   多人游戏大厅标题
    *   多人联机房间列表的持续时间
    *   osu!direct界面Overlay
    *   玩家信息界面的*好友*按钮
    *   设置界面的*调试*选项(**需要进一步验证是否准确**)
    *   多处的"View Profile"和从来没见过的"Start Chat"(**需要进一步验证是否完成**)
    *   选歌界面分数获取失败时的"Couldn't retrieve scores!"
* 优化了一下界面的翻译
    *   游戏Mod菜单 > 分数倍率
        *   `1.00x` 改为 `1.00倍`

## 2019/12/22
### **重要 : git相关**
* **分支由master变更至lang_zhCN**
### 其他
* 补全了以下部分的翻译
    *   解析谱面时取值过低/过高等问题的报错(Value is too low/high | Not a number)
### 游戏界面
* UI修改
    *   [音量调节](osu.Game/Overlays/Volume/VolumeMeter.cs)
        *   当某一项音量为`最小`时,显示"静音",字体大小30
        *   当某一项音量为`最大`时,显示"超大声",字体大小30
        *   其他时候字体大小为24
        *   增大了"音效","总体","音乐"的文字大小(? -> 20)
    *   [游戏间歇](osu.Game/Screens/Play/Break/BreakInfo.cs)
        *   增大了文字大小(? -> 20)
    *   [工具栏>个人信息按钮](osu.Game/Overlays/Toolbar/ToolbarUserButton.cs)
        *   进行了一点本地化修改
    *   [更改日志界面](osu.Game/Overlays/Changelog/Comments.cs)
        *   添加了前往[osulazer项目地址](https://github.com/ppy/osu)和[mf-osu项目地址](https://github.com/ppy/osu)的链接
    *   [界面](osu.Game/)
        *   将多个界面中的字体调大
* 翻译改进
    *   改进了`主菜单`中的`单人`和`多人`为`单人游戏`和`多人游戏`
* 翻译补全
    *   补全了`/me`,`/help`,`/join` 聊天指令的翻译

* Note:
    *   所有英文转中文的文字一般字号+4以避免发虚等问题
    *   osu默认字号:16
        *   特殊
            *  [玩家信息界面:186](./osu.Game/Overlays/Profile/Header/TopHeaderContainer.cs):15->17

## 2019/12/23
* 翻译补全
    *   补全了"音乐播放"快捷键按下时的翻译
    *   补全了"鼠标按键"快捷键按下时的翻译
    *   多人游戏创建房间时的标题
* 字体增大
    *   [弹出菜单](osu.Game/Overlays/OSD/Toast.cs)
        *   14,24,12 -> 18,28,16
    *   [主界面菜单按钮](osu.Game/Screens/Menu/Button.cs)
        *   (第128行)字体增大至20
    *   [选歌界面随机按钮](osu.Game/Screens/Select/FooterButtonRandom.cs)
        *   (行27)字体调整为18
    *   [选歌界面下方按钮](osu.Game/Screens/Select/FooterButton.cs)
        *   (行91)字体调整为18
    *   [多人游戏大厅](osu.Game/Screens/Multi/Components/RoomStatusInfo.cs)
        *   (行37)房间信息字体调整为17
        *   [参与人数](osu.Game/Screens/Multi/Lounge/Components/ParticipantInfo.cs)字体调整为17(貌似没有任何效果)
* UI修改
    *   [工具栏个人信息按钮](osu.Game/Overlays/Toolbar/ToolbarUserButton.cs)
        *   文字调整为"别来无恙,<玩家名>!"(`Text = {api.LocalUser.Value.Username};`->`Text = $"别来无恙, {api.LocalUser.Value.Username} !";`)
        *   字体调整为18
    *   ["功能尚未准备"弹出菜单](osu.Game/Screens/ScreenWhiteBox.cs)
        *   (行168)第二行字大小调整为24
        *   (行175)第三行字大小调整为18