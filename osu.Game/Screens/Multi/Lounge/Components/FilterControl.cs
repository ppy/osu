// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Threading;
using osu.Game.Overlays.SearchableList;
using osu.Game.Rulesets;
using osuTK.Graphics;

namespace osu.Game.Screens.Multi.Lounge.Components
{
    public class FilterControl : SearchableListFilterControl<RoomStatusFilter, RoomCategoryFilter>
    {
        protected override Color4 BackgroundColour => Color4.Black.Opacity(0.5f);
        protected override RoomStatusFilter DefaultTab => RoomStatusFilter.Open;
        protected override RoomCategoryFilter DefaultCategory => RoomCategoryFilter.Any;

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
            filter ??= new Bindable<FilterCriteria>();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ruleset.BindValueChanged(_ => updateFilter());
            Search.Current.BindValueChanged(_ => scheduleUpdateFilter());
            Dropdown.Current.BindValueChanged(_ => updateFilter());
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

            if (filter == null)
                return;

            filter.Value = new FilterCriteria
            {
                SearchString = Search.Current.Value ?? string.Empty,
                StatusFilter = Tabs.Current.Value,
                RoomCategoryFilter = Dropdown.Current.Value,
                Ruleset = ruleset.Value
            };
        }
    }

    public enum RoomStatusFilter
    {
        Open,

        [Description("Recently Ended")]
        Ended,
        Participated,
        Owned,
    }

    public enum RoomCategoryFilter
    {
        Any,
        Normal,
        Spotlight
    }
}
