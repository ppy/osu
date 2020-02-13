// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Threading;
using osu.Game.Graphics;
using osu.Game.Overlays.SearchableList;
using osu.Game.Rulesets;
using osuTK.Graphics;

namespace osu.Game.Screens.Multi.Lounge.Components
{
    public class FilterControl : SearchableListFilterControl<PrimaryFilter, SecondaryFilter>
    {
        protected override Color4 BackgroundColour => OsuColour.FromHex(@"362e42");
        protected override PrimaryFilter DefaultTab => PrimaryFilter.Open;
        protected override SecondaryFilter DefaultCategory => SecondaryFilter.Public;

        protected override float ContentHorizontalPadding => base.ContentHorizontalPadding + OsuScreen.HORIZONTAL_OVERFLOW_PADDING;

        [Resolved(CanBeNull = true)]
        private Bindable<FilterCriteria> filter { get; set; }

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; }

        public FilterControl()
        {
            DisplayStyleControl.Hide();
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (filter == null)
                filter = new Bindable<FilterCriteria>();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ruleset.BindValueChanged(_ => updateFilter());
            Search.Current.BindValueChanged(_ => scheduleUpdateFilter());
            Tabs.Current.BindValueChanged(_ => updateFilter(), true);
        }

        private ScheduledDelegate scheduledFilterUpdate;

        private void scheduleUpdateFilter()
        {
            scheduledFilterUpdate?.Cancel();
            scheduledFilterUpdate = Scheduler.AddDelayed(updateFilter, 200);
        }

        private void updateFilter()
        {
            scheduledFilterUpdate?.Cancel();

            filter.Value = new FilterCriteria
            {
                SearchString = Search.Current.Value ?? string.Empty,
                PrimaryFilter = Tabs.Current.Value,
                SecondaryFilter = DisplayStyleControl.Dropdown.Current.Value,
                Ruleset = ruleset.Value
            };
        }
    }

    public enum PrimaryFilter
    {
        Open,

        [Description("Recently Ended")]
        RecentlyEnded,
        Participated,
        Owned,
    }

    public enum SecondaryFilter
    {
        Public,
        //Private,
    }
}
