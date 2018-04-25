// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Screens.Edit.Components
{
    public class EditorSliderBar<T> : SettingsSlider<T>
        where T : struct, IEquatable<T>, IComparable, IConvertible
    {
        public EditorSliderBar()
        {

        }

        public Sliderbar Bar => (Sliderbar)Control;

        protected override Drawable CreateControl()
        {
            Sliderbar s = new Sliderbar
            {
                Margin = new MarginPadding { Top = 5, Bottom = 5 },
                RelativeSizeAxes = Axes.X,
            };
            return s;
        }
        
        public class Sliderbar : OsuSliderBar<T>
        {
            // Do something for showing a more proper tool tip
            private Color4 mainColour;
            private Color4 darkerColour;
            private Color4 lighterColour;
            
            public Color4 MainColour
            {
                get => mainColour;
                set
                {
                    mainColour = value;
                    AccentColour = mainColour;
                    DarkerColour = mainColour.Darken(0.1f);
                    LighterColour = mainColour.Lighten(0.1f);
                }
            }
            public Color4 DarkerColour
            {
                get => darkerColour;
                set
                {
                    darkerColour = value;
                    if (Nub != null)
                        Nub.GlowingAccentColour = darkerColour;
                }
            }
            public Color4 LighterColour
            {
                get => lighterColour;
                set
                {
                    lighterColour = value;
                    if (Nub != null)
                        Nub.GlowingAccentColour = darkerColour;
                }
            }
            
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
