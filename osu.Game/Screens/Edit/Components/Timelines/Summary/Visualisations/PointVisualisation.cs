// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Screens.Edit.Components.Timelines.Summary.Visualisations
{
    /// <summary>
    /// Represents a singular point on a timeline part.
    /// </summary>
    public partial class PointVisualisation : FastCircle
    {
        public readonly double StartTime;

        public const float MAX_WIDTH = 4;

        public PointVisualisation(double startTime)
        {
            RelativePositionAxes = Axes.Both;
            RelativeSizeAxes = Axes.Y;

            Anchor = Anchor.CentreLeft;
            Origin = Anchor.Centre;

            Width = MAX_WIDTH;
            Height = 0.4f;

            X = (float)startTime;
            StartTime = startTime;
        }
    }
}
