// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Configuration;
using osu.Framework.Platform;

namespace osu.Game.Configuration
{
    public class MfConfigManager : IniConfigManager<MfSetting>
    {
        protected override string Filename => "mf.ini";

        public MfConfigManager(Storage storage)
            : base(storage)
        {
        }

        protected override void InitialiseDefaults()
        {
            base.InitialiseDefaults();

            //UI Settings
            Set(MfSetting.OptUI, true);
            Set(MfSetting.TrianglesEnabled, true);
            Set(MfSetting.SongSelectBgBlur, 0.2f, 0f, 1f);

            //Intro Settings
            Set(MfSetting.IntroLoadDirectToSongSelect, false);

            //Gameplay Settings
            Set(MfSetting.SamplePlaybackGain, 1f, 0f, 20f);

            //MvisSettings
            Set(MfSetting.MvisParticleAmount, 350, 0, 350);
            Set(MfSetting.MvisContentAlpha, 1f, 0f, 1f);
            Set(MfSetting.MvisBgBlur, 0.2f, 0f, 1f);
            Set(MfSetting.MvisEnableStoryboard, false);
            Set(MfSetting.MvisUseOsuLogoVisualisation, false);
            Set(MfSetting.MvisIdleBgDim, 0.3f, 0f, 1f);
            Set(MfSetting.MvisBarCount, 120, 20, 120);
            Set(MfSetting.MvisBarRGBLighting, false);
            Set(MfSetting.MvisEnableBrick, false);
            Set(MfSetting.MvisEnableBgTriangles, true);
            Set(MfSetting.UseSayobot, true);
        }
    }

    public enum MfSetting
    {
        OptUI,
        TrianglesEnabled,
        UseSayobot,
        MvisParticleAmount,
        MvisBgBlur,
        MvisUseOsuLogoVisualisation,
        MvisEnableStoryboard,
        MvisIdleBgDim,
        MvisContentAlpha,
        MvisBarCount,
        MvisBarRGBLighting,
        MvisEnableBrick,
        MvisEnableBgTriangles,
        SamplePlaybackGain,
        SongSelectBgBlur,
        IntroLoadDirectToSongSelect
    }
}