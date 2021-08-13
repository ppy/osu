// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Screens.Edit.Components.Timelines.Summary.Visualisations;

namespace osu.Game.Screens.Edit.Components.Timelines.Summary.Parts
{
    public class ControlPointVisualisation : PointVisualisation
    {
        protected readonly ControlPoint Point;

        public ControlPointVisualisation(ControlPoint point)
        {
            Point = point;

            Height = 0.25f;
            Origin = Anchor.TopCentre;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = Point.GetRepresentingColour(colours);
        }
    }
}
