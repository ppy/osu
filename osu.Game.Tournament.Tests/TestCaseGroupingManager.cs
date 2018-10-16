// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.Game.Tournament.Screens.Ladder.Components;

namespace osu.Game.Tournament.Tests
{
    public class TestCaseGroupingManager : LadderTestCase
    {
        private readonly FillFlowContainer<GroupingRow> items;

        public TestCaseGroupingManager()
        {
            Add(new FillFlowContainer
            {
                Direction = FillDirection.Vertical,
                RelativeSizeAxes = Axes.Both,
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
                        Width = 100,
                        Text = "Add",
                        Action = addNew
                    },
                }
            });

        }

        [BackgroundDependencyLoader]
        private void load()
        {
            foreach (var g in Ladder.Groupings)
                items.Add(new GroupingRow(g));
        }

        protected override void SaveChanges()
        {
            Ladder.Groupings = items.Children.Select(c => c.Grouping).ToList();
            base.SaveChanges();
        }

        private void addNew() => items.Add(new GroupingRow(new TournamentGrouping()));

        public class GroupingRow : CompositeDrawable
        {
            public readonly TournamentGrouping Grouping;

            public GroupingRow(TournamentGrouping grouping)
            {
                Grouping = grouping;
                InternalChildren = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        Direction = FillDirection.Horizontal,
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            new SettingsTextBox { Width = 0.3f, Bindable = Grouping.Name },
                            new SettingsTextBox { Width = 0.3f, Bindable = Grouping.Description },
                            new SettingsSlider<int> { LabelText = "Best of", Width = 0.3f, Bindable = Grouping.BestOf },
                            new DangerousSettingsButton
                            {
                                Width = 0.1f,
                                Text = "Delete",
                                Action = () => Expire()
                            },
                        }
                    }
                };

                RelativeSizeAxes = Axes.X;
                Height = 40;
            }
        }
    }
}
