// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays.Browse;

namespace osu.Game.Overlays.Direct
{
    public class Header : BrowseHeader<DirectTab>
    {
        protected override Color4 BackgroundColour => OsuColour.FromHex(@"252f3a");
        protected override float TabStripWidth => 298;

        protected override DirectTab DefaultTab => DirectTab.Search;
        protected override Drawable CreateHeaderText() => new OsuSpriteText { Text = @"osu!direct", TextSize = 25 };

        public Header()
        {
            Tabs.Current.Value = DirectTab.Search;
            Tabs.Current.TriggerChange();
        }
    }

    public enum DirectTab
    {
        Search,
        [Description("Newest Maps")]
        NewestMaps,
        [Description("Top Rated")]
        TopRated,
        [Description("Most Played")]
        MostPlayed
    }
}
