// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Screens.Ladder.Components;

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
                new FillFlowContainer
                {
                    Direction = FillDirection.Vertical,
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.9f,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Children = new Drawable[]
                    {
                        items = new FillFlowContainer<GroupingRow>
                        {
                            Direction = FillDirection.Vertical,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                        },
                        new TriangleButton
                        {
                            Margin = new MarginPadding(20),
                            Width = 100,
                            Text = "Add",
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
            Scheduler.AddDelayed(() => LadderInfo.Groupings = items.Children.Select(c => c.Grouping).ToList(), 500, true);
        }

        private void addNew()
        {
            items.Add(new GroupingRow(new TournamentGrouping { StartDate = { Value = DateTimeOffset.UtcNow } }, updateGroupings));
            updateGroupings();
        }

        private void updateGroupings()
        {
            LadderInfo.Groupings = items.Children.Select(c => c.Grouping).ToList();
        }

        public class GroupingRow : CompositeDrawable
        {
            public readonly TournamentGrouping Grouping;

            public GroupingRow(TournamentGrouping grouping, Action onDelete)
            {
                Grouping = grouping;
                InternalChildren = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        Direction = FillDirection.Full,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            new SettingsTextBox { Width = 0.3f, Bindable = Grouping.Name },
                            new SettingsTextBox { Width = 0.3f, Bindable = Grouping.Description },
                            new SettingsSlider<int> { LabelText = "Best of", Width = 0.3f, Bindable = Grouping.BestOf },
                            new DangerousSettingsButton
                            {
                                Width = 0.1f,
                                Text = "Delete",
                                Action = () =>
                                {
                                    Expire();
                                    onDelete();
                                }
                            },
                            new DateTextBox { Width = 0.3f, Bindable = Grouping.StartDate },
                        }
                    }
                };

                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
            }
        }
    }
}
