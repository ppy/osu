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
            Set(OsuConfig.MouseSensitivity, 1.0);

            Set(OsuConfig.Username, string.Empty);
            Set(OsuConfig.Password, string.Empty);
            Set(OsuConfig.Token, string.Empty);

            Set(OsuConfig.PlayMode, PlayMode.Osu);

            Set(OsuConfig.VolumeGlobal, 0.8, 0, 1);
            Set(OsuConfig.VolumeMusic, 1.0, 0, 1);
            Set(OsuConfig.VolumeEffect, 1.0, 0, 1);
        }

        public OsuConfigManager(BasicStorage storage) : base(storage)
        {
        }
    }

    enum OsuConfig
    {
        // General
        //  Sign in
        Username,
        Password,
        //  Language
        Language,
        PreferOriginalForMetadata,
        AlternativeChatFont,
        //  Updates
        ReleaseStream,
        // Graphics
        //  Renderer
        FrameLimiter,
        FPSCounter,
        ReduceDroppedFrames,
        DetectPerformanceIssues,
        //  Layout
        Width,
        Height,
        EnableFullscreen,
        EnableLetterboxing,
        HorizontalPosition,
        VerticalPosition,
        //  Detail settings
        EnableSnakingSliders,
        EnableBackgroundVideo,
        EnableStoryboards,
        EnableComboBursts,
        EnableHitLighting,
        EnableShaders,
        EnableSofteningFilter,
        ScreenshotFormat,
        //  Main menu
        EnableMenuSnow,
        EnableMenuParallax,
        EnableMenuTips,
        EnableInterfaceVoices,
        EnableOsuMusicTheme,
        //  Song select
        EnableSongSelectThumbnails,
        // Gameplay
        //  General
        BackgroundDim,
        ProgressDisplay,
        ScoreMeterType,
        ScoreMeterSize,
        AlwaysShowKeyOverlay,
        ShowFirstHiddenApproachCircle,
        ScaleManiaScrollSpeed,
        RememberManiaScrollSpeed,
        //  Song select
        SongSelectMinimumStars,
        SongSelectMaximumStars,
        // Audio
        //  Devices
        OutputDevice,
        //  Volume
        VolumeGlobal,
        VolumeEffect,
        VolumeMusic,
        IgnoreBeatmapHitsounds,
        //  Offset
        UniversalOffset,
        // Skin
        SelectedSkin,
        IgnoreBeatmapSkins,
        UseSkinAudioSamples,
        UseTaikoSkin,
        UseSkinCursor,
        CursorSize,
        EnableAutomaticCursorSize,
        // Input
        //  Mouse
        MouseSensitivity,
        EnableRawInput,
        EnableMapRawInputToWindow,
        MouseConfinementMode,
        DisableMouseWheelInPlay,
        DisableMouseButtonsInPlay,
        EnableCursorRipples,
        //  Keyboard
        // TODO
        //  Other
        UseOSTabletSupport,
        EnableWiimoteDrumSupport,
        // Editor
        EnableEditorBackgroundVideo,
        EnableEditorDefaultSkin,
        EnableEditorSnakingSliders,
        EnableEditorHitAnimations,
        EnableEditorFollowPoints,
        EnableEditorStacking,
        // Online
        //  Alerts and Privacy
        EnableChatTicker,
        HideChatDuringPlay,
        EnableNotifyOnMention,
        EnableSoundOnMention,
        EnableChatNotifications,
        EnableCitySharing,
        EnableSpectators,
        AutoLinkBeatmapsToSpectators,
        ShowNotificationsDuringPlay,
        ShowFriendOnlineStatusNotifications,
        AllowAnyMultiplayerInvites,
        //  Integration
        EnableYahooIntegration,
        EnableMSNLiveIntegration,
        AutoStartOsuDirect,
        PreferNoVideo,
        //  In-game chat
        EnableWordFilter,
        EnableForeignFilter,
        EnablePrivateMessageLog,
        BlockStrangerPrivateMessages,
        ChatIgnoreList,
        ChatHighlightList,
        // Maintenance
        // (no persisted options)
        // Misc (not mapped to user-visible options)
        Token,
        PlayMode,
    }
}
