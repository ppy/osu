# 帮助: 
##   官方pr界面获取更新
*   任何以 `[M]` 开头的均为已合并至`ppy:master`的pr, 不需要在之后进行检查
*   任何以 `[O]` 开头的均为仍在开放中的pr, 需要不定义检查是否有更新
# 变更日志
### 2020/03/07
*   合并上游pr
    *   [m] [Ensure tournament screens respect aspect ratio in tests #8148]
    *   [m] [Add a short load delay for avatars to avoid unnecessary fetching #8149]
    *   [m] [Remove layout durations from tournament editor screens for better performance #8150]
    *   [m] [Simplify tournament video construction #8151]
    *   [m] [Fix test scene virtual track not respecting rate adjustments #8100]
    *   [m] [Fix osu! hitbox accepting input outside of circle #8163]

### 2020/03/09
*   合并上游pr
    *   [m] [Adjust most played beatmaps section to better match osu-web #8047]
    *   [m] [Fix video looping not propagating when set too early in initialisation #8178]
    *   [m] [Increase flexibility of StarCounter component #8175]
    *   [m] [Fix hit error ticks getting out of the hit error meter #8182]
    *   [m] [Fix textbox characters not animating when typing/backspacing #8186]
    *   同步Tournament相关代码,涉及pr:
        *   [m] [Apply tournament client usability changes #8170]
        *   [m] [Add new tournament design base elements #8171]
        *   [m] [Implement 2020 map pool design #8173]
        *   [m] [Implement 2020 ladder design #8174]
        *   [m] [Implement 2020 win screen design #8179]

### 2020/03/10
*   添加了一些东西,详见文件变更列表
*   合并上游pr
    *   [m] [Implement 2020 gameplay design #8176]
    *   [m] [Implement 2020 schedule design #8177]
    *   [m] [Update framework #8193]
    *   [m] [Add idle animation and fallback catcher support #8196]
    *   [m] [Fix hyperdash not initiating correctly when juice streams are present #8192]
    *   [m] [Fix osu! shaking instead of missing for early hits #8197]
*   **添加了"关于Mf-osu"界面**
*   **实现了实时显示系统时间的功能**

### 2020/03/11:
*   合并上游pr
    *   [m] [Fix crashes on some storyboards #8195]
    *   [m] [Fix perfect mod incorrectly failing in some scenarios #8084]
    *   [m] [Hide pp display on leaderboards if map is qualified or loved #7971]
    *   [m] [Use framework extension method for FromHex #8208]
    *   [m] [Fix osu!catch hitobjects appearing / fading too late #8204]
    *   [m] [Fix download failures potentially crashing game #8206]
    *   [m] [Don't play samples on catching a tiny droplet #8202]
    *   [m] [Fix hyperdash test having a zero-length juice stream #8201]
    *   [m] [Add afterimage glow when entering hyperdash #8200]
    *   [m] [Update text on disclaimer screen (and add tips) #8211]
    *   [m] [Reapply filters on next change after a forced beatmap display #8133]
    *   [m] [Fix changing ruleset at song select not scrolling the current selection back into view #8205]
    *   [m] [Implement FriendsOnlineStatusControl component #8108]
    *   [m] [Add catcher kiai/fail animation states #8198]
    *   [m] [Add random rotation and scale factors to osu!catch bananas #8199]
    *   [m] [Implement 2020 intro screen design #8180]

### 2020/03/12:
*   一些有关比赛端的小更新:
    *   [将`Bracket`翻译为`晋级榜图`,取消了原随机分队(seeding)的翻译](osu.Game.Tournament/TournamentSceneManager.cs)
    *   补全界面中的翻译
        *   [osu.Game.Tournament/Screens/Ladder/Components/DrawableMatchTeam.cs](osu.Game.Tournament/Screens/Ladder/Components/DrawableMatchTeam.cs)
        *   [osu.Game.Tournament/Screens/Ladder/Components/DrawableTournamentRound.cs](osu.Game.Tournament/Screens/Ladder/Components/DrawableTournamentRound.cs)
        *   [osu.Game.Tournament/Screens/Ladder/Components/LadderEditorSettings.cs](osu.Game.Tournament/Screens/Ladder/Components/LadderEditorSettings.cs)
        *   将seeding界面标题翻译为"选手介绍"
    *   优化界面中的翻译
        *   [osu.Game.Tournament/Screens/Schedule/ScheduleScreen.cs](osu.Game.Tournament/Screens/Schedule/ScheduleScreen.cs)
        *   [osu.Game.Tournament/Screens/TeamIntro/SeedingScreen.cs](osu.Game.Tournament/Screens/TeamIntro/SeedingScreen.cs)

*   合并上游pr
    *   [m] [Implement 2020 seeding screen design #8215]
    *   [m] [Use fixed width text for tournament score displays #8229]
    *   [m] [Disable adjusting volume via "select next" and "select previous" as fallbacks #8235]
    *   [m] [Allow videos to be loaded with any extension #8236]
    *   [m] [Add back dynamic components of tournament header #8237]
    *   [m] [Fix beatmap carousel tests loading beatmap manager beatmaps in test browser #8241]
    *   [m] [Apply osu!-side video sprite changes #8216]
    *   [m] [Fix catch beatmap processing not matching stable #8220]
    *   [m] [Add ability to set stable path for tourney client via environment variable #8226]
    *   [m] [Fix catcher showing miss sprite upon missing bananas #8230]
    *   [m] [Fallback on invalid AnimationFramerate for legacy skins #8245]

    *   [o] [Update loader animation #821_2]
    *   [o] [Replace SocialOverlay with DashboardOverlay #805_1]

*   补全翻译
    *   [补全了"该谱面暂时无法被下载"的翻译](osu.Game/Overlays/BeatmapSet/BeatmapAvailability.cs)
*   各种各样的修改

### 2020/03/13:
*   更新上游pr 
    *   [o] [Replace SocialOverlay with DashboardOverlay #805_1]
        *   更新翻译“仪表板”为“看板”(match osu-web)

*   合并上游pr
    *   [m] [Remove unlimited timing points in difficulty calculation #8219]
    *   [m] [Fix mod sprite bleeding border colour #8261]
    *   [m] [Expose half catcher width to movement skill #8256]
    *   [m] [Add more sane limit for maximum slider length #8217]

*   同步805_1的Tests

*   [优化了评论容器"显示更多"文字的字体大小](osu.Game/Overlays/Comments/GetCommentRepliesButton.cs)

### 2020/03/14:
*   各种修改

### 2020/03/15:
*   合并上游pr
    *   [m] [Split out CatcherArea nested classes and reorder methods #8257]
    *   [m] [Fix osu! slider ball tint colour being applied to follow circle #8267]
    *   [m] [Add resolution selector in tournament setup screen #8262]
    *   [m] [Switch game to use new font (Torus) #8259]

### 2020/03/16:
*   合并上游pr
    *   [m] [Switch to the next chat tab upon closing an active one #8270]
    *   [m] [Apply ruleset filter in all cases (even when bypassing filter for selection purposes) #8242]
    *   [m] [Improve beatmap carousel / song select fallback logic pathing #8246]
    *   [m] [Fix osu!catch trail animating (and displaying incorrect frame) #8213]
    *   [m] [Add the ability to click filtered difficulty icons #8247]
    *   [m] [Add the ability to click grouped beatmap difficulty icons #8248]
    *   [m] [Fix carousel scrolling being inoperable during beatmap import #8255]
    *   [m] [Update user panel design #8132]
    *   [m] [Use new logo name for showcase screen #8291]

*   更新上游pr
    *   [o] [Replace SocialOverlay with DashboardOverlay #8051] 
            -[依赖]-> #8132
            -[依赖]-> #8288

*   修复了o!c输入翻译缺失的问题。
*   将StandAloneChatDisplay的Placeholder和ChatOverlay同步为"在这里输入你要发送的消息"
*   优化了[比赛端图池界面](osu.Game.Tournament/Screens/MapPool/MapPoolScreen.cs)中谱面panel的高度
*   将[比赛端队伍编辑器界面](osu.Game.Tournament/Screens/Editors/TeamEditorScreen.cs)中的"编辑随机结果"改为"编辑选手介绍信息"
*   优化了"关于Mf-osu"的界面排版。

### 2020/03/17
*   优化了多处翻译
*   优化了"关于Mf-osu"的文本

*   合并上游pr
    *   [m] [Limit Torus font weight to bold #8300]
    *   [m] [Add beatmap loading timeout to prevent runaway loading scenarios #8260]
    *   [m] [Add black font weighting #8307]
    *   [m] [Implement a circle that displays the user's accuracy #8304]
    *   [m] [Update framework #8306]
    *   [m] [Fix header-text scaling on intro/winner screens #8301]
    *   [m] [Automatically mark the currently selected match as started on entering gameplay screen #8302]
    *   [m] [Fix replay scores not being populated via player #8305]
    *   [m] [Update framework #8306]
    *   [m] [Implement the results screen score panel #8308]
    *   [m] [Implement a circle that displays the user's accuracy #8304]
    *   [m] [Implement the top score panel contents #8309]
    *   [m] [Implement the middle score panel contents #8310]
    *   **[m] [实现新的结算页面 #8311]**

### 2020/03/19
*   合并上游pr
    *   [m] [Fix mapper name in score panel #8318]
    *   [m] [Fix results' beatmap title and artist language setting being swapped #8334]
    *   [m] [Fix mapper info alignment in score panel #8320]
    *   [m] [Update loader animation #8212]
    *   [m] [Update incorrect file path causing error on rider solution load #8335]
    *   [m] [Fix perfect display showing when misses are present #8336]
    *   [m] [Re-use colors defined for each rank in result screen accuracy circle #8323]
    *   [m] [Colourise results screen hit statistics #8338]
    *   [m] [Add date played to score panel #8340]
    *   [m] [Update rank badge colours #8339]
    *   [m] [Make score panel scroll if off-screen #8341]
    *   [m] [Show 'D' rank badge on accuracy circle #8349]
    *   [m] [Add testflight distribution step automation #8345]
    *   [m] [Reduce allocations of followpoints by reusing existing #8346]
    *   [m] [Update framework #8360]

*   暂不合并
    *   [m] [Chat overlay test scene improvements #8297]

### 2020/03/20
*   合并上游pr
    *   [m] [Fix potentially invalid push in player while already exiting #8355]
    *   [m] [Fix slider ticks/repeats contributing to accuracy #8362]
    *   [m] [Remove slider implicit judgement #8358]

*   添加了在休息时段，暂停界面和死亡界面直接调整视觉设置的功能
*   优化关于界面的文案

### 2020/03/23
*   合并上游pr
    *   [m] [Don't open profile if it's Autoplay #8374]
    *   [m] [Fix beat divisor control selecting invalid divisors on drag end #8329]
    *   [m] [Add better flow logic to map pool layout when few beatmaps are present #8172]
    *   [m] [Fix carousel not returning to previous selection after filter #8368]
    *   [m] [Fix selection not occurring when switching away from empty ruleset #8370]
    *   [m] [Increase sample concurrency to better match stable #8389]
    *   [m] [Fix very long spinners degrading in performance #8394]
    *   [m] [Hide scrollbars in tournament chat display #8404]
    *   [m] [Add ability to adjust (and save) chroma-key area width #8403]
    *   [m] [Fix some pieces of SettingsItem getting dimmed twice when disabled #8408]
    *   [m] [Remove taiko CentreHit/RimHit hitobject abstraction #8405]
    *   [m] [Bump Sentry from 2.1.0 to 2.1.1 #8410]
    *   [m] [Implement FriendDisplay component #8288]

*   更新上游pr
    *   [o] [Replace SocialOverlay with DashboardOverlay #8051] 

*   暂不合并
    *   [m] [Fix test scene potentially missing dependencies #8402]

### 2020/3/24
*   合并上游pr
    *   [m] [Fix crash when holding a key down while entering player #8409]
    *   [m] [Fix autoplay keyboard shortcut not working with keypad enter key #8415]
    *   [m] [Fix song select filter and footer not absorbing input from carousel #8414]
    *   [m] [Implement random mod for taiko #8406]
    *   [m] [Add replay recorder functionality #8427]
    *   [m] [Add local replay support for all rulesets #8428]

### 2020/3/25
*   合并上游pr
    *   [m] [Allow individual storyboard layers to disable masking #8426]
    *   [m] [Start background video playback based on provided offset #8167]
    *   [m] [Remove unused text transform helpers #8430]
    *   [m] [Fix track looping state not being reset when entering editor from song select #8433]
    *   [m] [Fix intro tests not asserting pass or working at all #8431]
    *   [m] [Fix beat divisor control selecting invalid divisors on drag end #8329]
*   实现了tau模式的回放功能

### 2020/3/26
*   合并上游pr
    *   [m] [Fix last seen date being visible in user panel when it shouldn't #8441]
*   关于页面文案调整

### 2020/3/27
*   合并上游pr
    *   [m] [Fix break overlay scaling with gameplay #8447]
    *   [m] [Reduce spread of stacked fruit #8450]
    *   [m] [Make slider judgements count towards base score / accuracy #8452]
    *   [m] [Only play slider end sounds if tracking #8451]
    *   [m] [Fix osu!mania replays recording incorrectly when key mod applied #8461]
    *   [m] [Disable raw input toggle on all but windows #8407]
    *   [m] [Fix NullReferenceException when starting the no-video version of a beatmap with video #8455]
    *   [m] [Update framework #8467]

### 2020/3/28
*   合并上游pr
    *   [m] [Improve robustness of loader tests #8476]
    *   [m] [Fix break overlay not displaying progress information #8474]
    *   [m] [Fix break overlay displaying in front of all other player overlays #8475]
    *   [m] [Show customised mod setting values in tooltip #8351]

*   修复UI问题

* 补全了"启用Mf自定义UI"的功能, 目前该选项将影响下列界面的样式/动画:
    *   可见性变更
        *   结算界面 背景动画
        *   主界面 "一个神秘的按钮"
        *   顶栏 "关于Mf-osu"和"时间"按钮
        *   顶栏 右侧除"通知"按钮以外所有按钮描述
        *   歌曲选择 底栏背景动画
        *   歌曲选择 歌曲排行榜背景动画
        *   多人游戏 房间排行榜背景动画
        *   游戏内 休息时段、失败、暂停时视觉效果设置菜单的
    *   动画效果变更
        *   音乐播放器切歌动画
        *   结算菜单的视差效果动画

* **!!! [Toolbar](osu.Game/Overlays/Toolbar/Toolbar.cs)过渡动画相关逻辑仍需改进 !!!**

### 2020/3/30
*   合并上游pr
    *   [m] [Rewrite beatmap carousel's select next logic to not use drawables #8481]
    *   [m] [Fix auto mod results screen not displaying correctly #8488]
    *   [m] [Implement Spun Out mod #7764]
    *   [m] [Add support for legacy skin sliderstartcircle / sliderstartcircleoverlay #8477]
    *   [m] [Add mania skin decoder #8496]
    *   [m] [Make reverse arrows not follow snaking when they are already hit #8486]
    *   [m] [Fix tooltips not showing inside ManualInputManagerTestScenes #8501]
    *   [m] [Bump Microsoft.Build.Traversal from 2.0.24 to 2.0.32 #8499]
    *   [m] [Hide "retry" button on results screen after watching a replay #8490]
    *   [m] [Fix imports with no matching beatmap IDs still retaining a potentially invalid set ID #8494]

*   更新上游pr
    *   [o] [Replace SocialOverlay with DashboardOverlay #8051] 

*   暂不合并
    *   [m] [Move non-headless tests to correct namespace #8493]

*   同步Tests

### 2020/3/31
*   合并上游pr
    *   [m] [Add support for HitCircleOverlayAboveNumber legacy skin property #8502]
    *   [m] [Fix replay imports failing for certain mod combinations #8525]
    *   [m] [Support widescreen per-layer storyboard masking #8509]
    *   [m] [Fix relax mod pressing too many keys #8522]
    *   [m] [Fix osu!catch catcher hit area being too large #8526]
    *   [m] [Remove ScaleDownToFit as it was implemented without enough safety #8521]

    *   [m] [Add mania key area skinning #8516]
    *   [m] [Implement basis for mania skinning #8513]
    *   [m] [Add mania column background skinning #8514]
    *   [m] [Add mania hit target skinning #8518]
    *   [m] [Add mania note skinning #8523]

*   暂不合并
    *   [m] [Fix catcher test resources being at wrong dpi definition #8520]

### 2020/04/01
*   合并上游pr
    *   [m] [Add mania hold note skinning #8524]
    *   [m] [Implement column width/spacing #8536]
    *   [m] [Fix barlines scrolling at different speeds in legacy skins #8538]
    *   [m] [Fix column lights positioned incorrectly #8537]
    *   [m] [Update framework #8544]
    *   [m] [Fix incorrect explosion position on default skin #8541]

### 2020/04/02
*   合并上游pr
    *   [m] [Add check to detect whether mania is skinned #8535]

*   合并上游pr
    *   [m] [Add startAtCurrentTime parameter to GetAnimation() #8555]
    *   [m] [Allow hold note tail to fallback to normal note image #8562]
    *   [m] [Fix crash caused by user json order changing #8566]
    *   [m] [Fix weird slider ball sizing #8568]
    *   [m] [Update framework #8570]

### 2020/04/03
*   合并上游pr
    *   [m] [Fix hold note animation not being reset #8564]
    *   [m] [Expand mania to fit vertical screen bounds #8563]
    *   [m] [Add skinning support for column line colour #8565]
    *   [m] [Implement mania normal-note hit-explosion skinning #8556]
    *   [m] [Fix GetDecoder getting fallback decoder too often #8584]

### 2020/04/04
*   合并上游pr
    *   [m] [Fix dynamic recompilation in intro test scenes #8590]
    *   [m] [Update usages of Animation and Video in line with framework changes #8592]
    *   [m] [Rework mania skin lookups to not require total playfield columns #8586]
    *   [m] [Fix mania scrolling at incorrect speeds #8589]
    *   [m] [Implement more familiar scroll speed options in mania #8597]
    *   [m] [Support HitCircleOverlayAboveNumer typo for old legacy skins #8602]
    *   [m] [Fix results star rating display not being centered when no mods are present #8603(没有commit?)]

### 2020/04/06
*   合并上游pr
    *   [o] [Load user rulesets from the game data directory #8607]
    *   [m] [Fix performance when parsing mania skins #8641]
    *   [m] [Update framework #8644]
    *   [m] [Bump SharpCompress from 0.24.0 to 0.25.0 #8639]
    *   [m] [Bump Microsoft.Build.Traversal from 2.0.32 to 2.0.34 #8638]

*   暂不合并
    *   [o] [Write a test for slider snaking #8489]

### 2020/04/07
*   合并上游pr
    *   [m] [Make legacy skins use startAtCurrentTime by default #8613]
    *   [m] [Add a simple constructor for BreakPeriod #8630]
    *   [m] [Fix results star rating display not being centered when no mods are present #8624]
    *   [m] [Fix sliderball accent colour not being set correctly #8619]
    *   [m] [Fix storyboard videos being offset incorrectly #8648]
    *   [m] [Fix SkinnableTestScene losing test resources on dynamic recompilation #8650]
    *   [m] [Make version-less skins fallback to version 1.0 #8643]
    *   [m] [!不完全合并] [Retrieve dll resources using a more reliable method #8660]
    *   [m] [Make note height scale by minimum column width #8652]
    *   [m] [Fix inconsistent scroll speeds in mania #8653]
    *   [m] [Implement mania note + key image configs #8642]
    *   [m] [Implement mania judgement line/column background/column light colours #8657]
    *   [o] [Add top rank to the beatmap carousel #7639]

*   更新上游pr
    *   [o] [Load user rulesets from the game data directory #8607]

*   更新了一个test文件
*   翻译改进
    *   RankArchived: "达成的排名" -> "取得的成绩"
*   更新framework至2020.407.0以修复storyboard尺寸问题

### 2020/04/08
*   合并上游pr
    *   [m] [Fix legacy skin texture fallback logic #8669]
    *   [m] [Increase size of default osu!mania skin's keys to allow clearance with HUD #8674]
    *   [m] [Fix dragging tournament ladder too far causing it to disappear #8673]
    *   [m] [Add osu!taiko drum skinning support #8598]

### 2020/04/09
*   合并上游pr
    *   [m] [!未合并tests] [Fix TestSceneColumn columns not getting a width #8679]
    *   [m] [Fix slider ball and follow circle blending for legacy skins #8680]
    *   [m] [!未合并tests] [Fix osu!taiko input drum alignment for old skin versions #8681]
    *   [m] [!未合并tests] [Fix hidden notes due to 0 minimum width #8677]
    *   [m] [Add top rank to the beatmap carousel #7639]
    *   [m] [Don't allow new transformations for reverse arrow after it's hit #8626]
    *   [m] [Implement mania stage bottom/left/right images #8676]
    *   [m] [Update overlay header elements to match osu-web #8454]

*   翻译调整
*   界面bug修复

### 2020/04/11
*   合并上游pr
    *   [m] [Fix possible legacy beatmap encoder nullref #8694]
    *   [m] [Fix crash when trying to edit long beatmaps #8695]
    *   [m] [!未合并tests] [Cleanup handling of hitobject updates #8693]
    *   [m] [!未合并tests] [Fix EditorBeatmap potentially not updating hitobjects #8703]

### 2020/04/12
*   合并上游pr
    *   [m] [Update framework #8712]
    *   [m] [!未合并tests] [Block out-of-order judgements from occurring (aka "note lock") #8337]

*   使[OsuButton](osu.Game/Graphics/UserInterface/OsuButton.cs)可以使用自定义字体大小
*   多人联机按钮细节修改
*   缓解了多人联机房间列表的对齐问题

### 2020/04/13
*   合并上游pr
    *   [m] [Remove unused changelog comments class #8733]
    *   [m] [Make beatmap info overlay present selected difficulty #8731]
    *   [m] [Implement OverlayScrollContainer component #8471]
    *   [m] [Add stereo shifted hitsound playback support #8699]
    *   [m] [Bump BenchmarkDotNet from 0.12.0 to 0.12.1 #8739]
    *   [m] [Add undo/redo support to the Editor #8696]
    *   [m] [Add basic taiko "hit" skinning support #8711]
    *   [m] [Fix connections hidden due to overlapping controlpoints #8737]
    *   [m] [Add change state support to more Editor components #8697]
    *   [m] [Use OverlayScrollContainer for overlays #8740]
    *   [m] [Limit upper number of editor beatmap states saved to 50 #8741]
    *   [m] [Rework slider control point placement to improve path progression #8736]

*   Mf自定义UI
    *   新增 : 优化歌曲加载界面效果
        *   **!!! 需要想办法让`LogoTrackingContainer`在`bg`不可见时追踪他 !!!**
        *   更新 : 使`LogoTrackContainer`不再瞬移

*   翻译文本优化

### 2020/4/14
*   `Triangles`兼容性修复
*   修复`Triangles`在未给定`IgnoreSettings`时透明度不正确的问题
*   修复`三角形粒子动画`设置无效的问题
*   继续调整歌曲加载界面动画


*   合并上游pr
    *   [m] [Fix scoring in classic mode not awarding exact numerical value for judgement #8750]
    *   [m] [Implement "prefer no-video" option #8716]
    *   [m] [Fix beatmap background not displaying when video is present #8751]
    *   [m] [Add support for testing arbitrary API requests/responses via Dummy API #8714]
    *   [m] [Make beatmap carousel select recommended difficulties #8444]
    *   [m] [Fade playfield to red when player health is low #8312]

*   暂不合并
    *   [m] [Mark dummy api test scene as headless #8752]

### 2020/4/25
*   继续调整动画
*   翻译补全