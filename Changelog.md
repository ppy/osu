# 帮助: 
##   官方pr界面获取更新
*   任何以 `[M]` 开头的均为已合并至`ppy:master`的pr, 不需要在之后进行检查
*   任何以 `[O]` 开头的均为仍在开放中的pr, 需要不定义检查是否有更新
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
    *   [O] [#7317 - 添加"打击结果"误差条](https://github.com/ppy/osu/pull/7317)
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
//////////////////////////////////////////////////////
## 2020/1/11:
*   从官方pr处获取的更新
    *   [?] [#7492 - 向主界面添加当前播放的音乐名](https://github.com/ppy/osu/pull/7492)
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
    *   [M] [#7534 - 修复tooltip语法](https://github.com/ppy/osu/pull/7534)

## 2020/1/20:
*   添加了游戏Mods的翻译
*   [将游戏Mods按钮文本字体变大](osu.Game/Overlays/Mods/ModButton.cs)
*   整合更新
    *   [M] [#7561 - 修复在比赛段的一个硬崩溃如果一局中包含一个空谱面](https://github.com/ppy/osu/pull/7561)
    *   [M] [#7564 - Update license year](https://github.com/ppy/osu/pull/7564)

## 2020/1/22:
*   整合更新
    *   [M] [#7582 - Allow parsing hex colour codes with alpha](https://github.com/ppy/osu/pull/7582)

## 2020/1/24
*   整合更新
    *   [M] [#7585, #7587 - Apply OnRelease method signature refactorings, 更新framework]()
        * [#7585](https://github.com/ppy/osu/pull/7585) 
        * [#7587](https://github.com/ppy/osu/pull/7587)