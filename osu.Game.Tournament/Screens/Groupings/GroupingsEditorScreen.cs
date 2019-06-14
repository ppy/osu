// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Screens.Ladder.Components;
using osuTK;

namespace osu.Game.Tournament.Screens.Groupings
{
    public class GroupingsEditorScreen : TournamentScreen, IProvideVideo
    {
        private readonly FillFlowContainer<GroupingRow> items;

        public GroupingsEditorScreen()
        {
            AddRangeInternal(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.Gray(0.2f),
                },
                new OsuScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.9f,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Child = items = new FillFlowContainer<GroupingRow>
                    {
                        Direction = FillDirection.Vertical,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                    },
                },
                new ControlPanel
                {
                    Children = new Drawable[]
                    {
                        new TriangleButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Add new",
                            Action = addNew
                        },
                    }
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            foreach (var g in LadderInfo.Groupings)
                items.Add(new GroupingRow(g, updateGroupings));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Scheduler.AddDelayed(updateGroupings, 500, true);
        }

        private void addNew()
        {
            items.Add(new GroupingRow(new TournamentGrouping
            {
                StartDate =
                {
                    Value = DateTimeOffset.UtcNow
                }
            }, updateGroupings));

            updateGroupings();
        }

        private void updateGroupings()
        {
            LadderInfo.Groupings.Clear();
            LadderInfo.Groupings.AddRange(items.Children.Select(c => c.Grouping));
        }

        public class GroupingRow : CompositeDrawable
        {
            public readonly TournamentGrouping Grouping;

            public GroupingRow(TournamentGrouping grouping, Action onDelete)
            {
                Margin = new MarginPadding(10);

                Grouping = grouping;
                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        Colour = OsuColour.Gray(0.1f),
                        RelativeSizeAxes = Axes.Both,
                    },
                    new FillFlowContainer
                    {
                        Margin = new MarginPadding(5),
                        Padding = new MarginPadding { Right = 160 },
                        Spacing = new Vector2(5),
                        Direction = FillDirection.Full,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            new SettingsTextBox
                            {
                                LabelText = "Name",
                                Width = 0.33f,
                                Bindable = Grouping.Name
                            },
                            new SettingsTextBox
                            {
                                LabelText = "Description",
                                Width = 0.33f,
                                Bindable = Grouping.Description
                            },
                            new DateTextBox
                            {
                                LabelText = "Start Time",
                                Width = 0.33f,
                                Bindable = Grouping.StartDate
                            },
                            new SettingsSlider<int>
                            {
                                LabelText = "Best of",
                                Width = 0.33f,
                                Bindable = Grouping.BestOf
                            },
                        }
                    },
                    new DangerousSettingsButton
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        RelativeSizeAxes = Axes.None,
                        Width = 150,
                        Text = "Delete",
                        Action = () =>
                        {
                            Expire();
                            onDelete();
                        },
                    }
                };

                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
            }
        }
    }
}
