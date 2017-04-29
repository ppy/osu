// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using OpenTK;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    /// <summary>
    /// A line that scrolls alongside hit objects in the playfield and visualises control points.
    /// </summary>
    public class DrawableBarLine : Container
    {
        /// <summary>
        /// The width of the line tracker.
        /// </summary>
        private const float tracker_width = 2f;

        /// <summary>
        /// Fade out time calibrated to a pre-empt of 1000ms.
        /// </summary>
        private const float base_fadeout_time = 100f;

        /// <summary>
        /// The visual line tracker.
        /// </summary>
        protected Box Tracker;

        /// <summary>
        /// The bar line.
        /// </summary>
        protected readonly BarLine BarLine;

        public DrawableBarLine(BarLine barLine)
        {
            BarLine = barLine;

            Anchor = Anchor.CentreLeft;
            Origin = Anchor.Centre;

            RelativePositionAxes = Axes.X;
            RelativeSizeAxes = Axes.Y;

            Width = tracker_width;

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

            LifetimeStart = BarLine.StartTime - BarLine.ScrollTime * 2;
            LifetimeEnd = BarLine.StartTime + BarLine.ScrollTime;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Delay(BarLine.StartTime - Time.Current);
            FadeOut(base_fadeout_time * BarLine.ScrollTime / 1000);
        }

        private void updateScrollPosition(double time) => MoveToX((float)((BarLine.StartTime - time) / BarLine.ScrollTime));

        protected override void Update()
        {
            base.Update();

            updateScrollPosition(Time.Current);
        }
    }
}