// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osuTK;

namespace osu.Game.Screens.Edit.Components.Timelines.Summary.Visualisations
{
    /// <summary>
    /// Represents a singular point on a timeline part.
    /// </summary>
    public class PointVisualisation : Box
    {
        public const float WIDTH = 1;

        public PointVisualisation(double startTime)
            : this()
        {
            X = (float)startTime;
        }

        public PointVisualisation()
        {
            Origin = Anchor.TopCentre;

            RelativePositionAxes = Axes.X;
            RelativeSizeAxes = Axes.Y;

            Width = WIDTH;
            EdgeSmoothness = new Vector2(WIDTH, 0);
        }
    }
}
