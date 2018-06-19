// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Settings
{
    public class SettingsSliderGrayscale : SettingsSlider<double>
    {
        private readonly Circle swatch;

        public override Bindable<double> Bindable
        {
            get => base.Bindable;
            set {
                base.Bindable = value;
                swatch.Colour = OsuColour.Gray((float)Bindable);
                Bindable.ValueChanged += _ => swatch.Colour = OsuColour.Gray((float)Bindable);
            }
        }

        public SettingsSliderGrayscale()
        {
            Add(swatch = new Circle
            {
                Width = 25,
                Height = 25,
                RelativePositionAxes = Axes.None
            });
        }

    }
}
