// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Options.Sections.Graphics
{
    public class DetailOptions : OptionsSubsection
    {
        protected override string Header => "Detail Settings";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new OptionCheckbox
                {
                    LabelText = "Snaking in sliders",
                    Bindable = config.GetBindable<bool>(OsuConfig.SnakingInSliders)
                },
                new OptionCheckbox
                {
                    LabelText = "Snaking out sliders",
                    Bindable = config.GetBindable<bool>(OsuConfig.SnakingOutSliders)
                },
            };
        }
    }
}
