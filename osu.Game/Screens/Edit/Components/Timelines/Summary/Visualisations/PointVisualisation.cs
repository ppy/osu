// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Screens.Edit.Components.Timelines.Summary.Visualisations
{
    /// <summary>
    /// Represents a singular point on a timeline part.
    /// </summary>
    public class PointVisualisation : Box
    {
        protected PointVisualisation(double startTime)
        {
            Origin = Anchor.TopCentre;

            RelativeSizeAxes = Axes.Y;
            Width = 1;
            EdgeSmoothness = new Vector2(1, 0);

            RelativePositionAxes = Axes.X;
            X = (float)startTime;
        }
    }
}
