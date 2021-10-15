// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public class TimelineControlPointGroup : CompositeDrawable
    {
        public readonly ControlPointGroup Group;

        private readonly IBindableList<ControlPoint> controlPoints = new BindableList<ControlPoint>();

        [Resolved]
        private OsuColour colours { get; set; }

        public TimelineControlPointGroup(ControlPointGroup group)
        {
            Group = group;

            RelativePositionAxes = Axes.X;
            RelativeSizeAxes = Axes.Y;
            AutoSizeAxes = Axes.X;

            Origin = Anchor.TopCentre;

            X = (float)group.Time;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            controlPoints.BindTo(Group.ControlPoints);
            controlPoints.BindCollectionChanged((_, __) =>
            {
                ClearInternal();

                foreach (var point in controlPoints)
                {
                    switch (point)
                    {
                        case TimingControlPoint timingPoint:
                            AddInternal(new TimingPointPiece(timingPoint));
                            break;
                    }
                }
            }, true);
        }
    }
}
