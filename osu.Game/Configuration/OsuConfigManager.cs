// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Framework.Platform;
using osu.Game.Overlays;
using osu.Game.Screens.Select;

namespace osu.Game.Configuration
{
    public class OsuConfigManager : ConfigManager<OsuConfig>
    {
        protected override void InitialiseDefaults()
        {
            // UI/selection defaults

            Set(OsuConfig.Ruleset, 0, 0, int.MaxValue);
            Set(OsuConfig.BeatmapDetailTab, BeatmapDetailTab.Details);

            Set(OsuConfig.DisplayStarsMinimum, 0.0, 0, 10);
            Set(OsuConfig.DisplayStarsMaximum, 10.0, 0, 10);

            Set(OsuConfig.ChatDisplayHeight, ChatOverlay.DEFAULT_HEIGHT, 0.2, 1);

            // Online settings

            Set(OsuConfig.Username, string.Empty);
            Set(OsuConfig.Token, string.Empty);

            Set(OsuConfig.SavePassword, false).ValueChanged += val =>
            {
                if (val) Set(OsuConfig.SaveUsername, true);
            };

            Set(OsuConfig.SaveUsername, true).ValueChanged += val =>
            {
                if (!val) Set(OsuConfig.SavePassword, false);
            };

            // Audio

            Set(OsuConfig.AudioDevice, string.Empty);

            Set(OsuConfig.MenuVoice, true);
            Set(OsuConfig.MenuMusic, true);

            Set(OsuConfig.AudioOffset, 0, -500.0, 500.0);

            // Input

            Set(OsuConfig.MenuCursorSize, 1.0, 0.5f, 2);
            Set(OsuConfig.GameplayCursorSize, 1.0, 0.5f, 2);

            Set(OsuConfig.MouseDisableButtons, false);
            Set(OsuConfig.MouseDisableWheel, false);

            // Graphics

            Set(OsuConfig.ShowFpsDisplay, false);

            Set(OsuConfig.MenuParallax, true);

            Set(OsuConfig.SnakingInSliders, true);
            Set(OsuConfig.SnakingOutSliders, true);

            // Gameplay

            Set(OsuConfig.DimLevel, 0.3, 0, 1);

            Set(OsuConfig.ShowInterface, true);
            Set(OsuConfig.KeyOverlay, false);

            // Update

            Set(OsuConfig.ReleaseStream, ReleaseStream.Lazer);
        }

        public OsuConfigManager(Storage storage) : base(storage)
        {
        }
    }

    public enum OsuConfig
    {
        Ruleset,
        Token,
        MenuCursorSize,
        GameplayCursorSize,
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
        AudioDevice,
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
