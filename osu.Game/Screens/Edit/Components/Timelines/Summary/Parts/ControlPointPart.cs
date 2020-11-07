// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Screens.Edit.Components.Timelines.Summary.Parts
{
    /// <summary>
    /// The part of the timeline that displays the control points.
    /// </summary>
    public class ControlPointPart : TimelinePart<GroupVisualisation>
    {
        private readonly IBindableList<ControlPointGroup> controlPointGroups = new BindableList<ControlPointGroup>();

        protected override void LoadBeatmap(WorkingBeatmap beatmap)
        {
            base.LoadBeatmap(beatmap);

            controlPointGroups.UnbindAll();
            controlPointGroups.BindTo(beatmap.Beatmap.ControlPointInfo.Groups);
            controlPointGroups.BindCollectionChanged((sender, args) =>
            {
                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Reset:
                        Clear();
                        break;

                    case NotifyCollectionChangedAction.Add:
                        foreach (var group in args.NewItems.OfType<ControlPointGroup>())
                            Add(new GroupVisualisation(group));
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        foreach (var group in args.OldItems.OfType<ControlPointGroup>())
                        {
                            var matching = Children.SingleOrDefault(gv => gv.Group == group);

                            matching?.Expire();
                        }

                        break;
                }
            }, true);
        }
    }
}
