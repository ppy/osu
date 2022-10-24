// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.Edit.Timing
{
    public class TimingScreen : EditorScreenWithTimeline
    {
        [Cached]
        public readonly Bindable<ControlPointGroup> SelectedGroup = new Bindable<ControlPointGroup>();

        public TimingScreen()
            : base(EditorScreenMode.Timing)
        {
        }

        protected override Drawable CreateMainContent() => new GridContainer
        {
            RelativeSizeAxes = Axes.Both,
            ColumnDimensions = new[]
            {
                new Dimension(),
                new Dimension(GridSizeMode.Absolute, 350),
            },
            Content = new[]
            {
                new Drawable[]
                {
                    new ControlPointList(),
                    new ControlPointSettings(),
                },
            }
        };

        public class ControlPointList : CompositeDrawable
        {
            private OsuButton deleteButton;
            private ControlPointTable table;

            private readonly IBindableList<ControlPointGroup> controlPointGroups = new BindableList<ControlPointGroup>();

            private RoundedButton addButton;

            [Resolved]
            private EditorClock clock { get; set; }

            [Resolved]
            protected EditorBeatmap Beatmap { get; private set; }

            [Resolved]
            private Bindable<ControlPointGroup> selectedGroup { get; set; }

            [Resolved(canBeNull: true)]
            private IEditorChangeHandler changeHandler { get; set; }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colours)
            {
                RelativeSizeAxes = Axes.Both;

                const float margins = 10;
                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        Colour = colours.Background4,
                        RelativeSizeAxes = Axes.Both,
                    },
                    new Box
                    {
                        Colour = colours.Background3,
                        RelativeSizeAxes = Axes.Y,
                        Width = ControlPointTable.TIMING_COLUMN_WIDTH + margins,
                    },
                    new OsuScrollContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Child = table = new ControlPointTable(),
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

                controlPointGroups.BindTo(Beatmap.ControlPointInfo.Groups);
                controlPointGroups.BindCollectionChanged((_, _) =>
                {
                    table.ControlGroups = controlPointGroups;
                    changeHandler?.SaveState();
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

            private Type trackedType;

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
                    // If the selected group only has one control point, update the tracking type.
                    if (selectedGroup.Value.ControlPoints.Count == 1)
                        trackedType = selectedGroup.Value?.ControlPoints.Single().GetType();
                    // If the selected group has more than one control point, choose the first as the tracking type
                    // if we don't already have a singular tracked type.
                    else if (trackedType == null)
                        trackedType = selectedGroup.Value?.ControlPoints.FirstOrDefault()?.GetType();
                }

                if (trackedType != null)
                {
                    // We don't have an efficient way of looking up groups currently, only individual point types.
                    // To improve the efficiency of this in the future, we should reconsider the overall structure of ControlPointInfo.

                    // Find the next group which has the same type as the selected one.
                    var found = Beatmap.ControlPointInfo.Groups
                                       .Where(g => g.ControlPoints.Any(cp => cp.GetType() == trackedType))
                                       .LastOrDefault(g => g.Time <= clock.CurrentTimeAccurate);

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
}
