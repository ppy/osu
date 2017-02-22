// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Options.Sections.Gameplay
{
    public class SongSelectOptions : OptionsSubsection
    {
        protected override string Header => "Song Select";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new OptionSlider<double>
                {
                    LabelText = "Display beatmaps from",
                    Bindable = (BindableDouble)config.GetBindable<double>(OsuConfig.DisplayStarsMinimum)
                },
                new OptionSlider<double>
                {
                    LabelText = "up to",
                    Bindable = (BindableDouble)config.GetBindable<double>(OsuConfig.DisplayStarsMaximum)
                },
            };
        }
    }
}

