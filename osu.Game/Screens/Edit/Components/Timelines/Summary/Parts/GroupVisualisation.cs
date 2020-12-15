// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Screens.Edit.Components.Timelines.Summary.Visualisations;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit.Components.Timelines.Summary.Parts
{
    public class GroupVisualisation : PointVisualisation
    {
        public readonly ControlPointGroup Group;

        private readonly IBindableList<ControlPoint> controlPoints = new BindableList<ControlPoint>();

        [Resolved]
        private OsuColour colours { get; set; }

        public GroupVisualisation(ControlPointGroup group)
            : base(group.Time)
        {
            Group = group;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            controlPoints.BindTo(Group.ControlPoints);
            controlPoints.BindCollectionChanged((_, __) =>
            {
                if (controlPoints.Count == 0)
                {
                    Colour = Color4.Transparent;
                    return;
                }

                Colour = controlPoints.Any(c => c is TimingControlPoint) ? colours.YellowDark : colours.Green;
            }, true);
        }
    }
}
