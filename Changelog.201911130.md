# 变更日志

# 详细信息:
## 2019/12/21
### 其他
* 添加[Changelog.md](Changelog.md)
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
**已在[osu默认字体设置](osu.Game/Graphics/OsuFont.cs)里面更改为默认20**

## 2019/12/23
* 翻译补全
    *   补全了"音乐播放"快捷键按下时的翻译
    *   补全了"鼠标按键"快捷键按下时的翻译
    *   多人游戏创建房间时的界面标题
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
* UI修改
    *   [工具栏个人信息按钮](osu.Game/Overlays/Toolbar/ToolbarUserButton.cs)
        *   文字调整为"别来无恙,<玩家名>!"(`Text = {api.LocalUser.Value.Username};`->`Text = $"别来无恙, {api.LocalUser.Value.Username} !";`)
        *   字体调整为18
    *   ["功能尚未准备"弹出菜单](osu.Game/Screens/ScreenWhiteBox.cs)
        *   (行168)第二行字大小调整为24
        *   (行175)第三行字大小调整为18

## 2019/12/24
* UI修改
    *   [osu!direct界面->谱面选择->排行榜第一名的显示](osu.Game/Overlays/BeatmapSet/Scores/TopScoreStatisticsSection.cs)
        *   (行126)字体大小调整为20
    *   **[osu!默认字体大小](osu.Game/Graphics/OsuFont.cs)**
        *   (行13)字体大小调整为20
            * **调整后大部分没有指定字体或无法调整字体的地方字号都将调整为18,如果你发现了界面排布错误,请及时[提交issue(github,优先推荐)](https://github.com/matrix-feather/osu/issues) [提交issue(gitee,如果github上不去则使用这个)](https://gitee.com/matrix-feather/osu/issues)**

* UI修改(失败)
    *   [osu!direct界面->谱面选择->排行榜的显示](osu.Game/Overlays/BeatmapSet/Scores/ScoreTable.cs)
        *   (行76~83)字体大小调整暂时失败:无法调整
    *   [多人联机游戏大厅->房间参与人数](osu.Game/Screens/Multi/Lounge/Components/ParticipantInfo.cs)
        *   字体大小调整暂时失败:代码中字体调整为17,然而界面没有任何改变

* 翻译补全
    *   [找不到osu!stable时的报错](osu.Desktop/OsuGameDesktop.cs)
    *   [失败界面标题](osu.Game/Screens/Play/FailOverlay.cs)
    *   **[选歌界面左侧侧边栏](osu.Game/Screens/Select/BeatmapDetailAreaTabControl.cs)**
        *   *把之前学编程时和命名空间有关的知识忘得一干二净*
        *   *下一步定位这类字体是在哪指定的*

* 翻译改进
    *   [暂停界面](osu.Game/Screens/Play/PauseOverlay.cs)
        *   副标题由 "你不会做我认为你会做的事，是吗:D" 改为 "要去做什么呢owo?"
    *   [Mods按钮](osu.Game/Screens/Select/FooterButtonMods.cs)
        *   文本由原有的"Mods"改为"额外玩法"
        *   *感觉这样子选歌界面看上去更和谐一点*
    *   [Mod选择界面](osu.Game/Overlays/Mods/ModSelectOverlay.cs)
        *   重置Mod按钮文本改为"恢复默认玩法"
        *   标题从"游戏Mods"改为"额外玩法"
        *   描述从"...有些Mod对你的游戏分数..."改为"...有些玩法对你的游戏分数..."
    *   [聊天界面](osu.Game/Overlays/ChatOverlay.cs)
        *   "在此输入你的信息"改为"再此输入你要发送的信息"
    *   [独立聊天界面](osu.Game/Online/Chat/StandAloneChatDisplay.cs)
        *   "在此输入你要发送的信息"改为"在此输入你要发送的信息"

* 文本翻译(失败)
    *   `An unhandled error has occurred`:无法找到
        *   猜测:存在于`osu!Framework`中
    *   `DEBUG LOG,visible,hidden`:无法被找到
        *   触发方法:`设置>调试>总体>显示日志Overlay`设置为`开`
    *   `*x refresh rate`:无法被找到
        *   猜测:存在于`osu!Framework`中

## 2019/12/25
* 翻译优化:
    *   [Mods按钮](osu.Game/Screens/Select/FooterButtonMods.cs)
        *   文本由原有的"额外玩法"改为"游戏Mods"
    *   [Mod选择界面](osu.Game/Overlays/Mods/ModSelectOverlay.cs)
        *   标题从"额外玩法"改为"游戏Mods"
        *   描述从"...有些玩法对你的游戏分数..."改为"...有些Mod对你的游戏分数..."
    *   [导入相关](osu.Game/Database/ArchiveModelManager.cs)
        *   翻译改进

* 翻译修正:
    *   [HUD>视觉设置](osu.Game/Screens/Play/PlayerSettings/VisualSettings.cs)
        *   (行13):从"可见度设置"变为"视觉效果设置"

## 2019/12/26
* 翻译重置:
    *   [多人创建房间的标题](osu.Game/Screens/Multi/Multiplayer.cs)
        *   (行116)翻译重置为`<玩家名>'s awesome room`


        
# 待定版测试
| 开始时间 | 结束时间 | 版本 |
| :--: | :--: | :--: |
| 2019/12/25 00:00:00 | 2019/12/28 00:00:00 | 2019.1113.0+matrixfeather 8 待定版本 |