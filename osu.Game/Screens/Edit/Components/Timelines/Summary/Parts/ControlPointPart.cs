// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Bindables;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Screens.Edit.Components.Timelines.Summary.Parts
{
    /// <summary>
    /// The part of the timeline that displays the control points.
    /// </summary>
    public class ControlPointPart : TimelinePart<GroupVisualisation>
    {
        private readonly IBindableList<ControlPointGroup> controlPointGroups = new BindableList<ControlPointGroup>();

        protected override void LoadBeatmap(EditorBeatmap beatmap)
        {
            base.LoadBeatmap(beatmap);

            controlPointGroups.UnbindAll();
            controlPointGroups.BindTo(beatmap.ControlPointInfo.Groups);
            controlPointGroups.BindCollectionChanged((sender, args) =>
            {
                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Reset:
                        Clear();
                        break;

                    case NotifyCollectionChangedAction.Add:
                        foreach (var group in args.NewItems.OfType<ControlPointGroup>())
                        {
                            // as an optimisation, don't add a visualisation if there are already groups with the same types in close proximity.
                            // for newly added control points (ie. lazer editor first where group is added empty) we always skip for simplicity.
                            // that is fine, because cases where this is causing a performance issue are mostly where external tools were used to create an insane number of points.
                            if (Children.Any(g => Math.Abs(g.Group.Time - group.Time) < 500 && g.IsVisuallyRedundant(group)))
                                continue;

                            Add(new GroupVisualisation(group));
                        }

                        break;

                    case NotifyCollectionChangedAction.Remove:
                        foreach (var group in args.OldItems.OfType<ControlPointGroup>())
                        {
                            var matching = Children.SingleOrDefault(gv => gv.Group == group);

                            if (matching != null)
                                matching.Expire();
                            else
                            {
                                // due to the add optimisation above, if a point is deleted which wasn't being displayed we need to recreate all points
                                // to guarantee an accurate representation.
                                //
                                // note that the case where control point (type) is added or removed from a non-displayed group is not handled correctly.
                                // this is an edge case which shouldn't affect the user too badly. we may flatten control point groups in the future
                                // which would allow this to be handled better.
                                Clear();
                                foreach (var g in controlPointGroups)
                                    Add(new GroupVisualisation(g));
                            }
                        }

                        break;
                }
            }, true);
        }
    }
}
