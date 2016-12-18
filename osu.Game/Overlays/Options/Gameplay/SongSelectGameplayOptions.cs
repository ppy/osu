//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Options.Gameplay
{
    public class SongSelectGameplayOptions : OptionsSubsection
    {
        protected override string Header => "Song Select";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SliderOption<double>
                {
                    LabelText = "Display beatmaps from",
                    Bindable = (BindableDouble)config.GetBindable<double>(OsuConfig.DisplayStarsMinimum)
                },
                new SliderOption<double>
                {
                    LabelText = "up to",
                    Bindable = (BindableDouble)config.GetBindable<double>(OsuConfig.DisplayStarsMaximum)
                },
            };
        }
    }
}

