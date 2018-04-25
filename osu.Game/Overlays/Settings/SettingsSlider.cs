// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Settings
{
    public class SettingsSlider<T> : SettingsSlider<T, OsuSliderBar<T>>
        where T : struct, IEquatable<T>, IComparable, IConvertible
    {
    }

    public class SettingsSlider<T, U> : SettingsItem<T>
        where T : struct, IEquatable<T>, IComparable, IConvertible
        where U : OsuSliderBar<T>, new()
    {
        protected override Drawable CreateControl() => new U
        {
            Margin = new MarginPadding { Top = 5, Bottom = 5 },
            RelativeSizeAxes = Axes.X
        };

        public float KeyboardStep;

        [BackgroundDependencyLoader]
        private void load()
        {
            var slider = Control as U;
            if (slider != null)
                slider.KeyboardStep = KeyboardStep;
        }
    }
}
