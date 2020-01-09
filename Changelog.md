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
*   字体增大
    *   [OsuTabControl](osu.Game/Graphics/UserInterface/OsuTabControl.cs)
    *   [OsuTabControlCheckbox](osu.Game/Screens/Edit/Components/Menus/EditorMenuBar.cs)
    *   [PageTabControl](osu.Game/Graphics/UserInterface/PageTabControl.cs)
    *   [TabControlOverlayHeader](osu.Game/Overlays/TabControlOverlayHeader.cs)
    *   [EditorMenuBar](osu.Game/Screens/Edit/Components/Menus/EditorMenuBar.cs)