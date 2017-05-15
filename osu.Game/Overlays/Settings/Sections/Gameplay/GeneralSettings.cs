// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;

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
                    Bindable = config.GetBindable<double>(OsuSetting.DimLevel)
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
            };
        }
    }
}
