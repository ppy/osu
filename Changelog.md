# 变更日志

# 详细信息:
## 2020/1/8:
### 游戏界面
*   [修正了比赛端的翻译错误](osu.Game.Tournament/TournamentSceneManager.cs)
    *   Thanks @[A M D](https://osu.ppy.sh/users/5321112) :D
*   [添加了开始界面的翻译](osu.Game/Screens/Menu/Disclaimer.cs)
*   [共1个界面的翻译改进](osu.Game/Overlays/Profile/Sections/BeatmapsSection.cs)
*   [共1个界面的翻译调整](osu.Game/Updater/SimpleUpdateManager.cs)
### 功能
*   从官方pr界面获取的更新
    *   [#5782 - 优化转盘分数](https://github.com/ppy/osu/pull/5782)
### 其他
*   [更改`osu.Desktop.csproj`中的游戏版本为2020.104.0](osu.Desktop/osu.Desktop.csproj)
### 安装包构建
*   linux
    *   添加了amd64和i386架构的ffmpeg库
## 2020/1/9:
### 游戏
*   从官方pr界面获取的更新
    *   [#7389 - 优化Mods按钮的显示](https://github.com/ppy/osu/pull/7389)
    *   [#7222 - 向"WindUp"和"WindDown"Mod添加反向选项](https://github.com/ppy/osu/pull/7222)
    *   [#7334 - 向Catch和Std模式添加"镜像"Mod](https://github.com/ppy/osu/pull/7334)
    *   [#7316 - 向击打圆圈添加kiai模式下的闪光](https://github.com/ppy/osu/pull/7316)
    *   [#7317 - 添加"打击结果"误差条](https://github.com/ppy/osu/pull/7317)
    *   [#7438 - 添加登录占位符](https://github.com/ppy/osu/pull/7438)
    *   [#7470 - 修复在退出编辑器后音乐速度没有恢复](https://github.com/ppy/osu/pull/7470)
    *   [#7462 - 调整ctb和mania模式的生命恢复](https://github.com/ppy/osu/pull/7462)
    *   [#7435 - 删除未使用的"LaneGlowPiece"和"GlowPiece](https://github.com/ppy/osu/pull/7435)
    *   [#7451 - Mark storyboard sample retrieval test headless](https://github.com/ppy/osu/pull/7451)
    *   [#7464 - Remove unused variable on DifficultyIcon](https://github.com/ppy/osu/pull/7464)
    *   [#7458 - Allow scrolling through DimmedLoadingLayer](https://github.com/ppy/osu/pull/7464)

*   字体增大
    *   [OsuTabControl](osu.Game/Graphics/UserInterface/OsuTabControl.cs)
    *   [OsuTabControlCheckbox](osu.Game/Screens/Edit/Components/Menus/EditorMenuBar.cs)
    *   [PageTabControl](osu.Game/Graphics/UserInterface/PageTabControl.cs)
    *   [TabControlOverlayHeader](osu.Game/Overlays/TabControlOverlayHeader.cs)
    *   [EditorMenuBar](osu.Game/Screens/Edit/Components/Menus/EditorMenuBar.cs)
## 2020/1/10:
### 游戏
*   从官方pr处获取的更新
    *   [#7450 - 修复osu!direct全局按键没有被正确绑定](https://github.com/ppy/osu/pull/7450)
*   将[direct界面的排行榜]()的字体增大
//////////////////////////////////////////////////////
## 2020/1/11:
*   从官方pr处获取的更新
    *   [#7492 - 向主界面添加当前播放的音乐名](https://github.com/ppy/osu/pull/7450)
    *   [#7485 - 观看回放时,显示当前回放的mods](https://github.com/ppy/osu/pull/7485)

## 2020/1/12:
*   从官方pr处获取的更新
    *   [#7498 - 修复了在DiscordRPC初始化完成前改变玩家状态会导致的崩溃](https://github.com/ppy/osu/pull/7498)
    *   [#7252 - 现在可以删除单个成绩了](https://github.com/ppy/osu/pull/7252)
    *   [#7494 - Fix default button absorbing drag scroll on settings](https://github.com/ppy/osu/pull/7494)
    *   [#7491 - Fix user status dropdown having no padding around items](https://github.com/ppy/osu/pull/7491)
    *   [#7486 - 更新osu.Framework版本](https://github.com/ppy/osu/pull/7486)
    *   [#7490 - 更新osu.Framework版本](https://github.com/ppy/osu/pull/7490)
    *   [#6464 - 使上一个曲目按钮在第一次单击时重新启动当前曲目](https://github.com/ppy/osu/pull/6464)
*   现在会从github获取mfosu的最新更新了

## 2020/1/14:
*   从官方pr处获取的更新
    *   [#7501 - 添加切换"正在播放"列表的快捷键](https://github.com/ppy/osu/pull/7501)
    *   [#7510 - 修复在mania下选歌界面显示不正确的键位数量](https://github.com/ppy/osu/pull/7510)
    *   [#7509 - Make gradient in NewsArticleCover be effected by hover](https://github.com/ppy/osu/pull/7509)
    *   [#7484 - Remove remaining usage of osuTK.MathHelper](https://github.com/ppy/osu/pull/7484)
    *   [#7497 - Move hit target bar height constant to more local class](https://github.com/ppy/osu/pull/7497)
    *   [#7457 - Fix CommentsContainer async loading wasn't really async](https://github.com/ppy/osu/pull/7457)
    *   [#7351 - Fix download manager potentially not handling cancel requests properly](https://github.com/ppy/osu/pull/7351)
    *   [#7384 - Set UserAgent for API requests](https://github.com/ppy/osu/pull/7384)
    *   [#7388 - Fix crashing TestSceneMedalOverlay](https://github.com/ppy/osu/pull/7388)
    *   [#7472 - Downgrade NUnit adapter to fix discovery issues](https://github.com/ppy/osu/pull/7472)
*   更新整合完毕,游戏基础版本更新至2020.112.0
## 2020/1/15:
*   从官方pr处获取的更新:
    *   [#7528 - 允许在歌曲选择界面通过Alt键调整音量](https://github.com/ppy/osu/pull/7528)
    *   [#7532 - 实现在编辑器内保存对谱面的更改](https://github.com/ppy/osu/pull/7532)
    *   [#7543 - Fix user agent missing in registration request](https://github.com/ppy/osu/pull/7543)
    *   [#7535 - Implement exporting beatmap package as .osz](https://github.com/ppy/osu/pull/7535)