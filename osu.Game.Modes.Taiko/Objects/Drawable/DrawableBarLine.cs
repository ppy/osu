// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using OpenTK;

namespace osu.Game.Modes.Taiko.Objects.Drawable
{
    /// <summary>
    /// A line that scrolls alongside hit objects in the playfield and visualises control points.
    /// </summary>
    public class DrawableBarLine : Container
    {
        /// <summary>
        /// The line.
        /// </summary>
        protected Box Tracker;

        /// <summary>
        /// The 
        /// </summary>
        protected readonly BarLine BarLine;

        public DrawableBarLine(BarLine barLine)
        {
            BarLine = barLine;

            Anchor = Anchor.CentreLeft;
            Origin = Anchor.Centre;

            RelativePositionAxes = Axes.X;
            RelativeSizeAxes = Axes.Y;

            Width = 2f;

            Children = new[]
            {
                Tracker = new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,

                    RelativeSizeAxes = Axes.Both,

                    EdgeSmoothness = new Vector2(0.5f, 0),
                    Alpha = 0.75f
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            LifetimeStart = BarLine.StartTime - BarLine.PreEmpt * 2;
            LifetimeEnd = BarLine.StartTime + BarLine.PreEmpt;

            Delay(BarLine.StartTime - Time.Current);
            FadeOut(100 * BarLine.PreEmpt / 1000);
        }

        private void moveToTimeOffset(double time) => MoveToX((float)((BarLine.StartTime - time) / BarLine.PreEmpt));

        protected override void Update()
        {
            base.Update();
            moveToTimeOffset(Time.Current);
        }
    }
}