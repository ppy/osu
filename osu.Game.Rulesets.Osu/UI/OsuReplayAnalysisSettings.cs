// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Rulesets.Osu.Configuration;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play.PlayerSettings;

namespace osu.Game.Rulesets.Osu.UI
{
    public partial class OsuReplayAnalysisSettings : ReplayAnalysisSettings
    {
        protected new OsuRulesetConfigManager Config => (OsuRulesetConfigManager)base.Config;

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

        public OsuReplayAnalysisSettings(Ruleset ruleset)
            : base(ruleset)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Config.BindWith(OsuRulesetSetting.ReplayClickMarkersEnabled, ShowClickMarkers);
            Config.BindWith(OsuRulesetSetting.ReplayFrameMarkersEnabled, ShowAimMarkers);
            Config.BindWith(OsuRulesetSetting.ReplayCursorPathEnabled, ShowCursorPath);
            Config.BindWith(OsuRulesetSetting.ReplayCursorHideEnabled, HideSkinCursor);
            Config.BindWith(OsuRulesetSetting.ReplayAnalysisDisplayLength, DisplayLength);
        }
    }
}
