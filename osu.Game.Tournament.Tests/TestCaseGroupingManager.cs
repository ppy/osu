// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays.Settings;
using osu.Game.Tournament.Screens.Ladder.Components;

namespace osu.Game.Tournament.Tests
{
    public class TestCaseGroupingManager : LadderTestCase
    {
        public TestCaseGroupingManager()
        {
            FillFlowContainer items;

            Add(items = new FillFlowContainer
            {
                Direction = FillDirection.Vertical,
                RelativeSizeAxes = Axes.Both
            });

            foreach (var g in Ladder.Groupings)
                items.Add(new GroupingRow(g));
        }

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
                            new SettingsTextBox { Width = 0.4f, Bindable = Grouping.Name },
                            new SettingsTextBox { Width = 0.4f, Bindable = Grouping.Description },
                        }
                    }
                };

                RelativeSizeAxes = Axes.X;
                Height = 40;
            }
        }
    }
}
