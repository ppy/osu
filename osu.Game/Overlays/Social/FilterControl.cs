// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Overlays.SearchableList;

namespace osu.Game.Overlays.Social
{
    public class FilterControl : SearchableListFilterControl<SocialSortCriteria, SortDirection>
    {
        protected override Color4 BackgroundColour => OsuColour.FromHex(@"47253a");
        protected override SocialSortCriteria DefaultTab => SocialSortCriteria.Rank;

        public FilterControl()
        {
            Tabs.Margin = new MarginPadding { Top = 10 };
        }
    }

    public enum SocialSortCriteria
    {
        Rank,
        Name,
        Location,
        //[Description("Time Zone")]
        //TimeZone,
        //[Description("World Map")]
        //WorldMap,
    }
}
