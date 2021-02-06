// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Configuration;
using osu.Framework.Platform;

namespace osu.Game.Configuration
{
    public class MConfigManager : IniConfigManager<MSetting>
    {
        protected override string Filename => "mf.ini";

        public MConfigManager(Storage storage)
            : base(storage)
        {
        }

        protected override void InitialiseDefaults()
        {
            base.InitialiseDefaults();

            //Other Settings
            Set(MSetting.UseSayobot, true);

            //UI Settings
            Set(MSetting.OptUI, true);
            Set(MSetting.TrianglesEnabled, true);
            Set(MSetting.SongSelectBgBlur, 0.2f, 0f, 1f);

            //Intro Settings
            Set(MSetting.IntroLoadDirectToSongSelect, false);

            //Gameplay Settings
            Set(MSetting.SamplePlaybackGain, 1f, 0f, 20f);

            //MvisSettings
            Set(MSetting.MvisParticleAmount, 350, 0, 350);
            Set(MSetting.MvisContentAlpha, 1f, 0f, 1f);
            Set(MSetting.MvisBgBlur, 0.2f, 0f, 1f);
            Set(MSetting.MvisEnableStoryboard, true);
            Set(MSetting.MvisStoryboardProxy, true);
            Set(MSetting.MvisUseOsuLogoVisualisation, false);
            Set(MSetting.MvisIdleBgDim, 0.9f, 0f, 1f);
            Set(MSetting.MvisEnableBgTriangles, true);
            Set(MSetting.MvisAdjustMusicWithFreq, true);
            Set(MSetting.MvisMusicSpeed, 1.0, 0.1, 2.0);
            Set(MSetting.MvisEnableNightcoreBeat, false);
            Set(MSetting.MvisPlayFromCollection, false);
            Set(MSetting.MvisInterfaceRed, value: 0, 0, 255f);
            Set(MSetting.MvisInterfaceGreen, value: 119f, 0, 255f);
            Set(MSetting.MvisInterfaceBlue, value: 255f, 0, 255f);

            //Mvis Settings(Upstream)
            Set(MSetting.MvisShowParticles, true);
            Set(MSetting.MvisBarType, MvisBarType.Rounded);
            Set(MSetting.MvisVisualizerAmount, 3, 1, 5);
            Set(MSetting.MvisBarWidth, 3.0, 1, 20);
            Set(MSetting.MvisBarsPerVisual, 120, 1, 200);
            Set(MSetting.MvisRotation, 0, 0, 359);
            Set(MSetting.MvisUseCustomColour, false);
            Set(MSetting.MvisRed, 0, 0, 255);
            Set(MSetting.MvisGreen, 0, 0, 255);
            Set(MSetting.MvisBlue, 0, 0, 255);

            //Purcashe Screen
            Set(MSetting.PPCount, 0, 0);
            Set(MSetting.PurcasheBgBeatmap, false);
            Set(MSetting.PurcasheBgTriangles, false);

            //实验性功能
            Set(MSetting.CustomWindowIconPath, "");
            Set(MSetting.UseCustomGreetingPicture, false);
        }
    }

    public enum MSetting
    {
        OptUI,
        TrianglesEnabled,
        UseSayobot,
        MvisParticleAmount,
        MvisBgBlur,
        MvisUseOsuLogoVisualisation,
        MvisEnableStoryboard,
        MvisStoryboardProxy,
        MvisIdleBgDim,
        MvisContentAlpha,
        MvisEnableBgTriangles,
        MvisShowParticles,
        MvisVisualizerAmount,
        MvisBarWidth,
        MvisBarsPerVisual,
        MvisBarType,
        MvisRotation,
        MvisUseCustomColour,
        MvisRed,
        MvisGreen,
        MvisBlue,
        MvisMusicSpeed,
        MvisAdjustMusicWithFreq,
        MvisEnableNightcoreBeat,
        MvisPlayFromCollection,
        MvisInterfaceRed,
        MvisInterfaceGreen,
        MvisInterfaceBlue,
        SamplePlaybackGain,
        SongSelectBgBlur,
        IntroLoadDirectToSongSelect,
        PPCount,
        PurcasheBgBeatmap,
        PurcasheBgTriangles,
        CustomWindowIconPath,
        UseCustomGreetingPicture
    }

    public enum MvisBarType
    {
        [Description("settings.mvis.barType.basic")]
        Basic,

        [Description("settings.mvis.barType.rounded")]
        Rounded,

        [Description("settings.mvis.barType.fall")]
        Fall
    }
}
