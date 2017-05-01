﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;

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
                new OptionSlider<double, StarSlider>
                {
                    LabelText = "Display beatmaps from",
                    Bindable = config.GetBindable<double>(OsuConfig.DisplayStarsMinimum)
                },
                new OptionSlider<double, StarSlider>
                {
                    LabelText = "up to",
                    Bindable = config.GetBindable<double>(OsuConfig.DisplayStarsMaximum)
                },
            };
        }

        private class StarSlider : OsuSliderBar<double>
        {
            public override string TooltipText => Current.Value.ToString(@"0.## stars");
        }
    }
}

