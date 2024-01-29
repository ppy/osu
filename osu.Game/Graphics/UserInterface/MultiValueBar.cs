// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Graphics.UserInterface
{
    // A modified implementation of Bar which supports multiple accent bars
    public partial class MultiValueBar : Container
    {
        public partial class SingleBarValue : Box
        {
            private const Easing easing = Easing.InOutCubic;

            private const int resize_duration = 250;

            private float length;

            public float Length
            {
                get => length;
                set
                {
                    length = Math.Clamp(value, 0, 1);
                    updateBarLength();
                }
            }

            private BarDirection direction = BarDirection.LeftToRight;

            public BarDirection Direction
            {
                get => direction;
                set
                {
                    direction = value;
                    updateBarLength();
                }
            }

            private void updateBarLength()
            {
                switch (direction)
                {
                    case BarDirection.LeftToRight:
                    case BarDirection.RightToLeft:
                        this.ResizeTo(new Vector2(Length, 1), resize_duration, easing);
                        break;

                    case BarDirection.TopToBottom:
                    case BarDirection.BottomToTop:
                        this.ResizeTo(new Vector2(1, Length), resize_duration, easing);
                        break;
                }

                switch (direction)
                {
                    case BarDirection.LeftToRight:
                    case BarDirection.TopToBottom:
                        Anchor = Anchor.TopLeft;
                        Origin = Anchor.TopLeft;
                        break;

                    case BarDirection.RightToLeft:
                    case BarDirection.BottomToTop:
                        Anchor = Anchor.BottomRight;
                        Origin = Anchor.BottomRight;
                        break;
                }
            }
        }

        private readonly Box background;

        public readonly SingleBarValue[] Bars;

        private readonly int barCount;

        public Color4 BackgroundColour
        {
            get => background.Colour;
            set => background.Colour = value;
        }

        public MultiValueBar(int barCount, float[] lengths, Color4[] colours)
        {
            this.barCount = barCount;

            List<Drawable> children = new List<Drawable>();
            background = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = new Color4(0, 0, 0, 0),
            };
            children.Add(background);

            Bars = new SingleBarValue[barCount];

            for (int i = 0; i < barCount; i++)
            {
                Bars[i] = new SingleBarValue
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 0,
                    Length = lengths[i],
                    Colour = colours[i]
                };
                children.Add(Bars[i]);
            }

            Children = children.ToArray();
        }
    }
}