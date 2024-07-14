// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Graphics.UserInterface
{
    public partial class Bar : Container, IHasAccentColour
    {
        private readonly Box background;
        private readonly Box bar;

        private const int resize_duration = 250;

        private const Easing easing = Easing.InOutCubic;

        private float length;

        /// <summary>
        /// Length of the bar, ranges from 0 to 1
        /// </summary>
        public float Length
        {
            get => length;
            set
            {
                length = Math.Clamp(value, 0, 1);
                updateBarLength();
            }
        }

        public Color4 BackgroundColour
        {
            get => background.Colour;
            set => background.Colour = value;
        }

        public Color4 AccentColour
        {
            get => bar.Colour;
            set => bar.Colour = value;
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

        public Bar()
        {
            Children = new[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = new Color4(0, 0, 0, 0)
                },
                bar = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 0,
                },
            };
        }

        private void updateBarLength()
        {
            switch (direction)
            {
                case BarDirection.LeftToRight:
                case BarDirection.RightToLeft:
                    bar.ResizeTo(new Vector2(length, 1), resize_duration, easing);
                    break;

                case BarDirection.TopToBottom:
                case BarDirection.BottomToTop:
                    bar.ResizeTo(new Vector2(1, length), resize_duration, easing);
                    break;
            }

            switch (direction)
            {
                case BarDirection.LeftToRight:
                case BarDirection.TopToBottom:
                    bar.Anchor = Anchor.TopLeft;
                    bar.Origin = Anchor.TopLeft;
                    break;

                case BarDirection.RightToLeft:
                case BarDirection.BottomToTop:
                    bar.Anchor = Anchor.BottomRight;
                    bar.Origin = Anchor.BottomRight;
                    break;
            }
        }
    }

    public enum BarDirection
    {
        LeftToRight,
        RightToLeft,
        TopToBottom,
        BottomToTop
    }
}
