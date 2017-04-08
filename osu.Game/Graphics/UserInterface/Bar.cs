// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using System;

namespace osu.Game.Graphics.UserInterface
{
    public class Bar : Container
    {
        private Box background;
        private readonly Box bar;

        private const int resize_duration = 250;

        private const EasingTypes easing = EasingTypes.InOutCubic;

        private float length;
        public float Length
        {
            get
            {
                return length;
            }
            set
            {
                length = MathHelper.Clamp(value, 0, 1);
                updateBarLength();
            }
        }

        public Color4 BackgroundColour
        {
            get
            {
                return background?.Colour ?? default(Color4);
            }
            set
            {
                if (background == null)
                    Add(background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Depth = 1,
                    });
                background.Colour = value;
            }
        }

        public Color4 BarColour
        {
            get
            {
                return bar.Colour;
            }
            set
            {
                bar.Colour = value;
            }
        }

        private BarDirection direction = BarDirection.LeftToRight;
        public BarDirection Direction
        {
            get
            {
                return direction;
            }
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
                bar = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                }
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

    [Flags]
    public enum BarDirection
    {
        LeftToRight = 1 << 0,
        RightToLeft = 1 << 1,
        TopToBottom = 1 << 2,
        BottomToTop = 1 << 3,

        Vertical = TopToBottom | BottomToTop,
        Horizontal = LeftToRight | RightToLeft,
    }
}