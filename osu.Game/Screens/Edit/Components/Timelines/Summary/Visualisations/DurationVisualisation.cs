// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Screens.Edit.Components.Timelines.Summary.Visualisations
{
    /// <summary>
    /// Represents a spanning point on a timeline part.
    /// </summary>
    public partial class DurationVisualisation : Circle
    {
        protected DurationVisualisation(double startTime, double endTime)
        {
            RelativePositionAxes = Axes.X;
            RelativeSizeAxes = Axes.Both;

            X = (float)startTime;
            Width = (float)(endTime - startTime);
        }
    }
}
