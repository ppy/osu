// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public partial class TimelineControlPointGroup : CompositeDrawable
    {
        public readonly ControlPointGroup Group;

        private readonly IBindableList<ControlPoint> controlPoints = new BindableList<ControlPoint>();

        public TimelineControlPointGroup(ControlPointGroup group)
        {
            Group = group;

            RelativePositionAxes = Axes.X;
            RelativeSizeAxes = Axes.Y;
            AutoSizeAxes = Axes.X;

            Origin = Anchor.TopLeft;

            X = (float)group.Time;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            controlPoints.BindTo(Group.ControlPoints);
            controlPoints.BindCollectionChanged((_, _) =>
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
