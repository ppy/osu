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
    public class ColouredEditorSliderBar<T> : SettingsSlider<T>
        where T : struct, IEquatable<T>, IComparable, IConvertible
    {
        private Color4 min;
        private Color4 mid;
        private Color4 max;

        public Color4 MinColour
        {
            get => min;
            set
            {
                min = value;
                if (Bar != null)
                    Bar.MinColour = value;
            }
        }
        public Color4 MidColour
        {
            get => mid;
            set
            {
                mid = value;
                if (Bar != null)
                    Bar.MidColour = value;
            }
        }
        public Color4 MaxColour
        {
            get => max;
            set
            {
                max = value;
                if (Bar != null)
                    Bar.MaxColour = value;
            }
        }

        public ColouredEditorSliderBar()
        {
            // Default values for sliders
            MinColour = Color4.YellowGreen;
            MidColour = Color4.Yellow;
            MaxColour = Color4.Orange;
        }
        public ColouredEditorSliderBar(Color4 minColour, Color4 midColour, Color4 maxColour)
        {
            MinColour = minColour;
            MidColour = midColour;
            MaxColour = maxColour;
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
            private Color4 minColour;
            private Color4 midColour;
            private Color4 maxColour;
            private Color4 mainColour;

            public Color4 MinColour
            {
                get => minColour;
                set
                {
                    minColour = value;
                    TriggerMinColourChanged();
                }
            }
            public Color4 MidColour
            {
                get => midColour;
                set
                {
                    midColour = value;
                    TriggerMidColourChanged();
                }
            }
            public Color4 MaxColour
            {
                get => maxColour;
                set
                {
                    maxColour = value;
                    TriggerMaxColourChanged();
                }
            }
            public Color4 MainColour
            {
                // TODO: Cause colours to fade
                get => mainColour;
                set
                {
                    mainColour = value;
                    this.FadeColour(mainColour, 500, Easing.OutQuint);
                    AccentColour = mainColour;
                    if (Nub != null)
                    {
                        bool mustGlow = Nub.Glowing;
                        Nub.FadeColour(mainColour, 500, Easing.OutQuint);
                        Nub.AccentColour = mainColour;
                        Nub.GlowColour = mainColour.Darken(0.8f);
                        Nub.GlowingAccentColour = mainColour.Lighten(0.8f);
                        Nub.Glowing = mustGlow;
                    }
                }
            }

            /// <summary>Occurs when the <seealso cref="MinColour"/> property has changed.</summary>
            public event Action MinColourChanged;
            /// <summary>Occurs when the <seealso cref="MidColour"/> property has changed.</summary>
            public event Action MidColourChanged;
            /// <summary>Occurs when the <seealso cref="MaxColour"/> property has changed.</summary>
            public event Action MaxColourChanged;

            public void TriggerMinColourChanged() => MinColourChanged?.Invoke();
            public void TriggerMidColourChanged() => MidColourChanged?.Invoke();
            public void TriggerMaxColourChanged() => MaxColourChanged?.Invoke();
            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                Current.ValueChanged += TriggerValueChanged;
                ValueChanged += SetValueColour;
                MinColourChanged += SetValueColour;
                MidColourChanged += SetValueColour;
                MaxColourChanged += SetValueColour;
                SetValueColour(); // Initialising the normal colours
            }

            public event Action ValueChanged;
            public virtual void TriggerValueChanged(T param)
            {
                ValueChanged?.Invoke();
            }

            public void SetValueColour()
            {
                Color4 result = new Color4(0, 0, 0, 255);
                var value = Convert.ToSingle(CurrentNumber.Value);
                var minValue = Convert.ToSingle(CurrentNumber.MinValue);
                var maxValue = Convert.ToSingle(CurrentNumber.MaxValue);
                var midValue = (minValue + maxValue) / 2;
                if (value == minValue)
                    result = MinColour;
                else if (value < midValue)
                {
                    var delta = midValue - minValue;
                    Color4 lambdas = new Color4((MidColour.R - MinColour.R) / delta, (MidColour.G - MinColour.G) / delta, (MidColour.B - MinColour.B) / delta, 1); // Contains the 3 lambdas (λR, λG, λB) which are the slopes of the lines
                    result.R = lambdas.R * (value - minValue) + MinColour.R; // R = λR * (value - minValue) + minR
                    result.G = lambdas.G * (value - minValue) + MinColour.G; // G = λG * (value - minValue) + minG
                    result.B = lambdas.B * (value - minValue) + MinColour.B; // B = λB * (value - minValue) + minB
                }
                else if (value == midValue)
                    result = MidColour;
                else if (value < maxValue)
                {
                    var delta = maxValue - midValue;
                    Color4 lambdas = new Color4((MaxColour.R - MidColour.R) / delta, (MaxColour.G - MidColour.G) / delta, (MaxColour.B - MidColour.B) / delta, 1); // Contains the 3 lambdas (λR, λG, λB) which are the slopes of the lines
                    result.R = lambdas.R * (value - midValue) + MidColour.R; // R = λR * (value - midValue) + midR
                    result.G = lambdas.G * (value - midValue) + MidColour.G; // G = λG * (value - midValue) + midG
                    result.B = lambdas.B * (value - midValue) + MidColour.B; // B = λB * (value - midValue) + midB
                }
                else if (value == maxValue)
                    result = MaxColour;
                MainColour = result;
            }
        }
    }
}
