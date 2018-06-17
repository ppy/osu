// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.UserInterface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace osu.Game.Overlays.Settings
{
    class SettingsSliderGrayscale : SettingsSlider<double>
    {
        private Circle Swatch;

        public override Bindable<double> Bindable
        {
            get => base.Bindable;
            set {
                base.Bindable = value;
                Swatch.Colour = Color4.FromHsl(new Vector4(0, 0, (float)Bindable, 1));
                Bindable.ValueChanged += _ => Swatch.Colour = Color4.FromHsl(new Vector4(0, 0, (float)Bindable, 1));
            }
        }

        public SettingsSliderGrayscale()
        {
            Add(Swatch = new Circle()
            {
                Width = 25,
                Height = 25,
                RelativePositionAxes = Axes.None
            });
        }

    }
}
