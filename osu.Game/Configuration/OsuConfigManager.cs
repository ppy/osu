//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Framework.Platform;
using osu.Game.GameModes.Play;
using osu.Game.Online.API;

namespace osu.Game.Configuration
{
    class OsuConfigManager : ConfigManager<OsuConfig>
    {
        protected override void InitialiseDefaults()
        {
            Set(OsuConfig.Width, 1366, 640);
            Set(OsuConfig.Height, 768, 480);
            Set(OsuConfig.MouseSpeed, 1.0);

            Set(OsuConfig.Username, string.Empty);
            Set(OsuConfig.Password, string.Empty);
            Set(OsuConfig.Token, string.Empty);

            Set(OsuConfig.PlayMode, PlayMode.Osu);

            Set(OsuConfig.VolumeUniversal, 0.8, 0, 1);
            Set(OsuConfig.VolumeMusic, 1.0, 0, 1);
            Set(OsuConfig.VolumeEffect, 1.0, 0, 1);
        }

        public OsuConfigManager(BasicStorage storage) : base(storage)
        {
        }
    }

    enum OsuConfig
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
        SnakingSliders,
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
