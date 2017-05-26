// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel;
using OpenTK.Graphics;
using osu.Game.Graphics;
using osu.Game.Overlays.Browse;
using osu.Game.Overlays.Social;

namespace osu.Game.Overlays
{
    public class SocialOverlay : BrowseOverlay<SocialTab, SocialSortCriteria>
    {
        protected override Color4 BackgroundColour => OsuColour.FromHex(@"60284b");
        protected override Color4 TrianglesColourLight => OsuColour.FromHex(@"672b51");
        protected override Color4 TrianglesColourDark => OsuColour.FromHex(@"5c2648");

        protected override BrowseFilterControl<SocialSortCriteria> CreateFilterControl() => new FilterControl();
        protected override BrowseHeader<SocialTab> CreateHeader() => new Header();
    }

    public enum SocialTab
    {
        [Description("Online Players")]
        OnlinePlayers,
        [Description("Online Friends")]
        OnlineFriends,
        [Description("Online Team Members")]
        OnlineTeamMembers,
        [Description("Chat Channels")]
        ChatChannels,
    }

    public enum SocialSortCriteria
    {
        Name,
        Rank,
        Location,
        [Description("Time Zone")]
        TimeZone,
        [Description("World Map")]
        WorldMap,
    }
}
