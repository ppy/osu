// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Screens.Edit.Components.Timelines.Summary.Parts;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    /// <summary>
    /// The part of the timeline that displays the control points.
    /// </summary>
    public class TimelineControlPointDisplay : TimelinePart<TimelineControlPointGroup>
    {
        private readonly IBindableList<ControlPointGroup> controlPointGroups = new BindableList<ControlPointGroup>();

        public TimelineControlPointDisplay()
        {
            RelativeSizeAxes = Axes.Both;
        }

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
                            Add(new TimelineControlPointGroup(group));
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
