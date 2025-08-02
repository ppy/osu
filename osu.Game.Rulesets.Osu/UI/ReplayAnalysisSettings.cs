// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Rulesets.Osu.Configuration;
using osu.Game.Screens.Play.PlayerSettings;

namespace osu.Game.Rulesets.Osu.UI
{
    public partial class ReplayAnalysisSettings : PlayerSettingsGroup
    {
        private readonly OsuRulesetConfigManager config;

        [SettingSource("Show click markers", SettingControlType = typeof(PlayerCheckbox))]
        public BindableBool ShowClickMarkers { get; } = new BindableBool();

        [SettingSource("Show frame markers", SettingControlType = typeof(PlayerCheckbox))]
        public BindableBool ShowAimMarkers { get; } = new BindableBool();

        [SettingSource("Show cursor path", SettingControlType = typeof(PlayerCheckbox))]
        public BindableBool ShowCursorPath { get; } = new BindableBool();

        [SettingSource("Hide gameplay cursor", SettingControlType = typeof(PlayerCheckbox))]
        public BindableBool HideSkinCursor { get; } = new BindableBool();

        [SettingSource("Display length", SettingControlType = typeof(PlayerSliderBar<int>))]
        public BindableInt DisplayLength { get; } = new BindableInt
        {
            MinValue = 200,
            MaxValue = 2000,
            Default = 800,
            Precision = 200,
        };

        public ReplayAnalysisSettings(OsuRulesetConfigManager config)
            : base("Analysis Settings")
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
