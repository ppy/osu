// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;

namespace osu.Game.Screens.Play.PlayerSettings
{
    public class PlayerSliderBar<T> : SettingsSlider<T>
        where T : struct, IEquatable<T>, IComparable, IConvertible
    {
        public OsuSliderBar<T> Bar => (OsuSliderBar<T>)Control;

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
