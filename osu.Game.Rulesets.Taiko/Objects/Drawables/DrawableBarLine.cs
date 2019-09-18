// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Objects;
using osuTK;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    /// <summary>
    /// A line that scrolls alongside hit objects in the playfield and visualises control points.
    /// </summary>
    public class DrawableBarLine : DrawableHitObject<HitObject>
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
            : base(barLine)
        {
            BarLine = barLine;

            Anchor = Anchor.CentreLeft;
            Origin = Anchor.Centre;

            RelativeSizeAxes = Axes.Y;
            Width = tracker_width;

            AddInternal(Tracker = new Box
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                EdgeSmoothness = new Vector2(0.5f, 0),
                Alpha = 0.75f
            });
        }
    }
}
