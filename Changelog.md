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

*   更新上游pr
    *   [o] [Replace SocialOverlay with DashboardOverlay #805_1] 
            -[依赖]-> #8132
            -[依赖]-> #8288

*   优化了"关于Mf-osu"的界面排版。
*   修复了o!c输入翻译缺失的问题。
*   将StandAloneChatDisplay的Placeholder和ChatOverlay同步为"在这里输入你要发送的消息"