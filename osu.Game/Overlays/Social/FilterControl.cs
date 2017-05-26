// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Game.Graphics;
using osu.Game.Overlays.Browse;

namespace osu.Game.Overlays.Social
{
    public class FilterControl : BrowseFilterControl<SocialSortCriteria>
    {
        protected override Color4 BackgroundColour => OsuColour.FromHex(@"47253a");
        protected override SocialSortCriteria DefaultTab => SocialSortCriteria.Name;
    }
}
