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
        public DrawableMajorBarLine(BarLine barLine)
            : base(barLine)
        {
            Add(new Triangle
            {
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.TopCentre,
                Position = new Vector2(0, 10),
                EdgeSmoothness = new Vector2(1),
                // Scaling height by 0.866 results in equiangular triangles (== 60° and equal side length)
                Size = new Vector2(20, 0.866f * 20),
            });

            Add(new Triangle
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Position = new Vector2(0, -10),
                EdgeSmoothness = new Vector2(1),
                // Scaling height by 0.866 results in equiangular triangles (== 60° and equal side length)
                Size = new Vector2(20, 0.866f * -20),
            });

            Tracker.Alpha = 0.5f;
        }
    }

    public class DrawableBarLine : Container
    {
        protected Box Tracker;

        private BarLine barLine;

        public DrawableBarLine(BarLine barLine)
        {
            this.barLine = barLine;

            Anchor = Anchor.CentreLeft;
            Origin = Anchor.Centre;

            RelativePositionAxes = Axes.X;
            Size = new Vector2(1, TaikoPlayfield.PLAYFIELD_BASE_HEIGHT);

            Children = new[]
            {
                Tracker = new Box()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,

                    RelativeSizeAxes = Axes.Both,
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            LifetimeStart = barLine.StartTime - barLine.PreEmpt * 2;
            LifetimeEnd = barLine.StartTime + barLine.PreEmpt;
        }

        protected virtual void MoveToOffset(double time)
        {
            MoveToX((float)((barLine.StartTime - time) / barLine.PreEmpt));
        }

        protected override void Update()
        {
            base.Update();

            MoveToOffset(Time.Current);
        }
    }
}
