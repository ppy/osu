// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Screens.Edit.Components.Timelines.Summary.Parts
{
    public partial class GroupVisualisation : CompositeDrawable
    {
        public readonly ControlPointGroup Group;

        private readonly IBindableList<ControlPoint> controlPoints = new BindableList<ControlPoint>();

        public GroupVisualisation(ControlPointGroup group)
        {
            RelativePositionAxes = Axes.X;

            RelativeSizeAxes = Axes.Both;
            Origin = Anchor.TopLeft;

            Group = group;
            X = (float)group.Time;

            // Run in constructor so IsRedundant calls can work correctly.
            controlPoints.BindTo(Group.ControlPoints);
            controlPoints.BindCollectionChanged((_, _) =>
            {
                ClearInternal();

                if (controlPoints.Count == 0)
                    return;

                foreach (var point in Group.ControlPoints)
                {
                    switch (point)
                    {
                        case TimingControlPoint:
                            AddInternal(new ControlPointVisualisation(point) { Y = 0, });
                            break;

                        case DifficultyControlPoint:
                            AddInternal(new ControlPointVisualisation(point) { Y = 0.25f, });
                            break;

                        case SampleControlPoint:
                            AddInternal(new ControlPointVisualisation(point) { Y = 0.5f, });
                            break;

                        case EffectControlPoint effect:
                            AddInternal(new EffectPointVisualisation(effect) { Y = 0.75f });
                            break;
                    }
                }
            }, true);
        }

        /// <summary>
        /// For display purposes, check whether the proposed group is made redundant by this visualisation group.
        /// </summary>
        public bool IsVisuallyRedundant(ControlPointGroup other) =>
            other.ControlPoints.All(c => InternalChildren.OfType<IControlPointVisualisation>().Any(c2 => c2.IsVisuallyRedundant(c)));
    }
}
