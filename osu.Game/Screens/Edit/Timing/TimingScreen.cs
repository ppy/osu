// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.Edit.Timing
{
    public class TimingScreen : EditorRoundedScreen
    {
        [Cached]
        private Bindable<ControlPointGroup> selectedGroup = new Bindable<ControlPointGroup>();

        public TimingScreen()
            : base(EditorScreenMode.Timing)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(new GridContainer
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
            });
        }

        public class ControlPointList : CompositeDrawable
        {
            private OsuButton deleteButton;
            private ControlPointTable table;

            private readonly IBindableList<ControlPointGroup> controlPointGroups = new BindableList<ControlPointGroup>();

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
                        Colour = colours.Background3,
                        RelativeSizeAxes = Axes.Both,
                    },
                    new Box
                    {
                        Colour = colours.Background2,
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
                                Text = "+ Add at current time",
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

                selectedGroup.BindValueChanged(selected => { deleteButton.Enabled.Value = selected.NewValue != null; }, true);

                controlPointGroups.BindTo(Beatmap.ControlPointInfo.Groups);
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

                Beatmap.ControlPointInfo.RemoveGroup(selectedGroup.Value);

                selectedGroup.Value = Beatmap.ControlPointInfo.Groups.FirstOrDefault(g => g.Time >= clock.CurrentTime);
            }

            private void addNew()
            {
                selectedGroup.Value = Beatmap.ControlPointInfo.GroupAt(clock.CurrentTime, true);
            }
        }
    }
}
