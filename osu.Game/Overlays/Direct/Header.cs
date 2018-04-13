// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays.SearchableList;

namespace osu.Game.Overlays.Direct
{
    public class Header : SearchableListHeader<DirectTab>
    {
        protected override Color4 BackgroundColour => OsuColour.FromHex(@"252f3a");

        protected override DirectTab DefaultTab => DirectTab.Search;
        protected override Drawable CreateHeaderText() => new OsuSpriteText { Text = @"osu!direct", TextSize = 25 };
        protected override FontAwesome Icon => FontAwesome.fa_osu_chevron_down_o;

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
