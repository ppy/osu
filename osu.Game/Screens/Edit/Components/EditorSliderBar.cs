// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Input;
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
        private bool isUsingAlternatePrecision;
        private bool IsUsingAlternatePrecision
        {
            get => isUsingAlternatePrecision;
            set
            {
                isUsingAlternatePrecision = value;
                if (typeof(T) == typeof(float))
                {
                    float? newValue = value ? alternatePrecision as float? : normalPrecision as float?;
                    if (Bar.Current is BindableFloat && newValue != null)
                        (Bar.Current as BindableFloat).Precision = (float)newValue;
                }
                else if (typeof(T) == typeof(double))
                {
                    double? newValue = value ? alternatePrecision as double? : normalPrecision as double?;
                    if (Bar.Current is BindableDouble && newValue != null)
                        (Bar.Current as BindableDouble).Precision = (double)newValue;
                }
            }
        }
        private T normalPrecision;
        public T NormalPrecision
        {
            get => normalPrecision;
            set
            {
                normalPrecision = value;
            }
        }
        private T alternatePrecision;
        public T AlternatePrecision
        {
            get => alternatePrecision;
            set
            {
                alternatePrecision = value;
            }
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

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            IsUsingAlternatePrecision = state.Keyboard.ShiftPressed;
            
            return base.OnKeyDown(state, args);
        }
        protected override bool OnKeyUp(InputState state, KeyUpEventArgs args)
        {
            IsUsingAlternatePrecision = state.Keyboard.ShiftPressed;

            return base.OnKeyUp(state, args);
        }

        public class Sliderbar : OsuSliderBar<T>
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
