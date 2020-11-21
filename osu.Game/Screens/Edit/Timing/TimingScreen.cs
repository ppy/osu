// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Edit.Compose.Components.Timeline;
using osuTK;

namespace osu.Game.Screens.Edit.Timing
{
    public class TimingScreen : EditorScreenWithTimeline
    {
        [Cached]
        private Bindable<ControlPointGroup> selectedGroup = new Bindable<ControlPointGroup>();

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
                new Dimension(GridSizeMode.Absolute, 200),
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

        protected override void OnTimelineLoaded(TimelineArea timelineArea)
        {
            base.OnTimelineLoaded(timelineArea);
            timelineArea.Timeline.Zoom = timelineArea.Timeline.MinZoom;
        }

        public class ControlPointList : CompositeDrawable
        {
            private OsuButton deleteButton;
            private ControlPointTable table;

            private readonly IBindableList<ControlPointGroup> controlPointGroups = new BindableList<ControlPointGroup>();

            [Resolved]
            private EditorClock clock { get; set; }

            [Resolved]
            protected IBindable<WorkingBeatmap> Beatmap { get; private set; }

            [Resolved]
            private Bindable<ControlPointGroup> selectedGroup { get; set; }

            [Resolved(canBeNull: true)]
            private IEditorChangeHandler changeHandler { get; set; }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                RelativeSizeAxes = Axes.Both;

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        Colour = colours.Gray0,
                        RelativeSizeAxes = Axes.Both,
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
                        Margin = new MarginPadding(10),
                        Spacing = new Vector2(5),
                        Children = new Drawable[]
                        {
                            deleteButton = new OsuButton
                            {
                                Text = "-",
                                Size = new Vector2(30, 30),
                                Action = delete,
                                Anchor = Anchor.BottomRight,
                                Origin = Anchor.BottomRight,
                            },
                            new OsuButton
                            {
                                Text = "+",
                                Action = addNew,
                                Size = new Vector2(30, 30),
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

                selectedGroup.BindValueChanged(selected => { deleteButton.Enabled.Value = selected.NewValue != null; }, true);

                controlPointGroups.BindTo(Beatmap.Value.Beatmap.ControlPointInfo.Groups);
                controlPointGroups.BindCollectionChanged((sender, args) =>
                {
                    table.ControlGroups = controlPointGroups;
                    changeHandler?.SaveState();
                }, true);
            }

            private void delete()
            {
                if (selectedGroup.Value == null)
                    return;

                Beatmap.Value.Beatmap.ControlPointInfo.RemoveGroup(selectedGroup.Value);

                selectedGroup.Value = Beatmap.Value.Beatmap.ControlPointInfo.Groups.FirstOrDefault(g => g.Time >= clock.CurrentTime);
            }

            private void addNew()
            {
                selectedGroup.Value = Beatmap.Value.Beatmap.ControlPointInfo.GroupAt(clock.CurrentTime, true);
            }
        }
    }
}
