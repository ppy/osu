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
        [SettingSource("Hit markers", SettingControlType = typeof(PlayerCheckbox))]
        public BindableBool HitMarkersEnabled { get; } = new BindableBool();

        [SettingSource("Aim markers", SettingControlType = typeof(PlayerCheckbox))]
        public BindableBool AimMarkersEnabled { get; } = new BindableBool();

        [SettingSource("Aim lines", SettingControlType = typeof(PlayerCheckbox))]
        public BindableBool AimLinesEnabled { get; } = new BindableBool();

        [SettingSource("Hide cursor", SettingControlType = typeof(PlayerCheckbox))]
        public BindableBool CursorHideEnabled { get; } = new BindableBool();

        public ReplayAnalysisSettings()
            : base("Analysis Settings")
        {
        }

        [BackgroundDependencyLoader]
        private void load(IRulesetConfigCache cache)
        {
            AddRange(this.CreateSettingsControls());

            var config = (OsuRulesetConfigManager)cache.GetConfigFor(new OsuRuleset())!;

            config.BindWith(OsuRulesetSetting.ReplayHitMarkersEnabled, HitMarkersEnabled);
            config.BindWith(OsuRulesetSetting.ReplayAimMarkersEnabled, AimMarkersEnabled);
            config.BindWith(OsuRulesetSetting.ReplayAimLinesEnabled, AimLinesEnabled);
            config.BindWith(OsuRulesetSetting.ReplayCursorHideEnabled, CursorHideEnabled);
        }
    }
}
