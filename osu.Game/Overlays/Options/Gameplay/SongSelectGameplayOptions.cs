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

        //private BindableInt starMinimum, starMaximum;
        //private StarCounter counterMin, counterMax;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            // TODO: Deal with bindable ints
            /*
            starMinimum = config.GetBindable<int>(OsuConfig.DisplayStarsMinimum);
            starMaximum = config.GetBindable<int>(OsuConfig.DisplayStarsMaximum);
            Children = new Drawable[]
            {
                new OptionsSlider { Label = "Display beatmaps from", Bindable = starMinimum },
                counterMin = new StarCounter { Count = starMinimum.Value },
                new OptionsSlider { Label = "up to", Bindable = starMaximum },
                counterMax = new StarCounter { Count = starMaximum.Value },
            };
            starMinimum.ValueChanged += starValueChanged;
            starMaximum.ValueChanged += starValueChanged;*/
        }

        private void starValueChanged(object sender, EventArgs e)
        {
            //counterMin.Count = starMinimum.Value;
            //counterMax.Count = starMaximum.Value;
        }
        
        protected override void Dispose(bool isDisposing)
        {
            //starMinimum.ValueChanged -= starValueChanged;
            //starMaximum.ValueChanged -= starValueChanged;
            base.Dispose(isDisposing);
        }
    }
}

