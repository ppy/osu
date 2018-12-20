// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel;
using osu.Game.Graphics;
using osu.Game.Overlays.SearchableList;
using osuTK.Graphics;

namespace osu.Game.Screens.Multi.Lounge.Components
{
    public class FilterControl : SearchableListFilterControl<PrimaryFilter, SecondaryFilter>
    {
        protected override Color4 BackgroundColour => OsuColour.FromHex(@"362e42");
        protected override PrimaryFilter DefaultTab => PrimaryFilter.Open;

        public FilterControl()
        {
            DisplayStyleControl.Hide();
        }

        public FilterCriteria CreateCriteria() => new FilterCriteria
        {
            SearchString = Search.Current.Value ?? string.Empty,
            PrimaryFilter = Tabs.Current,
            SecondaryFilter = DisplayStyleControl.Dropdown.Current
        };
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
