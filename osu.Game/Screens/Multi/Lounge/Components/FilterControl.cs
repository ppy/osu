// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Graphics;
using osu.Game.Overlays.SearchableList;
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

            Search.Current.BindValueChanged(_ => updateFilter());
            Tabs.Current.BindValueChanged(_ => updateFilter(), true);
        }

        private void updateFilter()
        {
            filter.Value = new FilterCriteria
            {
                SearchString = Search.Current.Value ?? string.Empty,
                PrimaryFilter = Tabs.Current.Value,
                SecondaryFilter = DisplayStyleControl.Dropdown.Current.Value
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
