// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Localisation;
using osu.Game.Rulesets.Osu.Configuration;
using osu.Game.Screens.Play.PlayerSettings;

namespace osu.Game.Rulesets.Osu.UI
{
    public partial class ReplayAnalysisSettings : PlayerSettingsGroup
    {
        private readonly OsuRulesetConfigManager config;

        [SettingSource(typeof(PlayerSettingsOverlayStrings), nameof(PlayerSettingsOverlayStrings.ShowClickMarkers), SettingControlType = typeof(PlayerCheckbox))]
        public BindableBool ShowClickMarkers { get; } = new BindableBool();

        [SettingSource(typeof(PlayerSettingsOverlayStrings), nameof(PlayerSettingsOverlayStrings.ShowFrameMarkers), SettingControlType = typeof(PlayerCheckbox))]
        public BindableBool ShowAimMarkers { get; } = new BindableBool();

        [SettingSource(typeof(PlayerSettingsOverlayStrings), nameof(PlayerSettingsOverlayStrings.ShowCursorPath), SettingControlType = typeof(PlayerCheckbox))]
        public BindableBool ShowCursorPath { get; } = new BindableBool();

        [SettingSource(typeof(PlayerSettingsOverlayStrings), nameof(PlayerSettingsOverlayStrings.HideGameplayCursor), SettingControlType = typeof(PlayerCheckbox))]
        public BindableBool HideSkinCursor { get; } = new BindableBool();

        [SettingSource(typeof(PlayerSettingsOverlayStrings), nameof(PlayerSettingsOverlayStrings.DisplayLength), SettingControlType = typeof(PlayerSliderBar<int>))]
        public BindableInt DisplayLength { get; } = new BindableInt
        {
            MinValue = 200,
            MaxValue = 2000,
            Default = 800,
            Precision = 200,
        };

        public ReplayAnalysisSettings(OsuRulesetConfigManager config)
            : base(PlayerSettingsOverlayStrings.AnalysisSettingsTitle)
        {
            this.config = config;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddRange(this.CreateSettingsControls());

            config.BindWith(OsuRulesetSetting.ReplayClickMarkersEnabled, ShowClickMarkers);
            config.BindWith(OsuRulesetSetting.ReplayFrameMarkersEnabled, ShowAimMarkers);
            config.BindWith(OsuRulesetSetting.ReplayCursorPathEnabled, ShowCursorPath);
            config.BindWith(OsuRulesetSetting.ReplayCursorHideEnabled, HideSkinCursor);
            config.BindWith(OsuRulesetSetting.ReplayAnalysisDisplayLength, DisplayLength);
        }
    }
}
