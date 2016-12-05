//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Configuration;
using osu.Framework.Platform;
using osu.Game.Modes;

namespace osu.Game.Configuration
{
    public class OsuConfigManager : ConfigManager<OsuConfig>
    {
        protected override void InitialiseDefaults()
        {
#pragma warning disable CS0612 // Type or member is obsolete
            Set(OsuConfig.Width, 1366, 640);
            Set(OsuConfig.Height, 768, 480);
            Set(OsuConfig.MouseSpeed, 1.0);

            Set(OsuConfig.Username, string.Empty);
            Set(OsuConfig.Password, string.Empty);
            Set(OsuConfig.Token, string.Empty);

            Set(OsuConfig.PlayMode, PlayMode.Osu);
            
            Set(OsuConfig.BeatmapDirectory, @"Songs"); // TODO: use this
            
            Set(OsuConfig.VolumeUniversal, 0.8, 0, 1);
            Set(OsuConfig.VolumeMusic, 1.0, 0, 1);
            Set(OsuConfig.VolumeEffect, 1.0, 0, 1);
            
            Set(OsuConfig.AllowPublicInvites, true);
            Set(OsuConfig.AutoChatHide, true);
            Set(OsuConfig.AutomaticDownload, true);
            Set(OsuConfig.AutomaticDownloadNoVideo, false);
            Set(OsuConfig.BlockNonFriendPM, false);
            Set(OsuConfig.BloomSoftening, false);
            Set(OsuConfig.BossKeyFirstActivation, true);
            Set(OsuConfig.ChatAudibleHighlight, true);
            Set(OsuConfig.ChatChannels, string.Empty);
            Set(OsuConfig.ChatFilter, false);
            Set(OsuConfig.ChatHighlightName, true);
            Set(OsuConfig.ChatMessageNotification, true);
            Set(OsuConfig.ChatLastChannel, string.Empty);
            Set(OsuConfig.ChatRemoveForeign, false);
            //Set(OsuConfig.ChatSortMode, UserSortMode.Rank);
            Set(OsuConfig.ComboBurst, true);
            Set(OsuConfig.ComboFire, false);
            Set(OsuConfig.ComboFireHeight, 3);
            Set(OsuConfig.ConfirmExit, false);
            Set(OsuConfig.AutoSendNowPlaying, true);
            Set(OsuConfig.CursorSize, 1.0, 0.5f, 2);
            Set(OsuConfig.AutomaticCursorSizing, false);
            Set(OsuConfig.DimLevel, 30, 0, 100);
            Set(OsuConfig.Display, 1);
            Set(OsuConfig.DisplayCityLocation, false);
            Set(OsuConfig.DistanceSpacingEnabled, true);
            Set(OsuConfig.EditorTip, 0);
            Set(OsuConfig.VideoEditor, Get<bool>(OsuConfig.Fullscreen));
            Set(OsuConfig.EditorDefaultSkin, false);
            Set(OsuConfig.EditorSnakingSliders, true);
            Set(OsuConfig.EditorHitAnimations, false);
            Set(OsuConfig.EditorFollowPoints, true);
            Set(OsuConfig.EditorStacking, true);
            Set(OsuConfig.ForceSliderRendering, false);
            Set(OsuConfig.FpsCounter, false);
            Set(OsuConfig.FrameTimeDisplay, false);
            Set(OsuConfig.GuideTips, @"");
            Set(OsuConfig.CursorRipple, false);
            Set(OsuConfig.HighlightWords, string.Empty);
            Set(OsuConfig.HighResolution, false);
            Set(OsuConfig.HitLighting, true);
            Set(OsuConfig.IgnoreBarline, false);
            Set(OsuConfig.IgnoreBeatmapSamples, false);
            Set(OsuConfig.IgnoreBeatmapSkins, false);
            Set(OsuConfig.IgnoreList, string.Empty);
            Set(OsuConfig.KeyOverlay, false);
            Set(OsuConfig.Language, @"unknown");
            Set(OsuConfig.AllowNowPlayingHighlights, false);
            Set(OsuConfig.LastVersion, string.Empty);
            Set(OsuConfig.LastVersionPermissionsFailed, string.Empty);
            Set(OsuConfig.LoadSubmittedThread, true);
            Set(OsuConfig.LobbyPlayMode, -1);
            Set(OsuConfig.ShowInterface, true);
            Set(OsuConfig.ShowInterfaceDuringRelax, false);
            Set(OsuConfig.LobbyShowExistingOnly, false);
            Set(OsuConfig.LobbyShowFriendsOnly, false);
            Set(OsuConfig.LobbyShowFull, false);
            Set(OsuConfig.LobbyShowInProgress, true);
            Set(OsuConfig.LobbyShowPassworded, true);
            Set(OsuConfig.LogPrivateMessages, false);
            Set(OsuConfig.LowResolution, false);
            //Set(OsuConfig.ManiaSpeed, SpeedMania.SPEED_DEFAULT, SpeedMania.SPEED_MIN, SpeedMania.SPEED_MAX);
            Set(OsuConfig.UsePerBeatmapManiaSpeed, true);
            Set(OsuConfig.ManiaSpeedBPMScale, true);
            Set(OsuConfig.MenuTip, 0);
            Set(OsuConfig.MouseDisableButtons, false);
            Set(OsuConfig.MouseDisableWheel, false);
            Set(OsuConfig.MouseSpeed, 1, 0.4, 6);
            Set(OsuConfig.Offset, 0, -300, 300);
            Set(OsuConfig.ScoreMeterScale, 1, 0.5, 2);
            //Set(OsuConfig.ScoreMeterScale, 1, 0.5, OsuGame.Tournament ? 10 : 2);
            Set(OsuConfig.DistanceSpacing, 0.8, 0.1, 6);
            Set(OsuConfig.EditorBeatDivisor, 1, 1, 16);
            Set(OsuConfig.EditorGridSize, 32, 4, 32);
            Set(OsuConfig.EditorGridSizeDesign, 32, 4, 32);
            Set(OsuConfig.HeightFullscreen, 9999, 240, 9999);
            Set(OsuConfig.CustomFrameLimit, 240, 240, 999);
            Set(OsuConfig.WidthFullscreen, 9999, 320, 9999);
            Set(OsuConfig.MsnIntegration, false);
            Set(OsuConfig.MyPcSucks, false);
            Set(OsuConfig.NotifyFriends, true);
            Set(OsuConfig.NotifySubmittedThread, true);
            Set(OsuConfig.PopupDuringGameplay, true);
            Set(OsuConfig.ProgressBarType, ProgressBarType.Pie);
            Set(OsuConfig.RankType, RankingType.Top);
            Set(OsuConfig.RefreshRate, 60);
            Set(OsuConfig.OverrideRefreshRate, Get<int>(OsuConfig.RefreshRate) != 60);
            //Set(OsuConfig.ScaleMode, ScaleMode.WidescreenConservative);
            Set(OsuConfig.ScoreboardVisible, true);
            Set(OsuConfig.ScoreMeter, ScoreMeterType.Error);
            //Set(OsuConfig.ScoreMeter, OsuGame.Tournament ? ScoreMeterType.Colour : ScoreMeterType.Error);
            Set(OsuConfig.ScreenshotId, 0);
            Set(OsuConfig.MenuSnow, false);
            Set(OsuConfig.MenuTriangles, true);
            Set(OsuConfig.SongSelectThumbnails, true);
            Set(OsuConfig.ScreenshotFormat, ScreenshotFormat.Jpg);
            Set(OsuConfig.ShowReplayComments, true);
            Set(OsuConfig.ShowSpectators, true);
            Set(OsuConfig.ShowStoryboard, true);
            //Set(OsuConfig.Skin, SkinManager.DEFAULT_SKIN);
            Set(OsuConfig.SkinSamples, true);
            Set(OsuConfig.SkipTablet, false);
            Set(OsuConfig.SnakingInSliders, true);
            Set(OsuConfig.SnakingOutSliders, false);
            Set(OsuConfig.Tablet, false);
            Set(OsuConfig.UpdatePending, false);
            Set(OsuConfig.UseSkinCursor, false);
            Set(OsuConfig.UseTaikoSkin, false);
            Set(OsuConfig.Video, true);
            Set(OsuConfig.Wiimote, false);
            Set(OsuConfig.YahooIntegration, false);
            Set(OsuConfig.ForceFrameFlush, false);
            Set(OsuConfig.DetectPerformanceIssues, true);
            Set(OsuConfig.Fullscreen, true);
            Set(OsuConfig.MenuMusic, true);
            Set(OsuConfig.MenuVoice, true);
            Set(OsuConfig.MenuParallax, true);
            Set(OsuConfig.RawInput, false);
            Set(OsuConfig.AbsoluteToOsuWindow, Get<bool>(OsuConfig.RawInput));
            Set(OsuConfig.ShowMenuTips, true);
            Set(OsuConfig.HiddenShowFirstApproach, true);
            Set(OsuConfig.ComboColourSliderBall, true);
            Set(OsuConfig.AlternativeChatFont, false);
            Set(OsuConfig.Password, string.Empty);
            Set(OsuConfig.Username, string.Empty);
            Set(OsuConfig.DisplayStarsMaximum, 10.0, 0.0, 10.0);
            Set(OsuConfig.DisplayStarsMinimum, 0.0, 0.0, 10.0);
            Set(OsuConfig.AudioDevice, string.Empty);
            Set(OsuConfig.ReleaseStream, ReleaseStream.Lazer);
            Set(OsuConfig.UpdateFailCount, 0);
            Set(OsuConfig.SavePassword, false);
            Set(OsuConfig.SaveUsername, true);
            //Set(OsuConfig.TreeSortMode, TreeGroupMode.Show_All);
            //Set(OsuConfig.TreeSortMode2, TreeSortMode.Title);
            Set(OsuConfig.Letterboxing, Get<bool>(OsuConfig.Fullscreen));
            Set(OsuConfig.LetterboxPositionX, 0, -100, 100);
            Set(OsuConfig.LetterboxPositionY, 0, -100, 100);
            Set(OsuConfig.FrameSync, FrameSync.Limit120);
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
            Set(OsuConfig.PermanentSongInfo, false);
            Set(OsuConfig.Ticker, false);
            Set(OsuConfig.CompatibilityContext, false);
            Set(OsuConfig.CanForceOptimusCompatibility, true);
            Set(OsuConfig.ConfineMouse, Get<bool>(OsuConfig.ConfineMouseToFullscreen) ?
                ConfineMouseMode.Fullscreen : ConfineMouseMode.Never);
#pragma warning restore CS0612 // Type or member is obsolete
        }

        //todo: make a UnicodeString class/struct rather than requiring this helper method.
        public string GetUnicodeString(string nonunicode, string unicode)
            => Get<bool>(OsuConfig.ShowUnicode) ? unicode ?? nonunicode : nonunicode ?? unicode;

        public OsuConfigManager(BasicStorage storage) : base(storage)
        {
        }
    }

    public enum OsuConfig
    {
        // New osu:
        PlayMode,
        Token,
        // Imported from old osu:
        BeatmapDirectory,
        VolumeUniversal,
        VolumeEffect,
        VolumeMusic,
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
        CursorSize,
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
        Offset,
        ScoreMeterScale,
        DistanceSpacing,
        EditorBeatDivisor,
        EditorGridSize,
        EditorGridSizeDesign,
        Height,
        Width,
        HeightFullscreen,
        CustomFrameLimit,
        WidthFullscreen,
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
        Fullscreen,
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
        Password,
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
        Letterboxing,
        LetterboxPositionX,
        LetterboxPositionY,
        FrameSync,
        ShowUnicode,
        PermanentSongInfo,
        Ticker,
        CompatibilityContext,
        CanForceOptimusCompatibility,

    }
}
