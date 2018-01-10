// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Game.Graphics.UserInterface;
using System;
using osu.Game.Graphics;
using osu.Framework.Graphics;
using osu.Game.Overlays.Settings;

namespace osu.Game.Screens.Play.ReplaySettings
{
    public class ReplaySliderBar<T> : SettingsSlider<T>
        where T : struct, IEquatable<T>
    {
        protected override Drawable CreateControl() => new Sliderbar
        {
            Margin = new MarginPadding { Top = 5, Bottom = 5 },
            RelativeSizeAxes = Axes.X
        };

        private class Sliderbar : OsuSliderBar<T>
        {
            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                AccentColour = colours.Yellow;
                Nub.AccentColour = colours.Yellow;
                Nub.GlowingAccentColour = colours.YellowLighter;
                Nub.GlowColour = colours.YellowDarker;
            }
        }
    }
}
