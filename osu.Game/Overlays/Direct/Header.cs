// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osuTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays.SearchableList;

namespace osu.Game.Overlays.Direct
{
    public class Header : SearchableListHeader<DirectTab>
    {
        protected override Color4 BackgroundColour => OsuColour.FromHex(@"252f3a");

        protected override DirectTab DefaultTab => DirectTab.Search;
        protected override Drawable CreateHeaderText() => new OsuSpriteText { Text = @"osu!direct", Font = OsuFont.GetFont(size: 25) };
        protected override IconUsage Icon => OsuIcon.ChevronDownCircle;

        public Header()
        {
            Tabs.Current.Value = DirectTab.NewestMaps;
            Tabs.Current.TriggerChange();
        }
    }

    public enum DirectTab
    {
        Search,

        [Description("Newest Maps")]
        NewestMaps = DirectSortCriteria.Ranked,

        [Description("Top Rated")]
        TopRated = DirectSortCriteria.Rating,

        [Description("Most Played")]
        MostPlayed = DirectSortCriteria.Plays,
    }
}
