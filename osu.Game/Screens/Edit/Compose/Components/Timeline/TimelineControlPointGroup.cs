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

        private BindableList<ControlPoint> controlPoints;

        [Resolved]
        private OsuColour colours { get; set; }

        public TimelineControlPointGroup(ControlPointGroup group)
        {
            Origin = Anchor.TopCentre;

            Group = group;

            RelativePositionAxes = Axes.X;
            RelativeSizeAxes = Axes.Y;

            Width = 1;

            X = (float)group.Time;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            controlPoints = (BindableList<ControlPoint>)Group.ControlPoints.GetBoundCopy();
            controlPoints.BindCollectionChanged((_, __) =>
            {
                foreach (var point in controlPoints)
                {
                    switch (point)
                    {
                        case DifficultyControlPoint difficultyPoint:
                            AddInternal(new DifficultyPointPiece(difficultyPoint));
                            break;
                    }
                }
            }, true);
        }
    }
}
