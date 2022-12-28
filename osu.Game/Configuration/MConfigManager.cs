// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.ComponentModel;
using osu.Framework;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Screens.LLin.SideBar.Tabs;
using osuTK.Graphics;

#nullable disable

namespace osu.Game.Configuration
{
    public class MConfigManager : IniConfigManager<MSetting>
    {
        protected override string Filename => "mf.ini";

        private static MConfigManager instance;
        public static MConfigManager GetInstance() => instance;

        public MConfigManager(Storage storage)
            : base(storage)
        {
            instance = this;
        }

        protected override void InitialiseDefaults()
        {
            base.InitialiseDefaults();

            //Other Settings
            SetDefault(MSetting.UseAccelForDefault, true);
            SetDefault(MSetting.DoNotShowDisclaimer, false);
            SetDefault(MSetting.AccelSource, "https://txy1.sayobot.cn/beatmaps/download/[NOVIDEO_SAYO]/[BID]");
            SetDefault(MSetting.CoverAccelSource, "https://a.sayobot.cn/beatmaps/[BID]/covers/cover.jpg"); //不加.jpg日志会刷Texture could not be loaded via STB
            SetDefault(MSetting.TrackPreviewAccelSource, "https://a.sayobot.cn/preview/[BID].mp3");
            SetDefault(MSetting.UseAccelSetToOldOption, false);

            //UI Settings
            SetDefault(MSetting.OptUI, true);
            SetDefault(MSetting.TrianglesEnabled, true);
            SetDefault(MSetting.SongSelectBgBlur, 0.2f, 0f, 1f);

            //Intro Settings
            SetDefault(MSetting.IntroLoadDirectToSongSelect, false);

            //Gameplay Settings
            SetDefault(MSetting.SamplePlaybackGain, 1f, 0f, 20f);

            //MvisSettings
            SetDefault(MSetting.MvisContentAlpha, 1f, 0f, 1f);
            SetDefault(MSetting.MvisBgBlur, 0.2f, 0f, 1f);
            SetDefault(MSetting.MvisStoryboardProxy, false);
            SetDefault(MSetting.MvisIdleBgDim, 0.8f, 0f, 1f);
            SetDefault(MSetting.MvisEnableBgTriangles, true);
            SetDefault(MSetting.MvisAdjustMusicWithFreq, true);
            SetDefault(MSetting.MvisMusicSpeed, 1.0, 0.1, 2.0);
            SetDefault(MSetting.MvisEnableNightcoreBeat, false);
            SetDefault(MSetting.MvisPlayFromCollection, false);
            SetDefault(MSetting.MvisInterfaceRed, value: 0, 0, 255f);
            SetDefault(MSetting.MvisInterfaceGreen, value: 119f, 0, 255f);
            SetDefault(MSetting.MvisInterfaceBlue, value: 255f, 0, 255f);
            SetDefault(MSetting.MvisCurrentAudioProvider, "DummyAudioPlugin@osu.Game.Screens.LLin.Plugins.Internal.DummyAudio");
            SetDefault(MSetting.MvisCurrentFunctionBar, "LegacyBottomBar@Mvis.Plugin.BottomBar");
            SetDefault(MSetting.MvisTabControlPosition, TabControlPosition.Right);
            SetDefault(MSetting.MvisAutoVSync, true);
            SetDefault(MSetting.MvisPlayerSettingsMaxWidth, 0.6f, 0.2f, 1f);
            SetDefault(MSetting.MvisUseTriangleV2, false);

            //实验性功能
            SetDefault(MSetting.CustomWindowIconPath, "");
            SetDefault(MSetting.UseCustomGreetingPicture, false);
            SetDefault(MSetting.AllowWindowFadeEffect, false);
            SetDefault(MSetting.UseSystemCursor, false);
            SetDefault(MSetting.PreferredFont, "Torus");
            SetDefault(MSetting.LoaderBackgroundColor, "#000000");

            //Gamemode集成
            SetDefault(MSetting.Gamemode, GamemodeActivateCondition.InGame);

            bool isLinuxPlatform = RuntimeInfo.OS == RuntimeInfo.Platform.Linux;

            //DBus集成
            SetDefault(MSetting.DBusIntegration, isLinuxPlatform);
            SetDefault(MSetting.DBusAllowPost, true);
            SetDefault(MSetting.EnableTray, isLinuxPlatform);
            SetDefault(MSetting.EnableSystemNotifications, isLinuxPlatform);
            SetDefault(MSetting.TrayIconName, "mfosu-panel");
            SetDefault(MSetting.DBusWaitOnline, 300d, 1d, 3000d);

            //Mpris
            SetDefault(MSetting.MprisUseAvatarlogoAsCover, true);
            SetDefault(MSetting.MprisUpdateInterval, 500d, 100d, 1000d);

            //排行榜
            //SetDefault(MSetting.InGameLeaderboardState, LeaderboardState.Fold);
        }

        public Color4 GetCustomLoaderColor()
        {
            try
            {
                if (Get<bool>(MSetting.UseCustomGreetingPicture))
                    return Color4Extensions.FromHex(Get<string>(MSetting.LoaderBackgroundColor));
                else
                    return Color4.Black;
            }
            catch (Exception e)
            {
                SetValue(MSetting.LoaderBackgroundColor, "#000000");
                Logger.Error(e, "无法获取加载器背景色, 已重置此键的值。");
                return Color4.Black;
            }
        }
    }

    public enum MSetting
    {
        OptUI,
        TrianglesEnabled,
        UseAccelForDefault,
        MvisBgBlur,
        MvisStoryboardProxy,
        MvisIdleBgDim,
        MvisContentAlpha,
        MvisEnableBgTriangles,
        MvisMusicSpeed,
        MvisAdjustMusicWithFreq,
        MvisEnableNightcoreBeat,
        MvisPlayFromCollection,
        MvisInterfaceRed,
        MvisInterfaceGreen,
        MvisInterfaceBlue,
        MvisTabControlPosition,
        SamplePlaybackGain,
        SongSelectBgBlur,
        IntroLoadDirectToSongSelect,
        CustomWindowIconPath,
        UseCustomGreetingPicture,
        AllowWindowFadeEffect,
        UseSystemCursor,
        PreferredFont,
        MvisCurrentAudioProvider,
        Gamemode,
        DoNotShowDisclaimer,
        LoaderBackgroundColor,
        MvisCurrentFunctionBar,
        DBusIntegration,
        DBusAllowPost,
        DBusWaitOnline,
        MprisUseAvatarlogoAsCover,
        MprisUpdateInterval,
        EnableTray,
        EnableSystemNotifications,
        TrayIconName,
        AccelSource,
        UseAccelSetToOldOption,
        CoverAccelSource,
        TrackPreviewAccelSource,
        InGameLeaderboardState,
        MvisAutoVSync,
        MvisPlayerSettingsMaxWidth,
        MvisUseTriangleV2
    }

    public enum GamemodeActivateCondition
    {
        [Description("从不")]
        Never,

        [Description("仅游戏内")]
        InGame,

        [Description("总是")]
        Always
    }
}
