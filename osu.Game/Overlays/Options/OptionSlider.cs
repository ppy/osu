﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Options
{
    public class OptionSlider<T> : FillFlowContainer where T : struct
    {
        private SliderBar<T> slider;
        private SpriteText text;

        public string LabelText
        {
            get { return text.Text; }
            set
            {
                text.Text = value;
                text.Alpha = string.IsNullOrEmpty(value) ? 0 : 1;
            }
        }

        public BindableNumber<T> Bindable
        {
            get { return slider.Bindable; }
            set
            {
                slider.Bindable = value;
                if (value?.Disabled ?? true)
                    Alpha = 0.3f;
            }
        }

        public OptionSlider()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Padding = new MarginPadding { Right = 5 };

            Children = new Drawable[]
            {
                text = new OsuSpriteText
                {
                    Alpha = 0,
                },
                slider = new OsuSliderBar<T>
                {
                    Margin = new MarginPadding { Top = 5, Bottom = 5 },
                    RelativeSizeAxes = Axes.X
                }
            };
        }
    }
}
