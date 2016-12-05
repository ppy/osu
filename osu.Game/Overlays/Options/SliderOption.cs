﻿using System;
using OpenTK.Graphics;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Overlays.Options
{
    public class SliderOption<T> : FlowContainer where T : struct,
        IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T>
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
            set { slider.Bindable = value; }
        }

        public SliderOption()
        {
            Direction = FlowDirection.VerticalOnly;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Children = new Drawable[]
            {
                text = new SpriteText { Alpha = 0 },
                slider = new SliderBar<T>
                {
                    Margin = new MarginPadding { Top = 5 },
                    Height = 10,
                    RelativeSizeAxes = Axes.X,
                    Color = Color4.White,
                    SelectionColor = new Color4(255, 102, 170, 255),
                }
            };
        }
    }
}