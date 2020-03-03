# 帮助: 
##   官方pr界面获取更新
*   任何以 `[M]` 开头的均为已合并至`ppy:master`的pr, 不需要在之后进行检查
*   任何以 `[O]` 开头的均为仍在开放中的pr, 需要不定义检查是否有更新
# 变更日志

# 详细信息:
# 2020.104.0
## 2020/1/8:
### 游戏界面
*   [修正了比赛端的翻译错误](osu.Game.Tournament/TournamentSceneManager.cs)
    *   Thanks @[A M D](https://osu.ppy.sh/users/5321112) :D
*   [添加了开始界面的翻译](osu.Game/Screens/Menu/Disclaimer.cs)
*   [共1个界面的翻译改进](osu.Game/Overlays/Profile/Sections/BeatmapsSection.cs)
*   [共1个界面的翻译调整](osu.Game/Updater/SimpleUpdateManager.cs)
### 功能
*   从官方pr界面获取的更新
    *   [O] [#5782 - 优化转盘分数](https://github.com/ppy/osu/pull/5782)
### 其他
*   [更改`osu.Desktop.csproj`中的游戏版本为2020.104.0](osu.Desktop/osu.Desktop.csproj)
### 安装包构建
*   linux
    *   添加了amd64和i386架构的ffmpeg库
## 2020/1/9:
### 游戏
*   从官方pr界面获取的更新
    *   [O] [#7389 - 优化Mods按钮的显示](https://github.com/ppy/osu/pull/7389)
    *   [O] [#7222 - 向"WindUp"和"WindDown"Mod添加反向选项](https://github.com/ppy/osu/pull/7222)
    *   [O] [#7334 - 向Catch和Std模式添加"镜像"Mod](https://github.com/ppy/osu/pull/7334)
    *   [O] [#7316 - 向击打圆圈添加kiai模式下的闪光](https://github.com/ppy/osu/pull/7316)
    *   [M] [#7317 - 添加"打击结果"误差条](https://github.com/ppy/osu/pull/7317)
    *   [M] [#7438 - 添加登录占位符](https://github.com/ppy/osu/pull/7438)
    *   [M] [#7470 - 修复在退出编辑器后音乐速度没有恢复](https://github.com/ppy/osu/pull/7470)
    *   [M] [#7462 - 调整ctb和mania模式的生命恢复](https://github.com/ppy/osu/pull/7462)
    *   [M] [#7435 - 删除未使用的"LaneGlowPiece"和"GlowPiece](https://github.com/ppy/osu/pull/7435)
    *   [M] [#7451 - Mark storyboard sample retrieval test headless](https://github.com/ppy/osu/pull/7451)
    *   [M] [#7464 - Remove unused variable on DifficultyIcon](https://github.com/ppy/osu/pull/7464)
    *   [M] [#7458 - Allow scrolling through DimmedLoadingLayer](https://github.com/ppy/osu/pull/7464)

*   字体增大
    *   [OsuTabControl](osu.Game/Graphics/UserInterface/OsuTabControl.cs)
    *   [OsuTabControlCheckbox](osu.Game/Screens/Edit/Components/Menus/EditorMenuBar.cs)
    *   [PageTabControl](osu.Game/Graphics/UserInterface/PageTabControl.cs)
    *   [TabControlOverlayHeader](osu.Game/Overlays/TabControlOverlayHeader.cs)
    *   [EditorMenuBar](osu.Game/Screens/Edit/Components/Menus/EditorMenuBar.cs)
## 2020/1/10:
### 游戏
*   从官方pr处获取的更新
    *   [M] [#7450 - 修复osu!direct全局按键没有被正确绑定](https://github.com/ppy/osu/pull/7450)
*   将[direct界面的排行榜]()的字体增大

# 2020.118.0
## 2020/1/11:
*   从官方pr处获取的更新
    *   [M] [#7492 - 向主界面添加当前播放的音乐名](https://github.com/ppy/osu/pull/7492)
    *   [M] [#7485 - 观看回放时,显示当前回放的mods](https://github.com/ppy/osu/pull/7485)

## 2020/1/12:
*   从官方pr处获取的更新
    *   [M] [#7498 - 修复了在DiscordRPC初始化完成前改变玩家状态会导致的崩溃](https://github.com/ppy/osu/pull/7498)
    *   [M] [#7252 - 现在可以删除单个成绩了](https://github.com/ppy/osu/pull/7252)
    *   [M] [#7494 - *修复默认按钮吸收设置上的拖动滚动](https://github.com/ppy/osu/pull/7494)
    *   [M] [#7491 - *修复用户状态下拉菜单项没有填充的问题](https://github.com/ppy/osu/pull/7491)
    *   [M] [#7486 - 更新Framework](https://github.com/ppy/osu/pull/7486)
    *   [M] [#7490 - 更新Framework](https://github.com/ppy/osu/pull/7490)
    *   [M] [#6464 - 使上一个曲目按钮在第一次单击时重新启动当前曲目](https://github.com/ppy/osu/pull/6464)
*   现在会从github获取mfosu的最新更新了

## 2020/1/14:
*   从官方pr处获取的更新
    *   [M] [#7501 - 添加切换"正在播放"列表的快捷键](https://github.com/ppy/osu/pull/7501)
    *   [M] [#7510 - 修复在mania下选歌界面显示不正确的键位数量](https://github.com/ppy/osu/pull/7510)
    *   [M] [#7509 - *使新闻文章封面中的渐变受到悬停的影响](https://github.com/ppy/osu/pull/7509)
    *   [M] [#7484 - 移除osuTK.MathHelper的剩余引用](https://github.com/ppy/osu/pull/7484)
    *   [M] [#7497 - *将命中目标栏高度恒定移到更多本地类](https://github.com/ppy/osu/pull/7497)
    *   [M] [#7457 - 修复 CommentsContainer 的异步加载并不是真正的异步](https://github.com/ppy/osu/pull/7457)
    *   [M] [#7351 - *修复下载管理器可能无法正确处理取消请求](https://github.com/ppy/osu/pull/7351)
    *   [M] [#7384 - 为API请求设置UserAgent](https://github.com/ppy/osu/pull/7384)
    *   [M] [#7388 - *修复崩溃的TestSceneMedalOverlay](https://github.com/ppy/osu/pull/7388)
    *   [M] [#7472 - *降级NUnit适配器以修复discovery的问题](https://github.com/ppy/osu/pull/7472)
*   更新整合完毕,游戏基础版本更新至2020.112.0
## 2020/1/15:
*   从官方pr处获取的更新:
    *   [M] [#7528 - 允许在歌曲选择界面通过Alt键调整音量](https://github.com/ppy/osu/pull/7528)
    *   [M] [#7532 - 实现在编辑器内保存对谱面的更改](https://github.com/ppy/osu/pull/7532)
    *   [M] [#7543 - *解决注册请求中缺少UserAgent的问题](https://github.com/ppy/osu/pull/7543)
    *   [M] [#7535 - 实现将谱面导出为 .osz 格式](https://github.com/ppy/osu/pull/7535)
*   翻译补全:
    *   [补全了在导入成绩时找不到对应谱面时的报错信息](osu.Game/Scoring/Legacy/LegacyScoreParser.cs)
## 2020/1/18:
*   从官方pr处获取的更新:
    *   [M] [#7544 - 修复聊天频道控件的 test scene](https://github.com/ppy/osu/pull/7544)
    *   [M] [#7549 - 修复json网络请求拥有不正确的UserAgent](https://github.com/ppy/osu/pull/7549)
    *   [M] [#7555 - 更新Framework](https://github.com/ppy/osu/pull/7555)
    *   [M] [#7556 - 移除了Traceable的mod图标(Remove usage of snapchat icon)](https://github.com/ppy/osu/pull/7556)
*   翻译优化
    *   将[选歌界面下方mod按钮](osu.Game/Screens/Select/FooterButtonMods.cs)处的mods复原为"`游戏Mods`"

*   整合更新
    *   [M] [#7425 - *为通道读取状态实现缺少的API代码](https://github.com/ppy/osu/pull/7425)
    *   [M] [#7523 - *删除TournamentFont中的重复条件](https://github.com/ppy/osu/pull/7423)
    *   [M] [#6991 - 添加对 `--sdl` 参数的支持](https://github.com/ppy/osu/pull/6991)
    *   [M] [#7434 - 添加滑条折返箭头的动画](https://github.com/ppy/osu/pull/7434)
    *   [M] [#7537 - *使无图标的mod显示首字母缩写词](https://github.com/ppy/osu/pull/7537)
    *   [M] [#7474 - 添加Benchmark项目](https://github.com/ppy/osu/pull/7474)
    *   [M] [#7479 - 在排行榜实现按国家分类(Implement CountryFilter component for RankingsOverlay)](https://github.com/ppy/osu/pull/7479)


# 2020.125.1
## 2020/1/20:
*   添加了游戏Mods的翻译
*   [将游戏Mods按钮文本字体变大](osu.Game/Overlays/Mods/ModButton.cs)
*   整合更新
    *   [M] [#7561 - 修复在比赛端的一个硬崩溃如果一局中包含一个空谱面](https://github.com/ppy/osu/pull/7561)
    *   [M] [#7564 - Update license year](https://github.com/ppy/osu/pull/7564)

## 2020/1/22:
*   整合更新
    *   [M] [#7582 - *允许用alpha解析十六进制颜色代码](https://github.com/ppy/osu/pull/7582)

## 2020/1/24
*   整合更新
    *   [M] [#7585, #7587 - 应用OnRelease方法签名重构, 更新framework]()
        * [#7585](https://github.com/ppy/osu/pull/7585) 
        * [#7587](https://github.com/ppy/osu/pull/7587)
    *   [M] [#7588 - 将beat snapping移动到其自己的接口中](https://github.com/ppy/osu/pull/7588)
    *   [M] [#7590 - 更新一些国家的名称](https://github.com/ppy/osu/pull/7590)
    *   [M] [#7592 - 将TotalCommentsCounter 添加至 CommentsContainer中](https://github.com/ppy/osu/pull/7592)
    *   [M] [#7594 - 修复编辑器速率调整会不正确影响全局Beatmap速率的问题](https://github.com/ppy/osu/pull/7594)
*   移除整合
    *   [O] [#5782 - 实现转盘额外分数](https://github.com/ppy/osu/pull/5782)        

## 2020/1/25:
*   整合更新
    *   [M] [#7492 - 向主界面添加当前播放的音乐名](https://github.com/ppy/osu/pull/7492)
    *   [M] [#7596 - *修正回归输入处理顺序](https://github.com/ppy/osu/pull/7596)
    *   [M] [#7609 - 更新Framework](https://github.com/ppy/osu/pull/7609)
    *   [M] [#7566 - *尽可能使用仅获取自动属性](https://github.com/ppy/osu/pull/7566)
    *   [M] [#7538 - *解耦蓝图容器以允许在时间轴中使用](https://github.com/ppy/osu/pull/7538)
    *   [M] [#7589 - *扩展编辑器时间轴功能](https://github.com/ppy/osu/pull/7589)
    *   [M] [#7541 - *实现OverlayHeader的配色方案](https://github.com/ppy/osu/pull/7541)
    *   [M] [#3576 - *连接星级过滤器](https://github.com/ppy/osu/pull/3576)
    *   [M] [#5247 - 添加显示难度分布表的设置](https://github.com/ppy/osu/pull/5247)
    *   [M] [#7548 - 添加Mod设置的 (de)serialization 支持](https://github.com/ppy/osu/pull/7548)
    *   [M] [#7577 - 使`难度调整`被选择时自动打开自定义界面](https://github.com/ppy/osu/pull/7577)
    *   [M] [#7591 - 改进在超宽屏上(或超低UI缩放时)的歌曲选择显示](https://github.com/ppy/osu/pull/7591)
    *   [M] [#7460 - *允许CommentContainer使用一种方法重新获取评论](https://github.com/ppy/osu/pull/7460)
    *   [M] [#7567 - 更新fastlane和一些插件](https://github.com/ppy/osu/pull/7567)

*   **暂时移除了自动下载更新的功能**

# 2020.125.2
## 2020/1/27
*   整合更新
    *   [M] [#7614 - Fix NullReferenceException on main menu for mobile game hosts](https://github.com/ppy/osu/pull/7614)
    *   [M] [#7618 - Move reverse-order comparer to ChannelTabControl](https://github.com/ppy/osu/pull/7618)
    *   [M] [#7622 - Remap osu!mania dual stage key bindings to be more ergonomic](https://github.com/ppy/osu/pull/7622)
    *   [M] [#7624 - Fix visual inconsistency in BreadcrumbControl](https://github.com/ppy/osu/pull/7624)
    *   [M] [#7627 - Fix typo on ScoreProcessor comment](https://github.com/ppy/osu/pull/7627)
    *   [M] [#7628 - Fix cursor not hiding for screenshots #7628](https://github.com/ppy/osu/pull/7628)
    *   [M] [#7630 - Fix changelog header not dimming correctly on initial build display](https://github.com/ppy/osu/pull/7630)
    *   [M] [#7597 - Implement OverlayColourProvider component](https://github.com/ppy/osu/pull/7597)
    *   [M] [#7631 - Bump DiscordRichPresence from 1.0.121 to 1.0.147](https://github.com/ppy/osu/pull/7631)
## 2020/1/29
*   添加翻译
    *   编辑器(**仍需后续完善**)
        *   添加了编辑器内物件右键菜单的翻译
            *   (滑条)[osu.Game.Rulesets.Osu/Edit/Blueprints/Sliders/   SliderSelectionBlueprint.cs]
            *   (物件)[osu.Game/Screens/Edit/Compose/Components/SelectionHandler.cs]
        *   [添加了编辑器内"Playback speed"的翻译](osu.Game/Screens/Edit/Components/    PlaybackControl.cs)
        *   [添加了Setup Mode和Design Mode界面的翻译]
            *   [Setup Mode](osu.Game/Screens/Edit/Setup/SetupScreen.cs)
            *   [Design Mode](osu.Game/Screens/Edit/Setup/DesignScreen.cs)
        *   [ControlPointTable.cs:time和attributes补全](osu.Game/Screens/Edit/Timing/ControlPointTable.cs)
        *   [effectSection](osu.Game/Screens/Edit/Timing/EffectSection.cs)

*   整合更新
    *   [M] [#7623 - Fix crash due to misordered selection events](https://github.com/ppy/osu/pull/7623)
    *   [M] [#7432 - Implement ability to create OverlayHeader with no TabControl](https://github.com/ppy/osu/pull/7432)
    *   [M] [#7638 - Use type switch in SerializationWriter](https://github.com/ppy/osu/pull/7638)
    *   [M] [#7637 - Refactor background creation in OverlayHeader](https://github.com/ppy/osu/pull/7637)
    *   [M] [#7650 - Change default method style for better IDE autocompletion](https://github.com/ppy/osu/pull/7650)
    *   [M] [#7636 - Make CommentsContainer use OverlayColourProvider](https://github.com/ppy/osu/pull/7636)
    *   [M] [#7645 - Allow OsuSliderBar tooltip to show as percentage as needed](https://github.com/ppy/osu/pull/7645)
    *   [M] [#7659 - Fix random PlaySongSelect test failures](https://github.com/ppy/osu/pull/7659)
    *   [M] [#7653 - Fix navigation test crashing when raw input is disabled](https://github.com/ppy/osu/pull/7653)
    *   [M] [#7652 - Fix key count being incorrectly adjusted by hard/easy mods](https://github.com/ppy/osu/pull/7652)
    *   [M] [#7634 - Add beat ticks to editor timeline](https://github.com/ppy/osu/pull/7634)

## 2020.130.1
### 2020/1/30:
*   整合更新
    *   [M] [#7675 - Fix possible crash when searching with no channel topic](https://github.com/ppy/osu/pull/7675)
    *   [M] [#7593 - Minor cleanups for Legacy Storyboard/Beatmap decoder](https://github.com/ppy/osu/pull/7593)
    *   [M] [#7647 - Move select tool to an actual tool implementation](https://github.com/ppy/osu/pull/7647)
    *   [M] [#7671 - Rename and tidy up DeletedCommentsCounter](https://github.com/ppy/osu/pull/7671)
    *   [M] [#7642 - Fix beat snap implementation being incorrect](https://github.com/ppy/osu/pull/7642)
    *   [M] [#7644 - Standardise editor timeline zoom across maps of all lengths](https://github.com/ppy/osu/pull/7644)
    *   [#7643 - Distance snap grid correct colouring](https://github.com/ppy/osu/pull/7643)
    *   [M] [#7649 - Ensure selection tool correctly matches selection state](https://github.com/ppy/osu/pull/7649)
    *   [M] [#7648 - Allow selecting composition tools using 1-4 keys](https://github.com/ppy/osu/pull/7648)
    *   [!_有变动,需要重新描述这个更新] [O->M] [#7222 - 向"WindUp"和"WindDown"Mod添加反向选项](https://github.com/ppy/osu/pull/7222)
    *   [M] [#7673 - Fix osu!catch not handling all vertical space](https://github.com/ppy/osu/pull/7673)
    *   [M] [#7670 - Bring UserProfileOverlay colour scheme in line with web](https://github.com/ppy/osu/pull/7670)
    *   [M] [#7676 - Fix negative replay frames being played back incorrectly](https://github.com/ppy/osu/pull/7676)
    *   [M] [#7667 - Fix editor being accessible for multiplayer song select](https://github.com/ppy/osu/pull/7667)
    *   [M] [#7663 - Fix presenting a beatmap from a different ruleset not working](https://github.com/ppy/osu/pull/7663)
    *   [M] [#7661 - Move navigation / game test scenes to new namespace](https://github.com/ppy/osu/pull/7661)
    *   [M] [#7613 - Remove build target from Fastfile](https://github.com/ppy/osu/pull/7613)
    *   [M] [#7664 - Fix percentage-formatted displays containing a space](https://github.com/ppy/osu/pull/7664)
    *   [M] [#7554 - Update profile scores in line with the web design](https://github.com/ppy/osu/pull/7554)
    *   [M] [#7654 - Add a method to recycle test storage between runs #7654](https://github.com/ppy/osu/pull/7654)

### 2020/2/2:
*   整合更新
    *   [m] [#7389 - Fix footer button content not correctly being centered](https://github.com/ppy/osu/pull/7389)
    *   [m] [#7683 - Fix chat test intermittently failing](https://github.com/ppy/osu/pull/7683)
    *   [m] [#7691 - Fix incorrect nUnit adapter version causing rider failures](https://github.com/ppy/osu/pull/7691)
    *   [m] [#7690 - Centralise screen exit logic to ScreenTestScene](https://github.com/ppy/osu/pull/7690)
    *   [m] [#7687 - Fix Alt+number shortcuts for tabs in chat overlay](https://github.com/ppy/osu/pull/7687)
    *   Make use of ElementAtOrDefault() when possible #76954
    *   Remove all usages of Bindable<float> and Bindable<double> #7709

### 2020/2/3:
*   整合更新
    *   [m] [#7710 - Adjust profile scores to closer match osu-web](https://github.com/ppy/osu/pull/7710)
    *   [m] [#7714 - Make slider tracking match what is on screen](https://github.com/ppy/osu/pull/7714)
    *   [m] [#7716 - Add ability to create ruleset selector in OverlayHeader](https://github.com/ppy/osu/pull/7716)
    *   Bump Sentry from 1.2.0 to 2.0.1 #7717

### 2020/2/4:
*   整合更新
    *   [m] [#7719 - Update BeatmapOverlay header in line with the web design](https://github.com/ppy/osu/pull/7719)
    *   [m] [#7720 - Fix potential nullref in UserDimBackgrounds tests](https://github.com/ppy/osu/pull/7720)
    *   [m] [#7723 - Expose TabControlOverlayHeader.Current value](https://github.com/ppy/osu/pull/7723)

### 2020/2/5:
*   整合更新
    *   [m] [#7724 - Update BeatmapSetOverlay to match web design](https://github.com/ppy/osu/pull/7724)
    *   [m] [#7725 - Adjust TopScoreStatisticsSection to closer match web design](https://github.com/ppy/osu/pull/7725)
    *   [m] [#7721 - Add {ScoreInfo,UserStatistics}.Accuracy](https://github.com/ppy/osu/pull/7721)
    *   [m] [#7726 - Add API to get rankings data for selected spotlight #7726](https://github.com/ppy/osu/pull/7726)
*   变更
    *   [+] 添加了原版的开场动画
        *   [Circles](osu.Game/Screens/Menu/IntroCircles.cs)
        *   [Triangles](osu.Game/Screens/Menu/IntroTriangles.cs)
        *   现在的开场动画默认为汉化后的Triangles(TrianglesCN)
        *   涉及修改的文件
            *   [osu.Game/Screens/Menu/OsuLogo.cs](osu.Game/Screens/Menu/OsuLogo.cs)
            *   [osu.Game/Screens/Loader.cs](osu.Game/Screens/Loader.cs)
            *   [osu.Game/Configuration/IntroSequence.cs](osu.Game/Configuration/IntroSequence.cs)
            *   [osu.Game/Configuration/OsuConfigManager.cs](osu.Game/Configuration/OsuConfigManager.cs)

### 2020/2/8:
*   整合更新
    *   [m] [Allow guest users to see the comments in CommentsContainer #7728]
    *   [m] [Make EditorBeatmap a drawable component #7732]
    *   [m] [Add the ability to extend hold notes (spinners / sliders etc.) via timeline #7734]
    *   [m] [Fix rooms test scene not displaying anything #7739]

    *   [m] [Simplify the way multiple subscreens handle their disable states via a custom stack #7741]
    *   [m] [Make multiplayer room listing filter by current ruleset #7740]

    *   [m] [Set a sane default keyboard step for mod settings #7742]
    *   [m] [Make ScreenshotManager a Component #7743]
    *   [m] [Fix lifetime calculation in overlapping scroll algorithm #7745]
    *   [m] [Update framework #7747]
    *   [m] [Fix incorrect distance snap grid being displayed when in selection mode #7749]
    *   [m] [Fix editor test scene exiting after loading #7750]
    *   [m] [Fix duration snapping still being incorrect #7751]
    *   [m] [Update the windows platform offset to match stable #7752]
    *   [m] [Make editor screens display below timeline #7754]
    *   [m] [Fix spinner placement blueprint in multiple ways #7755]
    *   [m] [Receive historical monthly user playcounts from API #7758]
    *   [m] [Fix beatmap overlay scoreboards having their statistics columns unsorted #7760]
    *   [m] [Make accuracy formatting more consistent #7763]
    *   [m] [Update default background dim to 80% to match stable #7766]
    *   [m] [Ensure OsuScreen leases are taken out synchronously #7692]
    *   [m] [Decouple ModSelectOverlay from global SelectedMods #7677]
    *   [m] [Refactor performFromMenu to work with multiple screen targets #7678]
    *   [m] [Fix too many ticks being displayed on beatmaps with multiple timing sections #7682]
    *   [m] [Match osu-stable follow circle behaviour and size #7704]
    *   [m] [Fix key counter ignoring visibility setting on HUD visibility change #7712]
    *   [m] [Apply precision when determining bar colour in difficulty statistics #7703]
    *   [m] [Return to song select after viewing a replay from the leaderboard #7679]
    *   [m] [Fix mod select overlay overflowing toolbar at max ui scale #7707]
    *   [m] [Implement SpotlightSelector component for RankingsOverlay #7488]
    *   [m] [Reimplement music playlist using framework's RearrangeableListContainer #7680]
    *   [m] [Update RankingsOverlay in line with the web design #7722]
    *   [m] [Implement OverlayRulesetSelector component #7418]
    *   [m] [Update profile recent activities in line with the web design #7668]
    *   [m] [Add placeholder for no comments case in CommentsContainer #7655]

*   翻译优化
    *   [osu.Game/Overlays/Profile/Sections/RecentSection.cs](osu.Game/Overlays/Profile/Sections/RecentSection.cs)
        *   "最近游玩"改为"近期活动"

*   Issues
    *   [mf #2 一点建议: 将"开始动画"处的”欢迎来到osu!“改回"welcome to osu!"]
        *   解决方案:
            *   创建翻译版的同时保留原版
            *   默认选择原版
    *   [mf #3 翻译错误: "设置 -> 细节设置"处的"故事版"应为"故事板"]
        *   已解决,同时发现并纠正了其他地方的一处相同错误

### 2020/2/9 更新发布后
*   整合更新
    *   [m] [Make the caret blink to the beat #7761]

### 2020/2/10
*   翻译补全
    *   **疑似**补全了比赛端下方SongBar的内容
        *   [文件](osu.Game.Tournament/Components/SongBar.cs)

### 2020/2/11
*   整合更新
    *   [m] [Minor cleanups for legacy beatmap decoders #7768]
    *   [m] [Improve mod settings tests #7773]
    *   [m] [Fix BeatSyncContainer failing at song select #7776]
    *   [m] [Bump Sentry from 2.0.1 to 2.0.2 #7781]
    *   [m] [Bump Microsoft.NET.Test.Sdk from 16.4.0 to 16.5.0 #7782]
*   补全了一些翻译

### 2020/2/14
*   整合更新
    *   [m] [Fix order and naming of Difficulty Adjust sliders #7780]
    *   [m] [Bypass song select filter to show externally changed beatmap temporarily #7783]
    *   [m] [Fix disposal-related errors by making WorkingBeatmap non-disposable #7784]
    *   [m] [Fix InfoColumn minWidth implementation #7792]
    *   [m] [Fix potential crash when exiting game while entering song select #7793]
    *   [m] [Add mouse down repeat support to timeline zoom buttons #7806]
    *   [m] [Disallow seeking on osu!direct download progress bars #7808]
    *   [m] [Improve extensibility of mod display expansion #7811]
    *   [m] [Make playlist beatmap and ruleset into bindables #7812]
    *   [m] [更新Framework #7813]
    *   [m] [Remove the concept of a "current" item from multiplayer rooms #7814]
    *   [m] [Make room participants into a bindable list #7815]
    *   [m] [Add missing null-allowance to leaderboard #7816]
    *   [m] [Use Span for OsuColour.FromHex #7827]

*   谱面排行榜部分字体增大
    *   [osu.Game/Overlays/BeatmapSet/Scores/TopScoreStatisticsSection.cs](osu.Game/Overlays/BeatmapSet/Scores/TopScoreStatisticsSection.cs)
    *   [osu.Game/Overlays/BeatmapSet/Scores/TopScoreUserSection.cs](osu.Game/Overlays/BeatmapSet/Scores/TopScoreUserSection.cs)
    *   [osu.Game/Overlays/BeatmapSet/Scores/ScoreTable.cs](osu.Game/Overlays/BeatmapSet/Scores/ScoreTable.cs)

*   整合时出现问题的更新
### 2020/02/15
*   整合更新
    *   [m] [Make beatmap detail area abstract #7832]
    *   [m] [Improve PlayerLoader code quality #7835]
    *   [m] [Use Resolved attribute instead of BackgroundDependencyLoader wherever possible #7837]

### 2020/02/16
*   整合更新
    *   [m] [Show current placement blueprint in timeline #7753]
    *   [m] [Seek to previous object's end time on successful placement #7756]
    *   [m] [Fix not being able to seek using scroll wheel in timeline while playing track #7807]
    *   [m] [Fix osu! gameplay cursor not adjusting to mod/convert circle size changes #7828]
    *   [m] [Fix player loading sequence continuing even when a priority overlay is visible #7836]
    *   [m] [Fix editor hit objects displaying incorrectly after StartTime change #7800]
    *   [m] [Fix blueprint showing even when mouse outside of container #7803]
    *   [m] [Update placement blueprint's position more often #7805]
    *   [m] [Fix relax mod not working correctly on beatmaps with overlapping sliders/spinners #7830]
    *   [m] [Add a key up delay for relax mod presses #7831]
    *   [m] [Implement a rearrangeable beatmap playlist control #7829]

    *   [m] [Implement the match beatmap detail area #7833]
    *   [m] [Add the ability to select multiple beatmaps in multiplayer song select #7834]
    *   [m] [Redesign match subscreen to add playlist support #7839]
    
    *   [m] [Adjust minor BeatmapSetOverlay details to better match osu-web #7730]
    *   [m] [Add spotlight selector to RankingsOverlay #7733]
    *   [m] [Add a container type to easily hide online content when not online #7546]
    *   [m] [Fix DownloadTrackingComposite incorrectly receiving cancelled state #7844]

### 2020/02/16 更新后:
*   整合更新
    *   [m] [Update inspectcode version and fix new issues #7841]
    *   [m] [Adjust user profile score to closer match web #7846]
    *   [m] [Replace hashcode override with local equality comparer #7848]
    *   [m] [Fix transform mod not being applied correctly #7855]
    *   [m] [Fix visible error being thrown when playing a no-video beatmap #7856]
    *   [o] [Display a login placeholder in direct when user isn't logged in. #7854]

*   [改进了聊天栏动作信息的显示方式](osu.Game/Overlays/Chat/ChatLine.cs)
*   [改进了LoginPlaceHolder的显示方式](osu.Game/Online/Placeholders/LoginPlaceholder.cs)
*   [向Toolbar添加了“时间”按钮](osu.Game/Overlays/Toolbar/ToolbarTimeButton.cs)
*   [删除了之前的ToolbarVideoButton]

### 2020/02/17:
*   增大字体
    *   [osu.Game/Overlays/TabControlOverlayHeader.cs](osu.Game/Overlays/TabControlOverlayHeader.cs)
    *   [osu.Game/Graphics/UserInterface/ScreenTitle.cs](osu.Game/Graphics/UserInterface/ScreenTitle.cs)
    *   [osu.Game/Screens/Select/Leaderboards/UserTopScoreContainer.cs](osu.Game/Screens/Select/Leaderboards/UserTopScoreContainer.cs)
*   优化翻译
    *   [多人联机的Header](osu.Game/Screens/Multi/Header.cs)
    *   [timing设计菜单的各种section](osu.Game/Screens/Edit/Timing/DifficultySection.cs)
*   补全翻译
    *   [!涉及界面逻辑代码更改] [编辑器滑条点１级菜单的翻译](osu.Game.Rulesets.Osu/Edit/Blueprints/Sliders/Components/PathControlPointVisualiser.cs)
    *   [SkipBarLine](osu.Game/Screens/Edit/Timing/EffectSection.cs)

*   整合更新
    *   [m] [Update framework #7861]
    *   [m] [Use OverlayColourProvider for CounterPill in profile overlay #7866]
    *   [m] [Update readme with local changelog and project management link #7868]
    *   [m] [Fix possible error in SpotlightsLayout after it's disposal #7872]
    *   [m] [Fix playlist items added with the wrong IDs #7876]
    *   [m] [Bump Sentry from 2.0.2 to 2.0.3 #7877]
    *   [m] [Bump ppy.osu.Framework.NativeLibs from 2019.1104.0 to 2020.213.0 #7878]

### 2020/02/18:
*   整合更新
    *   [m] [Restructure readme to better define prerequisites that are required for development only #7885]
    *   [m] [Fix osu!catch fruit exploding/dropping multiple timed is skin is changed during explode animation #7888]
    *   [m] [Fix ogg beatmap/skin samples not loading #7889]
    *   [m] [Fix playlist items potentially not updating to the correct selected state #7891]
*   跟踪更新
    *   [o] [Display a login placeholder in direct when user isn't logged in. #7854]

### 2020/02/19:
*   统一以下界面的部分字体大小为19号:
    *   [休息时段下方显示](osu.Game/Screens/Play/Break/BreakInfo.cs)
    *   ["/ 不计入排名 /"的显示字号](osu.Game/Screens/Play/HUD/ModDisplay.cs)
    *   [回放hud右侧工具栏](osu.Game/Screens/Play/PlayerSettings/PlayerSettingsGroup.cs)
*   统一以下界面的部分字体大小为17号:
    *   [osu.Game/Overlays/Direct/DirectGridPanel.cs](osu.Game/Overlays/Direct/DirectGridPanel.cs)
    *   [osu.Game/Screens/Select/BeatmapInfoWedge.cs](osu.Game/Screens/Select/BeatmapInfoWedge.cs)
    *   [osu.Game/Overlays/BeatmapSet/Scores/ScoreTable.cs](osu.Game/Overlays/BeatmapSet/Scores/ScoreTable.cs)
*   更改以下界面的部分字体大小为16号
    *   [92行左右　osu.Game/Overlays/Profile/Sections/Ranks/DrawableProfileScore.cs](osu.Game/Overlays/Profile/Sections/Ranks/DrawableProfileScore.cs)
*   统一以下界面的部分字体大小为18号:
    *   [osu.Game/Overlays/BeatmapSet/LeaderboardScopeSelector.cs](osu.Game/Overlays/BeatmapSet/LeaderboardScopeSelector.cs)
*   翻译修正
    *   [osu.Game/Overlays/Profile/Sections/BeatmapsSection.cs](osu.Game/Overlays/Profile/Sections/BeatmapsSection.cs)

*   整合更新
    *   [m] [Implement BeatmapListingSearchSection component #7892]
        *   前置pr
            *   [Implement BeatmapSearchFilter component #7858]
    *   [m] [Refactor RankingsOverlay to improve edge-case conditions #7893]

*   翻译补全
    *   编辑器补全
        *   [osu.Game/Screens/Edit/Timing/ControlPointTable.cs](osu.Game/Screens/Edit/Timing/ControlPointTable.cs)
            *   附带更改
                *   [osu.Game/Screens/Edit/Timing/RowAttribute.cs](osu.Game/Screens/Edit/Timing/RowAttribute.cs)
        *   工具栏(TOOLBOX)
            *   [osu.Game/Rulesets/Edit/ToolboxGroup.cs](osu.Game/Rulesets/Edit/ToolboxGroup.cs)
            *   [osu.Game/Rulesets/Edit/Tools/SelectTool.cs](osu.Game/Rulesets/Edit/Tools/SelectTool.cs)
            *   [osu.Game.Rulesets.Osu/Edit/HitCircleCompositionTool.cs](osu.Game.Rulesets.Osu/Edit/HitCircleCompositionTool.cs)
            *   [osu.Game.Rulesets.Osu/Edit/SliderCompositionTool.cs](osu.Game.Rulesets.Osu/Edit/SliderCompositionTool.cs)
            *   [osu.Game.Rulesets.Osu/Edit/SpinnerCompositionTool.cs](osu.Game.Rulesets.Osu/Edit/SpinnerCompositionTool.cs)
            *   [osu.Game.Rulesets.Mania/Edit/HoldNoteCompositionTool.cs](osu.Game.Rulesets.Mania/Edit/HoldNoteCompositionTool.cs)
            *   [osu.Game.Rulesets.Mania/Edit/NoteCompositionTool.cs](osu.Game.Rulesets.Mania/Edit/NoteCompositionTool.cs)

*   翻译失败
    *   [osu.Game/Screens/Edit/Timing/Section.cs](osu.Game/Screens/Edit/Timing/Section.cs)
        *   原因:
            *   DifficultySection,TimingSection等的标题用的是Section下的`LabelText = typeof(T).Name.Replace(typeof(ControlPoint).Name, string.Empty)`语句，然而 `typeof(T)` , `typeof(ControlPoint)` 相关文本未找到, 或许在`osu.Framework`中?

### 2020/02/20
*   未整合
    *   [m] [Fix possible test failures due to async loads #7.917]
        *   Tests项目目前暂不维护
    *   [m] [Implement BeatmapListingOverlay #7.912]
        *   存在前置pr
*   整合更新
    *   [m] [Fix music playlist being enumerated asynchronously #7875]
    *   [m] [Highlight max combo on beatmap leaderboards #7913]
    *   [m] [Make country names in RankingsOverlay clickable #7910]
    *   [m] [Fix osu!catch not cycling fruit as often as it should be #7898]
    *   [m] [Refactor overlined displays to support different sizing modes #7899]
    *   [m] [Add a global multiplayer background header #7909]
    *   [m] [Improve consistency of buttons in multiplayer #7921]
    *   [m] [Update the multiplayer room inspector design #7900]

### 2020/02/21
*   未整合
    *   [m] [Update osu!catch test scenes to show skinnable versions #7.879]
        *   Tests项目目前暂不维护
    *   [m] [Show skinnable test skin names and autosized component sizes #7.871]
        *   Tests项目目前暂不维护

*   整合更新
    *   [m] [Improve visual appearance of ProcessingOverlay #7922]
    *   [m] [Add tooltips with precise dates to beatmap set overlay #7779]
    *   [m] [Adjust font sizes and spacing in BeatmapSetOverlay to match osu-web values #7863]
    *   [m] [Allow selecting/playing a specific difficulty using the beatmapset difficulty icons #7809]
    *   [m] [**实现ctb模式的皮肤支持 #7881**]
    *   [m] [Move DrawableHitObject accent colour update duties to ruleset #7896]
    *   [m] [Rename drawable namespace to avoid clashes with framework class #7902]
    *   [m] [Add osu!catch droplet rotation animation #7901]
    *   [m] [Bump ppy.osu.Game.Resources from 2020.219.0 to 2020.221.0 #7928]
    *   [m] [Bump DiscordRichPresence from 1.0.147 to 1.0.150 #7929]
    *   [m] [Split out pulp formations into individual classes #7911]
    *   [m] [Implement CommentEditor component #7795]
    *   [m] [Make RankingsOverlay available in-game #7817]
    *   [m] [Use a single implementation for all loading animations #7931]
    *   [m] [更新Framework #7935]
    *   [m] [Add simple updater support for linux AppImages #7936]
    *   [m] [Use new loading layer in beatmap listing overlay #7932]
    *   [m] [Add explosion effect when catching fruit #7934]
    *   [m] [Add fill to default skin slider ball when tracking #7933]
    *   [m] [Add CommentsContainer to ChangelogOverlay #7930]
    *   [m] [Add ability to load long comment trees in CommentsContainer #7786]
    *   [m] [Fix indices in beatmap not being transferred to children #7897]
    *   [m] [Update with framework-side bindable list changes #7874]
    *   [m] [Support null leaderboard position #7919]
    *   [m] [Implement BeatmapListingSortTabControl component #7864]
    *   [m] [Implement BeatmapListingOverlay #7912]
    *   [m] [Update design of username and country name in RankingsOverlay #7820]

*   翻译修正
    *   [osu.Game/Screens/Edit/Timing/ControlPointTable.cs 第141行](osu.Game/Screens/Edit/Timing/ControlPointTable.cs)

## 2020/02/22:
*   一些翻译优化
*   翻出来一个新的谱面列表(BeatmapListingOverlay)
*   将各类Tests与官方同步

## 2020/2/25:
*   已停止继续分发二进制版本，请自行构建。
*   整合更新
    *   [m] [Reduce hit error display performance overhead #7967]
    *   [m] [Improve gameplay performance via follow point renderer optimisations #7968]
    *   [m] [Fix scale of loading animation on player loading screen #7969]
    *   [m] [Update framework #7989]
    *   [m] [Fix hitobjects with unknown lifetimes by enforcing non-null judgement #7973]
    *   [m] [Fix bar lines in osu!taiko and osu!mania not correctly being cleaned up #7974]
    *   [m] [Expose save option in editor to non-desktop platforms #7977]
    *   [m] [Fix potential crash when clicking on show more button in comments #7982]
    *   [!:因为和现有一些文件冲突，未整合全部] [Rework issue templates #7961]

### 2020/2/26:
*   整合更新
    *   [m] [Open Rankings when clicking on charts button #7955]
    *   [m] [Implement UserListToolbar component #7983]
    *   [m] [Add CommentsContainer to BeatmapSetOverlay #7951]
    *   [m] [Adjust ChangelogOverlay appearance to match web #7940]
    *   [m] [Increase HP awarded for GOODs from 1.5% to 2.5% #7997]

### 2020/2/27:
*   整合更新
    *   [m] [Fix hyperdash red glow not being visible #8004]
    *   [m] [Fix catcher dropping juice streams due to it considering ignored judgements #8005]
    *   [m] [Fix stutter when showing game HUD after being hidden for a while #8007]
    *   [m] [Use overlay theme colours for comment vote button #8014]
    *   [m] [Fix incorrect RepliesButton presentation in comments #8030]
    *   [m] [Hide logo background when exiting with the triangles intro #8026]
    *   [m] [Remove workarounds for CreateRoomRequest shortcomings #8027]
    *   [m] [Add delay for loading multiplayer beatmap covers #8028]
    *   [m] [Add labels and fix bindable values of SettingSource dropdowns #8040]
    *   [m] [Implement new multiplayer participants retrieval #8029]
    *   [m] [Remove legacy DrawableHitObject state management #8021]
    *   [m] [Fix iOS/Android lockups by disabling LLVM #8020]

### 2020/03/01:
*   整合更新
    *   [m] [Fix crash when reaching results screen on single threaded execution mode #8050]
    *   [m] [Update Framework #8053]
*   修复一处翻译缺失
    *   [osu.Game/Beatmaps/DummyWorkingBeatmap.cs:28-29](osu.Game/Beatmaps/DummyWorkingBeatmap.cs)

### 2020/03/03:
*   整合更新
    *   [m] [Bump Sentry from 2.0.3 to 2.1.0 #8088]
    *   [m] [Fix double-dimming in changelog stream badge area #8031]