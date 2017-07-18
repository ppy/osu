﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;

namespace osu.Game.Screens.Play
{
    public class SongProgressBar : SliderBar<double>
    {
        public Action<double> OnSeek;

        private readonly Box fill;
        private readonly Container handleBase;

        public Color4 FillColour
        {
            set { fill.Colour = value; }
        }

        public double StartTime
        {
            set { CurrentNumber.MinValue = value; }
        }

        public double EndTime
        {
            set { CurrentNumber.MaxValue = value; }
        }

        public double CurrentTime
        {
            set { CurrentNumber.Value = value; }
        }

        public SongProgressBar(float barHeight, float handleBarHeight, Vector2 handleSize)
        {
            CurrentNumber.MinValue = 0;
            CurrentNumber.MaxValue = 1;

            RelativeSizeAxes = Axes.X;
            Height = barHeight + handleBarHeight + handleSize.Y;

            Children = new Drawable[]
            {
                new Box
                {
                    Name = "Background",
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.X,
                    Height = barHeight,
                    Colour = Color4.Black,
                    Alpha = 0.5f,
                    Depth = 1,
                },
                fill = new Box
                {
                    Name = "Fill",
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Height = barHeight,
                },
                handleBase = new Container
                {
                    Name = "HandleBar container",
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.BottomLeft,
                    Width = 2,
                    Height = barHeight + handleBarHeight,
                    Colour = Color4.White,
                    Position = new Vector2(2, 0),
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Name = "HandleBar box",
                            RelativeSizeAxes = Axes.Both,
                        },
                        new Container
                        {
                            Name = "Handle container",
                            Origin = Anchor.BottomCentre,
                            Anchor = Anchor.TopCentre,
                            Size = handleSize,
                            CornerRadius = 5,
                            Masking = true,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Name = "Handle box",
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4.White
                                }
                            }
                        }
                    }
                }
            };
        }

        protected override void UpdateValue(float value)
        {
            var xFill = value * UsableWidth;
            fill.Width = xFill;
            handleBase.MoveToX(xFill);
        }

        protected override void OnUserChange() => OnSeek?.Invoke(Current);
    }
}
