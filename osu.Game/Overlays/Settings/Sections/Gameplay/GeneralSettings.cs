// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
        private void load(OsuConfigManager config, BindableVisualSettings visualSettings)
        {
            Children = new Drawable[]
            {
                new SettingsSlider<double>
                {
                    LabelText = "Background dim",
                    Bindable = visualSettings.DimLevel,
                    KeyboardStep = 0.1f
                },
                new SettingsSlider<double>
                {
                    LabelText = "Background blur",
                    Bindable = visualSettings.BlurLevel,
                    KeyboardStep = 0.1f
                },
                new SettingsCheckbox
                {
                    LabelText = "Show score overlay",
                    Bindable = config.GetBindable<bool>(OsuSetting.ShowInterface)
                },
                new SettingsCheckbox
                {
                    LabelText = "Always show key overlay",
                    Bindable = config.GetBindable<bool>(OsuSetting.KeyOverlay)
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
