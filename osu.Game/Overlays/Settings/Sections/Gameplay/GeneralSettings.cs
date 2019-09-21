// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Overlays.Settings.Sections.Gameplay
{
    public class GeneralSettings : SettingsSubsection
    {
        protected override string Header => "General";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsSlider<double>
                {
                    LabelText = "Background dim",
                    Bindable = config.GetBindable<double>(OsuSetting.DimLevel),
                    KeyboardStep = 0.01f
                },
                new SettingsSlider<double>
                {
                    LabelText = "Background blur",
                    Bindable = config.GetBindable<double>(OsuSetting.BlurLevel),
                    KeyboardStep = 0.01f
                },
                new SettingsCheckbox
                {
                    LabelText = "Show score overlay",
                    Bindable = config.GetBindable<bool>(OsuSetting.ShowInterface)
                },
                new SettingsCheckbox
                {
                    LabelText = "Show health display even when you can't fail",
                    Bindable = config.GetBindable<bool>(OsuSetting.ShowHealthDisplayWhenCantFail),
                },
                new SettingsCheckbox
                {
                    LabelText = "Always show key overlay",
                    Bindable = config.GetBindable<bool>(OsuSetting.KeyOverlay)
                },
                new SettingsEnumDropdown<ScoreMeterType>
                {
                    LabelText = "Score meter type",
                    Bindable = config.GetBindable<ScoreMeterType>(OsuSetting.ScoreMeter)
                },
                new SettingsEnumDropdown<ScoringMode>
                {
                    LabelText = "Score display mode",
                    Bindable = config.GetBindable<ScoringMode>(OsuSetting.ScoreDisplayMode)
                }
            };
        }
    }
}
