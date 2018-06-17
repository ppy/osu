// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;

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
                swatch.Colour = Color4.FromHsl(new Vector4(0, 0, (float)Bindable, 1));
                Bindable.ValueChanged += _ => swatch.Colour = Color4.FromHsl(new Vector4(0, 0, (float)Bindable, 1));
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
