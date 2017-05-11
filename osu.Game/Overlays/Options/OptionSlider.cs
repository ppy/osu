// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Options
{
    public class OptionSlider<T> : OptionSlider<T, OsuSliderBar<T>>
        where T : struct
    {
    }

    public class OptionSlider<T, U> : OptionItem<T>
        where T : struct
        where U : SliderBar<T>, new()
    {
        protected override Drawable CreateControl() => new U()
        {
            Margin = new MarginPadding { Top = 5, Bottom = 5 },
            RelativeSizeAxes = Axes.X
        };
    }
}
