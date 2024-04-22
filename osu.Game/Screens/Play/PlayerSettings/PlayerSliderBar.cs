// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Numerics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;

namespace osu.Game.Screens.Play.PlayerSettings
{
    public partial class PlayerSliderBar<T> : SettingsSlider<T>
        where T : struct, INumber<T>, IMinMaxValue<T>
    {
        public RoundedSliderBar<T> Bar => (RoundedSliderBar<T>)Control;

        protected override Drawable CreateControl() => new SliderBar();

        protected partial class SliderBar : RoundedSliderBar<T>
        {
            public SliderBar()
            {
                RelativeSizeAxes = Axes.X;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                AccentColour = colours.Yellow;
                Nub.AccentColour = colours.Yellow;
                Nub.GlowingAccentColour = colours.YellowLighter;
                Nub.GlowColour = colours.YellowDark;
            }
        }
    }
}
