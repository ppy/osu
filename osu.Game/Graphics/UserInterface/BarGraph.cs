// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Graphics.UserInterface
{
    public class BarGraph : FillFlowContainer<Bar>
    {
        private BarDirection direction = BarDirection.BottomToTop;
        public new BarDirection Direction
        {
            get
            {
                return direction;
            }
            set
            {
                direction = value;
                base.Direction = (direction & BarDirection.Horizontal) > 0 ? FillDirection.Vertical : FillDirection.Horizontal;
                foreach (var bar in Children)
                {
                    bar.Size = (direction & BarDirection.Horizontal) > 0 ? new Vector2(1, 1.0f / Children.Count()) : new Vector2(1.0f / Children.Count(), 1);
                    bar.Direction = direction;
                }
            }
        }

        public IEnumerable<float> Values
        {
            set
            {
                List<float> values = value.ToList();
                List<Bar> graphBars = Children.ToList();
                for (int i = 0; i < values.Count; i++)
                    if (graphBars.Count > i)
                    {
                        graphBars[i].Length = values[i] / values.Max();
                        graphBars[i].Size = (direction & BarDirection.Horizontal) > 0 ? new Vector2(1, 1.0f / values.Count) : new Vector2(1.0f / values.Count, 1);
                    }
                    else
                        Add(new Bar
                        {
                            RelativeSizeAxes = Axes.Both,
                            Size = (direction & BarDirection.Horizontal) > 0 ? new Vector2(1, 1.0f / values.Count) : new Vector2(1.0f / values.Count, 1),
                            Length = values[i] / values.Max(),
                            Direction = Direction,
                            BackgroundColour = new Color4(0, 0, 0, 0),
                        });
            }
        }
    }

    public class Bar : Container
    {
        private readonly Box background;
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

        public SRGBColour BackgroundColour
        {
            get
            {
                return background.Colour;
            }
            set
            {
                background.Colour = value;
            }
        }

        public SRGBColour BarColour
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
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
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