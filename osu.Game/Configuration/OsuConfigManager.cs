// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Configuration;
using osu.Framework.Configuration.Tracking;
using osu.Framework.Extensions;
using osu.Framework.Platform;
using osu.Game.Overlays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Filter;

namespace osu.Game.Configuration
{
    public class OsuConfigManager : IniConfigManager<OsuSetting>
    {
        protected override void InitialiseDefaults()
        {
            // UI/selection defaults
            Set(OsuSetting.Ruleset, 0, 0, int.MaxValue);
            Set(OsuSetting.Skin, 0, -1, int.MaxValue);

            Set(OsuSetting.BeatmapDetailTab, BeatmapDetailTab.Details);

            Set(OsuSetting.ShowConvertedBeatmaps, true);
            Set(OsuSetting.DisplayStarsMinimum, 0.0, 0, 10, 0.1);
            Set(OsuSetting.DisplayStarsMaximum, 10.0, 0, 10, 0.1);

            Set(OsuSetting.SongSelectGroupingMode, GroupMode.All);
            Set(OsuSetting.SongSelectSortingMode, SortMode.Title);

            Set(OsuSetting.RandomSelectAlgorithm, RandomSelectAlgorithm.RandomPermutation);

            Set(OsuSetting.ChatDisplayHeight, ChatOverlay.DEFAULT_HEIGHT, 0.2f, 1f);

            // Online settings
            Set(OsuSetting.Username, string.Empty);
            Set(OsuSetting.Token, string.Empty);

            Set(OsuSetting.SavePassword, false).ValueChanged += enabled =>
            {
                if (enabled.NewValue) Set(OsuSetting.SaveUsername, true);
            };

            Set(OsuSetting.SaveUsername, true).ValueChanged += enabled =>
            {
                if (!enabled.NewValue) Set(OsuSetting.SavePassword, false);
            };

            Set(OsuSetting.ExternalLinkWarning, true);

            // Audio
            Set(OsuSetting.VolumeInactive, 0.25, 0, 1, 0.01);

            Set(OsuSetting.MenuVoice, true);
            Set(OsuSetting.MenuMusic, true);

            Set(OsuSetting.AudioOffset, 0, -500.0, 500.0, 1);

            // Input
            Set(OsuSetting.MenuCursorSize, 1.0f, 0.5f, 2f, 0.01f);
            Set(OsuSetting.GameplayCursorSize, 1.0f, 0.1f, 2f, 0.01f);
            Set(OsuSetting.AutoCursorSize, false);

            Set(OsuSetting.MouseDisableButtons, false);
            Set(OsuSetting.MouseDisableWheel, false);

            // Graphics
            Set(OsuSetting.ShowFpsDisplay, false);

            Set(OsuSetting.ShowStoryboard, true);
            Set(OsuSetting.ShowVideoBackground, true);
            Set(OsuSetting.BeatmapSkins, true);
            Set(OsuSetting.BeatmapHitsounds, true);

            Set(OsuSetting.CursorRotation, true);

            Set(OsuSetting.MenuParallax, true);

            // Gameplay
            Set(OsuSetting.DimLevel, 0.3, 0, 1, 0.01);
            Set(OsuSetting.BlurLevel, 0, 0, 1, 0.01);

            Set(OsuSetting.HitLighting, true);

            Set(OsuSetting.ShowInterface, true);
            Set(OsuSetting.ShowHealthDisplayWhenCantFail, true);
            Set(OsuSetting.KeyOverlay, false);
            Set(OsuSetting.ScoreMeter, ScoreMeterType.HitErrorBoth);

            Set(OsuSetting.FloatingComments, false);

            Set(OsuSetting.ScoreDisplayMode, ScoringMode.Standardised);

            Set(OsuSetting.IncreaseFirstObjectVisibility, true);

            // Update
            Set(OsuSetting.ReleaseStream, ReleaseStream.Lazer);

            Set(OsuSetting.Version, string.Empty);

            Set(OsuSetting.ScreenshotFormat, ScreenshotFormat.Jpg);
            Set(OsuSetting.ScreenshotCaptureMenuCursor, false);

            Set(OsuSetting.SongSelectRightMouseScroll, false);

            Set(OsuSetting.Scaling, ScalingMode.Off);

            Set(OsuSetting.ScalingSizeX, 0.8f, 0.2f, 1f);
            Set(OsuSetting.ScalingSizeY, 0.8f, 0.2f, 1f);

            Set(OsuSetting.ScalingPositionX, 0.5f, 0f, 1f);
            Set(OsuSetting.ScalingPositionY, 0.5f, 0f, 1f);

            Set(OsuSetting.UIScale, 1f, 0.8f, 1.6f, 0.01f);

            Set(OsuSetting.UIHoldActivationDelay, 200f, 0f, 500f, 50f);

            Set(OsuSetting.IntroSequence, IntroSequence.Triangles);
        }

        public OsuConfigManager(Storage storage)
            : base(storage)
        {
        }

        public override TrackedSettings CreateTrackedSettings() => new TrackedSettings
        {
            new TrackedSetting<bool>(OsuSetting.MouseDisableButtons, v => new SettingDescription(!v, "gameplay mouse buttons", v ? "disabled" : "enabled")),
            new TrackedSetting<ScalingMode>(OsuSetting.Scaling, m => new SettingDescription(m, "scaling", m.GetDescription())),
        };
    }

    public enum OsuSetting
    {
        Ruleset,
        Token,
        MenuCursorSize,
        GameplayCursorSize,
        AutoCursorSize,
        DimLevel,
        BlurLevel,
        ShowStoryboard,
        ShowVideoBackground,
        KeyOverlay,
        ScoreMeter,
        FloatingComments,
        ShowInterface,
        ShowHealthDisplayWhenCantFail,
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
        SongSelectGroupingMode,
        SongSelectSortingMode,
        RandomSelectAlgorithm,
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
        UIScale,
        IntroSequence,
        UIHoldActivationDelay,
        HitLighting
    }
}
