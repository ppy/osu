// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Configuration.Tracking;
using osu.Framework.Extensions;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Input;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Filter;

namespace osu.Game.Configuration
{
    [ExcludeFromDynamicCompile]
    public class OsuConfigManager : IniConfigManager<OsuSetting>
    {
        protected override void InitialiseDefaults()
        {
            // UI/selection defaults
            Set(OsuSetting.Ruleset, 0, 0, int.MaxValue);
            Set(OsuSetting.Skin, 0, -1, int.MaxValue);

            Set(OsuSetting.BeatmapDetailTab, PlayBeatmapDetailArea.TabType.Details);
            Set(OsuSetting.BeatmapDetailModsFilter, false);

            Set(OsuSetting.ShowConvertedBeatmaps, true);
            Set(OsuSetting.DisplayStarsMinimum, 0.0, 0, 10, 0.1);
            Set(OsuSetting.DisplayStarsMaximum, 10.1, 0, 10.1, 0.1);

            Set(OsuSetting.SongSelectGroupingMode, GroupMode.All);
            Set(OsuSetting.SongSelectSortingMode, SortMode.Title);

            Set(OsuSetting.RandomSelectAlgorithm, RandomSelectAlgorithm.RandomPermutation);

            Set(OsuSetting.ChatDisplayHeight, ChatOverlay.DEFAULT_HEIGHT, 0.2f, 1f);

            // Online settings
            Set(OsuSetting.Username, string.Empty);
            Set(OsuSetting.Token, string.Empty);

            Set(OsuSetting.AutomaticallyDownloadWhenSpectating, false);

            Set(OsuSetting.SavePassword, false).ValueChanged += enabled =>
            {
                if (enabled.NewValue) Set(OsuSetting.SaveUsername, true);
            };

            Set(OsuSetting.SaveUsername, true).ValueChanged += enabled =>
            {
                if (!enabled.NewValue) Set(OsuSetting.SavePassword, false);
            };

            Set(OsuSetting.ExternalLinkWarning, true);
            Set(OsuSetting.PreferNoVideo, false);

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
            Set(OsuSetting.ConfineMouseMode, OsuConfineMouseMode.DuringGameplay);

            // Graphics
            Set(OsuSetting.ShowFpsDisplay, false);

            Set(OsuSetting.ShowStoryboard, true);
            Set(OsuSetting.BeatmapSkins, true);
            Set(OsuSetting.BeatmapHitsounds, true);

            Set(OsuSetting.CursorRotation, true);

            Set(OsuSetting.MenuParallax, true);

            // Gameplay
            Set(OsuSetting.DimLevel, 0.8, 0, 1, 0.01);
            Set(OsuSetting.BlurLevel, 0, 0, 1, 0.01);
            Set(OsuSetting.LightenDuringBreaks, true);

            Set(OsuSetting.HitLighting, true);

            Set(OsuSetting.HUDVisibilityMode, HUDVisibilityMode.Always);
            Set(OsuSetting.ShowProgressGraph, true);
            Set(OsuSetting.ShowHealthDisplayWhenCantFail, true);
            Set(OsuSetting.FadePlayfieldWhenHealthLow, true);
            Set(OsuSetting.KeyOverlay, false);
            Set(OsuSetting.PositionalHitSounds, true);
            Set(OsuSetting.AlwaysPlayFirstComboBreak, true);
            Set(OsuSetting.ScoreMeter, ScoreMeterType.HitErrorBoth);

            Set(OsuSetting.FloatingComments, false);

            Set(OsuSetting.ScoreDisplayMode, ScoringMode.Standardised);

            Set(OsuSetting.IncreaseFirstObjectVisibility, true);
            Set(OsuSetting.GameplayDisableWinKey, true);

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

            Set(OsuSetting.MenuBackgroundSource, BackgroundSource.Skin);
            Set(OsuSetting.SeasonalBackgroundMode, SeasonalBackgroundMode.Sometimes);

            Set(OsuSetting.EditorWaveformOpacity, 1f);
        }

        public OsuConfigManager(Storage storage)
            : base(storage)
        {
            Migrate();
        }

        public void Migrate()
        {
            // arrives as 2020.123.0
            var rawVersion = Get<string>(OsuSetting.Version);

            if (rawVersion.Length < 6)
                return;

            var pieces = rawVersion.Split('.');

            // on a fresh install or when coming from a non-release build, execution will end here.
            // we don't want to run migrations in such cases.
            if (!int.TryParse(pieces[0], out int year)) return;
            if (!int.TryParse(pieces[1], out int monthDay)) return;

            int combined = (year * 10000) + monthDay;

            if (combined < 20200305)
            {
                // the maximum value of this setting was changed.
                // if we don't manually increase this, it causes song select to filter out beatmaps the user expects to see.
                var maxStars = (BindableDouble)GetOriginalBindable<double>(OsuSetting.DisplayStarsMaximum);

                if (maxStars.Value == 10)
                    maxStars.Value = maxStars.MaxValue;
            }
        }

        public override TrackedSettings CreateTrackedSettings()
        {
            // these need to be assigned in normal game startup scenarios.
            Debug.Assert(LookupKeyBindings != null);
            Debug.Assert(LookupSkinName != null);

            return new TrackedSettings
            {
                new TrackedSetting<bool>(OsuSetting.MouseDisableButtons, v => new SettingDescription(!v, "gameplay mouse buttons", v ? "disabled" : "enabled", LookupKeyBindings(GlobalAction.ToggleGameplayMouseButtons))),
                new TrackedSetting<HUDVisibilityMode>(OsuSetting.HUDVisibilityMode, m => new SettingDescription(m, "HUD Visibility", m.GetDescription(), $"cycle: {LookupKeyBindings(GlobalAction.ToggleInGameInterface)} quick view: {LookupKeyBindings(GlobalAction.HoldForHUD)}")),
                new TrackedSetting<ScalingMode>(OsuSetting.Scaling, m => new SettingDescription(m, "scaling", m.GetDescription())),
                new TrackedSetting<int>(OsuSetting.Skin, m =>
                {
                    string skinName = LookupSkinName(m) ?? string.Empty;
                    return new SettingDescription(skinName, "skin", skinName, $"random: {LookupKeyBindings(GlobalAction.RandomSkin)}");
                })
            };
        }

        public Func<int, string> LookupSkinName { private get; set; }

        public Func<GlobalAction, string> LookupKeyBindings { get; set; }
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
        LightenDuringBreaks,
        ShowStoryboard,
        KeyOverlay,
        PositionalHitSounds,
        AlwaysPlayFirstComboBreak,
        ScoreMeter,
        FloatingComments,
        HUDVisibilityMode,
        ShowProgressGraph,
        ShowHealthDisplayWhenCantFail,
        FadePlayfieldWhenHealthLow,
        MouseDisableButtons,
        MouseDisableWheel,
        ConfineMouseMode,
        AudioOffset,
        VolumeInactive,
        MenuMusic,
        MenuVoice,
        CursorRotation,
        MenuParallax,
        BeatmapDetailTab,
        BeatmapDetailModsFilter,
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
        PreferNoVideo,
        Scaling,
        ScalingPositionX,
        ScalingPositionY,
        ScalingSizeX,
        ScalingSizeY,
        UIScale,
        IntroSequence,
        UIHoldActivationDelay,
        HitLighting,
        MenuBackgroundSource,
        GameplayDisableWinKey,
        SeasonalBackgroundMode,
        EditorWaveformOpacity,
        AutomaticallyDownloadWhenSpectating,
    }
}
