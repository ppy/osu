// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Screens.Play.PlayerSettings;

namespace osu.Game.Rulesets.Osu.UI
{
    public partial class OsuAnalysisSettings : AnalysisSettings
    {
        [SettingSource("Hit markers", SettingControlType = typeof(PlayerCheckbox))]
        public BindableBool HitMarkersEnabled { get; } = new BindableBool();

        [SettingSource("Aim markers", SettingControlType = typeof(PlayerCheckbox))]
        public BindableBool AimMarkersEnabled { get; } = new BindableBool();

        [SettingSource("Aim lines", SettingControlType = typeof(PlayerCheckbox))]
        public BindableBool AimLinesEnabled { get; } = new BindableBool();

        [SettingSource("Hide cursor", SettingControlType = typeof(PlayerCheckbox))]
        public BindableBool CursorHideEnabled { get; } = new BindableBool();

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            config.BindWith(OsuSetting.ReplayHitMarkersEnabled, HitMarkersEnabled);
            config.BindWith(OsuSetting.ReplayAimMarkersEnabled, AimMarkersEnabled);
            config.BindWith(OsuSetting.ReplayAimLinesEnabled, AimLinesEnabled);
            config.BindWith(OsuSetting.ReplayCursorHideEnabled, CursorHideEnabled);
        }
    }
}
