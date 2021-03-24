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
            SetDefault(OsuSetting.Ruleset, 0, 0, int.MaxValue);
            SetDefault(OsuSetting.Skin, 0, -1, int.MaxValue);

            SetDefault(OsuSetting.BeatmapDetailTab, PlayBeatmapDetailArea.TabType.Details);
            SetDefault(OsuSetting.BeatmapDetailModsFilter, false);

            SetDefault(OsuSetting.ShowConvertedBeatmaps, true);
            SetDefault(OsuSetting.DisplayStarsMinimum, 0.0, 0, 10, 0.1);
            SetDefault(OsuSetting.DisplayStarsMaximum, 10.1, 0, 10.1, 0.1);

            SetDefault(OsuSetting.SongSelectGroupingMode, GroupMode.All);
            SetDefault(OsuSetting.SongSelectSortingMode, SortMode.Title);

            SetDefault(OsuSetting.RandomSelectAlgorithm, RandomSelectAlgorithm.RandomPermutation);

            SetDefault(OsuSetting.ChatDisplayHeight, ChatOverlay.DEFAULT_HEIGHT, 0.2f, 1f);

            // Online settings
            SetDefault(OsuSetting.Username, string.Empty);
            SetDefault(OsuSetting.Token, string.Empty);

            SetDefault(OsuSetting.AutomaticallyDownloadWhenSpectating, false);

            SetDefault(OsuSetting.SavePassword, false).ValueChanged += enabled =>
            {
                if (enabled.NewValue) SetValue(OsuSetting.SaveUsername, true);
            };

            SetDefault(OsuSetting.SaveUsername, true).ValueChanged += enabled =>
            {
                if (!enabled.NewValue) SetValue(OsuSetting.SavePassword, false);
            };

            SetDefault(OsuSetting.ExternalLinkWarning, true);
            SetDefault(OsuSetting.PreferNoVideo, false);

            SetDefault(OsuSetting.ShowOnlineExplicitContent, false);

            // Audio
            SetDefault(OsuSetting.VolumeInactive, 0.25, 0, 1, 0.01);

            SetDefault(OsuSetting.MenuVoice, true);
            SetDefault(OsuSetting.MenuMusic, true);

            SetDefault(OsuSetting.AudioOffset, 0, -500.0, 500.0, 1);

            // Input
            SetDefault(OsuSetting.MenuCursorSize, 1.0f, 0.5f, 2f, 0.01f);
            SetDefault(OsuSetting.GameplayCursorSize, 1.0f, 0.1f, 2f, 0.01f);
            SetDefault(OsuSetting.AutoCursorSize, false);

            SetDefault(OsuSetting.MouseDisableButtons, false);
            SetDefault(OsuSetting.MouseDisableWheel, false);
            SetDefault(OsuSetting.ConfineMouseMode, OsuConfineMouseMode.DuringGameplay);

            // Graphics
            SetDefault(OsuSetting.ShowFpsDisplay, false);

            SetDefault(OsuSetting.ShowStoryboard, true);
            SetDefault(OsuSetting.BeatmapSkins, true);
            SetDefault(OsuSetting.BeatmapColours, true);
            SetDefault(OsuSetting.BeatmapHitsounds, true);

            SetDefault(OsuSetting.CursorRotation, true);

            SetDefault(OsuSetting.MenuParallax, true);

            // Gameplay
            SetDefault(OsuSetting.DimLevel, 0.8, 0, 1, 0.01);
            SetDefault(OsuSetting.BlurLevel, 0, 0, 1, 0.01);
            SetDefault(OsuSetting.LightenDuringBreaks, true);

            SetDefault(OsuSetting.HitLighting, true);

            SetDefault(OsuSetting.HUDVisibilityMode, HUDVisibilityMode.Always);
            SetDefault(OsuSetting.ShowProgressGraph, true);
            SetDefault(OsuSetting.ShowHealthDisplayWhenCantFail, true);
            SetDefault(OsuSetting.FadePlayfieldWhenHealthLow, true);
            SetDefault(OsuSetting.KeyOverlay, false);
            SetDefault(OsuSetting.PositionalHitSounds, true);
            SetDefault(OsuSetting.AlwaysPlayFirstComboBreak, true);
            SetDefault(OsuSetting.ScoreMeter, ScoreMeterType.HitErrorBoth);

            SetDefault(OsuSetting.FloatingComments, false);

            SetDefault(OsuSetting.ScoreDisplayMode, ScoringMode.Standardised);

            SetDefault(OsuSetting.IncreaseFirstObjectVisibility, true);
            SetDefault(OsuSetting.GameplayDisableWinKey, true);

            // Update
            SetDefault(OsuSetting.ReleaseStream, ReleaseStream.Lazer);

            SetDefault(OsuSetting.Version, string.Empty);

            SetDefault(OsuSetting.ScreenshotFormat, ScreenshotFormat.Jpg);
            SetDefault(OsuSetting.ScreenshotCaptureMenuCursor, false);

            SetDefault(OsuSetting.SongSelectRightMouseScroll, false);

            SetDefault(OsuSetting.Scaling, ScalingMode.Off);

            SetDefault(OsuSetting.ScalingSizeX, 0.8f, 0.2f, 1f);
            SetDefault(OsuSetting.ScalingSizeY, 0.8f, 0.2f, 1f);

            SetDefault(OsuSetting.ScalingPositionX, 0.5f, 0f, 1f);
            SetDefault(OsuSetting.ScalingPositionY, 0.5f, 0f, 1f);

            SetDefault(OsuSetting.UIScale, 1f, 0.8f, 1.6f, 0.01f);

            SetDefault(OsuSetting.UIHoldActivationDelay, 200f, 0f, 500f, 50f);

            SetDefault(OsuSetting.IntroSequence, IntroSequence.Triangles);

            SetDefault(OsuSetting.MenuBackgroundSource, BackgroundSource.Skin);
            SetDefault(OsuSetting.SeasonalBackgroundMode, SeasonalBackgroundMode.Sometimes);

            SetDefault(OsuSetting.DiscordRichPresence, DiscordRichPresenceMode.Full);

            SetDefault(OsuSetting.EditorWaveformOpacity, 1f);
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
        BeatmapColours,
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
        DiscordRichPresence,
        AutomaticallyDownloadWhenSpectating,
        ShowOnlineExplicitContent,
    }
}
