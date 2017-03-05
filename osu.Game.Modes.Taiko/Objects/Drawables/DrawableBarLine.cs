// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Modes.Taiko.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Modes.Taiko.Objects.Drawables
{
    public class DrawableMajorBarLine : DrawableBarLine
    {
        private Container arrows;

        public DrawableMajorBarLine(BarLine barLine)
            : base(barLine)
        {
            Add(arrows = new Container()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,

                RelativeSizeAxes = Axes.Both,

                Children = new[]
                {
                    // Top
                    new Triangle
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Position = new Vector2(0, -10),
                        EdgeSmoothness = new Vector2(1),
                        // Scaling height by 0.866 results in equiangular triangles (== 60° and equal side length)
                        Size = new Vector2(20, 0.866f * -20),
                    },
                    // Bottom
                    new Triangle
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.TopCentre,
                        Position = new Vector2(0, 10),
                        EdgeSmoothness = new Vector2(1),
                        // Scaling height by 0.866 results in equiangular triangles (== 60° and equal side length)
                        Size = new Vector2(20, 0.866f * 20),
                    }
                }
            });

            Tracker.Alpha = 1f;
        }
    }

    public class DrawableBarLine : Container
    {
        protected Box Tracker;

        protected readonly BarLine BarLine;

        public DrawableBarLine(BarLine barLine)
        {
            BarLine = barLine;

            Anchor = Anchor.CentreLeft;
            Origin = Anchor.Centre;

            RelativePositionAxes = Axes.X;
            Size = new Vector2(1, TaikoPlayfield.PLAYFIELD_BASE_HEIGHT);

            Children = new[]
            {
                Tracker = new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,

                    RelativeSizeAxes = Axes.Both,

                    Alpha = 0.5f
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            LifetimeStart = BarLine.StartTime - BarLine.PreEmpt * 2;
            LifetimeEnd = BarLine.StartTime + BarLine.PreEmpt;

            Delay(BarLine.StartTime);
            FadeOut(100 * BarLine.PreEmpt / 1000);
        }

        protected virtual void MoveToOffset(double time)
        {
            MoveToX((float)((BarLine.StartTime - time) / BarLine.PreEmpt));
        }

        protected override void Update()
        {
            base.Update();

            MoveToOffset(Time.Current);
        }
    }
}
