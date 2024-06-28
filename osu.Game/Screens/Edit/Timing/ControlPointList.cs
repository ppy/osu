// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.Edit.Timing
{
    public partial class ControlPointList : CompositeDrawable
    {
        private OsuButton deleteButton = null!;
        private RoundedButton addButton = null!;

        [Resolved]
        private EditorClock clock { get; set; } = null!;

        [Resolved]
        protected EditorBeatmap Beatmap { get; private set; } = null!;

        [Resolved]
        private Bindable<ControlPointGroup?> selectedGroup { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colours)
        {
            RelativeSizeAxes = Axes.Both;

            const float margins = 10;
            InternalChildren = new Drawable[]
            {
                new ControlPointTable
                {
                    RelativeSizeAxes = Axes.Both,
                    Groups = { BindTarget = Beatmap.ControlPointInfo.Groups, },
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Direction = FillDirection.Horizontal,
                    Margin = new MarginPadding(margins),
                    Spacing = new Vector2(5),
                    Children = new Drawable[]
                    {
                        deleteButton = new RoundedButton
                        {
                            Text = "-",
                            Size = new Vector2(30, 30),
                            Action = delete,
                            Anchor = Anchor.BottomRight,
                            Origin = Anchor.BottomRight,
                        },
                        addButton = new RoundedButton
                        {
                            Action = addNew,
                            Size = new Vector2(160, 30),
                            Anchor = Anchor.BottomRight,
                            Origin = Anchor.BottomRight,
                        },
                    }
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            selectedGroup.BindValueChanged(selected =>
            {
                deleteButton.Enabled.Value = selected.NewValue != null;

                addButton.Text = selected.NewValue != null
                    ? "+ Clone to current time"
                    : "+ Add at current time";
            }, true);
        }

        protected override bool OnClick(ClickEvent e)
        {
            selectedGroup.Value = null;
            return true;
        }

        protected override void Update()
        {
            base.Update();

            trackActivePoint();

            addButton.Enabled.Value = clock.CurrentTimeAccurate != selectedGroup.Value?.Time;
        }

        private Type? trackedType;

        /// <summary>
        /// Given the user has selected a control point group, we want to track any group which is
        /// active at the current point in time which matches the type the user has selected.
        ///
        /// So if the user is currently looking at a timing point and seeks into the future, a
        /// future timing point would be automatically selected if it is now the new "current" point.
        /// </summary>
        private void trackActivePoint()
        {
            // For simplicity only match on the first type of the active control point.
            if (selectedGroup.Value == null)
                trackedType = null;
            else
            {
                switch (selectedGroup.Value.ControlPoints.Count)
                {
                    // If the selected group has no control points, clear the tracked type.
                    // Otherwise the user will be unable to select a group with no control points.
                    case 0:
                        trackedType = null;
                        break;

                    // If the selected group only has one control point, update the tracking type.
                    case 1:
                        trackedType = selectedGroup.Value?.ControlPoints[0].GetType();
                        break;

                    // If the selected group has more than one control point, choose the first as the tracking type
                    // if we don't already have a singular tracked type.
                    default:
                        trackedType ??= selectedGroup.Value?.ControlPoints[0].GetType();
                        break;
                }
            }

            if (trackedType != null)
            {
                double accurateTime = clock.CurrentTimeAccurate;

                // We don't have an efficient way of looking up groups currently, only individual point types.
                // To improve the efficiency of this in the future, we should reconsider the overall structure of ControlPointInfo.

                // Find the next group which has the same type as the selected one.
                ControlPointGroup? found = null;

                for (int i = 0; i < Beatmap.ControlPointInfo.Groups.Count; i++)
                {
                    var g = Beatmap.ControlPointInfo.Groups[i];

                    if (g.Time > accurateTime)
                        continue;

                    for (int j = 0; j < g.ControlPoints.Count; j++)
                    {
                        if (g.ControlPoints[j].GetType() == trackedType)
                        {
                            found = g;
                            break;
                        }
                    }
                }

                if (found != null)
                    selectedGroup.Value = found;
            }
        }

        private void delete()
        {
            if (selectedGroup.Value == null)
                return;

            Beatmap.ControlPointInfo.RemoveGroup(selectedGroup.Value);

            selectedGroup.Value = Beatmap.ControlPointInfo.Groups.FirstOrDefault(g => g.Time >= clock.CurrentTime);
        }

        private void addNew()
        {
            bool isFirstControlPoint = !Beatmap.ControlPointInfo.TimingPoints.Any();

            var group = Beatmap.ControlPointInfo.GroupAt(clock.CurrentTime, true);

            if (isFirstControlPoint)
                group.Add(new TimingControlPoint());
            else
            {
                // Try and create matching types from the currently selected control point.
                var selected = selectedGroup.Value;

                if (selected != null && !ReferenceEquals(selected, group))
                {
                    foreach (var controlPoint in selected.ControlPoints)
                    {
                        group.Add(controlPoint.DeepClone());
                    }
                }
            }

            selectedGroup.Value = group;
        }
    }
}
