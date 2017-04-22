// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Configuration;
using osu.Framework.Platform;

namespace osu.Game.Configuration
{
    public class OsuConfigManager : ConfigManager<OsuConfig>
    {
        protected override void InitialiseDefaults()
        {
#pragma warning disable CS0612 // Type or member is obsolete

            Set(OsuConfig.Username, string.Empty);
            Set(OsuConfig.Token, string.Empty);

            Set(OsuConfig.Ruleset, 0, 0, int.MaxValue);

            Set(OsuConfig.AudioDevice, string.Empty);
            Set(OsuConfig.SavePassword, false);
            Set(OsuConfig.SaveUsername, true);

            Set(OsuConfig.MenuCursorSize, 1.0, 0.5f, 2);
            Set(OsuConfig.GameplayCursorSize, 1.0, 0.5f, 2);
            Set(OsuConfig.DimLevel, 30, 0, 100);

            Set(OsuConfig.MouseDisableButtons, false);
            Set(OsuConfig.MouseDisableWheel, false);

            Set(OsuConfig.SnakingInSliders, true);
            Set(OsuConfig.SnakingOutSliders, true);

            Set(OsuConfig.MenuParallax, true);

            Set(OsuConfig.ShowInterface, true);
            Set(OsuConfig.KeyOverlay, false);
            //todo: implement all settings below this line (remove the Disabled set when doing so).

            Set(OsuConfig.AudioOffset, 0, -500.0, 500.0);

            Set(OsuConfig.MouseSpeed, 1.0).Disabled = true;
            Set(OsuConfig.BeatmapDirectory, @"Songs").Disabled = true; // TODO: use thi.Disabled = trues
            Set(OsuConfig.AllowPublicInvites, true).Disabled = true;
            Set(OsuConfig.AutoChatHide, true).Disabled = true;
            Set(OsuConfig.AutomaticDownload, true).Disabled = true;
            Set(OsuConfig.AutomaticDownloadNoVideo, false).Disabled = true;
            Set(OsuConfig.BlockNonFriendPM, false).Disabled = true;
            Set(OsuConfig.Bloom, false).Disabled = true;
            Set(OsuConfig.BloomSoftening, false).Disabled = true;
            Set(OsuConfig.BossKeyFirstActivation, true).Disabled = true;
            Set(OsuConfig.ChatAudibleHighlight, true).Disabled = true;
            Set(OsuConfig.ChatChannels, string.Empty).Disabled = true;
            Set(OsuConfig.ChatFilter, false).Disabled = true;
            Set(OsuConfig.ChatHighlightName, true).Disabled = true;
            Set(OsuConfig.ChatMessageNotification, true).Disabled = true;
            Set(OsuConfig.ChatLastChannel, string.Empty).Disabled = true;
            Set(OsuConfig.ChatRemoveForeign, false).Disabled = true;
            //Set(OsuConfig.ChatSortMode, UserSortMode.Rank).Disabled = true;
            Set(OsuConfig.ComboBurst, true).Disabled = true;
            Set(OsuConfig.ComboFire, false).Disabled = true;
            Set(OsuConfig.ComboFireHeight, 3).Disabled = true;
            Set(OsuConfig.ConfirmExit, false).Disabled = true;
            Set(OsuConfig.AutoSendNowPlaying, true).Disabled = true;
            Set(OsuConfig.AutomaticCursorSizing, false).Disabled = true;
            Set(OsuConfig.Display, 1).Disabled = true;
            Set(OsuConfig.DisplayCityLocation, false).Disabled = true;
            Set(OsuConfig.DistanceSpacingEnabled, true).Disabled = true;
            Set(OsuConfig.EditorTip, 0).Disabled = true;
            Set(OsuConfig.VideoEditor, true).Disabled = true;
            Set(OsuConfig.EditorDefaultSkin, false).Disabled = true;
            Set(OsuConfig.EditorSnakingSliders, true).Disabled = true;
            Set(OsuConfig.EditorHitAnimations, false).Disabled = true;
            Set(OsuConfig.EditorFollowPoints, true).Disabled = true;
            Set(OsuConfig.EditorStacking, true).Disabled = true;
            Set(OsuConfig.ForceSliderRendering, false).Disabled = true;
            Set(OsuConfig.FpsCounter, false).Disabled = true;
            Set(OsuConfig.FrameTimeDisplay, false).Disabled = true;
            Set(OsuConfig.GuideTips, @"").Disabled = true;
            Set(OsuConfig.CursorRipple, false).Disabled = true;
            Set(OsuConfig.HighlightWords, string.Empty).Disabled = true;
            Set(OsuConfig.HighResolution, false).Disabled = true;
            Set(OsuConfig.HitLighting, true).Disabled = true;
            Set(OsuConfig.IgnoreBarline, false).Disabled = true;
            Set(OsuConfig.IgnoreBeatmapSamples, false).Disabled = true;
            Set(OsuConfig.IgnoreBeatmapSkins, false).Disabled = true;
            Set(OsuConfig.IgnoreList, string.Empty).Disabled = true;
            Set(OsuConfig.Language, @"unknown").Disabled = true;
            Set(OsuConfig.AllowNowPlayingHighlights, false).Disabled = true;
            Set(OsuConfig.LastVersion, string.Empty).Disabled = true;
            Set(OsuConfig.LastVersionPermissionsFailed, string.Empty).Disabled = true;
            Set(OsuConfig.LoadSubmittedThread, true).Disabled = true;
            Set(OsuConfig.LobbyPlayMode, -1).Disabled = true;
            Set(OsuConfig.ShowInterfaceDuringRelax, false).Disabled = true;
            Set(OsuConfig.LobbyShowExistingOnly, false).Disabled = true;
            Set(OsuConfig.LobbyShowFriendsOnly, false).Disabled = true;
            Set(OsuConfig.LobbyShowFull, false).Disabled = true;
            Set(OsuConfig.LobbyShowInProgress, true).Disabled = true;
            Set(OsuConfig.LobbyShowPassworded, true).Disabled = true;
            Set(OsuConfig.LogPrivateMessages, false).Disabled = true;
            Set(OsuConfig.LowResolution, false).Disabled = true;
            //Set(OsuConfig.ManiaSpeed, SpeedMania.SPEED_DEFAULT, SpeedMania.SPEED_MIN, SpeedMania.SPEED_MAX).Disabled = true;
            Set(OsuConfig.UsePerBeatmapManiaSpeed, true).Disabled = true;
            Set(OsuConfig.ManiaSpeedBPMScale, true).Disabled = true;
            Set(OsuConfig.MenuTip, 0).Disabled = true;
            Set(OsuConfig.MouseSpeed, 1, 0.4, 6).Disabled = true;
            Set(OsuConfig.ScoreMeterScale, 1, 0.5, 2).Disabled = true;
            //Set(OsuConfig.ScoreMeterScale, 1, 0.5, OsuGame.Tournament ? 10 : 2).Disabled = true;
            Set(OsuConfig.DistanceSpacing, 0.8, 0.1, 6).Disabled = true;
            Set(OsuConfig.EditorBeatDivisor, 1, 1, 16).Disabled = true;
            Set(OsuConfig.EditorGridSize, 32, 4, 32).Disabled = true;
            Set(OsuConfig.EditorGridSizeDesign, 32, 4, 32).Disabled = true;
            Set(OsuConfig.CustomFrameLimit, 240, 240, 999).Disabled = true;
            Set(OsuConfig.MsnIntegration, false).Disabled = true;
            Set(OsuConfig.MyPcSucks, false).Disabled = true;
            Set(OsuConfig.NotifyFriends, true).Disabled = true;
            Set(OsuConfig.NotifySubmittedThread, true).Disabled = true;
            Set(OsuConfig.PopupDuringGameplay, true).Disabled = true;
            Set(OsuConfig.ProgressBarType, ProgressBarType.Pie).Disabled = true;
            Set(OsuConfig.RankType, RankingType.Top).Disabled = true;
            Set(OsuConfig.RefreshRate, 60).Disabled = true;
            Set(OsuConfig.OverrideRefreshRate, Get<int>(OsuConfig.RefreshRate) != 60).Disabled = true;
            //Set(OsuConfig.ScaleMode, ScaleMode.WidescreenConservative).Disabled = true;
            Set(OsuConfig.ScoreboardVisible, true).Disabled = true;
            Set(OsuConfig.ScoreMeter, ScoreMeterType.Error).Disabled = true;
            //Set(OsuConfig.ScoreMeter, OsuGame.Tournament ? ScoreMeterType.Colour : ScoreMeterType.Error).Disabled = true;
            Set(OsuConfig.ScreenshotId, 0).Disabled = true;
            Set(OsuConfig.MenuSnow, false).Disabled = true;
            Set(OsuConfig.MenuTriangles, true).Disabled = true;
            Set(OsuConfig.SongSelectThumbnails, true).Disabled = true;
            Set(OsuConfig.ScreenshotFormat, ScreenshotFormat.Jpg).Disabled = true;
            Set(OsuConfig.ShowReplayComments, true).Disabled = true;
            Set(OsuConfig.ShowSpectators, true).Disabled = true;
            Set(OsuConfig.ShowStoryboard, true).Disabled = true;
            //Set(OsuConfig.Skin, SkinManager.DEFAULT_SKIN).Disabled = true;
            Set(OsuConfig.SkinSamples, true).Disabled = true;
            Set(OsuConfig.SkipTablet, false).Disabled = true;
            Set(OsuConfig.Tablet, false).Disabled = true;
            Set(OsuConfig.UpdatePending, false).Disabled = true;
            Set(OsuConfig.UseSkinCursor, false).Disabled = true;
            Set(OsuConfig.UseTaikoSkin, false).Disabled = true;
            Set(OsuConfig.Video, true).Disabled = true;
            Set(OsuConfig.Wiimote, false).Disabled = true;
            Set(OsuConfig.YahooIntegration, false).Disabled = true;
            Set(OsuConfig.ForceFrameFlush, false).Disabled = true;
            Set(OsuConfig.DetectPerformanceIssues, true).Disabled = true;
            Set(OsuConfig.MenuMusic, true).Disabled = true;
            Set(OsuConfig.MenuVoice, true).Disabled = true;
            Set(OsuConfig.RawInput, false).Disabled = true;
            Set(OsuConfig.AbsoluteToOsuWindow, Get<bool>(OsuConfig.RawInput)).Disabled = true;
            Set(OsuConfig.ShowMenuTips, true).Disabled = true;
            Set(OsuConfig.HiddenShowFirstApproach, true).Disabled = true;
            Set(OsuConfig.ComboColourSliderBall, true).Disabled = true;
            Set(OsuConfig.AlternativeChatFont, false).Disabled = true;
            Set(OsuConfig.DisplayStarsMaximum, 10.0, 0.0, 10.0).Disabled = true;
            Set(OsuConfig.DisplayStarsMinimum, 0.0, 0.0, 10.0).Disabled = true;
            Set(OsuConfig.ReleaseStream, ReleaseStream.Lazer).Disabled = true;
            Set(OsuConfig.UpdateFailCount, 0).Disabled = true;
            //Set(OsuConfig.TreeSortMode, TreeGroupMode.Show_All).Disabled = true;
            //Set(OsuConfig.TreeSortMode2, TreeSortMode.Title).Disabled = true;
            bool unicodeDefault = false;
            switch (Get<string>(OsuConfig.Language))
            {
                case @"zh":
                case @"ja":
                case @"ko":
                    unicodeDefault = true;
                    break;
            }
            Set(OsuConfig.ShowUnicode, unicodeDefault);
            Set(OsuConfig.PermanentSongInfo, false).Disabled = true;
            Set(OsuConfig.Ticker, false).Disabled = true;
            Set(OsuConfig.CompatibilityContext, false).Disabled = true;
            Set(OsuConfig.CanForceOptimusCompatibility, true).Disabled = true;
            Set(OsuConfig.ConfineMouse, Get<bool>(OsuConfig.ConfineMouseToFullscreen) ?
                ConfineMouseMode.Fullscreen : ConfineMouseMode.Never).Disabled = true;


            GetOriginalBindable<bool>(OsuConfig.SavePassword).ValueChanged += delegate
            {
                if (Get<bool>(OsuConfig.SavePassword)) Set(OsuConfig.SaveUsername, true);
            };
            GetOriginalBindable<bool>(OsuConfig.SaveUsername).ValueChanged += delegate
            {
                if (!Get<bool>(OsuConfig.SaveUsername)) Set(OsuConfig.SavePassword, false);
            };
#pragma warning restore CS0612 // Type or member is obsolete
        }

        public OsuConfigManager(Storage storage) : base(storage)
        {
        }
    }

    public enum OsuConfig
    {
        // New osu:
        Ruleset,
        Token,
        // Imported from old osu:
        BeatmapDirectory,
        AllowPublicInvites,
        AutoChatHide,
        AutomaticDownload,
        AutomaticDownloadNoVideo,
        BlockNonFriendPM,
        Bloom,
        BloomSoftening,
        BossKeyFirstActivation,
        ChatAudibleHighlight,
        ChatChannels,
        ChatFilter,
        ChatHighlightName,
        ChatMessageNotification,
        ChatLastChannel,
        ChatRemoveForeign,
        ChatSortMode,
        ComboBurst,
        ComboFire,
        ComboFireHeight,
        ConfirmExit,
        AutoSendNowPlaying,
        MenuCursorSize,
        GameplayCursorSize,
        AutomaticCursorSizing,
        DimLevel,
        Display,
        DisplayCityLocation,
        DistanceSpacingEnabled,
        EditorTip,
        VideoEditor,
        EditorDefaultSkin,
        EditorSnakingSliders,
        EditorHitAnimations,
        EditorFollowPoints,
        EditorStacking,
        ForceSliderRendering,
        FpsCounter,
        FrameTimeDisplay,
        GuideTips,
        CursorRipple,
        HighlightWords,
        HighResolution,
        HitLighting,
        IgnoreBarline,
        IgnoreBeatmapSamples,
        IgnoreBeatmapSkins,
        IgnoreList,
        KeyOverlay,
        Language,
        LastPlayMode,
        AllowNowPlayingHighlights,
        LastVersion,
        LastVersionPermissionsFailed,
        LoadSubmittedThread,
        LobbyPlayMode,
        ShowInterface,
        ShowInterfaceDuringRelax,
        LobbyShowExistingOnly,
        LobbyShowFriendsOnly,
        LobbyShowFull,
        LobbyShowInProgress,
        LobbyShowPassworded,
        LogPrivateMessages,
        LowResolution,
        ManiaSpeed,
        UsePerBeatmapManiaSpeed,
        ManiaSpeedBPMScale,
        MenuTip,
        MouseDisableButtons,
        MouseDisableWheel,
        MouseSpeed,
        AudioOffset,
        ScoreMeterScale,
        DistanceSpacing,
        EditorBeatDivisor,
        EditorGridSize,
        EditorGridSizeDesign,
        CustomFrameLimit,
        MsnIntegration,
        MyPcSucks,
        NotifyFriends,
        NotifySubmittedThread,
        PopupDuringGameplay,
        ProgressBarType,
        RankType,
        RefreshRate,
        OverrideRefreshRate,
        ScaleMode,
        ScoreboardVisible,
        ScoreMeter,
        ScreenshotId,
        MenuSnow,
        MenuTriangles,
        SongSelectThumbnails,
        ScreenshotFormat,
        ShowReplayComments,
        ShowSpectators,
        ShowStoryboard,
        Skin,
        SkinSamples,
        SkipTablet,
        SnakingInSliders,
        SnakingOutSliders,
        Tablet,
        UpdatePending,
        UserFilter,
        UseSkinCursor,
        UseTaikoSkin,
        Video,
        Wiimote,
        YahooIntegration,
        ForceFrameFlush,
        DetectPerformanceIssues,
        MenuMusic,
        MenuVoice,
        MenuParallax,
        RawInput,
        AbsoluteToOsuWindow,
        ConfineMouse,
        [Obsolete]
        ConfineMouseToFullscreen,
        ShowMenuTips,
        HiddenShowFirstApproach,
        ComboColourSliderBall,
        AlternativeChatFont,
        Username,
        DisplayStarsMaximum,
        DisplayStarsMinimum,
        AudioDevice,
        ReleaseStream,
        UpdateFailCount,
        SavePassword,
        SaveUsername,
        TreeSortMode,
        TreeSortMode2,
        ShowUnicode,
        PermanentSongInfo,
        Ticker,
        CompatibilityContext,
        CanForceOptimusCompatibility,

    }
}
