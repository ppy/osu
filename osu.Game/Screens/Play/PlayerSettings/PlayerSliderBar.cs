// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;

namespace osu.Game.Screens.Play.PlayerSettings
{
    public partial class PlayerSliderBar<T> : SettingsSlider<T>
        where T : struct, IEquatable<T>, IComparable<T>, IConvertible
    {
        public NormalSliderBar<T> Bar => (NormalSliderBar<T>)Control;

        protected override Drawable CreateControl() => new SliderBar();

        protected partial class SliderBar : NormalSliderBar<T>
        {
            public SliderBar()
            {
                RelativeSizeAxes = Axes.X;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                AccentColour = colours.Yellow;
                NormalNub.AccentColour = colours.Yellow;
                NormalNub.GlowingAccentColour = colours.YellowLighter;
                NormalNub.GlowColour = colours.YellowDark;
            }
        }
    }
}
