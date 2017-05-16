// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Framework.Platform;
using osu.Game.Overlays;
using osu.Game.Screens.Select;

namespace osu.Game.Configuration
{
    public class OsuConfigManager : ConfigManager<OsuSetting>
    {
        protected override void InitialiseDefaults()
        {
            // UI/selection defaults

            Set(OsuSetting.Ruleset, 0, 0, int.MaxValue);
            Set(OsuSetting.BeatmapDetailTab, BeatmapDetailTab.Details);

            Set(OsuSetting.DisplayStarsMinimum, 0.0, 0, 10);
            Set(OsuSetting.DisplayStarsMaximum, 10.0, 0, 10);

            Set(OsuSetting.ChatDisplayHeight, ChatOverlay.DEFAULT_HEIGHT, 0.2, 1);

            // Online settings

            Set(OsuSetting.Username, string.Empty);
            Set(OsuSetting.Token, string.Empty);

            Set(OsuSetting.SavePassword, false).ValueChanged += val =>
            {
                if (val) Set(OsuSetting.SaveUsername, true);
            };

            Set(OsuSetting.SaveUsername, true).ValueChanged += val =>
            {
                if (!val) Set(OsuSetting.SavePassword, false);
            };

            // Audio

            Set(OsuSetting.MenuVoice, true);
            Set(OsuSetting.MenuMusic, true);

            Set(OsuSetting.AudioOffset, 0, -500.0, 500.0);

            // Input

            Set(OsuSetting.MenuCursorSize, 1.0, 0.5f, 2);
            Set(OsuSetting.GameplayCursorSize, 1.0, 0.5f, 2);
            Set(OsuSetting.AutoCursorSize, false);

            Set(OsuSetting.MouseDisableButtons, false);
            Set(OsuSetting.MouseDisableWheel, false);

            // Graphics

            Set(OsuSetting.ShowFpsDisplay, false);

            Set(OsuSetting.MenuParallax, true);

            Set(OsuSetting.SnakingInSliders, true);
            Set(OsuSetting.SnakingOutSliders, true);

            // Gameplay

            Set(OsuSetting.DimLevel, 0.3, 0, 1);

            Set(OsuSetting.ShowInterface, true);
            Set(OsuSetting.KeyOverlay, false);

            // Update

            Set(OsuSetting.ReleaseStream, ReleaseStream.Lazer);
        }

        public OsuConfigManager(Storage storage) : base(storage)
        {
        }
    }

    public enum OsuSetting
    {
        Ruleset,
        Token,
        MenuCursorSize,
        GameplayCursorSize,
        AutoCursorSize,
        DimLevel,
        KeyOverlay,
        ShowInterface,
        MouseDisableButtons,
        MouseDisableWheel,
        AudioOffset,
        MenuMusic,
        MenuVoice,
        MenuParallax,
        BeatmapDetailTab,
        Username,
        ReleaseStream,
        SavePassword,
        SaveUsername,
        DisplayStarsMinimum,
        DisplayStarsMaximum,
        SnakingInSliders,
        SnakingOutSliders,
        ShowFpsDisplay,
        ChatDisplayHeight
    }
}
