# 变更日志

# 详细信息:
## 2020/1/8:
### 游戏界面
*   [修正了比赛端的翻译错误](osu.Game.Tournament/TournamentSceneManager.cs)
*   [添加了开始界面的翻译](osu.Game/Screens/Menu/Disclaimer.cs)
*   [共1个界面的翻译改进](osu.Game/Overlays/Profile/Sections/BeatmapsSection.cs)
*   [共1个界面的翻译调整](osu.Game/Updater/SimpleUpdateManager.cs)
### 功能
*   从[官方pr处](https://github.com/ppy/osu/pull/5782)获取了转盘分数优化更新
### 其他
*   [更改`osu.Desktop.csproj`中的游戏版本为2020.104.0](osu.Desktop/osu.Desktop.csproj)
### 安装包构建
*   linux
    *   添加了amd64和i386架构的ffmpeg库