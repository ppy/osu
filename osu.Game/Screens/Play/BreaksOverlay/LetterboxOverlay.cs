﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps.Timing;

namespace osu.Game.Screens.Play.BreaksOverlay
{
    public class LetterboxOverlay : VisibilityContainer
    {
        private const double fade_duration = BreakPeriod.MIN_BREAK_DURATION / 2;
        private const int height = 350;

        private static readonly Color4 transparent_black = new Color4(0, 0, 0, 0);

        public LetterboxOverlay()
        {
            RelativeSizeAxes = Axes.Both;
            Children = new Drawable[]
            {
                new Container
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    RelativeSizeAxes = Axes.X,
                    Height = height,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = new ColourInfo
                        {
                            TopLeft = Color4.Black,
                            TopRight = Color4.Black,
                            BottomLeft = transparent_black,
                            BottomRight = transparent_black,
                        }
                    }
                },
                new Container
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.X,
                    Height = height,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = new ColourInfo
                        {
                            TopLeft = transparent_black,
                            TopRight = transparent_black,
                            BottomLeft = Color4.Black,
                            BottomRight = Color4.Black,
                        }
                    }
                }
            };
        }

        protected override void PopIn() => this.FadeIn(fade_duration);
        protected override void PopOut() => this.FadeOut(fade_duration);
    }
}
