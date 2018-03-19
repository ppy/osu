// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using OpenTK;

namespace osu.Game.Screens.Edit.Screens.Compose.BeatSnap
{
    public class TickContainer : CompositeDrawable
    {
        public readonly BindableInt Divisor = new BindableInt();

        public new MarginPadding Padding { set => base.Padding = value; }

        private EquilateralTriangle marker;

        private readonly int[] availableDivisors;
        private readonly float tickSpacing;

        public TickContainer(params int[] divisors)
        {
            availableDivisors = divisors;
            tickSpacing = 1f / (availableDivisors.Length + 1);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            InternalChild = marker = new EquilateralTriangle
            {
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomCentre,
                RelativePositionAxes = Axes.X,
                Height = 7,
                EdgeSmoothness = new Vector2(1),
                Colour = colours.Gray4,
            };

            for (int i = 0; i < availableDivisors.Length; i++)
            {
                AddInternal(new Tick(availableDivisors[i])
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopCentre,
                    RelativePositionAxes = Axes.X,
                    X = getTickPosition(i)
                });
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Divisor.ValueChanged += v => updatePosition();
            updatePosition();
        }

        private void updatePosition() => marker.MoveToX(getTickPosition(Array.IndexOf(availableDivisors, Divisor.Value)), 100, Easing.OutQuint);

        private float getTickPosition(int index) => (index + 1) * tickSpacing;

        private class Tick : Box
        {
            private readonly int divisor;

            public Tick(int divisor)
            {
                this.divisor = divisor;

                Size = new Vector2(2, 10);
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                if (divisor >= 16)
                    Colour = colours.Red;
                else if (divisor >= 8)
                    Colour = colours.Yellow;
                else
                    Colour = colours.Gray4;
            }
        }
    }
}
