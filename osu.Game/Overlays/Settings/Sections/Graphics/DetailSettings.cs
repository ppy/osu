// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Settings.Sections.Graphics
{
    public class DetailSettings : SettingsSubsection
    {
        protected override string Header => "Detail Settings";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "Snaking in sliders",
                    Bindable = config.GetBindable<bool>(OsuSetting.SnakingInSliders)
                },
                new SettingsCheckbox
                {
                    LabelText = "Snaking out sliders",
                    Bindable = config.GetBindable<bool>(OsuSetting.SnakingOutSliders)
                },
            };
        }
    }
}
