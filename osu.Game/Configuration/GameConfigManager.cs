// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Framework.Configuration.Tracking;
using osu.Framework.Extensions;
using osu.Framework.Platform;
using osu.Game.Overlays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Select;

namespace osu.Game.Configuration
{
    public class GameConfigManager : IniConfigManager<GameSetting>
    {
        protected override void InitialiseDefaults()
        {
            // UI/selection defaults
            Set(GameSetting.Ruleset, 0, 0, int.MaxValue);
            Set(GameSetting.Skin, 0, 0, int.MaxValue);

            Set(GameSetting.BeatmapDetailTab, BeatmapDetailTab.Details);

            Set(GameSetting.ShowConvertedBeatmaps, true);
            Set(GameSetting.DisplayStarsMinimum, 0.0, 0, 10, 0.1);
            Set(GameSetting.DisplayStarsMaximum, 10.0, 0, 10, 0.1);

            Set(GameSetting.RandomSelectAlgorithm, RandomSelectAlgorithm.RandomPermutation);

            Set(GameSetting.ChatDisplayHeight, ChatOverlay.DEFAULT_HEIGHT, 0.2, 1);

            // Online settings
            Set(GameSetting.Username, string.Empty);
            Set(GameSetting.Token, string.Empty);

            Set(GameSetting.SavePassword, false).ValueChanged += val =>
            {
                if (val) Set(GameSetting.SaveUsername, true);
            };

            Set(GameSetting.SaveUsername, true).ValueChanged += val =>
            {
                if (!val) Set(GameSetting.SavePassword, false);
            };

            Set(GameSetting.ExternalLinkWarning, true);

            // Audio
            Set(GameSetting.VolumeInactive, 0.25, 0, 1, 0.01);

            Set(GameSetting.MenuVoice, true);
            Set(GameSetting.MenuMusic, true);

            Set(GameSetting.AudioOffset, 0, -500.0, 500.0, 1);

            // Input
            Set(GameSetting.MenuCursorSize, 1.0, 0.5f, 2, 0.01);
            Set(GameSetting.GameplayCursorSize, 1.0, 0.5f, 2, 0.01);
            Set(GameSetting.AutoCursorSize, false);

            Set(GameSetting.MouseDisableButtons, false);
            Set(GameSetting.MouseDisableWheel, false);

            // Graphics
            Set(GameSetting.ShowFpsDisplay, false);

            Set(GameSetting.ShowStoryboard, true);
            Set(GameSetting.BeatmapSkins, true);
            Set(GameSetting.BeatmapHitsounds, true);

            Set(GameSetting.CursorRotation, true);

            Set(GameSetting.MenuParallax, true);

            Set(GameSetting.SnakingInSliders, true);
            Set(GameSetting.SnakingOutSliders, true);

            // Gameplay
            Set(GameSetting.DimLevel, 0.3, 0, 1, 0.01);
            Set(GameSetting.BlurLevel, 0, 0, 1, 0.01);

            Set(GameSetting.ShowInterface, true);
            Set(GameSetting.KeyOverlay, false);

            Set(GameSetting.FloatingComments, false);

            Set(GameSetting.ScoreDisplayMode, ScoringMode.Standardised);

            Set(GameSetting.IncreaseFirstObjectVisibility, true);

            // Update
            Set(GameSetting.ReleaseStream, ReleaseStream.Lazer);

            Set(GameSetting.Version, string.Empty);

            Set(GameSetting.ScreenshotFormat, ScreenshotFormat.Jpg);
            Set(GameSetting.ScreenshotCaptureMenuCursor, false);

            Set(GameSetting.SongSelectRightMouseScroll, false);

            Set(GameSetting.Scaling, ScalingMode.Off);

            Set(GameSetting.ScalingSizeX, 0.8f, 0.2f, 1f);
            Set(GameSetting.ScalingSizeY, 0.8f, 0.2f, 1f);

            Set(GameSetting.ScalingPositionX, 0.5f, 0f, 1f);
            Set(GameSetting.ScalingPositionY, 0.5f, 0f, 1f);

            Set(GameSetting.UIScale, 1f, 0.8f, 1.6f, 0.01f);
        }

        public GameConfigManager(Storage storage)
            : base(storage)
        {
        }

        public override TrackedSettings CreateTrackedSettings() => new TrackedSettings
        {
            new TrackedSetting<bool>(GameSetting.MouseDisableButtons, v => new SettingDescription(!v, "gameplay mouse buttons", v ? "disabled" : "enabled")),
            new TrackedSetting<ScalingMode>(GameSetting.Scaling, m => new SettingDescription(m, "scaling", m.GetDescription())),
        };
    }

    public enum GameSetting
    {
        Ruleset,
        Token,
        MenuCursorSize,
        GameplayCursorSize,
        AutoCursorSize,
        DimLevel,
        BlurLevel,
        ShowStoryboard,
        KeyOverlay,
        FloatingComments,
        ShowInterface,
        MouseDisableButtons,
        MouseDisableWheel,
        AudioOffset,
        VolumeInactive,
        MenuMusic,
        MenuVoice,
        CursorRotation,
        MenuParallax,
        BeatmapDetailTab,
        Username,
        ReleaseStream,
        SavePassword,
        SaveUsername,
        DisplayStarsMinimum,
        DisplayStarsMaximum,
        RandomSelectAlgorithm,
        SnakingInSliders,
        SnakingOutSliders,
        ShowFpsDisplay,
        ChatDisplayHeight,
        Version,
        ShowConvertedBeatmaps,
        Skin,
        ScreenshotFormat,
        ScreenshotCaptureMenuCursor,
        SongSelectRightMouseScroll,
        BeatmapSkins,
        BeatmapHitsounds,
        IncreaseFirstObjectVisibility,
        ScoreDisplayMode,
        ExternalLinkWarning,
        Scaling,
        ScalingPositionX,
        ScalingPositionY,
        ScalingSizeX,
        ScalingSizeY,
        UIScale
    }
}
