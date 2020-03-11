// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osuTK.Graphics;
using osu.Framework.Graphics;
using osu.Game.Overlays.SearchableList;

namespace osu.Game.Overlays.Social
{
    public class FilterControl : SearchableListFilterControl<SocialSortCriteria, SortDirection>
    {
        protected override Color4 BackgroundColour => Color4Extensions.FromHex(@"47253a");
        protected override SocialSortCriteria DefaultTab => SocialSortCriteria.Rank;
        protected override SortDirection DefaultCategory => SortDirection.Ascending;

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
