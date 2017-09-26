using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Screens.Edit.Components.Timelines.Summary.Visualisations
{
    /// <summary>
    /// Represents a singular point on a timeline part.
    /// </summary>
    internal class PointVisualisation : Box
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
